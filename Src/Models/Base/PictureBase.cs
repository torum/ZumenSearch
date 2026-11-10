#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.Base;

// <summary>
// Base class for all Picture entities. Aggregate Root entities are property and listing.
// </summary>
// <remarks>
// 
// </remarks>
public abstract class PictureBase : EntityBase
{
    protected PictureBase(string id, EntityStatus staus) : base(id, staus)
    {
        //
    }

    public string ImageFilename
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    public string BasePath { get; set; } = string.Empty;

    public string? ThumbImagePath => GetThumbImagePath();

    private string? GetThumbImagePath()
    {
        if (string.IsNullOrWhiteSpace(ImageFilename) || string.IsNullOrWhiteSpace(BasePath))
        {
            return null;
        }

        var imagePath = Path.Combine(BasePath, ImageFilename);
        return Path.Exists(imagePath) ? imagePath : null;
    }

};
