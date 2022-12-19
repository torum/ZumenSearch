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
                AdministrativeDivisionCodeID = 010006,
                PrefectureCodeID = 1,
                PrefectureName = "北海道",
                PrefectureNameKana = "ホッカイドウ",
                PrefectureNameEnglish = "Hokkaido"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 020001,
                PrefectureCodeID = 2,
                PrefectureName = "青森県",
                PrefectureNameKana = "アオモリケン",
                PrefectureNameEnglish = "Aomori"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 030007,
                PrefectureCodeID = 3,
                PrefectureName = "岩手県",
                PrefectureNameKana = "イワテケン",
                PrefectureNameEnglish = "Iwate"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 040002,
                PrefectureCodeID = 4,
                PrefectureName = "宮城県",
                PrefectureNameKana = "ミヤギケン",
                PrefectureNameEnglish = "Miyagi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 050008,
                PrefectureCodeID = 5,
                PrefectureName = "秋田県",
                PrefectureNameKana = "アキタケン",
                PrefectureNameEnglish = "Akita"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 060003,
                PrefectureCodeID = 6,
                PrefectureName = "山形県",
                PrefectureNameKana = "ヤマガタケン",
                PrefectureNameEnglish = "Yamagata"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 070009,
                PrefectureCodeID = 7,
                PrefectureName = "福島県",
                PrefectureNameKana = "フクシマケン",
                PrefectureNameEnglish = "Fukushima"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 080004,
                PrefectureCodeID = 8,
                PrefectureName = "茨城県",
                PrefectureNameKana = "イバラキケン",
                PrefectureNameEnglish = "Ibaraki"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 090000,
                PrefectureCodeID = 9,
                PrefectureName = "栃木県",
                PrefectureNameKana = "トチギケン",
                PrefectureNameEnglish = "Tochigi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 100005,
                PrefectureCodeID = 10,
                PrefectureName = "群馬県",
                PrefectureNameKana = "グンマケン",
                PrefectureNameEnglish = "Gumma"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 110001,
                PrefectureCodeID = 11,
                PrefectureName = "埼玉県",
                PrefectureNameKana = "サイタマケン",
                PrefectureNameEnglish = "Saitama"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 120006,
                PrefectureCodeID = 12,
                PrefectureName = "千葉県",
                PrefectureNameKana = "チバケン",
                PrefectureNameEnglish = "Chiba"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 130001,
                PrefectureCodeID = 13,
                PrefectureName = "東京都",
                PrefectureNameKana = "トウキョウト",
                PrefectureNameEnglish = "Tokyo"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 140007,
                PrefectureCodeID = 14,
                PrefectureName = "神奈川県",
                PrefectureNameKana = "カナガワケン",
                PrefectureNameEnglish = "Kanagawa"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 150002,
                PrefectureCodeID = 15,
                PrefectureName = "新潟県",
                PrefectureNameKana = "ニイガタケン",
                PrefectureNameEnglish = "Niigata"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 160008,
                PrefectureCodeID = 16,
                PrefectureName = "富山県",
                PrefectureNameKana = "トヤマケン",
                PrefectureNameEnglish = "Toyama"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 170003,
                PrefectureCodeID = 17,
                PrefectureName = "石川県",
                PrefectureNameKana = "イシカワケン",
                PrefectureNameEnglish = "Ishikawa"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 180009,
                PrefectureCodeID = 18,
                PrefectureName = "福井県",
                PrefectureNameKana = "フクイケン",
                PrefectureNameEnglish = "Fukui"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 190004,
                PrefectureCodeID = 19,
                PrefectureName = "山梨県",
                PrefectureNameKana = "ヤマナシケン",
                PrefectureNameEnglish = "Yamanashi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 200000,
                PrefectureCodeID = 20,
                PrefectureName = "長野県",
                PrefectureNameKana = "ナガノケン",
                PrefectureNameEnglish = "Nagano"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 210005,
                PrefectureCodeID = 21,
                PrefectureName = "岐阜県",
                PrefectureNameKana = "ギフケン",
                PrefectureNameEnglish = "Gifu"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 220001,
                PrefectureCodeID = 22,
                PrefectureName = "静岡県",
                PrefectureNameKana = "シズオカケン",
                PrefectureNameEnglish = "Shizuoka"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 230006,
                PrefectureCodeID = 23,
                PrefectureName = "愛知県",
                PrefectureNameKana = "アイチケン",
                PrefectureNameEnglish = "Aichi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 240001,
                PrefectureCodeID = 24,
                PrefectureName = "三重県",
                PrefectureNameKana = "ミエケン",
                PrefectureNameEnglish = "Mie"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 250007,
                PrefectureCodeID = 25,
                PrefectureName = "滋賀県",
                PrefectureNameKana = "シガケン",
                PrefectureNameEnglish = "Shiga"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 260002,
                PrefectureCodeID = 26,
                PrefectureName = "京都府",
                PrefectureNameKana = "キョウトフ",
                PrefectureNameEnglish = "Kyoto"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 270008,
                PrefectureCodeID = 27,
                PrefectureName = "大阪府",
                PrefectureNameKana = "オオサカフ",
                PrefectureNameEnglish = "Osaka"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 280003,
                PrefectureCodeID = 28,
                PrefectureName = "兵庫県",
                PrefectureNameKana = "ヒョウゴケン",
                PrefectureNameEnglish = "Hyogo"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 290009,
                PrefectureCodeID = 29,
                PrefectureName = "奈良県",
                PrefectureNameKana = "ナラケン",
                PrefectureNameEnglish = "Nara"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 300004,
                PrefectureCodeID = 30,
                PrefectureName = "和歌山県",
                PrefectureNameKana = "ワカヤマケン",
                PrefectureNameEnglish = "Wakayama"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 310000,
                PrefectureCodeID = 31,
                PrefectureName = "鳥取県",
                PrefectureNameKana = "トットリケン",
                PrefectureNameEnglish = "Tottori"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 320005,
                PrefectureCodeID = 32,
                PrefectureName = "島根県",
                PrefectureNameKana = "シマネケン",
                PrefectureNameEnglish = "Shimane"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 330001,
                PrefectureCodeID = 33,
                PrefectureName = "岡山県",
                PrefectureNameKana = "オカヤマケン",
                PrefectureNameEnglish = "Okayama"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 340006,
                PrefectureCodeID = 34,
                PrefectureName = "広島県",
                PrefectureNameKana = "ヒロシマケン",
                PrefectureNameEnglish = "Hiroshima"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 350001,
                PrefectureCodeID = 35,
                PrefectureName = "山口県",
                PrefectureNameKana = "ヤマグチケン",
                PrefectureNameEnglish = "Yamaguchi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 360007,
                PrefectureCodeID = 36,
                PrefectureName = "徳島県",
                PrefectureNameKana = "トクシマケン",
                PrefectureNameEnglish = "Tokushima"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 370002,
                PrefectureCodeID = 37,
                PrefectureName = "香川県",
                PrefectureNameKana = "カガワケン",
                PrefectureNameEnglish = "Kagawa"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 380008,
                PrefectureCodeID = 38,
                PrefectureName = "愛媛県",
                PrefectureNameKana = "エヒメケン",
                PrefectureNameEnglish = "Ehime"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 390003,
                PrefectureCodeID = 39,
                PrefectureName = "高知県",
                PrefectureNameKana = "コウチケン",
                PrefectureNameEnglish = "Kochi"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 400009,
                PrefectureCodeID = 40,
                PrefectureName = "福岡県",
                PrefectureNameKana = "フクオカケン",
                PrefectureNameEnglish = "Fukuoka"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 410004,
                PrefectureCodeID = 41,
                PrefectureName = "佐賀県",
                PrefectureNameKana = "サガケン",
                PrefectureNameEnglish = "Saga"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 420000,
                PrefectureCodeID = 42,
                PrefectureName = "長崎県",
                PrefectureNameKana = "ナガサキケン",
                PrefectureNameEnglish = "Nagasaki"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 430005,
                PrefectureCodeID = 43,
                PrefectureName = "熊本県",
                PrefectureNameKana = "クマモトケン",
                PrefectureNameEnglish = "Kumamoto"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 440001,
                PrefectureCodeID = 44,
                PrefectureName = "大分県",
                PrefectureNameKana = "オオイタケン",
                PrefectureNameEnglish = "Oita"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 450006,
                PrefectureCodeID = 45,
                PrefectureName = "宮崎県",
                PrefectureNameKana = "ミヤザキケン",
                PrefectureNameEnglish = "Miyazaki"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 460001,
                PrefectureCodeID = 46,
                PrefectureName = "鹿児島県",
                PrefectureNameKana = "カゴシマケン",
                PrefectureNameEnglish = "Kagoshima"
            },
            new PrefectureCode()
            {
                AdministrativeDivisionCodeID = 470007,
                PrefectureCodeID = 47,
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
