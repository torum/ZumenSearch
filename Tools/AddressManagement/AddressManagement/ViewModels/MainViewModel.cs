using System.Collections.ObjectModel;

using AddressManagement.Contracts.ViewModels;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AddressManagement.ViewModels;

public class MainViewModel : ObservableRecipient, INavigationAware
{
    private readonly IPrefectureDataService _prefectureDataService;
    
    public ObservableCollection<PrefectureCode> PrefectureDataSource { get; } = new ObservableCollection<PrefectureCode>();


    public PrefectureCode? SelectedPrefecture
    {
        get;set;
    }

    public MainViewModel(IPrefectureDataService prefectureDataService)
    {
        _prefectureDataService = prefectureDataService;

        PopulatePrefecture();


        SelectedPrefecture = PrefectureDataSource.Where(x => x.PrefectureName == "東京都").FirstOrDefault();
    }

    private async void PopulatePrefecture()
    {
        PrefectureDataSource.Clear();

        var data = await _prefectureDataService.GetPrefectureDataAsync();

        foreach (var item in data)
        {
            PrefectureDataSource.Add(item);
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        /*
        PrefectureDataSource.Clear();

        var data = await _prefectureDataService.GetGridDataAsync();

        foreach (var item in data)
        {
            PrefectureDataSource.Add(item);
        }
        */
    }

    public void OnNavigatedFrom()
    {
    }
}
