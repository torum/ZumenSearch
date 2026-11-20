using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.Enums.Rent.Residentials;

namespace ZumenSearch.Domain.Entities.Rent.Residentials;

public sealed class ListingPicture : EntityBase
{
    public ListingPicture(string id, string imageLocation, EntityStatus status) : base(id, status)
    {
        ImageFilename = imageLocation;
        IsModified = false;
    }

    public string ImageFilename
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

    public ListingPictureType PictureType
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = ListingPictureType.Unspecified;

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

    public ListingPicture Clone()
    {
        var clone = new ListingPicture(Id, ImageFilename, Status)
        {
            BasePath = BasePath,
            Description = Description,
            PictureType = PictureType,
            IsMain = IsMain
        };

        clone.SetIsModified(IsModified);
        return clone;
    }
}
