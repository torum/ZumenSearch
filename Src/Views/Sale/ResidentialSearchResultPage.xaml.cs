using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Views.Sale;

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
            is Models.Common.PropertySearchResultItem selected &&
            ViewModel.EditSaleResidentialBldgCommand.CanExecute(selected))
        {
            ViewModel.EditSaleResidentialBldgCommand.Execute(selected);
        }
    }


}
