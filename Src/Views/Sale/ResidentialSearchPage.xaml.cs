using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Models.Base;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Sale;

public sealed partial class ResidentialSearchPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    private readonly INavigationService _navigationService;

    public ResidentialSearchPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();
    }

    private void Search_Click(
    object sender,
    RoutedEventArgs e)
    {
        ViewModel.SearchSaleResidentialBldgCommand.Execute(
            SearchTextBox.Text);
    }
}