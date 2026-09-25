using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public enum EnumCommercialKinds
{
    Unspecified,
    Office,
    Retail,
    Warehouse,
    Factory,
    Clinic,
    Restaurant,
    Hotel,
    Land,
    Other
}

public sealed class Kind(EnumCommercialKinds key)
{
    private static readonly IReadOnlyDictionary<
        EnumCommercialKinds,
        string> Labels = new Dictionary<
            EnumCommercialKinds,
            string>
        {
            [EnumCommercialKinds.Unspecified] = "未指定",
            [EnumCommercialKinds.Office] = "事務所",
            [EnumCommercialKinds.Retail] = "店舗",
            [EnumCommercialKinds.Warehouse] = "倉庫",
            [EnumCommercialKinds.Factory] = "工場",
            [EnumCommercialKinds.Clinic] = "診療所",
            [EnumCommercialKinds.Restaurant] = "飲食店",
            [EnumCommercialKinds.Hotel] = "ホテル・旅館",
            [EnumCommercialKinds.Land] = "事業用土地",
            [EnumCommercialKinds.Other] = "その他"
        };

    public EnumCommercialKinds Key => key;

    public string Label => Labels[Key];
}

public enum EnumStructures
{
    Unspecified,
    Wood,
    Block,
    LightSteel,
    Steel,
    RC,
    SRC,
    ALC,
    PC,
    HPC,
    Other
}

public sealed class Structure(EnumStructures key)
{
    private static readonly IReadOnlyDictionary<
        EnumStructures,
        string> Labels = new Dictionary<
            EnumStructures,
            string>
        {
            [EnumStructures.Unspecified] = "未指定",
            [EnumStructures.Wood] = "木造",
            [EnumStructures.Block] = "ブロック造",
            [EnumStructures.LightSteel] = "軽量鉄骨造",
            [EnumStructures.Steel] = "鉄骨造",
            [EnumStructures.RC] = "鉄筋コンクリート(RC)造",
            [EnumStructures.SRC] = "鉄骨鉄筋コンクリート(SRC)造",
            [EnumStructures.ALC] = "軽量気泡コンクリート(ALC)造",
            [EnumStructures.PC] = "プレキャストコンクリート(PC)造",
            [EnumStructures.HPC] = "鉄骨プレキャストコンクリート(HPC)造",
            [EnumStructures.Other] = "その他"
        };

    public EnumStructures Key => key;

    public string Label => Labels[Key];
}
