using ZumenSearch.Models.Common;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessService
{
    SqliteDataAccessResultWrapper InitializeDatabase(string dataBaseFilePath);

    SqliteDataAccessResultWrapper InsertRentResidential(Models.Rent.Residentials.Bldg.Property building);

    SqliteDataAccessResultWrapper UpdateRentResidential(Models.Rent.Residentials.Bldg.Property building);

    SqliteDataAccessResultWrapper DeleteRentResidential(string rentId);

    SqliteDataAccessSelectRecentPropertiesResultWrapper SelectRecentProperties();

    SqliteDataAccessSelectRentResidentialBuildingsResultWrapper SelectRentResidentialsByNameKeyword(string keyword);

    SqliteDataAccessSelectRentResidentialBuildingSingleResultWrapper SelectRentResidentialById(string id);

    SqliteDataAccessResultWrapper UpsertRentResidentialListing(string rentId, Models.Rent.Residentials.Room.Listing room);

    SqliteDataAccessSelectRentResidentialRoomsResultWrapper SelectRentResidentialListings();

    SqliteDataAccessSelectRentResidentialRoomSingleResultWrapper SelectRentResidentialListingById(string rentId, string roomId);

    SqliteDataAccessResultWrapper DeleteRentResidentialListing(string roomId);
}



