using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingUseTypeLabel(ListingUseType key)
{
    public ListingUseType Key => key;
    public string Label => Key switch { ListingUseType.Unspecified => "未指定", ListingUseType.Office => "事務所", ListingUseType.Retail => "店舗", ListingUseType.Restaurant => "飲食店", ListingUseType.Warehouse => "倉庫", ListingUseType.Factory => "工場", ListingUseType.Other => "その他", _ => throw new ArgumentOutOfRangeException(nameof(key)) };
    public static IReadOnlyList<ListingUseTypeLabel> GetAll() => Enum.GetValues<ListingUseType>().Select(value => new ListingUseTypeLabel(value)).ToArray();
}
