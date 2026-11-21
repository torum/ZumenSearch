using System.Threading.Tasks;

namespace ZumenSearch.Infrastructure.Configuration.Contracts;

public interface IConfigurationService
{
    void Initialize(string configFilePath);
    Task<ZumenSearch.Application.Configuration.AppSettings> LoadAppSettingsAsync();
    Task SaveConfigurationAsync();
    ZumenSearch.Application.Configuration.AppSettings CurrentAppSettings { get; }
    string ConfigFilePath { get; }
}
