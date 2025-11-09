using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;

namespace ZumenSearch.Services;

public interface IDataAccessTransportationService
{
    ObservableCollection<RailLine> GetRailLinesBy(string query);

    ObservableCollection<RailStation> GetRailStationsBy(string _railLineCode, string query);
}



