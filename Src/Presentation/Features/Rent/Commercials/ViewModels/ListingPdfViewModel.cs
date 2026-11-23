using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;
using ZumenSearch.Domain.Enums.Rent.Commercials;
using ZumenSearch.Presentation.Features.Rent.Commercials.Models;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels;

public sealed partial class ListingPdfViewModel : ObservableObject
{
    private readonly ListingPdf _model;
    private ListingPdfTypeLabel _pdfType;

    public ListingPdfViewModel(ListingPdf model, ListingViewModel parentViewModel)
    {
        _model = model;
        ParentViewModel = parentViewModel;
        _pdfType = ListingPdfTypes.First(item => item.Key == model.PdfType);
    }

    public ListingPdf Model => _model;
    public ListingViewModel ParentViewModel { get; }
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

    public ObservableCollection<ListingPdfTypeLabel> ListingPdfTypes { get; } =
    [
        new(ListingPdfType.Unspecified),
        new(ListingPdfType.Listing),
        new(ListingPdfType.Architectural),
        new(ListingPdfType.Registry),
        new(ListingPdfType.Map),
        new(ListingPdfType.Other)
    ];

    public ListingPdfTypeLabel PdfType
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
