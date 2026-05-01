using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using ZumenSearch.Services;

namespace ZumenSearch.Views;

public sealed partial class ShellPage : Page
{
    public Frame NavigationFrame => ContentFrame;

    private MainWindow? _mainWindow;
    private bool _activated;

    private readonly INavigationService _navigationService;
    //private readonly MainViewModel _viewModel;

    public ShellPage(INavigationService navigationService)//MainViewModel vm, 
    {
        //_viewModel = vm ?? throw new ArgumentNullException(nameof(vm));

        _navigationService = navigationService;

        InitializeComponent();

    }

    public void CallMeWhenMainWindowIsReady(MainWindow wnd)
    {
        _mainWindow = wnd;

        wnd.SetTitleBar(AppTitleBar);

        wnd.Activated += MainWindow_Activated;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Note: ContentFrame as param (instead of ViewModel) is expected by SearchPage for further navigation, such as navigating to SearchResultPage.
        ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.ResidentialSearchPage), ContentFrame, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());//, //
        
        var selectedItem = FindNavigationViewItemWithTag("ZumenSearch.Views.Rent.ResidentialSearchPage");
        if (selectedItem != null)
        {
            NavigationViewControl.SelectedItem = selectedItem;
            //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
        }
        else
        {
            Debug.WriteLine("No menu item with tag matching the current page found in NavigationViewControl @ZumenSearch.Views.ShellPage. Current page: " + ContentFrame.SourcePageType.FullName);
        }

        SetRegionsForCustomTitleBar("Page_Loaded");
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            AppTitleBarIcon.Opacity = 0.5;
        }
        else
        {
            AppTitleBarIcon.Opacity = 1;

            if (!_activated)
            {
                _activated = true;

                //Debug.WriteLine($"{sender}");
                if (sender is MainWindow wnd)
                {
                    _mainWindow = wnd;
                }
            }
        }
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        /*
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
        */

        SetRegionsForCustomTitleBar("NavigationViewControl_DisplayModeChanged");
    }

    private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        SetRegionsForCustomTitleBar("NavigationViewControl_Loaded");


        // Since we use ItemInvoked, we set selecteditem manually
        //NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
        /*
        var firstMenuItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
        if (firstMenuItem != null)
        {
            var childItem = firstMenuItem.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("ZumenSearch.Views.Rent.RentPage"));
            if (childItem != null)
            {
                childItem.First().IsSelected = true;
                navigationViewSelectedItem = childItem.First();
            }
            else { Debug.WriteLine("No child menu item with tag 'RentResidentials' found in NavView."); }
        }
        else
        {
            Debug.WriteLine("No first menu item found in NavView.");
        }
        */
        /*
        var childItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("RentResidentials"));
        if (childItem != null)
        {
            childItem.First().IsSelected = true;
            navigationViewSelectedItem = childItem.First();
        }
        else 
        { 
            Debug.WriteLine("No child menu item with tag 'RentResidentials' found in NavView."); 
        }
        */

        // Pass Frame when navigate.  //, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft } //, new SuppressNavigationTransitionInfo() //new EntranceNavigationTransitionInfo()
        //NavigationFrame.Navigate(typeof(Rent.RentSearchPage), NavigationFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
        //NavigationFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), NavigationFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });//
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



        /*
        if (_pages is null)
        {
            return;
        }

        if (args.IsSettingsInvoked == true)
        {
            navigationViewSelectedItem = sender.SelectedItem as NavigationViewItem;

            NavigationFrame.Navigate(typeof(SettingsPage), NavigationFrame, new SuppressNavigationTransitionInfo());//, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {

            if (args.InvokedItemContainer.Tag is not string tag || string.IsNullOrWhiteSpace(tag))
            {
                Debug.WriteLine("NavigationViewControl_ItemInvoked: Invalid tag or null.");
                //sender.SelectedItem = navigationViewSelectedItem;
                return;
            }


            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            if (item.Page is null)
            {
                Debug.WriteLine("NavView_ItemInvoked: Page is null for tag " + tag);
                // Don't. crash when complact menu.
                //sender.SelectedItem = navigationViewSelectedItem;

                return;
            }

            navigationViewSelectedItem = sender.SelectedItem as NavigationViewItem;

            // Pass Frame when navigate.
            NavigationFrame.Navigate(item.Page, NavigationFrame, new SuppressNavigationTransitionInfo());//args.RecommendedNavigationTransitionInfo
        }
        */
    }

    private void NavigationViewControl_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        NavigationViewControl.IsBackEnabled = ContentFrame.CanGoBack;
        
        if (ContentFrame.SourcePageType == typeof(Views.SettingsPage))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
        }
        else if (ContentFrame.SourcePageType != null)
        {
            var selectedItem = FindNavigationViewItemWithTag(ContentFrame.SourcePageType.FullName!);
            if (selectedItem != null) 
            {
                NavigationViewControl.SelectedItem = selectedItem;
                //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
            }
            else
            {
                //Debug.WriteLine("No menu item with tag matching the current page found in NavView. Current page: " + ContentFrame.SourcePageType.FullName);
            }
        }
    }

    private NavigationViewItem? FindNavigationViewItemWithTag(string tag)
    {
        foreach (var item in NavigationViewControl.MenuItems.OfType<NavigationViewItem>())
        {
            if (item.Tag.Equals(tag))
            {
                NavigationViewControl.SelectedItem = item;
                return item;
            }

            if (item.MenuItems.Count > 0)
            {
                foreach (var subItem in item.MenuItems.OfType<NavigationViewItem>())
                {
                    if (subItem.Tag.Equals(tag))
                    {
                        NavigationViewControl.SelectedItem = subItem;
                        return subItem;
                    }
                }
            }
        }

        return null;
    }

    private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack) ContentFrame.GoBack();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.CanGoBack) ContentFrame.GoBack();
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

    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        SetRegionsForCustomTitleBar("AppTitleBar_SizeChanged");
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

        // Back button size
        var width = this.SearchBox.Width;//ActualWidth won't work in certain cases.
        var height = this.SearchBox.Height;//ActualHeight won't work in certain cases.

        if (this.SearchBox.Visibility != Visibility.Visible)
        {
            Debug.WriteLine("SearchBox.Visibility != Visibility.Visible");
            width = 0;
            height = 0;
        }

        GeneralTransform transform1 = this.SearchBox.TransformToVisual(null);
        Windows.Foundation.Rect bounds1 = transform1.TransformBounds(new Windows.Foundation.Rect(0, 0,
                                                         width,
                                                         height));
        Windows.Graphics.RectInt32 SearchBoxRect = GetRect(bounds1, scaleAdjustment);

        // Back button size
        var width2 = this.BackButton.Width;//ActualWidth won't work in certain cases.
        var height2 = this.BackButton.Height;//ActualHeight won't work in certain cases.

        if (this.BackButton.Visibility != Visibility.Visible)
        {
            Debug.WriteLine("BackButton.Visibility != Visibility.Visible");
            width2 = 0;
            height2 = 0;
        }

        GeneralTransform transform2 = this.BackButton.TransformToVisual(null);
        Windows.Foundation.Rect bounds2 = transform2.TransformBounds(new Windows.Foundation.Rect(0, 0,
                                                    width2,
                                                    height2));
        Windows.Graphics.RectInt32 BackButtonRect = GetRect(bounds2, scaleAdjustment);

        var rectArray = new Windows.Graphics.RectInt32[] { SearchBoxRect, BackButtonRect };//, SettingsButton

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
