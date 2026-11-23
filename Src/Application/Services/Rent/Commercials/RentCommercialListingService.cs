using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Application.Services.Rent.Commercials;

public sealed class RentCommercialListingService(IRentCommercialListingRepository repository)
{
    public ResultWrapper Save(string propertyId, Listing listing) => repository.Upsert(propertyId, listing);
    public ListingSearchResultWrapper Search(RentCommercialListingSearchCriteria criteria) => repository.Search(criteria);
    public RentCommercialListingResultWrapper GetById(string propertyId, string listingId, bool duplicate = false) => repository.SelectById(propertyId, listingId, duplicate);
    public ResultWrapper Delete(string listingId) => repository.Delete(listingId);
}
