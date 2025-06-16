using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Models.Rent.Residentials
{
    // Kind：賃貸住居用の物件種目（アパート・マンション・一戸建て・他）
    public enum EnumKinds
    {
        Unspecified, Apartment, Mansion, House, TerraceHouse, TownHouse, ShareHouse, Dormitory
    }

    // 賃貸住居用物件の「種別」クラス
    public class Kind(string key, string label)
    {
        public string Label { get; set; } = label;
        public string Key { get; set; } = key;
    };



}
