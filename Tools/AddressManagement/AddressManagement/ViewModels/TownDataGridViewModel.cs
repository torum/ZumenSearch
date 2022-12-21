using System.Collections.ObjectModel;

using AddressManagement.Contracts.ViewModels;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AddressManagement.ViewModels;

public class TownDataGridViewModel : ObservableRecipient, INavigationAware
{
    public ObservableCollection<Town> TownDataSource { get; } = new ObservableCollection<Town>();


    public TownDataGridViewModel()
    {
    }

    public async void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
