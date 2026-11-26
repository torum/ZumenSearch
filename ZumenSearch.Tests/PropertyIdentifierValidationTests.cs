using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class PropertyIdentifierValidationTests
{
    public static (string Id, string Code, bool Valid, bool OnBasicPage)[] IdentifierCases =>
    [
        ("1", "AB12", false, false),
        ("123456789012", "AB12", false, true),
        ("１２３", "AB12", false, false),
        ("1234567890123", "A", false, false),
        ("1234567890123", "AB1", false, true),
        ("1234567890123", "ａ１", false, false),
        ("", "", true, false),
        ("1234567890123", "", true, false),
        ("", "AB12", true, false),
        ("1234567890123", "AB12", true, false),
        ("１２３４５６７８９０１２３", "ａｂ１２", true, false)
    ];

    [TestMethod]
    [DynamicData(nameof(IdentifierCases))]
    public async Task ResidentialIdentifiers_ValidateBeforeApplyingValuesAndNavigateToBasicPage(string id, string code, bool valid, bool onBasicPage)
    {
        var propertyId = $"identifier-validation-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Residential.Property(propertyId, EntityStatus.Saved);
        property.SetName("Original building");
        property.SetFudousanId("9999999999999");
        property.SetFudousanIdAdditionalCode("ZZ99");
        var navigation = new NavigationStub { OnBasicPage = onBasicPage };
        var repository = new RepositoryStub();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel(property, false, navigation, null!, null!, null!, new RentResidentialPropertyService(repository), null!, null!, null!);
            editor.Name = "Edited building";
            editor.FudousanId = id;
            editor.FudousanIdAdditionalCode = code;

            await editor.Save();

            if (!valid)
            {
                Assert.IsTrue(editor.IsDirty);
                Assert.IsFalse(editor.IsBusy);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
                Assert.AreEqual("入力項目に誤りがあります", editor.InfoBarErrorTitle);
                Assert.AreEqual(InfoBarSeverity.Warning, editor.InfoBarSeverity);
                Assert.AreEqual(editor.FudousanId.Length > 0 && editor.FudousanId.Length < 13 ? "不動産番号は半角数字13桁で入力してください。" : "特定コードは半角英数字4桁（英字は大文字）で入力してください。", editor.InfoBarErrorMessage);
                Assert.AreEqual("Original building", property.Name);
                Assert.AreEqual("9999999999999", property.FudousanId);
                Assert.AreEqual("ZZ99", property.FudousanIdAdditionalCode);
                Assert.IsFalse(property.IsModified);
                Assert.AreEqual(0, repository.SaveCalls);
                Assert.AreEqual(onBasicPage ? 0 : 1, navigation.NavigateCalls);
                if (!onBasicPage)
                {
                    Assert.AreEqual(NavigationKeys.RentResidentialPropertyBasic, navigation.LastPage);
                    Assert.AreSame(editor, navigation.LastParameter);
                }

                editor.FudousanId = "１２３４５６７８９０１２３";
                editor.FudousanIdAdditionalCode = "ａｂ１２";
                await editor.Save();
            }

            Assert.AreEqual(1, repository.SaveCalls);
            Assert.IsFalse(editor.IsDirty);
            Assert.IsFalse(editor.IsInfoBarErrorOpen);
            Assert.AreEqual("Edited building", property.Name);
            Assert.AreEqual(!valid || id.Length > 0 ? "1234567890123" : string.Empty, property.FudousanId);
            Assert.AreEqual(!valid || code.Length > 0 ? "AB12" : string.Empty, property.FudousanIdAdditionalCode);
            if (valid) Assert.AreEqual(0, navigation.NavigateCalls);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DynamicData(nameof(IdentifierCases))]
    public async Task CommercialIdentifiers_ValidateBeforeApplyingValuesAndNavigateToBasicPage(string id, string code, bool valid, bool onBasicPage)
    {
        var propertyId = $"identifier-validation-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Commercial.Property(propertyId, EntityStatus.Saved);
        property.SetName("Original building");
        property.SetFudousanId("9999999999999");
        property.SetFudousanIdAdditionalCode("ZZ99");
        var navigation = new NavigationStub { OnBasicPage = onBasicPage };
        var repository = new RepositoryStub();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel(property, false, navigation, null!, null!, null!, new RentCommercialPropertyService(repository), null!, null!, null!);
            editor.Name = "Edited building";
            editor.FudousanId = id;
            editor.FudousanIdAdditionalCode = code;

            await editor.Save();

            if (!valid)
            {
                Assert.IsTrue(editor.IsDirty);
                Assert.IsFalse(editor.IsBusy);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
                Assert.AreEqual("入力項目に誤りがあります", editor.InfoBarErrorTitle);
                Assert.AreEqual(InfoBarSeverity.Warning, editor.InfoBarSeverity);
                Assert.AreEqual(editor.FudousanId.Length > 0 && editor.FudousanId.Length < 13 ? "不動産番号は半角数字13桁で入力してください。" : "特定コードは半角英数字4桁（英字は大文字）で入力してください。", editor.InfoBarErrorMessage);
                Assert.AreEqual("Original building", property.Name);
                Assert.AreEqual("9999999999999", property.FudousanId);
                Assert.AreEqual("ZZ99", property.FudousanIdAdditionalCode);
                Assert.IsFalse(property.IsModified);
                Assert.AreEqual(0, repository.SaveCalls);
                Assert.AreEqual(onBasicPage ? 0 : 1, navigation.NavigateCalls);
                if (!onBasicPage)
                {
                    Assert.AreEqual(NavigationKeys.RentCommercialPropertyBasic, navigation.LastPage);
                    Assert.AreSame(editor, navigation.LastParameter);
                }

                editor.FudousanId = "１２３４５６７８９０１２３";
                editor.FudousanIdAdditionalCode = "ａｂ１２";
                await editor.Save();
            }

            Assert.AreEqual(1, repository.SaveCalls);
            Assert.IsFalse(editor.IsDirty);
            Assert.IsFalse(editor.IsInfoBarErrorOpen);
            Assert.AreEqual("Edited building", property.Name);
            Assert.AreEqual(!valid || id.Length > 0 ? "1234567890123" : string.Empty, property.FudousanId);
            Assert.AreEqual(!valid || code.Length > 0 ? "AB12" : string.Empty, property.FudousanIdAdditionalCode);
            if (valid) Assert.AreEqual(0, navigation.NavigateCalls);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public bool OnBasicPage { get; init; }
        public int NavigateCalls { get; private set; }
        public object? LastPage { get; private set; }
        public object? LastParameter { get; private set; }

        public bool NavigateTo(object? selectedPage, object? param)
        {
            NavigateCalls++;
            LastPage = selectedPage;
            LastParameter = param;
            return true;
        }

        public bool IsCurrentPageSameAs(object? selectedPage) => OnBasicPage;
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) => throw new NotSupportedException();
        public Frame? GetFrame() => null;
    }

    private sealed class RepositoryStub : IRentResidentialPropertyRepository, IRentCommercialPropertyRepository
    {
        public int SaveCalls { get; private set; }

        public ResultWrapper Upsert(Residential.Property residentialProperty)
        {
            SaveCalls++;
            return new ResultWrapper();
        }

        public ResultWrapper Upsert(Commercial.Property commercialProperty)
        {
            SaveCalls++;
            return new ResultWrapper();
        }

        RentResidentialPropertyResultWrapper IRentResidentialPropertyRepository.SelectById(string propertyId, bool duplicate) => throw new NotSupportedException();
        RentCommercialPropertyResultWrapper IRentCommercialPropertyRepository.SelectById(string propertyId, bool duplicate) => throw new NotSupportedException();
        public ResultWrapper Delete(string propertyId) => throw new NotSupportedException();
    }
}
