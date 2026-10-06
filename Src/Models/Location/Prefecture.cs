namespace ZumenSearch.Models.Location;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor
public sealed class Prefecture
{
    public Prefecture(string code, string municipalityCode, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(municipalityCode)) throw new ArgumentException("MunicipalityCode cannot be empty.", nameof(municipalityCode));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        Code = code;
        MunicipalityCode = municipalityCode;
        Name = name;
    }

    public Prefecture(string municipalityCode, string? name = "")
    {
        if (string.IsNullOrWhiteSpace(municipalityCode)) throw new ArgumentException("MunicipalityCode cannot be empty.", nameof(municipalityCode));

        MunicipalityCode = municipalityCode;

        if (string.IsNullOrWhiteSpace(name))
        {
            var prefecture = PrefectureMaster.Prefectures.FirstOrDefault(p => p.MunicipalityCode == municipalityCode) ?? throw new ArgumentException($"No prefecture found with MunicipalityCode '{municipalityCode}'.", nameof(municipalityCode));
            Code = prefecture.Code;
            Name = prefecture.Name;
        }
        else
        {
            Name = name;
        }
    }


    // 都道府県コード not used in the database, just in case.
    public string Code { get; private set; } = string.Empty;

    // 市区町村コード
    // db:location_pref_id
    public string MunicipalityCode { get; private set; } = string.Empty;

    // 都道府県名 not used in the database, reference from PrefectureMaster.Prefectures list.
    // db:location_prefecture
    public string Name { get; private set; } = string.Empty;

}
