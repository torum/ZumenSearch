using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Presentation.Search;
namespace ZumenSearch.Infrastructure.DataAccess.Contracts;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Required for clarity. And I don't care about Visual Basic Keywords.")]

public interface IDataAccessService
{
    ZumenSearch.Application.Results.ResultWrapper InitializeDatabase(string dataBaseFilePath);

    // All Properties
    ZumenSearch.Application.Results.PropertySearchResultWrapper SelectRecentProperties();
    ZumenSearch.Application.Results.PropertySearchResultWrapper SelectPropertiesByKeyword(string keyword, PropertySearchTarget searchTarget = PropertySearchTarget.PropertyName);

    // Rent Residential
    ZumenSearch.Application.Results.ResultWrapper UpsertRentResidential(ZumenSearch.Domain.Entities.Rent.Residentials.Property property);
    RentResidentialPropertyResultWrapper SelectRentResidentialById(string propertyId, bool duplicate = false);
    ZumenSearch.Application.Results.ResultWrapper DeleteRentResidential(string propertyId);
    ZumenSearch.Application.Results.ResultWrapper UpsertRentResidentialListing(string propertyId, ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing);
    ZumenSearch.Application.Results.ListingSearchResultWrapper SelectRentResidentialListings(RentResidentialListingSearchCriteria criteria);
    RentResidentialListingResultWrapper SelectRentResidentialListingById(string propertyId, string listingId, bool duplicate = false);
    ZumenSearch.Application.Results.ResultWrapper DeleteRentResidentialListing(string listingId);

    // Rent Commercial
    ZumenSearch.Application.Results.ResultWrapper UpsertRentCommercial(ZumenSearch.Domain.Entities.Rent.Commercials.Property property, IReadOnlyCollection<ZumenSearch.Presentation.Features.Rent.Commercials.Models.CommercialListingDraft> pendingListings, IReadOnlyCollection<string> deletedListingIds, bool? expectedCommittedUnitOwnership);
    ZumenSearch.Presentation.Features.Rent.Commercials.Models.PropertyResultWrapper SelectRentCommercialById(string propertyId);
    ZumenSearch.Application.Results.ResultWrapper DeleteRentCommercial(string propertyId);
    ZumenSearch.Application.Results.ResultWrapper UpsertRentCommercialListing(string propertyId,ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing);
    ZumenSearch.Application.Results.ListingSearchResultWrapper SelectRentCommercialListings(string keyword);
    ZumenSearch.Presentation.Features.Rent.Commercials.Models.ListingResultWrapper SelectRentCommercialListingById(string propertyId, string listingId);
    ZumenSearch.Application.Results.ResultWrapper DeleteRentCommercialListing(string listingId);

    // Lessor
    ZumenSearch.Application.Results.ResultWrapper UpsertRentLessor(ZumenSearch.Domain.Entities.Base.PersonBase lessor);
    ZumenSearch.Application.Results.PersonsSearchResultWrapper SelectRentLessorsByKeyword(string keyword);
    ZumenSearch.Application.Results.PersonResultWrapper SelectRentLessorById(string lessorId);
    ZumenSearch.Application.Results.ResultWrapper DeleteRentLessor(string lessorId);

    // Sales
    ZumenSearch.Application.Results.ResultWrapper UpsertSaleResidential(ZumenSearch.Domain.Entities.Sale.Residentials.Property property);

    PropertySearchResultWrapper SelectSaleResidentialsByNameKeyword(string keyword);

    ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyResultWrapper SelectSaleResidentialById(string id);

    ZumenSearch.Application.Results.ResultWrapper DeleteSaleResidential(string saleId);

    ZumenSearch.Application.Results.ResultWrapper UpsertSaleResidentialListing(string saleId,ZumenSearch.Domain.Entities.Sale.Residentials.Listing room);

    ZumenSearch.Application.Results.ListingSearchResultWrapper SelectSaleResidentialListings();

    ZumenSearch.Presentation.Features.Sale.Residentials.Models.ListingResultWrapper SelectSaleResidentialListingById(string saleId,string roomId);

    ZumenSearch.Application.Results.ResultWrapper DeleteSaleResidentialListing(string roomId);

    // Brokers
    ZumenSearch.Application.Results.ResultWrapper UpsertBroker(ZumenSearch.Domain.Entities.Base.PersonBase broker);
    ZumenSearch.Application.Results.PersonsSearchResultWrapper SelectBrokersByKeyword(string keyword);
    ZumenSearch.Application.Results.PersonResultWrapper SelectBrokerById(string brokerId);
    ZumenSearch.Application.Results.ResultWrapper DeleteBroker(string brokerId);

}
