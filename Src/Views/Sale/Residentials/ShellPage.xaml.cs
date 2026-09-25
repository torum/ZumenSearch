using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views.Sale.Residentials;

public sealed partial class ShellPage : Page
{
    public ViewModels.Sale.Residentials.PropertyViewModel ViewModel
    {
        get;
    }

    public EditorWindow Window
    {
        get;
    }

    private readonly INavigationGenericService _navigationService;

    public ShellPage(
        Models.Sale.Residentials.Property building,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService)
    {
        _navigationService = navigationService;

        ViewModel = new ViewModels.Sale.Residentials.PropertyViewModel(
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

        _navigationService.Initialize(
            ContentFrame,
            [
                (
                    "ZumenSearch.Views.Sale.Residentials.BasicPage",
                    "基本",
                    typeof(BasicPage)
                )
            ]);

        Loaded += ShellPage_Loaded;
    }

    private void ShellPage_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (ContentFrame.Content is null)
        {
            ContentFrame.Navigate(
                typeof(BasicPage),
                ViewModel);
        }
    }

    protected override void OnNavigatedTo(
        NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is ViewModels.Sale.Residentials.PropertyViewModel vm)
        {
            ContentFrame.Navigate(typeof(BasicPage), vm);
        }
    }
}