using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Rent.Commercials;

namespace ZumenSearch.Application.Contracts;

public interface IRentCommercialPropertyRepository
{
    ResultWrapper Upsert(Property commercialProperty);
    RentCommercialPropertyResultWrapper SelectById(string propertyId, bool duplicate = false);
    ResultWrapper Delete(string propertyId);
}
