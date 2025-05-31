using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;
using ZumenSearch.Views.Rent.Residentials;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public partial class SearchResultViewModel : ObservableRecipient
{


    public SearchResultViewModel()
    {
        //Debug.WriteLine("RentLivingSearchResultPage init!");
    }

    private RelayCommand? backCommand;

    public IRelayCommand BackCommand => backCommand ??= new RelayCommand(Back);

    private void Back()
    {
        //Debug.WriteLine("Back command executed!");

        //NavigationService.NavigateTo(typeof(ResidentialsViewModel).FullName!);
        MainShell shell = App.GetService<MainShell>();
        shell.NavFrame.Navigate(typeof(Views.Rent.Residentials.SearchPage), shell.NavFrame, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });


    }

    private RelayCommand? addNewCommand;

    public IRelayCommand AddNewCommand => addNewCommand ??= new RelayCommand(AddNew);

    private void AddNew()
    {
        MainViewModel vm = App.GetService<MainViewModel>();
        vm.CreateNewResidentialsEditorWindow();
    }
}
