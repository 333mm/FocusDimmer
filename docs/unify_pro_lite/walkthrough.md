# Pro版とLite版の統合（IAP導入）の完了

Pro版とLite版を1つのアプリケーションに統合し、Microsoft Storeのアドオン（IAP）を使用して機能をアンロックする仕組みを実装しました。また、旧Pro版ユーザーが自動的にプロ機能を利用できる救済策も導入しています。

## 実施した主な変更

### 1. プロジェクト構成の統一

- [FocusDimmer.csproj](file:///d:/Dev/FocusDimmer2/FocusDimmer2/FocusDimmer.csproj) から `FREE_VERSION` 定数を削除し、ビルド構成によらず同一のバイナリが生成されるようにしました。
- マニフェストファイルを [Package.appxmanifest](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Package.appxmanifest) に集約しました。

### 2. ストア連携サービスの実装

- [StoreService.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/StoreService.cs) を新規作成し、以下の機能を実装しました：
  - Microsoft Store からのアドオン購入状態の取得
  - 旧Pro版（`sanmiri.FocusDimmer_p3b9zhm3nac6p`）がインストールされているかの自動検知
  - アプリ内からのアドオン購入ダイアログの呼び出し

### 3. UIとロジックの動的化

- [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs) を更新し、アプリ起動時に非同期でPro版の状態を確認するようにしました。
- `IsPro` フラグを動的に更新することで、複数モニター対応や詳細設定のロック/アンロックが即座に反映されます。
- 初期化タイミングを調整し、アプリ内購入直後に再起動なしでサブモニターが表示されるようにしました。

## 検証方法

### ストア連携のシミュレーション

開発環境では実際のストア購入ができないため、以下の方法で動作を確認できます：

1. `StoreService.cs` 内の `_isProSubscribed = true;` を強制的にセットしてUIがアンロックされることを確認。
2. 他のモニターが正しく `MainWindow` のタブに追加されることを確認。

### UIの確認

- 未購入状態では、画面上部にアップグレードを促すバナーが表示され、タイトルに `(Free Lite)` が付与されていることを確認してください。
- バナーをクリックすると（パッケージ化されている場合）、購入ダイアログの呼び出しが行われます。

## 今後のステップ

- Microsoft Store パートナーセンターでアドオン（プロダクトID: `pro_upgrade` 等）を作成し、`StoreService.cs` の定数を確定させてください。
