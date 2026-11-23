using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Domain.Entities.Rent.Commercials;
using ZumenSearch.Domain.Enums.Rent.Commercials;
using ZumenSearch.Presentation.Features.Rent.Commercials.Models;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels;

public sealed partial class PropertyPdfViewModel : ObservableObject
{
    private readonly PropertyPdf _model;
    private PropertyPdfTypeLabel _pdfType;

    public PropertyPdfViewModel(PropertyPdf model, PropertyViewModel parentViewModel)
    {
        _model = model;
        ParentViewModel = parentViewModel;
        _pdfType = CommercialPdfTypes.First(item => item.Key == model.PdfType);
    }

    public PropertyPdf Model => _model;
    public PropertyViewModel ParentViewModel { get; }
    public string Id => _model.Id;
    public string PdfFilename => _model.PdfFilename;
    public string ThumbnailFilename => _model.ThumbnailFilename;
    public bool IsModified => _model.IsModified;

    public string? ThumbImagePath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_model.BasePath) || string.IsNullOrWhiteSpace(ThumbnailFilename)) return null;
            var path = Path.Combine(_model.BasePath, ThumbnailFilename);
            return Path.Exists(path) ? path : null;
        }
    }

    public ObservableCollection<PropertyPdfTypeLabel> CommercialPdfTypes { get; } =
    [
        new(PropertyPdfType.Unspecified),
        new(PropertyPdfType.Listing),
        new(PropertyPdfType.Architectural),
        new(PropertyPdfType.Registry),
        new(PropertyPdfType.Map),
        new(PropertyPdfType.Other)
    ];

    public PropertyPdfTypeLabel PdfType
    {
        get => _pdfType;
        set
        {
            if (value is null || _pdfType == value) return;
            _model.PdfType = value.Key;
            SetProperty(ref _pdfType, value);
        }
    }

    public string Description
    {
        get => _model.Description;
        set => SetProperty(_model.Description, value ?? string.Empty, _model, static (pdf, description) => pdf.Description = description);
    }

    public bool IsMain
    {
        get => _model.IsMain;
        set => SetProperty(_model.IsMain, value, _model, static (pdf, isMain) => pdf.IsMain = isMain);
    }
}
