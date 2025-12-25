# Legacy Pro 永続化とモニタ選択の修正

## ゴールの説明

1. **Legacy Pro 永続化**: レガシーPro版（旧アプリ）が一度検出されたら、その後ユーザーが統合版アプリの設定をリセットしたり、旧アプリをアンインストールしたりしても、Pro機能が有効なままになるように修正します。これはユーザードキュメントフォルダに `.legacy_token` というファイルを生成し、永続的なトークンとして利用することで実現します。
2. **モニタ選択バグ**: 特定の環境で起動時にモニタ選択ドロップダウンが空欄になる（何も選択されない）不具合を修正します。何も選択されていない場合は、強制的に最初のモニタを選択するようにします。

## ユーザーレビューが必要な点
>
> [!NOTE]
> 永続化の修正は `MyDocuments\FocusDimmer\.legacy_token` にトークンファイルを書き込もうとします。サンドボックス化されたアプリで適切な機能宣言がない場合、この書き込みは失敗する可能性があります（try-catchで捕捉され、その場合は通常の設定ファイル依存に戻ります）。

## 変更内容

### サービス層

#### [MODIFY] [StoreService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/StoreService.cs)

- `CheckLegacyToken()` メソッドを追加: `MyDocuments\FocusDimmer\.legacy_token` の存在を確認します。
- `SaveLegacyToken()` メソッドを追加: Legacy Pro検出時にこのトークンを作成します。
- `InitializeAsync` を更新:
  - `IsLegacyMigrated` (設定) を確認。
  - falseなら、`CheckLegacyToken()` を確認。
  - それもfalseなら、`CheckLegacyProInstalled()` (旧アプリ検出) を実行。
  - いずれかがtrueなら、`_isLegacyProDetected = true` とし、`SaveLegacyToken()` を呼び出してトークンを保存し、設定も更新します。

### UI層

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `Loaded` イベントハンドラ（または `InitializeMonitors`）にて、`MonitorTabs.SelectedIndex` が -1 (未選択) かつ `MonitorProfiles.Count > 0` の場合を確認するロジックを追加します。
- 条件に合致する場合、強制的に `MonitorTabs.SelectedIndex = 0` を設定します。

## 検証計画

### 手動検証

1. **モニタ選択**:
    - アプリを起動し、モニタタブが即座に選択状態になっているか（空欄でないか）確認します。
2. **Legacy Pro 永続化**:
    - `settings.json` を削除して「クリーンインストール」状態をシミュレート（またはコードで一時的に無視）。
    - 初回検出後、ドキュメントフォルダに `.legacy_token` が作成されていることを確認します。
    - 再度 `settings.json` を削除します。
    - アプリを再起動し、トークンによってPro機能が有効のままになるか確認します。
