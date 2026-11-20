using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Infrastructure.Configuration.Contracts;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Presentation.Services.Contracts;

namespace ZumenSearch.Presentation.Features.Sale.Residentials.Views;

public sealed partial class ShellPage : Page
{
    public ZumenSearch.Presentation.Features.Sale.Residentials.ViewModels.PropertyViewModel ViewModel
    {
        get;
    }

    public EditorWindow Window
    {
        get;
    }

    private readonly INavigationGenericService _navigationService;

    public ShellPage(
        ZumenSearch.Domain.Entities.Sale.Residentials.Property building,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService)
    {
        _navigationService = navigationService;

        ViewModel = new ZumenSearch.Presentation.Features.Sale.Residentials.ViewModels.PropertyViewModel(
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
                    NavigationKeys.SaleResidentialPropertyBasic,
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

        if (e.Parameter is ZumenSearch.Presentation.Features.Sale.Residentials.ViewModels.PropertyViewModel vm)
        {
            ContentFrame.Navigate(typeof(BasicPage), vm);
        }
    }
}
