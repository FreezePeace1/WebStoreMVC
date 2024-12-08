using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Application.Services;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;

namespace WebStoreMVC.Areas.Admin.Controllers;

[Route("[controller]")]
[Area("Admin"), Authorize(Policy = "AdminCookie", Roles = UserRoles.ADMINISTRATOR)]
public class ExportToFileController : Controller
{
    private readonly IExportToFileService _service;

    public ExportToFileController(IExportToFileService service)
    {
        _service = service;
    }

    [HttpGet]
    [Route("GetAllTableNames")]
    public IActionResult GetAllTableNames()
    {
        var response = _service.GetAllTableNames().Result;

        return View(response);
    }

    [HttpGet]
    [Route("GetCsvFile/{tableName}")]
    public async Task<IActionResult> GetCsvFile(string tableName)
    {
        var response = await _service.GetCsvFile(tableName);

        return File(response.Data, "application/octet-stream",$"{tableName}_csvData_{DateTime.Now}.csv");
    }

    [HttpGet]
    [Route("GetJsonFile/{tableName}")]
    public async Task<IActionResult> GetJsonFile(string tableName)
    {
        var response = await _service.GetJsonFile(tableName);

        return File(response.Data, "application/octet-stream", $"{tableName}_jsonData_{DateTime.Now}.json");
    }
}