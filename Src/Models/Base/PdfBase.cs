using CommunityToolkit.Mvvm.ComponentModel;

namespace ZumenSearch.Models.Base;

internal abstract class PdfBase : ObservableObject
{
    public string PdfLocation
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

    public string ThumbnailLocation
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

    protected PdfBase(string id)
    {
        Id = id;
    }
};
