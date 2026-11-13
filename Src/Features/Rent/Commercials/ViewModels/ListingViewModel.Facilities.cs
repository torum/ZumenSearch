using ZumenSearch.Models.Rent.Commercials.Listing;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class ListingViewModel
{
    public IReadOnlyList<ListingUnitConditionTypeLabel> UnitConditionOptions { get; } = ListingUnitConditionTypeLabel.GetAll();
    public IReadOnlyList<ListingFixtureTransferTypeLabel> FixtureTransferOptions { get; } = ListingFixtureTransferTypeLabel.GetAll();

    public bool HasAirConditioning { get; set => SetContractValue(ref field, value); }
    public bool HasPrivateToilet { get; set => SetContractValue(ref field, value); }
    public bool HasHotWater { get; set => SetContractValue(ref field, value); }
    public bool HasKitchen { get; set => SetContractValue(ref field, value); }
    public bool HasVentilation { get; set => SetContractValue(ref field, value); }
    public bool HasInternet { get; set => SetContractValue(ref field, value); }
    public string EquipmentDetails { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public ListingUnitConditionTypeLabel SelectedUnitCondition { get => field ?? UnitConditionOptions.First(x => x.Key == ListingUnitConditionType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFixtureTransferTypeLabel SelectedFixtureTransfer { get => field ?? FixtureTransferOptions.First(x => x.Key == ListingFixtureTransferType.Unconfirmed); set { if (value is not null) SetContractValue(ref field, value); } }
    public string BusinessHoursRestrictions { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string UseConditions { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public bool PermitsHeavyFoodService { get; set => SetContractValue(ref field, value); }
    public bool PermitsLightFoodService { get; set => SetContractValue(ref field, value); }
    public bool PermitsRetail { get; set => SetContractValue(ref field, value); }
    public bool PermitsServices { get; set => SetContractValue(ref field, value); }
    public bool PermitsOffice { get; set => SetContractValue(ref field, value); }
    public bool PermitsOther { get; set => SetContractValue(ref field, value); }
    public string PermittedBusinessDetails { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string FacilitiesRemarks { get => field ?? string.Empty; set => SetContractValue(ref field, value); }

    private void PopulateFacilitiesValues()
    {
        HasAirConditioning = _unit.HasAirConditioning;
        HasPrivateToilet = _unit.HasPrivateToilet;
        HasHotWater = _unit.HasHotWater;
        HasKitchen = _unit.HasKitchen;
        HasVentilation = _unit.HasVentilation;
        HasInternet = _unit.HasInternet;
        EquipmentDetails = _unit.EquipmentDetails;
        SelectedUnitCondition = UnitConditionOptions.First(x => x.Key == _unit.UnitCondition.Key);
        SelectedFixtureTransfer = FixtureTransferOptions.First(x => x.Key == _unit.FixtureTransfer.Key);
        BusinessHoursRestrictions = _unit.BusinessHoursRestrictions;
        UseConditions = _unit.UseConditions;
        PermitsHeavyFoodService = _unit.PermitsHeavyFoodService;
        PermitsLightFoodService = _unit.PermitsLightFoodService;
        PermitsRetail = _unit.PermitsRetail;
        PermitsServices = _unit.PermitsServices;
        PermitsOffice = _unit.PermitsOffice;
        PermitsOther = _unit.PermitsOther;
        PermittedBusinessDetails = _unit.PermittedBusinessDetails;
        FacilitiesRemarks = _unit.FacilitiesRemarks;
    }

    private void SetFacilitiesValues(Models.Rent.Commercials.Listing.Listing unit)
    {
        unit.SetHasAirConditioning(HasAirConditioning);
        unit.SetHasPrivateToilet(HasPrivateToilet);
        unit.SetHasHotWater(HasHotWater);
        unit.SetHasKitchen(HasKitchen);
        unit.SetHasVentilation(HasVentilation);
        unit.SetHasInternet(HasInternet);
        unit.SetEquipmentDetails(EquipmentDetails);
        unit.SetUnitConditionTypeLabel(SelectedUnitCondition);
        unit.SetFixtureTransferTypeLabel(SelectedFixtureTransfer);
        unit.SetBusinessHoursRestrictions(BusinessHoursRestrictions);
        unit.SetUseConditions(UseConditions);
        unit.SetPermitsHeavyFoodService(PermitsHeavyFoodService);
        unit.SetPermitsLightFoodService(PermitsLightFoodService);
        unit.SetPermitsRetail(PermitsRetail);
        unit.SetPermitsServices(PermitsServices);
        unit.SetPermitsOffice(PermitsOffice);
        unit.SetPermitsOther(PermitsOther);
        unit.SetPermittedBusinessDetails(PermittedBusinessDetails);
        unit.SetFacilitiesRemarks(FacilitiesRemarks);
    }
}
