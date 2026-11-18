using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Dialogs;

public partial class BrokerSelectViewModel : ObservableObject
{
    private readonly IDataAccessService _dataAccessService;

    private readonly CancellationTokenSource _cts;

    public BrokerSelectViewModel(IDataAccessService dataAccessService, CancellationTokenSource cts)
    {
        _dataAccessService = dataAccessService;
        _cts = cts;

        InitLoad();
    }

    private async void InitLoad()
    {
        // TODO:
        var res = await Task.Run(() => _dataAccessService.SelectBrokersByKeyword("*"), _cts.Token);

        if (res is not null)
        {
            SuggestedBrokers = new(res.PersonSearchResult);
        }
    }

    public string Query
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (string.IsNullOrEmpty(field))
                {
                    SuggestedBrokers?.Clear();
                }
            }

            SearchCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    public ObservableCollection<Models.SearchResult.PersonSearchResultItem>? SuggestedBrokers
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public Models.SearchResult.PersonSearchResultItem? SelectedBroker
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    SelectionChanged?.Invoke(this, field);
                }
            }
        }
    }

    public event EventHandler<Models.SearchResult.PersonSearchResultItem>? SelectionChanged; // TODO:

    [RelayCommand(CanExecute = nameof(CanSearch))]
    public async Task Search()
    {
        var searchText = string.Empty;
        if (string.IsNullOrEmpty(Query))
        {
            SuggestedBrokers?.Clear();

            // Allow empty (treat as "*")
            searchText = "*";
            //return;
        }
        else
        {
            searchText = Query;
        }

        var res = await Task.Run(() => _dataAccessService.SelectBrokersByKeyword(searchText), _cts.Token);
        // TODO:
        if (res is not null)
        {
            SuggestedBrokers = new(res.PersonSearchResult);
        }
    }
    private static bool CanSearch()
    {
        //return !string.IsNullOrEmpty(Query);
        // Allow empty (treat as "*")
        return true;
    }

    [RelayCommand]
    private static void AddNewBroker()
    {
        var mainVm = App.GetService<MainViewModel>();
        mainVm.AddNewBrokerCommand.Execute(null);
    }
}
