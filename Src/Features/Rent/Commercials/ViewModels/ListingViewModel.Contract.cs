using ZumenSearch.Models.Rent.Commercials.Listing;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class ListingViewModel
{
    public IReadOnlyList<ListingFeeUnitTypeLabel> FeeUnitTypes { get; } = ListingFeeUnitTypeLabel.GetAll();
    public IReadOnlyList<ListingContractTaxTypeLabel> ContractTaxTypes { get; } = ListingContractTaxTypeLabel.GetAll();
    public IReadOnlyList<ListingContractTypeLabel> ContractTypes { get; } = ListingContractTypeLabel.GetAll();

    public string ManagementFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string SecurityDeposit { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string GuaranteeDeposit { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string KeyMoney { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string RightsMoney { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string DepositDeduction { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string RenewalFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string RecontractFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ContractPeriodYears { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string GuarantorCompany { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string Insurance { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string OtherFees { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ContractConditions { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ContractRemarks { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public ListingFeeUnitTypeLabel SelectedSecurityDepositUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedGuaranteeDepositUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedKeyMoneyUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedRightsMoneyUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedDepositDeductionUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedRenewalFeeUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedRecontractFeeUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Yen); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingContractTaxTypeLabel SelectedRentTax { get => field ?? ContractTaxTypes.First(x => x.Key == ListingContractTaxType.Unconfirmed); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingContractTaxTypeLabel SelectedManagementFeeTax { get => field ?? ContractTaxTypes.First(x => x.Key == ListingContractTaxType.Unconfirmed); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingContractTypeLabel SelectedContractType { get => field ?? ContractTypes.First(x => x.Key == ListingContractType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }

    private void SetContractValue<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, value, propertyName)) MarkEdited();
    }

    private void PopulateContractValues()
    {
        ManagementFee = Format(_unit.ManagementFee);
        SecurityDeposit = Format(_unit.SecurityDeposit);
        GuaranteeDeposit = Format(_unit.GuaranteeDeposit);
        KeyMoney = Format(_unit.KeyMoney);
        RightsMoney = Format(_unit.RightsMoney);
        DepositDeduction = Format(_unit.DepositDeduction);
        RenewalFee = Format(_unit.RenewalFee);
        RecontractFee = Format(_unit.RecontractFee);
        ContractPeriodYears = _unit.ContractPeriodYears?.ToString(System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty;
        SelectedSecurityDepositUnit = FeeUnitTypes.First(x => x.Key == _unit.SecurityDepositUnit.Key);
        SelectedGuaranteeDepositUnit = FeeUnitTypes.First(x => x.Key == _unit.GuaranteeDepositUnit.Key);
        SelectedKeyMoneyUnit = FeeUnitTypes.First(x => x.Key == _unit.KeyMoneyUnit.Key);
        SelectedRightsMoneyUnit = FeeUnitTypes.First(x => x.Key == _unit.RightsMoneyUnit.Key);
        SelectedDepositDeductionUnit = FeeUnitTypes.First(x => x.Key == _unit.DepositDeductionUnit.Key);
        SelectedRenewalFeeUnit = FeeUnitTypes.First(x => x.Key == _unit.RenewalFeeUnit.Key);
        SelectedRecontractFeeUnit = FeeUnitTypes.First(x => x.Key == _unit.RecontractFeeUnit.Key);
        SelectedRentTax = ContractTaxTypes.First(x => x.Key == _unit.RentTax.Key);
        SelectedManagementFeeTax = ContractTaxTypes.First(x => x.Key == _unit.ManagementFeeTax.Key);
        SelectedContractType = ContractTypes.First(x => x.Key == _unit.ContractType.Key);
        GuarantorCompany = _unit.GuarantorCompany;
        Insurance = _unit.Insurance;
        OtherFees = _unit.OtherFees;
        ContractConditions = _unit.ContractConditions;
        ContractRemarks = _unit.ContractRemarks;
    }

    private bool SetContractValues(Models.Rent.Commercials.Listing.Listing unit)
    {
        (string Label, string Value, Action<decimal> Set)[] values =
        [
            ("共益費・管理費", ManagementFee, unit.SetManagementFee),
            ("敷金", SecurityDeposit, unit.SetSecurityDeposit),
            ("保証金", GuaranteeDeposit, unit.SetGuaranteeDeposit),
            ("礼金", KeyMoney, unit.SetKeyMoney),
            ("権利金", RightsMoney, unit.SetRightsMoney),
            ("敷引・償却金", DepositDeduction, unit.SetDepositDeduction),
            ("更新料", RenewalFee, unit.SetRenewalFee),
            ("再契約手数料", RecontractFee, unit.SetRecontractFee)
        ];
        var parsedValues = new decimal[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var (label, value, _) = values[index];
            if (!TryParseRent(value, out parsedValues[index]))
            {
                InfoBarErrorMessage = $"{label}には0以上の数値を入力してください。";
                IsInfoBarErrorOpen = true;
                _navigationService.NavigateTo(ContractPageName, this);
                return false;
            }
        }
        decimal? contractPeriodYears;
        if (string.IsNullOrWhiteSpace(ContractPeriodYears))
        {
            contractPeriodYears = null;
        }
        else if (TryParseRent(ContractPeriodYears, out var period))
        {
            contractPeriodYears = period;
        }
        else
        {
            InfoBarErrorMessage = "契約期間には0以上の数値を入力してください。";
            IsInfoBarErrorOpen = true;
            _navigationService.NavigateTo(ContractPageName, this);
            return false;
        }
        for (var index = 0; index < values.Length; index++) values[index].Set(parsedValues[index]);
        unit.SetContractPeriodYears(contractPeriodYears);
        unit.SetSecurityDepositUnitTypeLabel(SelectedSecurityDepositUnit);
        unit.SetGuaranteeDepositUnitTypeLabel(SelectedGuaranteeDepositUnit);
        unit.SetKeyMoneyUnitTypeLabel(SelectedKeyMoneyUnit);
        unit.SetRightsMoneyUnitTypeLabel(SelectedRightsMoneyUnit);
        unit.SetDepositDeductionUnitTypeLabel(SelectedDepositDeductionUnit);
        unit.SetRenewalFeeUnitTypeLabel(SelectedRenewalFeeUnit);
        unit.SetRecontractFeeUnitTypeLabel(SelectedRecontractFeeUnit);
        unit.SetRentTaxTypeLabel(SelectedRentTax);
        unit.SetManagementFeeTaxTypeLabel(SelectedManagementFeeTax);
        unit.SetContractTypeLabel(SelectedContractType);
        unit.SetGuarantorCompany(GuarantorCompany);
        unit.SetInsurance(Insurance);
        unit.SetOtherFees(OtherFees);
        unit.SetContractConditions(ContractConditions);
        unit.SetContractRemarks(ContractRemarks);
        return true;
    }
}
