using System.Collections;

namespace WebStoreMVC.Dtos;

public class ResponseDto
{
    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }
    public bool IsSucceed => ErrorMessage == null;
    
    public int? ErrorCode { get; set; }
}

public class ResponseDto <T> : ResponseDto
{
    public ResponseDto(string errorMessage,string successMessage,int errorCode)
    {
        ErrorMessage = errorMessage;
        SuccessMessage = successMessage;
        ErrorCode = errorCode;
    }

    public ResponseDto()
    {
        
    }

    public T Data { get; set; }
}