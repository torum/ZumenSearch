using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models;

public abstract class PictureBase : ObservableObject
{
    public string ImageLocation { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;

    public bool IsNew { get; set; }
    public bool IsModified { get; set; }

    protected PictureBase()
    {

    }
};
