# 実装計画: 背景透明度調整・プリセットUX向上・メニュー名称変更

ユーザーからのご要望に基づき、UIの質感向上（背景透明度）、プリセット機能の使いやすさ向上、およびメニュー項目の名称変更（除外設定、ホットキー）を実施します。

---

## 提案する変更内容

### 1. メニュー名称の更新（全7言語）
- **「アプリ除外」 → 「除外設定」**:
  - 日本語 (`ja.json`): `NavExclusions` を `アプリ除外` から `除外設定` に変更
  - 英語 (`en.json`): `NavExclusions` を `App Exclusions` から `Exclusions` に変更
  - 他言語 (`zh`, `de`, `es`, `fr`, `pt`) および `LocalizationService.cs` も同様に整合
- **「ショートカット」 → 「ホットキー」**:
  - 日本語 (`ja.json`): `NavHotkeys` を `ショートカット` から `ホットキー` に変更
  - 英語 (`en.json`): `NavHotkeys` を `Shortcuts` から `Hotkeys` に変更
  - 他言語 (`zh`, `de`, `es`, `fr`, `pt`) および `LocalizationService.cs` も同様に整合

---

### 2. 背景の透明度（半透明アクリル・マイカ効果）の調整
- **ウィンドウ背景・サイドバー背景の透過調整**:
  - `WindowBackground` を不透明な `#1E1E1E` から半透明の `#D9181818`（約85%不透明度）に調整し、Windows 11 の Mica / Acrylic 背景効果と合わせて背後が上品に透ける質感を実現。
  - `SidebarBackground` を `#80141414`、カード背景 `CardBackground` を `#0EFFFFFF` に設定し、美しいレイヤー感を演出。

---

### 3. プリセット機能の使い勝手（UX）向上
- **プリセット管理カードの刷新**:
  - 現在アクティブなプリセット名・切り替えコンボボックスを配置。
  - クイックアクションボタンを分かりやすく整理：
    - `[＋ 新規作成]`
    - `[現在の設定を保存]`
    - `[名前変更]`
    - `[削除]`
- **プロセス連動（自動切り替え）の直感化**:
  - 「このプリセットを自動適用するアプリケーション」の解説文と、追加ボタン・バッジリストを整理し、ユーザーが直感的にプロセス連動を設定できるように改善。

---

## 検証計画

### 1. ビルド検証
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Lite`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Debug`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -c Release`
- `dotnet build FocusDimmer2/FocusDimmer.csproj -p:DefineConstants="PRO" -c Release`

### 2. 動作・UI確認
- サイドバーの名称が「除外設定」「ホットキー」に正しく反映されていること。
- ウィンドウ背景が上品に半透明化されていること。
- プリセットの選択・作成・保存・名前変更・削除およびプロセス連動の追加・削除が直感的に動作すること。
