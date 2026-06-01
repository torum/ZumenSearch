using System.Collections.ObjectModel;

namespace ZumenSearch.Models.Common;

// Result Wrapper Class
internal abstract class ResultWrapper
{
    public ErrorObject Error = new();
    public bool IsError = false;
}

internal class SqliteDataAccessResultWrapper : ResultWrapper
{
    public int AffectedCount = 0;
}

internal sealed class SqliteDataAccessInsertResultWrapper : SqliteDataAccessResultWrapper
{
    //public List<EntryItem> InsertedEntries = new();
}

internal sealed class SqliteDataAccessSelectRentResidentialResultWrapper : SqliteDataAccessResultWrapper
{
    public ObservableCollection<Rent.Residentials.Bldg.EntryResidentialSearchResult> SelectedEntries = [];
}

internal sealed class SqliteDataAccessSelectRentResidentialFullResultWrapper : SqliteDataAccessResultWrapper
{
    public Rent.Residentials.Bldg.EntryResidential? EntryFull;
}

internal sealed class SqliteDataAccessSelectRentResidentialUnitsResultWrapper : SqliteDataAccessResultWrapper
{
    public ObservableCollection<Rent.Residentials.Unit.UnitResidentialSearchResult> SelectedUnits = [];
}
//

