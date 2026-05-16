using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Transportation;

public partial class RailLineSelectViewModel : ObservableObject
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
                    SuggestedRailLines?.Clear();
                }
            }

            SearchRailLineCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    public ObservableCollection<RailLine>? SuggestedRailLines
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

    public RailLine? SelectedRailLine
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

        SuggestedRailLines = _dataAccessTransportationService.GetRailLinesBy(Query);

    }
    private bool CanSearchRailLine()
    {
        return !string.IsNullOrEmpty(Query);
    }
}
