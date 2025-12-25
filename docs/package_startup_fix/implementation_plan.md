# 自動起動修正計画

## ゴール

パッケージ版 FocusDimmer における自動起動の問題を解決する。
現状、タスクマネージャーには表示されるが起動しない状態である。これの原因を特定し、正しく起動するように修正する。

## ユーザーレビューが必要な項目

なし

## 変更内容

### FocusDimmer.Package

#### [MODIFY] [Package.appxmanifest](file:///d:/Dev/FocusDimmer2/FocusDimmer.Package/Package.appxmanifest)

- `desktop:Extension` (Category="windows.startupTask") の `EntryPoint` 属性を `"Windows.FullTrustApplication"` に設定する。
  - 理由: Desktop Bridge (Win32) アプリで `Executable` を指定する場合、`EntryPoint` には `Windows.FullTrustApplication` を指定する必要があるため（エラー 80080204 対策）。
- `Executable` 属性は `$targetnametoken$.exe` のままとする。

### FocusDimmer2

#### [MODIFY] [StartupManager.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/StartupManager.cs)

- `StartupTask` の ID がマニフェストと一致しているか確認（一致していれば変更なし）。

## 検証計画

### 自動テスト

なし

### 手動検証

1. **マニフェスト修正の確認**:
   - 修正後のプロジェクトをビルドし、パッケージを作成する（またはデバッグ実行でデプロイする）。
   - スタートアップに登録する（アプリ内設定から）。
   - タスクマネージャーで「有効」になっていることを確認する。
   - PCを再起動、またはサインアウト/サインインを行い、アプリが自動起動するか確認する。
   - **注意**: デバッグ実行(`F5`)でもパッケージ登録は行われるため、Visual Studioからのデラッグ実行後に手動でアプリを起動し、スタートアップ設定をONにしてから再起動等でテスト可能。
