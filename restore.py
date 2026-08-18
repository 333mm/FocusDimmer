import os

xaml_path = r"d:\Dev\FocusDimmer2\FocusDimmer2\MainWindow.xaml"
backup_path = r"d:\Dev\FocusDimmer2\FocusDimmer2\MainWindow.xaml.orig"

with open(backup_path, "r", encoding="utf-8") as f:
    orig_lines = f.readlines()

def get_lines(start, end):
    return "".join(orig_lines[start - 1 : end])

resources_content = get_lines(23, 437)
resources_content = resources_content.replace("<Window.Resources>", "<components:FluentWindow.Resources>")
resources_content = resources_content.replace("</Window.Resources>", "</components:FluentWindow.Resources>")

header_content = get_lines(470, 543)
# 修正: Header の Grid.Row を 1 から 0 に変更して、Main Content と重ならないようにする
header_content = header_content.replace('Grid.Row="1"', 'Grid.Row="0"', 1)

overlay_card = get_lines(570, 660)
idle_card = get_lines(662, 730)
animation_card = get_lines(732, 821)
exclusion_card = get_lines(823, 913)
hotkeys_panel = get_lines(923, 945)
startup_grid = get_lines(949, 979)

def clean_exclusion_card(card):
    card = card.replace(", RelativeSource={RelativeSource AncestorType=Window}", "")
    card = card.replace("RelativeSource={RelativeSource AncestorType=Window}, ", "")
    card = card.replace("RelativeSource={RelativeSource AncestorType=Window}", "")
    card = card.replace("DataContext.", "")
    return card

exclusion_card_clean = clean_exclusion_card(exclusion_card)

# 新しい XAML の組み立て
new_xaml = f"""<components:FluentWindow x:Class="FocusDimmer.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:FocusDimmer"
        xmlns:components="clr-namespace:FocusDimmer.Components"
        xmlns:converters="clr-namespace:FocusDimmer.Converters"
        Title="{{Binding Strings.AppTitle}}" Height="680" Width="980"
        MinHeight="600" MinWidth="900"
        WindowStartupLocation="CenterScreen"
        ResizeMode="CanResize"
        Style="{{StaticResource FluentWindowStyle}}">

    <components:FluentWindow.Resources>
{resources_content}
    </components:FluentWindow.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
{header_content}

        <!-- Main Content (Row 1) -->
        <Grid Grid.Row="1" Margin="24,0,24,24">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="340"/>
            </Grid.ColumnDefinitions>

            <!-- Left Monitor Tabs (Column 0) -->
            <TabControl x:Name="MonitorTabs" Grid.Column="0" Background="Transparent" BorderThickness="0" 
                        Padding="0" ItemsSource="{{Binding MonitorProfiles}}" Margin="0,0,14,0">
                <TabControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <WrapPanel IsItemsHost="True" Orientation="Horizontal" Margin="0,0,0,10"/>
                    </ItemsPanelTemplate>
                </TabControl.ItemsPanel>

                <TabControl.ItemTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="&#xE7F4;" FontFamily="{{StaticResource IconFont}}" FontSize="14" Margin="0,0,8,0" VerticalAlignment="Center"/>
                            <TextBlock Text="{{Binding MonitorName}}" TextTrimming="CharacterEllipsis" MaxWidth="200" ToolTip="{{Binding MonitorName}}" VerticalAlignment="Center"/>
                        </StackPanel>
                    </DataTemplate>
                </TabControl.ItemTemplate>

                <TabControl.ContentTemplate>
                    <DataTemplate>
                        <Grid>
                            <ScrollViewer VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled" Padding="0,0,10,0">
                                <StackPanel>
{overlay_card}
{idle_card}
{animation_card}
                                </StackPanel>
                            </ScrollViewer>
                        </Grid>
                    </DataTemplate>
                </TabControl.ContentTemplate>
            </TabControl>

            <!-- Right Global Settings (Column 1) -->
            <ScrollViewer Grid.Column="1" VerticalScrollBarVisibility="Auto" HorizontalScrollBarVisibility="Disabled" Margin="14,0,0,0">
                <StackPanel Margin="0,0,0,20">
                    <!-- Global Settings Header -->
                    <StackPanel Margin="0,5,0,15">
                        <StackPanel Orientation="Horizontal" Margin="0,0,0,4">
                            <TextBlock Text="&#xE713;" FontFamily="{{StaticResource IconFont}}" FontSize="18" Foreground="{{StaticResource AccentColor}}" VerticalAlignment="Center" Margin="0,0,10,0"/>
                            <TextBlock Text="{{Binding Strings.HeaderGlobalSettings, FallbackValue='Global Settings'}}" FontSize="16" FontWeight="SemiBold" Foreground="{{StaticResource TextPrimary}}" VerticalAlignment="Center"/>
                            <!-- Badge -->
                            <Border Background="#2260CDFF" BorderBrush="#8860CDFF" BorderThickness="1" CornerRadius="4" Padding="6,2" Margin="10,0,0,0" VerticalAlignment="Center">
                                <TextBlock Text="{{Binding Strings.BadgeAllMonitors, FallbackValue='ALL MONITORS'}}" Foreground="#FF60CDFF" FontSize="10" FontWeight="Bold"/>
                            </Border>
                        </StackPanel>
                        <TextBlock Text="{{Binding Strings.SubHeaderGlobalSettings, FallbackValue='These settings apply to all connected monitors.'}}" FontSize="11" Foreground="{{StaticResource TextSecondary}}" Opacity="0.8" Margin="28,0,0,0"/>
                    </StackPanel>

                    <!-- Card 1: Exclusion Lists (共通) -->
{exclusion_card_clean}

                    <!-- Card 2: Hotkeys (共通) -->
                    <Border Style="{{StaticResource CardStyle}}">
                        <StackPanel>
                            <Grid Margin="0,0,0,20">
                                <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                                    <TextBlock Text="&#xE765;" FontFamily="{{StaticResource IconFont}}" FontSize="16" Foreground="{{StaticResource AccentColor}}" Margin="0,0,10,0"/>
                                    <TextBlock Text="{{Binding Strings.LabelToggle, FallbackValue='HOTKEYS'}}" FontSize="14" FontWeight="SemiBold" Foreground="{{StaticResource TextPrimary}}"/>
                                </StackPanel>
                            </Grid>
{hotkeys_panel}
                        </StackPanel>
                    </Border>

                    <!-- Card 3: Presets (共通) -->
                    <Border Style="{{StaticResource CardStyle}}" IsEnabled="{{Binding IsPro}}">
                        <StackPanel>
                            <Grid Margin="0,0,0,20">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
                                    <TextBlock Text="&#xE71B;" FontFamily="{{StaticResource IconFont}}" FontSize="16" Foreground="{{StaticResource AccentColor}}" Margin="0,0,10,0"/>
                                    <TextBlock Text="{{Binding Strings.HeaderPreset, FallbackValue='PRESETS'}}" FontSize="14" FontWeight="SemiBold" Foreground="{{StaticResource TextPrimary}}"/>
                                </StackPanel>
                                <TextBlock Grid.Column="1" Text="🔒 PRO" Foreground="{{StaticResource AccentColor}}" FontWeight="Bold" VerticalAlignment="Center"
                                           Visibility="{{Binding FreeBannerVisibility}}"/>
                            </Grid>

                            <Grid Margin="0,0,0,15">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="Auto"/>
                                    <ColumnDefinition Width="Auto"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                <ComboBox Grid.Column="0" x:Name="PresetComboBox" Style="{{StaticResource RoundedComboBox}}" 
                                          ItemsSource="{{Binding GlobalPresets}}" SelectedValue="{{Binding SelectedGlobalPresetId}}" 
                                          SelectedValuePath="Id" DisplayMemberPath="Name" SelectionChanged="GlobalPreset_SelectionChanged" Height="32"/>
                                <Button Grid.Column="1" Click="AddGlobalPreset_Click" Margin="8,0,0,0" Width="36" Height="32" ToolTip="{{Binding Strings.TooltipAddPreset}}">
                                    <TextBlock Text="&#xE710;" FontFamily="{{StaticResource IconFont}}" FontSize="12"/>
                                </Button>
                                <Button Grid.Column="2" Click="EditGlobalPresetName_Click" Margin="8,0,0,0" Width="36" Height="32" ToolTip="{{Binding Strings.TooltipEditPreset}}">
                                    <TextBlock Text="&#xE70F;" FontFamily="{{StaticResource IconFont}}" FontSize="12"/>
                                </Button>
                                <Button Grid.Column="3" Click="DeleteGlobalPreset_Click" Margin="8,0,0,0" Width="36" Height="32" ToolTip="{{Binding Strings.TooltipDeletePreset}}">
                                    <TextBlock Text="&#xE74D;" FontFamily="{{StaticResource IconFont}}" FontSize="12"/>
                                </Button>
                            </Grid>

                            <TextBlock Text="{{Binding Strings.LabelAssociatedProcesses}}" FontSize="12" Foreground="{{StaticResource TextSecondary}}" Margin="0,0,0,8"/>
                            <WrapPanel Orientation="Horizontal" Margin="0,0,0,10">
                                <ItemsControl ItemsSource="{{Binding SelectedGlobalPreset.ProcessRules}}">
                                    <ItemsControl.ItemsPanel>
                                        <ItemsPanelTemplate>
                                            <WrapPanel Orientation="Horizontal"/>
                                        </ItemsPanelTemplate>
                                    </ItemsControl.ItemsPanel>
                                    <ItemsControl.ItemTemplate>
                                        <DataTemplate>
                                            <Border Background="{{StaticResource ControlBackground}}" BorderBrush="{{StaticResource BorderBrush}}" BorderThickness="1" CornerRadius="13" Padding="8,4" Margin="0,0,8,8">
                                                <StackPanel Orientation="Horizontal">
                                                    <TextBlock Text="{{Binding ProcessName}}" FontSize="11" Foreground="{{StaticResource TextPrimary}}" VerticalAlignment="Center"/>
                                                    <Button Command="{{Binding DataContext.RemoveProcessRuleCommand, RelativeSource={{RelativeSource AncestorType=Window}}}}" CommandParameter="{{Binding}}" Background="Transparent" BorderThickness="0" Margin="6,0,0,0" Padding="0" Width="16" Height="16" Cursor="Hand">
                                                        <TextBlock Text="&#xE711;" FontFamily="{{StaticResource IconFont}}" FontSize="8" Foreground="{{StaticResource TextSecondary}}"/>
                                                    </Button>
                                                </StackPanel>
                                            </Border>
                                        </DataTemplate>
                                    </ItemsControl.ItemTemplate>
                                </ItemsControl>
                                <Button Click="ManageProcessRules_Click" Width="26" Height="26" VerticalAlignment="Top" Background="Transparent" Cursor="Hand" ToolTip="{{Binding Strings.BtnAddProcessRule}}">
                                    <Button.Template>
                                        <ControlTemplate TargetType="Button">
                                            <Border Background="{{TemplateBinding Background}}" BorderBrush="{{StaticResource BorderBrush}}" BorderThickness="1" CornerRadius="13">
                                                <TextBlock Text="&#xE710;" FontFamily="{{StaticResource IconFont}}" FontSize="12" Foreground="{{StaticResource TextPrimary}}" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                            </Border>
                                        </ControlTemplate>
                                    </Button.Template>
                                </Button>
                            </WrapPanel>
                        </StackPanel>
                    </Border>

                    <!-- Card 4: Startup & Close (共通) -->
                    <Border Style="{{StaticResource CardStyle}}">
                        <StackPanel>
{startup_grid}
                        </StackPanel>
                    </Border>
                </StackPanel>
            </ScrollViewer>
        </Grid>
    </Grid>
</components:FluentWindow>
"""

with open(xaml_path, "w", encoding="utf-8") as f_out:
    f_out.write(new_xaml)

print("MainWindow.xaml regenerated with Grid.Row fixed.")
