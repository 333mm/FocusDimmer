# 自動起動修正 実装計画

## 目標

アプリケーションの自動起動オプションが正しく機能するように修正する。
現在の実装では `StartupTask` API の呼び出しを同期的に待機 (`Wait()`) しており、これがデッドロックやタイムアウトを引き起こして誤った状態（無効）と判定されている可能性が高い。また、`async void`の使用により実行順序が保証されていない。

## ユーザーレビューが必要な事項

- 特になし（内部ロジックの改善）

## 変更内容

### Services

#### [MODIFY] [StartupManager.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Services/StartupManager.cs)

- メソッドを全て `async Task` ベースに変更する。
- `IsRegisteredInRegistry` (同期) を廃止し、`IsStartupEnabledAsync` (非同期) を追加。
- `RegisterInRegistry` / `RemoveFromRegistry` を `TryEnableStartupAsync` / `DisableStartupAsync` に変更し、結果を返すようにする。
- レジストリを使用するフォールバックロジックは、非パッケージ時は維持するが、現状パッケージ版がメインのため `StartupTask` APIを優先する。

### UI Layer

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- コンストラクタでの自動起動状態チェック (`IsRegisteredInRegistry`) を削除。
- `Loaded` イベント (`Window_Loaded`) 内で非同期に `StartupManager.IsStartupEnabledAsync` を呼び出し、チェックボックスの状態を更新する。
- チェックボックスの変更イベント (`AutoStart_Checked` / `Unchecked`) を `async` ハンドラにし、`StartupManager` の非同期メソッドを `await` する。
- イベントループを防ぐため、プログラムによるチェックボックス変更時はイベント発火を無視するフラグ制御を追加する。

## 検証計画

### 手動検証

1. アプリを起動し、設定画面で「Windows起動時に自動的に開始する」のチェックボックスが現在の状態（タスクマネージャーのスタートアップ状態）と一致することを確認。
2. チェックボックスを「オン」にする。
   - タスクマネージャーの「スタートアップ アプリ」を確認し、`Focus Dimmer` が「有効」になっていることを確認。
3. チェックボックスを「オフ」にする。
   - タスクマネージャーで「無効」になることを確認。
4. 再度「オン」にし、PCを再起動（またはサインアウト＆サインイン）して、アプリが自動起動することを確認。
