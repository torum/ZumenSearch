using System.Collections.ObjectModel;
using ZumenSearch.Domain.ValueObjects.Transportation;
namespace ZumenSearch.Infrastructure.DataAccess.Contracts;

public interface IDataAccessTransportationService
{
    ObservableCollection<RailLine> GetRailLinesBy(string query);

    ObservableCollection<RailStation> GetRailStationsBy(string railLineCode, string query);
}



