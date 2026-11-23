using ZumenSearch.Domain.Entities.Rent.Commercials;
using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingSearchCriteria
{
    public string Keyword { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string Line { get; set; } = string.Empty;
    public string LessorName { get; set; } = string.Empty;
    public string FudousanId { get; set; } = string.Empty;
    public ListingUseTypeLabel? Use { get; set; }
    public ListingAvailabilityStatusTypeLabel? CurrentStatus { get; set; }

    public HashSet<PropertyKindType> PropertyKinds { get; private set; } = [];
    public HashSet<StructureType> PropertyStructures { get; private set; } = [];
    public double MaximumBuildingAgeYears { get; set; } = double.NaN;

    public double MinimumRent { get; set; } = double.NaN;
    public double MaximumRent { get; set; } = double.NaN;
    public double MaximumManagementFee { get; set; } = double.NaN;
    public double MinimumFloorArea { get; set; } = double.NaN;
    public double MaximumFloorArea { get; set; } = double.NaN;
    public double MaximumStationWalkMinutes { get; set; } = double.NaN;

    public ListingHandlingTypeLabel? HandlingType { get; set; }
    public ListingTransactionTypeLabel? TransactionType { get; set; }
    public double MaximumLessorCommission { get; set; } = double.NaN;
    public ListingCommissionUnitTypeLabel? LessorCommissionUnit { get; set; }
    public double MaximumLesseeCommission { get; set; } = double.NaN;
    public ListingCommissionUnitTypeLabel? LesseeCommissionUnit { get; set; }
    public double MaximumListingBrokerCommission { get; set; } = double.NaN;
    public ListingCommissionUnitTypeLabel? ListingBrokerCommissionUnit { get; set; }
    public double MaximumTenantBrokerCommission { get; set; } = double.NaN;
    public ListingCommissionUnitTypeLabel? TenantBrokerCommissionUnit { get; set; }

    public bool NoSecurityDeposit { get; set; }
    public bool NoGuaranteeDeposit { get; set; }
    public bool NoKeyMoney { get; set; }
    public bool IsAvailableForRent { get; set; }
    public bool ImmediateOccupancy { get; set; }
    public bool HasParking { get; set; }
    public bool HasAutolock { get; set; }
    public bool HasElevator { get; set; }
    public bool HasAirConditioner { get; set; }
    public bool PermitsHeavyFoodService { get; set; }
    public bool PermitsLightFoodService { get; set; }
    public bool PermitsRetail { get; set; }
    public bool PermitsServices { get; set; }
    public bool PermitsOffice { get; set; }
    public bool PermitsOther { get; set; }

    public ListingSearchCriteria Clone()
    {
        var clone = (ListingSearchCriteria)MemberwiseClone();
        clone.PropertyKinds = [.. PropertyKinds];
        clone.PropertyStructures = [.. PropertyStructures];
        return clone;
    }

    public ZumenSearch.Application.SearchResults.RentCommercialListingSearchCriteria ToApplicationQuery() => new()
    {
        Keyword = string.IsNullOrWhiteSpace(Keyword) ? "*" : Keyword.Trim(),
        PropertyName = PropertyName,
        Address = Address,
        Station = Station,
        Line = Line,
        LessorName = LessorName,
        FudousanId = FudousanId,
        PropertyKinds = [.. PropertyKinds],
        PropertyStructures = [.. PropertyStructures],
        MaximumBuildingAgeYears = MaximumBuildingAgeYears,
        Use = Use?.Key is { } use && use != ListingUseType.Unspecified ? use : null,
        CurrentStatus = CurrentStatus?.Key is { } currentStatus && currentStatus != ListingAvailabilityStatusType.Unspecified ? currentStatus : null,
        MinimumRent = MinimumRent,
        MaximumRent = MaximumRent,
        MaximumManagementFee = MaximumManagementFee,
        MinimumFloorArea = MinimumFloorArea,
        MaximumFloorArea = MaximumFloorArea,
        MaximumStationWalkMinutes = MaximumStationWalkMinutes,
        HandlingType = HandlingType?.Key,
        TransactionType = TransactionType?.Key,
        MaximumLessorCommission = MaximumLessorCommission,
        LessorCommissionUnit = LessorCommissionUnit?.Key,
        MaximumLesseeCommission = MaximumLesseeCommission,
        LesseeCommissionUnit = LesseeCommissionUnit?.Key,
        MaximumListingBrokerCommission = MaximumListingBrokerCommission,
        ListingBrokerCommissionUnit = ListingBrokerCommissionUnit?.Key,
        MaximumTenantBrokerCommission = MaximumTenantBrokerCommission,
        TenantBrokerCommissionUnit = TenantBrokerCommissionUnit?.Key,
        NoSecurityDeposit = NoSecurityDeposit,
        NoGuaranteeDeposit = NoGuaranteeDeposit,
        NoKeyMoney = NoKeyMoney,
        IsAvailableForRent = IsAvailableForRent,
        ImmediateOccupancy = ImmediateOccupancy,
        HasParking = HasParking,
        HasAutolock = HasAutolock,
        HasElevator = HasElevator,
        HasAirConditioner = HasAirConditioner,
        PermitsHeavyFoodService = PermitsHeavyFoodService,
        PermitsLightFoodService = PermitsLightFoodService,
        PermitsRetail = PermitsRetail,
        PermitsServices = PermitsServices,
        PermitsOffice = PermitsOffice,
        PermitsOther = PermitsOther
    };
}
