using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Commercials.Listing;

public sealed partial class Listing : ListingBase
{
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

    public bool IsPropertyUnitOwnership { get; set; }
    public ObservableCollection<ListingPicture> Pictures { get; set; } = [];
    public List<ListingPicture> PicturesToBeDeleted { get; set; } = [];
    public ObservableCollection<ListingPdf> Pdfs { get; set; } = [];
    public List<ListingPdf> PdfsToBeDeleted { get; set; } = [];
    public ObservableCollection<Models.Base.PersonBase> Lessors { get; set; } = [];
    public ObservableCollection<Models.Base.PersonBase> LessorsToBeDeleted { get; set; } = [];
    public ObservableCollection<Models.Base.PersonBase> Brokers { get; set; } = [];
    public ObservableCollection<Models.Base.PersonBase> BrokersToBeDeleted { get; set; } = [];

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

    public ListingFloorGroundTypeLabel FloorGround { get; private set; } = new(ListingFloorGroundType.AboveGround);
    public int? FloorNumber { get; private set; }
    public decimal? FloorArea { get; private set; }
    public ListingUseTypeLabel Use { get; private set; } = new(ListingUseType.Unspecified);
    public string UseDetails { get; private set; } = string.Empty;
    public ListingAvailabilityStatusTypeLabel AvailabilityStatus { get; private set; } = new(ListingAvailabilityStatusType.Unspecified);
    public bool IsAvailableForRentNow { get; private set; }
    public ListingAvailabilityMonthTypeLabel AvailabilityMonth { get; private set; } = new(ListingAvailabilityMonthType.Unspecified);
    public ListingAvailabilityPeriodTypeLabel AvailabilityPeriod { get; private set; } = new(ListingAvailabilityPeriodType.Unspecified);
    public bool IsAvailableForImmediateMoveIn { get; private set; }

    public string Remarks
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    #region == Setter Methods ==

    private static T ParseType<T>(string value) where T : struct, Enum => Enum.TryParse<T>(value, out var result) && Enum.IsDefined(result) && !int.TryParse(value, out _) ? result : throw new ArgumentException($"Invalid {typeof(T).Name}: {value}");
    public void SetFloorGroundTypeLabel(ListingFloorGroundTypeLabel value) { FloorGround = value; IsModified = true; }
    public void SetFloorGroundTypeLabelFromString(string value) => SetFloorGroundTypeLabel(new(ParseType<ListingFloorGroundType>(value)));
    public void SetFloorNumber(int? value) { if (value is <= 0) throw new ArgumentOutOfRangeException(nameof(value)); FloorNumber = value; IsModified = true; }
    public void SetFloorArea(decimal? value) { if (value is < 0) throw new ArgumentOutOfRangeException(nameof(value)); FloorArea = value; IsModified = true; }
    public void SetUseTypeLabel(ListingUseTypeLabel value) { Use = value; IsModified = true; }
    public void SetUseTypeLabelFromString(string value) => SetUseTypeLabel(new(ParseType<ListingUseType>(value)));
    public void SetUseDetails(string value) { UseDetails = value; IsModified = true; }
    public void SetAvailabilityStatusTypeLabel(ListingAvailabilityStatusTypeLabel value) { AvailabilityStatus = value; IsModified = true; }
    public void SetAvailabilityStatusTypeLabelFromString(string value) => SetAvailabilityStatusTypeLabel(new(ParseType<ListingAvailabilityStatusType>(value)));
    public void SetIsAvailableForRentNow(bool value) { IsAvailableForRentNow = value; IsModified = true; }
    public void SetAvailabilityMonthTypeLabel(ListingAvailabilityMonthTypeLabel value) { AvailabilityMonth = value; IsModified = true; }
    public void SetAvailabilityMonthTypeLabelFromString(string value) => SetAvailabilityMonthTypeLabel(new(ParseType<ListingAvailabilityMonthType>(value)));
    public void SetAvailabilityPeriodTypeLabel(ListingAvailabilityPeriodTypeLabel value) { AvailabilityPeriod = value; IsModified = true; }
    public void SetAvailabilityPeriodTypeLabelFromString(string value) => SetAvailabilityPeriodTypeLabel(new(ParseType<ListingAvailabilityPeriodType>(value)));
    public void SetIsAvailableForImmediateMoveIn(bool value) { IsAvailableForImmediateMoveIn = value; IsModified = true; }

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