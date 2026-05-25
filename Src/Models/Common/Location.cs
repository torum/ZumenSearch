namespace ZumenSearch.Models.Common;

//「都道府県」リストクラス
public sealed class PrefectureList()
{
    public List<Prefecture> Prefectures =
    [
        new Prefecture("01","010006", "北海道"),
        new Prefecture("02","020001", "青森県"),
            new Prefecture("03","030007", "岩手県"),
            new Prefecture("04","040002", "宮城県"),
            new Prefecture("05","050008", "秋田県"),
            new Prefecture("06","060003", "山形県"),
            new Prefecture("07","070009", "福島県"),
            new Prefecture("08","080004", "茨城県"),
            new Prefecture("09","090000", "栃木県"),
            new Prefecture("10","100005", "群馬県"),
            new Prefecture("11","110001", "埼玉県"),
            new Prefecture("12","120006", "千葉県"),
            new Prefecture("13","130001", "東京都"),
            new Prefecture("14","140007", "神奈川県"),
            new Prefecture("15","150002", "新潟県"),
            new Prefecture("16","160008", "富山県"),
            new Prefecture("17","170003", "石川県"),
            new Prefecture("18","180009", "福井県"),
            new Prefecture("19","190004", "山梨県"),
            new Prefecture("20","200000", "長野県"),
            new Prefecture("21","210005", "岐阜県"),
            new Prefecture("22","220001", "静岡県"),
            new Prefecture("23","230006", "愛知県"),
            new Prefecture("24","240001", "三重県"),
            new Prefecture("25","250007", "滋賀県"),
            new Prefecture("26","260002", "京都府"),
            new Prefecture("27","270008", "大阪府"),
            new Prefecture("28","280003", "兵庫県"),
            new Prefecture("29","290009", "奈良県"),
            new Prefecture("30","300004", "和歌山県"),
            new Prefecture("31","310000", "鳥取県"),
            new Prefecture("32","320005", "島根県"),
            new Prefecture("33","330001", "岡山県"),
            new Prefecture("34","340006", "広島県"),
            new Prefecture("35","350001", "山口県"),
            new Prefecture("36","360007", "徳島県"),
            new Prefecture("37","370002", "香川県"),
            new Prefecture("38","380008", "愛媛県"),
            new Prefecture("39","390003", "高知県"),
            new Prefecture("40","400009", "福岡県"),
            new Prefecture("41","410004", "佐賀県"),
            new Prefecture("42","420000", "長崎県"),
            new Prefecture("43","430005", "熊本県"),
            new Prefecture("44","440001", "大分県"),
            new Prefecture("45","450006", "宮崎県"),
            new Prefecture("46","460001", "鹿児島県"),
            new Prefecture("47","470007", "沖縄県"),
    ];
}

// 「都道府県」クラス
public sealed class Prefecture(string code, string municipalityCode, string name)
{
    // 都道府県コード
    public string Code { get; private set; } = code;

    // 市区町村コード
    // db:loc_pref_id
    public string MunicipalityCode { get; private set; } = municipalityCode;

    // db:loc_prefecture
    public string Name { get; private set; } = name;
};

public sealed class CountyAndCity(string machiazaId, string county, string city)
{
    // db:loc_machiaza_id
    public string MachiazaId
    {
        get; init;
    } = machiazaId;

    // 郡 db:loc_county
    public string County { get; init; } = county;

    // 市区町村 db:loc_city
    public string City { get; init; } = city;

    public string Combined
    {
        get
        {
            if (string.IsNullOrEmpty(County) && string.IsNullOrEmpty(City))
            {
                return "";//該当なし
            }
            else
            {
                return County + City;
            }
        }
    }
}

public sealed class WardAndOaza(string machiazaId, string ward, string oaza)
{
    // db:loc_machiaza_id
    public string MachiazaId
    {
        get; init;
    } = machiazaId;

    // db:loc_ward
    public string Ward
    {
        get; init;
    } = ward;

    // db:loc_oaza_cho
    public string Oaza
    {
        get; init;
    } = oaza;

    public string Combined
    {
        get
        {
            if (string.IsNullOrEmpty(Ward) && string.IsNullOrEmpty(Oaza))
            {
                return "";//該当なし
            }
            else
            {
                return Ward + Oaza;
            }
        }
    }
}

public sealed class Choume(string machiazaId, string choume)
{
    // db:loc_machiaza_id
    public string MachiazaId
    {
        get; init;
    } = machiazaId;

    // db:loc_choume
    public string Chou
    {
        get; init;
    } = choume;
}

