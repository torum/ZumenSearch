using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

internal abstract class PictureBase : ObservableObject
{
    public string ImageLocation
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = string.Empty;

    public string Id { get; } = string.Empty;

    public bool IsNew { get; set; } = true;

    public bool IsModified { get; set; } = false;

    protected PictureBase(string id)
    {
        Id = id;
    }
};
