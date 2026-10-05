using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models.Base;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Sale;

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