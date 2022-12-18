using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZumenSearch.Contracts.Services;
using ZumenSearch.Contracts.ViewModels;

namespace ZumenSearch.ViewModels;

public class RentLivingSearchResultViewModel : ObservableRecipient, INavigationAware
{
    public INavigationService NavigationService
    {
        get;
    }

    public void OnNavigatedTo(object parameter)
    {
        // Run code when the app navigates to this page

        // if (parameter is not null)  Debug.WriteLine("object passed!" + (string)parameter);
    }

    public void OnNavigatedFrom()
    {
        // Run code when the app navigates away from this page
    }

    public RentLivingSearchResultViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;

        Debug.WriteLine("RentLivingSearchResultPage init!");
    }

    private RelayCommand? backCommand;

    public IRelayCommand BackCommand => backCommand ??= new RelayCommand(Back);

    private void Back()
    {
        Debug.WriteLine("Back command executed!");

        NavigationService.NavigateTo(typeof(RentLivingSearchViewModel).FullName!);
    }

    private RelayCommand? addNewCommand;

    public IRelayCommand AddNewCommand => addNewCommand ??= new RelayCommand(AddNew);

    private void AddNew()
    {
        Debug.WriteLine("AddNew command executed!");

        NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");
    }
}
