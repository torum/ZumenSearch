using ZumenSearch.Models;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessService
{
    ResultWrapper InitializeDatabase(string dataBaseFilePath);
    PropertiesResultWrapper SelectRecentProperties();

    // Rent Residential
    ResultWrapper UpsertRentResidential(Models.Rent.Residentials.Property building);
    PropertiesResultWrapper SelectRentResidentialsByNameKeyword(string keyword);
    RentResidentialBuildingSingleResultWrapper SelectRentResidentialById(string id);
    ResultWrapper DeleteRentResidential(string rentId);
    ResultWrapper UpsertRentResidentialListing(string rentId, Models.Rent.Residentials.Listing.Listing room);
    ListingsResultWrapper SelectRentResidentialListings();
    RentResidentialRoomSingleResultWrapper SelectRentResidentialListingById(string rentId, string roomId);
    ResultWrapper DeleteRentResidentialListing(string roomId);

    // Rent Commercial
    ResultWrapper UpsertRentCommercial(Models.Rent.Commercials.Property building);
    PropertiesResultWrapper SelectRentCommercialsByNameKeyword(string keyword);
    RentCommercialBuildingSingleResultWrapper SelectRentCommercialById(string id);
    ResultWrapper DeleteRentCommercial(string commercialId);
    ResultWrapper UpsertRentCommercialListing(string commercialId,Models.Rent.Commercials.Listing.Listing room);
    ListingsResultWrapper SelectRentCommercialListings();
    RentCommercialRoomSingleResultWrapper SelectRentCommercialListingById(string commercialId,string roomId);
    ResultWrapper DeleteRentCommercialListing(string roomId);

    // Person
    ResultWrapper UpsertRentLessor(Models.Base.PersonBase lessor);
    PersonsResultWrapper SelectRentLessorsByKeyword(string keyword);
    PersonSingleResultWrapper SelectRentLessorById(string id);
    ResultWrapper DeleteRentLessor(string id);


    // Sales
    ResultWrapper UpsertSaleResidential(Models.Sale.Residentials.Property building);

    PropertiesResultWrapper SelectSaleResidentialsByNameKeyword(string keyword);

    SaleResidentialBuildingSingleResultWrapper SelectSaleResidentialById(string id);

    ResultWrapper DeleteSaleResidential(string saleId);

    ResultWrapper UpsertSaleResidentialListing(string saleId,Models.Sale.Residentials.Listing.Listing room);

    ListingsResultWrapper SelectSaleResidentialListings();

    SaleResidentialRoomSingleResultWrapper SelectSaleResidentialListingById(string saleId,string roomId);

    ResultWrapper DeleteSaleResidentialListing(string roomId);


    // Brokers
    ResultWrapper UpsertBroker(Models.Base.PersonBase broker);
    PersonsResultWrapper SelectBrokersByKeyword(string keyword);
    PersonSingleResultWrapper SelectBrokerById(string id);
    ResultWrapper DeleteBroker(string id);




}



