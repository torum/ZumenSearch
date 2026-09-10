using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;

namespace ZumenSearch.Views.Rent.Residentials.Room;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Residentials.Room.MainViewModel ViewModel { get; private set; }
    //public ViewModels.Rent.Residentials.Bldg.MainViewModel? ParentViewModel { get; private set; } // Holding a reference to the parent ViewModel (Bldg.MainViewModel) (only IF opened by it) to allow communication between the Room ShellPage and its parent Bldg ShellPage.
    public Views.Rent.Residentials.Room.EditorWindow Window { get; private set; }

    public Frame NavigationFrame => ContentFrame;

    private bool _nvigated;
    //private bool _initialized;

    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        ("room", "", null),
        ("summary", "基本", typeof(Views.Rent.Residentials.Room.BasicPage)),
        ("contract", "契約条件", typeof(Views.Rent.Residentials.Room.ContractPage)),
        ("transaction", "取引条件", typeof(Views.Rent.Residentials.Room.TransactionPage)),
        ("appliances", "設備", typeof(Views.Rent.Residentials.Room.AppliancesPage)),
        ("pictures", "写真", typeof(Views.Rent.Residentials.Room.PictureListPage)),
        ("zumen", "図面", typeof(Views.Rent.Residentials.Room.ZumenPage)),
        ("kasinusi", "貸主", typeof(Views.Rent.Residentials.Room.KasinusiPage)),
        ("gyousya", "宅建業者", typeof(Views.Rent.Residentials.Room.GyousyaPage)),
    ];

    private readonly INavigationGenericService _navService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IModalDialogService _dlgService;

    public ShellPage(Views.Rent.Residentials.Room.EditorWindow window, Models.Rent.Residentials.Room.Listing room, IAbstractFactory<Models.Rent.Residentials.Room.Listing, ViewModels.Rent.Residentials.Room.MainViewModel> vmFactory, INavigationGenericService navigationResidentialService, IDispatcherService dispatcherService, IModalDialogService modalDialogService)
    {
        //Debug.WriteLine($"ShellPage {entry.Id}");

        Window = window;
        Window.Content = this;

        ViewModel = vmFactory.Create(room);

        _dlgService = modalDialogService;
        _dispatcherService = dispatcherService;
        _navService = navigationResidentialService;

        InitializeComponent();

        this.Loaded += ShellPage_Loaded;
        this.Unloaded += ShellPage_Unloaded;
        this.BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        _navService.Initialize(this.ContentFrame, _pages);
        ViewModel.SetNavigationService(_navService);

        Window.Title = "賃貸住居用：部屋";
        Window.ExtendsContentIntoTitleBar = true;
        Window.Activated += Window_Activated;
        Window.Closed += Window_Closed;
        Window.AppWindow.Closing += AppWindow_Closing;
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        // XamlRoot is no longer null.
        _dlgService.Initialize(this.XamlRoot);
        ViewModel.SetDialogService(_dlgService);

        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Room.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))
        {

        }
    }

    private void ShellPage_Unloaded(object sender, RoutedEventArgs e)
    {
        //
    }

    public void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
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
            var result = await _dlgService.ShowEditorCloseConfirmationDialog();

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

        /*
        if (!ewin.IsAutoClose)
        {
            mainVM.RoomEditorList.Remove(ewin);
            ParentViewModel?.ChildEditorList.Remove(ewin);
        }
        */
        mainVM.RoomEditorList.Remove(ewin);
        ViewModel.ParentViewModel?.ChildEditorList.Remove(ewin);
    }

    /*
    public void SetParentViewModel(ViewModels.Rent.Residentials.Bldg.MainViewModel parentVM)
    {
        ParentViewModel = parentVM;
    }
    */

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

        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Room.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))//new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }
        {
            _nvigated = true;

            if (BreadcrumbBar1.ItemsSource is ObservableCollection<Breadcrumb> crumbs)
            {
                if (crumbs.Count > 1)
                {
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals("summary"));
                    if (item.Page is not null)
                    {
                        crumbs.RemoveAt(crumbs.Count - 1); // Remove the last breadcrumb if exists to avoid duplication.
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Page.FullName! });
                    }
                }
            }

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("summary")).First();
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

    private void BackAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (this.ContentFrame != null && this.ContentFrame.CanGoBack)
        {
            this.ContentFrame.GoBack();
            args.Handled = true;
        }
    }

    public void OnEventTitleChanged(EventArgs args)
    {
        Window.Title = ViewModel?.WindowTitle ?? "賃貸住居用：部屋";
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }
}
