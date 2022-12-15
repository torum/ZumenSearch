using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZumenSearch.Contracts.Services;

namespace ZumenSearch.ViewModels;

public class MainViewModelChild : ObservableRecipient
{
    public INavigationService NavigationService
    {
        get;
    }


    private RelayCommand? backCommand;

    public IRelayCommand BackCommand => backCommand ??= new RelayCommand(Back);

    private void Back()
    {
        Debug.WriteLine("Back command executed!");

        NavigationService.NavigateTo(typeof(MainViewModel).FullName!);

    }


    public MainViewModelChild(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }
}
