using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Application.Results;

public sealed class RentCommercialListingResultWrapper : ResultWrapperBase
{
    public string PropertyName { get; set; } = string.Empty;
    public Listing? Listing { get; set; }
}
