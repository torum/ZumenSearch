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
