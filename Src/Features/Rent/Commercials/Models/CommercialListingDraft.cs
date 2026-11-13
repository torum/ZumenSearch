using ZumenSearch.Models.Base;
using ZumenSearch.Models.Rent.Commercials.Listing;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed record CommercialListingPictureDraft(string Id, string ImageFilename, Listing.ListingPictureType PictureType, string Description, bool IsMain, EntityStatus Status);
public sealed record CommercialListingPdfDraft(string Id, string PdfFilename, string ThumbnailFilename, Listing.ListingPdfType PdfType, string Description, bool IsMain, EntityStatus Status);

public sealed record CommercialListingCommittedData(
    IReadOnlyList<CommercialListingPdfDraft> Pdfs,
    IReadOnlyList<string> LessorIds,
    IReadOnlyList<string> BrokerIds);

public sealed class CommercialListingDraft
{
    public required string Id { get; init; }
    public required string PropertyId { get; init; }
    public required EntityStatus Status { get; init; }
    public required EntityStatus PropertyStatus { get; init; }
    public required string PropertyName { get; init; }
    public required string Name { get; init; }
    public required bool IsPropertyUnitOwnership { get; init; }
    public required decimal Chinryou { get; init; }
    public required ListingContractTaxTypeLabel RentTax { get; init; }
    public required decimal ManagementFee { get; init; }
    public required ListingContractTaxTypeLabel ManagementFeeTax { get; init; }
    public required decimal SecurityDeposit { get; init; }
    public required ListingFeeUnitTypeLabel SecurityDepositUnit { get; init; }
    public required decimal GuaranteeDeposit { get; init; }
    public required ListingFeeUnitTypeLabel GuaranteeDepositUnit { get; init; }
    public required decimal KeyMoney { get; init; }
    public required ListingFeeUnitTypeLabel KeyMoneyUnit { get; init; }
    public required decimal RightsMoney { get; init; }
    public required ListingFeeUnitTypeLabel RightsMoneyUnit { get; init; }
    public required decimal DepositDeduction { get; init; }
    public required ListingFeeUnitTypeLabel DepositDeductionUnit { get; init; }
    public required decimal RenewalFee { get; init; }
    public required ListingFeeUnitTypeLabel RenewalFeeUnit { get; init; }
    public required decimal RecontractFee { get; init; }
    public required ListingFeeUnitTypeLabel RecontractFeeUnit { get; init; }
    public required ListingContractTypeLabel ContractType { get; init; }
    public required decimal? ContractPeriodYears { get; init; }
    public required string GuarantorCompany { get; init; }
    public required string Insurance { get; init; }
    public required string OtherFees { get; init; }
    public required string ContractConditions { get; init; }
    public required string ContractRemarks { get; init; }
    public required ListingFloorGroundTypeLabel FloorGround { get; init; }
    public required int? FloorNumber { get; init; }
    public required decimal? FloorArea { get; init; }
    public required ListingUseTypeLabel Use { get; init; }
    public required string UseDetails { get; init; }
    public required ListingAvailabilityStatusTypeLabel AvailabilityStatus { get; init; }
    public required bool IsAvailableForRentNow { get; init; }
    public required ListingAvailabilityMonthTypeLabel AvailabilityMonth { get; init; }
    public required ListingAvailabilityPeriodTypeLabel AvailabilityPeriod { get; init; }
    public required bool IsAvailableForImmediateMoveIn { get; init; }
    public required ListingHandlingTypeLabel HandlingType { get; init; }
    public required ListingTransactionTypeLabel TransactionType { get; init; }
    public required string LessorCommission { get; init; }
    public required ListingCommissionUnitTypeLabel LessorCommissionUnit { get; init; }
    public required string LesseeCommission { get; init; }
    public required ListingCommissionUnitTypeLabel LesseeCommissionUnit { get; init; }
    public required string ListingBrokerCommission { get; init; }
    public required ListingCommissionUnitTypeLabel ListingBrokerCommissionUnit { get; init; }
    public required string TenantBrokerCommission { get; init; }
    public required ListingCommissionUnitTypeLabel TenantBrokerCommissionUnit { get; init; }
    public required string TransactionRemarks { get; init; }
    public required bool HasAirConditioning { get; init; }
    public required bool HasPrivateToilet { get; init; }
    public required bool HasHotWater { get; init; }
    public required bool HasKitchen { get; init; }
    public required bool HasVentilation { get; init; }
    public required bool HasInternet { get; init; }
    public required string EquipmentDetails { get; init; }
    public required ListingUnitConditionTypeLabel UnitCondition { get; init; }
    public required ListingFixtureTransferTypeLabel FixtureTransfer { get; init; }
    public required string BusinessHoursRestrictions { get; init; }
    public required string UseConditions { get; init; }
    public required bool PermitsHeavyFoodService { get; init; }
    public required bool PermitsLightFoodService { get; init; }
    public required bool PermitsRetail { get; init; }
    public required bool PermitsServices { get; init; }
    public required bool PermitsOffice { get; init; }
    public required bool PermitsOther { get; init; }
    public required string PermittedBusinessDetails { get; init; }
    public required string FacilitiesRemarks { get; init; }
    public required string Remarks { get; init; }
    public required IReadOnlyList<CommercialListingPictureDraft> Pictures { get; init; }
    public required IReadOnlyList<CommercialListingPictureDraft> PicturesToBeDeleted { get; init; }
    public required IReadOnlyList<CommercialListingPdfDraft> Pdfs { get; init; }
    public required IReadOnlyList<CommercialListingPdfDraft> PdfsToBeDeleted { get; init; }
    public required IReadOnlyList<string> LessorIds { get; init; }
    public required IReadOnlyList<string> LessorsToBeDeleted { get; init; }
    public required IReadOnlyList<string> BrokerIds { get; init; }
    public required IReadOnlyList<string> BrokersToBeDeleted { get; init; }

    public static CommercialListingDraft FromListing(Listing.Listing listing) => new()
    {
        Id = listing.Id,
        PropertyId = listing.PropertyId,
        Status = listing.Status,
        PropertyStatus = listing.PropertyStatus,
        PropertyName = listing.PropertyName,
        Name = listing.Name,
        IsPropertyUnitOwnership = listing.IsPropertyUnitOwnership,
        Chinryou = listing.Chinryou,
        RentTax = listing.RentTax,
        ManagementFee = listing.ManagementFee,
        ManagementFeeTax = listing.ManagementFeeTax,
        SecurityDeposit = listing.SecurityDeposit,
        SecurityDepositUnit = listing.SecurityDepositUnit,
        GuaranteeDeposit = listing.GuaranteeDeposit,
        GuaranteeDepositUnit = listing.GuaranteeDepositUnit,
        KeyMoney = listing.KeyMoney,
        KeyMoneyUnit = listing.KeyMoneyUnit,
        RightsMoney = listing.RightsMoney,
        RightsMoneyUnit = listing.RightsMoneyUnit,
        DepositDeduction = listing.DepositDeduction,
        DepositDeductionUnit = listing.DepositDeductionUnit,
        RenewalFee = listing.RenewalFee,
        RenewalFeeUnit = listing.RenewalFeeUnit,
        RecontractFee = listing.RecontractFee,
        RecontractFeeUnit = listing.RecontractFeeUnit,
        ContractType = listing.ContractType,
        ContractPeriodYears = listing.ContractPeriodYears,
        GuarantorCompany = listing.GuarantorCompany,
        Insurance = listing.Insurance,
        OtherFees = listing.OtherFees,
        ContractConditions = listing.ContractConditions,
        ContractRemarks = listing.ContractRemarks,
        FloorGround = listing.FloorGround,
        FloorNumber = listing.FloorNumber,
        FloorArea = listing.FloorArea,
        Use = listing.Use,
        UseDetails = listing.UseDetails,
        AvailabilityStatus = listing.AvailabilityStatus,
        IsAvailableForRentNow = listing.IsAvailableForRentNow,
        AvailabilityMonth = listing.AvailabilityMonth,
        AvailabilityPeriod = listing.AvailabilityPeriod,
        IsAvailableForImmediateMoveIn = listing.IsAvailableForImmediateMoveIn,
        HandlingType = listing.HandlingType,
        TransactionType = listing.TransactionType,
        LessorCommission = listing.LessorCommission,
        LessorCommissionUnit = listing.LessorCommissionUnit,
        LesseeCommission = listing.LesseeCommission,
        LesseeCommissionUnit = listing.LesseeCommissionUnit,
        ListingBrokerCommission = listing.ListingBrokerCommission,
        ListingBrokerCommissionUnit = listing.ListingBrokerCommissionUnit,
        TenantBrokerCommission = listing.TenantBrokerCommission,
        TenantBrokerCommissionUnit = listing.TenantBrokerCommissionUnit,
        TransactionRemarks = listing.TransactionRemarks,
        HasAirConditioning = listing.HasAirConditioning,
        HasPrivateToilet = listing.HasPrivateToilet,
        HasHotWater = listing.HasHotWater,
        HasKitchen = listing.HasKitchen,
        HasVentilation = listing.HasVentilation,
        HasInternet = listing.HasInternet,
        EquipmentDetails = listing.EquipmentDetails,
        UnitCondition = listing.UnitCondition,
        FixtureTransfer = listing.FixtureTransfer,
        BusinessHoursRestrictions = listing.BusinessHoursRestrictions,
        UseConditions = listing.UseConditions,
        PermitsHeavyFoodService = listing.PermitsHeavyFoodService,
        PermitsLightFoodService = listing.PermitsLightFoodService,
        PermitsRetail = listing.PermitsRetail,
        PermitsServices = listing.PermitsServices,
        PermitsOffice = listing.PermitsOffice,
        PermitsOther = listing.PermitsOther,
        PermittedBusinessDetails = listing.PermittedBusinessDetails,
        FacilitiesRemarks = listing.FacilitiesRemarks,
        Remarks = listing.Remarks,
        Pictures = listing.Pictures.Select(ToPictureDraft).ToArray(),
        PicturesToBeDeleted = listing.PicturesToBeDeleted.Select(ToPictureDraft).ToArray(),
        Pdfs = listing.Pdfs.Select(ToPdfDraft).ToArray(),
        PdfsToBeDeleted = listing.PdfsToBeDeleted.Select(ToPdfDraft).ToArray(),
        LessorIds = listing.Lessors.Select(person => person.Id).ToArray(),
        LessorsToBeDeleted = listing.LessorsToBeDeleted.Select(person => person.Id).ToArray(),
        BrokerIds = listing.Brokers.Select(person => person.Id).ToArray(),
        BrokersToBeDeleted = listing.BrokersToBeDeleted.Select(person => person.Id).ToArray()
    };

    public Listing.Listing ToEditorModel(IReadOnlyDictionary<string, ListingPicture> pictures, IReadOnlyDictionary<string, ListingPdf> pdfs)
    {
        var listing = new Listing.Listing(Id, Status, PropertyId, PropertyStatus, IsPropertyUnitOwnership, PropertyName);
        listing.SetName(Name);
        listing.SetChinryou(Chinryou);
        listing.SetRentTaxTypeLabel(RentTax);
        listing.SetManagementFee(ManagementFee);
        listing.SetManagementFeeTaxTypeLabel(ManagementFeeTax);
        listing.SetSecurityDeposit(SecurityDeposit);
        listing.SetSecurityDepositUnitTypeLabel(SecurityDepositUnit);
        listing.SetGuaranteeDeposit(GuaranteeDeposit);
        listing.SetGuaranteeDepositUnitTypeLabel(GuaranteeDepositUnit);
        listing.SetKeyMoney(KeyMoney);
        listing.SetKeyMoneyUnitTypeLabel(KeyMoneyUnit);
        listing.SetRightsMoney(RightsMoney);
        listing.SetRightsMoneyUnitTypeLabel(RightsMoneyUnit);
        listing.SetDepositDeduction(DepositDeduction);
        listing.SetDepositDeductionUnitTypeLabel(DepositDeductionUnit);
        listing.SetRenewalFee(RenewalFee);
        listing.SetRenewalFeeUnitTypeLabel(RenewalFeeUnit);
        listing.SetRecontractFee(RecontractFee);
        listing.SetRecontractFeeUnitTypeLabel(RecontractFeeUnit);
        listing.SetContractTypeLabel(ContractType);
        listing.SetContractPeriodYears(ContractPeriodYears);
        listing.SetGuarantorCompany(GuarantorCompany);
        listing.SetInsurance(Insurance);
        listing.SetOtherFees(OtherFees);
        listing.SetContractConditions(ContractConditions);
        listing.SetContractRemarks(ContractRemarks);
        listing.SetFloorGroundTypeLabel(FloorGround);
        listing.SetFloorNumber(FloorNumber);
        listing.SetFloorArea(FloorArea);
        listing.SetUseTypeLabel(Use);
        listing.SetUseDetails(UseDetails);
        listing.SetAvailabilityStatusTypeLabel(AvailabilityStatus);
        listing.SetIsAvailableForRentNow(IsAvailableForRentNow);
        listing.SetAvailabilityMonthTypeLabel(AvailabilityMonth);
        listing.SetAvailabilityPeriodTypeLabel(AvailabilityPeriod);
        listing.SetIsAvailableForImmediateMoveIn(IsAvailableForImmediateMoveIn);
        listing.SetHandlingTypeLabel(HandlingType);
        listing.SetTransactionTypeLabel(TransactionType);
        listing.SetLessorCommission(LessorCommission);
        listing.SetLessorCommissionUnitTypeLabel(LessorCommissionUnit);
        listing.SetLesseeCommission(LesseeCommission);
        listing.SetLesseeCommissionUnitTypeLabel(LesseeCommissionUnit);
        listing.SetListingBrokerCommission(ListingBrokerCommission);
        listing.SetListingBrokerCommissionUnitTypeLabel(ListingBrokerCommissionUnit);
        listing.SetTenantBrokerCommission(TenantBrokerCommission);
        listing.SetTenantBrokerCommissionUnitTypeLabel(TenantBrokerCommissionUnit);
        listing.SetTransactionRemarks(TransactionRemarks);
        listing.SetHasAirConditioning(HasAirConditioning);
        listing.SetHasPrivateToilet(HasPrivateToilet);
        listing.SetHasHotWater(HasHotWater);
        listing.SetHasKitchen(HasKitchen);
        listing.SetHasVentilation(HasVentilation);
        listing.SetHasInternet(HasInternet);
        listing.SetEquipmentDetails(EquipmentDetails);
        listing.SetUnitConditionTypeLabel(UnitCondition);
        listing.SetFixtureTransferTypeLabel(FixtureTransfer);
        listing.SetBusinessHoursRestrictions(BusinessHoursRestrictions);
        listing.SetUseConditions(UseConditions);
        listing.SetPermitsHeavyFoodService(PermitsHeavyFoodService);
        listing.SetPermitsLightFoodService(PermitsLightFoodService);
        listing.SetPermitsRetail(PermitsRetail);
        listing.SetPermitsServices(PermitsServices);
        listing.SetPermitsOffice(PermitsOffice);
        listing.SetPermitsOther(PermitsOther);
        listing.SetPermittedBusinessDetails(PermittedBusinessDetails);
        listing.SetFacilitiesRemarks(FacilitiesRemarks);
        listing.SetRemarks(Remarks);
        foreach (var picture in Pictures) listing.Pictures.Add(RestorePicture(picture, pictures));
        foreach (var picture in PicturesToBeDeleted) listing.PicturesToBeDeleted.Add(RestorePicture(picture, pictures));
        foreach (var pdf in Pdfs) listing.Pdfs.Add(RestorePdf(pdf, pdfs));
        foreach (var pdf in PdfsToBeDeleted) listing.PdfsToBeDeleted.Add(RestorePdf(pdf, pdfs));
        listing.SetIsModified(false);
        return listing;
    }

    private static ListingPicture RestorePicture(CommercialListingPictureDraft picture, IReadOnlyDictionary<string, ListingPicture> pictures)
    {
        var model = pictures[picture.Id];
        model.ImageFilename = picture.ImageFilename;
        model.Description = picture.Description;
        model.IsMain = picture.IsMain;
        model.SetLabelFromString(picture.PictureType.ToString());
        model.SetStatus(picture.Status);
        model.SetIsModified(false);
        return model;
    }

    private static ListingPdf RestorePdf(CommercialListingPdfDraft pdf, IReadOnlyDictionary<string, ListingPdf> pdfs)
    {
        var model = pdfs[pdf.Id];
        model.PdfFilename = pdf.PdfFilename;
        model.ThumbnailFilename = pdf.ThumbnailFilename;
        model.Description = pdf.Description;
        model.IsMain = pdf.IsMain;
        model.SetPdfTypeLabelFromString(pdf.PdfType.ToString());
        model.SetStatus(pdf.Status);
        model.SetIsModified(false);
        return model;
    }

    private static CommercialListingPictureDraft ToPictureDraft(ListingPicture picture) => new(picture.Id, picture.ImageFilename, picture.PictureType.Key, picture.Description, picture.IsMain, picture.Status);
    private static CommercialListingPdfDraft ToPdfDraft(ListingPdf pdf) => new(pdf.Id, pdf.PdfFilename, pdf.ThumbnailFilename, pdf.PdfType.Key, pdf.Description, pdf.IsMain, pdf.Status);
}
