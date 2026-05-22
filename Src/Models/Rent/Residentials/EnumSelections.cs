namespace ZumenSearch.Models.Rent.Residentials;

// Kind：物件種目（アパート・マンション・一戸建て・他）
public enum EnumKinds
{
    Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
}

public sealed class Kind(EnumKinds key, string label)
{
    public string Label => label;

    public EnumKinds Key => key;
};

// Structure: 建物構造
public enum EnumStructure
{
    Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
}

public sealed class Structure(EnumStructure key, string label)
{
    public string Label => label;

    public EnumStructure Key => key;
};


// KanriShutai：管理主体
public enum EnumKanriShutai
{
    Owner, Tasha, Jisya, Unspecified
}

/*
public class KanriShutai(string key, string label)
{
    public string Label { get; set; } = label;
    public string Key { get; set; } = key;
};
*/


// Management：建物管理形態
public enum EnumManagement
{
    Unspecified, Kashinushi, Tasha, Jisya, Unknown,
}
