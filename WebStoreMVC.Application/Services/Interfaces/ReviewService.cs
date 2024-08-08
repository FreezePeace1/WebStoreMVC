using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;

namespace WebStoreMVC.Services.Interfaces;

public class ReviewService : IReviewService
{
    private readonly WebStoreContext _dbContext;
    private readonly ILogger _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IHttpContextAccessor _httpContext;

    public ReviewService(WebStoreContext dbContext, ILogger logger, UserManager<AppUser> userManager,
        IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
        _httpContext = httpContext;
    }

    /*public async Task<ResponseDto<ShowReviewsModel>> ShowReviews(int productId)
    {
        try
        {
            var reviews = await _dbContext.UserReviews.Where(x => x.ProductId == productId).ToListAsync();

            if (!reviews.Any() || reviews == null)
            {
                return new ResponseDto<ShowReviewsModel>()
                {
                    ErrorMessage = ErrorMessage.DBDoesNotHaveAnyReviews,
                    ErrorCode = (int)ErrorCode.DBDoesNotHaveAnyReviews
                };
            }

            int ratingCount = reviews.Count;
            double middleRatingAmount = (double)ratingCount / reviews.Sum(x => x.Rating);

            ShowReviewsModel model = new()
            {
                UserReviews = reviews,
                MiddleRateAmount = middleRatingAmount,
                RatingAmount = ratingCount
            };
            
            return new ResponseDto<ShowReviewsModel>()
            {
                SuccessMessage = SuccessMessage.ReviewsAreFound,
                Data = model
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto<ShowReviewsModel>()
            {
                ErrorMessage = ErrorMessage.FailureToGetAnyReviewsFromDB,
                ErrorCode = (int)ErrorCode.FailureToGetAnyReviewsFromDB
            };
        }
        
    }*/

    
    public async Task<ResponseDto> PostReview(PostReviewDto reviewDto, int productId)
    {
        try
        {
            //Проверяем что пользователь заказывал товар на который оставляется отзыв
            var userName = _httpContext.HttpContext.User.Identity.Name;
            AppUser user = new AppUser();
            if (!userName.IsNullOrEmpty())
            {
                user = await _userManager.FindByNameAsync(userName);
            }

            var userOrders = await _dbContext.Orders.Where(x => x.AppUserId == user.Id).ToListAsync();

            List<OrderProduct> orderWithNeededProduct = new List<OrderProduct>();
            foreach (var userOrder in userOrders)
            {
                orderWithNeededProduct =
                    await _dbContext.OrderProducts.Where(x => x.OrderId == userOrder.OrderId).ToListAsync();
            }

            var isNeededProduct = orderWithNeededProduct.Any(x => x.ProductId == productId);

            if (!isNeededProduct)
            {
                return new ResponseDto()
                {
                    ErrorMessage = ErrorMessage.AccessErrorToReviewProduct,
                    ErrorCode = (int)ErrorCode.AccessErrorToReviewProduct
                };
            }

            //Добавляем новый отзыв в БД
            var newReview = new UserReview()
            {
                Rating = reviewDto.Rating,
                AppUserId = user?.Id,
                ProductId = productId,
                ReviewDateTime = DateTime.Now,
                ReviewDescription = reviewDto.ReviewDescription,
                UserName = reviewDto.UserName,
                UserEmail = reviewDto.UserEmail
            };

            await _dbContext.UserReviews.AddAsync(newReview);
            await _dbContext.SaveChangesAsync();

            return new ResponseDto()
            {
                SuccessMessage = SuccessMessage.ReviewHasCreated
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);
            return new ResponseDto()
            {
                ErrorMessage = ErrorMessage.FailureToCreateUserReview,
                ErrorCode = (int)ErrorCode.FailureToCreateUserReview
            };
        }
    }
}