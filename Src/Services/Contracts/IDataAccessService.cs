using ZumenSearch.Models.Common;

namespace ZumenSearch.Services.Contracts;

internal interface IDataAccessService
{
    SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath);

    SqliteDataAccessResultWrapper InsertRentResidential(Models.Rent.Residentials.Bldg.EntryResidential entry);

    SqliteDataAccessResultWrapper UpdateRentResidential(Models.Rent.Residentials.Bldg.EntryResidential entry);

    SqliteDataAccessResultWrapper DeleteRentResidential(string rentId);

    SqliteDataAccessSelectRentResidentialResultWrapper SelectRentResidentialsByNameKeyword(string keyword);

    SqliteDataAccessSelectRentResidentialFullResultWrapper SelectRentResidentialById(string id);

    SqliteDataAccessResultWrapper UpsertRentResidentialUnit(string rentId, Models.Rent.Residentials.Unit.UnitResidential room);

    SqliteDataAccessSelectRentResidentialUnitsResultWrapper SelectRentResidentialUnits();

}



