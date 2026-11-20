using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using WinRT.Interop;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Application.Configuration;
using ZumenSearch.Application.Results;
using ZumenSearch.Presentation.Shell.Models;
using ZumenSearch.Presentation.Factories.AbstractFactory;
using ZumenSearch.Presentation.Services.Contracts;
namespace ZumenSearch.Presentation.Features.Rent.Commercials.Views;

public sealed partial class ShellPage : Page
{
    public ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel ViewModel { get; }

    public EditorWindow Window { get; }

    private bool _isClosing;
    private bool _allowConfirmedClose;
    private bool _nvigated;
    private ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.EditorWindow? _closingWindow;

    private readonly INavigationGenericService _navigationService;
    private readonly IDialogGenericService _dialogService;

    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        (NavigationKeys.RentCommercialPropertyBasic, "基本", typeof(BasicPage)),
        (NavigationKeys.RentCommercialPropertyLocation, "所在地", typeof(LocationPage)),
        (NavigationKeys.RentCommercialPropertyTransportation, "交通", typeof(TransportationPage)),
        (NavigationKeys.RentCommercialPropertyFacilities, "設備", typeof(FacilitiesPage)),
        (NavigationKeys.RentCommercialPropertyManagement, "管理", typeof(KanriPage)),
        (NavigationKeys.RentCommercialPropertyPictures, "写真", typeof(PictureListPage)),
        (NavigationKeys.RentCommercialPropertyDocuments, "図面", typeof(ZumenListPage)),
        (NavigationKeys.RentCommercialPropertyLessors, "貸主", typeof(LessorListPage)),
        (NavigationKeys.RentCommercialPropertyBrokers, "宅建業者", typeof(BrokerListPage)),
        (NavigationKeys.RentCommercialPropertyListings, "募集物件", typeof(UnitListPage))
    ];

    public ShellPage(ZumenSearch.Domain.Entities.Rent.Commercials.Property building, IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Commercials.Property, INavigationGenericService, IDialogGenericService, ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel> vmFactory, INavigationGenericService navigationService, IDialogGenericService dialogService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;

        ViewModel = vmFactory.Create(building, _navigationService, _dialogService);

        Window = new EditorWindow(building.Id, ViewModel)
        {
            Content = this
        };

        InitializeComponent();

        _navigationService.Initialize(ContentFrame, _pages);
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        this.Loaded += ShellPage_Loaded;
        //this.Unloaded += ShellPage_Unloaded;
        //this.BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        Window.Title = "賃貸事業用：建物";
        Window.ExtendsContentIntoTitleBar = true;
        Window.Activated += Window_Activated;
        Window.Closed += Window_Closed;
        Window.AppWindow.Closing += AppWindow_Closing;
    }

    public async Task ShowEditorCloseConfirmationDialog()
    {
        if (_isClosing) return;

        _isClosing = true;
        ViewModel.SetClosing(true);
        try
        {
            if (ViewModel.SaveCommand.IsRunning && ViewModel.SaveCommand.ExecutionTask is Task pendingSave)
            {
                await pendingSave;
            }

            await ViewModel.WaitForEditorOperationsAsync();
            if (!ViewModel.CleanupIncompleteImports()) return;

            foreach (var editor in ViewModel.UnsavedChildEditorList.ToList())
            {
                _closingWindow = editor;
                editor.Activate();
                if (editor.Content is not ZumenSearch.Presentation.Features.Rent.Commercials.Views.Listing.ShellPage shell)
                {
                    ViewModel.InfoBarErrorMessage = "募集物件の編集画面を閉じることができませんでした。";
                    ViewModel.IsInfoBarErrorOpen = true;
                    return;
                }

                await shell.ShowEditorCloseConfirmationDialog();
                if (ViewModel.UnsavedChildEditorList.Contains(editor)) return;
            }

            _closingWindow = null;
            if (ViewModel.IsDiscardPending)
            {
                if (!ViewModel.DiscardChanges()) return;
            }
            else if (ViewModel.IsDirty)
            {
                var result = await _dialogService.ShowEditorCloseConfirmationDialog();
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.Save();
                    if (ViewModel.IsDirty) return;
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    if (!ViewModel.DiscardChanges()) return;
                }
                else
                {
                    return;
                }
            }

            CloseAfterConfirmation();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException or OperationCanceledException)
        {
            Debug.WriteLine($"Failed to finish commercial property operations before closing: {ex}");
            ViewModel.InfoBarErrorMessage = $"編集画面を閉じる前に処理を完了できませんでした: {ex.Message}";
            ViewModel.IsInfoBarErrorOpen = true;
        }
        finally
        {
            _closingWindow = null;
            _isClosing = false;
            ViewModel.SetClosing(false);
        }
    }

    private void CloseAfterConfirmation()
    {
        _allowConfirmedClose = true;
        try
        {
            Window.Close();
        }
        finally
        {
            _allowConfirmedClose = false;
        }
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

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        _dialogService.Initialize(this.XamlRoot, Window);

        if (ContentFrame.Content is null)
        {
            //ContentFrame.Navigate(typeof(BasicPage),ViewModel);
        }
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_nvigated)
        {
            return;
        }

        //Debug.WriteLine("NavView_Loaded: Navigating to BasicPage with ViewModel. ViewModel is " + (ViewModel != null ? "set" : "null"));

        if (ContentFrame.Navigate(typeof(ZumenSearch.Presentation.Features.Rent.Commercials.Views.BasicPage), ViewModel, new EntranceNavigationTransitionInfo()))//new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }
        {
            _nvigated = true;

            if (BreadcrumbBar1.ItemsSource is ObservableCollection<Breadcrumb> crumbs)
            {
                if (crumbs.Count > 1)
                {
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals(NavigationKeys.RentCommercialPropertyBasic, StringComparison.Ordinal));
                    if (item.Page is not null)
                    {
                        crumbs.RemoveAt(crumbs.Count - 1); // Remove the last breadcrumb if exists to avoid duplication.
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Tag });
                    }
                }
            }

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().Where(n => n.Tag.Equals(NavigationKeys.RentCommercialPropertyBasic)).First();
        }
        /*
        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(typeof(BasicPage), ViewModel);
        }*/
    }

    private void NavView_ItemInvoked(NavigationView sender,NavigationViewItemInvokedEventArgs args)
    {
        /*
        if (args.InvokedItemContainer?.Tag is string tag)
        {
            NavigateToPage(tag);
        }
        */


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
                        crumbs.Add(new Breadcrumb { Name = item.Label, Page = item.Tag });
                    }
                }
            }
            //, args.RecommendedNavigationTransitionInfo
        }
    }

    private void NavigateToPage(string tag)
    {
        if (_navigationService.IsCurrentPageSameAs(tag))
        {
            return;
        }

        _navigationService.NavigateTo(tag, ViewModel);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewModel.IsUnitOwnership) || !ViewModel.IsUnitOwnership) return;

        PruneHiddenHistory();
        if (ContentFrame.SourcePageType == typeof(ZumenListPage) || ContentFrame.SourcePageType == typeof(LessorListPage) || ContentFrame.SourcePageType == typeof(BrokerListPage))
        {
            _navigationService.NavigateTo(NavigationKeys.RentCommercialPropertyBasic, ViewModel);
        }
        NavView.IsBackEnabled = ContentFrame.CanGoBack;
    }

    private void PruneHiddenHistory()
    {
        if (!ViewModel.IsUnitOwnership) return;
        for (var index = ContentFrame.BackStack.Count - 1; index >= 0; index--)
        {
            var page = ContentFrame.BackStack[index].SourcePageType;
            if (page == typeof(ZumenListPage) || page == typeof(LessorListPage) || page == typeof(BrokerListPage)) ContentFrame.BackStack.RemoveAt(index);
        }
    }

    private void NavView_BackRequested(NavigationView sender,NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    private void ContentFrame_Navigated(object sender,Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        PruneHiddenHistory();
        NavView.IsBackEnabled = ContentFrame.CanGoBack;

        var page = _pages.FirstOrDefault(item => item.Page == e.SourcePageType);
        if (page.Page is null)
        {
            return;
        }

        NavView.SelectedItem = NavView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(item => Equals(item.Tag, page.Tag));

        if (ViewModel.BreadcrumbItems is ObservableCollection<Breadcrumb> crumbs && crumbs.Count > 1)
        {
            crumbs[^1] = new Breadcrumb
            {
                Name = page.Label,
                Page = page.Tag
            };
        }
    }

    private void ContentFrame_NavigationFailed(object sender,Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load page '{e.SourcePageType.FullName}'.");
    }

    private async void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (_allowConfirmedClose) return;

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
        if (sender is not EditorWindow editorWindow)
        {
            return;
        }

        editorWindow.Activated -= Window_Activated;
        editorWindow.Closed -= Window_Closed;
        editorWindow.AppWindow.Closing -= AppWindow_Closing;
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;

        var mainViewModel = App.GetService<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>();
        var appWindow = editorWindow.AppWindow;

        if (appWindow?.Presenter is OverlappedPresenter)
        {
            mainViewModel.RentCommercialEditorWinHeight = (int)appWindow.Size.Height;
            mainViewModel.RentCommercialEditorWinWidth = (int)appWindow.Size.Width;
            mainViewModel.RentCommercialEditorWinTop = (int)appWindow.Position.Y;
            mainViewModel.RentCommercialEditorWinLeft = (int)appWindow.Position.X;
        }

        WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.WindowClosedMessage<Microsoft.UI.Xaml.Window>(editorWindow));
        ViewModel.Dispose();
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
