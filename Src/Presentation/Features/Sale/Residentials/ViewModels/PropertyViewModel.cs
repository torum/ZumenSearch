using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Application.Configuration;
using ZumenSearch.Application.Results;
using ZumenSearch.Presentation.Shell.Models;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Infrastructure.Configuration.Contracts;
using ZumenSearch.Infrastructure.DataAccess.Contracts;
using ZumenSearch.Presentation.Services.Contracts;

namespace ZumenSearch.Presentation.Features.Sale.Residentials.ViewModels;

public sealed partial class PropertyViewModel : ObservableRecipient, IDisposable
{
    private const string BasicPageName = NavigationKeys.SaleResidentialPropertyBasic;
    private readonly ZumenSearch.Domain.Entities.Sale.Residentials.Property _building;
    private readonly INavigationGenericService _navigationService;
    private readonly IDialogGenericService _dialogService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDataAccessService _dataAccessService;
    private readonly CancellationTokenSource _cts = new();

    public string WindowTitle
    {
        get
        {
            var title = $"{field}";

            if (!string.IsNullOrWhiteSpace(Name))
            {
                title = $"{title}：{Name}";
            }

            title = _building.Status == EntityStatus.New
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

    public ObservableCollection<ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyContextType> Kinds { get; } =
    [
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.Apartment),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.Mansion),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.House),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.TerraceHouse),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.TownHouse),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.ShareHouse),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.Dormitory)
    ];

    public ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyContextType SelectedKind
    {
        get => field ??
            new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public ObservableCollection<ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyStructure> Structures { get; } =
    [
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.Wood),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.Block),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.LightSteel),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.Steel),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.RC),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.SRC),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.ALC),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.PC),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.HPC),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.RB),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.CFT),
        new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.Other)
    ];

    public ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyStructure SelectedStructure
    {
        get => field ??
            new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.Unspecified);
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
        ZumenSearch.Domain.Entities.Sale.Residentials.Property building,
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
        //IsActive = true;
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
            ?? new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.PropertyKindType.Unspecified);

        SelectedStructure =
            Structures.FirstOrDefault(
                item => item.Key == _building.PropertyStructure.Key)
            ?? new(ZumenSearch.Presentation.Features.Sale.Residentials.Models.StructureType.Unspecified);

        IsUnitOwnership = _building.IsUnitOwnership;

        FloorCountAboveGround =
            _building.FloorCountAboveGround == 0
                ? string.Empty
                : _building.FloorCountAboveGround.ToString(CultureInfo.InvariantCulture);

        FloorCountBasement =
            _building.FloorCountBasement == 0
                ? string.Empty
                : _building.FloorCountBasement.ToString(CultureInfo.InvariantCulture);

        TotalUnitCount =
            _building.TotalUnitCount == 0
                ? string.Empty
                : _building.TotalUnitCount.ToString(CultureInfo.InvariantCulture);

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
        _building.SetName(Name);
        _building.BuildingKind = SelectedKind;
        _building.PropertyStructure = SelectedStructure;
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
            ZumenSearch.Presentation.Utilities.Common.ReplaceZenkakuNumbers(FudousanId);

        _building.FudousanIdAdditionalCode =
            ZumenSearch.Presentation.Utilities.Common.ReplaceZenkakuNumbers(
                FudousanIdAdditionalCode);

        _building.Remarks = Remarks;
    }

    private static int ParseNonNegativeInteger(string value)
    {
        var normalized =
            ZumenSearch.Presentation.Utilities.Common.ReplaceZenkakuNumbers(value);

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
    public async Task Save()
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

        var result = await Task.Run(() => _dataAccessService.UpsertSaleResidential(_building), _cts.Token);

        if (result.IsError)
        {
            Debug.WriteLine(
                result.Error.Title + Environment.NewLine +
                result.Error.Message + Environment.NewLine +
                result.Error.Description + Environment.NewLine +
                result.Error.Operation + Environment.NewLine +
                result.Error.MethodName + Environment.NewLine +
                result.Error.FullDump);

            InfoBarErrorMessage =
                result.Error.Title + Environment.NewLine +
                result.Error.Message + Environment.NewLine +
                result.Error.Description + Environment.NewLine +
                result.Error.Operation + Environment.NewLine +
                result.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        IsDirty = false;
        _building.SetStatus(EntityStatus.Saved);
        _building.SetIsModified(false);
        IsInfoBarErrorOpen = false;
        WindowTitle = string.Empty;
    }

    private bool CanSave()
    {
        return IsDirty;
    }

    public void Dispose()
    {
        _cts?.Dispose();

        GC.SuppressFinalize(this);
    }
}
