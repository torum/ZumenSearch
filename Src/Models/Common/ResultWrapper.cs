using System.Collections.ObjectModel;

namespace ZumenSearch.Models.Common;

// Result Wrapper Class
public abstract class ResultWrapper
{
    public ErrorObject Error = new();
    public bool IsError = false;
}

public class SqliteDataAccessResultWrapper : ResultWrapper
{
    public int AffectedCount = 0;
}

public sealed class SqliteDataAccessInsertResultWrapper : SqliteDataAccessResultWrapper
{
    //public List<EntryItem> InsertedEntries = new();
}

public sealed class SqliteDataAccessSelectRentResidentialBuildingsResultWrapper : SqliteDataAccessResultWrapper
{
    public ObservableCollection<Rent.Residentials.PropertySearchResultItem> PropertySearchResult = [];
}

public sealed class SqliteDataAccessSelectRentResidentialBuildingSingleResultWrapper : SqliteDataAccessResultWrapper
{
    public Rent.Residentials.Bldg.Property? Building;
}

public sealed class SqliteDataAccessSelectRentResidentialRoomsResultWrapper : SqliteDataAccessResultWrapper
{
    public ObservableCollection<Rent.Residentials.ListingSearchResultItem> ListingSearchResult = [];
}

