using System.Collections.ObjectModel;
using ZumenSearch.Models;

namespace ZumenSearch.Services;

public interface IDataAccessTransportationService
{
    ObservableCollection<RailLine> GetRailLinesBy(string query);

    ObservableCollection<RailStation> GetRailStationsBy(string _railLineCode, string query);
}



