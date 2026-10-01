using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Dialogs;

// TODO: pass _cts as a parameter and make cancelable.

public partial class LessorSelectViewModel : ObservableObject
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
                    SuggestedLessors?.Clear();
                }
            }

            SearchCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    public ObservableCollection<Models.PersonSearchResultItem>? SuggestedLessors
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

    public Models.PersonSearchResultItem? SelectedLessor
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

    public event EventHandler<Models.PersonSearchResultItem>? SelectionChanged; // TODO:

    private readonly IDataAccessService _dataAccessService;

    private readonly CancellationTokenSource _cts;

    public LessorSelectViewModel(IDataAccessService dataAccessService, CancellationTokenSource cts)
    {
        _dataAccessService = dataAccessService;
        _cts = cts;

        InitLoad();
    }

    private async void InitLoad()
    {
        var res = await Task.Run(() => _dataAccessService.SelectRentLessorsByKeyword("*"), _cts.Token);
        //var res = _dataAccessService.SelectRentLessorByKeyword("*");
        if (res is not null)
        {
            SuggestedLessors = new(res.PersonSearchResult);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    public async Task Search()
    {
        var searchText = string.Empty;
        if (string.IsNullOrEmpty(Query))
        {
            SuggestedLessors?.Clear();

            // Allow empty (treat as "*")
            searchText = "*";
            //return;
        }
        else
        {
            searchText = Query;
        }

        var res = await Task.Run(() => _dataAccessService.SelectRentLessorsByKeyword(searchText), _cts.Token);
        //var res = _dataAccessService.SelectRentLessorByKeyword(Query);
        if (res is not null)
        {
            SuggestedLessors = new(res.PersonSearchResult);
        }
    }
    private bool CanSearch()
    {
        //return !string.IsNullOrEmpty(Query);
        // Allow empty (treat as "*")
        return true;
    }

    [RelayCommand]
    private void AddNewRentLessor()
    {
        var mainVm = App.GetService<MainViewModel>();
        mainVm.AddNewRentLessorCommand.Execute(null);
    }
}
