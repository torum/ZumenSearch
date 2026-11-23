namespace ZumenSearch.Domain.Entities.Rent.Commercials;

public sealed class ListingSummary(string id, string name)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
}
