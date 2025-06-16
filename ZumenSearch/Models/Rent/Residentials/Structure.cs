using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Models.Rent.Residentials
{
    // 建物構造
    public enum EnumStructure
    {
        Unspecified, Wood, Block, LightSteel, Steel, RC, SRC, ALC, PC, HPC, RB, CFT, Other
    }

    // 賃貸住居用物件の「構造」クラス
    public class Structure(string key, string label)
    {
        public string Label { get; set; } = label;
        public string Key { get; set; } = key;
    };
}
