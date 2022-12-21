using AddressManagement.Core.Models;

namespace AddressManagement.Core.Contracts.Services;


public interface IPostalCodeDataService
{
    Task<IEnumerable<PostalCode>> GetPostalCodeDataAsync(string postalCode);

}