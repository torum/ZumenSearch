using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials.Room;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// 部屋（編集用）
public sealed partial class Listing : ListingBase
{
    // Holding a reference to the parent Building (Property) object to allow communication between the Room Listing and its parent Building.
    //public Models.Rent.Residentials.Bldg.Property? Building { get; private set; }

    public string PropertyId { get; private set; }

    public string PropertyName
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {

            }
        }
    }

    // ステータス（保存済みか新規か）
    public EnumPropertyStatus PropertyStatus { get; set; } = EnumPropertyStatus.New;
    public EnumListingStatus ListingStatus { get; set; } = EnumListingStatus.New;

    // 物件写真（部屋）リスト
    public ObservableCollection<Picture> Pictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;//?
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき物件写真（部屋）のIDリスト
    public ObservableCollection<Picture> PicturesToBeDeleted = [];

    // 賃料（円）
    public decimal Chinryou
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            if (value > -1)
            {
                field = value;
                OnPropertyChanged();
                IsModified = true;
            }
            else
            {
                // TODO: show error
                //field = string.Empty;
                //IsModified = true;

                throw new ArgumentOutOfRangeException("Chinryou", "Must be at least 0.");
            }
        }
    }


    [ObservableProperty]
    public partial decimal KyouekiFee { get; set; }

    [ObservableProperty]
    public partial decimal Shikikin { get; set; }

    [ObservableProperty]
    public partial string ShikikinUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial decimal Reikin { get; set; }

    [ObservableProperty]
    public partial string ReikinUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial string ContractType { get; set; } = "未指定";

    [ObservableProperty]
    public partial int ContractPeriodYears { get; set; }

    [ObservableProperty]
    public partial decimal RenewalFee { get; set; }

    [ObservableProperty]
    public partial string RenewalFeeUnit { get; set; } = "ヵ月";

    [ObservableProperty]
    public partial decimal RecontractFee { get; set; }

    [ObservableProperty]
    public partial string RecontractFeeUnit { get; set; } = "円";

    [ObservableProperty]
    public partial string GuarantorCompany { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HousingInsurance { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OtherFees { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool PetFriendly { get; set; }

    [ObservableProperty]
    public partial bool TwoPersonOccupancy { get; set; }

    [ObservableProperty]
    public partial bool NoGuarantorNeeded { get; set; }

    [ObservableProperty]
    public partial bool OfficeUseAllowed { get; set; }

    [ObservableProperty]
    public partial string OtherConditions { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Remarks { get; set; } = string.Empty;

    public Listing(string id, string propertyId, string propertyName) : base(id)
    {
        PropertyId = propertyId;
        PropertyName = propertyName;
    }
}
