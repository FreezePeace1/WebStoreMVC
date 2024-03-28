namespace WebStoreMVC.Dtos;

public class ResponseDto
{
    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }
    public bool IsSucceed => ErrorMessage == null;
    
    public int? ErrorCode { get; set; }
}