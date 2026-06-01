using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ZumenSearch.Models.Rent.Residentials.Bldg;

#pragma warning disable IDE0290 // Use primary constructor


// Kind：物件種目（アパート・マンション・一戸建て・他）
public enum EnumKinds
{
    Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
}

internal sealed class Kind(EnumKinds key)
{
    private Dictionary<EnumKinds, string> BuildingKindTypeDictionary
    {
        get;
    } = new Dictionary<EnumKinds, string>()
        {
            {EnumKinds.Unspecified, "未指定"},
            {EnumKinds.Apartment, "アパート"},
            {EnumKinds.Mansion, "マンション"},
            {EnumKinds.House, "一戸建て"},
            {EnumKinds.TerraceHouse, "テラスハウス"},
            {EnumKinds.TownHouse, "タウンハウス"},
            {EnumKinds.ShareHouse, "シェアハウス"},
            {EnumKinds.Dormitory, "寮・下宿"}
        };

    public string Label => BuildingKindTypeDictionary[Key];

    public EnumKinds Key => key;
};

// Structure: 建物構造
public enum EnumStructure
{
    Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
}

internal sealed class Structure(EnumStructure key)
{
    private Dictionary<EnumStructure, string> BuildingStructureTypeDictionary
    {
        get;
    } = new Dictionary<EnumStructure, string>()
        {
            {EnumStructure.Unspecified, "未指定"},
            {EnumStructure.Wood, "木造"},
            {EnumStructure.Block, "ブロック造"},
            {EnumStructure.LightSteel, "軽量鉄骨造"},
            {EnumStructure.Steel, "鉄骨造"},
            {EnumStructure.RC, "鉄筋コンクリート(RC)造"},
            {EnumStructure.SRC, "鉄骨鉄筋コンクリート(SRC)造"},
            {EnumStructure.ALC, "軽量気泡コンクリート(ALC)造"},
            {EnumStructure.PC, "プレキャストコンクリート(PC)造"},
            {EnumStructure.HPC, "鉄骨プレキャストコンクリート(HPC)造"},
            {EnumStructure.RB, "鉄筋ブロック造"},
            {EnumStructure.CFT, "コンクリート充填鋼管(CFT)造"},
            {EnumStructure.Other, "その他"}

        };
    public string Label => BuildingStructureTypeDictionary[Key];

    public EnumStructure Key => key;
};

//
internal sealed class ElectricKind
{
    public string Label { get; set; }

    public EntryResidential.EnumElectricKind Key { get; set; }

    public ElectricKind(EntryResidential.EnumElectricKind key, string label)
    {
        Key = key;
        Label = label;
    }
};
/*
internal sealed partial class ElectricKind : ObservableObject
{
    [ObservableProperty]
    public partial string Label { get; set; }

    [ObservableProperty]
    public partial Models.Rent.Residentials.EntryResidential.EnumElectricKind Key { get; set; }

    public ElectricKind(Models.Rent.Residentials.EntryResidential.EnumElectricKind key, string label)
    {
        Key = key;
        Label = label;
    }
};
*/


internal class KanriShutai(EntryResidential.EnumKanriShutai key, string label)
{
    public string Label => label;

    public EntryResidential.EnumKanriShutai Key => key;
};
