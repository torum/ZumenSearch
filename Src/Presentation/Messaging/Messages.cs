using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ZumenSearch.Presentation.Messaging;

public class PropertyUpdatedMessage(ZumenSearch.Domain.Entities.Base.PropertyBase value) : ValueChangedMessage<ZumenSearch.Domain.Entities.Base.PropertyBase>(value)
{
}

public class ListingUpdatedMessage(ZumenSearch.Domain.Entities.Base.ListingBase value) : ValueChangedMessage<ZumenSearch.Domain.Entities.Base.ListingBase>(value)
{
}

public class LessorUpdatedMessage(ZumenSearch.Domain.Entities.Base.PersonBase value) : ValueChangedMessage<ZumenSearch.Domain.Entities.Base.PersonBase>(value)
{
}

public class BrokerUpdatedMessage(ZumenSearch.Domain.Entities.Base.PersonBase value) : ValueChangedMessage<ZumenSearch.Domain.Entities.Base.PersonBase>(value)
{
}

public class ListingDeletedMessage(string value) : ValueChangedMessage<string>(value)
{
}

public class LessorDeletedMessage(string value) : ValueChangedMessage<string>(value)
{
}

public class BrokerDeletedMessage(string value) : ValueChangedMessage<string>(value)
{
}

public class WindowClosedMessage<TWindow>(TWindow value) : ValueChangedMessage<TWindow>(value)
{
}

public class PropertyIsUnitOwnershipChangedMessage(string propertyId, bool value) : ValueChangedMessage<bool>(value)
{
    public string PropertyId { get; } = propertyId;
}

public record ShowErrorInfoBarInMainWindowMessage(string Title, string Message);
