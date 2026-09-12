using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views;

public sealed partial class ShellPage : Page
{
    public ViewModels.MainViewModel ViewModel { get; }
    public Frame NavigationFrame => this.ContentFrame;

    private MainWindow? _mainWindow;
    private readonly INavigationService _navigationService;

    public ShellPage(ViewModels.MainViewModel viewModel, INavigationService navigationService)
    {
        ViewModel = viewModel;
        _navigationService = navigationService;

        InitializeComponent();

        this.Loaded += Page_Loaded;
    }

    public void CallMeAfterMainWindowIsCreated(MainWindow wnd)
    {
        _mainWindow = wnd;

        // Set the title bar to content in the custom title bar grid.
        wnd.SetTitleBar(this.AppTitleBar);

        wnd.Activated += MainWindow_Activated;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (this.ContentFrame.Navigate(typeof(ZumenSearch.Views.SearchPage), null, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()))
        {
            SetRegionsForCustomTitleBar("Page_Loaded");
        }
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_DisplayModeChanged");
    }

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_Loaded");
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        this.AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            this.AppTitleBarIcon.Opacity = 0.5;
        }
        else
        {
            this.AppTitleBarIcon.Opacity = 1;
        }
    }

    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        SetRegionsForCustomTitleBar("AppTitleBar_SizeChanged");
    }

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (args.KeyboardAccelerator.Key == Windows.System.VirtualKey.F1)
        {
            // set this first.
            args.Handled = true;

            // TODO:

            return;
        }

        if (args.KeyboardAccelerator.Modifiers == Windows.System.VirtualKeyModifiers.Menu)
        {
            if (args.KeyboardAccelerator.Key == Windows.System.VirtualKey.Left)
            {
                // set this first.
                args.Handled = true;

                if (this.ContentFrame != null && this.ContentFrame.CanGoBack)
                {
                    this.ContentFrame.GoBack();
                }

                return;
            }

        }
    }

    private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            _navigationService.NavigateTo("ZumenSearch.Views.SettingsPage", SlideNavigationTransitionEffect.FromBottom);
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            _navigationService.NavigateTo(args.InvokedItemContainer.Tag, SlideNavigationTransitionEffect.FromBottom);
        }
        else
        {
            Debug.WriteLine("NavigationViewControl_ItemInvoked: No valid item invoked. IsSettingsInvoked: " + args.IsSettingsInvoked + ", InvokedItemContainer: " + (args.InvokedItemContainer != null) + ", Tag: " + (args.InvokedItemContainer?.Tag != null));
        }
    }

    private void NavigationViewControl_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        this.NavigationViewControl.IsBackEnabled = this.ContentFrame.CanGoBack;

        if (this.ContentFrame.SourcePageType == typeof(Views.SettingsPage))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            this.NavigationViewControl.SelectedItem = (NavigationViewItem)this.NavigationViewControl.SettingsItem;

            // Hide SearchBox
            //SearchBox.Visibility = Visibility.Visible;
            //SetRegionsForCustomTitleBar("NavigationViewControl_Navigated");
        }
        else if (this.ContentFrame.SourcePageType != null)
        {
            var selectedItem = FindNavigationViewItemWithTag(this.ContentFrame.SourcePageType.FullName!);
            if (selectedItem != null)
            {
                this.NavigationViewControl.SelectedItem = selectedItem;
                //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
            }
            else
            {
                //Debug.WriteLine("No menu item with tag matching the current page found in NavView. Current page: " + ContentFrame.SourcePageType.FullName);
            }

            if (ContentFrame.SourcePageType == typeof(Views.SearchPage))
            {
                // Hide SearchBox
                //SearchBox.Visibility = Visibility.Collapsed;
                //SetRegionsForCustomTitleBar("NavigationViewControl_Navigated");
            }
            else
            {
                //SearchBox.Visibility = Visibility.Visible;
                //SetRegionsForCustomTitleBar("NavigationViewControl_Navigated");
            }
        }
    }

    private NavigationViewItem? FindNavigationViewItemWithTag(string tag)
    {
        foreach (var item in this.NavigationViewControl.MenuItems.OfType<NavigationViewItem>())
        {
            if (item.Tag.Equals(tag))
            {
                this.NavigationViewControl.SelectedItem = item;
                return item;
            }

            if (item.MenuItems.Count > 0)
            {
                foreach (var subItem in item.MenuItems.OfType<NavigationViewItem>())
                {
                    if (subItem.Tag.Equals(tag))
                    {
                        this.NavigationViewControl.SelectedItem = subItem;
                        return subItem;
                    }
                }
            }
        }

        return null;
    }

    private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (this.ContentFrame.CanGoBack) this.ContentFrame.GoBack();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.ContentFrame.CanGoBack) this.ContentFrame.GoBack();
    }

    private void NavigationViewControl_PaneOpened(NavigationView sender, object args)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_PaneOpened");
    }

    private void NavigationViewControl_PaneClosed(NavigationView sender, object args)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_PaneClosed");
    }

    private void AppTitleBarGrid_Loaded(object sender, RoutedEventArgs e)
    {
        SetRegionsForCustomTitleBar("AppTitleBarGrid_Loaded");
    }

    private void SetRegionsForCustomTitleBar(string str)
    {
        if (_mainWindow is null)
        {
            Debug.WriteLine($"{str} MainWindow is null. Cannot set regions for custom title bar.");
            return;
        }

        if (this.XamlRoot is null)
        {
            Debug.WriteLine($"{str} XamlRoot is null. Cannot set regions for custom title bar.");
            return;
        }

        var scaleAdjustment = this.XamlRoot.RasterizationScale;

        /*
        // SearchBox size
        var width = this.SearchBox.ActualWidth;//ActualWidth won't work in certain cases. e.g. when visivility changed.
        var height = this.SearchBox.ActualHeight;//ActualHeight won't work in certain cases. e.g. when visivility changed.

        if (this.SearchBox.Visibility != Visibility.Visible)
        {
            //Debug.WriteLine("SearchBox.Visibility != Visibility.Visible");
            width = 0;
            height = 0;
        }

        GeneralTransform transform1 = this.SearchBox.TransformToVisual(null);
        Windows.Foundation.Rect bounds1 = transform1.TransformBounds(new Windows.Foundation.Rect(0, 0,
                                                         width,
                                                         height));
        Windows.Graphics.RectInt32 SearchBoxRect = GetRect(bounds1, scaleAdjustment);
        */
        // Back button size
        var width2 = this.BackButton.ActualWidth;//ActualWidth won't work in certain cases. e.g. when visivility changed.
        var height2 = this.BackButton.ActualHeight;//ActualHeight won't work in certain cases. e.g. when visivility changed.

        if (this.BackButton.Visibility != Visibility.Visible)
        {
            //Debug.WriteLine("BackButton.Visibility != Visibility.Visible");
            width2 = 0;
            height2 = 0;
        }

        GeneralTransform transform2 = this.BackButton.TransformToVisual(null);
        Windows.Foundation.Rect bounds2 = transform2.TransformBounds(new Windows.Foundation.Rect(0, 0,
                                                    width2,
                                                    height2));
        Windows.Graphics.RectInt32 BackButtonRect = GetRect(bounds2, scaleAdjustment);

        var rectArray = new Windows.Graphics.RectInt32[] { BackButtonRect };//SearchBoxRect, SettingsButton

        InputNonClientPointerSource nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(_mainWindow.AppWindow.Id);
        nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);

        //Debug.WriteLine($"{str} SetRegionsForCustomTitleBar called.");
    }

    private static Windows.Graphics.RectInt32 GetRect(Windows.Foundation.Rect bounds, double scale)
    {
        return new Windows.Graphics.RectInt32(
            _X: (int)Math.Round(bounds.X * scale),
            _Y: (int)Math.Round(bounds.Y * scale),
            _Width: (int)Math.Round(bounds.Width * scale),
            _Height: (int)Math.Round(bounds.Height * scale)
        );
    }
}
