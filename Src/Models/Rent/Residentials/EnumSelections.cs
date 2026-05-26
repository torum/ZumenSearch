using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ZumenSearch.Models.Rent.Residentials;

#pragma warning disable IDE0290 // Use primary constructor

internal sealed class Kind(Models.Rent.Residentials.EntryResidential.EnumKinds key, string label)
{
    public string Label => label;

    public Models.Rent.Residentials.EntryResidential.EnumKinds Key => key;
};

internal sealed class Structure(Models.Rent.Residentials.EntryResidential.EnumStructure key, string label)
{
    public string Label => label;

    public Models.Rent.Residentials.EntryResidential.EnumStructure Key => key;
};

internal sealed class ElectricKind
{
    public string Label { get; set; }

    public Models.Rent.Residentials.EntryResidential.EnumElectricKind Key { get; set; }

    public ElectricKind(Models.Rent.Residentials.EntryResidential.EnumElectricKind key, string label)
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


internal class KanriShutai(Models.Rent.Residentials.EntryResidential.EnumKanriShutai key, string label)
{
    public string Label => label;

    public Models.Rent.Residentials.EntryResidential.EnumKanriShutai Key => key;
};
