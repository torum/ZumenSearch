using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
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

    private NavigationViewItem? navigationViewSelectedItem;

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, Type? Page)> _pages =
    [
        ("room", null),
        ("room_summary", typeof(Views.Rent.Residentials.Editor.Modal.SummaryPage)),
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

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Since we use ItemInvoked, we set selecteditem manually
        /*
        // Pass Frame when navigate.
        ContentFrame.Navigate(typeof(RentLivingEdit.Units.RentLivingEditUnitEditBasicPage), ContentFrame, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());

        // Listen to the window directly so the app responds to accelerator keys regardless of which element has focus.
        //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=  CoreDispatcher_AcceleratorKeyActivated;

        //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

        //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
        */

        //NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
        var firstMenuItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
        if (firstMenuItem != null)
        {
            var childItem = firstMenuItem.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("room_summary"));
            if (childItem != null)
            {
                childItem.First().IsSelected = true;
                navigationViewSelectedItem = childItem.First();
            }
            else { Debug.WriteLine("No child menu item with tag 'room_summary' found in NavView."); }
        }
        else
        {
            Debug.WriteLine("No first menu item found in NavView.");
        }

        ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Editor.Modal.SummaryPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        //throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (_pages is null)
            return;

        if (args.IsSettingsInvoked == true)
        {
            //NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            if (args.InvokedItemContainer.Tag is not string tag || string.IsNullOrWhiteSpace(tag))
            {
                Debug.WriteLine("NavView_ItemInvoked: Invalid tag or null.");
                sender.SelectedItem = navigationViewSelectedItem;
                return;
            }

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            if (item.Page is null)
            {
                //Debug.WriteLine("NavView_ItemInvoked: Page is null for tag " + tag);
                sender.SelectedItem = navigationViewSelectedItem;

                return;
            }

            navigationViewSelectedItem = sender.SelectedItem as NavigationViewItem;

            ContentFrame.Navigate(item.Page, this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
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
