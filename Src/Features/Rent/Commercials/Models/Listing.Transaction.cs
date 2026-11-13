namespace ZumenSearch.Models.Rent.Commercials.Listing;

public sealed partial class Listing
{
    public ListingHandlingTypeLabel HandlingType { get; private set; } = new(ListingHandlingType.Unspecified);
    public ListingTransactionTypeLabel TransactionType { get; private set; } = new(ListingTransactionType.Unspecified);
    public string LessorCommission { get; private set; } = string.Empty;
    public string LesseeCommission { get; private set; } = string.Empty;
    public string ListingBrokerCommission { get; private set; } = string.Empty;
    public string TenantBrokerCommission { get; private set; } = string.Empty;
    public ListingCommissionUnitTypeLabel LessorCommissionUnit { get; private set; } = new(ListingCommissionUnitType.Percent);
    public ListingCommissionUnitTypeLabel LesseeCommissionUnit { get; private set; } = new(ListingCommissionUnitType.Percent);
    public ListingCommissionUnitTypeLabel ListingBrokerCommissionUnit { get; private set; } = new(ListingCommissionUnitType.Percent);
    public ListingCommissionUnitTypeLabel TenantBrokerCommissionUnit { get; private set; } = new(ListingCommissionUnitType.Percent);
    public string TransactionRemarks { get; private set; } = string.Empty;

    public void SetHandlingTypeLabel(ListingHandlingTypeLabel value) { HandlingType = value; IsModified = true; }
    public void SetHandlingTypeLabelFromString(string value) => SetHandlingTypeLabel(new(ParseType<ListingHandlingType>(value)));
    public void SetTransactionTypeLabel(ListingTransactionTypeLabel value) { TransactionType = value; IsModified = true; }
    public void SetTransactionTypeLabelFromString(string value) => SetTransactionTypeLabel(new(ParseType<ListingTransactionType>(value)));
    public void SetLessorCommission(string value) { LessorCommission = value; IsModified = true; }
    public void SetLesseeCommission(string value) { LesseeCommission = value; IsModified = true; }
    public void SetListingBrokerCommission(string value) { ListingBrokerCommission = value; IsModified = true; }
    public void SetTenantBrokerCommission(string value) { TenantBrokerCommission = value; IsModified = true; }
    public void SetLessorCommissionUnitTypeLabel(ListingCommissionUnitTypeLabel value) { LessorCommissionUnit = value; IsModified = true; }
    public void SetLessorCommissionUnitTypeLabelFromString(string value) => SetLessorCommissionUnitTypeLabel(new(ParseType<ListingCommissionUnitType>(value)));
    public void SetLesseeCommissionUnitTypeLabel(ListingCommissionUnitTypeLabel value) { LesseeCommissionUnit = value; IsModified = true; }
    public void SetLesseeCommissionUnitTypeLabelFromString(string value) => SetLesseeCommissionUnitTypeLabel(new(ParseType<ListingCommissionUnitType>(value)));
    public void SetListingBrokerCommissionUnitTypeLabel(ListingCommissionUnitTypeLabel value) { ListingBrokerCommissionUnit = value; IsModified = true; }
    public void SetListingBrokerCommissionUnitTypeLabelFromString(string value) => SetListingBrokerCommissionUnitTypeLabel(new(ParseType<ListingCommissionUnitType>(value)));
    public void SetTenantBrokerCommissionUnitTypeLabel(ListingCommissionUnitTypeLabel value) { TenantBrokerCommissionUnit = value; IsModified = true; }
    public void SetTenantBrokerCommissionUnitTypeLabelFromString(string value) => SetTenantBrokerCommissionUnitTypeLabel(new(ParseType<ListingCommissionUnitType>(value)));
    public void SetTransactionRemarks(string value) { TransactionRemarks = value; IsModified = true; }
}
