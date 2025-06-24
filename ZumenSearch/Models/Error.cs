using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Models;

namespace ZumenSearch.Models;

// ErrorInfo Class
public class ErrorObject
{
    public enum ErrTypes
    {
        DB, API, HTTP, XML, Other
    };

    // ErrTypes
    public ErrTypes ErrType { get; set; }

    // HTTP error code?
    public string ErrCode { get; set; } 

    // eg Error title, or type of Exception, .
    public string ErrDescription { get; set; }

    // Raw exception error messages, API error text translated via dictionary.
    public string ErrText { get; set;}

    // eg method name, or PATH info for REST
    public string ErrPlace { get; set; }

    // class name or site address 
    public string ErrPlaceParent { get; set; }  

    //
    public DateTime ErrDatetime { get; set; }

    public ErrorObject()
    {
        ErrType = ErrTypes.Other;
        ErrCode = "";
        ErrDescription = "";
        ErrText = "";
        ErrPlace = "";
        ErrPlaceParent = "";
        ErrDatetime = default;
    }
}
