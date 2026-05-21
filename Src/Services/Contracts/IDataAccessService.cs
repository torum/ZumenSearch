using ZumenSearch.Models.Common;
using ZumenSearch.Models.Rent.Residentials;

namespace ZumenSearch.Services.Contracts;

internal interface IDataAccessService
{
    SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath);

    SqliteDataAccessResultWrapper InsertRentResidential(Models.Rent.Residentials.EntryResidentialFull entry);

    SqliteDataAccessResultWrapper UpdateRentResidential(Models.Rent.Residentials.EntryResidentialFull entry);

    SqliteDataAccessResultWrapper DeleteRentResidential(string rentId);

    SqliteDataAccessSelectRentResidentialResultWrapper SelectRentResidentialsByNameKeyword(string keyword);

    SqliteDataAccessSelectRentResidentialFullResultWrapper SelectRentResidentialById(string id);

    SqliteDataAccessResultWrapper UpsertRentResidentialUnit(string rentId, Models.Rent.Residentials.UnitResidential room);

    SqliteDataAccessSelectRentResidentialUnitsResultWrapper SelectRentResidentialUnits();

}



