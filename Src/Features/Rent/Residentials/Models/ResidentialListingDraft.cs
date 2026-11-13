using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

public sealed record ResidentialListingPictureDraft(string Id, string ImageFilename, ListingPictureType PictureType, string Description, bool IsMain, EntityStatus Status);
public sealed record ResidentialListingPdfDraft(string Id, string PdfFilename, string ThumbnailFilename, ListingPdfType PdfType, string Description, bool IsMain, EntityStatus Status);

public sealed class ResidentialListingDraft
{
    private ResidentialListingDraft(Listing listing)
    {
        Id = listing.Id;
        PropertyId = listing.PropertyId;
        Status = listing.Status;
        PropertyStatus = listing.PropertyStatus;
        PropertyName = listing.PropertyName;
        Name = listing.Name;
        IsPropertyUnitOwnership = listing.IsPropertyUnitOwnership;
        FloorPlan = listing.FloorPlan.Key;
        FloorPlanExtra = listing.FloorPlanExtra.Key;
        FloorGround = listing.FloorGround.Key;
        FloorExclusiveAreaSqm = listing.FloorExclusiveAreaSqm;
        FloorNumber = listing.FloorNumber;
        IsCornerRoom = listing.IsCornerRoom;
        ExposureDirection = listing.SelectedExposureDirectionType.Key;
        AvailabilityStatus = listing.SelectedAvailabilityStatusType.Key;
        OccupancyStatus = listing.SelectedOccupancyStatus.Key;
        IsAvailableForRentNow = listing.IsAvailableForRentNow;
        AvailabilityMonth = listing.SelectedAvailabilityMonthType.Key;
        AvailabilityPeriod = listing.SelectedAvailabilityPeriodType.Key;
        IsAvailableForImmediateMoveIn = listing.IsAvailableForImmediateMoveIn;
        OccupancyStatusCheckedAt = listing.OccupancyStatusCheckedAt;
        Remarks = listing.Remarks;
        Chinryou = listing.Chinryou;
        ManagementFee = listing.ManagementFee;
        ManagementFeeUnit = listing.ManagementFeeUnit.Key;
        SecurityDeposit = listing.SecurityDeposit;
        SecurityDepositUnit = listing.SecurityDepositUnit.Key;
        KeyMoney = listing.KeyMoney;
        KeyMoneyUnit = listing.KeyMoneyUnit.Key;
        ContractType = listing.ContractType.Key;
        ContractPeriodYears = listing.ContractPeriodYears;
        RenewalFee = listing.RenewalFee;
        RenewalFeeUnit = listing.RenewalFeeUnit.Key;
        RecontractFee = listing.RecontractFee;
        RecontractFeeUnit = listing.RecontractFeeUnit.Key;
        GuarantorCompany = listing.GuarantorCompany;
        HousingInsurance = listing.HousingInsurance;
        OtherFees = listing.OtherFees;
        PetFriendly = listing.PetFriendly;
        TwoPersonOccupancy = listing.TwoPersonOccupancy;
        NoGuarantorNeeded = listing.NoGuarantorNeeded;
        OfficeUseAllowed = listing.OfficeUseAllowed;
        OtherConditions = listing.OtherConditions;
        ContractRemarks = listing.ContractRemarks;
        HandlingType = listing.HandlingType.Key;
        TransactionType = listing.TransactionType.Key;
        LessorCommission = listing.LessorCommission;
        LessorCommissionUnit = listing.LessorCommissionUnit.Key;
        LesseeCommission = listing.LesseeCommission;
        LesseeCommissionUnit = listing.LesseeCommissionUnit.Key;
        ListingBrokerCommission = listing.ListingBrokerCommission;
        ListingBrokerCommissionUnit = listing.ListingBrokerCommissionUnit.Key;
        TenantBrokerCommission = listing.TenantBrokerCommission;
        TenantBrokerCommissionUnit = listing.TenantBrokerCommissionUnit.Key;
        TransactionRemarks = listing.TransactionRemarks;
        HasAirConditioner = listing.HasAirConditioner;
        HasWoodFlooring = listing.HasWoodFlooring;
        HasVideoIntercom = listing.HasVideoIntercom;
        HasBathReheating = listing.HasBathReheating;
        HasGasStove = listing.HasGasStove;
        HasInductionCooktop = listing.HasInductionCooktop;
        HasMultipleBurners = listing.HasMultipleBurners;
        HasSystemKitchen = listing.HasSystemKitchen;
        HasLoft = listing.HasLoft;
        HasIndoorWashingMachineSpace = listing.HasIndoorWashingMachineSpace;
        HasBathroomDryer = listing.HasBathroomDryer;
        HasSeparateBathAndToilet = listing.HasSeparateBathAndToilet;
        HasSeparateWashbasin = listing.HasSeparateWashbasin;
        HasWarmWaterToiletSeat = listing.HasWarmWaterToiletSeat;
        ApplianceRemarks = listing.ApplianceRemarks;
        Pictures = Array.AsReadOnly(listing.Pictures.Select(ToPictureDraft).ToArray());
        PicturesToBeDeleted = Array.AsReadOnly(listing.PicturesToBeDeleted.Select(ToPictureDraft).ToArray());
        Pdfs = Array.AsReadOnly(listing.Pdfs.Select(ToPdfDraft).ToArray());
        PdfsToBeDeleted = Array.AsReadOnly(listing.PdfsToBeDeleted.Select(ToPdfDraft).ToArray());
        LessorIds = Array.AsReadOnly(listing.Lessors.Select(person => person.Id).ToArray());
        LessorsToBeDeleted = Array.AsReadOnly(listing.LessorsToBeDeleted.Select(person => person.Id).ToArray());
        BrokerIds = Array.AsReadOnly(listing.Brokers.Select(person => person.Id).ToArray());
        BrokersToBeDeleted = Array.AsReadOnly(listing.BrokersToBeDeleted.Select(person => person.Id).ToArray());
    }

    public string Id { get; }
    public string PropertyId { get; }
    public EntityStatus Status { get; }
    public EntityStatus PropertyStatus { get; }
    public string PropertyName { get; }
    public string Name { get; }
    public bool IsPropertyUnitOwnership { get; }
    public FloorPlanType FloorPlan { get; }
    public FloorPlanExtraType FloorPlanExtra { get; }
    public FloorGroundType FloorGround { get; }
    public string FloorExclusiveAreaSqm { get; }
    public string FloorNumber { get; }
    public bool IsCornerRoom { get; }
    public ExposureDirectionType ExposureDirection { get; }
    public AvailabilityStatusType AvailabilityStatus { get; }
    public OccupancyStatus OccupancyStatus { get; }
    public bool IsAvailableForRentNow { get; }
    public AvailabilityMonthType AvailabilityMonth { get; }
    public AvailabilityPeriodType AvailabilityPeriod { get; }
    public bool IsAvailableForImmediateMoveIn { get; }
    public DateTimeOffset? OccupancyStatusCheckedAt { get; }
    public string Remarks { get; }
    public decimal Chinryou { get; }
    public string ManagementFee { get; }
    public FeeUnitType ManagementFeeUnit { get; }
    public string SecurityDeposit { get; }
    public FeeUnitType SecurityDepositUnit { get; }
    public string KeyMoney { get; }
    public FeeUnitType KeyMoneyUnit { get; }
    public ContractType ContractType { get; }
    public string ContractPeriodYears { get; }
    public string RenewalFee { get; }
    public FeeUnitType RenewalFeeUnit { get; }
    public string RecontractFee { get; }
    public FeeUnitType RecontractFeeUnit { get; }
    public string GuarantorCompany { get; }
    public string HousingInsurance { get; }
    public string OtherFees { get; }
    public bool PetFriendly { get; }
    public bool TwoPersonOccupancy { get; }
    public bool NoGuarantorNeeded { get; }
    public bool OfficeUseAllowed { get; }
    public string OtherConditions { get; }
    public string ContractRemarks { get; }
    public ListingHandlingType HandlingType { get; }
    public ListingTransactionType TransactionType { get; }
    public string LessorCommission { get; }
    public ListingCommissionUnitType LessorCommissionUnit { get; }
    public string LesseeCommission { get; }
    public ListingCommissionUnitType LesseeCommissionUnit { get; }
    public string ListingBrokerCommission { get; }
    public ListingCommissionUnitType ListingBrokerCommissionUnit { get; }
    public string TenantBrokerCommission { get; }
    public ListingCommissionUnitType TenantBrokerCommissionUnit { get; }
    public string TransactionRemarks { get; }
    public bool HasAirConditioner { get; }
    public bool HasWoodFlooring { get; }
    public bool HasVideoIntercom { get; }
    public bool HasBathReheating { get; }
    public bool HasGasStove { get; }
    public bool HasInductionCooktop { get; }
    public bool HasMultipleBurners { get; }
    public bool HasSystemKitchen { get; }
    public bool HasLoft { get; }
    public bool HasIndoorWashingMachineSpace { get; }
    public bool HasBathroomDryer { get; }
    public bool HasSeparateBathAndToilet { get; }
    public bool HasSeparateWashbasin { get; }
    public bool HasWarmWaterToiletSeat { get; }
    public string ApplianceRemarks { get; }
    public IReadOnlyList<ResidentialListingPictureDraft> Pictures { get; }
    public IReadOnlyList<ResidentialListingPictureDraft> PicturesToBeDeleted { get; }
    public IReadOnlyList<ResidentialListingPdfDraft> Pdfs { get; }
    public IReadOnlyList<ResidentialListingPdfDraft> PdfsToBeDeleted { get; }
    public IReadOnlyList<string> LessorIds { get; }
    public IReadOnlyList<string> LessorsToBeDeleted { get; }
    public IReadOnlyList<string> BrokerIds { get; }
    public IReadOnlyList<string> BrokersToBeDeleted { get; }

    public static ResidentialListingDraft FromListing(Listing listing)
    {
        ArgumentNullException.ThrowIfNull(listing);
        return new(listing);
    }

    // Call only after the previous editor has closed; the retained objects are mutated in place.
    public Listing ToEditorModel(IEnumerable<ListingPicture> retainedPictures, IEnumerable<ListingPdf> retainedPdfs, IEnumerable<PersonBase> retainedLessors, IEnumerable<PersonBase> retainedBrokers)
    {
        ArgumentNullException.ThrowIfNull(retainedPictures);
        ArgumentNullException.ThrowIfNull(retainedPdfs);
        ArgumentNullException.ThrowIfNull(retainedLessors);
        ArgumentNullException.ThrowIfNull(retainedBrokers);
        var pictures = retainedPictures.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var pdfs = retainedPdfs.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var lessors = retainedLessors.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var brokers = retainedBrokers.ToDictionary(item => item.Id, StringComparer.Ordinal);
        EnsureRetained(Pictures.Select(item => item.Id).Concat(PicturesToBeDeleted.Select(item => item.Id)), pictures, "picture");
        EnsureRetained(Pdfs.Select(item => item.Id).Concat(PdfsToBeDeleted.Select(item => item.Id)), pdfs, "PDF");
        EnsureRetained(LessorIds.Concat(LessorsToBeDeleted), lessors, "lessor");
        EnsureRetained(BrokerIds.Concat(BrokersToBeDeleted), brokers, "broker");

        var listing = new Listing(Id, Status, PropertyId, PropertyStatus, IsPropertyUnitOwnership, PropertyName);
        listing.SetName(Name);
        listing.SetFloorPlanTypeLabel(new(FloorPlan));
        listing.SetFloorPlanExtraTypeLabel(new(FloorPlanExtra));
        listing.SetFloorGroundTypeLabel(new(FloorGround));
        listing.SetFloorExclusiveAreaSqm(FloorExclusiveAreaSqm);
        listing.SetFloorNumber(FloorNumber);
        listing.SetIsCornerRoom(IsCornerRoom);
        listing.SetExposureDirectionTypeLabel(new(ExposureDirection));
        listing.SetAvailabilityStatusTypeLabel(new(AvailabilityStatus));
        listing.SetOccupancyStatusLabel(new(OccupancyStatus));
        listing.SetIsAvailableForRentNow(IsAvailableForRentNow);
        listing.SetAvailabilityMonthTypeLabel(new(AvailabilityMonth));
        listing.SetAvailabilityPeriodTypeLabel(new(AvailabilityPeriod));
        listing.SetIsAvailableForImmediateMoveIn(IsAvailableForImmediateMoveIn);
        listing.SetOccupancyStatusCheckedAt(OccupancyStatusCheckedAt);
        listing.SetRemarks(Remarks);
        listing.SetChinryou(Chinryou);
        listing.SetManagementFee(ManagementFee);
        listing.SetManagementFeeUnitTypeLabel(new(ManagementFeeUnit));
        listing.SetSecurityDeposit(SecurityDeposit);
        listing.SetSecurityDepositUnitTypeLabel(new(SecurityDepositUnit));
        listing.SetKeyMoney(KeyMoney);
        listing.SetKeyMoneyUnitTypeLabel(new(KeyMoneyUnit));
        listing.SetContractTypeLabel(new(ContractType));
        listing.SetContractPeriodYears(ContractPeriodYears);
        listing.SetRenewalFee(RenewalFee);
        listing.SetRenewalFeeUnitTypeLabel(new(RenewalFeeUnit));
        listing.SetRecontractFee(RecontractFee);
        listing.SetRecontractFeeUnitTypeLabel(new(RecontractFeeUnit));
        listing.SetGuarantorCompany(GuarantorCompany);
        listing.SetHousingInsurance(HousingInsurance);
        listing.SetOtherFees(OtherFees);
        listing.SetPetFriendly(PetFriendly);
        listing.SetTwoPersonOccupancy(TwoPersonOccupancy);
        listing.SetNoGuarantorNeeded(NoGuarantorNeeded);
        listing.SetOfficeUseAllowed(OfficeUseAllowed);
        listing.SetOtherConditions(OtherConditions);
        listing.SetContractRemarks(ContractRemarks);
        listing.SetHandlingTypeLabel(new(HandlingType));
        listing.SetTransactionTypeLabel(new(TransactionType));
        listing.SetLessorCommission(LessorCommission);
        listing.SetLessorCommissionUnitTypeLabel(new(LessorCommissionUnit));
        listing.SetLesseeCommission(LesseeCommission);
        listing.SetLesseeCommissionUnitTypeLabel(new(LesseeCommissionUnit));
        listing.SetListingBrokerCommission(ListingBrokerCommission);
        listing.SetListingBrokerCommissionUnitTypeLabel(new(ListingBrokerCommissionUnit));
        listing.SetTenantBrokerCommission(TenantBrokerCommission);
        listing.SetTenantBrokerCommissionUnitTypeLabel(new(TenantBrokerCommissionUnit));
        listing.SetTransactionRemarks(TransactionRemarks);
        listing.SetHasAirConditioner(HasAirConditioner);
        listing.SetHasWoodFlooring(HasWoodFlooring);
        listing.SetHasVideoIntercom(HasVideoIntercom);
        listing.SetHasBathReheating(HasBathReheating);
        listing.SetHasGasStove(HasGasStove);
        listing.SetHasInductionCooktop(HasInductionCooktop);
        listing.SetHasMultipleBurners(HasMultipleBurners);
        listing.SetHasSystemKitchen(HasSystemKitchen);
        listing.SetHasLoft(HasLoft);
        listing.SetHasIndoorWashingMachineSpace(HasIndoorWashingMachineSpace);
        listing.SetHasBathroomDryer(HasBathroomDryer);
        listing.SetHasSeparateBathAndToilet(HasSeparateBathAndToilet);
        listing.SetHasSeparateWashbasin(HasSeparateWashbasin);
        listing.SetHasWarmWaterToiletSeat(HasWarmWaterToiletSeat);
        listing.SetApplianceRemarks(ApplianceRemarks);
        foreach (var draft in Pictures) listing.Pictures.Add(RestorePicture(draft, pictures[draft.Id]));
        foreach (var draft in PicturesToBeDeleted) listing.PicturesToBeDeleted.Add(RestorePicture(draft, pictures[draft.Id]));
        foreach (var draft in Pdfs) listing.Pdfs.Add(RestorePdf(draft, pdfs[draft.Id]));
        foreach (var draft in PdfsToBeDeleted) listing.PdfsToBeDeleted.Add(RestorePdf(draft, pdfs[draft.Id]));
        foreach (var id in LessorIds) listing.Lessors.Add(lessors[id]);
        foreach (var id in LessorsToBeDeleted) listing.LessorsToBeDeleted.Add(lessors[id]);
        foreach (var id in BrokerIds) listing.Brokers.Add(brokers[id]);
        foreach (var id in BrokersToBeDeleted) listing.BrokersToBeDeleted.Add(brokers[id]);
        listing.SetIsModified(false);
        return listing;
    }

    private static void EnsureRetained<T>(IEnumerable<string> ids, Dictionary<string, T> retained, string kind)
    {
        foreach (var id in ids)
        {
            if (!retained.ContainsKey(id)) throw new InvalidOperationException($"Missing retained residential listing {kind}: {id}");
        }
    }

    private static ResidentialListingPictureDraft ToPictureDraft(ListingPicture picture) => new(picture.Id, picture.ImageFilename, picture.PictureType.Key, picture.Description, picture.IsMain, picture.Status);
    private static ResidentialListingPdfDraft ToPdfDraft(ListingPdf pdf) => new(pdf.Id, pdf.PdfFilename, pdf.ThumbnailFilename, pdf.PdfType.Key, pdf.Description, pdf.IsMain, pdf.Status);

    private static ListingPicture RestorePicture(ResidentialListingPictureDraft draft, ListingPicture picture)
    {
        picture.ImageFilename = draft.ImageFilename;
        picture.PictureType = picture.RoomPictureTypes.First(item => item.Key == draft.PictureType);
        picture.Description = draft.Description;
        picture.IsMain = draft.IsMain;
        picture.SetStatus(draft.Status);
        picture.SetIsModified(false);
        return picture;
    }

    private static ListingPdf RestorePdf(ResidentialListingPdfDraft draft, ListingPdf pdf)
    {
        pdf.PdfFilename = draft.PdfFilename;
        pdf.ThumbnailFilename = draft.ThumbnailFilename;
        pdf.PdfType = pdf.RoomPdfTypes.First(item => item.Key == draft.PdfType);
        pdf.Description = draft.Description;
        pdf.IsMain = draft.IsMain;
        pdf.SetStatus(draft.Status);
        pdf.SetIsModified(false);
        return pdf;
    }
}
