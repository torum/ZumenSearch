using System.Collections.ObjectModel;
using ZumenSearch.Models.Common;

namespace ZumenSearch.Models;

// Result Wrapper Class
public abstract class ResultWrapperBase
{
    public ErrorObject Error = new();
    public bool IsError = false;
}

public class ResultWrapper : ResultWrapperBase
{
    public int AffectedCount = 0;
}

// Common Properties SearchResult
public sealed class PropertiesResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.PropertySearchResultItem> PropertySearchResult = [];
}

// Common Listings SearchResult
public sealed class ListingsResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.ListingSearchResultItem> ListingSearchResult = [];
}

// Rent Residential
public sealed class RentResidentialBuildingSingleResultWrapper : ResultWrapperBase
{
    public Models.Rent.Residentials.Property? Building;
}

// Rent Residential listing
public sealed class RentResidentialRoomSingleResultWrapper : ResultWrapperBase
{
    public string BuildingName = string.Empty;
    public Models.Rent.Residentials.Listing.Listing? Room;
}

// Rent Commercial
public sealed class RentCommercialBuildingSingleResultWrapper : ResultWrapperBase
{
    public Models.Rent.Commercials.Property? Building;
}

// Rent Commercial listing
public sealed class RentCommercialUnitSingleResultWrapper: ResultWrapperBase
{
    public string BuildingName = string.Empty;

    public Models.Rent.Commercials.Listing.Listing? Unit;
}

// Person SearchResult
public sealed class PersonsResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.PersonSearchResultItem> PersonSearchResult = [];
}

// Person
public sealed class PersonSingleResultWrapper : ResultWrapperBase
{
    public Models.Base.PersonBase? Person;
}

// Sales Residential
public sealed class SaleResidentialBuildingSingleResultWrapper : ResultWrapperBase
{
    public Models.Sale.Residentials.Property? Building;
}

// Sales Residential listing
public sealed class SaleResidentialRoomSingleResultWrapper : ResultWrapperBase
{
    public string BuildingName = string.Empty;

    public Models.Sale.Residentials.Listing.Listing? Room;
}

