using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Domain.Entities.Rent.Commercials;

namespace ZumenSearch.Application.Services.Rent.Commercials;

public sealed class RentCommercialPropertyService(IRentCommercialPropertyRepository repository)
{
    public ResultWrapper Save(Property commercialProperty) => repository.Upsert(commercialProperty);
    public RentCommercialPropertyResultWrapper GetById(string propertyId, bool duplicate = false) => repository.SelectById(propertyId, duplicate);
    public ResultWrapper Delete(string propertyId) => repository.Delete(propertyId);
}
