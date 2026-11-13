using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Rent.Commercials;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class PropertyViewModel
{
    public ObservableCollection<GasSupplyTypeLabel> GasTypeOptions { get; } = [new(GasSupplyType.Unconfirmed), new(GasSupplyType.CityGas), new(GasSupplyType.Propane), new(GasSupplyType.Other)];
    public ObservableCollection<WaterSupplyTypeLabel> WaterTypeOptions { get; } = [new(WaterSupplyType.Unconfirmed), new(WaterSupplyType.Public), new(WaterSupplyType.Private), new(WaterSupplyType.Well), new(WaterSupplyType.Other)];
    public ObservableCollection<SewerageTypeLabel> SewerageTypeOptions { get; } = [new(SewerageType.Unconfirmed), new(SewerageType.PublicSewer), new(SewerageType.SepticTank), new(SewerageType.Other)];
    public ObservableCollection<ParkingVacancyTypeLabel> ParkingVacancyOptions { get; } = [new(ParkingVacancyType.Unconfirmed), new(ParkingVacancyType.Available), new(ParkingVacancyType.Unavailable)];
    public ObservableCollection<ParkingTaxTypeLabel> ParkingTaxOptions { get; } = [new(ParkingTaxType.Unconfirmed), new(ParkingTaxType.Included), new(ParkingTaxType.Excluded)];

    public bool HasElectricity { get; set { if (SetProperty(ref field, value)) IsDirty = true; } }
    public string ElectricityDetails { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;
    public bool HasGas { get; set { if (SetProperty(ref field, value)) IsDirty = true; } }
    public GasSupplyTypeLabel SelectedGasType { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = new(GasSupplyType.Unconfirmed);
    public string GasDetails { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;
    public bool HasWater { get; set { if (SetProperty(ref field, value)) IsDirty = true; } }
    public WaterSupplyTypeLabel SelectedWaterType { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = new(WaterSupplyType.Unconfirmed);
    public string WaterDetails { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;
    public SewerageTypeLabel SelectedSewerageType { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = new(SewerageType.Unconfirmed);
    public string SewerageDetails { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;
    public bool HasInternet { get; set { if (SetProperty(ref field, value)) IsDirty = true; } }
    public bool HasFiber { get; set { if (SetProperty(ref field, value)) IsDirty = true; } }
    public string InternetDetails { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;
    public bool HasFireSafety { get; set { if (SetProperty(ref field, value)) IsDirty = true; } }
    public string FireSafetyDetails { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;
    public bool HasParking { get; set { if (SetProperty(ref field, value)) IsDirty = true; } }
    public ParkingVacancyTypeLabel SelectedParkingVacancy { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = new(ParkingVacancyType.Unconfirmed);
    public string ParkingSpaces { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;
    public string ParkingMonthlyFee { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;
    public ParkingTaxTypeLabel SelectedParkingTax { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = new(ParkingTaxType.Unconfirmed);
    public bool IsParkingFree
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(IsParkingFeeEnabled));
                OnPropertyChanged(nameof(ParkingFreeDescription));
            }
        }
    }
    public bool IsParkingFeeEnabled => !IsParkingFree;
    public string ParkingFreeDescription => IsParkingFree ? "現在の駐車場料金：無料" : string.Empty;
    public string ParkingDetails { get; set { if (SetProperty(ref field, value)) IsDirty = true; } } = string.Empty;

    private void LoadRemainingFacilities()
    {
        HasElectricity = _building.HasElectricity;
        ElectricityDetails = _building.ElectricityDetails;
        HasGas = _building.HasGas;
        SelectedGasType = GasTypeOptions.First(item => item.Key == _building.GasType);
        GasDetails = _building.GasDetails;
        HasWater = _building.HasWater;
        SelectedWaterType = WaterTypeOptions.First(item => item.Key == _building.WaterType);
        WaterDetails = _building.WaterDetails;
        SelectedSewerageType = SewerageTypeOptions.First(item => item.Key == _building.SewerageType);
        SewerageDetails = _building.SewerageDetails;
        HasInternet = _building.HasInternet;
        HasFiber = _building.HasFiber;
        InternetDetails = _building.InternetDetails;
        HasFireSafety = _building.HasFireSafety;
        FireSafetyDetails = _building.FireSafetyDetails;
        HasParking = _building.HasParking;
        SelectedParkingVacancy = ParkingVacancyOptions.First(item => item.Key == _building.ParkingVacancy);
        ParkingSpaces = _building.ParkingSpaces?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        ParkingMonthlyFee = _building.ParkingMonthlyFee?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        SelectedParkingTax = ParkingTaxOptions.First(item => item.Key == _building.ParkingTax);
        IsParkingFree = _building.IsParkingFree;
        ParkingDetails = _building.ParkingDetails;
    }

    private void ApplyRemainingFacilities()
    {
        _building.HasElectricity = HasElectricity;
        _building.ElectricityDetails = ElectricityDetails;
        _building.HasGas = HasGas;
        _building.GasType = SelectedGasType.Key;
        _building.GasDetails = GasDetails;
        _building.HasWater = HasWater;
        _building.WaterType = SelectedWaterType.Key;
        _building.WaterDetails = WaterDetails;
        _building.SewerageType = SelectedSewerageType.Key;
        _building.SewerageDetails = SewerageDetails;
        _building.HasInternet = HasInternet;
        _building.HasFiber = HasFiber;
        _building.InternetDetails = InternetDetails;
        _building.HasFireSafety = HasFireSafety;
        _building.FireSafetyDetails = FireSafetyDetails;
        _building.HasParking = HasParking;
        _building.ParkingVacancy = SelectedParkingVacancy.Key;
        _building.ParkingSpaces = string.IsNullOrWhiteSpace(ParkingSpaces) ? null : ParseInteger(ParkingSpaces);
        _building.ParkingMonthlyFee = string.IsNullOrWhiteSpace(ParkingMonthlyFee) ? null : ParseDecimal(ParkingMonthlyFee);
        _building.ParkingTax = SelectedParkingTax.Key;
        _building.IsParkingFree = IsParkingFree;
        _building.ParkingDetails = ParkingDetails;
    }
}
