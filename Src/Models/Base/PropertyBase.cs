using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Globalization;
using ZumenSearch.Models.Enums;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Transportation;

#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable IDE0079 // Remove unnecessary suppression

namespace ZumenSearch.Models.Base;

// Aggregate Root entity. Property Listing Context.

// <summary>
// Base class for all Aggregate Root entities : properties. Provides common properties and methods for managing entity state.
// </summary>
// <remarks>
// </remarks>
public abstract partial class PropertyBase : EntityAggregateBase
{
    protected PropertyBase(string id, EntityStatus status, PropertyKind kind) : base(id, status)
    {
        PropertyKind = kind;
    }

    public PropertyKind PropertyKind { get; init; }

    public Address Address
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Address();

    public Train Train1
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Train();

    public string BusStop1
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

    public int BusJyousya1
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int BusStopToho1
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public Train Train2
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Train();

    public string BusStop2
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

    public int BusJyousya2
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int BusStopToho2
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public Train Train3
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Train();

    public string BusStop3
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

    public int BusJyousya3
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int BusStopToho3
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public Train Train4
    {
        get;
        init
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new Train();

    public string BusStop4
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

    public int BusJyousya4
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int BusStopToho4
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    // TODO: Create a nested class for ThumbImage so that BasePath is always set along with ThumbnailFilename as parameters.
    #region == Thumb Image ==

    public string BasePath { get; set; } = string.Empty;

    public string ThumbnailFilename
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    // Automatically generate a thumbnail image when the property is accessed, if it hasn't been set yet.
    public ImageSource? ThumbImage 
    {
        get => field ?? CreateThumb();
        set; 
    }

    private BitmapImage? CreateThumb()
    {
        if (string.IsNullOrEmpty(ThumbnailFilename))
        {
            //Debug.WriteLine("ThumbnailImageFilePath is empty. (PropertyBase)");
            return null;
        }

        if (string.IsNullOrEmpty(BasePath))
        {
            Debug.WriteLine("BasePath is empty. (PropertyBase)");
            return null;
        }

        var thumbFilemame = System.IO.Path.Combine(BasePath, ThumbnailFilename);

        if (!Path.Exists(thumbFilemame))
        {
            Debug.WriteLine($"File ThumbnailImageFilePath does not exists. (PropertyBase) {thumbFilemame}");
            return null;
        }

        BitmapImage bitmapImage = new()
        {
            DecodePixelWidth = 280
        };
        
        Uri uri = new(thumbFilemame);
        bitmapImage.UriSource = uri;

        return bitmapImage;
    }

    #endregion

    #region == Public Methods ==

    public void SetBusStop1(string value) => BusStop1 = value;

    public void SetBusJyousya1(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BusJyousya1 = 0;
            return;
        }

        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("バス乗車時間（分）の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }

        if (result < 0)
        {
            throw new ArgumentException("バス乗車時間（分）の値が不正です。0以上の整数である必要があります。", nameof(value));
        }

        BusJyousya1 = result;
    }

    public void SetBusStopToho1(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BusStopToho1 = 0;
            return;
        }
        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("バス停から徒歩（分）の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }
        if (result < 0)
        {
            throw new ArgumentException("バス停から徒歩（分）の値が不正です。0以上の整数である必要があります。", nameof(value));
        }
        BusStopToho1 = result;
    }

    public void SetBusStop2(string value) => BusStop2 = value;

    public void SetBusJyousya2(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BusJyousya2 = 0;
            return;
        }

        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("バス乗車時間（分）の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }

        if (result < 0)
        {
            throw new ArgumentException("バス乗車時間（分）の値が不正です。0以上の整数である必要があります。", nameof(value));
        }

        BusJyousya2 = result;
    }

    public void SetBusStopToho2(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BusStopToho2 = 0;
            return;
        }
        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("バス停から徒歩（分）の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }
        if (result < 0)
        {
            throw new ArgumentException("バス停から徒歩（分）の値が不正です。0以上の整数である必要があります。", nameof(value));
        }
        BusStopToho2 = result;
    }

    public void SetBusStop3(string value) => BusStop3 = value;

    public void SetBusJyousya3(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BusJyousya3 = 0;
            return;
        }

        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("バス乗車時間（分）の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }

        if (result < 0)
        {
            throw new ArgumentException("バス乗車時間（分）の値が不正です。0以上の整数である必要があります。", nameof(value));
        }

        BusJyousya3 = result;
    }

    public void SetBusStopToho3(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BusStopToho3 = 0;
            return;
        }
        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("バス停から徒歩（分）の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }
        if (result < 0)
        {
            throw new ArgumentException("バス停から徒歩（分）の値が不正です。0以上の整数である必要があります。", nameof(value));
        }
        BusStopToho3 = result;
    }
    
    public void SetBusStop4(string value) => BusStop4 = value;

    public void SetBusJyousya4(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BusJyousya4 = 0;
            return;
        }

        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("バス乗車時間（分）の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }

        if (result < 0)
        {
            throw new ArgumentException("バス乗車時間（分）の値が不正です。0以上の整数である必要があります。", nameof(value));
        }

        BusJyousya4 = result;
    }

    public void SetBusStopToho4(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BusStopToho4 = 0;
            return;
        }
        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("バス停から徒歩（分）の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }
        if (result < 0)
        {
            throw new ArgumentException("バス停から徒歩（分）の値が不正です。0以上の整数である必要があります。", nameof(value));
        }
        BusStopToho4 = result;
    }

    #endregion

}


