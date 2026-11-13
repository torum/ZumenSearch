using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using ZumenSearch.Models.Transportation;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class TransportationEntryViewModel : ObservableObject
{
    public RailLine RailLine
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                RailStation = new RailStation(string.Empty, string.Empty, string.Empty);
            }
        }
    } = new(string.Empty, string.Empty);

    [ObservableProperty]
    public partial RailStation RailStation { get; set; } = new(string.Empty, string.Empty, string.Empty);

    [ObservableProperty]
    public partial string StationWalk { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BusStop { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BusRide { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BusWalk { get; set; } = string.Empty;

    public void Load(Train train, string busStop, int busRide, int busWalk)
    {
        RailLine = new(train.RailLine.LineCode, train.RailLine.LineName);
        RailStation = new(train.RailStation.LineCode, train.RailStation.StationCode, train.RailStation.StationName);
        StationWalk = FormatMinutes(train.EkiToho);
        BusStop = busStop;
        BusRide = FormatMinutes(busRide);
        BusWalk = FormatMinutes(busWalk);
    }

    public void ApplyTrain(Train train)
    {
        train.SetRailLine(RailLine.LineCode, RailLine.LineName);
        train.SetRailStation(RailStation.LineCode, RailStation.StationCode, RailStation.StationName);
        train.SetEkiToho(Helpers.Common.ReplaceZenkakuNumbers(StationWalk));
    }

    private static string FormatMinutes(int value) => value == 0 ? string.Empty : value.ToString(CultureInfo.InvariantCulture);
}
