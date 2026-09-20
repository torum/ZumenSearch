using ZumenSearch.Models;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessService
{
    ResultWrapper InitializeDatabase(string dataBaseFilePath);

    ResultWrapper UpsertRentResidential(Models.Rent.Residentials.Property building);
    PropertiesResultWrapper SelectRentResidentialsByNameKeyword(string keyword);
    RentResidentialBuildingSingleResultWrapper SelectRentResidentialById(string id);
    ResultWrapper DeleteRentResidential(string rentId);
    PropertiesResultWrapper SelectRecentProperties();

    ResultWrapper UpsertRentResidentialListing(string rentId, Models.Rent.Residentials.Listing.Listing room);
    ListingsResultWrapper SelectRentResidentialListings();
    RentResidentialRoomSingleResultWrapper SelectRentResidentialListingById(string rentId, string roomId);
    ResultWrapper DeleteRentResidentialListing(string roomId);

    ResultWrapper UpsertRentLessor(Models.Base.PersonBase lessor);
    PersonsResultWrapper SelectRentLessorsByKeyword(string keyword);
    PersonSingleResultWrapper SelectRentLessorById(string id);
    ResultWrapper DeleteRentLessor(string id);

    ResultWrapper UpsertBroker(Models.Base.PersonBase broker);
    PersonsResultWrapper SelectBrokersByKeyword(string keyword);
    PersonSingleResultWrapper SelectBrokerById(string id);
    ResultWrapper DeleteBroker(string id);

}



