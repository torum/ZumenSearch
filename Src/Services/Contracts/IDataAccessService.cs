using ZumenSearch.Models;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessService
{
    ResultWrapper InitializeDatabase(string dataBaseFilePath);

    ResultWrapper InsertRentResidential(Models.Rent.Residentials.Property building);

    ResultWrapper UpdateRentResidential(Models.Rent.Residentials.Property building);

    ResultWrapper DeleteRentResidential(string rentId);

    SelectPropertiesResultWrapper SelectRecentProperties();

    SelectPropertiesResultWrapper SelectRentResidentialsByNameKeyword(string keyword);

    SelectRentResidentialBuildingSingleResultWrapper SelectRentResidentialById(string id);

    ResultWrapper UpsertRentResidentialListing(string rentId, Models.Rent.Residentials.Listing.Listing room);

    SelectListingResultWrapper SelectRentResidentialListings();

    SelectRentResidentialRoomSingleResultWrapper SelectRentResidentialListingById(string rentId, string roomId);

    ResultWrapper DeleteRentResidentialListing(string roomId);

    ResultWrapper UpsertRentLessor(Models.Rent.Lessors.Person lessor);

    SelectPersonsResultWrapper SelectRentLessorByKeyword(string keyword);

    SelectRentLessorSingleResultWrapper SelectRentLessorById(string id);

}



