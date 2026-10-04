using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models;

namespace ZumenSearch.Models.Base;

// Result Wrapper 
public abstract class ResultWrapperBase
{
    public Error Error = new();
    public bool IsError = false;
}
