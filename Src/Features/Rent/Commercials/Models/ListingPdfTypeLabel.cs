namespace ZumenSearch.Models.Rent.Commercials.Listing;

public enum ListingPdfType
{
    Unspecified,
    Listing,
    FloorPlan,
    Other
}

public sealed class ListingPdfTypeLabel(ListingPdfType key)
{
    public ListingPdfType Key => key;
    public string Label => Key switch
    {
        ListingPdfType.Unspecified => "未指定",
        ListingPdfType.Listing => "募集図面",
        ListingPdfType.FloorPlan => "間取図",
        ListingPdfType.Other => "その他",
        _ => throw new ArgumentOutOfRangeException(nameof(Key))
    };
}
