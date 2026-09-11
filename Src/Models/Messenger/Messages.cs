using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Messenger;

public class PropertyStatusUpdatedMessage : ValueChangedMessage<Models.Base.EnumPropertyStatus>
{
    public PropertyStatusUpdatedMessage(Models.Base.EnumPropertyStatus value) : base(value)
    {

    }
}

public class PropertyNameUpdatedMessage : ValueChangedMessage<string>
{
    public PropertyNameUpdatedMessage(string value) : base(value)
    {

    }
}
