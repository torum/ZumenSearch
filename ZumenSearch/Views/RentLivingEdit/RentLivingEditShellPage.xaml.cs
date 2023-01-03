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

    public RentLivingEditShellPage()
    {
        ViewModel = App.GetService<RentLivingEditShellViewModel>();
        InitializeComponent();

        BreadcrumbBar1.ItemsSource = new ObservableCollection<Folder>{
            new Folder { Name = "物件情報の編集", Page = typeof(RentLivingSearchResultViewModel).FullName! },
        };

    }

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, Type Page)> _pages = new()
    {
        ("building", typeof(RentLivingEdit.RentLivingEditBuildingPage)),
        ("location", typeof(RentLivingEdit.RentLivingEditLocationPage)),
        ("transportation", typeof(RentLivingEdit.RentLivingEditTransportationPage)),
        ("appliance", typeof(RentLivingEdit.RentLivingEditAppliancePage)),
        ("pictures", typeof(RentLivingEdit.RentLivingEditPictureListPage)),
        ("units", typeof(RentLivingEdit.RentLivingEditUnitListPage)),
        ("zumen", typeof(RentLivingEdit.RentLivingEditZumenListPage)),
        ("kasinusi", typeof(RentLivingEdit.RentLivingEditKasinusiPage)),
        ("gyousya", typeof(RentLivingEdit.RentLivingEditGyousyaPage)),
    };

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Since we use ItemInvoked, we set selecteditem manually
        NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();

        // Pass Frame when navigate.  //, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft } //, new SuppressNavigationTransitionInfo() //new EntranceNavigationTransitionInfo()
        ContentFrame.Navigate(typeof(RentLivingEdit.RentLivingEditBuildingPage), ContentFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });

        // Listen to the window directly so the app responds to accelerator keys regardless of which element has focus.
        //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=  CoreDispatcher_AcceleratorKeyActivated;

        //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

        //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            //NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            /*
            var navItemTag = args.InvokedItemContainer.Tag.ToString();

            if (navItemTag is not null)
            {
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);

            }
            */
            if (_pages is null)
                return;

            var item = _pages.First(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            var _page = item.Page;

            if (_page is null)
                return;

            // Pass Frame when navigate.
            ContentFrame.Navigate(_page, ContentFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });//, args.RecommendedNavigationTransitionInfo
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        /*
        if (args.IsSettingsSelected == true)
        {
            //NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.SelectedItemContainer != null && args.SelectedItemContainer.Tag != null)
        {
            var navItemTag = args.SelectedItemContainer.Tag.ToString();

            if (navItemTag != null)
            {
                //NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);

                Debug.WriteLine("■■■SelectionChanged: "+ args.SelectedItemContainer.Tag.ToString());

                var item = _pages.First(p => p.Tag.Equals(args.SelectedItemContainer.Tag.ToString()));

                var _page = item.Page;
                if (_page is null)
                    return;

                var preNavPageType = ContentFrame.CurrentSourcePageType;
                if (preNavPageType is null)
                    return;
                if (Type.Equals(preNavPageType, _page))
                    return;

                ContentFrame.Navigate(_page, null, args.RecommendedNavigationTransitionInfo);
            }
        }
        */
    }

    private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
    {
        /*
        Type? _page = null;
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
            Debug.WriteLine("NavView_Navigate: " + _page.FullName);
        }
        */
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
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

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        //NavView.IsBackEnabled = ContentFrame.CanGoBack;

        if (ContentFrame.SourcePageType != null)
        {
            /*
            var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);
            // This only works for flat NavigationView
            NavView.SelectedItem = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .First(n => n.Tag.Equals(item.Tag));
            */
            NavView.Header = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();

        }
    }
}
