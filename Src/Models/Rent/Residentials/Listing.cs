using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Enums;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.Rent.Residentials;

// 部屋（編集用）
public sealed partial class Listing : ListingBase
{
    // 物件（建物の名前を保持 - タイトル等に表示）
    public string PropertyName
    {
        get => field ?? string.Empty;
        private set;
    }

    // 部屋が属する建物が区分所有化どうかをここでも保持（部屋を直接開いた際に必要）
    public bool IsPropertyUnitOwnership { get; private set; }

    #region == 部屋の基本情報 ==

    public ListingFloorPlanTypeLabel FloorPlan { get; private set; } = new(FloorPlanType.Unspecified);

    public ListingFloorPlanExtraTypeLabel FloorPlanExtra { get; private set; } = new(FloorPlanExtraType.Unspecified);

    public FloorGroundType FloorNumberType { get; set; } = FloorGroundType.AboveGround;

    public int RoomCount { get; set; } = 1;

    /*
     * 
    [ObservableProperty]
    public partial decimal FloorArea { get; set; }

    [ObservableProperty]
    public partial int? FloorNumber { get; set; }

    [ObservableProperty]
    public partial bool IsCornerRoom { get; set; }

    [ObservableProperty]
    public partial ExposureDirectionType MainExposureDirection { get; set; } = ExposureDirectionType.Unspecified;

    [ObservableProperty]
    public partial ListingCurrentStatusType CurrentStatus { get; set; } = ListingCurrentStatusType.Unspecified;

    [ObservableProperty]
    public partial bool IsRecruiting { get; set; }

    [ObservableProperty]
    public partial int? AvailableFromMonth { get; set; }

    [ObservableProperty]
    public partial AvailabilityPeriodType AvailableFromPeriod { get; set; } = AvailabilityPeriodType.Unspecified;

    [ObservableProperty]
    public partial bool IsImmediateOccupancy { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset? CurrentStatusCheckedAt { get; set; }


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

    public string Remarks { get; set; } = string.Empty;

    // 賃料（円）
    public decimal Chinryou
    {
        get;
        set
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

    public Listing(string id, EntityStatus status, string propertyId, EntityStatus propertyStatus, bool isPropertyUnitOwnership, string propertyName) : base(id, status, propertyId, propertyStatus, Enums.PropertyContextType.RentResidential)
    {
        //PropertyId = propertyId;
        IsPropertyUnitOwnership = isPropertyUnitOwnership;
        PropertyName = propertyName;
        //PropertyStatus = propertyStatus;
        //PropertyChanged += (_, _) => SetIsModified(true);
        SetIsModified(false);
    }

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

    #endregion
}
