using ZumenSearch.Domain.Entities.Base;
namespace ZumenSearch.Application.Results;

// Person
public sealed class PersonResultWrapper : ResultWrapperBase
{
    public PersonBase? Person { get; set; }
}
