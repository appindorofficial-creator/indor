namespace IndorMvcApp.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public bool ShowDetails { get; set; }

    public string? ExceptionType { get; set; }

    public string? ExceptionMessage { get; set; }

    public string? InnerExceptionMessage { get; set; }

    public string? StackTrace { get; set; }

    public string? Path { get; set; }
}
