using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials.Listing;

public sealed partial class Listing : ListingBase
{
    public bool IsPropertyUnitOwnership { get; set; }

    public string PropertyName
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public decimal Chinryou
    {
        get;
        private set { field = value; IsModified = true; }
    }

    /*
    [ObservableProperty]
    public partial decimal KyouekiFee { get; set; }

    [ObservableProperty]
    public partial decimal Shikikin { get; set; }

    [ObservableProperty]
    public partial string ShikikinUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial decimal Reikin { get; set; }

    [ObservableProperty]
    public partial string ReikinUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial decimal RenewalFee { get; set; }

    [ObservableProperty]
    public partial string RenewalFeeUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial decimal RecontractFee { get; set; }

    [ObservableProperty]
    public partial string RecontractFeeUnit { get; set; } = "円";

    [ObservableProperty]
    public partial decimal FloorArea { get; set; }

    [ObservableProperty]
    public partial string Usage { get; set; } = "未指定";

    [ObservableProperty]
    public partial string BusinessHours { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ParkingAvailable { get; set; }

    [ObservableProperty]
    public partial string OtherConditions { get; set; } = string.Empty;
    */

    public string Remarks
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public Listing(
        string id,
        EntityStatus status,
        string propertyId,
        EntityStatus propertyStatus,
        bool isPropertyUnitOwnership,
        string propertyName)
        : base(
            id,
            status,
            propertyId,
            propertyStatus,
            PropertyContextType.RentCommercial)
    {
        IsPropertyUnitOwnership = isPropertyUnitOwnership;
        PropertyName = propertyName;
        SetIsModified(false);
    }

    #region == Setter Methods ==

    public void SetPropertyName(string propertyName)
    {
        PropertyName = propertyName;
    }

    public void SetChinryou(decimal chinryou)
    {
        Chinryou = chinryou;
    }

    public void SetRemarks(string remarks)
    {
        Remarks = remarks;
    }


    #endregion
}