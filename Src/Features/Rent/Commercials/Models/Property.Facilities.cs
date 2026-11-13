namespace ZumenSearch.Models.Rent.Commercials;

public sealed partial class Property
{
    public bool HasElectricity { get; set { field = value; IsModified = true; } }
    public string ElectricityDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasGas { get; set { field = value; IsModified = true; } }
    public GasSupplyType GasType { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public string GasDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasWater { get; set { field = value; IsModified = true; } }
    public WaterSupplyType WaterType { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public string WaterDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public SewerageType SewerageType { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public string SewerageDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasInternet { get; set { field = value; IsModified = true; } }
    public bool HasFiber { get; set { field = value; IsModified = true; } }
    public string InternetDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasFireSafety { get; set { field = value; IsModified = true; } }
    public string FireSafetyDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasParking { get; set { field = value; IsModified = true; } }
    public ParkingVacancyType ParkingVacancy { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public int? ParkingSpaces { get; set { if (value < 0) throw new ArgumentOutOfRangeException(nameof(value)); field = value; IsModified = true; } }
    public decimal? ParkingMonthlyFee { get; set { if (value < 0) throw new ArgumentOutOfRangeException(nameof(value)); field = value; IsModified = true; } }
    public ParkingTaxType ParkingTax { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public bool IsParkingFree { get; set { field = value; IsModified = true; } }
    public string ParkingDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }

    private static void ValidateFacilityEnum<T>(T value) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
    }
}
