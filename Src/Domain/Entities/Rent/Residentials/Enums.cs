namespace ZumenSearch.Domain.Entities.Rent.Residentials;

public enum PropertyKindType { Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory }

public enum StructureType { Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other }

public enum FloorPlanType
{
    Unspecified,
    OneR,
    OneK,
    OneDK,
    OneLDK,
    TwoK,
    TwoDK,
    TwoLDK,
    ThreeK,
    ThreeDK,
    ThreeLDK,
    FourLDK,
    FiveLDK,
    SixLDK,
    OneSLDK,
    TwoSLDK,
    ThreeSLDK,
    FourSLDK
}

public enum FloorPlanExtraType { Unspecified, Loft, Maisonette, Studio }

public enum FloorGroundType { AboveGround, BelowGround }

public enum ExposureDirectionType { Unspecified, North, Northeast, East, Southeast, South, Southwest, West, Northwest }

public enum AvailabilityStatusType { Unspecified, ScheduledVacating, Vacant, ApplicationSubmitted, ContractedAndOccupied, OtherOrUnknown }

public enum AvailabilityMonthType { Unspecified, January, February, March, April, May, June, July, August, September, October, November, December }

public enum AvailabilityPeriodType { Unspecified, Early, Middle, Late }

public enum OccupancyStatus
{
    Unspecified,
    AvailableNow,
    Vacant,
    VacantSoon,
    AvailableFromDate,
    NegotiableMoveIn,
    Occupied,
    ScheduledToVacate,
    ApplicationPending,
    UnderScreening,
    TemporarilyUnavailable,
    TemporarilyReserved,
    UnderRenovation,
    UnderRemodeling,
    UnderConstruction,
    NewlyBuilt,
    Negotiable,
    Contracted
}

public enum FeeUnitType { Months, Yen }

public enum ContractType { Unspecified, Ordinary, FixedTerm }

public enum ListingHandlingType { Unspecified, OtherCompanyListing, OwnCompanyBrokerage, OwnCompanyManagement, OwnCompanyOwnership }

public enum ListingTransactionType { Unspecified, GeneralBrokerage, ExclusiveBrokerage, ExclusiveAgencyBrokerage, Agency, Lessor }

public enum ListingCommissionUnitType { Percent, Yen }
