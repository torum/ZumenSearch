using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Application.Services.Rent.Residentials;

public sealed class RentResidentialListingService(IRentResidentialListingRepository repository)
{
    public ResultWrapper Save(string propertyId, Listing listing) => repository.Upsert(propertyId, listing);
    public ListingSearchResultWrapper Search(RentResidentialListingSearchCriteria criteria) => repository.Search(criteria);
    public RentResidentialListingResultWrapper GetById(string propertyId, string listingId, bool duplicate = false) => repository.SelectById(propertyId, listingId, duplicate);
    public ResultWrapper Delete(string listingId) => repository.Delete(listingId);
}
