using ZumenSearch.Models.SearchResult;

namespace ZumenSearch.Services.Contracts;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Required for clarity. And I don't care about Visual Basic Keywords.")]

public interface IDataAccessService
{
    Models.SearchResult.ResultWrapper InitializeDatabase(string dataBaseFilePath);

    // All Properties
    Models.SearchResult.PropertySearchResultWrapper SelectRecentProperties();
    Models.SearchResult.PropertySearchResultWrapper SelectPropertiesByKeyword(string keyword);

    // Rent Residential
    Models.SearchResult.ResultWrapper UpsertRentResidential(Models.Rent.Residentials.Property property);
    Models.Rent.Residentials.PropertyResultWrapper SelectRentResidentialById(string propertyId, bool duplicate = false);
    Models.SearchResult.ResultWrapper DeleteRentResidential(string propertyId);
    Models.SearchResult.ResultWrapper UpsertRentResidentialListing(string propertyId, Models.Rent.Residentials.Listing listing);
    Models.SearchResult.ListingSearchResultWrapper SelectRentResidentialListings(Models.Rent.Residentials.ListingSearchCriteria criteria);
    Models.Rent.Residentials.ListingResultWrapper SelectRentResidentialListingById(string propertyId, string listingId, bool duplicate = false);
    Models.SearchResult.ResultWrapper DeleteRentResidentialListing(string listingId);

    // Rent Commercial
    Models.SearchResult.ResultWrapper UpsertRentCommercial(Models.Rent.Commercials.Property property, IReadOnlyCollection<Models.Rent.Commercials.CommercialListingDraft> pendingListings, IReadOnlyCollection<string> deletedListingIds, bool? expectedCommittedUnitOwnership);
    Models.Rent.Commercials.PropertyResultWrapper SelectRentCommercialById(string propertyId);
    Models.SearchResult.ResultWrapper DeleteRentCommercial(string propertyId);
    Models.SearchResult.ResultWrapper UpsertRentCommercialListing(string propertyId,Models.Rent.Commercials.Listing.Listing listing);
    Models.SearchResult.ListingSearchResultWrapper SelectRentCommercialListings(string keyword);
    Models.Rent.Commercials.ListingResultWrapper SelectRentCommercialListingById(string propertyId, string listingId);
    Models.SearchResult.ResultWrapper DeleteRentCommercialListing(string listingId);

    // Lessor
    Models.SearchResult.ResultWrapper UpsertRentLessor(Models.Base.PersonBase lessor);
    Models.SearchResult.PersonsSearchResultWrapper SelectRentLessorsByKeyword(string keyword);
    Models.Person.ResultWrapper SelectRentLessorById(string lessorId);
    Models.SearchResult.ResultWrapper DeleteRentLessor(string lessorId);

    // Sales
    Models.SearchResult.ResultWrapper UpsertSaleResidential(Models.Sale.Residentials.Property property);

    PropertySearchResultWrapper SelectSaleResidentialsByNameKeyword(string keyword);

    Models.Sale.Residentials.PropertyResultWrapper SelectSaleResidentialById(string id);

    Models.SearchResult.ResultWrapper DeleteSaleResidential(string saleId);

    Models.SearchResult.ResultWrapper UpsertSaleResidentialListing(string saleId,Models.Sale.Residentials.Listing room);

    Models.SearchResult.ListingSearchResultWrapper SelectSaleResidentialListings();

    Models.Sale.Residentials.ListingResultWrapper SelectSaleResidentialListingById(string saleId,string roomId);

    Models.SearchResult.ResultWrapper DeleteSaleResidentialListing(string roomId);

    // Brokers
    Models.SearchResult.ResultWrapper UpsertBroker(Models.Base.PersonBase broker);
    Models.SearchResult.PersonsSearchResultWrapper SelectBrokersByKeyword(string keyword);
    Models.Person.ResultWrapper SelectBrokerById(string brokerId);
    Models.SearchResult.ResultWrapper DeleteBroker(string brokerId);

}
