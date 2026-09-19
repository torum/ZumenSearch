using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Dialogs;

public partial class RailStationViewModel : ObservableObject
{
    private readonly string _railLineCode = string.Empty;

    public string Query
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (string.IsNullOrEmpty(field))
                {
                    SuggestedRailStations?.Clear();
                }
            }

            SearchRailStationCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    public ObservableCollection<RailStation>? SuggestedRailStations
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

    public RailStation? SelectedRailStation
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

    public event EventHandler<RailStation>? SelectionChanged;

    private readonly IDataAccessTransportationService _dataAccessTransportationService;

    public RailStationViewModel(IDataAccessTransportationService dataAccessTransportationService, string railLineCode)
    {
        _dataAccessTransportationService = dataAccessTransportationService;
        _railLineCode = railLineCode;

        // TODO: try catch
        SuggestedRailStations = _dataAccessTransportationService.GetRailStationsBy(_railLineCode, Query);
    }

    [RelayCommand(CanExecute = nameof(CanSearchRailStation))]
    public void SearchRailStation()
    {
        if (string.IsNullOrEmpty(Query))
        {
            SuggestedRailStations?.Clear();
            return;
        }

        // TODO: try catch
        SuggestedRailStations = _dataAccessTransportationService.GetRailStationsBy(_railLineCode, Query);

    }
    private bool CanSearchRailStation()
    {
        return !string.IsNullOrEmpty(Query);
    }

}
