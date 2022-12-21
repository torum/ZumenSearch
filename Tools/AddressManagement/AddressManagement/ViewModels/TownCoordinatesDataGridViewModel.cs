using System.Collections.ObjectModel;

using AddressManagement.Contracts.ViewModels;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AddressManagement.ViewModels;

public class TownCoordinatesDataGridViewModel : ObservableRecipient, INavigationAware
{
    public ObservableCollection<TownCoordinates> TownCoordinatesDataSource { get; } = new ObservableCollection<TownCoordinates>();


    public TownCoordinatesDataGridViewModel()
    {
    }

    public async void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
