namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

public sealed class ListingSummary(string id, string name)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    //public bool IsPendingDraft { get; } = isPendingDraft;
}
