using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Sale.Residentials;

public sealed partial class PropertyViewModel : ObservableRecipient
{
    private const string BasicPageName =
        "ZumenSearch.Views.Sale.Residentials.BasicPage";

    private readonly Models.Sale.Residentials.Property _building;
    private readonly INavigationGenericService _navigationService;
    private readonly IDialogGenericService _dialogService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDataAccessService _dataAccessService;

    public string WindowTitle
    {
        get
        {
            var title = $"{field}";

            if (!string.IsNullOrWhiteSpace(Name))
            {
                title = $"{title}：{Name}";
            }

            title = _building.Status == EnumEntryStatus.New
                ? $"{title}：新規"
                : $"{title}：編集";

            return title;
        }
        set => OnPropertyChanged();
    } = "売買住居用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; } =
    [
        new()
        {
            Name = "売買住居用",
            Page = BasicPageName
        },
        new()
        {
            Name = "基本",
            Page = BasicPageName
        }
    ];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsNameHasError { get; private set; }

    public string Name
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                ValidateName();
                WindowTitle = string.Empty;
            }
        }
    }

    public ObservableCollection<Models.Sale.Residentials.Kind> Kinds { get; } =
    [
        new(Models.Sale.Residentials.EnumResidentialKinds.Apartment),
        new(Models.Sale.Residentials.EnumResidentialKinds.Mansion),
        new(Models.Sale.Residentials.EnumResidentialKinds.House),
        new(Models.Sale.Residentials.EnumResidentialKinds.TerraceHouse),
        new(Models.Sale.Residentials.EnumResidentialKinds.TownHouse),
        new(Models.Sale.Residentials.EnumResidentialKinds.ShareHouse),
        new(Models.Sale.Residentials.EnumResidentialKinds.Dormitory)
    ];

    public Models.Sale.Residentials.Kind SelectedKind
    {
        get => field ??
            new(Models.Sale.Residentials.EnumResidentialKinds.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public ObservableCollection<Models.Sale.Residentials.Structure> Structures { get; } =
    [
        new(Models.Sale.Residentials.EnumStructures.Wood),
        new(Models.Sale.Residentials.EnumStructures.Block),
        new(Models.Sale.Residentials.EnumStructures.LightSteel),
        new(Models.Sale.Residentials.EnumStructures.Steel),
        new(Models.Sale.Residentials.EnumStructures.RC),
        new(Models.Sale.Residentials.EnumStructures.SRC),
        new(Models.Sale.Residentials.EnumStructures.ALC),
        new(Models.Sale.Residentials.EnumStructures.PC),
        new(Models.Sale.Residentials.EnumStructures.HPC),
        new(Models.Sale.Residentials.EnumStructures.RB),
        new(Models.Sale.Residentials.EnumStructures.CFT),
        new(Models.Sale.Residentials.EnumStructures.Other)
    ];

    public Models.Sale.Residentials.Structure SelectedStructure
    {
        get => field ??
            new(Models.Sale.Residentials.EnumStructures.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public bool IsUnitOwnership
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

    public string FloorCountAboveGround
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string FloorCountBasement
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string TotalUnitCount
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public DateTimeOffset? BuiltYearAndMonth
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

    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string Remarks
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public PropertyViewModel(
        Models.Sale.Residentials.Property building,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService)
    {
        _building = building;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;

        PopulateValues();
        IsDirty = false;
        IsActive = true;
    }

    public void DiscardChanges()
    {
        PopulateValues();
        IsDirty = false;
        IsInfoBarErrorOpen = false;
    }

    private void PopulateValues()
    {
        Name = _building.Name;

        SelectedKind =
            Kinds.FirstOrDefault(
                item => item.Key == _building.BuildingKind.Key)
            ?? new(Models.Sale.Residentials.EnumResidentialKinds.Unspecified);

        SelectedStructure =
            Structures.FirstOrDefault(
                item => item.Key == _building.BuildingStructure.Key)
            ?? new(Models.Sale.Residentials.EnumStructures.Unspecified);

        IsUnitOwnership = _building.IsUnitOwnership;

        FloorCountAboveGround =
            _building.FloorCountAboveGround == 0
                ? string.Empty
                : _building.FloorCountAboveGround.ToString();

        FloorCountBasement =
            _building.FloorCountBasement == 0
                ? string.Empty
                : _building.FloorCountBasement.ToString();

        TotalUnitCount =
            _building.TotalUnitCount == 0
                ? string.Empty
                : _building.TotalUnitCount.ToString();

        BuiltYearAndMonth =
            _building.BuiltYearAndMonth.Year == 1900
                ? null
                : _building.BuiltYearAndMonth;

        FudousanId = _building.FudousanId;
        FudousanIdAdditionalCode = _building.FudousanIdAdditionalCode;
        Remarks = _building.Remarks;

        WindowTitle = string.Empty;
    }

    private void SetValues()
    {
        _building.Name = Name;
        _building.BuildingKind = SelectedKind;
        _building.BuildingStructure = SelectedStructure;
        _building.IsUnitOwnership = IsUnitOwnership;

        _building.FloorCountAboveGround =
            ParseNonNegativeInteger(FloorCountAboveGround);

        _building.FloorCountBasement =
            ParseNonNegativeInteger(FloorCountBasement);

        _building.TotalUnitCount =
            ParseNonNegativeInteger(TotalUnitCount);

        _building.BuiltYearAndMonth =
            BuiltYearAndMonth
            ?? new DateTimeOffset(
                1900,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        _building.FudousanId =
            Helpers.Common.ReplaceZenkakuNumbers(FudousanId);

        _building.FudousanIdAdditionalCode =
            Helpers.Common.ReplaceZenkakuNumbers(
                FudousanIdAdditionalCode);

        _building.Remarks = Remarks;
    }

    private static int ParseNonNegativeInteger(string value)
    {
        var normalized =
            Helpers.Common.ReplaceZenkakuNumbers(value);

        return int.TryParse(
            normalized,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var result) && result >= 0
            ? result
            : 0;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage =
                "物件名（必須項目）が入力されていません。保存出来ませんでした。";

            IsNameHasError = true;
            return false;
        }

        if (new StringInfo(Name).LengthInTextElements > 100)
        {
            InfoBarErrorMessage =
                "物件名は100文字以内で入力してください。保存出来ませんでした。";

            IsNameHasError = true;
            return false;
        }

        IsNameHasError = false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        if (!IsDirty || !ValidateName())
        {
            if (IsDirty && IsNameHasError)
            {
                IsInfoBarErrorOpen = true;
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return;
        }

        SetValues();

        var result = _dataAccessService.UpsertSaleResidential(_building);

        if (result.IsError)
        {
            InfoBarErrorMessage =
                $"{result.Error.ErrText}{Environment.NewLine}" +
                $"{result.Error.ErrDescription}{Environment.NewLine}" +
                $"{result.Error.ErrPlace}";

            IsInfoBarErrorOpen = true;
            return;
        }

        IsDirty = false;
        _building.IsModified = false;
        _building.Status = EnumEntryStatus.Saved;
        IsInfoBarErrorOpen = false;
        WindowTitle = string.Empty;
    }

    private bool CanSave()
    {
        return IsDirty;
    }
}