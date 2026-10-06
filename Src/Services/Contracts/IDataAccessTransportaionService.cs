using System.Collections.ObjectModel;
using ZumenSearch.Models.Transportation;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessTransportationService
{
    ObservableCollection<RailLine> GetRailLinesBy(string query);

    ObservableCollection<RailStation> GetRailStationsBy(string railLineCode, string query);
}



