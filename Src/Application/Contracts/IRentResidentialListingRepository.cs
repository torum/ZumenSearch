using ZumenSearch.Application.Results;

namespace ZumenSearch.Application.Contracts;

public interface IRentResidentialListingRepository
{
    ResultWrapper Upsert(string propertyId, ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing);
    ListingSearchResultWrapper Search(ZumenSearch.Application.SearchResults.RentResidentialListingSearchCriteria criteria);
    RentResidentialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false);
    ResultWrapper Delete(string listingId);
}
