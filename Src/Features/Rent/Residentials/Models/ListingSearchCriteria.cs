namespace ZumenSearch.Models.Rent.Residentials;

public sealed class ListingSearchCriteria
{
    public string PropertyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string Line { get; set; } = string.Empty;
    public string LessorName { get; set; } = string.Empty;
    public string FudousanId { get; set; } = string.Empty;

    public double MinimumRent { get; set; } = double.NaN;
    public double MaximumRent { get; set; } = double.NaN;
    public double MaximumManagementFee { get; set; } = double.NaN;
    public bool NoSecurityDeposit { get; set; }
    public bool NoKeyMoney { get; set; }
    public double MaximumStationWalkMinutes { get; set; } = double.NaN;

    public HashSet<PropertyKindType> PropertyKinds { get; private set; } = [];
    public HashSet<StructureType> PropertyStructures { get; private set; } = [];
    public double MaximumBuildingAgeYears { get; set; } = double.NaN;
    public HashSet<FloorPlanType> FloorPlans { get; private set; } = [];
    public double MinimumFloorArea { get; set; } = double.NaN;
    public double MaximumFloorArea { get; set; } = double.NaN;

    public bool PetFriendly { get; set; }
    public bool TwoPersonOccupancy { get; set; }
    public bool NoGuarantorNeeded { get; set; }
    public bool OfficeUseAllowed { get; set; }

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

    public bool IsRecruiting { get; set; }
    public bool ImmediateOccupancy { get; set; }
    public bool CornerRoom { get; set; }
    public bool HasAutolock { get; set; }
    public bool HasElevator { get; set; }
    public bool HasParcelLocker { get; set; }
    public bool HasFreeInternet { get; set; }
    public bool HasAirConditioner { get; set; }
    public bool HasSeparateBathAndToilet { get; set; }
    public bool HasParking { get; set; }

    public ListingSearchCriteria Clone()
    {
        var clone = (ListingSearchCriteria)MemberwiseClone();
        clone.PropertyKinds = [.. PropertyKinds];
        clone.PropertyStructures = [.. PropertyStructures];
        clone.FloorPlans = [.. FloorPlans];
        return clone;
    }
}
