namespace ZumenSearch.Models.Rent.Commercials.Listing;

public sealed partial class Listing
{
    public bool HasAirConditioning { get; private set; }
    public bool HasPrivateToilet { get; private set; }
    public bool HasHotWater { get; private set; }
    public bool HasKitchen { get; private set; }
    public bool HasVentilation { get; private set; }
    public bool HasInternet { get; private set; }
    public string EquipmentDetails { get; private set; } = string.Empty;
    public ListingUnitConditionTypeLabel UnitCondition { get; private set; } = new(ListingUnitConditionType.Unspecified);
    public ListingFixtureTransferTypeLabel FixtureTransfer { get; private set; } = new(ListingFixtureTransferType.Unconfirmed);
    public string BusinessHoursRestrictions { get; private set; } = string.Empty;
    public string UseConditions { get; private set; } = string.Empty;
    public bool PermitsHeavyFoodService { get; private set; }
    public bool PermitsLightFoodService { get; private set; }
    public bool PermitsRetail { get; private set; }
    public bool PermitsServices { get; private set; }
    public bool PermitsOffice { get; private set; }
    public bool PermitsOther { get; private set; }
    public string PermittedBusinessDetails { get; private set; } = string.Empty;
    public string FacilitiesRemarks { get; private set; } = string.Empty;

    public void SetHasAirConditioning(bool value) { HasAirConditioning = value; IsModified = true; }
    public void SetHasPrivateToilet(bool value) { HasPrivateToilet = value; IsModified = true; }
    public void SetHasHotWater(bool value) { HasHotWater = value; IsModified = true; }
    public void SetHasKitchen(bool value) { HasKitchen = value; IsModified = true; }
    public void SetHasVentilation(bool value) { HasVentilation = value; IsModified = true; }
    public void SetHasInternet(bool value) { HasInternet = value; IsModified = true; }
    public void SetEquipmentDetails(string value) { EquipmentDetails = value; IsModified = true; }
    public void SetUnitConditionTypeLabel(ListingUnitConditionTypeLabel value) { UnitCondition = value; IsModified = true; }
    public void SetUnitConditionTypeLabelFromString(string value) => SetUnitConditionTypeLabel(new(ParseType<ListingUnitConditionType>(value)));
    public void SetFixtureTransferTypeLabel(ListingFixtureTransferTypeLabel value) { FixtureTransfer = value; IsModified = true; }
    public void SetFixtureTransferTypeLabelFromString(string value) => SetFixtureTransferTypeLabel(new(ParseType<ListingFixtureTransferType>(value)));
    public void SetBusinessHoursRestrictions(string value) { BusinessHoursRestrictions = value; IsModified = true; }
    public void SetUseConditions(string value) { UseConditions = value; IsModified = true; }
    public void SetPermitsHeavyFoodService(bool value) { PermitsHeavyFoodService = value; IsModified = true; }
    public void SetPermitsLightFoodService(bool value) { PermitsLightFoodService = value; IsModified = true; }
    public void SetPermitsRetail(bool value) { PermitsRetail = value; IsModified = true; }
    public void SetPermitsServices(bool value) { PermitsServices = value; IsModified = true; }
    public void SetPermitsOffice(bool value) { PermitsOffice = value; IsModified = true; }
    public void SetPermitsOther(bool value) { PermitsOther = value; IsModified = true; }
    public void SetPermittedBusinessDetails(string value) { PermittedBusinessDetails = value; IsModified = true; }
    public void SetFacilitiesRemarks(string value) { FacilitiesRemarks = value; IsModified = true; }
}
