using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Presentation.Factories.AbstractFactory;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Shell.Models;

namespace ZumenSearch.Presentation.Features.Brokers.Views;

public sealed partial class ShellPage : Page
{
    public ZumenSearch.Presentation.Features.Brokers.ViewModels.BrokerViewModel ViewModel { get;}
    public ZumenSearch.Presentation.Features.Brokers.Views.EditorWindow Window { get;}
    
    public Frame NavigationFrame => ContentFrame;

    // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        ("broker", "宅建業者", null),
        (NavigationKeys.BrokerBasic, "基本", typeof(ZumenSearch.Presentation.Features.Brokers.Views.BasicPage)),
    ];

    private bool _nvigated;

    private readonly INavigationGenericService _navigationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;

    public ShellPage(
        ZumenSearch.Domain.Entities.Base.PersonBase person,
        IAbstractFactory<ZumenSearch.Domain.Entities.Base.PersonBase, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Brokers.ViewModels.BrokerViewModel> vmFactory,
        INavigationGenericService navigationService,
        IDispatcherService dispatcherService,
        IDialogGenericService dialogService)
    {
        //Debug.WriteLine($"ShellPage {building.Id}");

        _navigationService = navigationService;
        _dispatcherService = dispatcherService;
        _dialogService = dialogService;

        ViewModel = vmFactory.Create(person, _navigationService, _dialogService);

        Window = new ZumenSearch.Presentation.Features.Brokers.Views.EditorWindow(person.Id, ViewModel)
        {
            Content = this
        };

        InitializeComponent();

        // Initialize the navigation service with the ContentFrame and pages.
        _navigationService.Initialize(this.ContentFrame, _pages);

        this.Loaded += ShellPage_Loaded;
        this.Unloaded += ShellPage_Unloaded;
        this.BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        Window.Title = "宅建業者";
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
        if (ContentFrame.Navigate(typeof(ZumenSearch.Presentation.Features.Rent.Residentials.Views.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))
        {

        }
        */
    }

    private void ShellPage_Unloaded(object sender, RoutedEventArgs e)
    {
        //
    }

#pragma warning disable CA1707 // Identifiers should not contain underscores
    public void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
#pragma warning restore CA1707 // Identifiers should not contain underscores
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
                    await ViewModel.Save();
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

#pragma warning disable CA1707 // Identifiers should not contain underscores
    public void Window_Closed(object sender, WindowEventArgs args)
#pragma warning restore CA1707 // Identifiers should not contain underscores
    {
        if (sender is not EditorWindow ewin)
        {
            return;
        }

        ewin.Activated -= Window_Activated;
        ewin.Closed -= Window_Closed;
        ewin.AppWindow.Closing -= AppWindow_Closing;

        var mainVM = App.GetService<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>();
        // Save window size and position.
        var appWindow = ewin.AppWindow;
        if (appWindow != null)
        {
            if (appWindow.Presenter is OverlappedPresenter)
            {
                mainVM.BrokerEditorWinHeight = (int)appWindow.Size.Height;
                mainVM.BrokerEditorWinWidth = (int)appWindow.Size.Width;
                mainVM.BrokerEditorWinTop = (int)appWindow.Position.Y;
                mainVM.BrokerEditorWinLeft = (int)appWindow.Position.X;
            }
        }

        //mainVM.BldgEditorList.Remove(ewin);
        // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
        WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.WindowClosedMessage<Microsoft.UI.Xaml.Window>(ewin));
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
        if (ContentFrame.Navigate(typeof(ZumenSearch.Presentation.Features.Brokers.Views.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))
        {
            _nvigated = true;

            if (BreadcrumbBar1.ItemsSource is ObservableCollection<Breadcrumb> crumbs)
            {
                if (crumbs.Count > 1)
                {
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals(NavigationKeys.BrokerBasic, StringComparison.Ordinal));
                    if (item.Page is not null)
                    {
                        crumbs.RemoveAt(crumbs.Count - 1); // Remove the last breadcrumb if exists to avoid duplication.
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Tag });
                    }
                }
            }

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals(NavigationKeys.BrokerBasic)).First();
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

            var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString(), StringComparison.Ordinal));

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
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Tag });
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
        //  CA2201 (Do not raise reserved exception types)
        //throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        this.NavView.IsBackEnabled = this.ContentFrame.CanGoBack;

        if (this.ContentFrame.SourcePageType != null)
        {
            var routeKey = _pages.FirstOrDefault(page => page.Page == this.ContentFrame.SourcePageType).Tag;
            var selectedItem = FindNavigationViewItemWithTag(routeKey);
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
