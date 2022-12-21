using System.Collections.ObjectModel;

using AddressManagement.Contracts.ViewModels;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AddressManagement.ViewModels;

public class PrefectureCodeDataGridViewModel : ObservableRecipient, INavigationAware
{
    readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    private readonly IPrefectureDataService _sampleDataService;

    public ObservableCollection<Prefecture> Source { get; } = new ObservableCollection<Prefecture>();

    public PrefectureCodeDataGridViewModel(IPrefectureDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;

        Task.Run(() => Load());
        
    }

    private async void Load()
    {
        var data = await _sampleDataService.GetPrefectureDataAsync();

        foreach (var item in data)
        {

            _dispatcherQueue.TryEnqueue(() =>
            {
                Source.Add(item);
            });
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        /*
         Source.Clear();

        var data = await _sampleDataService.GetPrefectureDataAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }

        */
    }

    public void OnNavigatedFrom()
    {
    }
}
