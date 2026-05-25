using System.Diagnostics;

namespace ZumenSearch.Helpers;

public static class Common
{
    public static string ReplaceZenkakuNumber(string text)
    {
        return text
        .Replace('０', '0')
        .Replace('１', '1')
        .Replace('２', '2')
        .Replace('３', '3')
        .Replace('４', '4')
        .Replace('５', '5')
        .Replace('６', '6')
        .Replace('７', '7')
        .Replace('８', '8')
        .Replace('９', '9')
        .Replace("，", "")
        .Replace("、", "")
        .Replace(",", "")
        .Replace(",", "");
    }

    public static bool CanConvertToPositiveNumber(string text)
    {
        if (int.TryParse(text, out var result))
        {
            if (result > -1)
            {
                return true;
            }
            else
            {
                Debug.WriteLine("整数変換に失敗。（マイナス）");
                return false;
            }
        }
        else
        {
            Debug.WriteLine("整数変換に失敗。");
            return false;
        }
    }
}
