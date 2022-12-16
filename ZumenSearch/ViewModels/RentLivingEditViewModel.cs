using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using ZumenSearch.Contracts.Services;
using ZumenSearch.Contracts.ViewModels;
using ZumenSearch.Services;

namespace ZumenSearch.ViewModels;

public class RentLivingEditViewModel : ObservableRecipient, INavigationAware
{
    private object? _selected;
    public object? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
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

    public RentLivingEditViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        Debug.WriteLine("RentLivingEditViewModel init!");
    }


    private RelayCommand? backCommand;

    public IRelayCommand BackCommand => backCommand ??= new RelayCommand(Back);

    private void Back()
    {
        Debug.WriteLine("Back command executed!");

        //NavigationService.NavigateTo(typeof(RentLivingSearchViewModel).FullName!);
        NavigationService.GoBack();

    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        //IsBackEnabled = NavigationService.CanGoBack;
        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}
