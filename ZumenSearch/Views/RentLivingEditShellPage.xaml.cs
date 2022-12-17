using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using ZumenSearch.ViewModels;
using Microsoft.UI.Xaml.Media.Animation;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.Views;



public sealed partial class RentLivingEditShellPage : Page
{
    public RentLivingEditShellViewModel ViewModel
    {
        get;
    }

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
{
        ("building", typeof(RentLivingEditBuildingPage)),
        ("location", typeof(RentLivingEditLocationPage)),
        ("transportation", typeof(RentLivingEditTransportationPage)),
        ("appliance", typeof(RentLivingEditAppliancePage)),
        ("pictures", typeof(RentLivingEditPictureShellPage)),
        ("units", typeof(RentLivingEditUnitShellPage)),
        ("zumen", typeof(RentLivingEditZumenPage)),
    };

    public RentLivingEditShellPage()
    {
        ViewModel = App.GetService<RentLivingEditShellViewModel>();
        InitializeComponent();

        // [BreadcrumbBar]
        // BreadcrumbBar1.ItemsSource = new string[] { "条件検索", "検索結果一覧" };
        BreadcrumbBar1.ItemsSource = new ObservableCollection<Folder>{
            new Folder { Name = "物件情報の入力", Page = typeof(RentLivingSearchResultViewModel).FullName! },
        };

        //BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        /*
        if (BreadcrumbBar1.ItemsSource is not ObservableCollection<Folder> items) return;

        if (args.Index == 0)
        {
            ViewModel.NavigationService.NavigateTo(items[args.Index].Page);
            Debug.WriteLine(items[args.Index].Page);
        }
        */
    }

    // Add "using" for WinUI controls.
    // using muxc = Microsoft.UI.Xaml.Controls;

    private double NavViewCompactModeThresholdWidth
    {
        get
        {
            return NavView.CompactModeThresholdWidth;
        }
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }


    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // You can also add items in code.
        /*
        NavView.MenuItems.Add(new NavigationViewItemSeparator());
        NavView.MenuItems.Add(new NavigationViewItem
        {
            Content = "My content",
            Icon = new SymbolIcon((Symbol)0xF1AD),
            Tag = "content"
        });
        _pages.Add(("content", typeof(MyContentPage)));
        */

        // Add handler for ContentFrame navigation.
        ContentFrame.Navigated += On_Navigated;

        // NavView doesn't load any page by default, so load home page.
        NavView.SelectedItem = NavView.MenuItems[0];
        // If navigation occurs on SelectionChanged, this isn't needed.
        // Because we use ItemInvoked to navigate, we need to call Navigate
        // here to load the home page.
        NavView_Navigate("building", new EntranceNavigationTransitionInfo());

        // Listen to the window directly so the app responds
        // to accelerator keys regardless of which element has focus.
        //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=  CoreDispatcher_AcceleratorKeyActivated;

        //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

        //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.InvokedItemContainer != null)
        {
            var navItemTag = args.InvokedItemContainer.Tag.ToString();
            NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void NavView_Navigate(
        string navItemTag,
        NavigationTransitionInfo transitionInfo)
    {
        Type _page = null;
        if (navItemTag == "settings")
        {
            //_page = typeof(SettingsPage);
        }
        else
        {
            var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
            _page = item.Page;
        }
        // Get the page type before navigation so you can prevent duplicate
        // entries in the backstack.
        var preNavPageType = ContentFrame.CurrentSourcePageType;

        // Only navigate if the selected page isn't currently loaded.
        if (!(_page is null) && !Type.Equals(preNavPageType, _page))
        {
            ContentFrame.Navigate(_page, null, transitionInfo);
        }
    }

    private void NavView_BackRequested(NavigationView sender,
                                       NavigationViewBackRequestedEventArgs args)
    {
        TryGoBack();
    }

    private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
    {
        // When Alt+Left are pressed navigate back
        if (e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown
            && e.VirtualKey == VirtualKey.Left
            && e.KeyStatus.IsMenuKeyDown == true
            && !e.Handled)
        {
            e.Handled = TryGoBack();
        }
    }

    private void System_BackRequested(object sender, BackRequestedEventArgs e)
    {
        if (!e.Handled)
        {
            e.Handled = TryGoBack();
        }
    }

    private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
    {
        // Handle mouse back button.
        if (e.CurrentPoint.Properties.IsXButton1Pressed)
        {
            e.Handled = TryGoBack();
        }
    }

    private bool TryGoBack()
    {
        if (!ContentFrame.CanGoBack)
            return false;

        // Don't go back if the nav pane is overlayed.
        if (NavView.IsPaneOpen &&
            (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
             NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
            return false;

        ContentFrame.GoBack();
        return true;
    }

    private void On_Navigated(object sender, NavigationEventArgs e)
    {
        //NavView.IsBackEnabled = ContentFrame.CanGoBack;
        /*
        if (ContentFrame.SourcePageType == typeof(SettingsPage))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
            NavView.Header = "Settings";
        }
        else if (ContentFrame.SourcePageType != null)
        {
            var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

            NavView.SelectedItem = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .First(n => n.Tag.Equals(item.Tag));

            NavView.Header =
                ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
        }
        */
        if (ContentFrame.SourcePageType != null)
        {
            var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

            // This only works for flat NavigationView
            NavView.SelectedItem = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .First(n => n.Tag.Equals(item.Tag));

            NavView.Header =
                ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
        }
    }
}
