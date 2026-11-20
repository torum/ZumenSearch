using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;

namespace ZumenSearch.Application.Services.Rent.Residentials;

public sealed class RentPersonService(IRentResidentialPersonRepository repository)
{
    public PersonsSearchResultWrapper SearchLessors(string keyword) => repository.SelectLessorsByKeyword(keyword);
    public PersonResultWrapper GetLessorById(string lessorId) => repository.SelectLessorById(lessorId);
    public ResultWrapper DeleteLessor(string lessorId) => repository.DeleteLessor(lessorId);
    public PersonsSearchResultWrapper SearchBrokers(string keyword) => repository.SelectBrokersByKeyword(keyword);
    public PersonResultWrapper GetBrokerById(string brokerId) => repository.SelectBrokerById(brokerId);
    public ResultWrapper DeleteBroker(string brokerId) => repository.DeleteBroker(brokerId);
}
