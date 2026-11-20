using ZumenSearch.Domain.ValueObjects.Location;
namespace ZumenSearch.Infrastructure.DataAccess.Contracts;

public interface IDataAccessLocationService
{
    List<CountyAndCity> GetCountyAndCityByPref(string pref);

    List<WardAndOaza> GetWardAndOazaByPrefCountyCity(string pref, string county, string city);

    List<Choume> GetChoumeByPrefCountyCityWardOaza(string pref, string county, string city, string ward, string oaza);
}



