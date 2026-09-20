using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Dialogs;

// TODO: pass _cts as a parameter and make cancelable.

public partial class BrokerSelectViewModel : ObservableObject
{
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

    public ObservableCollection<Models.Common.PersonSearchResultItem>? SuggestedBrokers
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

    public Models.Common.PersonSearchResultItem? SelectedBroker
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

    public event EventHandler<Models.Common.PersonSearchResultItem>? SelectionChanged; // TODO:

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
        var res = await Task.Run(() => _dataAccessService.SelectBrokersByKeyword("*"), _cts.Token);
        //var res = _dataAccessService.SelectLessorByKeyword("*");
        if (res is not null)
        {
            SuggestedBrokers = new(res.PersonSearchResult);
        }
    }

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
        //var res = _dataAccessService.SelectRentLessorByKeyword(Query);
        if (res is not null)
        {
            SuggestedBrokers = new(res.PersonSearchResult);
        }
    }
    private bool CanSearch()
    {
        //return !string.IsNullOrEmpty(Query);
        // Allow empty (treat as "*")
        return true;
    }

    [RelayCommand]
    private void AddNewBroker()
    {
        var mainVm = App.GetService<MainViewModel>();
        mainVm.AddNewBrokerCommand.Execute(null);
    }
}
