using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Rent.Residentials.Listing;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// 部屋（編集用）
public sealed partial class Listing : ListingBase
{

    // 部屋が属する建物が区分所有化どうかをここでも保持（部屋を直接開いた際に必要）
    public bool IsPropertyUnitOwnership { get; set; }

    // 物件（建物の名前を保持 - タイトル等に表示）
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


    // 部屋写真リスト
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

    // DBへの更新時にDBから削除されるべき部屋写真のIDリスト
    public ObservableCollection<Picture> PicturesToBeDeleted = [];

    // 部屋図面リスト
    public ObservableCollection<Pdf> Pdfs
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

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public ObservableCollection<Pdf> PdfsToBeDeleted = [];

    // 貸主のリスト
    public ObservableCollection<Models.Rent.Lessors.Person> Lessors
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき貸主のIDリスト
    public ObservableCollection<Models.Rent.Lessors.Person> LessorsToBeDeleted = [];


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

    public Listing(string id, EnumEntryStatus status, string propertyId, EnumEntryStatus propertyStatus, bool isPropertyUnitOwnership, string propertyName) : base(id, status, propertyId, propertyStatus, EnumPropertyKind.RentResidential)
    {
        //PropertyId = propertyId;
        IsPropertyUnitOwnership = isPropertyUnitOwnership;
        PropertyName = propertyName;
        //PropertyStatus = propertyStatus;
    }
}
