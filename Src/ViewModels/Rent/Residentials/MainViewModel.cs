using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage.Search;
using Windows.System;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Views;
using ZumenSearch.Views.Rent.Residentials;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public partial class MainViewModel : ObservableObject
{
    #region == Private Variables ==

    private Views.Rent.Residentials.Bldg.BldgShellPage? _bldgShellPage;
    private Views.Rent.Residentials.Unit.UnitShellPage? _unitShellPage;

    #endregion

    #region == Public Properties ==

    public string Id => _id;

    //public Views.Rent.Residentials.EditorWindow Win { get; init; }

    //public Views.Rent.Residentials.ShellPage Shell { get; init; }

    public UnitViewModel Unit { get; init; }

    public BldgViewModel Bldg { get; init; }

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
                return field;
            }
            else
            {
                var str = $"{field} - {Bldg.Name}";

                if (!string.IsNullOrEmpty(Unit.RoomName))
                {
                    str =  $"{str}: {Unit.RoomName}";
                }

                if (Bldg.EntryStatus == EnumEntryStatus.New)
                {
                    str = $"{str} (新規)";
                }
                else
                {
                    str = $"{str} (更新)";
                }

                return str;
            }
            
        }
    } = "賃貸住居用";

    #endregion

    #region == Events

    // The event handlers below are used to notify the UI about various actions that can be performed in the editor.
    // EditorShell subscribes to these events to handle the actions accordingly.
    public event EventHandler? EventBackToSummary;
    public event EventHandler? EventEditLocation;
    public event EventHandler? EventEditTransportation;
    public event EventHandler? EventEditAppliance;
    public event EventHandler? EventEditPictures;
    public event EventHandler? EventEditUnits;

    // TODO:
    //public event EventHandler<bool>? EventIsUnitOwnership; // show or hides navigationview' menu accordingly.
    //public event EventHandler? EventTitleChanged;

    // Who subscribes to this event?
    public event EventHandler? EventGoBack;

    #endregion

    #region == Services ==

    // The IDataAccessService is used to access the data layer for saving and updating entries.
    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IModalDialogService _dlgService;
    private readonly IDispatcherService _dispatcherService;

    #endregion

    private string _id = string.Empty;
    private Views.Rent.Residentials.ShellPage? _shell;
    private Views.Rent.Residentials.EditorWindow? _win;

    public MainViewModel(Models.Rent.Residentials.EntryResidentialFull entry, IDispatcherService dispatcherService, IDataAccessService dataAccessService, IModalDialogService modalDialogService, IDataAccessLocationService dataAccessLocationService)
    {
        //Win = editorWindow;
        //Shell = shellPage;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;
        _dlgService = modalDialogService;
        _dispatcherService = dispatcherService;

        _id = entry.Id;//Guid.CreateVersion7().ToString("N");

        //Debug.WriteLine($"MainViewModel {entry.Id}");

        Unit = new UnitViewModel(this, dataAccessService);
        Bldg = new BldgViewModel(this, entry, dataAccessService, dataAccessLocationService);

        // Update title with dummy value.
        WindowTitle = string.Empty;
    }

    #region == Private Methods ==

    #endregion

    #region == Public Methods ==

    public void SetEditorWindow(Views.Rent.Residentials.EditorWindow win)
    {
        _win = win;

        _win.SetEntryIdToWindow(_id);
    }

    public void SetEditorShell(Views.Rent.Residentials.ShellPage shell)
    {
        _shell = shell;
    }

    public void SetUnitShell(Views.Rent.Residentials.Unit.UnitShellPage shell)
    {
        _unitShellPage = shell;
    }

    public void SetBldgShell(Views.Rent.Residentials.Bldg.BldgShellPage shell)
    {
        _bldgShellPage = shell;
    }

    #endregion

    #region == Commands ==

    #region == Navigation related commands == 

    [RelayCommand]
    private void GoToBldgShellPage()
    {
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
            // TODO:
            if (Unit.IsDirty)
            {
                Debug.WriteLine("TODO: GoToBldgShellPage: show warning : The current room has unsaved changes.");
                return;
            }
        }

        if (_shell.NavigationFrame.Navigate(typeof(Views.Rent.Residentials.Bldg.BldgShellPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight }))
        {

        }
    }

    [RelayCommand]
    private void GoToUnitShellPage()
    {
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
                Debug.WriteLine("TODO: GoToBldgShellPage: show warning : The current building has unsaved changes.");
                //return;// TODO: no need to block this?
            }
        }

        if (_shell.NavigationFrame.Navigate(typeof(Views.Rent.Residentials.Unit.UnitShellPage), this, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight }))
        {

        }
    }

    // Go Back command (don't use this?)
    [RelayCommand]
    private void GoBack()
    {
        EventGoBack?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void GoBackToSummary()
    { 
        EventBackToSummary?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void EditLocation()
    {
        EventEditLocation?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void EditTransportation()
    {
        EventEditTransportation?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void EditAppliance()
    {
        EventEditAppliance?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void EditPictures()
    {
        EventEditPictures?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void EditUnits()
    {
        EventEditUnits?.Invoke(this, EventArgs.Empty);
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
