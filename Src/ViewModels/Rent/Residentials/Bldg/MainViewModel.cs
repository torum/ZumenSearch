using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Rent.Residentials.Bldg;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.Views;

namespace ZumenSearch.ViewModels.Rent.Residentials.Bldg;

public sealed partial class MainViewModel : ObservableObject
{
    #region == Public Properties ==

    public string Id => _id;

    public INavigationGenericService? ResidentialNavigationService => _navService;

    public readonly List<Views.Rent.Residentials.Room.EditorWindow> ChildEditorList = [];

    // Local directory path to save blob data such as pictures and PDFs.

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "建物", Page = typeof(Views.Rent.Residentials.Bldg.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Bldg.BasicPage).FullName! }
    ];


    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    public string WindowTitle
    {
        get
        {
            if (string.IsNullOrEmpty(Name))
            {
                return $"{field} (新規)";
            }
            else
            {
                var str = $"{field}：{Name}";

                if (PropertyStatus == EnumPropertyStatus.New)
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
        set
        {
            OnPropertyChanged();
        }
    } = "賃貸住居用";

    #region == ステータス ==

    public EnumPropertyStatus PropertyStatus => _building.PropertyStatus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    /*
    public bool IsDirty
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                _mainViewModel.IsDirty = true;
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
    */

    // TODO: update this to hold more info such as page so that it can be navigated to the page.
    [ObservableProperty]
    public partial bool HasErrors { get; private set; }

    #endregion

    #region == 基本プロパティ == 

    // 物件名
    public string Name
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;

                ValidateName(value);

                // Update title with dummy value.
                WindowTitle = string.Empty;

                // Moved to after the save.
                //_selectedSearchResult?.Name = value; // Update the selected search result's name if it exists
            }
        }
    }

    [ObservableProperty]
    public partial bool NameHasError { get; private set; }

    [ObservableProperty]
    public partial string NameErrorMessage { get; private set; } = string.Empty;

    private void ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            NameErrorMessage = "物件名（必須項目）を入力してください";
            NameHasError = true;

            HasErrors = true;

            return;
        }

        var realLength = new StringInfo(value).LengthInTextElements;
        if (realLength > 100)
        {
            NameErrorMessage = "物件名は100文字以内で入力してください";
            NameHasError = true;

            HasErrors = true;

            return;
        }

        NameHasError = false;
    }

    // 物件種別
    public ObservableCollection<Kind> Kinds =
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new Kind(Models.Rent.Residentials.Bldg.EnumKinds.Apartment),
        new Kind(Models.Rent.Residentials.Bldg.EnumKinds.Mansion),
        new Kind(Models.Rent.Residentials.Bldg.EnumKinds.House),
        new Kind(Models.Rent.Residentials.Bldg.EnumKinds.TerraceHouse),
        new Kind(Models.Rent.Residentials.Bldg.EnumKinds.TownHouse),
        new Kind(Models.Rent.Residentials.Bldg.EnumKinds.ShareHouse),
        new Kind(Models.Rent.Residentials.Bldg.EnumKinds.Dormitory)
    ];

    public Kind SelectedKind
    {
        get => field ?? new(Models.Rent.Residentials.Bldg.EnumKinds.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // 区分所有か一括所有か
    public bool IsUnitOwnership
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;

                //

                // If this is set, then show/hide the owner and zumen from shell menu.
                //EventIsUnitOwnershipChanged?.Invoke(this, field);
            }
        }
    }

    //public bool IsNotUnitOwnership => !IsUnitOwnership;

    // 建物構造
    public ObservableCollection<Structure> Structures =
    [
        //new Structure(EnumStructures.Unspecified.ToString(), "未指定"),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.Wood),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.Block),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.LightSteel),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.Steel),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.RC),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.SRC),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.ALC),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.PC),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.HPC),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.RB),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.CFT),
        new Structure(Models.Rent.Residentials.Bldg.EnumStructures.Other)
    ];

    public Structure SelectedStructure
    {
        get => field ?? new(Models.Rent.Residentials.Bldg.EnumStructures.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // 地上階
    public string AboveGroundFloorCount
    {
        get => field ?? string.Empty;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
            }
            else if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
            }

            OnPropertyChanged();
        }
    }

    // 地下階
    public string BasementFloorCount
    {
        get => field ?? string.Empty;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
            }
            else if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
            }

            OnPropertyChanged();
        }
    }

    // 総戸数
    public string TotalUnitCount
    {
        get => field ?? string.Empty;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
            }
            else if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
            }

            OnPropertyChanged();
        }
    }

    // 築年月
    public DateTimeOffset? BuiltYearAndMonth
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BuiltYearAndMonthPreview));
            }
        }
    }

    // 築年月（和暦表示）
    public string BuiltYearAndMonthPreview
    {
        get
        {
            var s = string.Empty;

            if (BuiltYearAndMonth is not null)
            {
                var cultureJp = new CultureInfo("ja-jp", false);
                cultureJp.DateTimeFormat.Calendar = new JapaneseCalendar();

                return BuiltYearAndMonth.Value.ToString("ggy年M月", cultureJp);
                //return _builtYearAndMonth.ToString("O"); // For serialization
            }

            return s;
        }
    }

    // 不動産ID (13桁)
    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            // TODO: check 13桁.

            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
        }
    }

    // 特定コード（４桁）建物全体は0000
    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty; // keep non-null empty string.
        set
        {
            // TODO: check （４桁）

            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
        }
    } = "0000";

    // 備考
    public string Remarks
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(RemarksPreview));
            }
        }
    } = string.Empty;

    public string RemarksPreview
    {
        get
        {
            string s;
            if (Remarks.Length > 14)
            {
                s = Remarks[..14] + "...";
            }
            else
            {
                s = Remarks;
            }

            return s;
        }
    }

    #endregion

    #region == 所在地プロパティ ==

    public string? MachiazaId;

    public ObservableCollection<Prefecture> Prefectures = new(new PrefectureList().Prefectures);

    public Prefecture? SelectedPef
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    IsDirty = true;
                    MachiazaId = null; // Reset MachiazaId
                }

                OnPropertyChanged(nameof(AddressPreview));

                if (field is null)
                {
                    Cities = null;
                    return;
                }

                var dataset = new List<CountyAndCity>();

                dataset = _dataAccessLocationService.GetCountyAndCityByPref(field.Name);

                Cities = [.. dataset.DistinctBy(p => p.Combined)];

            }
        }
    }

    public ObservableCollection<CountyAndCity>? Cities
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public CountyAndCity? SelectedCity
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    IsDirty = true;
                    MachiazaId = field?.MachiazaId;
                }

                OnPropertyChanged(nameof(AddressPreview));

                if (SelectedPef is null)
                {
                    Towns = null;
                    return;
                }

                if (field is null)
                {
                    Towns = null;
                    return;
                }

                var dataset = new List<WardAndOaza>();

                dataset = _dataAccessLocationService.GetWardAndOazaByPrefCountyCity(SelectedPef.Name, field.County, field.City);

                Towns = [.. dataset.DistinctBy(p => p.Combined)];
            }
        }
    }

    public ObservableCollection<WardAndOaza>? Towns
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public WardAndOaza? SelectedTown
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    IsDirty = true;
                    MachiazaId = field?.MachiazaId;
                }

                OnPropertyChanged(nameof(AddressPreview));

                if (SelectedPef is null)
                {
                    Chous = null;
                    return;
                }
                if (SelectedCity is null)
                {
                    Chous = null;
                    return;
                }

                if (field is null)
                {
                    Chous = null;
                    return;
                }

                // TODO: move this

                var dataset = new List<Choume>();

                dataset = _dataAccessLocationService.GetChoumeByPrefCountyCityWardOaza(SelectedPef.Name, SelectedCity.County, SelectedCity.City, field.Ward, field.Oaza);

                Chous = [.. dataset.DistinctBy(p => p.Chou)];
            }
        }
    }

    public ObservableCollection<Choume>? Chous
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public Choume? SelectedChou
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    // The value (_selectedChou) may be an empty string (and it is OK).
                    IsDirty = true;
                    MachiazaId = field?.MachiazaId;
                }

                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public string Edaban
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    } = string.Empty;

    // TODO:
    public string PostalCode
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    } = string.Empty;

    public string AddressPreview
    {
        get
        {
            // If the SelectedPef is not set, return an empty string.
            if (SelectedPef == null)
            {
                return string.Empty;
            }

            var s = string.Empty;
            if (!string.IsNullOrEmpty(Edaban))
            {
                s = "-" + Edaban;
            }
            // TODO:
            return $"{SelectedPef.Name}{SelectedCity?.Combined}{SelectedTown?.Combined}{SelectedChou?.Chou}{s}";
        }
    }

    // 緯度（Lat）
    public string LocationLatitude
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsDirty = true;
            OnPropertyChanged(nameof(LocationLatitude));
            OnPropertyChanged(nameof(GeoUri));
            ShowGoogleMapsCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    // 経度（Lon）
    public string LocationLongitude
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsDirty = true;
            OnPropertyChanged(nameof(LocationLongitude));
            OnPropertyChanged(nameof(GeoUri));
            ShowGoogleMapsCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    public string GeoUri
    {
        get
        {
            if (string.IsNullOrEmpty(LocationLatitude) || string.IsNullOrEmpty(LocationLongitude))
            {
                return "https://maps.google.co.jp/";
            }

            return $"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}";
        }
    }

    #endregion

    #region == 交通プロパティ ==

    public RailLine? SelectedRailLine1
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                // Clear  old value.
                SelectedRailStation1 = null;
            }

            ShowRailStationSelect1Command.NotifyCanExecuteChanged();
        }
    }

    public RailStation? SelectedRailStation1
    {
        get;
        set
        {
            if ((value is null) && (field is not null))
            {
                // Clear old value.
                field.StationName = string.Empty;
                OnPropertyChanged(nameof(SelectedRailStation1));
            }

            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string EkiToho1
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(EkiToho1));
        }
    } = string.Empty;

    public string BusStop
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public string BusJyousya1
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(BusJyousya1));
        }
    } = string.Empty;

    public string BusStopToho1
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(BusStopToho1));
        }
    } = string.Empty;

    // TODO: more.

    #endregion

    #region == 設備プロパティ ==

    #region == 一般 ==

    public bool HasAutolock
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool HasElevator
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool HasSecurityCamera
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool HasParcelLocker
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    // TODO: Do I use this now?
    public string AppliancePreview
    {
        get
        {
            var s = string.Empty;

            if (HasAutolock)
            {
                s += "オートロック";
            }
            if (HasElevator)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ",";
                }
                s += "エレベーター";
            }
            if (HasSecurityCamera)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ",";
                }
                s += "防犯カメラ";
            }
            if (HasParcelLocker)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ",";
                }
                s += "宅配ボックス";
            }

            return s;
        }
    }

    #endregion

    #region == 電気 ==

    public bool HasElectric
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = false;

    public ObservableCollection<ElectricKind> ElectricKinds =
    [
        new ElectricKind(Models.Rent.Residentials.Bldg.Property.EnumElectricKind.AllElectric, "オール電化"),
        new ElectricKind(Models.Rent.Residentials.Bldg.Property.EnumElectricKind.Unspecified, "未指定")
    ];

    public ElectricKind SelectedElectricKind
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new ElectricKind(Models.Rent.Residentials.Bldg.Property.EnumElectricKind.Unspecified, "未指定");

    public string ElectricDetail
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    #endregion

    // TODO: more.

    #endregion

    #region == 管理プロパティ ==

    public ObservableCollection<KanriShutai> KanriShutais =
    [
        new KanriShutai(Models.Rent.Residentials.Bldg.Property.EnumKanriShutai.Unspecified, "未指定"),
        new KanriShutai(Models.Rent.Residentials.Bldg.Property.EnumKanriShutai.Jisya, "自社管理"),
        new KanriShutai(Models.Rent.Residentials.Bldg.Property.EnumKanriShutai.Tasya, "他社管理"),
        new KanriShutai(Models.Rent.Residentials.Bldg.Property.EnumKanriShutai.Kashinushi, "貸主管理")
    ];

    public KanriShutai? SelectedKanriShutai
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;

                if (field is null)
                {
                    Debug.WriteLine("SelectedKanriShutai is null");
                    return;
                }

                // Toggle Visibilities
                IsKanriUnspecified = field.Key == Models.Rent.Residentials.Bldg.Property.EnumKanriShutai.Unspecified;
                IsKanriJisya = field.Key == Models.Rent.Residentials.Bldg.Property.EnumKanriShutai.Jisya;
                IsKanriTasya = field.Key == Models.Rent.Residentials.Bldg.Property.EnumKanriShutai.Tasya;
                IsKanriKashinushi = field.Key == Models.Rent.Residentials.Bldg.Property.EnumKanriShutai.Kashinushi;
            }
        }
    }

    [ObservableProperty]
    public partial bool IsKanriUnspecified { get; set; } = true;

    [ObservableProperty]
    public partial bool IsKanriJisya { get; set; }

    [ObservableProperty]
    public partial bool IsKanriTasya { get; set; }

    [ObservableProperty]
    public partial bool IsKanriKashinushi { get; set; }

    // （自社管理）管理内容:建物維持管理 
    public bool KanriIsMaintenanceManagementIfIsKanriJisya
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // （自社管理）管理内容:入居者管理
    public bool KanriIsTenantManagementIfIsKanriJisya
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // （自社管理）管理内容:入金管理
    public bool KanriIsPaymentManagementIfIsKanriJisya
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // （自社管理）管理内容:その他管理
    public bool KanriIsOtherManagementIfIsKanriJisya
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // （自社管理）管理備考
    public string KanriRemarkIfIsKanriJisya
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // （他社管理）管理会社名
    public string KanriNameOfCompanyIfIsKanriTasya
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    // （他社管理）管理会社連絡先
    public string KanriContactInfoOfCompanyIfIsKanriTasya
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    // （他社管理）備考
    public string KanriRemarkIfIsKanriTasya
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // （貸主管理）備考
    public string KanriRemarkIfIsKanriKashinushi
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }


    #endregion

    #region == 写真プロパティ ==

    public ObservableCollection<Picture> BuildingPictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    #endregion

    #region == PDFプロパティ ==

    public ObservableCollection<Pdf> BuildingPdfs
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    #endregion

    #region == 部屋プロパティ ==

    public ObservableCollection<Models.Rent.Residentials.Room.Listing> Rooms
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = [];

    /*
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedUnitCommand))]
    public partial Models.Rent.Residentials.Room.Listing? SelectedRoom { get; set; }
    */

    #endregion

    #endregion

    #region == Events

    #endregion

    #region == Private Variables ==

    private readonly string _id = string.Empty;

    // The Entry property holds the COPY of current RentResidential entry being edited.
    // Do not use it directly in the UI. Apply changes to this object in SaveAsync() to save the changes.
    // MainViewModel creates a new instance of this class and call EditorShell.SetEntry(EntryResidentialFull) and sets this property.
    private readonly Models.Rent.Residentials.Bldg.Property _building;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedBuildingPictureFileList = [];
    private readonly List<string> _unsavedBuildingPdfFileList = [];
    private readonly List<string> _unsavedBuildingPdfThumbnailFileList = [];

    // Holds the selected search result from the MainWindow which is used to update title/name and other properties.
    private Models.Rent.Residentials.PropertySearchResultItem? _selectedSearchResult;

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<Models.Rent.Residentials.Room.Listing, Views.Rent.Residentials.Room.ShellPage> _shellFactory;
    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private IModalDialogService? _dlgService;
    private INavigationGenericService? _navService;

    #endregion

    public MainViewModel(Models.Rent.Residentials.Bldg.Property building, IAbstractFactory<Models.Rent.Residentials.Room.Listing, Views.Rent.Residentials.Room.ShellPage> shellFactory, IDispatcherService dispatcherService, IDataAccessService dataAccessService, IDataAccessLocationService dataAccessLocationService)
    {
        _building = building;
        _id = building.Id;

        _shellFactory = shellFactory;

        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;
        _dispatcherService = dispatcherService;

        //EntryDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.AppDataPictureFolder, "Rent"), "Residential_Building"), _id);

        // Update title with dummy value.
        WindowTitle = string.Empty;

        try
        {
            PopulateEntryValues();

            // Reset errors
            NameHasError = false;
            // TODO: more.

            HasErrors = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BldgViewModel: {ex}");
        }
        finally
        {
            IsDirty = false;
        }
    }

    #region == Private Methods ==

    private void PopulateEntryValues()
    {
        // Basics

        Name = _building.Name;

        //SelectedKind = _building.BuildingKind;
        var kindkey = Kinds.FirstOrDefault(k => k.Key == _building.BuildingKind.Key);
        SelectedKind = kindkey is null ? new(Models.Rent.Residentials.Bldg.EnumKinds.Unspecified) : kindkey;

        IsUnitOwnership = _building.IsUnitOwnership;

        //SelectedStructure = _building.BuildingStructure;
        var Structurekey = Structures.FirstOrDefault(k => k.Key == _building.BuildingStructure.Key);
        SelectedStructure = Structurekey is null ? new(Models.Rent.Residentials.Bldg.EnumStructures.Unspecified) : Structurekey;

        AboveGroundFloorCount = _building.AboveGroundFloorCount == 0 ? string.Empty : _building.AboveGroundFloorCount.ToString();
        BasementFloorCount = _building.BasementFloorCount == 0 ? string.Empty : _building.BasementFloorCount.ToString();
        TotalUnitCount = _building.TotalUnitCount == 0 ? string.Empty : _building.TotalUnitCount.ToString();
        BuiltYearAndMonth = _building.BuiltYearAndMonth.Year != 1900 ? _building.BuiltYearAndMonth : null;
        FudousanId = _building.FudousanId;
        FudousanIdAdditionalCode = _building.FudousanIdAdditionalCode;
        Remarks = _building.Remarks;


        // Location

        if (!string.IsNullOrEmpty(_building.LocPrefId))
        {
            var hoge = Prefectures.FirstOrDefault<Prefecture>(p => p.MunicipalityCode.Equals(_building.LocPrefId));
            if (hoge is not null)
            {
                SelectedPef = hoge;
            }
        }

        if ((Cities is not null) && ((!string.IsNullOrEmpty(_building.LocCounty)) || (!string.IsNullOrEmpty(_building.LocCity))))
        {
            foreach (var cty in Cities)
            {
                if (cty.County.Equals(_building.LocCounty) && cty.City.Equals(_building.LocCity))
                {
                    SelectedCity = cty;
                    break;
                }
            }
        }

        if ((Towns is not null) && ((!string.IsNullOrEmpty(_building.LocWard)) || (!string.IsNullOrEmpty(_building.LocOazaCho))))
        {
            foreach (var twn in Towns)
            {
                if (twn.Ward.Equals(_building.LocWard) && twn.Oaza.Equals(_building.LocOazaCho))
                {
                    SelectedTown = twn;
                    break;
                }
            }
        }

        if (Chous is not null) // Allow an empty string. //&& (!string.IsNullOrEmpty(_building.LocChoume))
        {
            var hoge = Chous.FirstOrDefault<Choume>(p => p.Chou.Equals(_building.LocChoume));
            if (hoge is not null)
            {
                SelectedChou = hoge;
            }
        }

        Edaban = _building.LocEdaban;

        //TODO: Set other properties


        // Pictures:
        BuildingPictures = new ObservableCollection<Models.Rent.Residentials.Bldg.Picture>(_building.BuildingPictures); // create a copy.

        foreach (var item in BuildingPictures)
        {
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnBuildingPicturePropertyChanged;
        }

        BuildingPictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Bldg.Picture item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from BuildingPictures");
                    IsDirty = true;

                    item.PropertyChanged -= OnBuildingPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Bldg.Picture item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to BuildingPictures");
                    IsDirty = true;

                    item.PropertyChanged += OnBuildingPicturePropertyChanged;
                }
            }
        };

        // PDFs
        BuildingPdfs = new ObservableCollection<Models.Rent.Residentials.Bldg.Pdf>(_building.BuildingPdfs); // create a copy.

        foreach (var item in BuildingPdfs)
        {
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnBuildingPdfPropertyChanged;
        }

        BuildingPdfs.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Bldg.Pdf item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from BuildingPdfs");
                    IsDirty = true;

                    item.PropertyChanged -= OnBuildingPdfPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Bldg.Pdf item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to BuildingPdfs");
                    IsDirty = true;

                    item.PropertyChanged += OnBuildingPdfPropertyChanged;
                }
            }
        };

        // Rooms
        Rooms = new ObservableCollection<Models.Rent.Residentials.Room.Listing>(_building.Rooms); // create a copy.

        foreach (var item in Rooms)
        {
            //
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnRoomPropertyChanged;
        }

        Rooms.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Room.Listing item in e.OldItems)
                {
                    //Debug.WriteLine($"Item {item.Id} Removed from Rooms");
                    //IsDirty = true;

                    item.PropertyChanged -= OnRoomPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Room.Listing item in e.NewItems)
                {
                    //Debug.WriteLine($"Item {item.Id} Added to Rooms");
                    //IsDirty = true;

                    item.PropertyChanged += OnRoomPropertyChanged;
                }
            }
        };

        //Debug.WriteLine($"PopulateEntryValues: Completed populating values from Entry to VM. Entry ID: {_building.Id}, Rooms Count: {Rooms.Count}");
    }

    private void OnBuildingPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Bldg.Picture picBldg)
        {
            Debug.WriteLine("OnBuildingPicturePropertyChanged returned non PictureBldg.");
            return;
        }

        //Debug.WriteLine($"Property {e.PropertyName} changed");

        if (picBldg.IsModified)
        {
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain"))
            {
                if (picBldg.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in BuildingPictures)
                    {
                        if (item != picBldg)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
        }
    }

    private void OnBuildingPdfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Bldg.Pdf pdfBldg)
        {
            Debug.WriteLine("OnBuildingPdfPropertyChanged returned non PdfBldg.");
            return;
        }

        //Debug.WriteLine($"Property {e.PropertyName} changed");

        if (pdfBldg.IsModified)
        {
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain"))
            {
                if (pdfBldg.IsMain)
                {
                    // Clear all other pdfs
                    foreach (var item in BuildingPdfs)
                    {
                        if (item != pdfBldg)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
        }
    }

    private void OnRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Room.Listing room)
        {
            Debug.WriteLine("OnRoomPropertyChanged returned non Room.");
            return;
        }

        Debug.WriteLine($"Property {e.PropertyName} changed");

        if (room.IsModified)
        {
            /*
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            
            if (prop.Equals("IsMain"))
            {
                if (picBldg.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in BuildingPictures)
                    {
                        if (item != picBldg)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
            */
        }
    }

    private void SetValuesToEntry()
    {
        if (!IsDirty)
        {
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            // TODO: Show InfoBar?
            HasErrors = true;
            return;
        }

        // 物件名
        _building.Name = Name;

        _building.BuildingKind = SelectedKind;
        _building.IsUnitOwnership = IsUnitOwnership;
        _building.BuildingStructure = SelectedStructure;
        _building.AboveGroundFloorCount = int.TryParse(AboveGroundFloorCount, out var aboveGroundFloorCount) ? aboveGroundFloorCount : 0; //Convert.ToInt32(AboveGroundFloorCount)
        _building.BasementFloorCount = int.TryParse(BasementFloorCount, out var basementFloorCount) ? basementFloorCount : 0;
        _building.TotalUnitCount = int.TryParse(TotalUnitCount, out var totalUnitCount) ? totalUnitCount : 0;
        _building.BuiltYearAndMonth = BuiltYearAndMonth ?? new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _building.FudousanId = FudousanId;
        _building.FudousanIdAdditionalCode = FudousanIdAdditionalCode;
        _building.Remarks = Remarks;
        // TODO: Set other properties



        // 所在地
        _building.LocPrefId = (SelectedPef is not null) ? SelectedPef.MunicipalityCode : string.Empty;
        _building.LocPrefecture = (SelectedPef is not null) ? SelectedPef.Name : string.Empty;
        _building.LocMachiazaId = (!string.IsNullOrEmpty(MachiazaId)) ? MachiazaId : string.Empty;
        _building.LocCounty = (SelectedCity is not null) ? SelectedCity.County : string.Empty;
        _building.LocCity = (SelectedCity is not null) ? SelectedCity.City : string.Empty;
        _building.LocWard = (SelectedTown is not null) ? SelectedTown.Ward : string.Empty;
        _building.LocOazaCho = (SelectedTown is not null) ? SelectedTown.Oaza : string.Empty;
        _building.LocChoume = (SelectedChou is not null) ? SelectedChou.Chou : string.Empty;
        _building.LocEdaban = (!string.IsNullOrEmpty(Edaban)) ? Edaban : string.Empty;
        _building.LocLocationFull = AddressPreview;


        //TODO: Set other properties for Entry.
        //////////////////

        // 写真
        _building.BuildingPictures = BuildingPictures;

        // 図面
        _building.BuildingPdfs = BuildingPdfs;

        // 部屋
        _building.Rooms = Rooms;

    }

    private void SaveAsNew()
    {
        var resInsert = _dataAccessService.InsertRentResidential(_building);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on insert. @SaveAsNew in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: return error object.
            return;
        }
        else
        {
            Debug.WriteLine("No errors on insert.");

            IsDirty = false;

            _unsavedBuildingPictureFileList.Clear();
            _unsavedBuildingPdfThumbnailFileList.Clear();
            _unsavedBuildingPdfFileList.Clear();

            _building.IsDirty = false;
            _building.PropertyStatus = EnumPropertyStatus.Saved;

            // Update title with dummy value.
            WindowTitle = string.Empty;
        }

        return;
    }

    private void SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(_building);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on update. @SaveAsUpdate in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: return error object.
            return;
        }
        else
        {
            Debug.WriteLine("No errors on update.");

            IsDirty = false;

            _building.IsDirty = false;
            _building.PropertyStatus = EnumPropertyStatus.Saved;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clean up deleted picture file.
            if (_building.BuildingPicturesToBeDeleted.Count > 0)
            {
                foreach (var file in _building.BuildingPicturesToBeDeleted)
                {
                    File.Delete(file.ImageLocation);
                }
                _building.BuildingPicturesToBeDeleted.Clear();
            }

            // Clean up deleted PDF and thumb file.
            if (_building.BuildingPdfsToBeDeleted.Count > 0)
            {
                foreach (var file in _building.BuildingPdfsToBeDeleted)
                {
                    File.Delete(file.PdfLocation);
                    File.Delete(file.ThumbnailLocation);
                }
                _building.BuildingPdfsToBeDeleted.Clear();
            }

            _unsavedBuildingPictureFileList.Clear();
            _unsavedBuildingPdfThumbnailFileList.Clear();
            _unsavedBuildingPdfFileList.Clear();
        }
    }

    private void DiscardUnsavedFiles()
    {
        if (_unsavedBuildingPictureFileList.Count > 0)
        {
            foreach (var file in _unsavedBuildingPictureFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved picture file: {file}");
                    File.Delete(file);
                }
            }
            _unsavedBuildingPictureFileList.Clear();
        }

        if (_unsavedBuildingPdfThumbnailFileList.Count > 0)
        {
            foreach (var file in _unsavedBuildingPdfThumbnailFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved PDF Thumbnail file: {file}");
                    File.Delete(file);
                }
            }
            _unsavedBuildingPdfThumbnailFileList.Clear();
        }

        if (_unsavedBuildingPdfFileList.Count > 0)
        {
            foreach (var file in _unsavedBuildingPdfFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved PDF file: {file}");
                    File.Delete(file);
                }
            }
            _unsavedBuildingPdfFileList.Clear();
        }

        if (_building.PropertyStatus == EnumPropertyStatus.New)
        {
            if (Directory.Exists(_building.PropertyDataDirectoryPath))
            {
                Debug.WriteLine($"Deleting folder: {_building.PropertyDataDirectoryPath}");
                Directory.Delete(_building.PropertyDataDirectoryPath, true);
            }
        }
    }

    #endregion

    #region == Public Methods ==

    public void SetNavigationService(INavigationGenericService nav)
    {
        _navService = nav;
    }

    public void SetDialogService(IModalDialogService dialog)
    {
        _dlgService = dialog;
    }

    public void SetSearchResult(Models.Rent.Residentials.PropertySearchResultItem? searchResult)
    {
        _selectedSearchResult = searchResult;
    }

    public void RemoveRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId)) return;
        var room = Rooms.FirstOrDefault(r => r.Id == roomId);
        if (room is null) return;
        if (Rooms.Contains(room))
        {
            Rooms.Remove(room);
            //IsDirty = true;
        }
    }

    // TODO: change these to commands.
    public async Task SetNewBuildingPdfsAsync(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_building.PropertyDataDirectoryPath}  @SetNewBuildingPdfsAsync()");

        if (!Directory.Exists(_building.PropertyDataDirectoryPath))
        {
            Directory.CreateDirectory(_building.PropertyDataDirectoryPath);
        }

        //List<string> list = [];

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                continue;
            }

            // TODO: check file ext for valid image type.
            // TODO: set max file size?


            string extension = Path.GetExtension(System.IO.Path.GetFileName(filePath));
            if (!extension.Equals(".pdf")) // TODO: check case.
            {
                continue;
            }

            //using var sourceStream = File.Open(file, FileMode.Open);

            StorageFile sfile = await StorageFile.GetFileFromPathAsync(filePath);
            PdfDocument pdfDocument = await PdfDocument.LoadFromFileAsync(sfile);

            if (pdfDocument.PageCount > 0)
            {
                using PdfPage pdfPage = pdfDocument.GetPage(0);
                using var stream = new InMemoryRandomAccessStream();

                // Set screen standard DPI
                //float targetDpi = 96f;
                //float scaleFactor = targetDpi / 72f; 
                //uint calculatedWidth = (uint)Math.Round(pdfPage.Size.Width * scaleFactor);

                var options = new PdfPageRenderOptions
                {
                    // Set the desired target width in pixels (e.g., 1024px)
                    // Aspect ratio is locked; height scales automatically.
                    DestinationWidth = 512//calculatedWidth//1024
                };

                await pdfPage.RenderToStreamAsync(stream, options);

                //var bitmapImage = new BitmapImage();
                //await bitmapImage.SetSourceAsync(stream);

                string newId = Guid.CreateVersion7().ToString("N");
                var thumbnailDestFilePath = Path.Combine(_building.PropertyDataDirectoryPath, newId + ".bmp");

                using var destinationStream = File.Create(thumbnailDestFilePath);
                using var managedSourceStream = stream.AsStreamForRead();
                await managedSourceStream.CopyToAsync(destinationStream);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPdfThumbnailFileList.Add(thumbnailDestFilePath);

                var pdfDestFilePath = Path.Combine(_building.PropertyDataDirectoryPath, newId + extension);
                File.Copy(filePath, pdfDestFilePath);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPdfFileList.Add(pdfDestFilePath);

                var pdf = new Models.Rent.Residentials.Bldg.Pdf(newId, pdfDestFilePath, thumbnailDestFilePath)
                {
                    IsNew = true,
                    ParentViewModel = this
                };

                BuildingPdfs.Add(pdf);

                OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
                DeleteBuildingPdfCommand.NotifyCanExecuteChanged();

                IsDirty = true;
            }
            else
            {
                Debug.WriteLine("0 page.");
            }
        }
    }

    public void DiscardChanges()
    {
        DiscardUnsavedFiles();

        IsDirty = false;
    }

    #endregion

    #region == Commands ==

    #region == Save related commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        if (!IsDirty)
        {
            return;
        }

        // Validate input.
        ValidateName(Name);
        // TODO: more.
        if (HasErrors)
        {
            // TODO: Show InfoBar.
            InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            IsInfoBarErrorOpen = true;

            HasErrors = false;
            return;
        }

        SetValuesToEntry();

        if (_building.PropertyStatus == EnumPropertyStatus.New)
        {
            SaveAsNew();
        }
        else
        {
            SaveAsUpdate();
        }

        // Update the selected search result's values such asname if it exists.
        _selectedSearchResult?.Name = Name;

        foreach (var room in Rooms)
        {
            room.PropertyName = Name;
        }

    }
    private bool CanSave()
    {
        if (IsDirty)
        {
            return true;
        }
        return false;
    }

    #endregion

    #region == Pictures and Pdf related commands ==

    [RelayCommand(CanExecute = nameof(CanAddNewBuildingPictures))]
    private async Task AddNewBuildingPictures(List<string> filePathList)
    {
        // File open is handleed in the code behind since this ViewModel does not know about the Window which is required for the Picker.

        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        //Debug.WriteLine($"destDirectory={_building.PropertyDataDirectoryPath}  @SetNewBuildingPicturesAsync()");

        if (!Directory.Exists(_building.PropertyDataDirectoryPath))
        {
            Directory.CreateDirectory(_building.PropertyDataDirectoryPath);
        }

        //List<string> list = [];

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                continue;
            }

            // TODO: check file ext for valid image type.
            // TODO: set max file size?

            // TODO: Create thumbnail image?


            using var sourceStream = File.Open(filePath, FileMode.Open);

            string newId = Guid.CreateVersion7().ToString("N");
            string extension = Path.GetExtension(System.IO.Path.GetFileName(filePath));
            var destFilePath = Path.Combine(_building.PropertyDataDirectoryPath, newId + extension);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = File.Create(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.Bldg.Picture(newId, destFilePath)
            {
                IsNew = true,
                ParentViewModel = this
            };

            BuildingPictures.Add(pic);

            OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteBuildingPictureCommand.NotifyCanExecuteChanged();

            IsDirty = true;

            // Keep track of unsaved files to delete them when discarding.
            _unsavedBuildingPictureFileList.Add(destFilePath);
        }
    }
    private static bool CanAddNewBuildingPictures()
    {
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPicture))]
    private void DeleteBuildingPicture(Models.Rent.Residentials.Bldg.Picture picBldg)
    {
        if (picBldg is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (BuildingPictures.Remove(picBldg))
        {
            _building.BuildingPicturesToBeDeleted.Add(picBldg);
            IsDirty = true;
        }
    }
    private static bool CanDeleteBuildingPicture(Models.Rent.Residentials.Bldg.Picture picBldg)
    {
        return picBldg is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPdf))]
    private void DeleteBuildingPdf(Models.Rent.Residentials.Bldg.Pdf pdfBldg)
    {
        if (pdfBldg is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (BuildingPdfs.Remove(pdfBldg))
        {
            _building.BuildingPdfsToBeDeleted.Add(pdfBldg);
            IsDirty = true;
        }
    }
    private static bool CanDeleteBuildingPdf(Models.Rent.Residentials.Bldg.Pdf pdfBldg)
    {
        return pdfBldg is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenBuildingBlobDirectory))]
    private void OpenBuildingBlobDirectory()
    {
        if (Directory.Exists(_building.PropertyDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _building.PropertyDataDirectoryPath);
            }
            catch (Exception ex)
            {
                // TODO: show error to user.
                Debug.WriteLine($"Error opening folder: {ex.Message}");
            }
        }
    }
    private bool CanOpenBuildingBlobDirectory()
    {
        if (Directory.Exists(_building.PropertyDataDirectoryPath))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region == Rooms related commands ==

    // Add New Modal window command
    [RelayCommand]
    private void AddNewUnit() 
    {
        var editorShell = _shellFactory.Create(new Models.Rent.Residentials.Room.Listing(Guid.CreateVersion7().ToString("N"), _building.Id, Name));
        //editorShell.SetParentViewModel(this);

        editorShell.ViewModel.SetParentViewModel(this);

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        mainVM.RoomEditorList.Add(editorShell.Window);
        this.ChildEditorList.Add(editorShell.Window);
        //shell.ParentWin = shell.Win;

        if (editorShell.Window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorShell.Window.SetListingIdToWindow(editorShell.ViewModel.Id);
        editorShell.Window.SetViewModelToWindow(editorShell.ViewModel);

        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

        editorShell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainVM.RoomEditorWinLeft, mainVM.RoomEditorWinTop, mainVM.RoomEditorWinWidth, mainVM.RoomEditorWinHeight));

        //editorWindow.AppWindow.Show();
        editorShell.Window.Activate();
        editorShell.Window.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand(CanExecute = nameof(EditSelectedUnitCanExecute))]
    private void EditSelectedUnit(Models.Rent.Residentials.Room.Listing room)
    {
        if (room is null) return;

        /*
        _mainViewModel.Unit.SetEditUnit(room);

        _mainViewModel.GoToUnitShellPageCommand.Execute(this);
        */

        var rentId = room.PropertyId;
        var unitId = room.Id;

        if (string.IsNullOrEmpty(rentId))//if (selected == null)
        {
            Debug.WriteLine("EditSelectedUnitCommand executed but no rentId.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

        //var isFound = false;

        var mainVM = App.GetService<ViewModels.MainViewModel>();

        // Check if the selected item is already being edited in another window.
        foreach (var editWin in mainVM.RoomEditorList.ToList())
        {
            if (editWin.Id != unitId)
            {
                continue;
            }

            Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");

            try
            {
                editWin.Activate();

                if (editWin.Content is Views.Rent.Residentials.Room.ShellPage editShell)
                {
                    editShell.Window?.AppWindow.MoveInZOrderBelow(editWin.AppWindow.Id);
                }

                editWin.AppWindow.MoveInZOrderAtTop();

                //isFound = true;

                return;
            }
            catch (COMException)
            {
                // 既に閉じられたウィンドウをリストから除去
                mainVM.RoomEditorList.Remove(editWin);
                ChildEditorList.Remove(editWin);
            }
            /*
            //Debug.WriteLine($"Checking editor window with Id: {editorWindow.Id} for selected item with Id: {unitId}");
            if (editorWindow.Id == unitId)
            {
                // If the editor window for this item is already open, activate it.
                Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");
                isFound = true;

                editorWindow.Activate();

                // TODO:
                //var mainWindow = App.GetService<MainWindow>();
                //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                if (editorWindow.Content is Views.Rent.Residentials.Room.ShellPage shell)
                {
                    shell.Win?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
                }
                editorWindow.AppWindow.MoveInZOrderAtTop();

                return;
            }
            */
        }
        /*
        if (isFound)
        {
            // If the editor window for this item is already open, no need to create a new one.
            return;
        }
        */

        /*
        // Access Database to get the full entry data.
        var res = _dataAccessService.SelectRentResidentialById(rentId);// Go back to UI thred. Let's not do > .ConfigureAwait(false);
        if (res.IsError)
        {
            Debug.WriteLine(res.Error.ErrText + Environment.NewLine + res.Error.ErrDescription + Environment.NewLine + res.Error.ErrPlace + Environment.NewLine + res.Error.ErrPlaceParent);

            //ErrorMain = res.Error;
            //IsMainErrorInfoBarVisible = true;

            // TODO: Show error message to user
            return;
        }

        if (res.EntryFull == null)
        {
            Debug.WriteLine($"EntryResidentialFull for {rentId} is null. Cannot open editor.");
            return;
        }
        */

        //var editorShell = _shellFactory.Create(res.EntryFull);

        var editorShell = _shellFactory.Create(room);
        //editorShell.SetParentViewModel(this);

        editorShell.ViewModel.SetParentViewModel(this);

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        editorWindow.SetListingIdToWindow(editorShell.ViewModel.Id);
        editorWindow.SetViewModelToWindow(editorShell.ViewModel);

        mainVM.RoomEditorList.Add(editorWindow);
        this.ChildEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainVM.RoomEditorWinLeft, mainVM.RoomEditorWinTop, mainVM.RoomEditorWinWidth, mainVM.RoomEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        // TODO:
        //var mainWindow = App.GetService<MainWindow>();
        //mainWindow?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
        if (editorWindow.Content is Views.Rent.Residentials.Room.ShellPage shell)
        {
            shell.Window?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
        }
        editorWindow.AppWindow.MoveInZOrderAtTop();

    }
    public bool EditSelectedUnitCanExecute(Models.Rent.Residentials.Room.Listing room)
    {
        if (room is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DupeSelectedUnitCanExecute))]
    private void DupeSelectedUnit(Models.Rent.Residentials.Room.Listing room)
    {
        if (room is null) return;

        //
    }
    public bool DupeSelectedUnitCanExecute(Models.Rent.Residentials.Room.Listing room)
    {
        if (room is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteSelectedUnitCanExecute))]
    private void DeleteSelectedUnit(Models.Rent.Residentials.Room.Listing room)
    {
        if (room is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (Rooms.Remove(room))
        {
            // No. Don't
            //if (_building.Rooms.Remove(room)) { }
            _building.RoomsToBeDeleted.Add(room);
            IsDirty = true;
        }

        // Make sure to set it to null.
        //SelectedRoom = null;
    }
    public bool DeleteSelectedUnitCanExecute(Models.Rent.Residentials.Room.Listing room)
    {
        if (room is null) return false;
        return true;
    }


    #endregion

    #region == Location related commands ==

    [RelayCommand(CanExecute = nameof(CanShowGoogleMaps))]
    public async Task ShowGoogleMaps()
    {
        if (string.IsNullOrEmpty(LocationLatitude) || string.IsNullOrEmpty(LocationLongitude))
        {
            return;
        }

        var uriGoogleMaps = new Uri($"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}");
        await Launcher.LaunchUriAsync(uriGoogleMaps);

    }
    private bool CanShowGoogleMaps()
    {
        if (string.IsNullOrEmpty(LocationLatitude) || string.IsNullOrEmpty(LocationLongitude))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region == Transportation related commands ==

    [RelayCommand]
    public async Task ShowRailLineSelect1()
    {
        if (_dlgService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        var railLine = await _dlgService.ShowRailLineSelectDialog();

        if (railLine is not null)
        {
            SelectedRailLine1 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect1))]
    public async Task ShowRailStationSelect1()
    {
        if (_dlgService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        if (SelectedRailLine1 is null)
        {
            return;
        }

        var railStation = await _dlgService.ShowRailStationSelectDialog(SelectedRailLine1.LineCode);

        if (railStation is not null)
        {
            SelectedRailStation1 = railStation;
        }
    }
    private bool CanShowRailStationSelect1()
    {
        if (SelectedRailLine1 is null)
        {
            return false;
        }

        return true;
    }

    #endregion

    #endregion

}
