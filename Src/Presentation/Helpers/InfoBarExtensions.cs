using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace ZumenSearch.Presentation.Helpers;

public static class InfoBarExtensions
{
    public static readonly DependencyProperty IsTextSelectionEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsTextSelectionEnabled",
            typeof(bool),
            typeof(InfoBarExtensions),
            new PropertyMetadata(false, OnIsTextSelectionEnabledChanged));

    public static bool GetIsTextSelectionEnabled(DependencyObject obj) =>
        (bool)obj.GetValue(IsTextSelectionEnabledProperty);

    public static void SetIsTextSelectionEnabled(DependencyObject obj, bool value) =>
        obj.SetValue(IsTextSelectionEnabledProperty, value);

    private static void OnIsTextSelectionEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InfoBar infoBar) return;

        // Clean up existing handlers to prevent memory leaks
        infoBar.Loaded -= InfoBar_Loaded;
        infoBar.Unloaded -= InfoBar_Unloaded;

        if ((bool)e.NewValue)
        {
            infoBar.Loaded += InfoBar_Loaded;
            infoBar.Unloaded += InfoBar_Unloaded;

            // Watch IsOpen in case the template is applied after Loaded
            long token = infoBar.RegisterPropertyChangedCallback(InfoBar.IsOpenProperty, OnIsOpenChanged);
            infoBar.Tag = token;

            ApplyTextSelection(infoBar);
        }
    }

    private static void InfoBar_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is InfoBar infoBar)
        {
            ApplyTextSelection(infoBar);
        }
    }

    private static void OnIsOpenChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (sender is InfoBar infoBar && infoBar.IsOpen)
        {
            // Queue on DispatcherQueue so the visual tree finishes expanding
            infoBar.DispatcherQueue?.TryEnqueue(() => ApplyTextSelection(infoBar));
        }
    }

    private static void InfoBar_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is InfoBar infoBar && infoBar.Tag is long token)
        {
            infoBar.UnregisterPropertyChangedCallback(InfoBar.IsOpenProperty, token);
            infoBar.Tag = null;
        }
    }

    private static void ApplyTextSelection(DependencyObject parent)
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBlock textBlock &&
               (textBlock.Name == "Title" || textBlock.Name == "Message"))
            {
                textBlock.IsTextSelectionEnabled = true;
            }
            ApplyTextSelection(child);
        }
    }
}
