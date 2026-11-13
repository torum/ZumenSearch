using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public sealed class TransportationEntryViewModel : ObservableObject, IDisposable
{
    private readonly PropertyViewModel _owner;
    private readonly int _index;

    private static readonly string[] LineProperties = [nameof(PropertyViewModel.SelectedRailLine1), nameof(PropertyViewModel.SelectedRailLine2), nameof(PropertyViewModel.SelectedRailLine3), nameof(PropertyViewModel.SelectedRailLine4)];
    private static readonly string[] StationProperties = [nameof(PropertyViewModel.SelectedRailStation1), nameof(PropertyViewModel.SelectedRailStation2), nameof(PropertyViewModel.SelectedRailStation3), nameof(PropertyViewModel.SelectedRailStation4)];
    private static readonly string[] WalkProperties = [nameof(PropertyViewModel.EkiToho1), nameof(PropertyViewModel.EkiToho2), nameof(PropertyViewModel.EkiToho3), nameof(PropertyViewModel.EkiToho4)];
    private static readonly string[] StopProperties = [nameof(PropertyViewModel.BusStop1), nameof(PropertyViewModel.BusStop2), nameof(PropertyViewModel.BusStop3), nameof(PropertyViewModel.BusStop4)];
    private static readonly string[] RideProperties = [nameof(PropertyViewModel.BusJyousya1), nameof(PropertyViewModel.BusJyousya2), nameof(PropertyViewModel.BusJyousya3), nameof(PropertyViewModel.BusJyousya4)];
    private static readonly string[] BusWalkProperties = [nameof(PropertyViewModel.BusStopToho1), nameof(PropertyViewModel.BusStopToho2), nameof(PropertyViewModel.BusStopToho3), nameof(PropertyViewModel.BusStopToho4)];

    public TransportationEntryViewModel(PropertyViewModel owner, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 3);
        _owner = owner;
        _index = index;
        _owner.PropertyChanged += OwnerPropertyChanged;
    }

    public string RailLineName => _index switch { 0 => _owner.SelectedRailLine1.LineName, 1 => _owner.SelectedRailLine2.LineName, 2 => _owner.SelectedRailLine3.LineName, _ => _owner.SelectedRailLine4.LineName };
    public string RailStationName => _index switch { 0 => _owner.SelectedRailStation1.StationName, 1 => _owner.SelectedRailStation2.StationName, 2 => _owner.SelectedRailStation3.StationName, _ => _owner.SelectedRailStation4.StationName };

    public string StationWalk
    {
        get => _index switch { 0 => _owner.EkiToho1, 1 => _owner.EkiToho2, 2 => _owner.EkiToho3, _ => _owner.EkiToho4 };
        set { switch (_index) { case 0: _owner.EkiToho1 = value; break; case 1: _owner.EkiToho2 = value; break; case 2: _owner.EkiToho3 = value; break; default: _owner.EkiToho4 = value; break; } }
    }

    public string BusStop
    {
        get => _index switch { 0 => _owner.BusStop1, 1 => _owner.BusStop2, 2 => _owner.BusStop3, _ => _owner.BusStop4 };
        set { switch (_index) { case 0: _owner.BusStop1 = value; break; case 1: _owner.BusStop2 = value; break; case 2: _owner.BusStop3 = value; break; default: _owner.BusStop4 = value; break; } }
    }

    public string BusRide
    {
        get => _index switch { 0 => _owner.BusJyousya1, 1 => _owner.BusJyousya2, 2 => _owner.BusJyousya3, _ => _owner.BusJyousya4 };
        set { switch (_index) { case 0: _owner.BusJyousya1 = value; break; case 1: _owner.BusJyousya2 = value; break; case 2: _owner.BusJyousya3 = value; break; default: _owner.BusJyousya4 = value; break; } }
    }

    public string BusWalk
    {
        get => _index switch { 0 => _owner.BusStopToho1, 1 => _owner.BusStopToho2, 2 => _owner.BusStopToho3, _ => _owner.BusStopToho4 };
        set { switch (_index) { case 0: _owner.BusStopToho1 = value; break; case 1: _owner.BusStopToho2 = value; break; case 2: _owner.BusStopToho3 = value; break; default: _owner.BusStopToho4 = value; break; } }
    }

    public IAsyncRelayCommand ShowRailLineSelectCommand => _index switch { 0 => _owner.ShowRailLineSelect1Command, 1 => _owner.ShowRailLineSelect2Command, 2 => _owner.ShowRailLineSelect3Command, _ => _owner.ShowRailLineSelect4Command };
    public IAsyncRelayCommand ShowRailStationSelectCommand => _index switch { 0 => _owner.ShowRailStationSelect1Command, 1 => _owner.ShowRailStationSelect2Command, 2 => _owner.ShowRailStationSelect3Command, _ => _owner.ShowRailStationSelect4Command };

    private void OwnerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == LineProperties[_index]) OnPropertyChanged(nameof(RailLineName));
        else if (e.PropertyName == StationProperties[_index]) OnPropertyChanged(nameof(RailStationName));
        else if (e.PropertyName == WalkProperties[_index]) OnPropertyChanged(nameof(StationWalk));
        else if (e.PropertyName == StopProperties[_index]) OnPropertyChanged(nameof(BusStop));
        else if (e.PropertyName == RideProperties[_index]) OnPropertyChanged(nameof(BusRide));
        else if (e.PropertyName == BusWalkProperties[_index]) OnPropertyChanged(nameof(BusWalk));
    }

    public void Dispose() => _owner.PropertyChanged -= OwnerPropertyChanged;
}
