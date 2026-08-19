# 実装計画: Windows 11 設定アプリ風モダン Fluent UI デザイン刷新

最新の Windows 11 設定アプリ（Windows Settings）や WinUI 3 / Fluent Design System に準拠した、統一感のあるおしゃれで洗練された UI デザインに全面刷新します。

---

## 提案する変更内容

### 1. カラーパレットとレイヤー構造（Mica / Fluent Dark）の刷新
- **背景と階層感**:
  - ウィンドウ背景: 落ち着いた深いダークグレー（`#1F1F1F` 〜 `#202020`）
  - サイドバー（NavigationView）: 微妙な透過トーン＋繊細な右境界線（`#12FFFFFF`）
  - カード背景: WinUI 3 標準のレイヤードカード（`#2B2B2B` / `#0CFFFFFF`）、繊細な光彩ボーダー（`#18FFFFFF`）と角丸 `8px`

---

### 2. Windows 11 風コントロールスタイルの導入・洗練
- **ToggleSwitch（トグルスイッチ）スタイルの新設**:
  - Windows 11 の象徴であるモダンなカプセル型トグルスイッチ（ON でアクセントカラー＋白丸、OFF で枠線＋グレー丸）を XAML スタイルとして定義。
  - 主要なスイッチ機能（自動起動、トレイ格納、全暗、デスクトップのみ減光など）を、Windows 11 設定アプリ風の **「設定タイトル＋説明文＋右側トグルスイッチ」** のスマートな行レイアウト（SettingsCard 行）に統一。
- **Segmented / Pill タブコントロール**:
  - モニタータブ（Display 1, Display 2...）を、WinUI 3 風の **セグメントピルタブ（カプセル状の背景に浮かぶ角丸タブ）** スタイルに刷新。
- **Slider（スライダー）**:
  - 繊細な 4px トラック、つまみ（Thumb）のホバー・ドラッグアニメーションを Windows 11 標準デザインに精密化。
- **ボタン ＆ 入力フォーム**:
  - Standard Button / Accent Button / ComboBox / TextBox をすべて `CornerRadius="6"`、自然なホバー・フォーカスアニメーションに統一。

---

### 3. 各ページ（Monitors, Exclusions, Hotkeys, Presets, General）の統一レイアウト
- **Monitors ページ**:
  - 外観（色・不透明度）、動作設定、高度な設定（アイドル・全暗）、アニメーション設定を、Windows 11 設定アプリのカテゴリカード風にすっきりと整列。
- **General ページ**:
  - 「起動時に自動起動」「トレイに最小化」「言語設定」を、Windows 11 設定アプリと全く同一の美しい行構成に。
- **Exclusions / Hotkeys / Presets ページ**:
  - 入力エリア、ショートカット記録ボックス、ルールタグのピルデザインをモダンに統一。

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Lite`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -p:DefineConstants="PRO" -c Release`

### 2. UI・動作の確認
- ナビゲーションの切り替え、モニタータブの切り替えが滑らかに動作すること。
- 各種トグルスイッチ、スライダー、カラーピッカー、テキストボックスが正常にバインディングされ動作すること。
- Windows 11 のダークテーマと自然に調和する美しい見た目になっていること。
