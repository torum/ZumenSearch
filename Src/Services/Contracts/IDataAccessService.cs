using ZumenSearch.Models;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessService
{
    ResultWrapper InitializeDatabase(string dataBaseFilePath);

    ResultWrapper InsertRentResidential(Models.Rent.Residentials.Property building);

    ResultWrapper UpdateRentResidential(Models.Rent.Residentials.Property building);

    ResultWrapper DeleteRentResidential(string rentId);

    PropertiesResultWrapper SelectRecentProperties();

    PropertiesResultWrapper SelectRentResidentialsByNameKeyword(string keyword);

    RentResidentialBuildingSingleResultWrapper SelectRentResidentialById(string id);

    ResultWrapper UpsertRentResidentialListing(string rentId, Models.Rent.Residentials.Listing.Listing room);

    ListingsResultWrapper SelectRentResidentialListings();

    RentResidentialRoomSingleResultWrapper SelectRentResidentialListingById(string rentId, string roomId);

    ResultWrapper DeleteRentResidentialListing(string roomId);

    ResultWrapper UpsertRentLessor(Models.Rent.Lessors.Person lessor);

    PersonsResultWrapper SelectRentLessorByKeyword(string keyword);

    RentLessorSingleResultWrapper SelectRentLessorById(string id);

    ResultWrapper DeleteRentLessor(string id);

}



