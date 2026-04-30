using System.Collections.ObjectModel;

namespace ZumenSearch.Models;

// Result Wrapper Class
public abstract class ResultWrapper
{
    public ErrorObject Error = new();
    public bool IsError = false;
}

public class SqliteDataAccessResultWrapper: ResultWrapper
{
    public int AffectedCount = 0;
}

public class SqliteDataAccessInsertResultWrapper: SqliteDataAccessResultWrapper
{
    //public List<EntryItem> InsertedEntries = new();
}

public class SqliteDataAccessSelectRentResidentialResultWrapper : SqliteDataAccessResultWrapper
{
    public ObservableCollection<Models.Rent.Residentials.EntryResidentialSearchResult> SelectedEntries = [];
}

public class SqliteDataAccessSelectRentResidentialFullResultWrapper : SqliteDataAccessResultWrapper
{
    public Models.Rent.Residentials.EntryResidentialFull? EntryFull;
}

