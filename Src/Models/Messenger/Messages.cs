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

public class ListingUpdatedMessage : ValueChangedMessage<Models.Rent.Residentials.Room.Listing>
{
    public ListingUpdatedMessage(Models.Rent.Residentials.Room.Listing value) : base(value)
    {

    }
}

public class ListingDeletedMessage : ValueChangedMessage<string>
{
    public ListingDeletedMessage(string value) : base(value)
    {

    }
}

public class ListingWindowClosedMessage : ValueChangedMessage<Views.Rent.Residentials.Room.EditorWindow> 
{
    public ListingWindowClosedMessage(Views.Rent.Residentials.Room.EditorWindow value) : base(value)
    {

    }
}

public class PropertyWindowClosedMessage : ValueChangedMessage<Views.Rent.Residentials.Bldg.EditorWindow>
{
    public PropertyWindowClosedMessage(Views.Rent.Residentials.Bldg.EditorWindow value) : base(value)
    {

    }
}

