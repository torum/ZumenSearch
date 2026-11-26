using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Services.Contracts;
using R = ZumenSearch.Presentation.Features.Rent.Residentials.Models;
using C = ZumenSearch.Presentation.Features.Rent.Commercials.Models;
using RD = ZumenSearch.Domain.Entities.Rent.Residentials;
using CD = ZumenSearch.Domain.Entities.Rent.Commercials;
using RV = ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel;
using CV = ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class PropertySelectionTests
{
    [TestMethod]
    public void ResidentialSelections_ValidateAndCanonicalize()
    {
        Residential(e => Check(e, nameof(e.SelectedPropertyKind), () => e.SelectedPropertyKind, v => e.SelectedPropertyKind = v!, e.PropertyKinds, x => x.Key, k => new R.PropertyKindTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.PropertyKindType)999));
        Residential(e => Check(e, nameof(e.SelectedStructureType), () => e.SelectedStructureType, v => e.SelectedStructureType = v!, e.StructureTypes, x => x.Key, k => new R.PropertyStructureTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.StructureType)999));
        Residential(e => Check(e, nameof(e.SelectedElectricKind), () => e.SelectedElectricKind, v => e.SelectedElectricKind = v!, e.ElectricKinds, x => x.Key, k => new R.PropertyElectricKind(k, "external"), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.Property.EnumElectricType)999));
        Residential(e => Check(e, nameof(e.SelectedGasType), () => e.SelectedGasType, v => e.SelectedGasType = v!, e.GasTypes, x => x.Key, k => new R.FacilityGasTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityGasType)999));
        Residential(e => Check(e, nameof(e.SelectedTapWaterType), () => e.SelectedTapWaterType, v => e.SelectedTapWaterType = v!, e.TapWaterTypes, x => x.Key, k => new R.FacilityTapWaterTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityTapWaterType)999));
        Residential(e => Check(e, nameof(e.SelectedSewerageType), () => e.SelectedSewerageType, v => e.SelectedSewerageType = v!, e.SewerageTypes, x => x.Key, k => new R.FacilitySewerageTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilitySewerageType)999));
        Residential(e => Check(e, nameof(e.SelectedParkingContractType), () => e.SelectedParkingContractType, v => e.SelectedParkingContractType = v!, e.ParkingContractTypes, x => x.Key, k => new R.FacilityParkingContractTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityParkingContractType)999));
        Residential(e => Check(e, nameof(e.SelectedParkingFeeTaxType), () => e.SelectedParkingFeeTaxType, v => e.SelectedParkingFeeTaxType = v!, e.TaxTypes, x => x.Key, k => new R.FacilityTaxTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityTaxType)999));
        Residential(e => Check(e, nameof(e.SelectedParkingAvailability), () => e.SelectedParkingAvailability, v => e.SelectedParkingAvailability = v!, e.AvailabilityTypes, x => x.Key, k => new R.FacilityAvailabilityTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityAvailabilityType)999));
        Residential(e => Check(e, nameof(e.SelectedBicycleParkingFeeTaxType), () => e.SelectedBicycleParkingFeeTaxType, v => e.SelectedBicycleParkingFeeTaxType = v!, e.TaxTypes, x => x.Key, k => new R.FacilityTaxTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityTaxType)999));
        Residential(e => Check(e, nameof(e.SelectedBicycleParkingAvailability), () => e.SelectedBicycleParkingAvailability, v => e.SelectedBicycleParkingAvailability = v!, e.AvailabilityTypes, x => x.Key, k => new R.FacilityAvailabilityTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityAvailabilityType)999));
        Residential(e => Check(e, nameof(e.SelectedMotorcycleParkingFeeTaxType), () => e.SelectedMotorcycleParkingFeeTaxType, v => e.SelectedMotorcycleParkingFeeTaxType = v!, e.TaxTypes, x => x.Key, k => new R.FacilityTaxTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityTaxType)999));
        Residential(e => Check(e, nameof(e.SelectedMotorcycleParkingAvailability), () => e.SelectedMotorcycleParkingAvailability, v => e.SelectedMotorcycleParkingAvailability = v!, e.AvailabilityTypes, x => x.Key, k => new R.FacilityAvailabilityTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.FacilityAvailabilityType)999));
        Residential(e => Check(e, nameof(e.SelectedKanriShutai), () => e.SelectedKanriShutai!, v => e.SelectedKanriShutai = v, e.KanriShutais, x => x.Key, k => new R.PropertyKanriShutai(k, "external"), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (RD.Property.EnumKanriShutai)999));
    }

    [TestMethod]
    public void CommercialSelections_ValidateAndCanonicalize()
    {
        Commercial(e => Check(e, nameof(e.SelectedPropertyKind), () => e.SelectedPropertyKind, v => e.SelectedPropertyKind = v!, e.PropertyKinds, x => x.Key, k => new C.PropertyTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (CD.PropertyKindType)999));
        Commercial(e => Check(e, nameof(e.SelectedStructureType), () => e.SelectedStructureType, v => e.SelectedStructureType = v!, e.StructureTypes, x => x.Key, k => new C.PropertyStructureTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (CD.StructureType)999));
        Commercial(e => Check(e, nameof(e.SelectedGasType), () => e.SelectedGasType, v => e.SelectedGasType = v!, e.GasTypeOptions, x => x.Key, k => new C.GasSupplyTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (CD.GasSupplyType)999));
        Commercial(e => Check(e, nameof(e.SelectedWaterType), () => e.SelectedWaterType, v => e.SelectedWaterType = v!, e.WaterTypeOptions, x => x.Key, k => new C.WaterSupplyTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (CD.WaterSupplyType)999));
        Commercial(e => Check(e, nameof(e.SelectedSewerageType), () => e.SelectedSewerageType, v => e.SelectedSewerageType = v!, e.SewerageTypeOptions, x => x.Key, k => new C.SewerageTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (CD.SewerageType)999));
        Commercial(e => Check(e, nameof(e.SelectedParkingVacancy), () => e.SelectedParkingVacancy, v => e.SelectedParkingVacancy = v!, e.ParkingVacancyOptions, x => x.Key, k => new C.ParkingVacancyTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (CD.ParkingVacancyType)999));
        Commercial(e => Check(e, nameof(e.SelectedParkingTax), () => e.SelectedParkingTax, v => e.SelectedParkingTax = v!, e.ParkingTaxOptions, x => x.Key, k => new C.ParkingTaxTypeLabel(k), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (CD.ParkingTaxType)999));
        Commercial(e => Check(e, nameof(e.SelectedKanriShutai), () => e.SelectedKanriShutai!, v => e.SelectedKanriShutai = v, e.KanriShutais, x => x.Key, k => new C.PropertyKanriShutai(k, "external"), () => e.IsDirty, () => e.IsInfoBarErrorOpen, () => e.InfoBarErrorMessage, (CD.Property.EnumKanriShutai)999));
    }

    [TestMethod]
    public void UninitializedBasicSelections_UseCanonicalDefaults()
    {
        var residential = (RV)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(RV));
        residential.PropertyKinds = R.PropertyKindTypeLabel.GetAll();
        residential.StructureTypes = R.PropertyStructureTypeLabel.GetAll();
        var residentialKind = residential.PropertyKinds.First(item => item.Key == RD.PropertyKindType.Unspecified);
        var residentialStructure = residential.StructureTypes.First(item => item.Key == RD.StructureType.Unspecified);
        Assert.AreSame(residentialKind, residential.SelectedPropertyKind);
        Assert.AreSame(residentialStructure, residential.SelectedStructureType);
        var residentialNotifications = new List<string?>();
        residential.PropertyChanged += (_, args) => residentialNotifications.Add(args.PropertyName);
        residential.SelectedPropertyKind = residentialKind;
        residential.SelectedStructureType = residentialStructure;
        Assert.AreEqual(0, residentialNotifications.Count);
        Assert.IsFalse(residential.IsDirty);

        var commercial = (CV)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(CV));
        commercial.PropertyKinds = C.PropertyTypeLabel.GetAll();
        commercial.StructureTypes = C.PropertyStructureTypeLabel.GetAll();
        var commercialKind = commercial.PropertyKinds.First(item => item.Key == CD.PropertyKindType.Unspecified);
        var commercialStructure = commercial.StructureTypes.First(item => item.Key == CD.StructureType.Unspecified);
        Assert.AreSame(commercialKind, commercial.SelectedPropertyKind);
        Assert.AreSame(commercialStructure, commercial.SelectedStructureType);
        var commercialNotifications = new List<string?>();
        commercial.PropertyChanged += (_, args) => commercialNotifications.Add(args.PropertyName);
        commercial.SelectedPropertyKind = commercialKind;
        commercial.SelectedStructureType = commercialStructure;
        Assert.AreEqual(0, commercialNotifications.Count);
        Assert.IsFalse(commercial.IsDirty);
    }

    [TestMethod]
    public void ReplacedCanonicalOptions_NotifyWithoutDirtying()
    {
        Residential(e =>
        {
            var kind = new R.PropertyKindTypeLabel(e.SelectedPropertyKind.Key);
            e.PropertyKinds = e.PropertyKinds.Select(item => item.Key == kind.Key ? kind : item).ToArray();
            var electric = new R.PropertyElectricKind(e.SelectedElectricKind.Key, "replacement");
            e.ElectricKinds[e.ElectricKinds.IndexOf(e.SelectedElectricKind)] = electric;
            var management = new R.PropertyKanriShutai(e.SelectedKanriShutai!.Key, "replacement");
            e.KanriShutais[e.KanriShutais.IndexOf(e.SelectedKanriShutai)] = management;
            var notifications = new List<string?>();
            e.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);
            e.SelectedPropertyKind = kind;
            e.SelectedElectricKind = electric;
            e.SelectedKanriShutai = management;
            Assert.AreSame(kind, e.SelectedPropertyKind);
            Assert.AreSame(electric, e.SelectedElectricKind);
            Assert.AreSame(management, e.SelectedKanriShutai);
            CollectionAssert.AreEquivalent(new[] { nameof(e.SelectedPropertyKind), nameof(e.SelectedElectricKind), nameof(e.SelectedKanriShutai) }, notifications);
            Assert.IsFalse(e.IsDirty);
            Assert.IsTrue(e.IsKanriUnspecified);
        });
        Commercial(e =>
        {
            var kind = new C.PropertyTypeLabel(e.SelectedPropertyKind.Key);
            e.PropertyKinds = e.PropertyKinds.Select(item => item.Key == kind.Key ? kind : item).ToArray();
            var gas = new C.GasSupplyTypeLabel(e.SelectedGasType.Key);
            e.GasTypeOptions[e.GasTypeOptions.IndexOf(e.SelectedGasType)] = gas;
            var management = new C.PropertyKanriShutai(e.SelectedKanriShutai!.Key, "replacement");
            e.KanriShutais[e.KanriShutais.IndexOf(e.SelectedKanriShutai)] = management;
            var notifications = new List<string?>();
            e.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);
            e.SelectedPropertyKind = kind;
            e.SelectedGasType = gas;
            e.SelectedKanriShutai = management;
            Assert.AreSame(kind, e.SelectedPropertyKind);
            Assert.AreSame(gas, e.SelectedGasType);
            Assert.AreSame(management, e.SelectedKanriShutai);
            CollectionAssert.AreEquivalent(new[] { nameof(e.SelectedPropertyKind), nameof(e.SelectedGasType), nameof(e.SelectedKanriShutai) }, notifications);
            Assert.IsFalse(e.IsDirty);
            Assert.IsTrue(e.IsKanriUnspecified);
        });
    }

    private static void Check<T, TKey>(ObservableRecipient editor, string name, Func<T> get, Action<T?> set, IEnumerable<T> options, Func<T, TKey> key, Func<TKey, T> create, Func<bool> dirty, Func<bool> errorOpen, Func<string> errorMessage, TKey invalid) where T : class where TKey : struct, Enum
    {
        var initial = get();
        Assert.IsNotNull(initial, name);
        Assert.IsFalse(dirty(), name);
        var notifications = 0;
        editor.PropertyChanged += (_, e) => { if (e.PropertyName == name) notifications++; };
        set(null);
        Assert.AreSame(initial, get(), name);
        Assert.IsFalse(dirty(), name);
        set(create(key(initial)));
        Assert.AreSame(initial, get(), name);
        Assert.AreEqual(0, notifications, name);
        Assert.IsFalse(dirty(), name);
        set(create(invalid));
        Assert.IsTrue(errorOpen(), name);
        StringAssert.Contains(errorMessage(), name);
        StringAssert.Contains(errorMessage(), invalid.ToString());
        Assert.AreSame(initial, get(), name);
        Assert.AreEqual(0, notifications, name);
        Assert.IsFalse(dirty(), name);
        var next = options.First(item => !EqualityComparer<TKey>.Default.Equals(key(item), key(initial)));
        set(create(key(next)));
        Assert.AreSame(next, get(), name);
        Assert.AreEqual(1, notifications, name);
        Assert.IsTrue(dirty(), name);
    }

    private static void Residential(Action<RV> action)
    {
        var id = $"selection-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        try
        {
            using var editor = new RV(new RD.Property(id, EntityStatus.New), false, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            action(editor);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    private static void Commercial(Action<CV> action)
    {
        var id = $"selection-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        try
        {
            using var editor = new CV(new CD.Property(id, EntityStatus.New), false, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            action(editor);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }
}
