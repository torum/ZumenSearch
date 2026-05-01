using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;

namespace ZumenSearch.Services;

public interface IDataAccessLocationService
{
    List<CountyAndCity> GetCountyAndCityByPref(string pref);

    List<WardAndOaza> GetWardAndOazaByPrefCountyCity(string pref, string county, string city);

    List<Choume> GetChoumeByPrefCountyCityWardOaza(string pref, string county, string city, string ward, string oaza);
}



