using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Models.Rent.Residentials;

// 賃貸住居用の検索結果表示用物件クラス（建物）
public partial class EntryResidentialSearchResult : EntryBase
{
    public EntryResidentialSearchResult(string id) : base(id)
    {
        //
    }
}

// 賃貸住居用の物件クラス（建物）
public partial class EntryResidentialFull : EntryBase
{
    public EntryResidentialFull()
    {
        //
    }

    public EntryResidentialFull(string id) : base(id)
    {
        //
    }


    private ObservableCollection<PictureBuilding> _buildingPictures = [];
    public ObservableCollection<PictureBuilding> BuildingPictures
    {
        get => _buildingPictures;
        set
        {
            if (SetProperty(ref _buildingPictures, value))
            {
                IsDirty = true;
            }
        }
    }

    // DBへの更新時にDBから削除されるべき物件写真のIDリスト
    public ObservableCollection<PictureBuilding> BuildingPicturesToBeDeleted = [];

    /*
    // 建物管理
    //

    // 建物構造
    //

    // 築年
    private int _builtYear;
    public int BuiltYear
    {
        get
        {
            return _builtYear;
        }
        set
        {
            if (_builtYear == value) return;

            _builtYear = value;
            this.NotifyPropertyChanged("BuiltYear");

            // 変更フラグ
            IsDirty = true;
        }
    }

    // 地上n階建て
    private int _floors;
    public int Floors
    {
        get
        {
            return _floors;
        }
        set
        {
            if (_floors == value) return;

            _floors = value;
            this.NotifyPropertyChanged("Floors");

            // 変更フラグ
            IsDirty = true;
        }
    }

    // 地下n階建て
    private int _floorsBasement;
    public int FloorsBasement
    {
        get
        {
            return _floorsBasement;
        }
        set
        {
            if (_floorsBasement == value) return;

            _floorsBasement = value;
            this.NotifyPropertyChanged("FloorsBasement");

            // 変更フラグ
            IsDirty = true;
        }
    }

    // 総戸数
    private int _totalRoomNumber;
    public int TotalRoomNumber
    {
        get
        {
            return _totalRoomNumber;
        }
        set
        {
            if (_totalRoomNumber == value) return;

            _totalRoomNumber = value;
            this.NotifyPropertyChanged("TotalRoomNumber");

            // 変更フラグ
            IsDirty = true;
        }
    }

    // 取引態様
    //

    // 建物備考
    //

    // 建物設備
    //

    //

    //
    
    //

    // 建物設備備考
    //


    // 写真一覧
    public ObservableCollection<RentLivingPicture> RentLivingPictures { get; set; } = new ObservableCollection<RentLivingPicture>();

    // 写真のDBへの更新時にDBから削除されるべき物件写真のIDリスト
    public List<string> RentLivingPicturesToBeDeletedIDs = new List<string>();

    // 図面一覧
    public ObservableCollection<RentLivingPdf> RentLivingPdfs { get; set; } = new ObservableCollection<RentLivingPdf>();

    // 図面のDBへの更新時にDBから削除されるべき図面のIDリスト
    public List<string> RentLivingPdfsToBeDeletedIDs = new List<string>();

    // 部屋一覧
    public ObservableCollection<RentLivingRoom> RentLivingRooms { get; set; } = new ObservableCollection<RentLivingRoom>();

    // DBへの更新時にDBから削除されるべき部屋のIDリスト
    public List<string> RentLivingRoomToBeDeletedIDs = new List<string>();
    */


}
