using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials.Listing;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;
using CommercialModels = ZumenSearch.Presentation.Features.Rent.Commercials.Models;
using ResidentialModels = ZumenSearch.Presentation.Features.Rent.Residentials.Models;
using CommercialEditor = ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel;
using ResidentialEditor = ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class ListingTypedLabelSelectionTests
{
    [TestMethod]
    public void ResidentialSelections_CompareKeysAndKeepCanonicalOptions()
    {
        Action<ResidentialEditor>[] checks =
        [
            e => Verify(e, nameof(e.SelectedFloorPlanType), () => e.SelectedFloorPlanType, v => e.SelectedFloorPlanType = v, e.FloorPlanTypes, v => new ResidentialModels.ListingFloorPlanTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedFloorPlanExtraType), () => e.SelectedFloorPlanExtraType, v => e.SelectedFloorPlanExtraType = v, e.FloorPlanExtraTypes, v => new ResidentialModels.ListingFloorPlanExtraTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedFloorGroundType), () => e.SelectedFloorGroundType, v => e.SelectedFloorGroundType = v, e.FloorGroundTypes, v => new ResidentialModels.ListingFloorGroundTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedExposureDirectionType), () => e.SelectedExposureDirectionType, v => e.SelectedExposureDirectionType = v, e.ExposureDirectionTypes, v => new ResidentialModels.ListingExposureDirectionTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedCurrentStatusType), () => e.SelectedCurrentStatusType, v => e.SelectedCurrentStatusType = v, e.CurrentStatusTypes, v => new ResidentialModels.ListingCurrentStatusTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedAvailabilityMonthType), () => e.SelectedAvailabilityMonthType, v => e.SelectedAvailabilityMonthType = v, e.AvailabilityMonthTypes, v => new ResidentialModels.ListingAvailabilityMonthTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedAvailabilityPeriodType), () => e.SelectedAvailabilityPeriodType, v => e.SelectedAvailabilityPeriodType = v, e.AvailabilityPeriodTypes, v => new ResidentialModels.ListingAvailabilityPeriodTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedManagementFeeUnit), () => e.SelectedManagementFeeUnit, v => e.SelectedManagementFeeUnit = v, e.FeeUnitTypes, v => new ResidentialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty, new ResidentialModels.ListingFeeUnitTypeLabel((Residential.FeeUnitType)999), () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage),
            e => Verify(e, nameof(e.SelectedSecurityDepositUnit), () => e.SelectedSecurityDepositUnit, v => e.SelectedSecurityDepositUnit = v, e.FeeUnitTypes, v => new ResidentialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedKeyMoneyUnit), () => e.SelectedKeyMoneyUnit, v => e.SelectedKeyMoneyUnit = v, e.FeeUnitTypes, v => new ResidentialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedRenewalFeeUnit), () => e.SelectedRenewalFeeUnit, v => e.SelectedRenewalFeeUnit = v, e.FeeUnitTypes, v => new ResidentialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedRecontractFeeUnit), () => e.SelectedRecontractFeeUnit, v => e.SelectedRecontractFeeUnit = v, e.FeeUnitTypes, v => new ResidentialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedContractType), () => e.SelectedContractType, v => e.SelectedContractType = v, e.ContractTypes, v => new ResidentialModels.ListingContractTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedHandlingType), () => e.SelectedHandlingType, v => e.SelectedHandlingType = v, e.HandlingTypes, v => new ResidentialModels.ListingHandlingTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedTransactionType), () => e.SelectedTransactionType, v => e.SelectedTransactionType = v, e.TransactionTypes, v => new ResidentialModels.ListingTransactionTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedLessorCommissionUnit), () => e.SelectedLessorCommissionUnit, v => e.SelectedLessorCommissionUnit = v, e.CommissionUnitTypes, v => new ResidentialModels.ListingCommissionUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedLesseeCommissionUnit), () => e.SelectedLesseeCommissionUnit, v => e.SelectedLesseeCommissionUnit = v, e.CommissionUnitTypes, v => new ResidentialModels.ListingCommissionUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedListingBrokerCommissionUnit), () => e.SelectedListingBrokerCommissionUnit, v => e.SelectedListingBrokerCommissionUnit = v, e.CommissionUnitTypes, v => new ResidentialModels.ListingCommissionUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedTenantBrokerCommissionUnit), () => e.SelectedTenantBrokerCommissionUnit, v => e.SelectedTenantBrokerCommissionUnit = v, e.CommissionUnitTypes, v => new ResidentialModels.ListingCommissionUnitTypeLabel(v.Key), () => e.IsDirty)
        ];
        var propertyId = $"listing-labels-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        try
        {
            foreach (var check in checks)
            {
                var listing = new Residential.Listing("listing", EntityStatus.Saved, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
                using var editor = new ResidentialEditor(listing, false, new NavigationStub(), null!, null!, null!, null!);
                check(editor);
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    public void CommercialSelections_CompareKeysAndKeepCanonicalOptions()
    {
        Action<CommercialEditor>[] checks =
        [
            e => Verify(e, nameof(e.SelectedFloorGroundType), () => e.SelectedFloorGroundType, v => e.SelectedFloorGroundType = v, e.FloorGroundTypes, v => new CommercialModels.ListingFloorGroundTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedUseType), () => e.SelectedUseType, v => e.SelectedUseType = v, e.UseTypes, v => new CommercialModels.ListingUseTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedCurrentStatusType), () => e.SelectedCurrentStatusType, v => e.SelectedCurrentStatusType = v, e.CurrentStatusTypes, v => new CommercialModels.ListingAvailabilityStatusTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedAvailabilityMonthType), () => e.SelectedAvailabilityMonthType, v => e.SelectedAvailabilityMonthType = v, e.AvailabilityMonthTypes, v => new CommercialModels.ListingAvailabilityMonthTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedAvailabilityPeriodType), () => e.SelectedAvailabilityPeriodType, v => e.SelectedAvailabilityPeriodType = v, e.AvailabilityPeriodTypes, v => new CommercialModels.ListingAvailabilityPeriodTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedUnitCondition), () => e.SelectedUnitCondition, v => e.SelectedUnitCondition = v, e.UnitConditionOptions, v => new CommercialModels.ListingUnitConditionTypeLabel(v.Key), () => e.IsDirty, new CommercialModels.ListingUnitConditionTypeLabel((Commercial.ListingUnitConditionType)999), () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage),
            e => Verify(e, nameof(e.SelectedFixtureTransfer), () => e.SelectedFixtureTransfer, v => e.SelectedFixtureTransfer = v, e.FixtureTransferOptions, v => new CommercialModels.ListingFixtureTransferTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedSecurityDepositUnit), () => e.SelectedSecurityDepositUnit, v => e.SelectedSecurityDepositUnit = v, e.FeeUnitTypes, v => new CommercialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedGuaranteeDepositUnit), () => e.SelectedGuaranteeDepositUnit, v => e.SelectedGuaranteeDepositUnit = v, e.FeeUnitTypes, v => new CommercialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedKeyMoneyUnit), () => e.SelectedKeyMoneyUnit, v => e.SelectedKeyMoneyUnit = v, e.FeeUnitTypes, v => new CommercialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedRightsMoneyUnit), () => e.SelectedRightsMoneyUnit, v => e.SelectedRightsMoneyUnit = v, e.FeeUnitTypes, v => new CommercialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedDepositDeductionUnit), () => e.SelectedDepositDeductionUnit, v => e.SelectedDepositDeductionUnit = v, e.FeeUnitTypes, v => new CommercialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedRenewalFeeUnit), () => e.SelectedRenewalFeeUnit, v => e.SelectedRenewalFeeUnit = v, e.FeeUnitTypes, v => new CommercialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedRecontractFeeUnit), () => e.SelectedRecontractFeeUnit, v => e.SelectedRecontractFeeUnit = v, e.FeeUnitTypes, v => new CommercialModels.ListingFeeUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedRentTax), () => e.SelectedRentTax, v => e.SelectedRentTax = v, e.ContractTaxTypes, v => new CommercialModels.ListingContractTaxTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedManagementFeeTax), () => e.SelectedManagementFeeTax, v => e.SelectedManagementFeeTax = v, e.ContractTaxTypes, v => new CommercialModels.ListingContractTaxTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedContractType), () => e.SelectedContractType, v => e.SelectedContractType = v, e.ContractTypes, v => new CommercialModels.ListingContractTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedHandlingType), () => e.SelectedHandlingType, v => e.SelectedHandlingType = v, e.HandlingTypes, v => new CommercialModels.ListingHandlingTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedTransactionType), () => e.SelectedTransactionType, v => e.SelectedTransactionType = v, e.TransactionTypes, v => new CommercialModels.ListingTransactionTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedLessorCommissionUnit), () => e.SelectedLessorCommissionUnit, v => e.SelectedLessorCommissionUnit = v, e.CommissionUnitTypes, v => new CommercialModels.ListingCommissionUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedLesseeCommissionUnit), () => e.SelectedLesseeCommissionUnit, v => e.SelectedLesseeCommissionUnit = v, e.CommissionUnitTypes, v => new CommercialModels.ListingCommissionUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedListingBrokerCommissionUnit), () => e.SelectedListingBrokerCommissionUnit, v => e.SelectedListingBrokerCommissionUnit = v, e.CommissionUnitTypes, v => new CommercialModels.ListingCommissionUnitTypeLabel(v.Key), () => e.IsDirty),
            e => Verify(e, nameof(e.SelectedTenantBrokerCommissionUnit), () => e.SelectedTenantBrokerCommissionUnit, v => e.SelectedTenantBrokerCommissionUnit = v, e.CommissionUnitTypes, v => new CommercialModels.ListingCommissionUnitTypeLabel(v.Key), () => e.IsDirty)
        ];
        var propertyId = $"listing-labels-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        try
        {
            foreach (var check in checks)
            {
                var listing = new Commercial.Listing("listing", EntityStatus.Saved, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
                using var editor = new CommercialEditor(listing, false, new NavigationStub(), null!, null!, null!, null!);
                check(editor);
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    private static void Verify<T>(INotifyPropertyChanged editor, string propertyName, Func<T> get, Action<T> set, IReadOnlyList<T> options, Func<T, T> clone, Func<bool> isDirty, T? invalid = null, Func<bool>? hasError = null, Func<string>? errorMessage = null) where T : class
    {
        var selected = get();
        Assert.IsTrue(options.Any(option => ReferenceEquals(option, selected)), propertyName);
        Assert.IsFalse(isDirty(), propertyName);
        var notifications = 0;
        editor.PropertyChanged += (_, e) => { if (e.PropertyName == propertyName) notifications++; };
        set(clone(selected));
        Assert.AreSame(selected, get(), propertyName);
        Assert.IsFalse(isDirty(), propertyName);
        set(null!);
        Assert.AreSame(selected, get(), propertyName);
        Assert.IsFalse(isDirty(), propertyName);
        if (invalid is not null)
        {
            set(invalid);
            Assert.AreSame(selected, get(), propertyName);
            Assert.IsFalse(isDirty(), propertyName);
            Assert.IsTrue(hasError!(), propertyName);
            StringAssert.Contains(errorMessage!(), propertyName);
            StringAssert.Contains(errorMessage(), "999");
        }
        var next = options.First(option => !ReferenceEquals(option, selected));
        set(clone(next));
        Assert.AreSame(next, get(), propertyName);
        Assert.IsTrue(isDirty(), propertyName);
        Assert.AreEqual(1, notifications, propertyName);
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }
}
