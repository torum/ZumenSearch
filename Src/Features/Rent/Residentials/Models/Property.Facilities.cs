namespace ZumenSearch.Models.Rent.Residentials;

public enum FacilityGasType { Unspecified, CityGas, Propane }
public enum FacilityTapWaterType { Unspecified, Public, Private, Other }
public enum FacilitySewerageType { Unspecified, Public, SepticTank, Other }
public enum FacilityParkingContractType { Optional, Required }
public enum FacilityAvailabilityType { Unspecified, Available, Full, Nearby }
public enum FacilityTaxType { Included, Excluded }

public sealed partial class Property
{
    public bool HasGas { get; set { field = value; IsModified = true; } }
    public FacilityGasType GasType { get; set { field = value; IsModified = true; } }
    public string GasDetail { get; set { field = value; IsModified = true; } } = string.Empty;
    public FacilityTapWaterType TapWaterType { get; set { field = value; IsModified = true; } }
    public string TapWaterDetail { get; set { field = value; IsModified = true; } } = string.Empty;
    public FacilitySewerageType SewerageType { get; set { field = value; IsModified = true; } }
    public string SewerageDetail { get; set { field = value; IsModified = true; } } = string.Empty;

    public bool GroundDigital { get; set { field = value; IsModified = true; } }
    public bool BsAntenna { get; set { field = value; IsModified = true; } }
    public bool CsAntenna { get; set { field = value; IsModified = true; } }
    public bool Catv { get; set { field = value; IsModified = true; } }
    public bool Usen { get; set { field = value; IsModified = true; } }
    public bool Internet { get; set { field = value; IsModified = true; } }
    public bool Fiber { get; set { field = value; IsModified = true; } }
    public bool InternetFree { get; set { field = value; IsModified = true; } }
    public string TvAntennaDetail { get; set { field = value; IsModified = true; } } = string.Empty;
    public string InternetDetail { get; set { field = value; IsModified = true; } } = string.Empty;

    public bool HasParking { get; set { field = value; IsModified = true; } }
    public bool IsParkingFree { get; set { field = value; IsModified = true; } }
    public FacilityParkingContractType ParkingContractType { get; set { field = value; IsModified = true; } }
    public string ParkingFee { get; set { field = value; IsModified = true; } } = string.Empty;
    public FacilityTaxType ParkingFeeTaxType { get; set { field = value; IsModified = true; } }
    public FacilityAvailabilityType ParkingAvailability { get; set { field = value; IsModified = true; } }
    public string ParkingSpaces { get; set { field = value; IsModified = true; } } = string.Empty;
    public string ParkingDistance { get; set { field = value; IsModified = true; } } = string.Empty;
    public string ParkingNotes { get; set { field = value; IsModified = true; } } = string.Empty;

    public bool HasBicycleParking { get; set { field = value; IsModified = true; } }
    public bool IsBicycleParkingFree { get; set { field = value; IsModified = true; } }
    public FacilityAvailabilityType BicycleParkingAvailability { get; set { field = value; IsModified = true; } }
    public string BicycleParkingFee { get; set { field = value; IsModified = true; } } = string.Empty;
    public FacilityTaxType BicycleParkingFeeTaxType { get; set { field = value; IsModified = true; } }
    public string BicycleParkingNotes { get; set { field = value; IsModified = true; } } = string.Empty;

    public bool HasMotorcycleParking { get; set { field = value; IsModified = true; } }
    public bool IsMotorcycleParkingFree { get; set { field = value; IsModified = true; } }
    public FacilityAvailabilityType MotorcycleParkingAvailability { get; set { field = value; IsModified = true; } }
    public string MotorcycleParkingFee { get; set { field = value; IsModified = true; } } = string.Empty;
    public FacilityTaxType MotorcycleParkingFeeTaxType { get; set { field = value; IsModified = true; } }
    public string MotorcycleParkingNotes { get; set { field = value; IsModified = true; } } = string.Empty;
}
