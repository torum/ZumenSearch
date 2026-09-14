using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ZumenSearch.Helpers;

public static class Common
{
    public static string ReplaceZenkakuNumbers(string text)
    {
        // 1. Convert full-width numbers (０-９) to half-width (0-9)
        string convertedText = Regex.Replace(text, "[０-９]", m =>
            ((char)(m.Value[0] - '０' + '0')).ToString()
        );

        // 2. Remove all non-number characters
        string result = Regex.Replace(convertedText, "[^0-9]", "");

        return result;

        /*
        string intermediateText = Regex.Replace(text, "[，、,]", "");

        // 2. Convert full-width numbers (０-９) to half-width (0-9)
        string result = Regex.Replace(intermediateText, "[０-９]", m =>
            ((char)(m.Value[0] - '０' + '0')).ToString()
        );

        return result;
        */

        /*
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
        */
    }

    public static bool CanConvertToPositiveNumber(string text)
    {
        if (long.TryParse(text, out var result))
        {
            if (result > -1)
            {
                return true;
            }
            else
            {
                Debug.WriteLine($"整数変換に失敗（マイナス）：{text}");
                return false;
            }
        }
        else
        {
            Debug.WriteLine($"整数変換に失敗：{text}");
            return false;
        }
    }
}
