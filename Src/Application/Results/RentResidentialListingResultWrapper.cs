using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Application.Results;

public sealed class RentResidentialListingResultWrapper : ResultWrapperBase
{
    public string PropertyName { get; set; } = string.Empty;
    public Listing? Listing { get; set; }
}
