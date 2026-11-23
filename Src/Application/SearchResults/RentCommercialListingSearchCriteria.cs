using ZumenSearch.Domain.Entities.Rent.Commercials;
using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Application.SearchResults;

public sealed class RentCommercialListingSearchCriteria
{
    public string Keyword { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string Line { get; set; } = string.Empty;
    public string LessorName { get; set; } = string.Empty;
    public string FudousanId { get; set; } = string.Empty;
    public ListingUseType? Use { get; set; }
    public ListingAvailabilityStatusType? CurrentStatus { get; set; }

    public HashSet<PropertyKindType> PropertyKinds { get; set; } = [];
    public HashSet<StructureType> PropertyStructures { get; set; } = [];
    public double MaximumBuildingAgeYears { get; set; } = double.NaN;

    public double MinimumRent { get; set; } = double.NaN;
    public double MaximumRent { get; set; } = double.NaN;
    public double MaximumManagementFee { get; set; } = double.NaN;
    public double MinimumFloorArea { get; set; } = double.NaN;
    public double MaximumFloorArea { get; set; } = double.NaN;
    public double MaximumStationWalkMinutes { get; set; } = double.NaN;

    public ListingHandlingType? HandlingType { get; set; }
    public ListingTransactionType? TransactionType { get; set; }
    public double MaximumLessorCommission { get; set; } = double.NaN;
    public ListingCommissionUnitType? LessorCommissionUnit { get; set; }
    public double MaximumLesseeCommission { get; set; } = double.NaN;
    public ListingCommissionUnitType? LesseeCommissionUnit { get; set; }
    public double MaximumListingBrokerCommission { get; set; } = double.NaN;
    public ListingCommissionUnitType? ListingBrokerCommissionUnit { get; set; }
    public double MaximumTenantBrokerCommission { get; set; } = double.NaN;
    public ListingCommissionUnitType? TenantBrokerCommissionUnit { get; set; }

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
}
