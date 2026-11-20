using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Infrastructure.Configuration.Contracts;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Factories.AbstractFactory;
using ZumenSearch.Presentation.Shell.ViewModels;

namespace ZumenSearch.Presentation.Search.Views.Sale;

public sealed partial class CommercialSearchPage : Page
{
    private readonly INavigationService _navigationService;

    public CommercialSearchPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();
    }

    public MainViewModel ViewModel
    {
        get;
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        // TODO:
        //ViewModel.SearchSaleResidentialBldgCommand.Execute(SearchTextBox.Text);
    }
}