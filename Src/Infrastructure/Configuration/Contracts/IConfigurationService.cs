using System.Threading.Tasks;

namespace ZumenSearch.Infrastructure.Configuration.Contracts;

public interface IConfigurationService
{
    void Initialize(string configFilePath);
    Task<ZumenSearch.Application.Configuration.AppSettings> LoadConfigurationAsync();
    Task SaveConfigurationAsync();
    ZumenSearch.Application.Configuration.AppSettings CurrentConfiguration { get; }
    string ConfigFilePath { get; }
}