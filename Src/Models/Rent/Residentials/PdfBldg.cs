using System.Collections.ObjectModel;
using System.Diagnostics;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials;

public partial class PdfBldg : PdfBase
{
    public ViewModels.Rent.Residentials.MainViewModel? ParentViewModel { get; set; }

    // Do not use SetProperty. PropertyChanged is being subscribed.
    public string Description
    {
        get;
        set
        {
            if (field == value) return;

            if (value is not null)
            {
                // Set this before raize PropertyChanged.
                IsModified = true;

                // Raize PropertyChanged event here.
                field = value;
            }
            else
            {
                field = string.Empty;
            }

            OnPropertyChanged();
        }
    } = string.Empty;

    // Do not use SetProperty.
    public bool IsMain
    {
        get;
        set
        {
            if (field == value) return;

            // Set this before raize PropertyChanged.
            IsModified = true;

            // Raize PropertyChanged event here.
            field = value;

            OnPropertyChanged();
        }
    }

    public PdfBldg(string id, string pdfLocation, string thumbnailLocation) : base(id)
    {
        PdfLocation = pdfLocation;
        ThumbnailLocation = thumbnailLocation;

        IsModified = false;
    }
};
