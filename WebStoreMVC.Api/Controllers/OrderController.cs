using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IAuthService _authService;

    public OrderController(IOrderService orderService, IAuthService authService)
    {
        _orderService = orderService;
        _authService = authService;
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
            TempData["UserEmail"] = dto.UserEmail;
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

        Session session = await service.GetAsync(TempData["Session"]?.ToString());

        if (session.PaymentStatus == "paid")
        {
            string userPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var isUserExists = await _authService.IsUserExists(TempData["UserEmail"].ToString());
            if (!User.Identity.IsAuthenticated && !isUserExists)
            {
                RegisterDto registerDto = new()
                {
                    Email = TempData["UserEmail"].ToString(),
                    Username = $"User{Guid.NewGuid().ToString("N").Substring(0,5)}",
                    Password = userPassword,
                    ConfirmedPassword = userPassword
                };

                var registration = await _authService.Register(registerDto);
                if (registration.IsSucceed)
                {
                    await _orderService.SendUserInfoAfterSuccessfulOrder(registerDto);
                }
            }

            await _orderService.SaveUserOrder();

            var lastOrder = await _orderService.GetLastOrder();

            if (User.Identity.IsAuthenticated)
            {
                var orderWithUserMail = new OrderWithUserMail()
                {
                    Order = lastOrder.Data,
                    UserEmail = TempData["UserEmail"].ToString()
                };

                await _orderService.SendOrderToUserEmail(orderWithUserMail);
            }

            return View("SuccessfulTransaction", lastOrder.Data);
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
        return View(new Order());
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

    [HttpGet("FindOrder")]
    [Route("FindOrder")]
    public async Task<IActionResult> FindOrder(FindOrderModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var orderInfo = await _orderService.FindUserOrder(model.OrderId);

        if (orderInfo.IsSucceed)
        {
            return View("../Account/Index", orderInfo);
        }

        if (!orderInfo.IsSucceed)
        {
            var error = orderInfo.ErrorMessage;
            ModelState.AddModelError(string.Empty, error.ToString());
        }

        return View(model);
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