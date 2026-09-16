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

public sealed class SelectPropertiesResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.PropertySearchResultItem> PropertySearchResult = [];
}

public sealed class SelectRentResidentialBuildingSingleResultWrapper : ResultWrapperBase
{
    public Models.Rent.Residentials.Property? Building;
}

public sealed class SelectListingResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.ListingSearchResultItem> ListingSearchResult = [];
}

public sealed class SelectRentResidentialRoomSingleResultWrapper : ResultWrapperBase
{
    public string BuildingName = string.Empty;
    public Models.Rent.Residentials.Listing.Listing? Room;
}

public sealed class SelectPersonsResultWrapper : ResultWrapperBase
{
    public ObservableCollection<Models.Common.PersonSearchResultItem> PersonSearchResult = [];
}

public sealed class SelectRentLessorSingleResultWrapper : ResultWrapperBase
{
    public Models.Rent.Lessors.Person? Lessor;
}

