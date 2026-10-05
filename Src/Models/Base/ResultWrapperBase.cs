using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Models;

namespace ZumenSearch.Models.Base;

// Result Wrapper 
public abstract class ResultWrapperBase
{
    public ErrorInfo Error { get; set; } = new();
    public bool IsError { get; set; }
}
