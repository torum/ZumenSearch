using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Models.Rent.Residentials
{
    public class Enums
    {

        // Kind：賃貸住居用の物件種目（アパート・マンション・一戸建て・他）
        public enum EnumKinds
        {
            Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
        }

        // 建物構造
        public enum EnumStructure
        {
            Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
        }

        // 建物管理形態
        public enum EnumManagement
        {
            Unspecified, Kashinushi, Tasha, Jisya, Unknown,
        }

        /*
        // TODO: not really used? 一括所有の建物か、区分所有か -> uise checkbox.
        public enum Ownerships
        {
            Unknown, All, Unit
        }
        */
    }
}
