# 実装計画: スマートなPRO版誘導UI ＆ マルチモニター表示ロック機能

ユーザー要望に基づき、PRO版への誘導をうるさすぎない洗練されたデザイン（WinUI 3 / Fluent Design準拠）にリファインし、マルチモニター構成の表示とロックUIを実装します。

---

## 提案する変更内容

### 1. マルチモニター構成の可視化とスマートなロック表示
- **フリー版でもすべてのモニターをタブに表示**:
  - [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs): `InitializeMonitors()` で、フリー版であっても PC に接続されているすべてのモニターを `MonitorProfiles` に追加します（ユーザーのモニター環境を認識）。
  - サブモニター（Display 2 以降）は、フリー版ではオーバーレイ（減光）を生成せず非アクティブとします。
- **タブのスマート表示**:
  - [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml): サブモニターのタブの横に控えめな「🔒」アイコンを表示。
- **サブモニター設定画面のスマートなロックカード**:
  - フリー版でサブモニターを選択した際、画面上部に洗練された **「🔒 PRO Feature: マルチモニター減光制御」** の案内カードと、上品な `[PROにアップグレード]` ボタンを配置。設定フォームは半透明に落ち着かせます。

---

### 2. サイドバー ＆ 各ページの PRO 誘導表示のリファイン
- **サイドバーのプロモバナー**:
  - 派手なベタ塗りをやめ、WinUI / Fluent 風の控えめな透過アクセントトーン＋繊細なボーダーのスマートなカードにリファイン。
- **Presets ページのスマートカード**:
  - プリセットページ最上部のバナーを、統一された落ち着きのあるモダンなデザインに調整。
- **PRO バッジのピルデザイン統一**:
  - 各カード（Advanced Dimming, Animation, Presets）の「🔒 PRO」バッジを、角丸の小さなピルバッジ（透明度のある背景＋アクセントカラー）で統一し、すっきりとした見た目に仕上げます。

---

### 3. 多言語対応（全 7 言語）
- [Languages/*.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/):
  - `HeaderMultiMonitorPro`: マルチモニター減光制御
  - `DescMultiMonitorPro`: 接続された複数のディスプレイを個別に減光・連動制御するにはPRO版が必要です。
  - `BtnUpgradePro`: PROにアップグレード
- [LocalizationService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/LocalizationService.cs): 対応プロパティの追加。

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Lite` (フリー版環境の検証)
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -p:DefineConstants="PRO" -c Release` (PRO版環境の検証)

### 2. 動作確認
- **Lite 構成での確認**:
  - 複数モニター環境で Display 1 と Display 2 の両方がタブに表示されること。
  - Display 2 タブに 🔒 アイコンが表示され、クリック時にスマートなロック案内とアップグレードボタンが表示されること。
  - サイドバーや Presets 画面の PRO 誘導が上品で自然に馴染んでいること。
- **Pro 構成での確認**:
  - 🔒 アイコンやロック案内が一切表示されず、全モニターが快適に操作できること。
