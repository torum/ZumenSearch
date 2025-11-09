using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZumenSearch.Models;
using ZumenSearch.Services;

namespace ZumenSearch.ViewModels.Transportation;

public partial class RailLineSelectViewModel : ObservableObject
{
    private string _query = string.Empty;
    public string Query
    {
        get => _query;
        set
        {
            if (SetProperty(ref _query, value))
            {
                if (string.IsNullOrEmpty(_query))
                {
                    SuggestedRailLines?.Clear();
                }
            }

            SearchRailLineCommand.NotifyCanExecuteChanged();
        }
    }

    private ObservableCollection<RailLine>? _suggestedRailLines = [];
    public ObservableCollection<RailLine>? SuggestedRailLines
    {
        get => _suggestedRailLines;
        set
        {
            if (SetProperty(ref _suggestedRailLines, value))
            {
                //
            }
        }
    }

    private RailLine? _selectedRailLine;
    public RailLine? SelectedRailLine
    {
        get => _selectedRailLine;
        set
        {
            if (SetProperty(ref _selectedRailLine, value))
            {
                if (_selectedRailLine is not null)
                {
                    SelectionChanged?.Invoke(this, _selectedRailLine);
                }
            }
        }
    }

    public event EventHandler<RailLine>? SelectionChanged;

    private readonly IDataAccessTransportationService _dataAccessTransportationService;

    public RailLineSelectViewModel(IDataAccessTransportationService dataAccessTransportationService)
    {
        _dataAccessTransportationService = dataAccessTransportationService;

        SuggestedRailLines = _dataAccessTransportationService.GetRailLinesBy(string.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanSearchRailLine))]
    public void SearchRailLine()
    {
        if (string.IsNullOrEmpty(Query))
        {
            SuggestedRailLines?.Clear();
            return;
        }

        SuggestedRailLines = _dataAccessTransportationService.GetRailLinesBy(_query);

    }
    private bool CanSearchRailLine()
    {
        return !string.IsNullOrEmpty(Query);
    }
}
