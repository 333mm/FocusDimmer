# レガシーPro版移行バナーのUX改善

## 目標

レガシーPro版ユーザー向けの移行案内バナーの使い勝手を改善します。
現在は常に表示されていますが、ユーザーが一度確認したり意図的に閉じた場合は、大きなバナーを非表示にし、代わりにヘッダーの「PRO」バッジ内に小さなメガホンアイコンを表示するようにして、視覚的なノイズを減らします。

## 変更内容

### FocusDimmer.Models

#### [MODIFY] [AppSettings.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Models/AppSettings.cs)

- `bool IsLegacyBannerDismissed` プロパティを追加します。

### FocusDimmer

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)

- **バナー部分**:
  - グリッドレイアウトに変更し、右端に「閉じるボタン (×)」を追加します。
  - バナー全体の `Visibility` を、`LegacyBannerVisibility`（新設/変更）プロパティにバインドするように変更します。
- **PROバッジ部分**:
  - 内部にメガホンアイコン（`IsLegacyBannerDismissed` が true の場合のみ表示）を追加します。
  - アイコンクリック時にも移行案内を表示できるようにします（あるいはツールチップ等で案内）。

#### [MODIFY] [MainWindow.xaml.cs](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml.cs)

- `bool IsLegacyBannerDismissed` プロパティの変更通知を実装します。
- `LegacyBannerVisibility` プロパティのロジックを変更します:
  - `IsLegacyPro` かつ `!IsLegacyBannerDismissed` の場合のみ表示。
- `ProBadgeWithIconVisibility` (仮) などのロジックを追加、または既存の `ProBadgeVisibility` 内でアイコンの出し分けを制御します。
- **イベントハンドラ**:
  - バナーの「閉じる」ボタン押下時: `IsLegacyBannerDismissed = true` にして設定を保存。
  - バナー本体クリック時（`MigrationInfo_Click`）: 既存の処理に加え、`IsLegacyBannerDismissed = true` にして設定を保存（「一度開いた」とみなす）。
  - PROバッジ（メガホン）クリック時: `MigrationInfo_Click` と同様の案内を表示するようにイベントを追加（PROバッジ自体をクリック可能にするか、アイコン部分のみボタン化）。

## 検証計画

### 手動検証

1. **初期状態**: レガシーPro状態（デバッグ等で再現）で起動。バナーが表示されていることを確認。PROバッジは通常の「PRO」。
2. **閉じる動作**: バナーの×ボタンを押す。バナーが消え、PROバッジの中にメガホンが表示されることを確認。再起動しても状態が維持されることを確認。
3. **開く動作**: （リセット後）バナーをクリックして案内ダイアログを表示。「はい/いいえ」選択後、バナーが消えてPROバッジにメガホンが表示されることを確認。
4. **アイコン動作**: PROバッジ（メガホン付き）をクリックすると、再度案内ダイアログが出ることを確認（※この要望は明示されていませんが、「メガホンだけで表示」の意図はアクセス手段を残すことと推測されるため実装推奨）。
