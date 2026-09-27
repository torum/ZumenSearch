using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel ViewModel { get; }

    public EditorWindow Window { get; }

    private readonly INavigationGenericService _navigationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;

    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        ("ZumenSearch.Views.Rent.Commercials.BasicPage","基本",typeof(BasicPage)),
        ("ZumenSearch.Views.Rent.Commercials.UnitListPage","募集区画",typeof(UnitListPage))
    ];

    public ShellPage(
        Models.Rent.Commercials.Property building,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService)
    {
        _navigationService = navigationService;
        _dispatcherService = dispatcherService;
        _dialogService = dialogService;

        ViewModel =
            new ViewModels.Rent.Commercials.PropertyViewModel(
                building,
                navigationService,
                dialogService,
                dispatcherService,
                dataAccessService);

        Window = new EditorWindow(building.Id, ViewModel)
        {
            Content = this
        };

        InitializeComponent();

        _navigationService.Initialize(ContentFrame, _pages);

        this.Loaded += ShellPage_Loaded;
        //this.Unloaded += ShellPage_Unloaded;
        //this.BreadcrumbBar1.ItemClicked += BreadcrumbBar_ItemClicked;

        Window.Title = "賃貸事業用：建物";
        Window.ExtendsContentIntoTitleBar = true;
        //Window.Activated += Window_Activated;
        Window.Closed += Window_Closed;
        //Window.AppWindow.Closing += AppWindow_Closing;
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
        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(typeof(BasicPage), ViewModel);
        }
    }

    private void NavView_ItemInvoked(NavigationView sender,NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is string tag)
        {
            NavigateToPage(tag);
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

    private void NavView_BackRequested(NavigationView sender,NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    private void ContentFrame_Navigated(object sender,Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        NavView.IsBackEnabled = ContentFrame.CanGoBack;

        var tag = e.SourcePageType.FullName;
        if (tag is null)
        {
            return;
        }

        NavView.SelectedItem = NavView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(item => Equals(item.Tag, tag));

        var page = _pages.FirstOrDefault(item => item.Tag == tag);
        if (page.Page is not null
            && ViewModel.BreadcrumbItems is ObservableCollection<Breadcrumb> crumbs
            && crumbs.Count > 1)
        {
            crumbs[^1] = new Breadcrumb
            {
                Name = page.Label,
                Page = page.Page.FullName!
            };
        }
    }

    private void ContentFrame_NavigationFailed(object sender,Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException(
            $"Failed to load page '{e.SourcePageType.FullName}'.");
    }

    public async Task ShowEditorCloseConfirmationDialog()
    {
        if (ViewModel == null)
        {
            return;
        }

        if (!ViewModel.IsDirty)
        {
            return;
        }

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

    public void Window_Closed(object sender, WindowEventArgs args)
    {
        if (sender is not EditorWindow editorWindow)
        {
            return;
        }

        editorWindow.Closed -= Window_Closed;

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();
        var appWindow = editorWindow.AppWindow;

        if (appWindow?.Presenter is OverlappedPresenter)
        {
            mainViewModel.RentCommercEditorWinHeight = (int)appWindow.Size.Height;
            mainViewModel.RentCommercEditorWinWidth = (int)appWindow.Size.Width;
            mainViewModel.RentCommercEditorWinTop = (int)appWindow.Position.Y;
            mainViewModel.RentCommercEditorWinLeft = (int)appWindow.Position.X;
        }

        WeakReferenceMessenger.Default.Send(new Models.Messenger.WindowClosedMessage(editorWindow));
    }
}