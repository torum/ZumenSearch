using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ZumenSearch.Models.Messenger;

public class PropertyUpdatedMessage(Models.Base.PropertyBase value) : ValueChangedMessage<Models.Base.PropertyBase>(value)
{
}

public class ListingUpdatedMessage(Models.Base.ListingBase value) : ValueChangedMessage<Models.Base.ListingBase>(value)
{
}

public class LessorUpdatedMessage(Models.Base.PersonBase value) : ValueChangedMessage<Models.Base.PersonBase>(value)
{
}

public class BrokerUpdatedMessage(Models.Base.PersonBase value) : ValueChangedMessage<Models.Base.PersonBase>(value)
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

public class WindowClosedMessage(Microsoft.UI.Xaml.Window value) : ValueChangedMessage<Microsoft.UI.Xaml.Window>(value)
{
}

public class PropertyIsUnitOwnershipChangedMessage(bool value) : ValueChangedMessage<bool>(value)
{
}

