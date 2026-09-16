using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Messenger;

public class PropertyUpdatedMessage : ValueChangedMessage<Models.Base.PropertyBase>
{
    public PropertyUpdatedMessage(Models.Base.PropertyBase value) : base(value)
    {

    }
}

public class ListingUpdatedMessage : ValueChangedMessage<Models.Rent.Residentials.Listing.Listing>
{
    public ListingUpdatedMessage(Models.Rent.Residentials.Listing.Listing value) : base(value)
    {

    }
}

public class ListingDeletedMessage : ValueChangedMessage<string>
{
    public ListingDeletedMessage(string value) : base(value)
    {

    }
}

public class ListingWindowClosedMessage : ValueChangedMessage<Views.Rent.Residentials.Listing.EditorWindow> 
{
    public ListingWindowClosedMessage(Views.Rent.Residentials.Listing.EditorWindow value) : base(value)
    {

    }
}

public class PropertyWindowClosedMessage : ValueChangedMessage<Views.Rent.Residentials.EditorWindow>
{
    public PropertyWindowClosedMessage(Views.Rent.Residentials.EditorWindow value) : base(value)
    {

    }
}

public class LessorWindowClosedMessage : ValueChangedMessage<Views.Rent.Lessors.EditorWindow>
{
    public LessorWindowClosedMessage(Views.Rent.Lessors.EditorWindow value) : base(value)
    {

    }
}

public class PropertyIsUnitOwnershipChangedMessage : ValueChangedMessage<bool>
{
    public PropertyIsUnitOwnershipChangedMessage(bool value) : base(value)
    {

    }
}

