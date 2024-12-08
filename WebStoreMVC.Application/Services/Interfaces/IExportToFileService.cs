using WebStoreMVC.Dtos;

namespace WebStoreMVC.Application.Services;

public interface IExportToFileService
{
    public Task<ResponseDto<List<string?>>> GetAllTableNames();

    public Task<ResponseDto<byte[]>> GetCsvFile(string tableName);

    public Task<ResponseDto<byte[]>> GetJsonFile(string tableName);
    
}