using System.Collections.ObjectModel;

using AddressManagement.Contracts.ViewModels;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AddressManagement.ViewModels;

public class PrefectureCodeDataGridViewModel : ObservableRecipient, INavigationAware
{
    private readonly IPrefectureDataService _sampleDataService;

    public ObservableCollection<PrefectureCode> Source { get; } = new ObservableCollection<PrefectureCode>();

    public PrefectureCodeDataGridViewModel(IPrefectureDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        var data = await _sampleDataService.GetPrefectureDataAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
