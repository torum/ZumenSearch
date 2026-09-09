using ZumenSearch.Models.Common;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessService
{
    SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath);

    SqliteDataAccessResultWrapper InsertRentResidential(Models.Rent.Residentials.Bldg.Property building);

    SqliteDataAccessResultWrapper UpdateRentResidential(Models.Rent.Residentials.Bldg.Property building);

    SqliteDataAccessResultWrapper DeleteRentResidential(string rentId);

    SqliteDataAccessSelectRentResidentialBuildingsResultWrapper SelectRentResidentialsByNameKeyword(string keyword);

    SqliteDataAccessSelectRentResidentialBuildingSingleResultWrapper SelectRentResidentialById(string id);

    SqliteDataAccessResultWrapper UpsertRentResidentialUnit(string rentId, Models.Rent.Residentials.Room.Listing room);

    SqliteDataAccessSelectRentResidentialRoomsResultWrapper SelectRentResidentialRooms();

}



