using ZumenSearch.Domain.Entities.Rent.Commercials;

namespace ZumenSearch.Application.Results;

public sealed class RentCommercialPropertyResultWrapper : ResultWrapperBase
{
    public Property? Property { get; set; }
}
