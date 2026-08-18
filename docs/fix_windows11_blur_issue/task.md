# タスクリスト: Windows 11最新環境におけるブラー問題の修正

- [x] 現状のコード調査と原因の確定 <!-- id: 0 -->
- [x] 実装計画の作成とユーザー承認 <!-- id: 1 -->
- [x] NativeMethods.cs の DWM 関連定数の追加 <!-- id: 2 -->
- [x] WindowHelper.cs のバックドロップ適用ロジックの改善 (Mica/Acrylic対応・Win10/11後方互換性) <!-- id: 3 -->
- [x] App.xaml の FluentWindowStyle における GlassFrameThickness の修正 (-1 -> 0) <!-- id: 4 -->
- [x] MainWindow.xaml および App.xaml の WindowBackground 設定の最適化 <!-- id: 5 -->
- [x] FluentWindow および各ウィンドウでのバックドロップ適用処理の整合性確認 <!-- id: 6 -->
- [x] Windhawk Translucent Windows 干渉対策 (SourceInitializedでのWS_EX_TRANSPARENT設定・DWMブラー強制解除) <!-- id: 7 -->
- [x] プロジェクトのビルド確認と警告・エラーの解消 (Debug, Release, Pro) <!-- id: 8 -->
- [x] 修正内容のウォークスルー作成 <!-- id: 9 -->
