using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;

namespace ZumenSearch.Views.Rent.Residentials.Listing;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Residentials.Listing.ListingViewModel ViewModel { get; }

    public Views.Rent.Residentials.Listing.EditorWindow Window { get; }

    public Frame NavigationFrame => ContentFrame;

    private bool _nvigated;
    //private bool _initialized;

    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        ("room", "", null),
        ("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage", "基本", typeof(Views.Rent.Residentials.Listing.BasicPage)),
        ("ZumenSearch.Views.Rent.Residentials.Listing.ContractPage", "契約条件", typeof(Views.Rent.Residentials.Listing.ContractPage)),
        ("ZumenSearch.Views.Rent.Residentials.Listing.TransactionPage", "取引条件", typeof(Views.Rent.Residentials.Listing.TransactionPage)),
        ("ZumenSearch.Views.Rent.Residentials.Listing.AppliancesPage", "設備", typeof(Views.Rent.Residentials.Listing.AppliancesPage)),
        ("ZumenSearch.Views.Rent.Residentials.Listing.PictureListPage", "写真", typeof(Views.Rent.Residentials.Listing.PictureListPage)),
        ("ZumenSearch.Views.Rent.Residentials.Listing.ZumenListPage", "図面", typeof(Views.Rent.Residentials.Listing.ZumenListPage)),
        ("ZumenSearch.Views.Rent.Residentials.Listing.LessorListPage", "貸主", typeof(Views.Rent.Residentials.Listing.LessorListPage)),
        ("ZumenSearch.Views.Rent.Residentials.Listing.BrokerListPage", "宅建業者", typeof(Views.Rent.Residentials.Listing.BrokerListPage)),
    ];

    private readonly INavigationGenericService _navigationlService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;

    public ShellPage(
        Models.Rent.Residentials.Listing.Listing room, 
        IAbstractFactory<Models.Rent.Residentials.Listing.Listing, Services.Contracts.INavigationGenericService, IDialogGenericService, ViewModels.Rent.Residentials.Listing.ListingViewModel> vmFactory,
        INavigationGenericService navigationlService, 
        IDispatcherService dispatcherService,
        IDialogGenericService dialogService)
    {
        //Debug.WriteLine($"ShellPage {entry.Id}");

        _navigationlService = navigationlService;
        _dispatcherService = dispatcherService;
        _dialogService = dialogService;

        ViewModel = vmFactory.Create(room, _navigationlService, _dialogService);

        Window = new Views.Rent.Residentials.Listing.EditorWindow(room.Id, ViewModel)
        {
            Content = this
        };

        InitializeComponent();

        _navigationlService.Initialize(this.ContentFrame, _pages);

        this.Loaded += ShellPage_Loaded;
        this.Unloaded += ShellPage_Unloaded;
        this.BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        Window.Title = "賃貸住居用：部屋";
        Window.ExtendsContentIntoTitleBar = true;
        Window.Activated += Window_Activated;
        Window.Closed += Window_Closed;
        Window.AppWindow.Closing += AppWindow_Closing;
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        // XamlRoot is no longer null.
        _dialogService.Initialize(this.XamlRoot, Window);
        /*
        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Listing.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))
        {

        }
        */
    }

    private void ShellPage_Unloaded(object sender, RoutedEventArgs e)
    {
        //
    }

    public void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

        BreadcrumbBar1.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.5 : 1;
        NavView.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.7 : 1;
        //ContentFrame.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.7 : 1;
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (ViewModel.IsDirty)
        {
            args.Cancel = true; // needs Cancel = true here in order to show dialog.

            await ShowEditorCloseConfirmationDialog();
        }
    }

    public async Task ShowEditorCloseConfirmationDialog()
    {
        if (ViewModel == null)
        {
            return;
        }

        if (ViewModel.IsDirty)
        {
            // show ConfirmationDialog
            var result = await _dialogService.ShowEditorCloseConfirmationDialog();

            if (result == ContentDialogResult.Primary)
            {
                if (ViewModel.IsDirty)
                {
                    ViewModel.Save();
                }

                if (ViewModel.IsDirty == false)
                {
                    Window.Close();
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // Discard change and close.
                ViewModel.DiscardChanges();

                Window.Close();
            }
            else if (result == ContentDialogResult.None)
            {
                // Cancel.
            }
        }
    }

    public void Window_Closed(object sender, WindowEventArgs args)
    {
        if (sender is not EditorWindow ewin)
        {
            return;
        }

        ewin.Activated -= Window_Activated;
        ewin.Closed -= Window_Closed;
        ewin.AppWindow.Closing -= AppWindow_Closing;

        ViewModel.CleanUp();

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        // Save window size and position.
        var appWindow = ewin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter)
            {
                mainVM.RoomEditorWinHeight = (int)appWindow.Size.Height;
                mainVM.RoomEditorWinWidth = (int)appWindow.Size.Width;
                mainVM.RoomEditorWinTop = (int)appWindow.Position.Y;
                mainVM.RoomEditorWinLeft = (int)appWindow.Position.X;
            }
        }

        //mainVM.RoomEditorList.Remove(ewin);

        // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingWindowClosedMessage(ewin));
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index == 0)
        {
            //var hoge = args.Item as Breadcrumb;
            //Debug.WriteLine("BreadcrumbBar_ItemClicked: " + hoge?.Name + ", Page: " + hoge?.Page);
            //ViewModel?.GoToBldgShellPageCommand.Execute(null);
        }
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_nvigated)
        {
            return;
        }

        //Debug.WriteLine("NavView_Loaded: Navigating to BasicPage with ViewModel. ViewModel is " + (ViewModel != null ? "set" : "null"));

        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Listing.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))//new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }
        {
            _nvigated = true;

            if (BreadcrumbBar1.ItemsSource is ObservableCollection<Breadcrumb> crumbs)
            {
                if (crumbs.Count > 1)
                {
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage"));
                    if (item.Page is not null)
                    {
                        crumbs.RemoveAt(crumbs.Count - 1); // Remove the last breadcrumb if exists to avoid duplication.
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Page.FullName! });
                    }
                }
            }

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage")).First();
        }
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (_pages is null)
        {
            return;
        }

        if (args.IsSettingsInvoked == true)
        {
            // Do nothing. 
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
        {
            if (args.InvokedItemContainer.Tag is not string tag || string.IsNullOrWhiteSpace(tag))
            {
                Debug.WriteLine("ShellPage: NavView_ItemInvoked: Invalid tag or null.");
                return;
            }

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            if (item.Page is null)
            {
                Debug.WriteLine("ShellPage: NavView_ItemInvoked: Page is null for tag " + tag);
                return;
            }

            if (ContentFrame.Navigate(item.Page, ViewModel, new DrillInNavigationTransitionInfo())) //new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom })SuppressNavigationTransitionInfo
            {
                if (BreadcrumbBar1.ItemsSource is ObservableCollection<Breadcrumb> crumbs)
                {
                    if (crumbs.Count > 1)
                    {
                        crumbs.RemoveAt(crumbs.Count - 1); // Remove the last breadcrumb if exists to avoid duplication.
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Page.FullName! });
                    }
                }
            }
            //, args.RecommendedNavigationTransitionInfo
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (this.ContentFrame.CanGoBack) this.ContentFrame.GoBack();
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        this.NavView.IsBackEnabled = this.ContentFrame.CanGoBack;

        if (this.ContentFrame.SourcePageType != null)
        {
            var selectedItem = FindNavigationViewItemWithTag(this.ContentFrame.SourcePageType.FullName!);
            if (selectedItem != null)
            {
                this.NavView.SelectedItem = selectedItem;
                //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
            }
            else
            {
                Debug.WriteLine($"No menu item with tag matching the current page found in NavView. Current page: {ContentFrame.SourcePageType.FullName}");
            }
        }
    }

    private NavigationViewItem? FindNavigationViewItemWithTag(string tag)
    {
        foreach (var item in this.NavView.MenuItems.OfType<NavigationViewItem>())
        {
            if (item.Tag.Equals(tag))
            {
                this.NavView.SelectedItem = item;
                return item;
            }

            if (item.MenuItems.Count > 0)
            {
                foreach (var subItem in item.MenuItems.OfType<NavigationViewItem>())
                {
                    if (subItem.Tag.Equals(tag))
                    {
                        this.NavView.SelectedItem = subItem;
                        return subItem;
                    }
                }
            }
        }

        foreach (var item in this.NavView.FooterMenuItems.OfType<NavigationViewItem>())
        {
            if (item.Tag.Equals(tag))
            {
                this.NavView.SelectedItem = item;
                return item;
            }
        }

        return null;
    }

    private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        // set this first.
        args.Handled = true;

        if (args.KeyboardAccelerator.Key == Windows.System.VirtualKey.F1)
        {
            // TODO:

            return;
        }

        if (args.KeyboardAccelerator.Modifiers == Windows.System.VirtualKeyModifiers.Menu)
        {
            if (args.KeyboardAccelerator.Key == Windows.System.VirtualKey.Left)
            {
                if (this.ContentFrame != null && this.ContentFrame.CanGoBack)
                {
                    this.ContentFrame.GoBack();
                }

                return;
            }
        }

        if (args.KeyboardAccelerator.Modifiers == Windows.System.VirtualKeyModifiers.Control)
        {
            if (args.KeyboardAccelerator.Key == Windows.System.VirtualKey.S)
            {
                if (ViewModel.SaveCommand.CanExecute(null))
                {
                    ViewModel.SaveCommand.Execute(null);
                }

                return;
            }
        }
    }

}
