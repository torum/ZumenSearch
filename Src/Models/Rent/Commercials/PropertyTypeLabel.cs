namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyTypeLabel(PropertyKindType key)
{
    private static readonly Dictionary<
        PropertyKindType,
        string> Labels = new Dictionary<
            PropertyKindType,
            string>
        {
            [PropertyKindType.Unspecified] = "未指定",
            [PropertyKindType.Office] = "事務所",
            [PropertyKindType.Retail] = "店舗",
            [PropertyKindType.Warehouse] = "倉庫",
            [PropertyKindType.Factory] = "工場",
            [PropertyKindType.Clinic] = "診療所",
            [PropertyKindType.Restaurant] = "飲食店",
            [PropertyKindType.Hotel] = "ホテル・旅館",
            [PropertyKindType.Land] = "事業用土地",
            [PropertyKindType.Other] = "その他"
        };

    public PropertyKindType Key => key;

    public string Label => Labels[Key];
}
