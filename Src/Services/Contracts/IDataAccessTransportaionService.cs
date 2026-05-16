using System.Collections.ObjectModel;
using ZumenSearch.Models.Common;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessTransportationService
{
    ObservableCollection<RailLine> GetRailLinesBy(string query);

    ObservableCollection<RailStation> GetRailStationsBy(string _railLineCode, string query);
}



