using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ZumenSearch.Models;
using ZumenSearch.Services.Contracts;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Services;

/*
 * WinUI3/AvaloniaUI does not provide a way to get the restore bounds of a window like WPF does when it is maximized or minimized.
 * So, we need to manually track the restore bounds of the window when it is in the restored state.
 * AppWindow_Changed(WinUI3)/Window_PositionChanged(AvaloniaUI) and Window_SizeChanged event handlers are called when the window state changes, such as when the window is maximized, minimized, or restored.
 * Make sure to update the restore bounds when the window is in the restored state.
 * Also, use Window_Activated event handler to update the restore bounds when the window is activated, because sometimes the window state change events are not fired when the window is activated.
 * This way, even if the window is closed while maximized or minimized, we can still save the restore bounds to the configuration file.
    _winRestoreHeight = sender.Size.Height;
    _winRestoreWidth = sender.Size.Width;
    _winRestoreTop = sender.Position.Y;
    _winRestoreLeft = sender.Position.X;
 */

public sealed class ConfigurationService : IConfigurationService
{
    private bool _isInitialized;
    private bool _isConfigLoaded;
    private string? _configFilePath = string.Empty;
    private XDocument? _xdoc;
    private readonly IDispatcherService _dispatcherService;

    public ConfigurationService(IDispatcherService dispatcherService)
    {
        _dispatcherService = dispatcherService;

        _isConfigLoaded = false;
        CurrentConfiguration = new Models.Configuration();
    }

    public Models.Configuration CurrentConfiguration { get; private set; }
    public string ConfigFilePath => _configFilePath ?? string.Empty;

    public void Initialize(string configFilePath)
    {
        _configFilePath = configFilePath;
        _isConfigLoaded = false;
        CurrentConfiguration = new Models.Configuration();
        _xdoc = null;
        _isInitialized = true;

        var folderPath = Path.GetDirectoryName(configFilePath);
        if (folderPath is not null)
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        else
        {
            throw new InvalidOperationException("Invalid configuration file path.");
        }
    }

    public async Task<Models.Configuration> LoadConfigurationAsync()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("ConfigurationService is not initialized.");
        }

        var config = new Models.Configuration();

        if (_isConfigLoaded)
        {
            Debug.WriteLine("LoadConfigurationAsync() - Already loaded, returning current configuration.");
            return CurrentConfiguration ?? config;
        }

        if (string.IsNullOrEmpty(_configFilePath))
        {
            throw new InvalidOperationException("Configuration file path is not set.");
        }
        if (!System.IO.File.Exists(_configFilePath))
        {
            Debug.WriteLine($"Configuration file not found: {_configFilePath}");
            // Set _isConfigLoaded to true so that SaveConfigurationAsync() can create the file with default settings.
            _isConfigLoaded = true;
            return config;
        }

        Debug.WriteLine($"LoadConfigurationAsync() {_configFilePath}");

        try
        {
            _xdoc = XDocument.Load(_configFilePath);
            if (_xdoc.Root is null)
            {
                return config;
            }

            #region == Main Window setting ==
            
            // Main Window element
            var mainWindow = _xdoc.Root.Element("MainWindow");
            if (mainWindow is not null)
            {
                int wY = 24;
                int wX = 24;

                // Window sizes
                var hoge = mainWindow.Attribute("top");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.MainWindow.Top = wY;
                    }
                }

                hoge = mainWindow.Attribute("left");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.MainWindow.Left = wX;
                    }
                }

                hoge = mainWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.MainWindow.Height = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = mainWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.MainWindow.Width = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Restore window sizes
                hoge = mainWindow.Attribute("restoreTop");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.MainWindow.RestoreTop = wY;
                    }
                }

                hoge = mainWindow.Attribute("restoreLeft");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.MainWindow.RestoreLeft = wX;
                    }
                }

                hoge = mainWindow.Attribute("restoreHeight");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.MainWindow.RestoreHeight = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = mainWindow.Attribute("restoreWidth");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.MainWindow.RestoreWidth = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Window state
                hoge = mainWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        // TODO: Since there is no restorebounds in AvaloniaUI or WinUI3.....
                        config.MainWindow.WindowState = OverlappedPresenterState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        config.MainWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        // Ignore minimized state, and restore to normal state.
                        config.MainWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                }

                #region == Layout ==


                #endregion

            }

            #endregion

            #region == RentResidentialEditorWindow setting ==

            // Sub Window element
            var subWindow = _xdoc.Root.Element("RentResidentialEditorWindow");
            if (subWindow is not null)
            {
                int wY = 24;
                int wX = 24;

                // Window sizes
                var hoge = subWindow.Attribute("top");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.RentResidentialEditorWindow.Top = wY;
                    }
                }

                hoge = subWindow.Attribute("left");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.RentResidentialEditorWindow.Left = wX;
                    }
                }

                hoge = subWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentResidentialEditorWindow.Height = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentResidentialEditorWindow.Width = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Restore window sizes
                hoge = subWindow.Attribute("restoreTop");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.RentResidentialEditorWindow.RestoreTop = wY;
                    }
                }

                hoge = subWindow.Attribute("restoreLeft");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.RentResidentialEditorWindow.RestoreLeft = wX;
                    }
                }

                hoge = subWindow.Attribute("restoreHeight");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentResidentialEditorWindow.RestoreHeight = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("restoreWidth");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentResidentialEditorWindow.RestoreWidth = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Window state
                hoge = subWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        // TODO: Since there is no restorebounds in AvaloniaUI or WinUI3.....
                        config.RentResidentialEditorWindow.WindowState = OverlappedPresenterState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        config.RentResidentialEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        // Ignore minimized state, and restore to normal state.
                        config.RentResidentialEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                }

                #region == Layout ==


                #endregion

            }

            #endregion

            #region == RentResidentialListingEditorWindow setting ==

            // Sub Window element
            subWindow = _xdoc.Root.Element("RentResidentialListingEditorWindow");
            if (subWindow is not null)
            {
                int wY = 24;
                int wX = 24;

                // Window sizes
                var hoge = subWindow.Attribute("top");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.RentResidentialListingEditorWindow.Top = wY;
                    }
                }

                hoge = subWindow.Attribute("left");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.RentResidentialListingEditorWindow.Left = wX;
                    }
                }

                hoge = subWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentResidentialListingEditorWindow.Height = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentResidentialListingEditorWindow.Width = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Restore window sizes
                hoge = subWindow.Attribute("restoreTop");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.RentResidentialListingEditorWindow.RestoreTop = wY;
                    }
                }

                hoge = subWindow.Attribute("restoreLeft");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.RentResidentialListingEditorWindow.RestoreLeft = wX;
                    }
                }

                hoge = subWindow.Attribute("restoreHeight");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentResidentialListingEditorWindow.RestoreHeight = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("restoreWidth");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentResidentialListingEditorWindow.RestoreWidth = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Window state
                hoge = subWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        // TODO: Since there is no restorebounds in AvaloniaUI or WinUI3.....
                        config.RentResidentialListingEditorWindow.WindowState = OverlappedPresenterState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        config.RentResidentialListingEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        // Ignore minimized state, and restore to normal state.
                        config.RentResidentialListingEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                }

                #region == Layout ==


                #endregion

            }

            #endregion

            #region == RentCommercialEditorWindow setting ==

            // Sub Window element
            subWindow = _xdoc.Root.Element("RentCommercialEditorWindow");
            if (subWindow is not null)
            {
                int wY = 24;
                int wX = 24;

                // Window sizes
                var hoge = subWindow.Attribute("top");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.RentCommercialEditorWindow.Top = wY;
                    }
                }

                hoge = subWindow.Attribute("left");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.RentCommercialEditorWindow.Left = wX;
                    }
                }

                hoge = subWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentCommercialEditorWindow.Height = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentCommercialEditorWindow.Width = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Restore window sizes
                hoge = subWindow.Attribute("restoreTop");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.RentCommercialEditorWindow.RestoreTop = wY;
                    }
                }

                hoge = subWindow.Attribute("restoreLeft");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.RentCommercialEditorWindow.RestoreLeft = wX;
                    }
                }

                hoge = subWindow.Attribute("restoreHeight");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentCommercialEditorWindow.RestoreHeight = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("restoreWidth");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentCommercialEditorWindow.RestoreWidth = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Window state
                hoge = subWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        // TODO: Since there is no restorebounds in AvaloniaUI or WinUI3.....
                        config.RentCommercialEditorWindow.WindowState = OverlappedPresenterState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        config.RentCommercialEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        // Ignore minimized state, and restore to normal state.
                        config.RentCommercialEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                }

                #region == Layout ==


                #endregion

            }

            #endregion

            #region == RentCommercialListingEditorWindow setting ==

            // Sub Window element
            subWindow = _xdoc.Root.Element("RentCommercialListingEditorWindow");
            if (subWindow is not null)
            {
                int wY = 24;
                int wX = 24;

                // Window sizes
                var hoge = subWindow.Attribute("top");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.RentCommercialListingEditorWindow.Top = wY;
                    }
                }

                hoge = subWindow.Attribute("left");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.RentCommercialListingEditorWindow.Left = wX;
                    }
                }

                hoge = subWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentCommercialListingEditorWindow.Height = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentCommercialListingEditorWindow.Width = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Restore window sizes
                hoge = subWindow.Attribute("restoreTop");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.RentCommercialListingEditorWindow.RestoreTop = wY;
                    }
                }

                hoge = subWindow.Attribute("restoreLeft");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.RentCommercialListingEditorWindow.RestoreLeft = wX;
                    }
                }

                hoge = subWindow.Attribute("restoreHeight");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentCommercialListingEditorWindow.RestoreHeight = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("restoreWidth");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.RentCommercialListingEditorWindow.RestoreWidth = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Window state
                hoge = subWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        // TODO: Since there is no restorebounds in AvaloniaUI or WinUI3.....
                        config.RentCommercialListingEditorWindow.WindowState = OverlappedPresenterState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        config.RentCommercialListingEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        // Ignore minimized state, and restore to normal state.
                        config.RentCommercialListingEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                }

                #region == Layout ==


                #endregion

            }

            #endregion

            #region == LessorEditorWindow setting ==

            // Sub Window element
            subWindow = _xdoc.Root.Element("LessorEditorWindow");
            if (subWindow is not null)
            {
                int wY = 24;
                int wX = 24;

                // Window sizes
                var hoge = subWindow.Attribute("top");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.LessorEditorWindow.Top = wY;
                    }
                }

                hoge = subWindow.Attribute("left");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.LessorEditorWindow.Left = wX;
                    }
                }

                hoge = subWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.LessorEditorWindow.Height = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.LessorEditorWindow.Width = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Restore window sizes
                hoge = subWindow.Attribute("restoreTop");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.LessorEditorWindow.RestoreTop = wY;
                    }
                }

                hoge = subWindow.Attribute("restoreLeft");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.LessorEditorWindow.RestoreLeft = wX;
                    }
                }

                hoge = subWindow.Attribute("restoreHeight");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.LessorEditorWindow.RestoreHeight = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("restoreWidth");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.LessorEditorWindow.RestoreWidth = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Window state
                hoge = subWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        // TODO: Since there is no restorebounds in AvaloniaUI or WinUI3.....
                        config.LessorEditorWindow.WindowState = OverlappedPresenterState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        config.LessorEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        // Ignore minimized state, and restore to normal state.
                        config.LessorEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                }

                #region == Layout ==


                #endregion

            }

            #endregion

            #region == LessorEditorWindow setting ==

            // Sub Window element
            subWindow = _xdoc.Root.Element("BrokerEditorWindow");
            if (subWindow is not null)
            {
                int wY = 24;
                int wX = 24;

                // Window sizes
                var hoge = subWindow.Attribute("top");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.BrokerEditorWindow.Top = wY;
                    }
                }

                hoge = subWindow.Attribute("left");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.BrokerEditorWindow.Left = wX;
                    }
                }

                hoge = subWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.BrokerEditorWindow.Height = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.BrokerEditorWindow.Width = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Restore window sizes
                hoge = subWindow.Attribute("restoreTop");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.BrokerEditorWindow.RestoreTop = wY;
                    }
                }

                hoge = subWindow.Attribute("restoreLeft");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.BrokerEditorWindow.RestoreLeft = wX;
                    }
                }

                hoge = subWindow.Attribute("restoreHeight");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.BrokerEditorWindow.RestoreHeight = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = subWindow.Attribute("restoreWidth");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.BrokerEditorWindow.RestoreWidth = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Window state
                hoge = subWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        // TODO: Since there is no restorebounds in AvaloniaUI or WinUI3.....
                        config.BrokerEditorWindow.WindowState = OverlappedPresenterState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        config.BrokerEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        // Ignore minimized state, and restore to normal state.
                        config.BrokerEditorWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                }

                #region == Layout ==


                #endregion

            }

            #endregion

            #region == Theme ==

            var opts = _xdoc.Root.Element("Theme");
            if (opts != null)
            {
                // Old name. Leave it for backward compatibility.
                var xvalue = opts.Attribute("current");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (Enum.TryParse(xvalue.Value, out ElementTheme cacheTheme))
                        {
                            config.Theme = cacheTheme;
                            //isFoundNewThemeSetting = true;
                        }
                    }
                }
                // Old name. Leave it for backward compatibility.
                xvalue = opts.Attribute("backdrop");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (Enum.TryParse(xvalue.Value, out SystemBackdropOption cacheBackdrop))
                        {
                            config.SystemBackdrop = cacheBackdrop;
                            //isFoundNewThemeSetting = true;
                        }
                    }
                }
                // New name. 
                xvalue = opts.Attribute("ThemeName");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (Enum.TryParse(xvalue.Value, out ElementTheme cacheTheme))
                        {
                            config.Theme = cacheTheme;
                            //isFoundNewThemeSetting = true;
                        }
                    }
                }
                // New name. 
                xvalue = opts.Attribute("SystemBackdrop");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (Enum.TryParse(xvalue.Value, out SystemBackdropOption cacheBackdrop))
                        {
                            config.SystemBackdrop = cacheBackdrop;
                            //isFoundNewThemeSetting = true;
                        }
                    }
                }
            }

            #endregion

            #region == Options ==
            /*
            // TODO: Change to "Options" in the future, but keep "Opts" for backward compatibility.
            opts = _xdoc.Root.Element("Opts");
            if (opts is not null)
            {
                var hoge = opts.Attribute("isAutoScrollToNowPlaying");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsAutoScrollToNowPlaying = true;
                    }
                    else
                    {
                        config.IsAutoScrollToNowPlaying = false;
                    }
                }

                hoge = opts.Attribute("isArtistSortWithoutThePrefix");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsArtistSortWithoutThePrefix = true;
                    }
                    else
                    {
                        config.IsArtistSortWithoutThePrefix = false;
                    }
                }

                hoge = opts.Attribute("isAlbumSortWithoutThePrefix");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsAlbumSortWithoutThePrefix = true;
                    }
                    else
                    {
                        config.IsAlbumSortWithoutThePrefix = false;
                    }
                }

                hoge = opts.Attribute("isUpdateOnStartup");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsUpdateOnStartup = true;
                    }
                    else
                    {
                        config.IsUpdateOnStartup = false;
                    }
                }

                hoge = opts.Attribute("isDownloadAlbumArtEmbeddedUsingReadPicture");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsDownloadAlbumArtEmbeddedUsingReadPicture = true;

                    }
                    else
                    {
                        config.IsDownloadAlbumArtEmbeddedUsingReadPicture = false;
                    }
                }

                hoge = opts.Attribute("isForceSetVolumeExplicitly");
                if (hoge is not null)
                {
                    if (hoge.Value == "True")
                    {
                        config.IsForceSetVolumeExplicitly = true;

                    }
                    else
                    {
                        config.IsForceSetVolumeExplicitly = false;
                    }
                }
            }
            */
            #endregion

            _isConfigLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading configuration: {ex}");
            _dispatcherService.TryEnqueue(() => { (App.Current as App)?.AppendErrorLog("Error loading configuration", ex.Message); });
        }

        CurrentConfiguration = config;

        return CurrentConfiguration;
    }

    public async Task SaveConfigurationAsync()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("ConfigurationService is not initialized.");
        }

        // Make sure Window and settings have been fully loaded and not overriding with empty data.
        if (!_isConfigLoaded)
        {
            // Should be fine now. -> Don't return, because we want to save the configuration even if it hasn't been loaded yet, like the first time.
            //return;
        }
        if (string.IsNullOrEmpty(_configFilePath))
        {
            throw new InvalidOperationException("Configuration file path is not set.");
        }

        Debug.WriteLine("SaveConfigurationAsync()");

        // TODO: Load xdoc from global variable if exists, and apply changes to it, then save it back to file.
        //_xdoc

        var config = CurrentConfiguration;

        XmlDocument doc = new();
        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

        // Root Document Element
        XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
        doc.AppendChild(root);

        XmlAttribute attrs;

        #region == Main Window settings ==

        var w = config.MainWindow;
        //double windowWidth = 780;

        // Main Window element
        XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

        // Main Window attributes

        // Window sizes
        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Width.ToString(CultureInfo.InvariantCulture) : w.RestoreWidth.ToString(CultureInfo.InvariantCulture);

        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        // Restore window sizes
        attrs = doc.CreateAttribute("restoreHeight");
        attrs.Value = w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreWidth");
        attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreTop");
        attrs.Value = w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreLeft");
        attrs.Value = w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("state");
        attrs.Value = w.WindowState switch
        {
            OverlappedPresenterState.Maximized => "Maximized",
            OverlappedPresenterState.Restored => "Normal",
            OverlappedPresenterState.Minimized => "Minimized",
            _ => attrs.Value
        };
        mainWindow.SetAttributeNode(attrs);

        #region == Layout ==


        #endregion

        // set MainWindow element to root.
        root.AppendChild(mainWindow);

        #endregion

        #region == RentResidentialEditorWindow settings ==

        w = config.RentResidentialEditorWindow;
        // Window element
        XmlElement subWindow = doc.CreateElement(string.Empty, "RentResidentialEditorWindow", string.Empty);

        // Window attributes

        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Width.ToString(CultureInfo.InvariantCulture) : w.RestoreWidth.ToString(CultureInfo.InvariantCulture);

        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        // Restore window sizes
        attrs = doc.CreateAttribute("restoreHeight");
        attrs.Value = w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreWidth");
        attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreTop");
        attrs.Value = w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreLeft");
        attrs.Value = w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("state");
        attrs.Value = w.WindowState switch
        {
            OverlappedPresenterState.Maximized => "Maximized",
            OverlappedPresenterState.Restored => "Normal",
            OverlappedPresenterState.Minimized => "Minimized",
            _ => attrs.Value
        };
        subWindow.SetAttributeNode(attrs);

        #region == Layout ==


        #endregion

        // set subWindow element to root.
        root.AppendChild(subWindow);

        #endregion

        #region == RentResidentialListingEditorWindow settings ==

        w = config.RentResidentialListingEditorWindow;
        // Window element
        subWindow = doc.CreateElement(string.Empty, "RentResidentialListingEditorWindow", string.Empty);

        // Window attributes

        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Width.ToString(CultureInfo.InvariantCulture) : w.RestoreWidth.ToString(CultureInfo.InvariantCulture);

        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        // Restore window sizes
        attrs = doc.CreateAttribute("restoreHeight");
        attrs.Value = w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreWidth");
        attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreTop");
        attrs.Value = w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreLeft");
        attrs.Value = w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("state");
        attrs.Value = w.WindowState switch
        {
            OverlappedPresenterState.Maximized => "Maximized",
            OverlappedPresenterState.Restored => "Normal",
            OverlappedPresenterState.Minimized => "Minimized",
            _ => attrs.Value
        };
        subWindow.SetAttributeNode(attrs);

        #region == Layout ==


        #endregion

        // set subWindow element to root.
        root.AppendChild(subWindow);

        #endregion

        #region == RentCommercialEditorWindow settings ==

        w = config.RentCommercialEditorWindow;
        // Window element
        subWindow = doc.CreateElement(string.Empty, "RentCommercialEditorWindow", string.Empty);

        // Window attributes

        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Width.ToString(CultureInfo.InvariantCulture) : w.RestoreWidth.ToString(CultureInfo.InvariantCulture);

        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        // Restore window sizes
        attrs = doc.CreateAttribute("restoreHeight");
        attrs.Value = w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreWidth");
        attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreTop");
        attrs.Value = w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreLeft");
        attrs.Value = w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("state");
        attrs.Value = w.WindowState switch
        {
            OverlappedPresenterState.Maximized => "Maximized",
            OverlappedPresenterState.Restored => "Normal",
            OverlappedPresenterState.Minimized => "Minimized",
            _ => attrs.Value
        };
        subWindow.SetAttributeNode(attrs);

        #region == Layout ==


        #endregion

        // set subWindow element to root.
        root.AppendChild(subWindow);

        #endregion

        #region == RentCommercialListingEditorWindow settings ==

        w = config.RentCommercialListingEditorWindow;
        // Window element
        subWindow = doc.CreateElement(string.Empty, "RentCommercialListingEditorWindow", string.Empty);

        // Window attributes

        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Width.ToString(CultureInfo.InvariantCulture) : w.RestoreWidth.ToString(CultureInfo.InvariantCulture);

        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        // Restore window sizes
        attrs = doc.CreateAttribute("restoreHeight");
        attrs.Value = w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreWidth");
        attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreTop");
        attrs.Value = w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreLeft");
        attrs.Value = w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("state");
        attrs.Value = w.WindowState switch
        {
            OverlappedPresenterState.Maximized => "Maximized",
            OverlappedPresenterState.Restored => "Normal",
            OverlappedPresenterState.Minimized => "Minimized",
            _ => attrs.Value
        };
        subWindow.SetAttributeNode(attrs);

        #region == Layout ==


        #endregion

        // set subWindow element to root.
        root.AppendChild(subWindow);

        #endregion

        #region == LessorEditorWindow settings ==

        w = config.LessorEditorWindow;
        // Window element
        subWindow = doc.CreateElement(string.Empty, "LessorEditorWindow", string.Empty);

        // Window attributes

        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Width.ToString(CultureInfo.InvariantCulture) : w.RestoreWidth.ToString(CultureInfo.InvariantCulture);

        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        // Restore window sizes
        attrs = doc.CreateAttribute("restoreHeight");
        attrs.Value = w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreWidth");
        attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreTop");
        attrs.Value = w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreLeft");
        attrs.Value = w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("state");
        attrs.Value = w.WindowState switch
        {
            OverlappedPresenterState.Maximized => "Maximized",
            OverlappedPresenterState.Restored => "Normal",
            OverlappedPresenterState.Minimized => "Minimized",
            _ => attrs.Value
        };
        subWindow.SetAttributeNode(attrs);

        #region == Layout ==


        #endregion

        // set subWindow element to root.
        root.AppendChild(subWindow);

        #endregion

        #region == BrokerEditorWindow settings ==

        w = config.BrokerEditorWindow;
        // Window element
        subWindow = doc.CreateElement(string.Empty, "BrokerEditorWindow", string.Empty);

        // Window attributes

        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Width.ToString(CultureInfo.InvariantCulture) : w.RestoreWidth.ToString(CultureInfo.InvariantCulture);

        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        // Restore window sizes
        attrs = doc.CreateAttribute("restoreHeight");
        attrs.Value = w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreWidth");
        attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreTop");
        attrs.Value = w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreLeft");
        attrs.Value = w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        subWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("state");
        attrs.Value = w.WindowState switch
        {
            OverlappedPresenterState.Maximized => "Maximized",
            OverlappedPresenterState.Restored => "Normal",
            OverlappedPresenterState.Minimized => "Minimized",
            _ => attrs.Value
        };
        subWindow.SetAttributeNode(attrs);

        #region == Layout ==


        #endregion

        // set subWindow element to root.
        root.AppendChild(subWindow);

        #endregion

        #region == Theme ==

        XmlElement thm = doc.CreateElement(string.Empty, "Theme", string.Empty);

        attrs = doc.CreateAttribute("ThemeName");
        attrs.Value = config.Theme.ToString();
        thm.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("SystemBackdrop");
        attrs.Value = config.SystemBackdrop.ToString();
        thm.SetAttributeNode(attrs);

        /// 
        root.AppendChild(thm);

        #endregion

        #region == Options ==

        #endregion

        try
        {
            // Now this is redundant, because we already create the directory in Initialize() method.
            if (!Directory.Exists(App.AppDataFolder))
            {
                Directory.CreateDirectory(App.AppDataFolder);
            }

            //doc.Save(App.AppConfigFilePath);
            var temporaryPath = _configFilePath + ".tmp";
            doc.Save(temporaryPath);
            File.Move(temporaryPath, _configFilePath, overwrite: true);
        }
        //catch (System.IO.FileNotFoundException) { }
        catch (Exception ex)
        {
            _dispatcherService.TryEnqueue(() =>
            {
                (App.Current as App)?.AppendErrorLog("Exception@SaveConfigurationAsync", ex.ToString());
            });
        }

    }

}
