using ZumenSearch.Application.Results;

namespace ZumenSearch.Application.Contracts;

public interface IPersonRepository
{
    PersonsSearchResultWrapper SelectRentLessorsByKeyword(string keyword);
    PersonResultWrapper SelectRentLessorById(string lessorId);
    ResultWrapper DeleteRentLessor(string lessorId);
    PersonsSearchResultWrapper SelectBrokersByKeyword(string keyword);
    PersonResultWrapper SelectBrokerById(string brokerId);
    ResultWrapper UpsertBroker(ZumenSearch.Domain.Entities.Base.PersonBase broker);
    ResultWrapper DeleteBroker(string brokerId);
}
