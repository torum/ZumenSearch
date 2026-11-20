using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.Enums.Rent.Residentials;

namespace ZumenSearch.Domain.Entities.Rent.Residentials;

public sealed class PropertyPdf : EntityBase
{
    public PropertyPdf(string id, string pdfLocation, string thumbnailLocation, EntityStatus status) : base(id, status)
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

    public PropertyPdfType PdfType
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = PropertyPdfType.Unspecified;

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

    public PropertyPdf Clone()
    {
        var clone = new PropertyPdf(Id, PdfFilename, ThumbnailFilename, Status)
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
