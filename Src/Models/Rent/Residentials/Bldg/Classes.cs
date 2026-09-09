using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ZumenSearch.Models.Rent.Residentials.Bldg;

#pragma warning disable IDE0290 // Use primary constructor

// Kind：物件種目（アパート・マンション・一戸建て・他）
public enum EnumKinds
{
    Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
}

public sealed class Kind(EnumKinds key)
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
public enum EnumStructures
{
    Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
}

public sealed class Structure(EnumStructures key)
{
    private Dictionary<EnumStructures, string> BuildingStructureTypeDictionary
    {
        get;
    } = new Dictionary<EnumStructures, string>()
        {
            {EnumStructures.Unspecified, "未指定"},
            {EnumStructures.Wood, "木造"},
            {EnumStructures.Block, "ブロック造"},
            {EnumStructures.LightSteel, "軽量鉄骨造"},
            {EnumStructures.Steel, "鉄骨造"},
            {EnumStructures.RC, "鉄筋コンクリート(RC)造"},
            {EnumStructures.SRC, "鉄骨鉄筋コンクリート(SRC)造"},
            {EnumStructures.ALC, "軽量気泡コンクリート(ALC)造"},
            {EnumStructures.PC, "プレキャストコンクリート(PC)造"},
            {EnumStructures.HPC, "鉄骨プレキャストコンクリート(HPC)造"},
            {EnumStructures.RB, "鉄筋ブロック造"},
            {EnumStructures.CFT, "コンクリート充填鋼管(CFT)造"},
            {EnumStructures.Other, "その他"}

        };
    public string Label => BuildingStructureTypeDictionary[Key];

    public EnumStructures Key => key;
};

//
public sealed class ElectricKind
{
    public string Label { get; set; }

    public Property.EnumElectricKind Key { get; set; }

    public ElectricKind(Property.EnumElectricKind key, string label)
    {
        Key = key;
        Label = label;
    }
};
/*
public sealed partial class ElectricKind : ObservableObject
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


public class KanriShutai(Property.EnumKanriShutai key, string label)
{
    public string Label => label;

    public Property.EnumKanriShutai Key => key;
};
