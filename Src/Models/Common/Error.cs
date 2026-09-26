namespace ZumenSearch.Models.Common;

// ErrorInfo Class
public sealed class ErrorObject
{
    public enum ErrTypes
    {
        DB, API, HTTP, XML, Other
    };

    // ErrTypes
    public ErrTypes Type { get; set; } = ErrTypes.Other;

    // HTTP error code?
    public string Code { get; set; } = string.Empty;

    // Raw exception error messages, API error text translated via dictionary.
    public string Message { get; set; } = string.Empty;

    // Addtional explanation.
    public string Description { get; set; } = string.Empty;

    // eg Error title, or type of Exception, .
    public string Title { get; set; } = string.Empty;

    // exception name, the message, the stack trace, and any inner exceptions
    public string FullDump { get; set; } = string.Empty;

    // eg method name, or PATH info for REST
    public string Operation { get; set; } = string.Empty;

    // class name or site address 
    public string MethodName { get; set; } = string.Empty;

    //
    public DateTime OccuredAt { get; set; } = default;

    public ErrorObject()
    {
    }
}
