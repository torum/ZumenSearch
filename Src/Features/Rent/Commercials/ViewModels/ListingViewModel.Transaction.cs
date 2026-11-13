using System.Globalization;
using System.Text;
using ZumenSearch.Models.Rent.Commercials.Listing;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class ListingViewModel
{
    private const string TransactionPageName = "ZumenSearch.Views.Rent.Commercials.Listing.TransactionPage";

    public IReadOnlyList<ListingHandlingTypeLabel> HandlingTypes { get; } = ListingHandlingTypeLabel.GetAll();
    public IReadOnlyList<ListingTransactionTypeLabel> TransactionTypes { get; } = ListingTransactionTypeLabel.GetAll();
    public IReadOnlyList<ListingCommissionUnitTypeLabel> CommissionUnitTypes { get; } = ListingCommissionUnitTypeLabel.GetAll();
    public ListingHandlingTypeLabel SelectedHandlingType { get => field ?? HandlingTypes.First(x => x.Key == ListingHandlingType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingTransactionTypeLabel SelectedTransactionType { get => field ?? TransactionTypes.First(x => x.Key == ListingTransactionType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }
    public string LessorCommission { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string LesseeCommission { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ListingBrokerCommission { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string TenantBrokerCommission { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public ListingCommissionUnitTypeLabel SelectedLessorCommissionUnit { get => field ?? CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingCommissionUnitTypeLabel SelectedLesseeCommissionUnit { get => field ?? CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingCommissionUnitTypeLabel SelectedListingBrokerCommissionUnit { get => field ?? CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingCommissionUnitTypeLabel SelectedTenantBrokerCommissionUnit { get => field ?? CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent); set { if (value is not null) SetContractValue(ref field, value); } }
    public string TransactionRemarks { get => field ?? string.Empty; set => SetContractValue(ref field, value); }

    private void PopulateTransactionValues()
    {
        SelectedHandlingType = HandlingTypes.First(x => x.Key == _unit.HandlingType.Key);
        SelectedTransactionType = TransactionTypes.First(x => x.Key == _unit.TransactionType.Key);
        LessorCommission = _unit.LessorCommission;
        LesseeCommission = _unit.LesseeCommission;
        ListingBrokerCommission = _unit.ListingBrokerCommission;
        TenantBrokerCommission = _unit.TenantBrokerCommission;
        SelectedLessorCommissionUnit = CommissionUnitTypes.First(x => x.Key == _unit.LessorCommissionUnit.Key);
        SelectedLesseeCommissionUnit = CommissionUnitTypes.First(x => x.Key == _unit.LesseeCommissionUnit.Key);
        SelectedListingBrokerCommissionUnit = CommissionUnitTypes.First(x => x.Key == _unit.ListingBrokerCommissionUnit.Key);
        SelectedTenantBrokerCommissionUnit = CommissionUnitTypes.First(x => x.Key == _unit.TenantBrokerCommissionUnit.Key);
        TransactionRemarks = _unit.TransactionRemarks;
    }

    private bool SetTransactionValues(Models.Rent.Commercials.Listing.Listing unit)
    {
        (string Label, string Value)[] values =
        [
            ("貸主手数料", LessorCommission),
            ("借主手数料", LesseeCommission),
            ("元付手数料", ListingBrokerCommission),
            ("客付け手数料", TenantBrokerCommission)
        ];
        foreach (var (label, value) in values)
        {
            var text = value.Trim().Normalize(NormalizationForm.FormKC);
            if (text.Length > 0 && (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var number) || number < 0))
            {
                InfoBarErrorMessage = $"{label}は0以上の数値で入力してください。";
                IsInfoBarErrorOpen = true;
                if (!_navigationService.IsCurrentPageSameAs(TransactionPageName)) _navigationService.NavigateTo(TransactionPageName, this);
                return false;
            }
        }
        unit.SetHandlingTypeLabel(SelectedHandlingType);
        unit.SetTransactionTypeLabel(SelectedTransactionType);
        unit.SetLessorCommission(LessorCommission);
        unit.SetLesseeCommission(LesseeCommission);
        unit.SetListingBrokerCommission(ListingBrokerCommission);
        unit.SetTenantBrokerCommission(TenantBrokerCommission);
        unit.SetLessorCommissionUnitTypeLabel(SelectedLessorCommissionUnit);
        unit.SetLesseeCommissionUnitTypeLabel(SelectedLesseeCommissionUnit);
        unit.SetListingBrokerCommissionUnitTypeLabel(SelectedListingBrokerCommissionUnit);
        unit.SetTenantBrokerCommissionUnitTypeLabel(SelectedTenantBrokerCommissionUnit);
        unit.SetTransactionRemarks(TransactionRemarks);
        return true;
    }
}
