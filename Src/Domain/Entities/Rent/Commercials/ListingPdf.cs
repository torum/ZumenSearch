using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.Enums.Rent.Commercials;

namespace ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

public sealed class ListingPdf : EntityBase
{
    public ListingPdf(string id, string pdfLocation, string thumbnailLocation, EntityStatus status) : base(id, status)
    {
        PdfFilename = pdfLocation;
        ThumbnailFilename = thumbnailLocation;
        IsModified = false;
    }

    public string PdfFilename
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    public string BasePath { get; set; } = string.Empty;

    public string ThumbnailFilename
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    public ListingPdfType PdfType
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = ListingPdfType.Unspecified;

    public string Description
    {
        get;
        set
        {
            value ??= string.Empty;
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    public bool IsMain
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    }

    public ListingPdf Clone()
    {
        var clone = new ListingPdf(Id, PdfFilename, ThumbnailFilename, Status)
        {
            BasePath = BasePath,
            Description = Description,
            PdfType = PdfType,
            IsMain = IsMain
        };

        clone.SetIsModified(IsModified);
        return clone;
    }
}
