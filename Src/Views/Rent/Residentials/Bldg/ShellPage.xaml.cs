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

namespace ZumenSearch.Views.Rent.Residentials.Bldg;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Residentials.Bldg.MainViewModel ViewModel { get; private set; }
    public Views.Rent.Residentials.Bldg.EditorWindow Window { get; private set; }
    public Frame NavigationFrame => ContentFrame;

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        ("building", "建物", null),
        ("summary", "基本", typeof(Views.Rent.Residentials.Bldg.BasicPage)),
        ("location", "所在地", typeof(Views.Rent.Residentials.Bldg.LocationPage)),
        ("transportation", "交通", typeof(Views.Rent.Residentials.Bldg.TransportationPage)),
        ("facilities", "設備", typeof(Views.Rent.Residentials.Bldg.FacilitiesPage)),
        ("kanri", "管理", typeof(Views.Rent.Residentials.Bldg.KanriPage)),
        ("pictures", "写真", typeof(Views.Rent.Residentials.Bldg.PictureListPage)),
        ("units", "部屋", typeof(Views.Rent.Residentials.Bldg.UnitListPage)),
        ("zumen", "図面", typeof(Views.Rent.Residentials.Bldg.ZumenListPage)),
        ("kasinusi", "貸主", typeof(Views.Rent.Residentials.Bldg.KasinusiPage)),
        ("gyousya", "宅建業者", typeof(Views.Rent.Residentials.Bldg.GyousyaPage))
    ];

    private readonly INavigationGenericService _navService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IModalDialogService _dlgService;
    private bool _isClosing;
    private Views.Rent.Residentials.Room.EditorWindow? _closingWindow;
    private bool _nvigated;

    public ShellPage(Views.Rent.Residentials.Bldg.EditorWindow window, Models.Rent.Residentials.Bldg.Property building, IAbstractFactory<Models.Rent.Residentials.Bldg.Property, ViewModels.Rent.Residentials.Bldg.MainViewModel> vmFactory, INavigationGenericService navigationResidentialService, IDispatcherService dispatcherService, IModalDialogService modalDialog)
    {
        //Debug.WriteLine($"ShellPage {building.Id}");

        Window = window;
        Window.Content = this;

        ViewModel = vmFactory.Create(building);

        _dlgService = modalDialog;
        _dispatcherService = dispatcherService;
        _navService = navigationResidentialService;

        InitializeComponent();

        // Initialize the navigation service with the ContentFrame and set it in the ViewModel.
        _navService.Initialize(this.ContentFrame, _pages);
        ViewModel.SetNavigationService(_navService);

        this.Loaded += ShellPage_Loaded;
        this.Unloaded += ShellPage_Unloaded;
        this.BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        Window.Title = "賃貸住居用：建物";
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

        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Bldg.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))
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
        if (_isClosing)
        {
            // Prevent re-entrancy if already in the process of closing.
            args.Cancel = true;
            if (_closingWindow is not null)
            {
                // Activate the child editor window that is currently being closed.
                _closingWindow.Activate();
                _closingWindow.AppWindow.MoveInZOrderAtTop();
            }

            return;
        }

        _isClosing = true;
        try
        {
            if (ViewModel == null)
            {
                return;
            }

            var isCanceled = false;
            var childEditors = ViewModel.ChildEditorList.ToList();

            if (childEditors.Count > 0)
            {
                foreach (var editor in childEditors)
                {
                    if (editor.ViewModel is null)
                    {
                        Debug.WriteLine("AppWindow_Closing: editor.ViewModel is null");
                        continue;
                    }
                    if (editor.ViewModel.IsDirty)
                    {
                        args.Cancel = true;
                        isCanceled = true;

                        editor.Activate();
                        editor.AppWindow.MoveInZOrderAtTop();

                        _closingWindow = editor;
                        if (editor.Content is Views.Rent.Residentials.Room.ShellPage shell)
                        {
                            // Show confirmation dialog to user to save changes or not.
                            await shell.ShowEditorCloseConfirmationDialog();
                        }
                        _closingWindow = null;

                        break;
                    }
                }

                if (!isCanceled)
                {
                    // Close() may modify ChildEditorList through Closed handlers.
                    // Enumerate the snapshot instead of the live List<T>.
                    foreach (var editor in childEditors)
                    {
                        editor.IsAutoClose = true;
                        editor.Close();
                    }
                }
            }

            if (isCanceled)
            {
                args.Cancel = true;
                return;
            }

            if (ViewModel.IsDirty)
            {
                args.Cancel = true; // needs Cancel = true here in order to show dialog.
                await ShowEditorCloseConfirmationDialog();
            }
        }
        finally
        {
            _closingWindow = null;
            _isClosing = false;
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
                mainVM.BldgEditorWinHeight = (int)appWindow.Size.Height;
                mainVM.BldgEditorWinWidth = (int)appWindow.Size.Width;
                mainVM.BldgEditorWinTop = (int)appWindow.Position.Y;
                mainVM.BldgEditorWinLeft = (int)appWindow.Position.X;
            }
        }

        /*
        if (!ewin.IsAutoClose)
        {
            mainVM.BldgEditorList.Remove(ewin);
        }
        */
        mainVM.BldgEditorList.Remove(ewin);
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

        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.Bldg.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))//new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }
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
                Debug.WriteLine("BldgShellPage: NavView_ItemInvoked: Invalid tag or null.");
                return;
            }

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

            if (item.Page is null)
            {
                Debug.WriteLine("BldgShellPage: NavView_ItemInvoked: Page is null for tag " + tag);
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
        Window.Title = ViewModel?.WindowTitle ?? "賃貸住居用：建物";
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }
}
