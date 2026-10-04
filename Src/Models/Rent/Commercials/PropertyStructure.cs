using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyStructure(EnumStructures key)
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
