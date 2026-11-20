namespace ZumenSearch.Application.Results;

// Result Wrapper 
public abstract class ResultWrapperBase
{
    public ErrorInfo Error { get; set; } = new();
    public bool IsError { get; set; }
}
