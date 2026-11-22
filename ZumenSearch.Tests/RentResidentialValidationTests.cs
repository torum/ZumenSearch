using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels;
using ZumenSearch.Presentation.Services.Contracts;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class RentResidentialValidationTests
{
    [TestMethod]
    public async Task PropertyEditor_RejectsMissingNameAndInvalidFloorCount()
    {
        var id = $"validation-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        try
        {
            var property = new Property(id, EntityStatus.New);
            using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            editor.Name = "Building";
            editor.Name = " ";
            await editor.Save();
            Assert.IsTrue(editor.IsNameHasError);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.IsTrue(editor.IsDirty);
            Assert.AreEqual(EntityStatus.New, property.Status);

            editor.Name = "Building";
            editor.FloorCountAboveGround = "-1";
            await editor.Save();
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.IsTrue(editor.IsDirty);
            Assert.AreEqual(EntityStatus.New, property.Status);
            Assert.AreEqual(string.Empty, property.Name);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task ListingEditor_RejectsMissingNameAndInvalidRent()
    {
        var propertyId = $"validation-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        try
        {
            var listing = new Listing("listing", EntityStatus.New, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
            using var editor = new ListingViewModel(listing, false, new NavigationStub(), null!, null!, null!, null!);
            editor.Name = "101";
            editor.Name = " ";
            await editor.Save();
            Assert.IsTrue(editor.IsNameHasError);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.IsTrue(editor.IsDirty);
            Assert.AreEqual(EntityStatus.New, listing.Status);

            editor.Name = "101";
            editor.Chinryou = "-1";
            await editor.Save();
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.IsTrue(editor.IsDirty);
            Assert.AreEqual(EntityStatus.New, listing.Status);
            Assert.AreEqual(string.Empty, listing.Name);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
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
