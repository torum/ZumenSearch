using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Application.Configuration;
using ZumenSearch.Application.Results;
using ZumenSearch.Presentation.Shell.Models;
using ZumenSearch.Infrastructure.Configuration.Contracts;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Shell.ViewModels;

namespace ZumenSearch.Presentation.Search.Views.Sale;

public sealed partial class ResidentialSearchResultPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    private readonly INavigationService _navigationService;

    public ResidentialSearchResultPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = App.GetService<INavigationService>();

        InitializeComponent();

    }
    private void ResultList_DoubleTapped(
    object sender,
    DoubleTappedRoutedEventArgs e)
    {
        if (ResultList.SelectedItem
            is ZumenSearch.Application.SearchResults.PropertySearchResultItem selected &&
            ViewModel.EditSaleResidentialCommand.CanExecute(selected))
        {
            ViewModel.EditSaleResidentialCommand.Execute(selected);
        }
    }


}
