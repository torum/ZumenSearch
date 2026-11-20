using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Application.Services.Rent.Residentials;

public sealed class RentResidentialPropertyService(IRentResidentialPropertyRepository repository)
{
    public ResultWrapper Save(Property residentialProperty) => repository.Upsert(residentialProperty);
    public RentResidentialPropertyResultWrapper GetById(string propertyId, bool duplicate = false) => repository.SelectById(propertyId, duplicate);
    public ResultWrapper Delete(string propertyId) => repository.Delete(propertyId);
}
