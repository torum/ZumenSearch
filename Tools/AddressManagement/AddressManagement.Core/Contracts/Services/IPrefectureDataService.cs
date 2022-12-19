using AddressManagement.Core.Models;

namespace AddressManagement.Core.Contracts.Services;

public interface IPrefectureDataService
{
    Task<IEnumerable<PrefectureCode>> GetPrefectureDataAsync();

}
