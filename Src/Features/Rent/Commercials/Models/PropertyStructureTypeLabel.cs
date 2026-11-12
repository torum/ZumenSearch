namespace ZumenSearch.Models.Rent.Commercials;

public sealed class PropertyStructureTypeLabel(StructureType key)
{
    private static readonly Dictionary<
        StructureType,
        string> Labels = new Dictionary<
            StructureType,
            string>
        {
            [StructureType.Unspecified] = "未指定",
            [StructureType.Wood] = "木造",
            [StructureType.Block] = "ブロック造",
            [StructureType.LightSteel] = "軽量鉄骨造",
            [StructureType.Steel] = "鉄骨造",
            [StructureType.RC] = "鉄筋コンクリート(RC)造",
            [StructureType.SRC] = "鉄骨鉄筋コンクリート(SRC)造",
            [StructureType.ALC] = "軽量気泡コンクリート(ALC)造",
            [StructureType.PC] = "プレキャストコンクリート(PC)造",
            [StructureType.HPC] = "鉄骨プレキャストコンクリート(HPC)造",
            [StructureType.Other] = "その他"
        };

    public StructureType Key => key;

    public string Label => Labels[Key];
}
