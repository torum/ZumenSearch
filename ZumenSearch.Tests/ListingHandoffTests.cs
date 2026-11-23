using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Factories.AbstractFactory;
using ZumenSearch.Presentation.Services.Contracts;
using CommercialListing = ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing;
using CommercialListingViewModel = ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel;
using ResidentialListing = ZumenSearch.Domain.Entities.Rent.Residentials.Listing;
using ResidentialListingViewModel = ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class ListingHandoffTests
{
    [TestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public void Factories_KeepBooleanArgumentsInConstructorOrder(bool isDuplicate, bool isHandedOff)
    {
        using var services = new ServiceCollection().AddSingleton<FactoryDependency>().BuildServiceProvider();
        var dependency = services.GetRequiredService<FactoryDependency>();
        var shellFactory = new AbstractFactory<string, bool, bool, ShellArguments>(services);
        var shell = shellFactory.Create("listing", isDuplicate, isHandedOff);
        Assert.AreEqual("listing", shell.Listing);
        Assert.AreEqual(isDuplicate, shell.IsDuplicate);
        Assert.AreEqual(isHandedOff, shell.IsHandedOff);
        Assert.AreSame(dependency, shell.Dependency);

        var viewModelFactory = new AbstractFactory<string, bool, bool, int, long, ViewModelArguments>(services);
        var viewModel = viewModelFactory.Create("listing", isDuplicate, isHandedOff, 1, 2L);
        Assert.AreEqual("listing", viewModel.Listing);
        Assert.AreEqual(isDuplicate, viewModel.IsDuplicate);
        Assert.AreEqual(isHandedOff, viewModel.IsHandedOff);
        Assert.AreEqual(1, viewModel.Navigation);
        Assert.AreEqual(2L, viewModel.Dialog);
        Assert.AreSame(dependency, viewModel.Dependency);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ResidentialDiscard_PreservesOnlyHandedOffDirectory(bool isHandedOff)
    {
        var propertyId = $"handoff-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        var marker = Path.Combine(directory, "marker.txt");
        try
        {
            var listing = new ResidentialListing("listing", EntityStatus.New, propertyId, EntityStatus.New, false, "Building", string.Empty);
            using var editor = new ResidentialListingViewModel(listing, false, new NavigationStub(), null!, null!, null!, null!, isHandedOff);
            File.WriteAllText(marker, "parent-owned");
            editor.DiscardChanges();
            Assert.AreEqual(isHandedOff, File.Exists(marker));
            Assert.AreEqual(isHandedOff, Directory.Exists(directory));
        }
        finally
        {
            if (File.Exists(marker)) File.Delete(marker);
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void CommercialDiscard_PreservesOnlyHandedOffDirectory(bool isHandedOff)
    {
        var propertyId = $"handoff-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        var marker = Path.Combine(directory, "marker.txt");
        try
        {
            var listing = new CommercialListing("listing", EntityStatus.New, propertyId, EntityStatus.New, false, "Building", string.Empty);
            using var editor = new CommercialListingViewModel(listing, false, new NavigationStub(), null!, null!, null!, null!, isHandedOff);
            File.WriteAllText(marker, "parent-owned");
            editor.DiscardChanges();
            Assert.AreEqual(isHandedOff, File.Exists(marker));
            Assert.AreEqual(isHandedOff, Directory.Exists(directory));
        }
        finally
        {
            if (File.Exists(marker)) File.Delete(marker);
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    public sealed class FactoryDependency;

    public sealed class ShellArguments(string listing, bool isDuplicate, bool isHandedOff, FactoryDependency dependency)
    {
        public string Listing { get; } = listing;
        public bool IsDuplicate { get; } = isDuplicate;
        public bool IsHandedOff { get; } = isHandedOff;
        public FactoryDependency Dependency { get; } = dependency;
    }

    public sealed class ViewModelArguments(string listing, bool isDuplicate, int navigation, long dialog, FactoryDependency dependency, bool isHandedOff = false)
    {
        public string Listing { get; } = listing;
        public bool IsDuplicate { get; } = isDuplicate;
        public bool IsHandedOff { get; } = isHandedOff;
        public int Navigation { get; } = navigation;
        public long Dialog { get; } = dialog;
        public FactoryDependency Dependency { get; } = dependency;
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }
}
