using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials;

internal sealed partial class MainViewModel : ObservableObject
{
    #region == Public Properties ==

    public string Id => _id;

    public UnitViewModel Unit { get; init; }

    public BldgViewModel Bldg { get; init; }

    public INavigationResidentialService? ResidentialNavigationService => _navService;

    public IModalDialogService? ResidentialDialogService => _dlgService;

    // Local directory path to save blob data such as pictures and PDFs.
    public string EntryDataDirectoryPath { get; init; }

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "建物", Page = typeof(Views.Rent.Residentials.Bldg.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Bldg.BasicPage).FullName! }
    ];

    public ObservableCollection<Breadcrumb> BreadcrumbItemsUnit { get; set; } =
    [
        new() { Name = "建物", Page = typeof(Views.Rent.Residentials.Bldg.BasicPage).FullName! },
        new() { Name = "部屋", Page = typeof(Views.Rent.Residentials.Unit.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Unit.BasicPage).FullName! }
    ];

    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    // TODO: split into parts so that xaml can use span to colorlize them.
    public string WindowTitle
    {
        set
        {
            OnPropertyChanged();
        }
        get
        {
            if (string.IsNullOrEmpty(Bldg.Name))
            {
                return $"{field} (新規)";
            }
            else
            {
                var str = $"{field} - {Bldg.Name}";

                if (!string.IsNullOrEmpty(Unit.Name))
                {
                    str = $"{str}: {Unit.Name}";
                }

                if (Bldg.EntryStatus == EnumEntryStatus.New)
                {
                    str = $"{str} (新規)";
                }
                else
                {
                    str = $"{str} (編集)";
                }

                return str;
            }

        }
    } = "賃貸住居用";


    #endregion

    #region == Events

    // The event handlers below are used to notify the UI about various actions that can be performed in the editor.
    // EditorShell subscribes to these events to handle the actions accordingly.
    /*
    public event EventHandler? EventBackToSummary;
    public event EventHandler? EventEditLocation;
    public event EventHandler? EventEditTransportation;
    public event EventHandler? EventEditAppliance;
    public event EventHandler? EventEditPictures;
    public event EventHandler? EventEditUnits;
    */

    // TODO:
    //public event EventHandler? EventTitleChanged;

    // Who subscribes to this event?
    //public event EventHandler? EventGoBack;

    #endregion

    #region == Services ==

    // The IDataAccessService is used to access the data layer for saving and updating entries.
    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private IModalDialogService? _dlgService;
    private readonly IDispatcherService _dispatcherService;
    private INavigationResidentialService? _navService;

    #endregion

    #region == Private Variables ==

    private readonly string _id = string.Empty;

    #endregion

    public MainViewModel(Models.Rent.Residentials.EntryResidential entry, IDispatcherService dispatcherService, IDataAccessService dataAccessService, IDataAccessLocationService dataAccessLocationService)
    {
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;
        _dispatcherService = dispatcherService;

        _id = entry.Id;//Guid.CreateVersion7().ToString("N");
        EntryDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.AppDataPictureFolder, "Rent"), "Residential_Building"), _id);

        Unit = new UnitViewModel(this, dataAccessService);
        Bldg = new BldgViewModel(this, entry, dataAccessService, dataAccessLocationService);

        // Update title with dummy value.
        WindowTitle = string.Empty;
    }

    #region == Private Methods ==

    #endregion

    #region == Public Methods ==

    public void SetEditorShell(Views.Rent.Residentials.ShellPage shell)
    {
        //_shell = shell;
    }

    public void SetEditorNavigationService(INavigationResidentialService nav)
    {
        _navService = nav;
    }

    public void SetEditorDialogService(IModalDialogService dialog)
    {
        _dlgService = dialog;
    }

    public void SetUnitShell(Views.Rent.Residentials.Unit.UnitShellPage shell)
    {
        //_unitShellPage = shell;
    }

    public void SetBldgShell(Views.Rent.Residentials.Bldg.BldgShellPage shell)
    {
        //_bldgShellPage = shell;
    }

    #endregion

    #region == Commands ==

    #region == Navigation related commands == 

    [RelayCommand]
    private void GoToBldgShellPage()
    {
        var frame = _navService?.GetFrame();
        if (frame is null)
        {
            Debug.WriteLine("Nav service is not set. Cannot navigate to Building Shell Page.");

            return;
        }

        if (frame.CurrentSourcePageType == typeof(Views.Rent.Residentials.Bldg.BldgShellPage))
        {
            return;
        }

        if (frame.CurrentSourcePageType == typeof(Views.Rent.Residentials.Unit.UnitShellPage))
        {
            if (Unit.IsDirty)
            {
                //UnitShellPage's OnNavigatingFrom takes care of this.
                //Debug.WriteLine("GoToBldgShellPage: show warning : The current room has unsaved changes.");
                //return;
            }
        }

        if (frame.Navigate(typeof(Views.Rent.Residentials.Bldg.BldgShellPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight })) // //
        {

        }

        /*
        if (_shell is null)
        {
            Debug.WriteLine("ShellPage is not set. Cannot navigate to Building Shell Page.");
            return;
        }

        if (_shell.NavigationFrame.CurrentSourcePageType == typeof(Views.Rent.Residentials.Bldg.BldgShellPage))
        {
            return;
        }

        if (_shell.NavigationFrame.CurrentSourcePageType == typeof(Views.Rent.Residentials.Unit.UnitShellPage))
        {
            if (Unit.IsDirty)
            {
                //UnitShellPage's OnNavigatingFrom takes care of this.
                //Debug.WriteLine("GoToBldgShellPage: show warning : The current room has unsaved changes.");
                //return;
            }
        }

        if (_shell.NavigationFrame.Navigate(typeof(Views.Rent.Residentials.Bldg.BldgShellPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight })) // //
        {

        }
        */
    }

    [RelayCommand]
    private void GoToUnitShellPage()
    {
        var frame = _navService?.GetFrame();
        if (frame is null)
        {
            Debug.WriteLine("Nav service is not set. Cannot navigate to Building Shell Page.");
            return;
        }

        if (frame.CurrentSourcePageType == typeof(Views.Rent.Residentials.Unit.UnitShellPage))
        {
            return;
        }

        if (frame.CurrentSourcePageType == typeof(Views.Rent.Residentials.Bldg.BldgShellPage))
        {
            // TODO:
            if (Bldg.IsDirty)
            {
                Debug.WriteLine("TODO: GoToBldgShellPage: show warning? : The current building has unsaved changes.");
                //return;// TODO: no need to block this?
            }
        }

        if (frame.Navigate(typeof(Views.Rent.Residentials.Unit.UnitShellPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft })) //
        {


        }
        /*
        if (_shell is null)
        {
            Debug.WriteLine("ShellPage is not set. Cannot navigate to Building Shell Page.");
            return;
        }

        if (_shell.NavigationFrame.CurrentSourcePageType == typeof(Views.Rent.Residentials.Unit.UnitShellPage))
        {
            return;
        }

        if (_shell.NavigationFrame.CurrentSourcePageType == typeof(Views.Rent.Residentials.Bldg.BldgShellPage))
        {
            // TODO:
            if (Bldg.IsDirty)
            {
                Debug.WriteLine("TODO: GoToBldgShellPage: show warning? : The current building has unsaved changes.");
                //return;// TODO: no need to block this?
            }
        }

        if (_shell.NavigationFrame.Navigate(typeof(Views.Rent.Residentials.Unit.UnitShellPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft })) //
        {

        }
        */
    }



    #endregion

    #endregion

    #region == TEMP code for modal window(for setting an owner) ==

#pragma warning disable IDE0079
#pragma warning disable SYSLIB1054

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]

    internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    internal const int GWL_HWNDPARENT = (-8);

    internal static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 4)
        {
            return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }
        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    }

    // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
    internal static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
    internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079

    #endregion
}
