# Pro機能検証ガイド

デバッグ環境（Visual Studio）において、実際のストア購入を行わずに「Pro版」としての動作や「旧Pro版からの移行」をテストする方法を解説します。

> [!IMPORTANT]
> **起動プロジェクトの確認**
> テストを行う際は、必ずソリューションエクスプローラーで **`FocusDimmer.Package`** を右クリックし、「スタートアップ プロジェクトに設定」してから実行してください。単体の `FocusDimmer2` プロジェクトを実行すると、パッケージ関連のAPI（StoreContextなど）が動作せずクラッシュします。

## 1. Pro版（購入済み状態）をテストする方法

実際に購入処理を通さなくても、コードを1行変更するだけで「購入済み」として振る舞わせることができます。

**対象ファイル:** `FocusDimmer2\Services\StoreService.cs`

`RequestPurchaseAsync` メソッドの結果判定を一時的に変更するか、またはプロパティを直接書き換えるのが簡単です。

### 手順

`IsPro` プロパティの定義を以下のように変更します。

```csharp
// 【変更前】
public bool IsPro => _isProSubscribed || _isLegacyProDetected;

// 【変更後: 強制的にProにする】
public bool IsPro => true; // _isProSubscribed || _isLegacyProDetected;
```

または、`InitializeAsync` メソッド内で強制的にフラグを立てます。

```csharp
public async Task<bool> InitializeAsync(...)
{
    // ... (省略) ...

    // ★テスト用: 無条件でPro扱いにする
    _isProSubscribed = true;

    // ... (省略) ...
}
```

これにより、アプリ起動時に常に「Pro版」として認識され、モニター数制限の解除などを確認できます。

---

## 2. 旧Pro版検知（移行）をテストする方法

旧アプリ（Legacy Pro）がインストールされていない環境でも、検知ロジックを強制的に成功させることで、移行メッセージや設定の引き継ぎをテストできます。

**対象ファイル:** `FocusDimmer2\Services\StoreService.cs`

`CheckLegacyProInstalled` メソッドが常に `true` を返すように変更します。

### 手順

```csharp
private bool CheckLegacyProInstalled()
{
    // ★テスト用: 強制的に検知させる
    return true; 
    
    /* 以下の元のロジックはコメントアウト
    try
    {
        // ...
    }
    catch
    {
        return false;
    }
    */
}
```

この状態で、`AppSettings` の `IsLegacyMigrated` が `false`（未移行）の状態であれば、アプリ起動時に「旧Pro版が見つかりました」という旨のダイアログや処理が走ることを確認できます。
（※設定ファイル `user.config` を削除して初期状態に戻すとテストしやすくなります）

## 3. 注意点

- **テストが終わったら必ず元に戻してください。** そのままリリースすると、無料でPro機能が使えてしまいます。
- コミットする前に `git status` や差分確認で、`StoreService.cs` の変更が含まれていないか確認することを推奨します。
