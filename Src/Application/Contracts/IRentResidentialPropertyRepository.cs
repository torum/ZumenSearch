using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Application.Contracts;

public interface IRentResidentialPropertyRepository
{
    ResultWrapper Upsert(Property residentialProperty);
    RentResidentialPropertyResultWrapper SelectById(string propertyId, bool duplicate = false);
    ResultWrapper Delete(string propertyId);
}
