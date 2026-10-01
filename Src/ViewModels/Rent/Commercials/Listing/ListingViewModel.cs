using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Services.Contracts;
using CommunityToolkit.Mvvm.Messaging;

namespace ZumenSearch.ViewModels.Rent.Commercials.Listing;

public sealed partial class ListingViewModel : ObservableRecipient
{
    private const string BasicPageName = "ZumenSearch.Views.Rent.Commercials.Listing.BasicPage";

    private readonly Models.Rent.Commercials.Listing.Listing _unit;
    private readonly IDataAccessService _dataAccessService;
    private readonly INavigationGenericService _navigationService;

    public ListingViewModel(
        Models.Rent.Commercials.Listing.Listing unit,
        INavigationGenericService navigationService,
        IDataAccessService dataAccessService)
    {
        _unit = unit ?? throw new ArgumentNullException(nameof(unit));
        _navigationService = navigationService;
        _dataAccessService = dataAccessService;

        PopulateValues();
        IsDirty = false;
        IsActive = true;
    }

    #region == Properties ==

    public string WindowTitle
    {
        get
        {
            var title = $"{field}：{_unit.PropertyName}";
            if (!string.IsNullOrWhiteSpace(Name))
            {
                title += $"：{Name}";
            }

            return $"{title}：{(_unit.Status == EnumEntryStatus.New ? "新規" : "編集")}";
        }
        set => OnPropertyChanged();
    } = "賃貸事業用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; } =
    [
        new()
        {
            Name = "募集物件",
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

    public bool IsPropertyUnitOwnership => _unit.IsPropertyUnitOwnership;

    public string Name
    {
        get => field ?? string.Empty;
        set
        {
            var name = value?.Trim() ?? string.Empty;
            if (SetProperty(ref field, name))
            {
                IsNameHasError = false;
                WindowTitle = string.Empty;
                IsDirty = true;
            }
        }
    }

    [ObservableProperty] public partial string Chinryou { get; set; } = string.Empty;
    [ObservableProperty] public partial string KyouekiFee { get; set; } = string.Empty;
    [ObservableProperty] public partial string Shikikin { get; set; } = string.Empty;
    [ObservableProperty] public partial string ShikikinUnit { get; set; } = "ヵ月";
    [ObservableProperty] public partial string Reikin { get; set; } = string.Empty;
    [ObservableProperty] public partial string ReikinUnit { get; set; } = "ヵ月";
    [ObservableProperty] public partial string RenewalFee { get; set; } = string.Empty;
    [ObservableProperty] public partial string RenewalFeeUnit { get; set; } = "ヵ月";
    [ObservableProperty] public partial string RecontractFee { get; set; } = string.Empty;
    [ObservableProperty] public partial string RecontractFeeUnit { get; set; } = "円";
    [ObservableProperty] public partial string FloorArea { get; set; } = string.Empty;
    [ObservableProperty] public partial string Usage { get; set; } = "未指定";
    [ObservableProperty] public partial string BusinessHours { get; set; } = string.Empty;
    [ObservableProperty] public partial bool ParkingAvailable { get; set; }
    [ObservableProperty] public partial string OtherConditions { get; set; } = string.Empty;
    [ObservableProperty] public partial string Remarks { get; set; } = string.Empty;

    #endregion

    #region == Public Methods ==

    public void CleanUp()
    {
        /*
        // TODO: ?
        foreach (var item in Pictures)
        {
            item.PropertyChanged -= OnPicturePropertyChanged;
        }

        // TODO: ?
        Pictures.Clear();
        */

        // Unsubscribe
        //WeakReferenceMessenger.Default.UnregisterAll(this);
        //or
        this.IsActive = false;
    }

    public void DiscardChanges()
    {
        // lator
        //DiscardUnsavedFiles();

        //_room = null;
        IsDirty = false;
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues()
    {
        Name = _unit.Name;
        Chinryou = Format(_unit.Chinryou);
        KyouekiFee = Format(_unit.KyouekiFee);
        Shikikin = Format(_unit.Shikikin);
        ShikikinUnit = _unit.ShikikinUnit;
        Reikin = Format(_unit.Reikin);
        ReikinUnit = _unit.ReikinUnit;
        RenewalFee = Format(_unit.RenewalFee);
        RenewalFeeUnit = _unit.RenewalFeeUnit;
        RecontractFee = Format(_unit.RecontractFee);
        RecontractFeeUnit = _unit.RecontractFeeUnit;
        FloorArea = Format(_unit.FloorArea);
        Usage = _unit.Usage;
        BusinessHours = _unit.BusinessHours;
        ParkingAvailable = _unit.ParkingAvailable;
        OtherConditions = _unit.OtherConditions;
        Remarks = _unit.Remarks;
        WindowTitle = string.Empty;
    }

    private void SetValues()
    {
        _unit.Name = Name;
        _unit.Chinryou = ParseDecimal(Chinryou);
        _unit.KyouekiFee = ParseDecimal(KyouekiFee);
        _unit.Shikikin = ParseDecimal(Shikikin);
        _unit.ShikikinUnit = ShikikinUnit;
        _unit.Reikin = ParseDecimal(Reikin);
        _unit.ReikinUnit = ReikinUnit;
        _unit.RenewalFee = ParseDecimal(RenewalFee);
        _unit.RenewalFeeUnit = RenewalFeeUnit;
        _unit.RecontractFee = ParseDecimal(RecontractFee);
        _unit.RecontractFeeUnit = RecontractFeeUnit;
        _unit.FloorArea = ParseDecimal(FloorArea);
        _unit.Usage = Usage;
        _unit.BusinessHours = BusinessHours;
        _unit.ParkingAvailable = ParkingAvailable;
        _unit.OtherConditions = OtherConditions;
        _unit.Remarks = Remarks;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage = "区画名（必須項目）が入力されていません。保存出来ませんでした。";
            IsNameHasError = true;
            return false;
        }

        if (new StringInfo(Name).LengthInTextElements > 100)
        {
            InfoBarErrorMessage = "区画名は100文字以内で入力してください。";
            IsNameHasError = true;
            return false;
        }

        IsNameHasError = false;
        return true;
    }

    private bool SaveToNew()
    {
        WeakReferenceMessenger.Default.Send(
            new Models.Messenger.ListingUpdatedMessage(_unit));

        return true;
    }

    private bool SaveAsUpdate()
    {
        var result = _dataAccessService.UpsertRentCommercialListing(
            _unit.PropertyId,
            _unit);

        if (result.IsError)
        {
            InfoBarErrorMessage = string.Join(
                Environment.NewLine,
                new[]
                {
                result.Error.Title,
                result.Error.Message,
                result.Error.Description,
                result.Error.Operation,
                result.Error.MethodName
                }.Where(message => !string.IsNullOrWhiteSpace(message)));

            IsInfoBarErrorOpen = true;
            return false;
        }

        _unit.Status = EnumEntryStatus.Saved;
        _unit.PropertyStatus = EnumEntryStatus.Saved;
        _unit.IsModified = false;

        WeakReferenceMessenger.Default.Send(
            new Models.Messenger.ListingUpdatedMessage(_unit));

        return true;
    }

    private static string Format(decimal value) =>
        value == 0
            ? string.Empty
            : value.ToString(CultureInfo.CurrentCulture);

    private static decimal ParseDecimal(string? value)
    {
        var normalized = Helpers.Common.ReplaceZenkakuNumbers(value ?? string.Empty);

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out var result) && result >= 0
            ? result
            : 0;
    }

    #endregion

    #region == Commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        if (!IsDirty)
        {
            return;
        }

        if (!ValidateName())
        {
            IsInfoBarErrorOpen = true;
            _navigationService.NavigateTo(BasicPageName, this);
            return;
        }

        SetValues();

        bool saveResult;

        if (_unit.PropertyStatus == EnumEntryStatus.New)
        {
            saveResult = SaveToNew();
        }
        else
        {
            saveResult = SaveAsUpdate();
        }

        if (!saveResult)
        {
            return;
        }

        IsDirty = false;
        IsInfoBarErrorOpen = false;
        WindowTitle = string.Empty;

        /*
        var result = _dataAccessService.UpsertRentCommercialListing(
            _unit.PropertyId,
            _unit);

        if (result.IsError)
        {
            InfoBarErrorMessage = string.Join(
                Environment.NewLine,
                new[]
                {
                    result.Error.Title,
                    result.Error.Message,
                    result.Error.Description,
                    result.Error.Operation,
                    result.Error.MethodName
                }.Where(message => !string.IsNullOrWhiteSpace(message)));

            IsInfoBarErrorOpen = true;
            return;
        }

        _unit.Status = EnumEntryStatus.Saved;
        _unit.PropertyStatus = EnumEntryStatus.Saved;
        _unit.IsModified = false;
        IsDirty = false;
        IsInfoBarErrorOpen = false;
        WindowTitle = string.Empty;
        */
    }

    private bool CanSave() => IsDirty;

    [RelayCommand]
    private void OpenPropertyEditorWindow()
    {
        var vm = App.GetService<ViewModels.MainViewModel>();

        if (vm.EditRentCommercialFromIdCommand.CanExecute(_unit.PropertyId))
        {
            //await vm.EditRentCommercialFromIdCommand(_unit.PropertyId);
            vm.EditRentCommercialFromIdCommand.Execute(_unit.PropertyId);
        }
    }

    #endregion

}