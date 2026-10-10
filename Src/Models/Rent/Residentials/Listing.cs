using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.Rent.Residentials;

// 部屋（編集用）
public sealed partial class Listing : ListingBase
{
    public Listing(string id, EntityStatus status, string propertyId, EntityStatus propertyStatus, bool isPropertyUnitOwnership, string propertyName) : base(id, status, propertyId, propertyStatus, PropertyContextType.RentResidential)
    {
        //PropertyId = propertyId;
        IsPropertyUnitOwnership = isPropertyUnitOwnership;
        PropertyName = propertyName;
        //PropertyStatus = propertyStatus;
        //PropertyChanged += (_, _) => SetIsModified(true);
        SetIsModified(false);
    }

    // 物件（建物の名前を保持 - タイトル等に表示）
    public string PropertyName
    {
        get => field ?? string.Empty;
        private set;
    }

    // 部屋が属する建物が区分所有化どうかをここでも保持（部屋を直接開いた際に必要）
    public bool IsPropertyUnitOwnership { get; private set; }

    #region == 部屋の基本情報 ==

    public ListingFloorPlanTypeLabel FloorPlan
    {
        get => field ?? new(FloorPlanType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingFloorPlanExtraTypeLabel FloorPlanExtra
    {
        get => field ?? new(FloorPlanExtraType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingFloorGroundTypeLabel FloorGround
    {
        get => field ?? new(FloorGroundType.AboveGround);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public string FloorExclusiveAreaSqm
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public string FloorNumber
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool IsCornerRoom
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingExposureDirectionTypeLabel SelectedExposureDirectionType
    {
        get => field ?? new(ExposureDirectionType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingAvailabilityStatusTypeLabel SelectedAvailabilityStatusType
    {
        get => field ?? new(AvailabilityStatusType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingAvailabilityMonthTypeLabel SelectedAvailabilityMonthType
    {
        get => field ?? new(AvailabilityMonthType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingAvailabilityPeriodTypeLabel SelectedAvailabilityPeriodType
    {
        get => field ?? new(AvailabilityPeriodType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingOccupancyStatusLabel SelectedOccupancyStatus
    {
        get => field ?? new(OccupancyStatus.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool IsAvailableForImmediateMoveIn
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool IsAvailableForRentNow
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public DateTimeOffset? OccupancyStatusCheckedAt
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }


    /*
    [ObservableProperty]
    public partial decimal KyouekiFee { get; set; }

    [ObservableProperty]
    public partial decimal Shikikin { get; set; }

    [ObservableProperty]
    public partial string ShikikinUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial decimal Reikin { get; set; }

    [ObservableProperty]
    public partial string ReikinUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial string ContractType { get; set; } = "未指定";

    [ObservableProperty]
    public partial int ContractPeriodYears { get; set; }

    [ObservableProperty]
    public partial decimal RenewalFee { get; set; }

    [ObservableProperty]
    public partial string RenewalFeeUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial decimal RecontractFee { get; set; }

    [ObservableProperty]
    public partial string RecontractFeeUnit { get; set; } = "円";

    [ObservableProperty]
    public partial string GuarantorCompany { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HousingInsurance { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OtherFees { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool PetFriendly { get; set; }

    [ObservableProperty]
    public partial bool TwoPersonOccupancy { get; set; }

    [ObservableProperty]
    public partial bool NoGuarantorNeeded { get; set; }

    [ObservableProperty]
    public partial bool OfficeUseAllowed { get; set; }

    [ObservableProperty]
    public partial string OtherConditions { get; set; } = string.Empty;
    */

    public string Remarks { get; private set; } = string.Empty;

    // 賃料（円）
    public decimal Chinryou
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            if (value > -1)
            {
                field = value;
                //OnPropertyChanged();
                IsModified = true;
            }
            else
            {
                // TODO: show error
                //field = string.Empty;
                //IsModified = true;

                throw new ArgumentOutOfRangeException("Chinryou", "Must be at least 0.");
            }
        }
    }


    #endregion

    #region == 部屋に属するリスト ==

    // 部屋写真リスト
    public ObservableCollection<ListingPicture> Pictures
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき部屋写真のIDリスト
    public ObservableCollection<ListingPicture> PicturesToBeDeleted { get; set; } = [];

    // 部屋図面リスト
    public ObservableCollection<ListingPdf> Pdfs
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public ObservableCollection<ListingPdf> PdfsToBeDeleted { get; set; } = [];

    // 貸主のリスト
    public ObservableCollection<Models.Base.PersonBase> Lessors
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき貸主のIDリスト
    public ObservableCollection<Models.Base.PersonBase> LessorsToBeDeleted { get; set; } = [];

    // 宅建業者のリスト
    public ObservableCollection<Models.Base.PersonBase> Brokers
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき宅建業者のIDリスト
    public ObservableCollection<Models.Base.PersonBase> BrokersToBeDeleted { get; set; } = [];

    #endregion

    #region == Setter Methods ==

    public void SetPropertyName(string propertyName)
    {
        PropertyName = propertyName;
    }

    public void SetIsPropertyUnitOwnership(bool isUnitOwnership)
    {
        IsPropertyUnitOwnership = isUnitOwnership;
    }

    public void SetFloorPlanTypeLabel(ListingFloorPlanTypeLabel floorPlan)
    {
        FloorPlan = floorPlan;
    }
    public void SetFloorPlanTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorPlan = new ListingFloorPlanTypeLabel(FloorPlanType.Unspecified);
            return;
        }

        if (Enum.TryParse<FloorPlanType>(Str, out var result))
        {
            FloorPlan = new ListingFloorPlanTypeLabel(result);
        }
    }

    public void SetFloorPlanExtraTypeLabel(ListingFloorPlanExtraTypeLabel floorPlanExtra)
    {
        FloorPlanExtra = floorPlanExtra;
    }
    public void SetFloorPlanExtraTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorPlanExtra = new ListingFloorPlanExtraTypeLabel(FloorPlanExtraType.Unspecified);
            return;
        }

        if (Enum.TryParse<FloorPlanExtraType>(Str, out var result))
        {
            FloorPlanExtra = new ListingFloorPlanExtraTypeLabel(result);
        }
    }

    public void SetFloorGroundTypeLabel(ListingFloorGroundTypeLabel floorGround)
    {
        FloorGround = floorGround;
    }

    public void SetFloorGroundTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorGround = new ListingFloorGroundTypeLabel(FloorGroundType.AboveGround);
            return;
        }

        if (Enum.TryParse<FloorGroundType>(Str, out var result))
        {
            FloorGround = new ListingFloorGroundTypeLabel(result);
        }
    }

    public void SetFloorExclusiveAreaSqm(string floorExclusiveAreaSqm)
    {
        FloorExclusiveAreaSqm = floorExclusiveAreaSqm;
    }

    public void SetFloorNumber(string floorNumber)
    {
        FloorNumber = floorNumber;
    }

    public void SetIsCornerRoom(bool isCornerRoom)
    {
        IsCornerRoom = isCornerRoom;
    }

    public void SetExposureDirectionTypeLabel(ListingExposureDirectionTypeLabel exposureDirectionType)
    {
        SelectedExposureDirectionType = exposureDirectionType;
    }

    public void SetExposureDirectionTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedExposureDirectionType = new ListingExposureDirectionTypeLabel(ExposureDirectionType.Unspecified);
            return;
        }

        if (Enum.TryParse<ExposureDirectionType>(Str, out var result))
        {
            SelectedExposureDirectionType = new ListingExposureDirectionTypeLabel(result);
        }
    }

    public void SetAvailabilityStatusTypeLabel(ListingAvailabilityStatusTypeLabel availabilityStatusType)
    {
        SelectedAvailabilityStatusType = availabilityStatusType;
    }

    public void SetAvailabilityStatusTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedAvailabilityStatusType = new ListingAvailabilityStatusTypeLabel(AvailabilityStatusType.Unspecified);
            return;
        }

        if (Enum.TryParse<AvailabilityStatusType>(Str, out var result))
        {
            SelectedAvailabilityStatusType = new ListingAvailabilityStatusTypeLabel(result);
        }
    }

    public void SetAvailabilityMonthTypeLabel(ListingAvailabilityMonthTypeLabel availabilityMonthType)
    {
        SelectedAvailabilityMonthType = availabilityMonthType;
    }

    public void SetAvailabilityMonthTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedAvailabilityMonthType = new ListingAvailabilityMonthTypeLabel(AvailabilityMonthType.Unspecified);
            return;
        }

        if (Enum.TryParse<AvailabilityMonthType>(Str, out var result))
        {
            SelectedAvailabilityMonthType = new ListingAvailabilityMonthTypeLabel(result);
        }
    }

    public void SetAvailabilityPeriodTypeLabel(ListingAvailabilityPeriodTypeLabel availabilityPeriodType)
    {
        SelectedAvailabilityPeriodType = availabilityPeriodType;
    }

    public void SetAvailabilityPeriodTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedAvailabilityPeriodType = new ListingAvailabilityPeriodTypeLabel(AvailabilityPeriodType.Unspecified);
            return;
        }

        if (Enum.TryParse<AvailabilityPeriodType>(Str, out var result))
        {
            SelectedAvailabilityPeriodType = new ListingAvailabilityPeriodTypeLabel(result);
        }
    }

    public void SetOccupancyStatusLabel(ListingOccupancyStatusLabel occupancyStatus)
    {
        SelectedOccupancyStatus = occupancyStatus;
    }

    public void SetOccupancyStatusLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedOccupancyStatus = new ListingOccupancyStatusLabel(OccupancyStatus.Unspecified);
            return;
        }

        if (Enum.TryParse<OccupancyStatus>(Str, out var result))
        {
            SelectedOccupancyStatus = new ListingOccupancyStatusLabel(result);
        }
    }

    public void SetIsAvailableForImmediateMoveIn(bool isAvailable)
    {
        IsAvailableForImmediateMoveIn = isAvailable;
    }

    public void SetIsAvailableForRentNow(bool isAvailable)
    {
        IsAvailableForRentNow = isAvailable;
    }

    public void SetOccupancyStatusCheckedAt(DateTimeOffset? checkedAt)
    {
        OccupancyStatusCheckedAt = checkedAt;
    }

    public void SetOccupancyStatusCheckedAtFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            OccupancyStatusCheckedAt = null;
            return;
        }

        if (DateTimeOffset.TryParse(Str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
        {
            OccupancyStatusCheckedAt = result;
        }
    }

    public void SetRemarks(string remarks)
    {
        Remarks = remarks;
    }

    public void SetChinryou(decimal chinryou)
    {
        Chinryou = chinryou;
    }

    public void SetChinryouFromString(string Str)
    {
        if (string.IsNullOrWhiteSpace(Str))
        {
            SetChinryou(0);
            return;
        }

        var text = Helpers.Common.ReplaceZenkakuNumbers(Str.Trim());
        if (Helpers.Common.CanConvertToPositiveNumber(text) &&
            long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            SetChinryou(result);
        }
    }

    #endregion
}
