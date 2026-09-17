using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinRT.Interop;
using ZumenSearch.Models.Common;
using ZumenSearch.Services;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;

namespace ZumenSearch.Views.Rent.Residentials;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Residentials.PropertyViewModel ViewModel { get; }
    public Views.Rent.Residentials.EditorWindow Window { get; }
    public Frame NavigationFrame => ContentFrame;

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
        ("ZumenSearch.Views.Rent.Residentials.RoomListPage", "部屋", typeof(Views.Rent.Residentials.RoomListPage)),
        ("ZumenSearch.Views.Rent.Residentials.ZumenListPage", "図面", typeof(Views.Rent.Residentials.ZumenListPage)),
        ("ZumenSearch.Views.Rent.Residentials.LessorListPage", "貸主", typeof(Views.Rent.Residentials.LessorListPage)),
        ("ZumenSearch.Views.Rent.Residentials.BrokerListPage", "宅建業者", typeof(Views.Rent.Residentials.BrokerListPage))
    ];

    private bool _isClosing;
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
        _dialogService.Initialize(this.XamlRoot, Window);
        /*
        if (ContentFrame.Navigate(typeof(ZumenSearch.Views.Rent.Residentials.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))
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
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (_isClosing)
        {
            // Prevent re-entrancy if already in the process of closing.
            args.Cancel = true;
            if (_closingWindow is not null)
            {
                IntPtr hWnd = WindowNative.GetWindowHandle(_closingWindow);
                NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

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
                _closingWindow = null;
                _isClosing = false;
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

                        IntPtr hWnd = WindowNative.GetWindowHandle(editor);
                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE); // Ensure it's not minimized
                        NativeMethods.SetForegroundWindow(hWnd); // Attempt to set it as the foreground window

                        editor.Activate();
                        editor.AppWindow.MoveInZOrderAtTop();

                        _closingWindow = editor;
                        if (editor.Content is Views.Rent.Residentials.Listing.ShellPage shell)
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

        ViewModel.CleanUp();

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

        //mainVM.BldgEditorList.Remove(ewin);
        // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyWindowClosedMessage(ewin));
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
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals("ZumenSearch.Views.Rent.Residentials.BasicPage"));
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
