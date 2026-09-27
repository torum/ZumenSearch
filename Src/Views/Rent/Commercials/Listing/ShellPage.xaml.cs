using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views.Rent.Commercials.Listing;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Commercials.Listing.ListingViewModel ViewModel { get; }
    public EditorWindow Window { get; }

    private readonly INavigationGenericService _navigationService;
    private readonly IDialogGenericService _dialogService;

    private readonly List<(string Tag, string Label, Type? Page)> _pages =
    [
        (
            "ZumenSearch.Views.Rent.Commercials.Listing.BasicPage",
            "基本",
            typeof(BasicPage)
        )
    ];

    public ShellPage(
        Models.Rent.Commercials.Listing.Listing unit,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDataAccessService dataAccessService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;

        ViewModel = new ViewModels.Rent.Commercials.Listing.ListingViewModel(
            unit,
            navigationService,
            dataAccessService);

        Window = new EditorWindow(unit.Id, ViewModel)
        {
            Content = this
        };

        InitializeComponent();
        _navigationService.Initialize(ContentFrame, _pages);

        Loaded += ShellPage_Loaded;
        Window.AppWindow.Closing += AppWindow_Closing;
    }

    private void ShellPage_Loaded(object sender, RoutedEventArgs e)
    {
        _dialogService.Initialize(XamlRoot, Window);

        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(typeof(BasicPage), ViewModel);
        }
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(typeof(BasicPage), ViewModel);
        }
    }

    private void NavView_ItemInvoked(
        NavigationView sender,
        NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer?.Tag is string tag)
        {
            _navigationService.NavigateTo(tag, ViewModel);
        }
    }

    private void ContentFrame_NavigationFailed(
        object sender,
        NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException(
            $"Failed to load page '{e.SourcePageType.FullName}'.");
    }

    private bool _allowClose;

    private async void AppWindow_Closing(
        Microsoft.UI.Windowing.AppWindow sender,
        Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        if (_allowClose || !ViewModel.IsDirty)
        {
            return;
        }

        args.Cancel = true;

        var result = await _dialogService.ShowEditorCloseConfirmationDialog();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.Save();
            if (!ViewModel.IsDirty)
            {
                _allowClose = true;
                Window.Close();
            }
        }
        else if (result == ContentDialogResult.Secondary)
        {
            ViewModel.DiscardChanges();
            _allowClose = true;
            Window.Close();
        }
    }
}