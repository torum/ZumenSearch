using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
//using ZumenSearch.ViewModels.Rent.Residentials.Editor.Units;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Editor.Modal;

public sealed partial class ModalShell : Page
{
    public ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel ViewModel
    {
        get;
    }

    // For uses of Navigation in other pages.
    public Frame NavFrame
    {
        get => ContentFrame;
    }

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, Type Page)> _pages =
    [
        ("room_basic", typeof(Views.Rent.Residentials.Editor.Modal.BasicPage)),
        ("room_status", typeof(Views.Rent.Residentials.Editor.Modal.StatusPage)),
        ("room_contract", typeof(Views.Rent.Residentials.Editor.Modal.ContractPage)),
        ("room_transaction", typeof(Views.Rent.Residentials.Editor.Modal.TransactionPage)),
        ("room_appliance", typeof(Views.Rent.Residentials.Editor.Modal.AppliancePage)),
        ("room_pictures", typeof(Views.Rent.Residentials.Editor.Modal.PicturePage)),
        ("room_zumen", typeof(Views.Rent.Residentials.Editor.Modal.ZumenPage)),
        ("room_kasinusi", typeof(Views.Rent.Residentials.Editor.Modal.KasinusiPage)),
        ("room_gyousya", typeof(Views.Rent.Residentials.Editor.Modal.GyousyaPage)),
    ];


    public ModalShell(ModalWindow dialogWindow)
    {
        ViewModel = new ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel();//App.GetService<RentLivingEditUnitShellViewModel>();
        InitializeComponent();


        dialogWindow.ExtendsContentIntoTitleBar = true;
        dialogWindow.SetTitleBar(AppTitleBar);

        dialogWindow.Activated += UnitsWindow_Activated;
        dialogWindow.Closed += UnitsWindow_Closed;




        /*
        BreadcrumbBarRoom.ItemsSource = new ObservableCollection<Folder>{
            new() { Name = "一覧", Page = typeof(RentLivingEditUnitListViewModel).FullName!},
            new() { Name = "編集", Page = typeof(RentLivingEditUnitEditShellViewModel).FullName! },
        };
        BreadcrumbBarRoom.ItemClicked += BreadcrumbBarRoom_ItemClicked;

        ViewModel.EventGoBack += (sender, arg) => OnEventGoBack(arg);
        */
    }

    public void UnitsWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

    }

    public void UnitsWindow_Closed(object sender, WindowEventArgs args)
    {
        //
    }

    private void BreadcrumbBarRoom_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        /*
        if (BreadcrumbBarRoom.ItemsSource is not ObservableCollection<Folder>)
        {
            return;
        }

        if (args.Index == 0)
        {
            if (ParentContentFrame == null)
            {
                return;
            }

            ParentContentFrame.Navigate(typeof(RentLivingEdit.RentLivingEditUnitListPage), ParentContentFrame, new EntranceNavigationTransitionInfo());
        }
        */
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        /*
        // Since we use ItemInvoked, we set selecteditem manually

        // Pass Frame when navigate.
        ContentFrame.Navigate(typeof(RentLivingEdit.Units.RentLivingEditUnitEditBasicPage), ContentFrame, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());

        // Listen to the window directly so the app responds to accelerator keys regardless of which element has focus.
        //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=  CoreDispatcher_AcceleratorKeyActivated;

        //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

        //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
        */

        NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();

        ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Editor.Modal.BasicPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        //throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            //NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {

            if (_pages is null)
            {
                return;
            }

            var item = _pages.First(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            var _page = item.Page;

            if (_page is null)
            {
                return;
            }

            ContentFrame.Navigate(_page, this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        }
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        //NavView.IsBackEnabled = ContentFrame.CanGoBack;
        /*
        if (ContentFrame.SourcePageType != null)
        {

            NavView.Header = ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();

        }
        */
    }

    public void OnEventGoBack(string arg)
    {
        // TODO: arg is temp.
        /*
        if (ParentContentFrame == null)
        {
            return;
        }

        // 
        if (ParentContentFrame.CanGoBack)
            ParentContentFrame.Navigate(typeof(RentLivingEditUnitListPage), ParentContentFrame, new EntranceNavigationTransitionInfo());
        */
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if ((e.Parameter is Frame) && (e.Parameter != null))
        {
            //ParentContentFrame = (e.Parameter as Frame);
        }

        base.OnNavigatedTo(e);
    }
}
