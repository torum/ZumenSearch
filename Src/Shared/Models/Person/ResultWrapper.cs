using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Person;

// Person
public sealed class ResultWrapper : ResultWrapperBase
{
    public PersonBase? Person { get; set; }
}
