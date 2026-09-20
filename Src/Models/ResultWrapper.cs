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

public sealed class PropertiesResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.PropertySearchResultItem> PropertySearchResult = [];
}

public sealed class RentResidentialBuildingSingleResultWrapper : ResultWrapperBase
{
    public Models.Rent.Residentials.Property? Building;
}

public sealed class ListingsResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.ListingSearchResultItem> ListingSearchResult = [];
}

public sealed class RentResidentialRoomSingleResultWrapper : ResultWrapperBase
{
    public string BuildingName = string.Empty;
    public Models.Rent.Residentials.Listing.Listing? Room;
}

public sealed class PersonsResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.PersonSearchResultItem> PersonSearchResult = [];
}

/*
public sealed class RentLessorSingleResultWrapper : ResultWrapperBase
{
    public Models.Base.PersonBase? Lessor;
}

public sealed class BrokerSingleResultWrapper : ResultWrapperBase
{
    public Models.Brokers.PersonWrappers? Broker;
}
*/

public sealed class PersonSingleResultWrapper : ResultWrapperBase
{
    public Models.Base.PersonBase? Person;
}

