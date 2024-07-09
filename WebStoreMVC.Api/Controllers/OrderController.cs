using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Controllers;


/// <summary>
/// Контроллер для сервиса заказов
/// </summary>
public class OrderController : Controller
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Пользователь записывает данные для доставки товара, а также:
    /// 1) Данные заказа
    /// 2) Данные корзины (товары связываются с номером заказа)
    /// Всё это записывается в БД
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>Данные о заказе</returns>
    [HttpPost("SaveCustomerInfo")]
    [Route("SaveCustomerInfo")]
    public async Task<ActionResult<ResponseDto<CustomerInfo>>> SaveCustomerInfo(CustomerInfoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        
        var response = await _orderService.SaveCustomerInfo(dto);

        if (!response.IsSucceed)
        {
            var error = response.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }
        
        if (response.IsSucceed)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(dto);
    }
    
    /// <summary>
    /// Берем с сессии данные корзины и показываем пользователю перед заполнением формы заказа
    /// </summary>
    /// <returns></returns>
    [HttpGet("ShowCartInfo")]
    [Route("ShowCartInfo")]
    public ActionResult<ResponseDto<List<CartItem>>> ShowCartInfo()
    {
        var response = _orderService.ShowCartInfo();

        if (response.IsSucceed)
        {
            return View(response);
        }
        
        if (!response.IsSucceed)
        {
            var error = response.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        return new ResponseDto<List<CartItem>>()
        {
            ErrorMessage = response.ErrorMessage,
            ErrorCode = response.ErrorCode
        };

    }

    /// <summary>
    /// Удаление заказа пользователя, доступ только у Админа
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("DeleteUserOrder")]
    [Route("DeleteUserOrder")]
    [Area("Admin"), Authorize(Policy = "AdminCookie", Roles = UserRoles.ADMINISTRATOR)]
    public async Task<ActionResult<ResponseDto>> DeleteUserOrder(string id)
    {
        var response = await _orderService.DeleteUserOrder(id);

        if (response.IsSucceed)
        {
            return RedirectToAction("Index", "Home");
        }

        return NotFound("Заказ не был найден");
    }
}