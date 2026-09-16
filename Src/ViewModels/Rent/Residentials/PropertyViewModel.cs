using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public sealed partial class PropertyViewModel : ObservableRecipient, IRecipient<ListingUpdatedMessage>, IRecipient<ListingWindowClosedMessage>, IRecipient<ListingDeletedMessage>
{
    #region == Public Properties ==

    // TODO: Do I need this?
    //public string Id => _id;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    #region == 画面表示関連 ==

    public string WindowTitle
    {
        get
        {
            var str = $"{field}";

            if (!string.IsNullOrEmpty(Name))
            {
                str = $"{field}：{Name}";
            }

            if (_building.Status == EnumEntryStatus.New)
            {
                str = $"{str}：(新規)";
            }
            else
            {
                str = $"{str}：(編集)";
            }

            return str;
        }
        set
        {
            OnPropertyChanged();
        }
    } = "賃貸住居用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "建物", Page = typeof(Views.Rent.Residentials.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.BasicPage).FullName! }
    ];

    #endregion

    #region == エラー関連 ==

    /*
    // TODO: update this to hold more info such as page so that it can be navigated to the page.
    [ObservableProperty]
    public partial bool HasErrors { get; private set; }
    */

    // InfoBarError is researved only for unsavable error.
    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsNameHasError { get; private set; }

    #endregion

    #region == 基本プロパティ == 

    // 物件名
    public string Name
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;

                ValidateName();

                // Update title with dummy value.
                WindowTitle = string.Empty;

                // Moved to after the save.
                //_selectedSearchResult?.Name = value; // Update the selected search result's name if it exists
            }
        }
    }

    // 物件種別
    public ObservableCollection<Kind> Kinds =
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new Models.Rent.Residentials.Kind(EnumResidentialKinds.Apartment),
        new Models.Rent.Residentials.Kind(EnumResidentialKinds.Mansion),
        new Models.Rent.Residentials.Kind(EnumResidentialKinds.House),
        new Models.Rent.Residentials.Kind(EnumResidentialKinds.TerraceHouse),
        new Models.Rent.Residentials.Kind(EnumResidentialKinds.TownHouse),
        new Models.Rent.Residentials.Kind(EnumResidentialKinds.ShareHouse),
        new Models.Rent.Residentials.Kind(EnumResidentialKinds.Dormitory)
    ];

    public Kind SelectedKind
    {
        get => field ?? new(EnumResidentialKinds.Unspecified);
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

                IsUnitOwnershipVisible = !value;

                // If this is set, then show/hide the owner and zumen from shell menu.
                //EventIsUnitOwnershipChanged?.Invoke(this, field);
                WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyIsUnitOwnershipChangedMessage(value));
            }
        }
    }

    [ObservableProperty]
    public partial bool IsUnitOwnershipVisible { get; private set; } = true;

    // 建物構造
    public ObservableCollection<Structure> Structures =
    [
        //new Structure(EnumStructures.Unspecified.ToString(), "未指定"),
        new Models.Rent.Residentials.Structure(EnumStructures.Wood),
        new Models.Rent.Residentials.Structure(EnumStructures.Block),
        new Models.Rent.Residentials.Structure(EnumStructures.LightSteel),
        new Models.Rent.Residentials.Structure(EnumStructures.Steel),
        new Models.Rent.Residentials.Structure(EnumStructures.RC),
        new Models.Rent.Residentials.Structure(EnumStructures.SRC),
        new Models.Rent.Residentials.Structure(EnumStructures.ALC),
        new Models.Rent.Residentials.Structure(EnumStructures.PC),
        new Models.Rent.Residentials.Structure(EnumStructures.HPC),
        new Models.Rent.Residentials.Structure(EnumStructures.RB),
        new Models.Rent.Residentials.Structure(EnumStructures.CFT),
        new Models.Rent.Residentials.Structure(EnumStructures.Other)
    ];

    public Structure SelectedStructure
    {
        get => field ?? new(EnumStructures.Unspecified);
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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            /*
            text = Helpers.Common.ReplaceZenkakuNumbers(text);

            if (string.IsNullOrEmpty(text))
            {
                // Do nothing.
                return;
            }
            */

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex =  new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                Debug.WriteLine($"AboveGroundFloorCount: not * digits");
                // TODO: show err?
            }
            /*
            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                // Do nothing.
                return;
            }
            */
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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                Debug.WriteLine($"BasementFloorCount: not * digits");
                // TODO: show err?
            }
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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,5}$"); // 5 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,5})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                Debug.WriteLine($"TotalUnitCount: not * digits");
                // TODO: show err?
            }
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

            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            var regex = new Regex(@"^(?:\d{13})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"FudousanId: not 13 digits");
                // TODO: show err?
            }

            /*
            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                // Do nothing.
                return;
            }
            */
        }
    }

    // 特定コード（４桁）建物全体は0000
    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty; // keep non-null empty string.
        set
        {
            // TODO: check （４桁）

            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            var regex = new Regex(@"^(?:\d{4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"FudousanIdAdditionalCode: not 4 digits");
                // TODO: show err?
            }
            /*
            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                // Do nothing.
                return;
            }
            */
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
            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"EkiToho1: not * digits");
                // TODO: show err?
            }
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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"BusJyousya1: not * digits");
                // TODO: show err?
            }
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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"BusJyousya1: not * digits");
                // TODO: show err?
            }
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
        new Models.Rent.Residentials.ElectricKind(Models.Rent.Residentials.Property.EnumElectricKind.AllElectric, "オール電化"),
        new Models.Rent.Residentials.ElectricKind(Models.Rent.Residentials.Property.EnumElectricKind.Unspecified, "未指定")
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
    } = new Models.Rent.Residentials.ElectricKind(Models.Rent.Residentials.Property.EnumElectricKind.Unspecified, "未指定");

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
        new Models.Rent.Residentials.KanriShutai(Models.Rent.Residentials.Property.EnumKanriShutai.Unspecified, "未指定"),
        new Models.Rent.Residentials.KanriShutai(Models.Rent.Residentials.Property.EnumKanriShutai.Jisya, "自社管理"),
        new Models.Rent.Residentials.KanriShutai(Models.Rent.Residentials.Property.EnumKanriShutai.Tasya, "他社管理"),
        new Models.Rent.Residentials.KanriShutai(Models.Rent.Residentials.Property.EnumKanriShutai.Kashinushi, "貸主管理")
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
                IsKanriUnspecified = field.Key == Models.Rent.Residentials.Property.EnumKanriShutai.Unspecified;
                IsKanriJisya = field.Key == Models.Rent.Residentials.Property.EnumKanriShutai.Jisya;
                IsKanriTasya = field.Key == Models.Rent.Residentials.Property.EnumKanriShutai.Tasya;
                IsKanriKashinushi = field.Key == Models.Rent.Residentials.Property.EnumKanriShutai.Kashinushi;
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

    public ObservableCollection<Models.Rent.Residentials.Picture> BuildingPictures
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

    public ObservableCollection<Models.Rent.Residentials.Pdf> BuildingPdfs
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

    public ObservableCollection<Models.Rent.Residentials.Listing.Listing> Rooms
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
    public partial Models.Rent.Residentials.Listing.Listing? SelectedRoom { get; set; }
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
    private readonly Models.Rent.Residentials.Property _building;

    private readonly string _propertyDataDirectoryPath = string.Empty;

    // Child windows.
    public readonly List<Views.Rent.Residentials.Listing.EditorWindow> ChildEditorList = [];

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedBuildingPictureFileList = [];
    private readonly List<string> _unsavedBuildingPdfFileList = [];
    private readonly List<string> _unsavedBuildingPdfThumbnailFileList = [];

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<Models.Rent.Residentials.Listing.Listing, Views.Rent.Residentials.Listing.ShellPage> _shellFactory;
    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public PropertyViewModel(
        Models.Rent.Residentials.Property building, 
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IAbstractFactory<Models.Rent.Residentials.Listing.Listing, Views.Rent.Residentials.Listing.ShellPage> shellFactory, 
        IDispatcherService dispatcherService, 
        IDataAccessService dataAccessService, 
        IDataAccessLocationService dataAccessLocationService)
    {
        _building = building;
        _id = building.Id;

        _navigationService = navigationService; // _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.RoomListPage", this, new DrillInNavigationTransitionInfo());
        _dialogService = dialogService;

        _shellFactory = shellFactory;

        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        // TODO:
        _propertyDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, "Rent"), "Residential"), _id);

        // Update title with dummy value.
        WindowTitle = string.Empty;

        try
        {
            PopulateValues();

            // Reset errors
            IsNameHasError = false;
            // TODO: more.

            //HasErrors = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BldgViewModel: {ex}");
        }
        finally
        {
            IsDirty = false;
        }

        // Ready to receive messages.
        this.IsActive = true;
    }

    #region == Messages ==

    public void Receive(ListingUpdatedMessage listing)
    {
        //Debug.WriteLine("Received ListingUpdatedMessage @PropertyViewModel");

        var room = listing.Value;
        if (room is null)
        {
            return;
        }

        var existingRoom = this.Rooms.FirstOrDefault(r => r.Id == room.Id);
        if (existingRoom is not null)
        {
            // Update existing room
            var index = this.Rooms.IndexOf(existingRoom);
            this.Rooms[index] = room;
        }
        else
        {
            // Add new room
            this.Rooms.Add(room);
        }

        if (room.Status == EnumEntryStatus.New)
        {
            //Debug.WriteLine("(room.ListingStatus == EnumListingStatus.New) @PropertyViewModel");
            IsDirty = true;
        }
    }

    public void Receive(ListingDeletedMessage listingId)
    {
        var id = listingId.Value;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var room = Rooms.FirstOrDefault(r => r.Id == id);
        if (room is null) return;
        Rooms.Remove(room);
    }

    public void Receive(ListingWindowClosedMessage window)
    {
        var ewin = window.Value;

        if (ewin is null)
        {
            return;
        }

        this.ChildEditorList.Remove(ewin);
    }

    #endregion

    #region == Public Methods ==

    public void DiscardChanges()
    {
        DiscardUnsavedFiles();

        IsDirty = false;
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues()
    {
        // Basics

        Name = _building.Name;

        //SelectedKind = _building.BuildingKind;
        var kindkey = Kinds.FirstOrDefault(k => k.Key == _building.BuildingKind.Key);
        SelectedKind = kindkey is null ? new(EnumResidentialKinds.Unspecified) : kindkey;

        IsUnitOwnership = _building.IsUnitOwnership;

        //IsUnitOwnershipVisible = !IsUnitOwnership;

        //SelectedStructure = _building.BuildingStructure;
        var Structurekey = Structures.FirstOrDefault(k => k.Key == _building.BuildingStructure.Key);
        SelectedStructure = Structurekey is null ? new(EnumStructures.Unspecified) : Structurekey;

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
        BuildingPictures = new ObservableCollection<Models.Rent.Residentials.Picture>(_building.Pictures); // create a copy.

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
                foreach (Models.Rent.Residentials.Picture item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from BuildingPictures. @CollectionChanged in PopulateEntryValues of Bldg.MainViewModel");
                    IsDirty = true;

                    item.PropertyChanged -= OnBuildingPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Picture item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to BuildingPictures. @CollectionChanged in PopulateEntryValues of Bldg.MainViewModel");
                    IsDirty = true;

                    item.PropertyChanged += OnBuildingPicturePropertyChanged;
                }
            }
        };

        // PDFs
        BuildingPdfs = new ObservableCollection<Models.Rent.Residentials.Pdf>(_building.Pdfs); // create a copy.

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
                foreach (Models.Rent.Residentials.Pdf item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from BuildingPdfs, @CollectionChanged in PopulateEntryValues of Bldg.MainViewModel");
                    IsDirty = true;

                    item.PropertyChanged -= OnBuildingPdfPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Pdf item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to BuildingPdfs. @CollectionChanged in PopulateEntryValues of Bldg.MainViewModel");
                    IsDirty = true;

                    item.PropertyChanged += OnBuildingPdfPropertyChanged;
                }
            }
        };

        // Rooms
        Rooms = new ObservableCollection<Models.Rent.Residentials.Listing.Listing>(_building.Rooms); // create a copy.

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
                foreach (Models.Rent.Residentials.Listing.Listing item in e.OldItems)
                {
                    //Debug.WriteLine($"Item {item.Id} Removed from Rooms. @Rooms.CollectionChanged in PopulateEntryValues of Bldg.MainViewModel");
                    //IsDirty = true; // Don't

                    item.PropertyChanged -= OnRoomPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Listing.Listing item in e.NewItems)
                {
                    //Debug.WriteLine($"Item {item.Id} Added to Rooms. @Rooms.CollectionChanged in PopulateEntryValues of Bldg.MainViewModel");
                    //IsDirty = true; // don't

                    item.PropertyChanged += OnRoomPropertyChanged;
                }
            }
        };

        //Debug.WriteLine($"PopulateEntryValues: Completed populating values from Entry to VM. Entry ID: {_building.Id}, Rooms Count: {Rooms.Count}");
    }

    private void OnBuildingPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Picture picBldg)
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
        if (sender is not Models.Rent.Residentials.Pdf pdfBldg)
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
        if (sender is not Models.Rent.Residentials.Listing.Listing room)
        {
            Debug.WriteLine("OnRoomPropertyChanged returned non Room.");
            return;
        }

        //Debug.WriteLine($"Property {e.PropertyName} changed");

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

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage = "物件名（必須項目）が入力されていません。保存出来ませんでした。";

            IsNameHasError = true;

            //HasErrors = true;

            return false;
        }

        var realLength = new StringInfo(Name).LengthInTextElements;
        if (realLength > 100)
        {
            InfoBarErrorMessage = "物件名は100文字以内で入力してください。保存出来ませんでした。";

            IsNameHasError = true;

            //HasErrors = true;

            return false;
        }

        IsNameHasError = false;
        return true;
    }

    private void SetValues()
    {
        if (!IsDirty)
        {
            return;
        }

        // 物件名
        _building.Name = Name;

        _building.BuildingKind = SelectedKind;
        _building.IsUnitOwnership = IsUnitOwnership;
        _building.BuildingStructure = SelectedStructure;
        _building.AboveGroundFloorCount = int.TryParse(Helpers.Common.ReplaceZenkakuNumbers(AboveGroundFloorCount), out var aboveGroundFloorCount) ? aboveGroundFloorCount : 0; //Convert.ToInt32(AboveGroundFloorCount)
        _building.BasementFloorCount = int.TryParse(Helpers.Common.ReplaceZenkakuNumbers(BasementFloorCount), out var basementFloorCount) ? basementFloorCount : 0;
        _building.TotalUnitCount = int.TryParse(Helpers.Common.ReplaceZenkakuNumbers(TotalUnitCount), out var totalUnitCount) ? totalUnitCount : 0;
        _building.BuiltYearAndMonth = BuiltYearAndMonth ?? new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _building.FudousanId = Helpers.Common.ReplaceZenkakuNumbers(FudousanId);
        _building.FudousanIdAdditionalCode = Helpers.Common.ReplaceZenkakuNumbers(FudousanIdAdditionalCode);
        _building.Remarks = Remarks;

        // TODO: Set other properties
        // TODO: Don't forget to check if Helpers.Common.ReplaceZenkakuNumbers is needed.

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

        // TODO: Set other properties
        // TODO: Don't forget to check if Helpers.Common.ReplaceZenkakuNumbers is needed.
        



        // Reset main ThumbnailImageFilePath here.
        _building.ThumbnailImageFilePath = string.Empty;

        // 写真
        _building.Pictures = BuildingPictures;
        var thumbImg = BuildingPictures.FirstOrDefault(i => i.IsMain == true);
        if (thumbImg is not null)
        {
            _building.ThumbnailImageFilePath = thumbImg.ImageLocation;
        }

        // 図面
        _building.Pdfs = BuildingPdfs;
        if (string.IsNullOrWhiteSpace(_building.ThumbnailImageFilePath))
        {
            var thumbPdf = BuildingPdfs.FirstOrDefault(i => i.IsMain == true);
            if (thumbPdf is not null)
            {
                _building.ThumbnailImageFilePath = thumbPdf.ThumbnailLocation;
            }
        }

        // 部屋
        _building.Rooms = Rooms;

    }

    private bool SaveAsNew()
    {
        var resInsert = _dataAccessService.InsertRentResidential(_building);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on insert. @SaveAsNew in Residentials PropertyViewModel");

            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: fix format.
            var errText = resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent;
            InfoBarErrorMessage = errText;
            IsInfoBarErrorOpen = true;

            return false;
        }
        else
        {
            Debug.WriteLine("No errors on insert.");
            return true;
        }
    }

    private bool SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(_building);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on update. @SaveAsUpdate in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: fix format.
            var errText = resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent;
            InfoBarErrorMessage = errText;
            IsInfoBarErrorOpen = true;

            return false;
        }
        else
        {
            Debug.WriteLine("No errors on update.");
            return true;
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

        if (_building.Status == EnumEntryStatus.New)
        {
            if (Directory.Exists(_propertyDataDirectoryPath))
            {
                Debug.WriteLine($"Deleting folder: {_propertyDataDirectoryPath}");
                Directory.Delete(_propertyDataDirectoryPath, true);
            }
        }
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
        if (!ValidateName())
        {
            //InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            IsInfoBarErrorOpen = true;

            if (!_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.BasicPage"))
            {
                _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.BasicPage", this);
            }

            return;
        }

        // TODO: more.

        SetValues();

        bool saveResult;

        if (_building.Status == EnumEntryStatus.New)
        {
            saveResult = SaveAsNew();
        }
        else
        {
            saveResult = SaveAsUpdate();
        }

        if (saveResult)
        {
            IsDirty = false;

            _building.IsModified = false;
            _building.Status = EnumEntryStatus.Saved;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

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

            // Clear rooms pic and pdfs
            if (_building.RoomsToBeDeleted.Count > 0)
            {
                foreach (var room in _building.RoomsToBeDeleted)
                {
                    foreach (var roomPic in room.Pictures)
                    {
                        File.Delete(roomPic.ImageLocation);
                    }

                    foreach (var roomPdf in room.Pdfs)
                    {
                        File.Delete(roomPdf.PdfLocation);
                        File.Delete(roomPdf.ThumbnailLocation);
                    }
                }
                _building.RoomsToBeDeleted.Clear();
            }

            _unsavedBuildingPictureFileList.Clear();
            _unsavedBuildingPdfThumbnailFileList.Clear();
            _unsavedBuildingPdfFileList.Clear();

            // Jjust in case.
            _building.Status = EnumEntryStatus.Saved;
            _building.IsModified = false;

            foreach (var room in Rooms)
            {
                room.PropertyName = Name;
                room.PropertyStatus = EnumEntryStatus.Saved;
                room.Status = EnumEntryStatus.Saved;
            }

            // Just in case.
            foreach (var room in _building.Rooms)
            {
                room.PropertyName = Name;
                room.PropertyStatus = EnumEntryStatus.Saved;
                room.Status = EnumEntryStatus.Saved;
            }

            WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyUpdatedMessage(_building as Models.Base.PropertyBase));
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

    #region == Pictures and Pdfs related commands ==

    [RelayCommand(CanExecute = nameof(CanAddNewBuildingPictures))]
    private async Task AddNewBuildingPictures(List<string> filePathList)
    {
        // File open is handleed in the code behind since this ViewModel does not know about the Window which is required for the Picker.

        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        //Debug.WriteLine($"destDirectory={_building.PropertyDataDirectoryPath}  @SetNewBuildingPicturesAsync()");

        if (!Directory.Exists(_propertyDataDirectoryPath))
        {
            Directory.CreateDirectory(_propertyDataDirectoryPath);
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
            var destFilePath = Path.Combine(_propertyDataDirectoryPath, newId + extension);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = File.Create(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.Picture(newId, destFilePath)
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
    private static bool CanAddNewBuildingPictures(List<string> filePathList)
    {
        if (filePathList.Count < 1)
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPicture))]
    private void DeleteBuildingPicture(Models.Rent.Residentials.Picture picBldg)
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
    private static bool CanDeleteBuildingPicture(Models.Rent.Residentials.Picture picBldg)
    {
        return picBldg is not null;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewBuildingPdfs))]
    private async Task AddNewBuildingPdfs(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_propertyDataDirectoryPath}  @AddNewBuildingPdfs()");

        if (!Directory.Exists(_propertyDataDirectoryPath))
        {
            Directory.CreateDirectory(_propertyDataDirectoryPath);
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
                var thumbnailDestFilePath = Path.Combine(_propertyDataDirectoryPath, newId + ".bmp");

                using var destinationStream = File.Create(thumbnailDestFilePath);
                using var managedSourceStream = stream.AsStreamForRead();
                await managedSourceStream.CopyToAsync(destinationStream);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPdfThumbnailFileList.Add(thumbnailDestFilePath);

                var pdfDestFilePath = Path.Combine(_propertyDataDirectoryPath, newId + extension);
                File.Copy(filePath, pdfDestFilePath);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPdfFileList.Add(pdfDestFilePath);

                var pdf = new Models.Rent.Residentials.Pdf(newId, pdfDestFilePath, thumbnailDestFilePath)
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
    private static bool CanAddNewBuildingPdfs(List<string> filePathList)
    {
        if (filePathList.Count < 1)
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPdf))]
    private void DeleteBuildingPdf(Models.Rent.Residentials.Pdf pdfBldg)
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
    private static bool CanDeleteBuildingPdf(Models.Rent.Residentials.Pdf pdfBldg)
    {
        return pdfBldg is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenBuildingBlobDirectory))]
    private void OpenBuildingBlobDirectory()
    {
        if (Directory.Exists(_propertyDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _propertyDataDirectoryPath);
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
        if (Directory.Exists(_propertyDataDirectoryPath))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region == Rooms related commands ==

    // Add New Modal window command
    [RelayCommand]
    private void AddNewRoom() 
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var editorShell = _shellFactory.Create(new Models.Rent.Residentials.Listing.Listing(newId, _building.Id, EnumEntryStatus.New, _building.Status, Name));
        editorShell.ViewModel.IsUnitOwnershipVisible = this.IsUnitOwnership;

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        mainVM.RoomEditorList.Add(editorShell.Window);

        this.ChildEditorList.Add(editorShell.Window);

        if (editorShell.Window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

        editorShell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainVM.RoomEditorWinLeft, mainVM.RoomEditorWinTop, mainVM.RoomEditorWinWidth, mainVM.RoomEditorWinHeight));

        //editorWindow.AppWindow.Show();
        editorShell.Window.Activate();
        editorShell.Window.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand(CanExecute = nameof(EditSelectedRoomCanExecute))]
    private void EditSelectedRoom(Models.Rent.Residentials.Listing.Listing room)
    {
        if (room is null) return;

        var rentId = room.PropertyId;
        var unitId = room.Id;

        if (string.IsNullOrEmpty(rentId))//if (selected == null)
        {
            Debug.WriteLine("EditSelectedUnitCommand executed but no rentId.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

        var mainVM = App.GetService<ViewModels.MainViewModel>();

        // Check if the selected item is already being edited in another window.
        foreach (var editWin in mainVM.RoomEditorList.ToList())
        {
            if (editWin.Id != unitId)
            {
                continue;
            }

            //Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");

            try
            {
                editWin.Activate();

                if (editWin.Content is Views.Rent.Residentials.Listing.ShellPage editShell)
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
                //mainVM.RoomEditorList.Remove(editWin);
                //ChildEditorList.Remove(editWin);
            }
        }

        var editorShell = _shellFactory.Create(room);
        editorShell.ViewModel.IsUnitOwnershipVisible = this.IsUnitOwnership;

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

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

        if (editorWindow.Content is Views.Rent.Residentials.Listing.ShellPage shell)
        {
            shell.Window?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
        }
        editorWindow.AppWindow.MoveInZOrderAtTop();

    }
    public static bool EditSelectedRoomCanExecute(Models.Rent.Residentials.Listing.Listing room)
    {
        if (room is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DupeSelectedRoomCanExecute))]
    private void DupeSelectedRoom(Models.Rent.Residentials.Listing.Listing room)
    {
        if (room is null) return;

        //
    }
    public static bool DupeSelectedRoomCanExecute(Models.Rent.Residentials.Listing.Listing room)
    {
        if (room is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteSelectedRoomCanExecute))]
    private void DeleteSelectedRoom(Models.Rent.Residentials.Listing.Listing room)
    {
        if (room is null)
        {
            return;
        }

        // 
        var mainVM = App.GetService<ViewModels.MainViewModel>();

        // Check if the selected item is already being edited in another window.
        foreach (var editWin in mainVM.RoomEditorList.ToList())
        {
            if (editWin.Id != room.Id)
            {
                continue;
            }

            //Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");

            try
            {
                editWin.Activate();

                if (editWin.Content is Views.Rent.Residentials.Listing.ShellPage editShell)
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
                //mainVM.RoomEditorList.Remove(editWin);
                //ChildEditorList.Remove(editWin);
            }
        }


        // TODO: show dialog to comfirm.

        if (Rooms.Remove(room))
        {
            // No. Don't
            //if (_building.Rooms.Remove(room)) { }
            _building.RoomsToBeDeleted.Add(room);
            IsDirty = true;
        }
    }
    public static bool DeleteSelectedRoomCanExecute(Models.Rent.Residentials.Listing.Listing room)
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
        if (_dialogService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        var railLine = await _dialogService.ShowRailLineSelectDialog();

        if (railLine is not null)
        {
            SelectedRailLine1 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect1))]
    public async Task ShowRailStationSelect1()
    {
        if (_dialogService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        if (SelectedRailLine1 is null)
        {
            return;
        }

        var railStation = await _dialogService.ShowRailStationSelectDialog(SelectedRailLine1.LineCode);

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
