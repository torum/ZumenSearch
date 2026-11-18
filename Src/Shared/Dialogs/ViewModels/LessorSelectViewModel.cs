using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Dialogs;

public partial class LessorSelectViewModel : ObservableObject
{
    private readonly IDataAccessService _dataAccessService;

    private readonly CancellationTokenSource _cts;

    public LessorSelectViewModel(IDataAccessService dataAccessService, CancellationTokenSource cts)
    {
        _dataAccessService = dataAccessService;
        _cts = cts;

        // Fire and forget.
        InitLoad();
    }

    private async void InitLoad()
    {
        try
        {
            var res = await Task.Run(() => _dataAccessService.SelectRentLessorsByKeyword("*"), _cts.Token);
            if (res.IsError)
            {
                Debug.WriteLine(
                    res.Error.Title + Environment.NewLine +
                    res.Error.Message + Environment.NewLine +
                    res.Error.Description + Environment.NewLine +
                    res.Error.Operation + Environment.NewLine +
                    res.Error.MethodName + Environment.NewLine +
                    res.Error.FullDump);

                ShowErrorInfoBar(res.Error);
            }
            else
            {
                SuggestedLessors = new(res.PersonSearchResult);
            }
        }
        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
        {
            return;
        }
    }

    private void ShowErrorInfoBar(Models.ErrorInfo err)
    {
        InfoBarErrorTitle = err.Title;
        InfoBarErrorMessage =
            err.Message + Environment.NewLine +
            err.Description + Environment.NewLine +
            err.Operation + Environment.NewLine +
            err.MethodName;
        InfoBarSeverity = InfoBarSeverity.Error;
        IsInfoBarErrorOpen = true;
    }

    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string InfoBarErrorTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Warning;

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

    public ObservableCollection<Models.SearchResult.PersonSearchResultItem>? SuggestedLessors
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

    public Models.SearchResult.PersonSearchResultItem? SelectedLessor
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
            SuggestedLessors?.Clear();

            // Allow empty (treat as "*")
            searchText = "*";
            //return;
        }
        else
        {
            searchText = Query;
        }

        // TODO: Catch and ignore catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested). This may not be necessary...but.
        var res = await Task.Run(() => _dataAccessService.SelectRentLessorsByKeyword(searchText), _cts.Token);
        if (res.IsError)
        {
            Debug.WriteLine(
                res.Error.Title + Environment.NewLine +
                res.Error.Message + Environment.NewLine +
                res.Error.Description + Environment.NewLine +
                res.Error.Operation + Environment.NewLine +
                res.Error.MethodName + Environment.NewLine +
                res.Error.FullDump);

            ShowErrorInfoBar(res.Error);
        }
        else
        {
            SuggestedLessors = new(res.PersonSearchResult);
        }
    }
    private static bool CanSearch()
    {
        //return !string.IsNullOrEmpty(Query);
        // Allow empty (treat as "*")
        return true;
    }

    [RelayCommand]
    private static void AddNewRentLessor()
    {
        var mainVm = App.GetService<MainViewModel>();
        mainVm.AddNewRentLessorCommand.Execute(null);
    }
}
