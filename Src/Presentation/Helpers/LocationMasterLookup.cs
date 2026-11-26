using Microsoft.Data.Sqlite;

namespace ZumenSearch.Presentation.Helpers;

internal static class LocationMasterLookup
{
    public static List<T> Load<T>(Func<List<T>> query, Action<string> showWarning)
    {
        try
        {
            var items = query();
            showWarning(items.Count == 0 ? "住所マスタに選択できるデータがありません。住所を確認してください。" : string.Empty);
            return items;
        }
        catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException)
        {
            Debug.WriteLine($"Location master lookup failed: {ex}");
            showWarning("住所マスタを読み込めません。住所を確認してください。表示されていない住所は保存時に削除されます。");
            return [];
        }
    }
}
