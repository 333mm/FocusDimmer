# Implementation Plan - ContextMenu Styling for Visibility

## Goal

Ensure the right-click `ContextMenu` has readable text and background colors regardless of whether the OS is in Light or Dark mode.

## Proposed Changes

### 1. [App.xaml](file:///d:/Dev/FocusDimmer2/FocusDimmer2/App.xaml)

- Add a new `Style` for `ContextMenu` and `MenuItem`.
- Use `DynamicResource` with system color keys for background and foreground to automatically follow OS theme.
  - Background: `SystemColors.WindowBrushKey` or a custom dark background that is readable.
  - Foreground: `SystemColors.WindowTextBrushKey`.
- Alternatively, since the app is primarily dark-themed, we can define a fixed "Dark Fluent" style for menus that ensures readability against dark backgrounds.
- **Decision**: Use `SystemColors` for background/foreground to strictly follow user's "consider OS mode" request, but apply a Fluent-like padding and corner radius.

### 2. Implementation Details

```xml
<Style TargetType="{x:Type ContextMenu}">
    <Setter Property="Background" Value="{DynamicResource {x:Static SystemColors.WindowBrushKey}}"/>
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="Padding" Value="2"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type ContextMenu}">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4">
                    <ItemsPresenter Margin="4"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<Style TargetType="{x:Type MenuItem}">
    <Setter Property="Foreground" Value="{DynamicResource {x:Static SystemColors.WindowTextBrushKey}}"/>
    <Setter Property="Padding" Value="10,6"/>
    <!-- ... Triggers for Hover ... -->
</Style>
```

## Verification Plan

1. **Light Mode**: Set OS to Light Mode. Right-click. Verify background is light and text is dark.
2. **Dark Mode**: Set OS to Dark Mode. Right-click. Verify background is dark and text is light.
3. **App Consistency**: Ensure the menu doesn't look too "classic" compared to the rest of the Fluent UI.
