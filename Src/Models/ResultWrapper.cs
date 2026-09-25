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

// Properties

public sealed class PropertiesResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.PropertySearchResultItem> PropertySearchResult = [];
}

// Rent Residential

public sealed class RentResidentialBuildingSingleResultWrapper : ResultWrapperBase
{
    public Models.Rent.Residentials.Property? Building;
}

// TODO:
public sealed class ListingsResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.ListingSearchResultItem> ListingSearchResult = [];
}

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

public sealed class RentCommercialRoomSingleResultWrapper: ResultWrapperBase
{
    public string BuildingName = string.Empty;

    public Models.Rent.Commercials.Listing.Listing? Room;
}


// Person

public sealed class PersonsResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.PersonSearchResultItem> PersonSearchResult = [];
}

public sealed class PersonSingleResultWrapper : ResultWrapperBase
{
    public Models.Base.PersonBase? Person;
}


// Sales
public sealed class SaleResidentialBuildingSingleResultWrapper : ResultWrapperBase
{
    public Models.Sale.Residentials.Property? Building;
}

public sealed class SaleResidentialRoomSingleResultWrapper : ResultWrapperBase
{
    public string BuildingName = string.Empty;

    public Models.Sale.Residentials.Listing.Listing? Room;
}

