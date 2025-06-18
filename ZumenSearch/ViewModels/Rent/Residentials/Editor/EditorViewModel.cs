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
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services;
using ZumenSearch.Views;
using ZumenSearch.Views.Rent.Residentials.Editor;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

public partial class EditorViewModel : ObservableObject
{
    #region == Properties ==

    // The Entry property holds the COPY of current RentResidential entry being edited. Do not use it directly in the UI. Apply changes to this object in SaveAsync() to save the changes.
    // MainViewModel creates a new instance of this class and call EditorShell.SetEntry(EntryResidentialFull) and sets this property.
    private Models.Rent.Residentials.EntryResidentialFull _entry = new();

    // This flag indicates if the entry is dirty (i.e., has unsaved changes).
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

    #region == 基本 == 

    // 物件名
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
                OnPropertyChanged(nameof(BasicsPreview));
            }
        }
    }

    #region == 種別 ==

    // For the use of the ComboBox in the UI, we define a collection of kinds.
    public ObservableCollection<Kind> Kinds =
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new Kind(Models.Rent.Residentials.EnumKinds.Apartment.ToString(), "アパート"),
        new Kind(Models.Rent.Residentials.EnumKinds.Mansion.ToString(), "マンション"),
        new Kind(Models.Rent.Residentials.EnumKinds.House.ToString(), "一戸建て"),
        new Kind(Models.Rent.Residentials.EnumKinds.TerraceHouse.ToString(), "テラスハウス"),
        new Kind(Models.Rent.Residentials.EnumKinds.TownHouse.ToString(), "タウンハウス"),
        new Kind(Models.Rent.Residentials.EnumKinds.ShareHouse.ToString(), "シェアハウス"),
        new Kind(Models.Rent.Residentials.EnumKinds.Dormitory.ToString(), "寮・下宿")
    ];

    // The SelectedKind property holds the currently selected kind from the ComboBox in the UI.
    private Kind _selectedKind = new(Models.Rent.Residentials.EnumKinds.Unspecified.ToString(), "");
    public Kind SelectedKind
    {
        get => _selectedKind;
        set
        {
            if (SetProperty(ref _selectedKind, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(BasicsPreview));
            }
        }
    }
    #endregion

    // 区分所有か一括所有かを示すプロパティ
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
                OnPropertyChanged(nameof(BasicsPreview));

                // If this is set, then show/hide the owner and zumen from shell menu.
                EventIsUnitOwnership?.Invoke(this, _isUnitOwnership);
            }
        }
    }

    public string BasicsPreview
    {
        get
        {
            string s = string.Empty;

            if (!string.IsNullOrEmpty(_name))
            {
                s = _name;
            }

            if (!string.IsNullOrEmpty(_selectedKind.Label))
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += _selectedKind.Label;
            }

            if (_isUnitOwnership)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += "区分所有";
            }

            return s;
        }
    }

    #endregion

    #region == 所在地 ==

    private Pref _selectedPef = new(0, "");
    public Pref SelectedPef
    {
        get => _selectedPef;
        set
        {
            if (SetProperty(ref _selectedPef, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview)); // Notify that the address preview has changed
            }
        }
    }

    public ObservableCollection<Pref> Prefs =
    [
        new Pref(1, "北海道"),
        new Pref(2, "青森県"),
            new Pref(3, "岩手県"),
            new Pref(4, "宮城県"),
            new Pref(5, "秋田県"),
            new Pref(6, "山形県"),
            new Pref(7, "福島県"),
            new Pref(8, "茨城県"),
            new Pref(9, "栃木県"),
            new Pref(10, "群馬県"),
            new Pref(11, "埼玉県"),
            new Pref(12, "千葉県"),
            new Pref(13, "東京都"),
            new Pref(14, "神奈川県"),
            new Pref(15, "新潟県"),
            new Pref(16, "富山県"),
            new Pref(17, "石川県"),
            new Pref(18, "福井県"),
            new Pref(19, "山梨県"),
            new Pref(20, "長野県"),
            new Pref(21, "岐阜県"),
            new Pref(22, "静岡県"),
            new Pref(23, "愛知県"),
            new Pref(24, "三重県"),
            new Pref(25, "滋賀県"),
            new Pref(26, "京都府"),
            new Pref(27, "大阪府"),
            new Pref(28, "兵庫県"),
            new Pref(29, "奈良県"),
            new Pref(30, "和歌山県"),
            new Pref(31, "鳥取県"),
            new Pref(32, "島根県"),
            new Pref(33, "岡山県"),
            new Pref(34, "広島県"),
            new Pref(35, "山口県"),
            new Pref(36, "徳島県"),
            new Pref(37, "香川県"),
            new Pref(38, "愛媛県"),
            new Pref(39, "高知県"),
            new Pref(40, "福岡県"),
            new Pref(41, "佐賀県"),
            new Pref(42, "長崎県"),
            new Pref(43, "熊本県"),
            new Pref(44, "大分県"),
            new Pref(45, "宮崎県"),
            new Pref(46, "鹿児島県"),
            new Pref(47, "沖縄県"),
    ];

    private string _cityName = string.Empty;
    public string CityName
    {
        get => _cityName;
        set
        {
            if (SetProperty(ref _cityName, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    private string _townName = string.Empty;
    public string TownName
    {
        get => _townName;
        set
        {
            if (SetProperty(ref _townName, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    private string _choume = string.Empty;
    public string Choume
    {
        get => _choume;
        set
        {
            if (SetProperty(ref _choume, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    private string _banchi = string.Empty;
    public string Banchi
    {
        get => _banchi;
        set
        {
            if (SetProperty(ref _banchi, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    // TODO:
    private string _postalCode = string.Empty;
    public string PostalCode
    {
        get => _postalCode;
        set
        {
            if (SetProperty(ref _postalCode, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public string AddressPreview
    {
        get
        {
            // If the SelectedPef is not set, return an empty string.
            if (SelectedPef == null)
            {
                return string.Empty;
            }

            string s = string.Empty;
            if (!string.IsNullOrEmpty(_banchi))
            {
                s = "-" + _banchi;
            }
            // TODO:
            return $"{SelectedPef.Name}{_cityName}{_townName}{_choume}{s}";
        }
    }

    #endregion

    #region = 交通 ==

    private string _ensen = string.Empty;
    public string Ensen
    {
        get => _ensen;
        set
        {
            if (SetProperty(ref _ensen, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private string _eki = string.Empty;
    public string Eki
    {
        get => _eki;
        set
        {
            if (SetProperty(ref _eki, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private int _ekiToho = 0;
    public int EkiToho
    {
        get => _ekiToho;
        set
        {
            if (SetProperty(ref _ekiToho, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private string _busStop = string.Empty;
    public string BusStop
    {
        get => _busStop;
        set
        {
            if (SetProperty(ref _busStop, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private int __busJyousya = 0;
    public int BusJyousya
    {
        get => __busJyousya;
        set
        {
            if (SetProperty(ref __busJyousya, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private int _busStopToho = 0;
    public int BusStopToho
    {
        get => _busStopToho;
        set
        {
            if (SetProperty(ref _busStopToho, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    public string TransportationPreview
    {
        get
        {
            string s = string.Empty;

            if (!string.IsNullOrEmpty(_ensen))
            {
                s = _ensen;
            }

            if (!string.IsNullOrEmpty(_eki))
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += $"{_eki}駅";
            }

            if (_ekiToho > 0)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += $"徒歩{_ekiToho}分";
            }

            return s;
        }
    }

    #endregion

    #region == 構造 ==

    private Structure _selectedStructure = new(Models.Rent.Residentials.EnumStructure.Unspecified.ToString(), "");
    public Structure SelectedStructure
    {
        get => _selectedStructure;
        set
        {
            if (SetProperty(ref _selectedStructure, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    public ObservableCollection<Structure> Structures =
    [
        //new Structure(EnumStructure.Unspecified.ToString(), "未指定"),
        new Structure(Models.Rent.Residentials.EnumStructure.Wood.ToString(), "木造"),
        new Structure(Models.Rent.Residentials.EnumStructure.Block.ToString(), "ブロック造"),
        new Structure(Models.Rent.Residentials.EnumStructure.LightSteel.ToString(), "軽量鉄骨造"),
        new Structure(Models.Rent.Residentials.EnumStructure.Steel.ToString(), "鉄骨造"),
        new Structure(Models.Rent.Residentials.EnumStructure.RC.ToString(), "鉄筋コンクリート(RC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.SRC.ToString(), "鉄骨鉄筋コンクリート(SRC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.ALC.ToString(), "軽量気泡コンクリート(ALC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.PC.ToString(), "プレキャストコンクリート(PC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.HPC.ToString(), "鉄骨プレキャストコンクリート(HPC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.RB.ToString(), "鉄筋ブロック造"),
        new Structure(Models.Rent.Residentials.EnumStructure.CFT.ToString(), "コンクリート充填鋼管(CFT)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.Other.ToString(), "その他")
    ];

    private int _basementFloorCount = 0;
    public int BasementFloorCount
    {
        get => _basementFloorCount;
        set
        {
            if (SetProperty(ref _basementFloorCount, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    private int _aboveGroundFloorCount = 0;
    public int AboveGroundFloorCount
    {
        get => _aboveGroundFloorCount;
        set
        {
            if (SetProperty(ref _aboveGroundFloorCount, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    private int _totalUnitCount = 0;
    public int TotalUnitCount
    {
        get => _totalUnitCount;
        set
        {
            if (SetProperty(ref _totalUnitCount, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    private DateTimeOffset? _selectedBuildingBuiltMonthYear;// = new DateTime(1950, 1, 1);
    public DateTimeOffset? SelectedBuildingBuiltMonthYear
    {
        get => _selectedBuildingBuiltMonthYear;
        set
        {
            if (SetProperty(ref _selectedBuildingBuiltMonthYear, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    public string StructurePreview
    {
        get
        {
            string s = string.Empty;

            if (_selectedStructure != null)
            {
                if (!string.IsNullOrEmpty(_selectedStructure.Label))
                {
                    s = _selectedStructure.Label;
                }
            }
            if (AboveGroundFloorCount > 0)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }

                s += $"地上{AboveGroundFloorCount}階";
            }
            if (BasementFloorCount > 0)
            {
                if (AboveGroundFloorCount > 0)
                {
                    s += ", ";
                }
                s += $"地下{BasementFloorCount}階";
            }
            if (TotalUnitCount > 0)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += $"総戸数{TotalUnitCount}戸";
            }
            if (SelectedBuildingBuiltMonthYear != null)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += $"{SelectedBuildingBuiltMonthYear:yyyy年M月}築";
            }

                
            return s;//$"{SelectedStructure.Label}, 地上{AboveGroundFloorCount}, 地下{BasementFloorCount}階建て, 総戸数{TotalUnitCount}戸, {BuildingBuiltMonthYear:yyyy年M月}築";
        }
    }

    #endregion

    #region == 設備 ==


    #endregion

    #region == 管理 ==


    #endregion

    #region ==  その他 ==


    #endregion

    #endregion

    #region == Events

    // The event handlers below are used to notify the UI about various actions that can be performed in the editor.
    // EditorShell subscribes to these events to handle the actions accordingly.
    public event EventHandler? EventAddNewUnit;
    // Who subscribes to this event?
    public event EventHandler? EventGoBack;
    // EditorShell subscribes to this event to handle the back navigation.
    public event EventHandler? EventBackToSummary;
    public event EventHandler? EventEditLocation;
    public event EventHandler? EventEditTransportation;
    public event EventHandler? EventEditAppliance;
    // EditorShell subscribes to this event to handle the actions accordingly.
    public event EventHandler<bool>? EventIsUnitOwnership;

    #endregion

    #region == Services ==
    
    // The IDataAccessService is used to access the data layer for saving and updating entries.
    private readonly IDataAccessService _dataAccessService;

    #endregion

    #region == Constructor ==

    // Constructor for the EditorViewModel class, initializes the data access service and the entry.
    #pragma warning disable IDE0290 
    public EditorViewModel(IDataAccessService dataAccessService)
    
    {
        _dataAccessService = dataAccessService;

    }
    #pragma warning restore IDE0290

    #endregion

    #region == Methods ==

    public void SetEntry(Models.Rent.Residentials.EntryResidentialFull entry)
    {
        if (entry != null)
        {
            _entry = entry;
            /*
            if (_editorWindow == null)
            {
                // TODO: Log an error or handle it accordingly.
                return;
            }

            // Set the Id property of the window to the Entry.Id.
            _editorWindow.Id = _entry.Id;
            */

            Name = _entry.Name;
            //TODO: Set other properties

            IsEntryDirty = false;
        }
    }

    /*
    public void SetEditorWindow(Views.Rent.Residentials.Editor.EditorWindow editorWin)
    {
        _editorWindow = editorWin;

        // Set the Id property of the window to the Entry.Id if _entry is not null (most likely is for now).
        if (_entry != null)
        {
            _editorWindow.Id = _entry.Id;
        }
    }
    */

    #endregion

    #region == Commands ==

    private RelayCommand? saveCommand;

    public IRelayCommand SaveCommand => saveCommand ??= new RelayCommand(SaveAsync);

    private async void SaveAsync()
    {
        if (IsEntryDirty)
        {
            if (string.IsNullOrEmpty(Name))
            {
                Debug.WriteLine("TODO: Name is null or empty. This field is required. Show alart and abort.");
            }

            _entry.Name = Name;
            //TODO: Set other properties for Entry.

            if (string.IsNullOrEmpty(_entry.Id))
            {
                // If the Entry.Id is empty, generate a new ID and save as new.
                _entry.SetId = Guid.CreateVersion7().ToString();
                await SaveAsNew().ConfigureAwait(false);
            }
            else
            {
                // If the Entry.Id is not empty, update the existing entry.
                await SaveAsUpdate().ConfigureAwait(false);
            }
        }
    }

    private Task SaveAsNew()
    {
        var resInsert = _dataAccessService.InsertRentResidential(_entry.Id, _entry.Name, "some comment");
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
            // Clear these dirty flags.
            IsEntryDirty = false;
            _entry.IsDirty = false;

            Debug.WriteLine("No errors on insert.");
        }

        // Return a completed task to satisfy the method's return type
        return Task.CompletedTask;
    }

    private Task SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(_entry.Id, _entry.Name, "some update comment");
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
            // Clear these dirty flags.
            IsEntryDirty = false;
            _entry.IsDirty = false;

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
    private RelayCommand? addNewUnitCommand;

    public IRelayCommand AddNewUnitCommand => addNewUnitCommand ??= new RelayCommand(AddNewUnit);

    private void AddNewUnit()
    {
        //NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");
        EventAddNewUnit?.Invoke(this, EventArgs.Empty);
    }

    // Go Back command (don't use this?)
    private RelayCommand? goBackCommand;

    public IRelayCommand BackCommand => goBackCommand ??= new RelayCommand(GoBack);

    private void GoBack()
    {
        EventGoBack?.Invoke(this, EventArgs.Empty);
    }

    private RelayCommand? backToSummaryCommand;

    public IRelayCommand BackToSummaryCommand => backToSummaryCommand ??= new RelayCommand(GoBackToSummary);

    public void GoBackToSummary()
    {
        EventBackToSummary?.Invoke(this, EventArgs.Empty);
    }

    private RelayCommand? editLocationCommand;
    public IRelayCommand EditLocationCommand => editLocationCommand ??= new RelayCommand(EditLocation);

    public void EditLocation()
    {
        EventEditLocation?.Invoke(this, EventArgs.Empty);
    }

    private RelayCommand? editTransportationCommand;
    public IRelayCommand EditTransportationCommand => editTransportationCommand ??= new RelayCommand(EditTransportation);

    private void EditTransportation()
    {
        EventEditTransportation?.Invoke(this, EventArgs.Empty);
    }

    private RelayCommand? editApplianceCommand;
    public IRelayCommand EditApplianceCommand => editApplianceCommand ??= new RelayCommand(EditAppliance);

    public void EditAppliance()
    {
        EventEditAppliance?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}
