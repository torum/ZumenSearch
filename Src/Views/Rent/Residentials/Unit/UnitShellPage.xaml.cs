using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using Windows.UI.Core;
using ZumenSearch.Models;
using ZumenSearch.Services;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Rent.Residentials.Unit;

public sealed partial class UnitShellPage : Page
{
    public ViewModels.Rent.ResidentialsViewModel? ViewModel
    {
        get;private set;
    }

    private NavigationViewItem? navigationViewSelectedItem;

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, Type? Page)> _pages =
    [
        ("room", null),
        ("room_summary", typeof(Views.Rent.Residentials.Unit.BasicPage)),
        ("room_status", typeof(Views.Rent.Residentials.Unit.StatusPage)),
        ("room_contract", typeof(Views.Rent.Residentials.Unit.ContractPage)),
        ("room_transaction", typeof(Views.Rent.Residentials.Unit.TransactionPage)),
        ("room_appliance", typeof(Views.Rent.Residentials.Unit.AppliancePage)),
        ("room_pictures", typeof(Views.Rent.Residentials.Unit.PicturePage)),
        ("room_zumen", typeof(Views.Rent.Residentials.Unit.ZumenPage)),
        ("room_kasinusi", typeof(Views.Rent.Residentials.Unit.KasinusiPage)),
        ("room_gyousya", typeof(Views.Rent.Residentials.Unit.GyousyaPage)),
    ];

    private readonly IDispatcherService _dispatcherService;
    private readonly IModalDialogService _dlg;

    public UnitShellPage() : this(App.GetService<IModalDialogService>(), App.GetService<IDispatcherService>())
    {
        // parameterless ctor used by XAML activator; chains to DI ctor
    }

    public UnitShellPage(IModalDialogService modalDialog, IDispatcherService dispatcherService)//MainWindow mainWindow, IDispatcherService dispatcherService, ModalWindow dialogWindow, ViewModels.Rent.Residentials.Unit.ModalViewModel vm, ViewModels.Rent.Residentials.ResidentialsViewModel editorVm
    {
        _dlg = modalDialog;
        _dispatcherService = dispatcherService;

        InitializeComponent();

        /*
        ViewModel = vm;//new ViewModels.Rent.Residentials.Editor.Modal.ModalViewModel();//App.GetService<RentLivingEditUnitShellViewModel>();
        EditorVM = editorVm;
        _mainWindow = mainWindow;
        _dispatcherService = dispatcherService;

        // Subscribe to ViewModel's events
        ViewModel.EventBackToSummary += (sender, arg) => OnEventBackToSummary(arg);

        InitializeComponent();

        dialogWindow.ExtendsContentIntoTitleBar = true;
        dialogWindow.SetTitleBar(AppTitleBar);

        dialogWindow.Activated += UnitsWindow_Activated;
        dialogWindow.Closed += UnitsWindow_Closed;
        */
    }

    public void UnitsWindow_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        //var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        //AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

    }

    public void UnitsWindow_Closed(object sender, WindowEventArgs args)
    {
        //
        /*
        if (sender is ModalWindow mwin)
        {
            // Save window size and position.
            var appWindow = mwin.AppWindow;
            if (appWindow != null)
            {
                if (appWindow.Presenter is OverlappedPresenter)
                {
                    EditorVM.ModalWinWidth = (int)appWindow.Size.Width;
                    EditorVM.ModalWinHeight = (int)appWindow.Size.Height;
                    EditorVM.ModalWinTop = (int)appWindow.Position.Y;
                    EditorVM.ModalWinLeft = (int)appWindow.Position.X;
                }
            }
            else
            {
                //Debug.WriteLine("appWindow is null");
            }
        }
        */
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

        NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
        navigationViewSelectedItem = NavView.SelectedItem as NavigationViewItem;

        /*
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
        */

        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Unit.BasicPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
        {
            BreadcrumbBar1.ItemsSource = new ObservableCollection<Breadcrumb>{
                new() { Name = "建物", Page = typeof(Views.Rent.Residentials.Bldg.BasicPage).FullName!},
                new() { Name = "部屋", Page = typeof(Views.Rent.Residentials.Unit.BasicPage).FullName!},
                new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Unit.BasicPage).FullName!},
            };

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("room_summary")).First();
        }
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        //throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (_pages is null)
        {
            return;
        }

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

            ContentFrame.Navigate(item.Page, ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
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

    public void OnEventBackToSummary(string arg)
    {
        _dispatcherService?.TryEnqueue(() =>
        {
            ContentFrame.Navigate(typeof(Views.Rent.Residentials.Unit.BasicPage), ViewModel, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom });
        });
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
        if ((e.Parameter is ViewModels.Rent.ResidentialsViewModel) && (e.Parameter != null))
        {
            //_editorShell = e.Parameter as Views.Rent.Residentials.EditorShell;
            ViewModel = e.Parameter as ViewModels.Rent.ResidentialsViewModel;

            ViewModel?.SetUnitShell(this);
        }
        else
        {
            Debug.WriteLine("UnitShellPage.OnNavigatedTo: Invalid parameter. Expected ResidentialsViewModel.");
        }

        base.OnNavigatedTo(e);
    }

}
