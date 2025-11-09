using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZumenSearch.Models;
using ZumenSearch.Services;

namespace ZumenSearch.ViewModels.Transportation;

public partial class RailStationSelectViewModel : ObservableObject
{
    private readonly string _railLineCode = string.Empty;

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
                    SuggestedRailStations?.Clear();
                }
            }

            SearchRailStationCommand.NotifyCanExecuteChanged();
        }
    }

    private ObservableCollection<RailStation>? _suggestedRailStations = [];
    public ObservableCollection<RailStation>? SuggestedRailStations
    {
        get => _suggestedRailStations;
        set
        {
            if (SetProperty(ref _suggestedRailStations, value))
            {
                //
            }
        }
    }

    private RailStation? _selectedRailStation;
    public RailStation? SelectedRailStation
    {
        get => _selectedRailStation;
        set
        {
            if (SetProperty(ref _selectedRailStation, value))
            {
                if (_selectedRailStation is not null)
                {
                    SelectionChanged?.Invoke(this, _selectedRailStation);
                }
            }
        }
    }


    public event EventHandler<RailStation>? SelectionChanged;

    private readonly IDataAccessTransportationService _dataAccessTransportationService;

    public RailStationSelectViewModel(IDataAccessTransportationService dataAccessTransportationService, string railLineCode)
    {
        _dataAccessTransportationService = dataAccessTransportationService;
        _railLineCode = railLineCode;

        SuggestedRailStations = _dataAccessTransportationService.GetRailStationsBy(_railLineCode, _query);
    }

    [RelayCommand(CanExecute = nameof(CanSearchRailStation))]
    public void SearchRailStation()
    {
        if (string.IsNullOrEmpty(Query))
        {
            SuggestedRailStations?.Clear();
            return;
        }

        SuggestedRailStations = _dataAccessTransportationService.GetRailStationsBy(_railLineCode, _query);

    }
    private bool CanSearchRailStation()
    {
        return !string.IsNullOrEmpty(Query);
    }

}
