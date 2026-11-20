using ZumenSearch.Application.Results;

namespace ZumenSearch.Application.Contracts;

public interface IRentResidentialPersonRepository
{
    PersonsSearchResultWrapper SelectLessorsByKeyword(string keyword);
    PersonResultWrapper SelectLessorById(string lessorId);
    ResultWrapper DeleteLessor(string lessorId);
    PersonsSearchResultWrapper SelectBrokersByKeyword(string keyword);
    PersonResultWrapper SelectBrokerById(string brokerId);
    ResultWrapper DeleteBroker(string brokerId);
}
