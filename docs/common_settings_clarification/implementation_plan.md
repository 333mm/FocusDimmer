# 実装計画: モニタ共通設定UIの視覚的明確化

右側カラムに表示されている「共通設定（Global Settings）」について、ユーザーがこれがすべてのモニターに適用される共通の設定であることを一目で理解できるように、UIの視覚的改善を行います。

## ユーザーレビューが必要な項目
特になし（既存のレイアウト構造を崩さず、ヘッダーにバッジと補足説明を追加するのみの安全な変更です）。

## 提案される変更

### 1. UI（XAML）の変更

#### [MODIFY] [MainWindow.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/MainWindow.xaml)
右側カラム（共通設定部）のヘッダー部分を変更し、ローカライズされた共通設定タイトルを表示するとともに、隣に「モニター共通（ALL MONITORS）」を示すバッジを追加します。さらに、その下に「これらの設定はすべてのモニターに適用されます。」という補足テキストを配置します。

**変更前のヘッダー部分:**
```xaml
<!-- Global Settings Header -->
<StackPanel Orientation="Horizontal" Margin="0,5,0,15">
    <TextBlock Text="&#xE713;" FontFamily="{StaticResource IconFont}" FontSize="18" Foreground="{StaticResource AccentColor}" VerticalAlignment="Center" Margin="0,0,10,0"/>
    <TextBlock Text="{Binding Strings.HeaderGlobalSettings, FallbackValue='Global Settings'}" FontSize="16" FontWeight="SemiBold" Foreground="{StaticResource TextPrimary}" VerticalAlignment="Center"/>
</StackPanel>
```

**変更後のヘッダー部分:**
```xaml
<!-- Global Settings Header -->
<StackPanel Margin="0,5,0,15">
    <StackPanel Orientation="Horizontal" Margin="0,0,0,4">
        <TextBlock Text="&#xE713;" FontFamily="{StaticResource IconFont}" FontSize="18" Foreground="{StaticResource AccentColor}" VerticalAlignment="Center" Margin="0,0,10,0"/>
        <TextBlock Text="{Binding Strings.HeaderGlobalSettings, FallbackValue='Global Settings'}" FontSize="16" FontWeight="SemiBold" Foreground="{StaticResource TextPrimary}" VerticalAlignment="Center"/>
        <!-- 共通設定を示すバッジ -->
        <Border Background="#2260CDFF" BorderBrush="#8860CDFF" BorderThickness="1" CornerRadius="4" Padding="6,2" Margin="10,0,0,0" VerticalAlignment="Center">
            <TextBlock Text="{Binding Strings.BadgeAllMonitors, FallbackValue='ALL MONITORS'}" Foreground="#FF60CDFF" FontSize="10" FontWeight="Bold"/>
        </Border>
    </StackPanel>
    <!-- 補足説明テキスト -->
    <TextBlock Text="{Binding Strings.SubHeaderGlobalSettings, FallbackValue='These settings apply to all connected monitors.'}" FontSize="11" Foreground="{StaticResource TextSecondary}" Opacity="0.8" Margin="28,0,0,0"/>
</StackPanel>
```

---

### 2. 多言語ファイルの追加・更新

現在 `HeaderGlobalSettings` キーは各言語の JSON ファイルに定義されておらず、フォールバック値（英語）が表示されています。また、今回追加するサブヘッダーとバッジ用のキーも追加します。

以下のすべての言語ファイルにキーを追加します。
- [ja.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/ja.json)
- [en.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/en.json)
- [zh.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/zh.json)
- [de.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/de.json)
- [es.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/es.json)
- [fr.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/fr.json)
- [pt.json](file:///d:/Dev/FocusDimmer2/FocusDimmer2/Languages/pt.json)

**各言語ファイルに追加する内容:**

#### [ja.json]
```json
  "HeaderGlobalSettings": "共通設定",
  "SubHeaderGlobalSettings": "これらの設定はすべてのモニターに適用されます。",
  "BadgeAllMonitors": "モニター共通",
```

#### [en.json]
```json
  "HeaderGlobalSettings": "Global Settings",
  "SubHeaderGlobalSettings": "These settings apply to all connected monitors.",
  "BadgeAllMonitors": "ALL MONITORS",
```

#### [zh.json]
```json
  "HeaderGlobalSettings": "全局设置",
  "SubHeaderGlobalSettings": "这些设置适用于所有连接的显示器。",
  "BadgeAllMonitors": "所有显示器",
```

#### [de.json]
```json
  "HeaderGlobalSettings": "Globale Einstellungen",
  "SubHeaderGlobalSettings": "Diese Einstellungen gelten für alle angeschlossenen Monitore.",
  "BadgeAllMonitors": "ALLE MONITORE",
```

#### [es.json]
```json
  "HeaderGlobalSettings": "Ajustes globales",
  "SubHeaderGlobalSettings": "Estos ajustes se aplican a todos los monitores conectados.",
  "BadgeAllMonitors": "TODOS MONITORES",
```

#### [fr.json]
```json
  "HeaderGlobalSettings": "Paramètres globaux",
  "SubHeaderGlobalSettings": "Ces paramètres s'appliquent à tous les moniteurs connectés.",
  "BadgeAllMonitors": "TOUS LES ÉCRANS",
```

#### [pt.json]
```json
  "HeaderGlobalSettings": "Configurações globais",
  "SubHeaderGlobalSettings": "Estas configurações se aplicam a todos os monitores conectados.",
  "BadgeAllMonitors": "TODOS MONITORES",
```

---

## 検証計画

### 自動ビルドテスト
- `dotnet build` コマンドにより、コードのコンパイルがエラーなしで通ることを確認します。

### 目視・動作確認
- アプリ起動後、右側カラムのヘッダーがローカライズされ、補足説明と「モニター共通」バッジが正しく表示されているか確認します。
- 言語を「日本語」「English」等に切り替えた際、ヘッダー・サブヘッダー・バッジのテキストが連動して切り替わることを確認します。
