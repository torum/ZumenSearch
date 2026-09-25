using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.Views.Rent.Commercials;

public sealed partial class ShellPage : Page
{
    public ViewModels.Rent.Commercials.PropertyViewModel ViewModel
    {
        get;
    }

    public EditorWindow Window
    {
        get;
    }

    private readonly INavigationGenericService _navigationService;

    public ShellPage(
        Models.Rent.Commercials.Property building,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService)
    {
        _navigationService = navigationService;

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

        _navigationService.Initialize(
            ContentFrame,
            [
                (
                    "ZumenSearch.Views.Rent.Commercials.BasicPage",
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
}