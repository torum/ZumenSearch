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
using static ZumenSearch.Models.Rent.Residentials.Enums;

namespace ZumenSearch.ViewModels.Rent.Residentials.Editor;

// 賃貸住居用物件の「種別」クラス
public class Kind(string key, string label)
{
    public string Label { get; set; } = label;
    public string Key { get; set; } = key;
};

// 賃貸住居用物件の「構造」クラス
public class Structure(string key, string label)
{
    public string Label { get; set; } = label;
    public string Key { get; set; } = key;
};

// 賃貸住居用物件の「都道府県」クラス
public class Pref
{
    public Pref(int iD, string name)
    {
        this.ID = iD;
        this.Name = name;
    }

    public int ID { get; private set; } // jis_code の初めの二桁

    public string Name { get; private set; }
};

// 賃貸住居用物件の編集用本体ViewModelクラス
public partial class EditorViewModel : ObservableObject
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
        new Kind(EnumKinds.Apartment.ToString(), "アパート"),
        new Kind(EnumKinds.Mansion.ToString(), "マンション"),
        new Kind(EnumKinds.House.ToString(), "一戸建て"),
        new Kind(EnumKinds.TerraceHouse.ToString(), "テラスハウス"),
        new Kind(EnumKinds.TownHouse.ToString(), "タウンハウス"),
        new Kind(EnumKinds.ShareHouse.ToString(), "シェアハウス"),
        new Kind(EnumKinds.Dormitory.ToString(), "寮・下宿")
    ];

    // The SelectedKind property holds the currently selected kind from the ComboBox in the UI.
    private Kind _selectedKind = new(EnumKinds.Unspecified.ToString(), "");
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
            }
        }
    }

    public string BasicsPreview
    {
        get
        {
            // If the SelectedPef is not set, return an empty string.
            if (SelectedPef == null)
            {
                return string.Empty;
            }

            string s = string.Empty;
            if (_name != string.Empty)
            {
                s = _name ?? string.Empty;
            }

            if (_selectedKind.Label != string.Empty)
            {
                s += ", " + _selectedKind.Label;
            }

            if (_isUnitOwnership)
            {
                s += ", " + "区分所有";
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

    // TODO:
    public Dictionary<string, int> PrefsDictionary
    {
        get; set;
    } = new Dictionary<string, int>()
        {
            {"北海道", 1},
            {"青森県", 2},
            {"岩手県", 3},
            {"宮城県", 4},
            {"秋田県", 5},
            {"山形県", 6},
            {"福島県", 7},
            {"茨城県", 8},
            {"栃木県", 9},
            {"群馬県", 10},
            {"埼玉県", 11},
            {"千葉県", 12},
            {"東京都", 13},
            {"神奈川県",14},
            {"新潟県", 15},
            {"富山県", 16},
            {"石川県", 17},
            {"福井県", 18},
            {"山梨県", 19},
            {"長野県", 20},
            {"岐阜県", 21},
            {"静岡県", 22},
            {"愛知県", 23},
            {"三重県", 24},
            {"滋賀県", 25},
            {"京都府", 26},
            {"大阪府", 27},
            {"兵庫県", 28},
            {"奈良県", 29},
            {"和歌山県",30},
            {"鳥取県", 31},
            {"島根県", 32},
            {"岡山県", 33},
            {"広島県", 34},
            {"山口県", 35},
            {"徳島県", 36},
            {"香川県", 37},
            {"愛媛県", 38},
            {"高知県", 39},
            {"福岡県", 40},
            {"佐賀県", 41},
            {"長崎県", 42},
            {"熊本県", 43},
            {"大分県", 44},
            {"宮崎県", 45},
            {"鹿児島県", 16},
            {"沖縄県", 47},
        };

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
            if (_banchi != string.Empty)
            {
                s = "-" + _banchi;
            }
            // TODO:
            return $"{SelectedPef.Name}{_cityName}{_townName}{_choume}{s}";
        }
    }

    #endregion

    #region = 交通 ==



    #endregion

    #region == 建物構造 ==

    private Structure _selectedStructure = new(EnumStructure.Unspecified.ToString(), "");
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
            // If the SelectedPef is not set, return an empty string.
            if (_selectedStructure == null)
            {
                return string.Empty;
            }

            string s = string.Empty;
            if (_selectedStructure.Label != string.Empty)
            {
                s = _selectedStructure.Label;
            }
            if (AboveGroundFloorCount > 0)
            {
                if (s != string.Empty)
                {
                    s += ", ";
                }

                s= $"地上{AboveGroundFloorCount}階";
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
                if (s != string.Empty)
                {
                    s += ", ";
                }
                s += $"総戸数{TotalUnitCount}戸";
            }
            if (SelectedBuildingBuiltMonthYear != null)
            {
                if (s != string.Empty)
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


    // The Entry property holds the COPY of current RentResidential entry being edited. Do not use it directly in the UI. Apply changes to this object in SaveAsync() to save the changes.
    private Models.Rent.Residentials.EntryResidential _entry = new();
    public Models.Rent.Residentials.EntryResidential Entry
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

    #region == Events

    // The event handlers below are used to notify the UI about various actions that can be performed in the editor.
    //
    public event EventHandler<string>? EventAddNewUnit;
    //
    public event EventHandler<string>? EventGoBack;
    public event EventHandler<string>? EventBackToSummary;
    //
    public event EventHandler<string>? EventEditStructure;
    public event EventHandler<string>? EventEditLocation;
    public event EventHandler<string>? EventEditTransportation;
    public event EventHandler<string>? EventEditAppliance;

    #endregion

    // The IDataAccessService is used to access the data layer for saving and updating entries.
    private readonly IDataAccessService _dataAccessService;

    // Constructor for the EditorViewModel class, initializes the data access service and the entry.
    public EditorViewModel(IDataAccessService dataAccessService)
    {
        _dataAccessService = dataAccessService;

    }

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

            Entry.Name = Name;
            //TODO: Set other properties for Entry.

            if (string.IsNullOrEmpty(Entry.Id))
            {
                // If the Entry.Id is empty, generate a new ID and save as new.
                Entry.SetId = Guid.CreateVersion7().ToString();
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
            // Clear these dirty flags.
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
            // Clear these dirty flags.
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
    private RelayCommand? addNewUnitCommand;

    public IRelayCommand AddNewUnitCommand => addNewUnitCommand ??= new RelayCommand(AddNewUnit);

    private void AddNewUnit()
    {
        //NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");
        EventAddNewUnit?.Invoke(this, "asdf");
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

    public void GoBackToSummary()
    {
        EventBackToSummary?.Invoke(this, "asdf");
    }

    private RelayCommand? editStructureCommand;
    public IRelayCommand EditStructureCommand => editStructureCommand ??= new RelayCommand(EditStructure);

    public void EditStructure()
    {
        EventEditStructure?.Invoke(this, "asdf");
    }

    private RelayCommand? editLocationCommand;
    public IRelayCommand EditLocationCommand => editLocationCommand ??= new RelayCommand(EditLocation);

    public void EditLocation()
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

    public void EditAppliance()
    {
        EventEditAppliance?.Invoke(this, "asdf");
    }

    #endregion
}
