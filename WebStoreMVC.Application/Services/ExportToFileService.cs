using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using Serilog;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using Xceed.Words.NET;

namespace WebStoreMVC.Application.Services;

public class ExportToFileService : IExportToFileService
{
    private readonly WebStoreContext _context;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public ExportToFileService(WebStoreContext context, ILogger logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ResponseDto<List<string?>>> GetAllTableNames()
    {
        try
        {
            var tableNames = _context.Model.GetEntityTypes()
                .Select(x => x.GetTableName())
                .Distinct()
                .ToList();

            return new ResponseDto<List<string?>>()
            {
                Data = tableNames,
                SuccessMessage = SuccessMessage.GettingTableNamesIsDone
            };
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);

            return new ResponseDto<List<string?>>()
            {
                ErrorMessage = ErrorMessage.FailureToGetTableNames,
                ErrorCode = (int)ErrorCode.FailureToGetTableNames
            };
        }
    }

    public async Task<ResponseDto<byte[]>> GetCsvFile(string tableName)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        if (!_context.Model.GetEntityTypes().Any(x => x.GetTableName() == tableName))
        {
            return new ResponseDto<byte[]>()
            {
                ErrorMessage = ErrorMessage.FailureToGetTableNames,
                ErrorCode = (int)ErrorCode.FailureToGetTableNames
            };
        }

        try
        {
            await using (var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                var commandText = $"SELECT * FROM public.\"{tableName}\"";

                await using (var command = new NpgsqlCommand(commandText, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        var data = dataTable;

                        if (data == null)
                        {
                            return new ResponseDto<byte[]>()
                            {
                                ErrorMessage = ErrorMessage.FailureToGetData,
                                ErrorCode = (int)ErrorCode.FailureToGetData
                            };
                        }

                        StringBuilder csvData = new StringBuilder();

                        // Добавляем строки данных
                        foreach (DataRow row in dataTable.Rows)
                        {
                            for (int i = 0; i < dataTable.Columns.Count; i++)
                            {
                                if (row[i].ToString().Contains(",") || row[i].ToString().Contains(";"))
                                {
                                    csvData.Append($"\"{row[i]}\"");
                                    if (i < dataTable.Columns.Count - 1)
                                        csvData.Append(",");
                                }
                                else
                                {
                                    csvData.Append(row[i].ToString());
                                    if (i < dataTable.Columns.Count - 1)
                                        csvData.Append(",");
                                }
                            }

                            csvData.AppendLine();
                        }

                        var byteArray = Encoding.UTF8.GetBytes(csvData.ToString());

                        return new ResponseDto<byte[]>()
                        {
                            SuccessMessage = SuccessMessage.DataIsRecieved,
                            Data = byteArray
                        };
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);

            return new ResponseDto<byte[]>()
            {
                ErrorMessage = ErrorMessage.FailureToConvertFile,
                ErrorCode = (int)ErrorCode.FailureToConvertFile
            };
        }
    }

    public async Task<ResponseDto<byte[]>> GetJsonFile(string tableName)
    {
        if (!_context.Model.GetEntityTypes().Any(x => x.GetTableName() == tableName))
        {
            return new ResponseDto<byte[]>()
            {
                ErrorMessage = ErrorMessage.FailureToGetTableNames,
                ErrorCode = (int)ErrorCode.FailureToGetTableNames
            };
        }

        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            var dataTable = new DataTable();

            await using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var commandText = $"SELECT * FROM public.\"{tableName}\"";

                await using (var command = new NpgsqlCommand(commandText, connection))
                {
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count == 0)
                        {
                            return new ResponseDto<byte[]>
                            {
                                ErrorMessage = ErrorMessage.FailureToGetData,
                                ErrorCode = (int)ErrorCode.FailureToGetData
                            };
                        }

                        var productList = new List<object>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            var anonymousObject = new ExpandoObject() as IDictionary<string, Object>;
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                anonymousObject.Add(col.ColumnName, row[col]);
                            }

                            productList.Add(anonymousObject);
                        }

                        var jsonProductList = JsonConvert.SerializeObject(productList, Formatting.Indented);
                        var byteArray = Encoding.UTF8.GetBytes(jsonProductList);

                        return new ResponseDto<byte[]>()
                        {
                            Data = byteArray
                        };
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);

            return new ResponseDto<byte[]>()
            {
                ErrorMessage = ErrorMessage.FailureToConvertFile,
                ErrorCode = (int)ErrorCode.FailureToConvertFile
            };
        }
    }
    
}