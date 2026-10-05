using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Models;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Sale;

public sealed partial class CommercialSearchResultPage : Page
{
    private readonly INavigationService _navigationService;

    public CommercialSearchResultPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();

    }

    public MainViewModel ViewModel
    {
        get;
    }

    private void ResultList_DoubleTapped(object sender,DoubleTappedRoutedEventArgs e)
    {
        // TODO:
        /*
        if (ResultList.SelectedItem
            is Models.PropertySearchResultItem selected &&
            ViewModel.EditSaleResidentialCommand.CanExecute(selected))
        {
            ViewModel.EditSaleResidentialCommand.Execute(selected);
        }
        */
    }


}
