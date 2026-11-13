using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using WinRT.Interop;
using ZumenSearch.Models;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.Views.Services.Contracts;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class ShellPage : Page
{

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        ("building", "建物", null),
        ("ZumenSearch.Views.Rent.Residentials.BasicPage", "基本", typeof(Views.Rent.Residentials.BasicPage)),
        ("ZumenSearch.Views.Rent.Residentials.LocationPage", "所在地", typeof(Views.Rent.Residentials.LocationPage)),
        ("ZumenSearch.Views.Rent.Residentials.TransportationPage", "交通", typeof(Views.Rent.Residentials.TransportationPage)),
        ("ZumenSearch.Views.Rent.Residentials.FacilitiesPage", "設備", typeof(Views.Rent.Residentials.FacilitiesPage)),
        ("ZumenSearch.Views.Rent.Residentials.KanriPage", "管理", typeof(Views.Rent.Residentials.KanriPage)),
        ("ZumenSearch.Views.Rent.Residentials.PictureListPage", "写真", typeof(Views.Rent.Residentials.PictureListPage)),
        ("ZumenSearch.Views.Rent.Residentials.ZumenListPage", "図面", typeof(Views.Rent.Residentials.ZumenListPage)),
        ("ZumenSearch.Views.Rent.Residentials.LessorListPage", "貸主", typeof(Views.Rent.Residentials.LessorListPage)),
        ("ZumenSearch.Views.Rent.Residentials.BrokerListPage", "宅建業者", typeof(Views.Rent.Residentials.BrokerListPage)),
        ("ZumenSearch.Views.Rent.Residentials.ListingListPage", "募集物件", typeof(Views.Rent.Residentials.ListingListPage))
    ];

    private bool _isClosing;
    private bool _allowClose;
    private Views.Rent.Residentials.Listing.EditorWindow? _closingWindow;
    private bool _nvigated;

    private readonly INavigationGenericService _navigationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;

    public ShellPage(
        Models.Rent.Residentials.Property building,
        IAbstractFactory<Models.Rent.Residentials.Property, INavigationGenericService, IDialogGenericService, ViewModels.Rent.Residentials.PropertyViewModel> vmFactory,
        INavigationGenericService navigationService, 
        IDispatcherService dispatcherService,
        IDialogGenericService dialogService)
    {
        //Debug.WriteLine($"ShellPage {building.Id}");

        _navigationService = navigationService;
        _dispatcherService = dispatcherService;
        _dialogService = dialogService;

        ViewModel = vmFactory.Create(building, _navigationService, _dialogService);

        Window = new Views.Rent.Residentials.EditorWindow(building.Id, ViewModel)
        {
            Content = this
        };

        InitializeComponent();

        // Initialize the navigation service with the ContentFrame and pages.
        _navigationService.Initialize(this.ContentFrame, _pages);
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        this.Loaded += ShellPage_Loaded;
        this.BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        Window.Title = "賃貸住居用：建物";
        Window.ExtendsContentIntoTitleBar = true;
        Window.Activated += Window_Activated;
        Window.Closed += Window_Closed;
        Window.AppWindow.Closing += AppWindow_Closing;
    }

    public ViewModels.Rent.Residentials.PropertyViewModel ViewModel { get; }
    public Views.Rent.Residentials.EditorWindow Window { get; }
    public Frame NavigationFrame => ContentFrame;

    public async Task ShowEditorCloseConfirmationDialog()
    {
        if (_isClosing) return;
        _isClosing = true;
        ViewModel.SetClosing(true);
        try
        {
            //if (!await ViewModel.WaitForEditorOperationsAsync()) return;
            var main = App.GetService<ViewModels.MainViewModel>();
            await main.WaitForResidentialOperationsAsync();
            if (ViewModel.SaveCommand.IsRunning && ViewModel.SaveCommand.ExecutionTask is Task pendingSave) await pendingSave;
            //if (!ViewModel.CleanIncompleteAttachments()) return;

            foreach (var editor in ViewModel.UnsavedChildEditorList.ToList())
            {
                _closingWindow = editor;
                editor.Activate();
                if (editor.Content is not Views.Rent.Residentials.Listing.ShellPage shell) return;
                await shell.ShowEditorCloseConfirmationDialog();
                if (main.RentResidentialListingEditorList.Contains(editor)) return;
            }
            _closingWindow = null;

            if (ViewModel.IsDiscardPending)
            {
                if (!ViewModel.DiscardChanges()) return;
            }
            else if (ViewModel.IsDirty)
            {
                var result = await _dialogService.ShowEditorCloseConfirmationDialog();
                await main.WaitForResidentialOperationsAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if (ViewModel.IsDirty) await ViewModel.Save();
                    if (ViewModel.IsDirty) return;
                }
                else if (result != ContentDialogResult.Secondary || !ViewModel.DiscardChanges())
                {
                    return;
                }
            }

            _allowClose = true;
            Window.Close();
        }
        finally
        {
            _allowClose = false;
            _closingWindow = null;
            _isClosing = false;
            if (ViewModel.IsActive) ViewModel.SetClosing(false);
        }
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        // XamlRoot is no longer null.
        _dialogService.Initialize(this.XamlRoot, Window);
        /*
        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))
        {

        }
        */
    }

    private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";
        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

        BreadcrumbBar1.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.5 : 1;

        //NavView.Opacity = args.WindowActivationState == WindowActivationState.Deactivated ? 0.7 : 1;
        var compositor = Microsoft.UI.Xaml.Media.CompositionTarget.GetCompositorForCurrentThread();
        var visual = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(NavView);

        var animation = compositor.CreateScalarKeyFrameAnimation();
        if (args.WindowActivationState != WindowActivationState.CodeActivated)
        {
            animation.InsertKeyFrame(0f, 1f); // Start opacity
            animation.InsertKeyFrame(1f, 0.7f); // End opacity
        }
        else
        {
            animation.InsertKeyFrame(0f, 0.7f); // Start opacity
            animation.InsertKeyFrame(1f, 1f); // End opacity
        }
        animation.Duration = TimeSpan.FromMilliseconds(150);
        visual.StartAnimation("Opacity", animation);
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (_allowClose) return;
        args.Cancel = true;
        if (_isClosing)
        {
            _closingWindow?.Activate();
            return;
        }
        await ShowEditorCloseConfirmationDialog();
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        if (sender is not EditorWindow ewin)
        {
            return;
        }

        ewin.Activated -= Window_Activated;
        ewin.Closed -= Window_Closed;
        ewin.AppWindow.Closing -= AppWindow_Closing;
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        // Save window size and position.
        var appWindow = ewin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter)
            {
                mainVM.RentResidentialEditorWinHeight = (int)appWindow.Size.Height;
                mainVM.RentResidentialEditorWinWidth = (int)appWindow.Size.Width;
                mainVM.RentResidentialEditorWinTop = (int)appWindow.Position.Y;
                mainVM.RentResidentialEditorWinLeft = (int)appWindow.Position.X;
            }
        }

        // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.WindowClosedMessage<Microsoft.UI.Xaml.Window>(ewin));

        // AbstractFactory will manage instances and IHost dispose the AbstractFactory in turn dispose ViewModel when the app is shutdown,
        // so we don't need to call Dispose() here.
        // But let's do it here anyway.
        ViewModel.Dispose();
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

        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))//new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }
        {
            _nvigated = true;

            if (BreadcrumbBar1.ItemsSource is ObservableCollection<Breadcrumb> crumbs)
            {
                if (crumbs.Count > 1)
                {
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals("ZumenSearch.Views.Rent.Residentials.BasicPage", StringComparison.Ordinal));
                    if (item.Page is not null)
                    {
                        crumbs.RemoveAt(crumbs.Count - 1); // Remove the last breadcrumb if exists to avoid duplication.
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Page.FullName! });
                    }
                }
            }

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals("ZumenSearch.Views.Rent.Residentials.BasicPage")).First();
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

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString(), StringComparison.Ordinal));

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

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewModel.IsUnitLevelOwnership) || !ViewModel.IsUnitLevelOwnership) return;
        PruneHiddenHistory();
        if (ContentFrame.SourcePageType == typeof(ZumenListPage) || ContentFrame.SourcePageType == typeof(LessorListPage) || ContentFrame.SourcePageType == typeof(BrokerListPage))
        {
            _navigationService.NavigateTo(typeof(BasicPage).FullName!, ViewModel);
        }
        NavView.IsBackEnabled = ContentFrame.CanGoBack;
    }

    private void PruneHiddenHistory()
    {
        if (!ViewModel.IsUnitLevelOwnership) return;
        for (var index = ContentFrame.BackStack.Count - 1; index >= 0; index--)
        {
            var page = ContentFrame.BackStack[index].SourcePageType;
            if (page == typeof(ZumenListPage) || page == typeof(LessorListPage) || page == typeof(BrokerListPage)) ContentFrame.BackStack.RemoveAt(index);
        }
    }

    private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (this.ContentFrame.CanGoBack) this.ContentFrame.GoBack();
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        //  CA2201 (Do not raise reserved exception types)
        //throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        PruneHiddenHistory();
        this.NavView.IsBackEnabled = this.ContentFrame.CanGoBack;
        var page = _pages.FirstOrDefault(item => item.Page == e.SourcePageType);
        if (page.Page is not null && ViewModel.BreadcrumbItems.Count > 1)
        {
            ViewModel.BreadcrumbItems[^1] = new Breadcrumb { Name = page.Label, Page = page.Page.FullName! };
        }

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
                Debug.WriteLine($"No menu item with tag matching the current page found in NavView. Current page: { ContentFrame.SourcePageType.FullName}");
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

    private async void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        //Debug.WriteLine("KeyboardAccelerator_Invoked");

        // set this first.
        args.Handled = true;
        if (!ViewModel.IsEditorEnabled) return;

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

    #region == BringToFront ==

    private static partial class NativeMethods
    {
        internal const int SW_RESTORE = 9; // Restores a minimized window and brings it to the foreground.

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    }

    #endregion
}
