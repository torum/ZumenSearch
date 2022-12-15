using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Contracts.Services;
using ZumenSearch.Contracts.ViewModels;
using ZumenSearch.Helpers;
using ZumenSearch.Services;

namespace ZumenSearch.ViewModels;

public class MainViewModel : ObservableObject, INavigationAware
{
    public INavigationService NavigationService
    {
        get;
    }

    private RelayCommand? searchCommand;

    public IRelayCommand SearchCommand => searchCommand ??= new RelayCommand(Search);

    private void Search()
    {
        Debug.WriteLine("Search command executed!");

        NavigationService.NavigateTo(typeof(MainViewModelChild).FullName!);

    }

    public void OnNavigatedTo(object parameter)
    {
        // Run code when the app navigates to this page
    }

    public void OnNavigatedFrom()
    {
        // Run code when the app navigates away from this page
    }

    public MainViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;


        Debug.WriteLine("MainViewmodel init!");
    }
}
