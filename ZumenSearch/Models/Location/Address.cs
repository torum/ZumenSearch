using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumenSearch.Models.Location
{

    // 賃貸住居用物件の「都道府県」クラス
    public class Pref(int iD, string name)
    {
        public int ID { get; private set; } = iD;

        public string Name { get; private set; } = name;
    };


    // TODO:
    /*
    public Dictionary<string, int> PrefsDictionary
    {
        get; set;
    } = new Dictionary<string, int>()
        {
            {"北海道", 1},
            {"青森県", 2},
            {"岩手県", 3},
            {"宮城県", 4},
            {"秋田県", 5},
            {"山形県", 6},
            {"福島県", 7},
            {"茨城県", 8},
            {"栃木県", 9},
            {"群馬県", 10},
            {"埼玉県", 11},
            {"千葉県", 12},
            {"東京都", 13},
            {"神奈川県",14},
            {"新潟県", 15},
            {"富山県", 16},
            {"石川県", 17},
            {"福井県", 18},
            {"山梨県", 19},
            {"長野県", 20},
            {"岐阜県", 21},
            {"静岡県", 22},
            {"愛知県", 23},
            {"三重県", 24},
            {"滋賀県", 25},
            {"京都府", 26},
            {"大阪府", 27},
            {"兵庫県", 28},
            {"奈良県", 29},
            {"和歌山県",30},
            {"鳥取県", 31},
            {"島根県", 32},
            {"岡山県", 33},
            {"広島県", 34},
            {"山口県", 35},
            {"徳島県", 36},
            {"香川県", 37},
            {"愛媛県", 38},
            {"高知県", 39},
            {"福岡県", 40},
            {"佐賀県", 41},
            {"長崎県", 42},
            {"熊本県", 43},
            {"大分県", 44},
            {"宮崎県", 45},
            {"鹿児島県", 16},
            {"沖縄県", 47},
        };
    */

    /*
    public class Address
    {
        public Address(int prefID, string prefName, int cityID, string cityName, int townID, string townName)
        {
            this.Pref = new PrefPart(prefID, prefName);
            this.CityID = cityID;
            this.CityName = cityName;
            this.TownID = townID;
            this.TownName = townName;
        }
        public PrefPart Pref { get; private set; } // 都道府県
        public int CityID { get; private set; } // 市区町村コード
        public string CityName { get; private set; } // 市区町村名
        public int TownID { get; private set; } // 町域コード
        public string TownName { get; private set; } // 町域名
    }
    */

}
