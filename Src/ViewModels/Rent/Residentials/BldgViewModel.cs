using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials;

internal sealed partial class BldgViewModel : ObservableObject
{
    //Constants

    //Static Fields (Readonly first, then mutable)

    //Instance Fields (Readonly first, then mutable)

    #region == Public Properties ==

    #region == ステータス ==

    internal EnumEntryStatus EntryStatus => _entry.EntryStatus;

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
                _mainViewModel.WindowTitle = string.Empty;

                //EventTitleChanged?.Invoke(this, EventArgs.Empty);//TODO
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
        }
        else
        {
            NameHasError = false;
        }
    }

    // 物件種別
    public ObservableCollection<Models.Rent.Residentials.Kind> Kinds =
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new Kind(Models.Rent.Residentials.EntryResidential.EnumKinds.Apartment, "アパート"),
        new Kind(Models.Rent.Residentials.EntryResidential.EnumKinds.Mansion, "マンション"),
        new Kind(Models.Rent.Residentials.EntryResidential.EnumKinds.House, "一戸建て"),
        new Kind(Models.Rent.Residentials.EntryResidential.EnumKinds.TerraceHouse, "テラスハウス"),
        new Kind(Models.Rent.Residentials.EntryResidential.EnumKinds.TownHouse, "タウンハウス"),
        new Kind(Models.Rent.Residentials.EntryResidential.EnumKinds.ShareHouse, "シェアハウス"),
        new Kind(Models.Rent.Residentials.EntryResidential.EnumKinds.Dormitory, "寮・下宿")
    ];

    public Models.Rent.Residentials.Kind SelectedKind
    {
        get => field ?? new(Models.Rent.Residentials.EntryResidential.EnumKinds.Unspecified, "未指定");
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
                EventIsUnitOwnershipChanged?.Invoke(this, field);
            }
        }
    }

    //public bool IsNotUnitOwnership => !IsUnitOwnership;

    // 建物構造
    public ObservableCollection<Models.Rent.Residentials.Structure> Structures =
    [
        //new Structure(EnumStructure.Unspecified.ToString(), "未指定"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.Wood, "木造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.Block, "ブロック造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.LightSteel, "軽量鉄骨造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.Steel, "鉄骨造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.RC, "鉄筋コンクリート(RC)造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.SRC, "鉄骨鉄筋コンクリート(SRC)造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.ALC, "軽量気泡コンクリート(ALC)造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.PC, "プレキャストコンクリート(PC)造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.HPC, "鉄骨プレキャストコンクリート(HPC)造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.RB, "鉄筋ブロック造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.CFT, "コンクリート充填鋼管(CFT)造"),
        new Structure(Models.Rent.Residentials.EntryResidential.EnumStructure.Other, "その他")
    ];

    public Models.Rent.Residentials.Structure SelectedStructure
    {
        get => field ?? new(Models.Rent.Residentials.EntryResidential.EnumStructure.Unspecified, "未指定");
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

    public ObservableCollection<Models.Rent.Residentials.ElectricKind> ElectricKinds =
    [
        new ElectricKind(Models.Rent.Residentials.EntryResidential.EnumElectricKind.AllElectric, "オール電化"),
        new ElectricKind(Models.Rent.Residentials.EntryResidential.EnumElectricKind.Unspecified, "未指定")
    ];

    public Models.Rent.Residentials.ElectricKind SelectedElectricKind
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new ElectricKind(Models.Rent.Residentials.EntryResidential.EnumElectricKind.Unspecified, "未指定");

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

    public ObservableCollection<Models.Rent.Residentials.KanriShutai> KanriShutais =
    [
        new KanriShutai(Models.Rent.Residentials.EntryResidential.EnumKanriShutai.Unspecified, "未指定"),
        new KanriShutai(Models.Rent.Residentials.EntryResidential.EnumKanriShutai.Jisya, "自社管理"),
        new KanriShutai(Models.Rent.Residentials.EntryResidential.EnumKanriShutai.Tasya, "他社管理"),
        new KanriShutai(Models.Rent.Residentials.EntryResidential.EnumKanriShutai.Kashinushi, "貸主管理")
    ];

    public Models.Rent.Residentials.KanriShutai? SelectedKanriShutai
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
                IsKanriUnspecified = field.Key == Models.Rent.Residentials.EntryResidential.EnumKanriShutai.Unspecified;
                IsKanriJisya = field.Key == Models.Rent.Residentials.EntryResidential.EnumKanriShutai.Jisya;
                IsKanriTasya = field.Key == Models.Rent.Residentials.EntryResidential.EnumKanriShutai.Tasya;
                IsKanriKashinushi = field.Key == Models.Rent.Residentials.EntryResidential.EnumKanriShutai.Kashinushi;
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

    internal ObservableCollection<PictureBldg> BuildingPictures
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

    internal ObservableCollection<PdfBldg> BuildingPdfs
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

    internal ObservableCollection<Models.Rent.Residentials.UnitResidential> Rooms
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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedUnitCommand))]
    internal partial Models.Rent.Residentials.UnitResidential? SelectedRoom { get; set; }

    #endregion

    #endregion

    #region == Events ==

    public event EventHandler<bool>? EventIsUnitOwnershipChanged; // show or hides navigationview' menu accordingly.

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;

    #endregion

    #region == Private Variables ==

    private readonly ViewModels.Rent.Residentials.MainViewModel _mainViewModel;

    // The Entry property holds the COPY of current RentResidential entry being edited.
    // Do not use it directly in the UI. Apply changes to this object in SaveAsync() to save the changes.
    // MainViewModel creates a new instance of this class and call EditorShell.SetEntry(EntryResidentialFull) and sets this property.
    private readonly Models.Rent.Residentials.EntryResidential _entry;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedBuildingPictureFileList = [];
    private readonly List<string> _unsavedBuildingPdfFileList = [];
    private readonly List<string> _unsavedBuildingPdfThumbnailFileList = [];

    #endregion

    internal BldgViewModel(ViewModels.Rent.Residentials.MainViewModel vm, Models.Rent.Residentials.EntryResidential entry, IDataAccessService dataAccessService, IDataAccessLocationService dataAccessLocationService)
    {
        _mainViewModel = vm;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        _entry = entry;

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

    //Finalizers / Destructors

    //Delegates and Events

    //Properties (Public first, then internal/private)

    //Methods (Grouped by feature or accessibility)

    //Nested Types (Enums, Structs, Classes)

    #region == Private Methods ==

    private void PopulateEntryValues()
    {
        // Basics

        Name = _entry.Name;

        // Location

        if (!string.IsNullOrEmpty(_entry.LocPrefId))
        {
            var hoge = Prefectures.FirstOrDefault<Prefecture>(p => p.MunicipalityCode.Equals(_entry.LocPrefId));
            if (hoge is not null)
            {
                SelectedPef = hoge;
            }
        }

        if ((Cities is not null) && ((!string.IsNullOrEmpty(_entry.LocCounty)) || (!string.IsNullOrEmpty(_entry.LocCity))))
        {
            foreach (var cty in Cities)
            {
                if (cty.County.Equals(_entry.LocCounty) && cty.City.Equals(_entry.LocCity))
                {
                    SelectedCity = cty;
                    break;
                }
            }
        }

        if ((Towns is not null) && ((!string.IsNullOrEmpty(_entry.LocWard)) || (!string.IsNullOrEmpty(_entry.LocOazaCho))))
        {
            foreach (var twn in Towns)
            {
                if (twn.Ward.Equals(_entry.LocWard) && twn.Oaza.Equals(_entry.LocOazaCho))
                {
                    SelectedTown = twn;
                    break;
                }
            }
        }

        if (Chous is not null) // Allow an empty string. //&& (!string.IsNullOrEmpty(_entry.LocChoume))
        {
            var hoge = Chous.FirstOrDefault<Choume>(p => p.Chou.Equals(_entry.LocChoume));
            if (hoge is not null)
            {
                SelectedChou = hoge;
            }
        }

        Edaban = _entry.LocEdaban;

        //TODO: Set other properties


        // Pictures:
        BuildingPictures = new ObservableCollection<Models.Rent.Residentials.PictureBldg>(_entry.BuildingPictures); // create a copy.

        foreach (var item in BuildingPictures)
        {
            item.ParentViewModel = _mainViewModel;//this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnBuildingPicturePropertyChanged;
        }

        BuildingPictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.PictureBldg item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from BuildingPictures");
                    IsDirty = true;

                    item.PropertyChanged -= OnBuildingPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.PictureBldg item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to BuildingPictures");
                    IsDirty = true;

                    item.PropertyChanged += OnBuildingPicturePropertyChanged;
                }
            }
        };

        // PDFs
        BuildingPdfs = new ObservableCollection<Models.Rent.Residentials.PdfBldg>(_entry.BuildingPdfs); // create a copy.

        foreach (var item in BuildingPdfs)
        {
            item.ParentViewModel = _mainViewModel;//this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnBuildingPdfPropertyChanged;
        }

        BuildingPdfs.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.PdfBldg item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from BuildingPdfs");
                    IsDirty = true;

                    item.PropertyChanged -= OnBuildingPdfPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.PdfBldg item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to BuildingPdfs");
                    IsDirty = true;

                    item.PropertyChanged += OnBuildingPdfPropertyChanged;
                }
            }
        };

        // Rooms
        Rooms = new ObservableCollection<Models.Rent.Residentials.UnitResidential>(_entry.Rooms); // create a copy.

        //Debug.WriteLine($"PopulateEntryValues: Completed populating values from Entry to VM. Entry ID: {_entry.Id}, Rooms Count: {Rooms.Count}");
    }

    private void OnBuildingPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ZumenSearch.Models.Rent.Residentials.PictureBldg picBldg)
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
        if (sender is not ZumenSearch.Models.Rent.Residentials.PdfBldg pdfBldg)
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
        _entry.Name = Name;

        // 所在地
        _entry.LocPrefId = (SelectedPef is not null) ? SelectedPef.MunicipalityCode : string.Empty;
        _entry.LocPrefecture = (SelectedPef is not null) ? SelectedPef.Name : string.Empty;
        _entry.LocMachiazaId = (!string.IsNullOrEmpty(MachiazaId)) ? MachiazaId : string.Empty;
        _entry.LocCounty = (SelectedCity is not null) ? SelectedCity.County : string.Empty;
        _entry.LocCity = (SelectedCity is not null) ? SelectedCity.City : string.Empty;
        _entry.LocWard = (SelectedTown is not null) ? SelectedTown.Ward : string.Empty;
        _entry.LocOazaCho = (SelectedTown is not null) ? SelectedTown.Oaza : string.Empty;
        _entry.LocChoume = (SelectedChou is not null) ? SelectedChou.Chou : string.Empty;
        _entry.LocEdaban = (!string.IsNullOrEmpty(Edaban)) ? Edaban : string.Empty;
        _entry.LocLocationFull = AddressPreview;


        //TODO: Set other properties for Entry.
        //////////////////

        // 写真
        _entry.BuildingPictures = BuildingPictures;

        // 図面
        _entry.BuildingPdfs = BuildingPdfs;

        // 部屋
        _entry.Rooms = Rooms;

    }

    private void SaveAsNew()
    {
        var resInsert = _dataAccessService.InsertRentResidential(_entry);
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

            _entry.IsDirty = false;
            _entry.EntryStatus = EnumEntryStatus.Saved;

            // Update title with dummy value.
            _mainViewModel.WindowTitle = string.Empty;
        }

        return;
    }

    private void SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(_entry);
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

            _entry.IsDirty = false;
            _entry.EntryStatus = EnumEntryStatus.Saved;

            // Update title with dummy value.
            _mainViewModel.WindowTitle = string.Empty;

            // Clean up deleted picture file.
            if (_entry.BuildingPicturesToBeDeleted.Count > 0)
            {
                foreach (var file in _entry.BuildingPicturesToBeDeleted)
                {
                    File.Delete(file.ImageLocation);
                }
                _entry.BuildingPicturesToBeDeleted.Clear();
            }

            // Clean up deleted PDF and thumb file.
            if (_entry.BuildingPdfsToBeDeleted.Count > 0)
            {
                foreach (var file in _entry.BuildingPdfsToBeDeleted)
                {
                    File.Delete(file.PdfLocation);
                    File.Delete(file.ThumbnailLocation);
                }
                _entry.BuildingPdfsToBeDeleted.Clear();
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

        if (_entry.EntryStatus == EnumEntryStatus.New)
        {
            if (Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
            {
                Debug.WriteLine($"Deleting folder: {_mainViewModel.EntryDataDirectoryPath}");
                Directory.Delete(_mainViewModel.EntryDataDirectoryPath, true);
            }
        }
    }

    #endregion

    #region == Public Methods ==

    // TODO: change these to commands.

    public async Task SetNewBuildingPdfsAsync(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_mainViewModel.EntryDataDirectoryPath}  @SetNewBuildingPdfsAsync()");

        if (!Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
        {
            Directory.CreateDirectory(_mainViewModel.EntryDataDirectoryPath);
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
                var thumbnailDestFilePath = Path.Combine(_mainViewModel.EntryDataDirectoryPath, newId + ".bmp");

                using var destinationStream = File.Create(thumbnailDestFilePath);
                using var managedSourceStream = stream.AsStreamForRead();
                await managedSourceStream.CopyToAsync(destinationStream);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPdfThumbnailFileList.Add(thumbnailDestFilePath);

                var pdfDestFilePath = Path.Combine(_mainViewModel.EntryDataDirectoryPath, newId + extension);
                File.Copy(filePath, pdfDestFilePath);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPdfFileList.Add(pdfDestFilePath);

                var pdf = new Models.Rent.Residentials.PdfBldg(newId, pdfDestFilePath, thumbnailDestFilePath)
                {
                    IsNew = true,
                    ParentViewModel = _mainViewModel
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
            _mainViewModel.InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            _mainViewModel.IsInfoBarErrorOpen = true;

            HasErrors = false;
            return;
        }


        SetValuesToEntry();

        if (_entry.EntryStatus == EnumEntryStatus.New)
        {
            SaveAsNew();
        }
        else
        {
            SaveAsUpdate();
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

        Debug.WriteLine($"destDirectory={_mainViewModel.EntryDataDirectoryPath}  @SetNewBuildingPicturesAsync()");

        if (!Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
        {
            Directory.CreateDirectory(_mainViewModel.EntryDataDirectoryPath);
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
            var destFilePath = Path.Combine(_mainViewModel.EntryDataDirectoryPath, newId + extension);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = File.Create(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.PictureBldg(newId, destFilePath)
            {
                IsNew = true,
                ParentViewModel = _mainViewModel
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
    private void DeleteBuildingPicture(Models.Rent.Residentials.PictureBldg picBldg)
    {
        if (picBldg is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (BuildingPictures.Remove(picBldg))
        {
            _entry.BuildingPicturesToBeDeleted.Add(picBldg);
            IsDirty = true;
        }
    }
    private static bool CanDeleteBuildingPicture(Models.Rent.Residentials.PictureBldg picBldg)
    {
        return picBldg is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPdf))]
    private void DeleteBuildingPdf(Models.Rent.Residentials.PdfBldg pdfBldg)
    {
        if (pdfBldg is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (BuildingPdfs.Remove(pdfBldg))
        {
            _entry.BuildingPdfsToBeDeleted.Add(pdfBldg);
            IsDirty = true;
        }
    }
    private static bool CanDeleteBuildingPdf(Models.Rent.Residentials.PdfBldg pdfBldg)
    {
        return pdfBldg is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenBuildingBlobDirectory))]
    private void OpenBuildingBlobDirectory()
    {
        if (Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _mainViewModel.EntryDataDirectoryPath);
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
        if (Directory.Exists(_mainViewModel.EntryDataDirectoryPath))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region == Unit related commands ==

    // Add New Modal window command
    [RelayCommand]
    private void AddNewUnit()
    {
        _mainViewModel.Unit.SetEditUnit(new UnitResidential(Guid.CreateVersion7().ToString("N")));

        _mainViewModel.GoToUnitShellPageCommand.Execute(this);
    }

    [RelayCommand(CanExecute = nameof(EditSelectedUnitCanExecute))]
    private void EditSelectedUnit(UnitResidential room)
    {
        if (room is null) return;

        _mainViewModel.Unit.SetEditUnit(room);

        _mainViewModel.GoToUnitShellPageCommand.Execute(this);

    }
    public bool EditSelectedUnitCanExecute()
    {
        if (SelectedRoom is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DupeSelectedUnitCanExecute))]
    private void DupeSelectedUnit(UnitResidential room)
    {
        if (room is null) return;

        //
    }
    public bool DupeSelectedUnitCanExecute()
    {
        if (SelectedRoom is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteSelectedUnitCanExecute))]
    private void DeleteSelectedUnit(UnitResidential room)
    {
        if (room is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (Rooms.Remove(room))
        {
            // No. Don't
            //if (_entry.Rooms.Remove(room)) { }
            _entry.RoomsToBeDeleted.Add(room);
            IsDirty = true;
        }

        // Make sure to set it to null.
        SelectedRoom = null;
    }
    public bool DeleteSelectedUnitCanExecute()
    {
        if (SelectedRoom is null) return false;
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
        if (_mainViewModel.ResidentialDialogService is null)
        {
            Debug.WriteLine("_mainViewModel.ResidentialDialogService is null");
            return;
        }

        var railLine = await _mainViewModel.ResidentialDialogService.ShowRailLineSelectDialog();

        if (railLine is not null)
        {
            SelectedRailLine1 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect1))]
    public async Task ShowRailStationSelect1()
    {
        if (_mainViewModel.ResidentialDialogService is null)
        {
            Debug.WriteLine("_mainViewModel.ResidentialDialogService is null");
            return;
        }

        if (SelectedRailLine1 is null)
        {
            return;
        }

        var railStation = await _mainViewModel.ResidentialDialogService.ShowRailStationSelectDialog(SelectedRailLine1.LineCode);

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
