using ZumenSearch.Models;

namespace ZumenSearch.Services;

public interface IDataAccessLocationService
{
    List<CountyAndCity> GetCountyAndCityByPref(string pref);

    List<WardAndOaza> GetWardAndOazaByPrefCountyCity(string pref, string county, string city);

    List<Choume> GetChoumeByPrefCountyCityWardOaza(string pref, string county, string city, string ward, string oaza);
}



