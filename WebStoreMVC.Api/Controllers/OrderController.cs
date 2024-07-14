using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.FinancialConnections;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;
using Session = Stripe.Checkout.Session;
using SessionService = Stripe.Checkout.SessionService;

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
    /// Пользователь сохраняет данные для доставки, если всё прошло успешно, то переходим к оплате
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>Данные для доставки</returns>
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
            return RedirectToAction("StripePayment");
        }

        return View(dto);
    }
    
    /// <summary>
    /// Пользователь проводит оплату через Stripe
    /// </summary>
    /// <param name=></param>
    /// <returns>303 status code</returns>
    public async Task<ActionResult<ResponseDto>> StripePayment()
    {
        var response = await _orderService.StripePayment();
        
        TempData["Session"] = response.SuccessMessage;
            
        return new StatusCodeResult(303);
    }
    
    
    /// <summary>
    /// Если оплата проходит успешно, то убавляем товары из БД (т к их заказали) и отправляем пользователя
    /// к странице поздравления, иначе к странице неудачной транзакции
    /// Также если пользователь обновит страницу, то его перекинет на начальную страницу
    /// </summary>
    /// <param name=></param>
    /// <returns>View</returns>
    public async Task<IActionResult> OrderConfirmation()
    {
        var service = new SessionService();

        if (TempData["Session"] == null)
        {
            return RedirectToAction("Index", "Home");
        }

        Session session = service.Get(TempData["Session"].ToString());

        if (session.PaymentStatus == "paid")
        {
            await _orderService.SaveUserOrder();
            
            return View("SuccessfulTransaction");
        }

        return View("FailureTransaction");
    }

    /// <summary>
    /// Отправляет пользователя к странице удачной транзакции
    /// </summary>
    /// <param name=></param>
    /// <returns>View</returns>
    [HttpGet("SuccessfulTransaction")]
    [Route("SuccessfulTransaction")]
    public IActionResult SuccessfulTransaction()
    {
        return View();
    }
    
    /// <summary>
    /// Отправляет пользователя к странице неудачной транзакции
    /// </summary>
    /// <param name=></param>
    /// <returns>View</returns>
    [HttpGet("FailureTransaction")]
    [Route("FailureTransaction")]
    public IActionResult FailureTransaction()
    {
        return View();
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