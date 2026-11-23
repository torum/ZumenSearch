using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.Enums.Rent.Commercials;

namespace ZumenSearch.Domain.Entities.Rent.Commercials;

public sealed class PropertyPicture : EntityBase
{
    public PropertyPicture(string id, string imageLocation, EntityStatus status) : base(id, status)
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

    public PropertyPictureType PictureType
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            IsModified = true;
        }
    } = PropertyPictureType.Unspecified;

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

    public PropertyPicture Clone()
    {
        var clone = new PropertyPicture(Id, ImageFilename, Status)
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
