using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services;
using ZumenSearch.Views;
using static ZumenSearch.Models.Rent.Residentials.Enums;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

// 賃貸住居用物件の「種別」クラス
public class Kind(string key, string label)
{
    public string Label { get; set; } = label;
    public string Key { get; set; } = key;
};

public class Structure(string key, string label)
{
    public string Label { get; set; } = label;
    public string Key { get; set; } = key;
};

// 賃貸住居用物件の編集用ViewModel本体クラス
public partial class EditorViewModel : ObservableObject, INotifyPropertyChanged
{
    // Flag that indicates if the entry is dirty (i.e., has unsaved changes).
    private bool _isEntryDirty;
    public bool IsEntryDirty
    {
        get => _isEntryDirty;
        set
        {
            if (SetProperty(ref _isEntryDirty, value))
            {
                //
            }
        }
    }

    // The Name property holds the name of the entry being edited.
    private string? _name;
    public string Name
    {
        get => _name ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref _name, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    // For the use of the ComboBox in the UI, we define a collection of kinds.
    public ObservableCollection<Kind> Kinds =
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new Kind(EnumKinds.Apartment.ToString(), "アパート"),
        new Kind(EnumKinds.Mansion.ToString(), "マンション"),
        new Kind(EnumKinds.House.ToString(), "一戸建て"),
        new Kind(EnumKinds.TerraceHouse.ToString(), "テラスハウス"),
        new Kind(EnumKinds.TownHouse.ToString(), "タウンハウス"),
        new Kind(EnumKinds.ShareHouse.ToString(), "シェアハウス"),
        new Kind(EnumKinds.Dormitory.ToString(), "寮・下宿")
    ];

    // The SelectedKind property holds the currently selected kind from the ComboBox in the UI.
    private Kind _selectedKind = new(EnumKinds.Unspecified.ToString(), "未指定");
    public Kind SelectedKind
    {
        get => _selectedKind;
        set
        {
            if (SetProperty(ref _selectedKind, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    // The IsUnitOwnership property distinguish the building ownership type.
    private bool _isUnitOwnership;
    public bool IsUnitOwnership
    {
        get => _isUnitOwnership;
        set
        {
            if (SetProperty(ref _isUnitOwnership, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    private Structure _selectedStructure = new(EnumStructure.Unspecified.ToString(), "未指定");
    public Structure SelectedStructure
    {
        get => _selectedStructure;
        set
        {
            if (SetProperty(ref _selectedStructure, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    public ObservableCollection<Structure> Structures =
    [
        //new Structure(EnumStructure.Unspecified.ToString(), "未指定"),
        new Structure(EnumStructure.Wood.ToString(), "木造"),
        new Structure(EnumStructure.Block.ToString(), "ブロック造"),
        new Structure(EnumStructure.LightSteel.ToString(), "軽量鉄骨造"),
        new Structure(EnumStructure.Steel.ToString(), "鉄骨造"),
        new Structure(EnumStructure.RC.ToString(), "鉄筋コンクリート(RC)造"),
        new Structure(EnumStructure.SRC.ToString(), "鉄骨鉄筋コンクリート(SRC)造"),
        new Structure(EnumStructure.ALC.ToString(), "軽量気泡コンクリート(ALC)造"),
        new Structure(EnumStructure.PC.ToString(), "プレキャストコンクリート(PC)造"),
        new Structure(EnumStructure.HPC.ToString(), "鉄骨プレキャストコンクリート(HPC)造"),
        new Structure(EnumStructure.RB.ToString(), "鉄筋ブロック造"),
        new Structure(EnumStructure.CFT.ToString(), "コンクリート充填鋼管(CFT)造"),
        new Structure(EnumStructure.Other.ToString(), "その他")
    ];












    // The Entry property holds the COPY of current RentResidential entry being edited. Do not use it directly in the UI.
    private Models.Rent.Residentials.RentResidential _entry = new();
    public Models.Rent.Residentials.RentResidential Entry
    {
        get => _entry;
        set
        {
            if (value != null)
            {
                _entry = value;

                if (EditorWin != null)
                {
                    // If the EditorWin is not null, set the Id property of the window to the Entry.Id.
                    EditorWin.Id = _entry.Id;
                }
                else
                {
                    // TODO: If EditorWin is null, log an error or handle it accordingly.
                    Debug.WriteLine("EditorWin is null. Cannot set Id.");
                }

                Name = _entry.Name;
                //TODO: Set other properties

                IsEntryDirty = false;
            }
        }
    }

    // The EditorWin property holds the reference to the EditorWindow instance that is currently being used for editing.
    public Views.Rent.Residentials.Editor.EditorWindow? EditorWin
    {
        get; set;
    }

    // The event handlers below are used to notify the UI about various actions that can be performed in the editor.
    //
    public event EventHandler<string>? EventAddNew;
    //
    public event EventHandler<string>? EventGoBack;
    public event EventHandler<string>? EventBackToSummary;
    //
    public event EventHandler<string>? EventEditStructure;
    public event EventHandler<string>? EventEditLocation;
    public event EventHandler<string>? EventEditTransportation;
    public event EventHandler<string>? EventEditAppliance;

    // The IDataAccessService is used to access the data layer for saving and updating entries.
    private readonly IDataAccessService _dataAccessService;

    // Constructor for the EditorViewModel class, initializes the data access service and the entry.
    public EditorViewModel(IDataAccessService dataAccessService)
    {
        _dataAccessService = dataAccessService;

    }

    private RelayCommand? saveCommand;

    public IRelayCommand SaveCommand => saveCommand ??= new RelayCommand(Save);

    private async void Save()
    {
        if (IsEntryDirty)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Debug.WriteLine("TODO: Name is null or empty. This field is required. Show alart and abort.");
            }

            Entry.Name = Name;
            //TODO: Set other properties for Entry.

            if (string.IsNullOrEmpty(Entry.Id))
            {
                // If the Entry.Id is empty, generate a new ID and save as new.
                Entry.SetId = Guid.NewGuid().ToString();
                await SaveAsNewAsync();
            }
            else
            {
                // If the Entry.Id is not empty, update the existing entry.
                await SaveAsUpdate();
            }
        }
    }

    private Task SaveAsNewAsync()
    {
        var resInsert = _dataAccessService.InsertRentResidential(Entry.Id, Entry.Name, "some comment");
        if (resInsert.IsError)
        {
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // TODO: Show error message to user
            });
        }
        else
        {
            IsEntryDirty = false;
            Entry.IsDirty = false;
            Debug.WriteLine("No errors on insert.");
        }

        // Return a completed task to satisfy the method's return type
        return Task.CompletedTask;
    }

    private Task SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(Entry.Id, Entry.Name, "some update comment");
        if (resInsert.IsError)
        {
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // TODO: Show error message to user
            });
        }
        else
        {
            IsEntryDirty = false;
            Entry.IsDirty = false;
            Debug.WriteLine("No errors on update.");

            /*
            //UpdateSource()
            if (_viewModel.RentResidentialSearchResult.Contains(Entry))
            {
                // If the Entry is already in the search result, update it in the list.
                var index = _viewModel.RentResidentialSearchResult.IndexOf(Entry);
                if (index >= 0)
                {
                    _viewModel.RentResidentialSearchResult[index] = Entry;
                }
            }
            */
        }

        // Return a completed task to satisfy the method's return type
        return Task.CompletedTask;
    }

    // Add New Modal window command
    private RelayCommand? addNewCommand;

    public IRelayCommand AddNewCommand => addNewCommand ??= new RelayCommand(AddNew);

    private void AddNew()
    {
        //NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");
        EventAddNew?.Invoke(this, "asdf");
    }

    // Go Back command (don't use this?)
    private RelayCommand? goBackCommand;

    public IRelayCommand BackCommand => goBackCommand ??= new RelayCommand(GoBack);

    private void GoBack()
    {
        EventGoBack?.Invoke(this, "asdf");
    }

    private RelayCommand? backToSummaryCommand;

    public IRelayCommand BackToSummaryCommand => backToSummaryCommand ??= new RelayCommand(GoBackToSummary);

    private void GoBackToSummary()
    {
        EventBackToSummary?.Invoke(this, "asdf");
    }

    private RelayCommand? editStructureCommand;
    public IRelayCommand EditStructureCommand => editStructureCommand ??= new RelayCommand(EditStructure);

    private void EditStructure()
    {
        EventEditStructure?.Invoke(this, "asdf");
    }

    private RelayCommand? editLocationCommand;
    public IRelayCommand EditLocationCommand => editLocationCommand ??= new RelayCommand(EditLocation);

    private void EditLocation()
    {
        EventEditLocation?.Invoke(this, "asdf");
    }

    private RelayCommand? editTransportationCommand;
    public IRelayCommand EditTransportationCommand => editTransportationCommand ??= new RelayCommand(EditTransportation);

    private void EditTransportation()
    {
        EventEditTransportation?.Invoke(this, "asdf");
    }

    private RelayCommand? editApplianceCommand;
    public IRelayCommand EditApplianceCommand => editApplianceCommand ??= new RelayCommand(EditAppliance);

    private void EditAppliance()
    {
        EventEditAppliance?.Invoke(this, "asdf");
    }
}
