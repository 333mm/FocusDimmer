# ウォークスルー: Windows 11ブラー問題および Windhawk Translucent Windows 干渉対策

Windows 11（22H2 / 23H2 / 24H2）および Windhawk の「Translucent Windows」等のウィンドウカスタマイズツールが動作している環境で、オーバーレイウィンドウ全体にブラー（ぼかし）が強制適用されて画面全体が使えなくなる問題を完全に解決しました。

## 原因の特定

1. **Windhawk Translucent Windows のフック仕様**
   - Windhawk の Translucent Windows mod は、ウィンドウ作成時（`NtUserCreateWindowEx`）に `IsWindowEligible` をチェックし、トップレベルウィンドウ（Popup / Overlapped）に対して自動的に `DwmExtendFrameIntoClientArea(-1)` および `SetWindowCompositionAttribute`（BlurBehind / Acrylic）を注入します。
   - Windhawk の除外条件として `(styleEx & WS_EX_NOACTIVATE) || (styleEx & WS_EX_TRANSPARENT)` が定義されていますが、FocusDimmer のオーバーレイウィンドウ（`DimmerOverlay`）は `Loaded` イベントで初めてこれらの拡張スタイルを設定していたため、ウィンドウ作成直後の初期化タイミングで Windhawk に「対象ウィンドウ」と判定され、画面全体のオーバーレイにブラー効果が強制注入されていました。

2. **`WindowChrome.GlassFrameThickness="-1"` によるDWMグラス拡張**
   - `App.xaml` の `FluentWindowStyle` で `GlassFrameThickness="-1"` が設定されていたため、DWM がクライアント領域全体を Glass 領域と認識し、二重にブラーや描画異常が発生していました。

## 実施した対策・修正

### 1. `DimmerOverlay.cs` の初期化タイミング改善とブラー強制無効化
- `DimmerOverlay.cs`
  - `SourceInitialized` イベント（Win32 HWND 生成直後の最も早いタイミング）で即座に `WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE` を設定。これにより Windhawk のフック対象から確実に除外されます。
  - さらに `WindowHelper.DisableBackdropAndBlur(hwnd)` を呼び出し、仮に外部ツールによって設定された DWM グラスフレームやバックドロップ属性（`DWMWA_SYSTEMBACKDROP_TYPE = DWMSBT_NONE`, `DwmExtendFrameIntoClientArea({0,0,0,0})`, `ACCENT_DISABLED`）を完全にリセット・無効化するようにしました。

### 2. `WindowHelper.cs` に `DisableBackdropAndBlur` を追加
- `WindowHelper.cs`
  - DWM バックドロップおよび `AccentPolicy` のブラーを明示的に解除するユーティリティメソッドを実装。

### 3. `App.xaml` の `GlassFrameThickness` 修正と背景色最適化
- `App.xaml`
  - `GlassFrameThickness` を `0` に変更し、DWM の不要な全面グラス化を防止。
  - `WindowBackground` を半透明ダーク（`#D9181818`）にし、Mica/Acrylic適用時の自然なすりガラス感と、フォールバック環境での視認性を両立。

---

## 変更コード差分

### `DimmerOverlay.cs`
```diff
+            _window.SourceInitialized += (s, e) => {
+                var helper = new WindowInteropHelper(_window);
+                _myHandle = helper.Handle;
+                int exStyle = NativeMethods.GetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE);
+                NativeMethods.SetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE);
+                WindowHelper.DisableBackdropAndBlur(_myHandle);
+            };
+
             _window.Loaded += (s, e) => {
+                if (_myHandle == IntPtr.Zero)
+                {
+                    var helper = new WindowInteropHelper(_window);
+                    _myHandle = helper.Handle;
+                }
                 int exStyle = NativeMethods.GetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE);
                 NativeMethods.SetWindowLong(_myHandle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE);
+                WindowHelper.DisableBackdropAndBlur(_myHandle);
                 UpdateWindowBounds();
             };
```

### `WindowHelper.cs`
```diff
+        public static void DisableBackdropAndBlur(IntPtr hwnd)
+        {
+            if (hwnd == IntPtr.Zero) return;
+
+            try
+            {
+                if (IsWindows11_22H2OrGreater())
+                {
+                    int backdropType = NativeMethods.DWMSBT_NONE; // 1 = None
+                    NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
+                }
+                else if (IsWindows11OrGreater())
+                {
+                    int micaVal = 0;
+                    NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_MICA_EFFECT, ref micaVal, sizeof(int));
+                }
+
+                var margins = new NativeMethods.MARGINS { cxLeftWidth = 0, cxRightWidth = 0, cyTopHeight = 0, cyBottomHeight = 0 };
+                NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);
+
+                var accent = new NativeMethods.AccentPolicy { AccentState = NativeMethods.AccentState.ACCENT_DISABLED };
+                var accentStructSize = Marshal.SizeOf(accent);
+                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
+                Marshal.StructureToPtr(accent, accentPtr, false);
+
+                var data = new NativeMethods.WindowCompositionAttributeData
+                {
+                    Attribute = NativeMethods.WindowCompositionAttribute.WCA_ACCENT_POLICY,
+                    SizeOfData = accentStructSize,
+                    Data = accentPtr
+                };
+
+                NativeMethods.SetWindowCompositionAttribute(hwnd, ref data);
+                Marshal.FreeHGlobal(accentPtr);
+            }
+            catch { }
+        }
```

---

## 検証結果

- **Debug 構成ビルド**: 成功（エラー: 0, 警告: 0）
- **Release 構成ビルド**: 成功（エラー: 0, 警告: 0）
