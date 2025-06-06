using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ZumenSearch.Models.Rent.Enums;

namespace ZumenSearch.Models.Rent
{
    // 賃貸住居用の物件クラス（建物）
    public partial class RentResidential : Entry
    {
        public RentResidential()
        {
            //
        }

        public RentResidential(string id) : base(id)
        {
            //
        }

        /*
        // 賃貸住居用の物件種別　（アパート・マンション・戸建て・他）
        private RentLivingKinds _kind;
        public RentLivingKinds Kind
        {
            get
            {
                return _kind;
            }
            set
            {
                if (_kind == value) return;

                _kind = value;
                this.NotifyPropertyChanged(nameof(Kind));

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 賃貸住居用の物件種別ラベル
        public Dictionary<RentLivingKinds, string> RentLivingKindToLabel { get; } = new Dictionary<RentLivingKinds, string>()
        {
            {RentLivingKinds.Unspecified, ""},
            {RentLivingKinds.Apartment, "アパート"},
            {RentLivingKinds.Mansion, "マンション"},
            {RentLivingKinds.House, "一戸建て"},
            {RentLivingKinds.TerraceHouse, "テラスハウス"},
            {RentLivingKinds.TownHouse, "タウンハウス"},
            {RentLivingKinds.ShareHouse, "シェアハウス"},
            {RentLivingKinds.Dormitory, "寮・下宿"},
        };

        public Dictionary<string, RentLivingKinds> StringToRentLivingKind { get; } = new Dictionary<string, RentLivingKinds>()
        {
            {"Unspecified", RentLivingKinds.Unspecified},
            {"Apartment", RentLivingKinds.Apartment},
            {"Mansion", RentLivingKinds.Mansion},
            {"House", RentLivingKinds.House},
            {"TerraceHouse", RentLivingKinds.TerraceHouse},
            {"TownHouse", RentLivingKinds.TownHouse},
            {"ShareHouse", RentLivingKinds.ShareHouse},
            {"Dormitory", RentLivingKinds.Dormitory},
        };



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

}
