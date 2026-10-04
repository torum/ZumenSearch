using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Location;

public sealed class Prefecture(string code, string municipalityCode, string name)
{
    // 都道府県コード
    public string Code { get; private set; } = code;

    // 市区町村コード
    // db:location_pref_id
    public string MunicipalityCode { get; private set; } = municipalityCode;

    // db:location_prefecture
    public string Name { get; private set; } = name;
};
