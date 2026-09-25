using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ZumenSearch.Models.Sale.Residentials;

#pragma warning disable IDE0290 // Use primary constructor

public enum EnumResidentialKinds
{
    Unspecified,
    Apartment,
    Mansion,
    House,
    TerraceHouse,
    TownHouse,
    ShareHouse,
    Dormitory
}

public sealed class Kind(EnumResidentialKinds key)
{
    private static readonly IReadOnlyDictionary<EnumResidentialKinds, string> Labels =
        new Dictionary<EnumResidentialKinds, string>
        {
            [EnumResidentialKinds.Unspecified] = "未指定",
            [EnumResidentialKinds.Apartment] = "アパート",
            [EnumResidentialKinds.Mansion] = "マンション",
            [EnumResidentialKinds.House] = "一戸建て",
            [EnumResidentialKinds.TerraceHouse] = "テラスハウス",
            [EnumResidentialKinds.TownHouse] = "タウンハウス",
            [EnumResidentialKinds.ShareHouse] = "シェアハウス",
            [EnumResidentialKinds.Dormitory] = "寮・下宿"
        };

    public EnumResidentialKinds Key => key;

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
    RB,
    CFT,
    Other
}

public sealed class Structure(EnumStructures key)
{
    private static readonly IReadOnlyDictionary<EnumStructures, string> Labels =
        new Dictionary<EnumStructures, string>
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
            [EnumStructures.RB] = "鉄筋ブロック造",
            [EnumStructures.CFT] = "コンクリート充填鋼管(CFT)造",
            [EnumStructures.Other] = "その他"
        };

    public EnumStructures Key => key;

    public string Label => Labels[Key];
}