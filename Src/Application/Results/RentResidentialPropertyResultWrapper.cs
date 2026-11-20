using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Application.Results;

public sealed class RentResidentialPropertyResultWrapper : ResultWrapperBase
{
    public Property? Property { get; set; }
}
