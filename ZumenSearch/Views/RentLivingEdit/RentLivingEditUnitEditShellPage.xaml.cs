using Microsoft.UI.Xaml;
using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using Windows.UI.Core;
using ZumenSearch.ViewModels.RentLivingEdit;
using System.Collections.ObjectModel;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.RentLivingEdit;

public sealed partial class RentLivingEditUnitEditShellPage : Page
{
    public RentLivingEditUnitEditShellViewModel ViewModel
    {
        get;
    }

    private Frame? ParentContentFrame;

    public RentLivingEditUnitEditShellPage()
    {
        ViewModel = new RentLivingEditUnitEditShellViewModel();//App.GetService<RentLivingEditUnitShellViewModel>();
        InitializeComponent();

        BreadcrumbBarRoom.ItemsSource = new ObservableCollection<Folder>{
            new Folder { Name = "一覧", Page = typeof(RentLivingEditUnitListViewModel).FullName!},
            new Folder { Name = "編集", Page = typeof(RentLivingEditUnitEditShellViewModel).FullName! },
        };
        BreadcrumbBarRoom.ItemClicked += BreadcrumbBarRoom_ItemClicked;

        ViewModel.eventGoBack += (sender, arg) => OnEventGoBack(arg);
    }

    private void BreadcrumbBarRoom_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (BreadcrumbBarRoom.ItemsSource is not ObservableCollection<Folder>) 
            return;

        if (args.Index == 0)
        {
            if (ParentContentFrame == null)
                return;

            ParentContentFrame.Navigate(typeof(RentLivingEdit.RentLivingEditUnitListPage), ParentContentFrame, new EntranceNavigationTransitionInfo());
        }
    }

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, Type Page)> _pages = new()
    {
        ("room_basic", typeof(RentLivingEdit.Units.RentLivingEditUnitEditBasicPage)),
        ("room_status", typeof(RentLivingEdit.Units.RentLivingEditUnitEditStatusPage)),
        ("room_contract", typeof(RentLivingEdit.Units.RentLivingEditUnitEditContractPage)),
        ("room_transaction", typeof(RentLivingEdit.Units.RentLivingEditUnitEditTransactionPage)),
        ("room_appliance", typeof(RentLivingEdit.Units.RentLivingEditUnitEditAppliancePage)),
        ("room_pictures", typeof(RentLivingEdit.Units.RentLivingEditUnitEditPicturePage)),
        ("room_zumen", typeof(RentLivingEdit.Units.RentLivingEditUnitEditZumenPage)),
    };

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Since we use ItemInvoked, we set selecteditem manually
        NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();

        // Pass Frame when navigate.
        ContentFrame.Navigate(typeof(RentLivingEdit.Units.RentLivingEditUnitEditBasicPage), ContentFrame, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());

        // Listen to the window directly so the app responds to accelerator keys regardless of which element has focus.
        //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=  CoreDispatcher_AcceleratorKeyActivated;

        //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

        //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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

            ContentFrame.Navigate(_page, ContentFrame, args.RecommendedNavigationTransitionInfo);
        }
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

    public void OnEventGoBack(string arg)
    {
        // TODO: arg is temp.

        if (ParentContentFrame == null)
            return;

        // 
        if (ParentContentFrame.CanGoBack)
            ParentContentFrame.Navigate(typeof(RentLivingEditUnitListPage), ParentContentFrame, new EntranceNavigationTransitionInfo());
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is Frame) && (e.Parameter != null))
        {
            ParentContentFrame = (e.Parameter as Frame);
        }

        base.OnNavigatedTo(e);
    }
}
