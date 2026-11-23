using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;

namespace ZumenSearch.Application.Services.Persons;

public sealed class PersonService(IPersonRepository repository)
{
    public PersonsSearchResultWrapper SearchRentLessors(string keyword) => repository.SelectRentLessorsByKeyword(keyword);
    public PersonResultWrapper GetRentLessorById(string lessorId) => repository.SelectRentLessorById(lessorId);
    public ResultWrapper DeleteRentLessor(string lessorId) => repository.DeleteRentLessor(lessorId);
    public PersonsSearchResultWrapper SearchBrokers(string keyword) => repository.SelectBrokersByKeyword(keyword);
    public PersonResultWrapper GetBrokerById(string brokerId) => repository.SelectBrokerById(brokerId);
    public ResultWrapper UpsertBroker(ZumenSearch.Domain.Entities.Base.PersonBase broker) => repository.UpsertBroker(broker);
    public ResultWrapper DeleteBroker(string brokerId) => repository.DeleteBroker(brokerId);
}
