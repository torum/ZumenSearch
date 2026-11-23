using ZumenSearch.Application.Results;

namespace ZumenSearch.Application.Contracts;

public interface IRentCommercialListingRepository
{
    ResultWrapper Upsert(string propertyId, ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing);
    ListingSearchResultWrapper Search(ZumenSearch.Application.SearchResults.RentCommercialListingSearchCriteria criteria);
    RentCommercialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false);
    ResultWrapper Delete(string listingId);
}
