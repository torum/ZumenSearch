using ZumenSearch.Models.SearchResult;

namespace ZumenSearch.Services.Contracts;

public interface IDataAccessService
{
    ResultWrapper InitializeDatabase(string dataBaseFilePath);

    // All Properties
    PropertySearchResultWrapper SelectRecentProperties();
    PropertySearchResultWrapper SelectPropertiesByKeyword(string keyword);

    // Rent Residential
    ResultWrapper UpsertRentResidential(Models.Rent.Residentials.Property building);
    Models.Rent.Residentials.PropertyResultWrapper SelectRentResidentialById(string id);
    ResultWrapper DeleteRentResidential(string propertyId);
    ResultWrapper UpsertRentResidentialListing(string propertyId, Models.Rent.Residentials.Listing room);
    ListingSearchResultWrapper SelectRentResidentialListings();
    Models.Rent.Residentials.ListingResultWrapper SelectRentResidentialListingById(string propertyId, string roomId);
    ResultWrapper DeleteRentResidentialListing(string roomId);

    // Rent Commercial
    ResultWrapper UpsertRentCommercial(Models.Rent.Commercials.Property building);
    //PropertiesResultWrapper SelectRentCommercialsByNameKeyword(string keyword);
    Models.Rent.Commercials.PropertyResultWrapper SelectRentCommercialById(string id);
    ResultWrapper DeleteRentCommercial(string commercialId);
    ResultWrapper UpsertRentCommercialListing(string commercialId,Models.Rent.Commercials.Listing.Listing room);
    ListingSearchResultWrapper SelectRentCommercialListings();
    Models.Rent.Commercials.ListingResultWrapper SelectRentCommercialListingById(string commercialId,string roomId);
    ResultWrapper DeleteRentCommercialListing(string roomId);

    // Lessor
    ResultWrapper UpsertRentLessor(Models.Base.PersonBase lessor);
    PersonsSearchResultWrapper SelectRentLessorsByKeyword(string keyword);
    Models.Person.ResultWrapper SelectRentLessorById(string id);
    ResultWrapper DeleteRentLessor(string id);


    // Sales
    ResultWrapper UpsertSaleResidential(Models.Sale.Residentials.Property building);

    PropertySearchResultWrapper SelectSaleResidentialsByNameKeyword(string keyword);

    Models.Sale.Residentials.PropertyResultWrapper SelectSaleResidentialById(string id);

    ResultWrapper DeleteSaleResidential(string saleId);

    ResultWrapper UpsertSaleResidentialListing(string saleId,Models.Sale.Residentials.Listing room);

    ListingSearchResultWrapper SelectSaleResidentialListings();

    Models.Sale.Residentials.ListingResultWrapper SelectSaleResidentialListingById(string saleId,string roomId);

    ResultWrapper DeleteSaleResidentialListing(string roomId);


    // Brokers
    ResultWrapper UpsertBroker(Models.Base.PersonBase broker);
    PersonsSearchResultWrapper SelectBrokersByKeyword(string keyword);
    Models.Person.ResultWrapper SelectBrokerById(string id);
    ResultWrapper DeleteBroker(string id);




}



