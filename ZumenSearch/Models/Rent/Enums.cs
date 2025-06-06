using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Models.Rent
{
    internal class Enums
    {
        #region == Enums ==

        // Type：賃貸物件のタイプ（住居用・事業用・駐車場）
        public enum RentTypes
        {
            RentLiving, RentBussiness, RentParking
        }

        // Kind：賃貸住居用の物件種目（アパート・マンション・一戸建て・他）
        public enum RentLivingKinds
        {

            Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
        }

        // 建物管理形態
        public enum BuildingManagement
        {
            Unspecified, Kashinushi, Tasha, Jisya, Unknown,
        }

        public enum BuildingStructure
        {
            Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
        }

        // 一括所有の建物か、区分所有か
        public enum Ownerships
        {
            Unknown, All, Unit
        }

        #endregion
    }
}
