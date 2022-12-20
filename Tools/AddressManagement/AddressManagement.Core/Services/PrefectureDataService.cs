using System.Collections.Generic;
using System.Diagnostics;
using AddressManagement.Core.Contracts.Services;
using AddressManagement.Core.Models;

namespace AddressManagement.Core.Services;

// 1. Contracts/Services/IPrefectureDataService.cs
// 2. Services/PrefectureDataService.cs
// 3. Models/Prefecture.cs
public class PrefectureDataService : IPrefectureDataService
{
    private List<PrefectureCode> _prefectures;

    public PrefectureDataService()
    {
        _prefectures = new List<PrefectureCode>(Prefectures());
    }

    private static IEnumerable<PrefectureCode> Prefectures()
    {
        return new List<PrefectureCode>() 
        {
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "010006",
                Code = "01",
                PrefectureName = "北海道",
                PrefectureNameKana = "ホッカイドウ",
                PrefectureNameEnglish = "Hokkaido"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "020001",
                Code = "02",
                PrefectureName = "青森県",
                PrefectureNameKana = "アオモリケン",
                PrefectureNameEnglish = "Aomori"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "030007",
                Code = "03",
                PrefectureName = "岩手県",
                PrefectureNameKana = "イワテケン",
                PrefectureNameEnglish = "Iwate"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "040002",
                Code = "04",
                PrefectureName = "宮城県",
                PrefectureNameKana = "ミヤギケン",
                PrefectureNameEnglish = "Miyagi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "050008",
                Code = "05",
                PrefectureName = "秋田県",
                PrefectureNameKana = "アキタケン",
                PrefectureNameEnglish = "Akita"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "060003",
                Code = "06",
                PrefectureName = "山形県",
                PrefectureNameKana = "ヤマガタケン",
                PrefectureNameEnglish = "Yamagata"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "070009",
                Code = "07",
                PrefectureName = "福島県",
                PrefectureNameKana = "フクシマケン",
                PrefectureNameEnglish = "Fukushima"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "080004",
                Code = "08",
                PrefectureName = "茨城県",
                PrefectureNameKana = "イバラキケン",
                PrefectureNameEnglish = "Ibaraki"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "090000",
                Code = "09",
                PrefectureName = "栃木県",
                PrefectureNameKana = "トチギケン",
                PrefectureNameEnglish = "Tochigi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "100005",
                Code = "10",
                PrefectureName = "群馬県",
                PrefectureNameKana = "グンマケン",
                PrefectureNameEnglish = "Gumma"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "110001",
                Code = "11",
                PrefectureName = "埼玉県",
                PrefectureNameKana = "サイタマケン",
                PrefectureNameEnglish = "Saitama"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "120006",
                Code = "12",
                PrefectureName = "千葉県",
                PrefectureNameKana = "チバケン",
                PrefectureNameEnglish = "Chiba"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "130001",
                Code = "13",
                PrefectureName = "東京都",
                PrefectureNameKana = "トウキョウト",
                PrefectureNameEnglish = "Tokyo"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "140007",
                Code = "14",
                PrefectureName = "神奈川県",
                PrefectureNameKana = "カナガワケン",
                PrefectureNameEnglish = "Kanagawa"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "150002",
                Code = "15",
                PrefectureName = "新潟県",
                PrefectureNameKana = "ニイガタケン",
                PrefectureNameEnglish = "Niigata"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "160008",
                Code = "16",
                PrefectureName = "富山県",
                PrefectureNameKana = "トヤマケン",
                PrefectureNameEnglish = "Toyama"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "170003",
                Code = "17",
                PrefectureName = "石川県",
                PrefectureNameKana = "イシカワケン",
                PrefectureNameEnglish = "Ishikawa"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "180009",
                Code = "18",
                PrefectureName = "福井県",
                PrefectureNameKana = "フクイケン",
                PrefectureNameEnglish = "Fukui"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "190004",
                Code = "19",
                PrefectureName = "山梨県",
                PrefectureNameKana = "ヤマナシケン",
                PrefectureNameEnglish = "Yamanashi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "200000",
                Code = "20",
                PrefectureName = "長野県",
                PrefectureNameKana = "ナガノケン",
                PrefectureNameEnglish = "Nagano"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "210005",
                Code = "21",
                PrefectureName = "岐阜県",
                PrefectureNameKana = "ギフケン",
                PrefectureNameEnglish = "Gifu"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "220001",
                Code = "22",
                PrefectureName = "静岡県",
                PrefectureNameKana = "シズオカケン",
                PrefectureNameEnglish = "Shizuoka"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "230006",
                Code = "23",
                PrefectureName = "愛知県",
                PrefectureNameKana = "アイチケン",
                PrefectureNameEnglish = "Aichi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "240001",
                Code = "24",
                PrefectureName = "三重県",
                PrefectureNameKana = "ミエケン",
                PrefectureNameEnglish = "Mie"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "250007",
                Code = "25",
                PrefectureName = "滋賀県",
                PrefectureNameKana = "シガケン",
                PrefectureNameEnglish = "Shiga"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "260002",
                Code = "26",
                PrefectureName = "京都府",
                PrefectureNameKana = "キョウトフ",
                PrefectureNameEnglish = "Kyoto"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "270008",
                Code = "27",
                PrefectureName = "大阪府",
                PrefectureNameKana = "オオサカフ",
                PrefectureNameEnglish = "Osaka"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "280003",
                Code = "28",
                PrefectureName = "兵庫県",
                PrefectureNameKana = "ヒョウゴケン",
                PrefectureNameEnglish = "Hyogo"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "290009",
                Code = "29",
                PrefectureName = "奈良県",
                PrefectureNameKana = "ナラケン",
                PrefectureNameEnglish = "Nara"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "300004",
                Code = "30",
                PrefectureName = "和歌山県",
                PrefectureNameKana = "ワカヤマケン",
                PrefectureNameEnglish = "Wakayama"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "310000",
                Code = "31",
                PrefectureName = "鳥取県",
                PrefectureNameKana = "トットリケン",
                PrefectureNameEnglish = "Tottori"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "320005",
                Code = "32",
                PrefectureName = "島根県",
                PrefectureNameKana = "シマネケン",
                PrefectureNameEnglish = "Shimane"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "330001",
                Code = "33",
                PrefectureName = "岡山県",
                PrefectureNameKana = "オカヤマケン",
                PrefectureNameEnglish = "Okayama"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "340006",
                Code = "34",
                PrefectureName = "広島県",
                PrefectureNameKana = "ヒロシマケン",
                PrefectureNameEnglish = "Hiroshima"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "350001",
                Code = "35",
                PrefectureName = "山口県",
                PrefectureNameKana = "ヤマグチケン",
                PrefectureNameEnglish = "Yamaguchi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "360007",
                Code = "36",
                PrefectureName = "徳島県",
                PrefectureNameKana = "トクシマケン",
                PrefectureNameEnglish = "Tokushima"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "370002",
                Code = "37",
                PrefectureName = "香川県",
                PrefectureNameKana = "カガワケン",
                PrefectureNameEnglish = "Kagawa"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "380008",
                Code = "38",
                PrefectureName = "愛媛県",
                PrefectureNameKana = "エヒメケン",
                PrefectureNameEnglish = "Ehime"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "390003",
                Code = "39",
                PrefectureName = "高知県",
                PrefectureNameKana = "コウチケン",
                PrefectureNameEnglish = "Kochi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "400009",
                Code = "40",
                PrefectureName = "福岡県",
                PrefectureNameKana = "フクオカケン",
                PrefectureNameEnglish = "Fukuoka"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "410004",
                Code = "41",
                PrefectureName = "佐賀県",
                PrefectureNameKana = "サガケン",
                PrefectureNameEnglish = "Saga"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "420000",
                Code = "42",
                PrefectureName = "長崎県",
                PrefectureNameKana = "ナガサキケン",
                PrefectureNameEnglish = "Nagasaki"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "430005",
                Code = "43",
                PrefectureName = "熊本県",
                PrefectureNameKana = "クマモトケン",
                PrefectureNameEnglish = "Kumamoto"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "440001",
                Code = "44",
                PrefectureName = "大分県",
                PrefectureNameKana = "オオイタケン",
                PrefectureNameEnglish = "Oita"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "450006",
                Code = "45",
                PrefectureName = "宮崎県",
                PrefectureNameKana = "ミヤザキケン",
                PrefectureNameEnglish = "Miyazaki"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "460001",
                Code = "46",
                PrefectureName = "鹿児島県",
                PrefectureNameKana = "カゴシマケン",
                PrefectureNameEnglish = "Kagoshima"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCode = "470007",
                Code = "47",
                PrefectureName = "沖縄県",
                PrefectureNameKana = "オキナワケン",
                PrefectureNameEnglish = "Okinawa"
            }
        };
    }

    public async Task<IEnumerable<PrefectureCode>> GetPrefectureDataAsync()
    {
        _prefectures ??= new List<PrefectureCode>(Prefectures());

        await Task.CompletedTask;
        return _prefectures;
    }

}
