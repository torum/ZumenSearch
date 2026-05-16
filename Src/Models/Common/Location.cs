namespace ZumenSearch.Models.Common;


// 賃貸住居用物件の「都道府県」クラス
public class Pref(string code, string municipalityCode, string name)
{
    // 都道府県コード
    public string Code { get; private set; } = code;

    // 市区町村コード
    // db:loc_pref_id
    public string MunicipalityCode { get; private set; } = municipalityCode;

    // db:loc_prefecture
    public string Name { get; private set; } = name;
};

public class CountyAndCity(string machiazaId, string county, string city)
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

public class WardAndOaza(string machiazaId, string ward, string oaza)
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

public class Choume(string machiazaId, string choume)
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

