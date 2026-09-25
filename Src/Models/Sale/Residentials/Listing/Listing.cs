using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Sale.Residentials.Listing;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

public sealed partial class Listing : ListingBase
{
    public bool IsPropertyUnitOwnership { get; set; }

    public string PropertyName
    {
        get => field ?? string.Empty;
        set => SetProperty(ref field, value);
    }

    // Sale-specific fields

    [ObservableProperty]
    public partial decimal SalePrice { get; set; }

    [ObservableProperty]
    public partial decimal ManagementFee { get; set; }

    [ObservableProperty]
    public partial decimal RepairReserveFund { get; set; }

    [ObservableProperty]
    public partial string OwnershipType { get; set; } = "所有権";

    [ObservableProperty]
    public partial string OccupancyStatus { get; set; } = "空室";

    [ObservableProperty]
    public partial string DeliveryTiming { get; set; } = "相談";

    [ObservableProperty]
    public partial string Remarks { get; set; } = string.Empty;

    public Listing(
        string id,
        EnumEntryStatus status,
        string propertyId,
        EnumEntryStatus propertyStatus,
        bool isPropertyUnitOwnership,
        string propertyName)
        : base(
            id,
            status,
            propertyId,
            propertyStatus,
            EnumPropertyKind.SaleResidential)
    {
        IsPropertyUnitOwnership = isPropertyUnitOwnership;
        PropertyName = propertyName;
    }
}