using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models.Base;

namespace ZumenSearch.Models.Person;

// Person
public sealed class ResultWrapper : ResultWrapperBase
{
    public PersonBase? Person { get; set; }
}
