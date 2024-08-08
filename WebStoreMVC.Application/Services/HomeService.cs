using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Services;

public class HomeService : IHomeService
{
    private readonly WebStoreContext _context;
    private readonly IProductsService _productsService;
    private readonly ILogger _logger;

    public HomeService(WebStoreContext context, IProductsService productsService,ILogger logger)
    {
        _context = context;
        _productsService = productsService;
        _logger = logger;
    }

    public async Task<ResponseDto<List<Product>>> Store()
    {
        try
        {
            var products = await _context.Products.Take(15).ToListAsync();

            return new ResponseDto<List<Product>>()
            {
                Data = products,
                SuccessMessage = SuccessMessage.ProductsAreReceived
            };
        }
        catch (Exception e)
        {
            _logger.Error(e,e.Message);

            return new ResponseDto<List<Product>>()
            {
                ErrorMessage = ErrorMessage.ProductsAreNotFound,
                ErrorCode = (int)ErrorCode.ProductsAreNotFound
            };
        }
    }

    private AllInfoProductModel FillingDataForPagination(AllInfoProductModel productModel,int currentPage,int pageSize,int totalPages)
    {
        productModel.CurrentPage = currentPage;
        productModel.PageSize = pageSize;
        productModel.TotalPages = totalPages;
        productModel.UserReviews = productModel.UserReviews.Skip((currentPage - 1) * pageSize).Take(pageSize);

        productModel.StartedPage = productModel.CurrentPage - 5;
        productModel.EndedPage = productModel.CurrentPage + 4;

        if (productModel.StartedPage <= 0)
        {
            productModel.EndedPage = productModel.EndedPage
                                   - (productModel.StartedPage - 1);
            productModel.StartedPage = 1;
        }

        if (productModel.EndedPage > productModel.TotalPages)
        {
            productModel.EndedPage = productModel.TotalPages;
            if (productModel.EndedPage > 10)
            {
                productModel.StartedPage = productModel.EndedPage - 9;
            }
        }

        return productModel;
    }
    
    /*[SuppressMessage("ReSharper.DPA", "DPA0007: Large number of DB records", MessageId = "count: 113")]*/
    public async Task<ResponseDto<AllInfoProductModel>> ShowProductInfo(int id,int currentPage = 1)
    {
        try
        {
            var currentProduct = await _context.Products.Where(x => x.ProductId == id).FirstOrDefaultAsync();

            if (currentProduct == null)
            {
                return new ResponseDto<AllInfoProductModel>()
                {
                    ErrorMessage = ErrorMessage.ProductsAreNotFound,
                    ErrorCode = (int)ErrorCode.ProductsAreNotFound
                };
            }
            var reviews =  _context.UserReviews.Where(x => x.ProductId == id);

            var ratingCount = reviews.Count();
            var middleRatingAmount = (double) reviews.Sum(x => x.Rating) /  ratingCount;
            var oneStarCount = reviews.Count(x => x.Rating == 1);
            var twoStarCount = reviews.Count(x => x.Rating == 2);
            var threeStarCount = reviews.Count(x => x.Rating == 3);
            var fourStarCount = reviews.Count(x => x.Rating == 4);
            var fiveStarCount = reviews.Count(x => x.Rating == 5);

            AllInfoProductModel productModel = new()
            {
                ProductInfo = currentProduct,
                UserReviews = reviews,
                MiddleRateAmount = middleRatingAmount,
                RatingAmount = ratingCount,
                OneStarCount = oneStarCount,
                TwoStarCount = twoStarCount,
                ThreeStarCount = threeStarCount,
                FourStarCount = fourStarCount,
                FiveStarCount = fiveStarCount
            };

            int totalReviews = productModel.UserReviews.Count();
            int pageSize = 2;
            int totalPages = (int)Math.Ceiling(totalReviews / (double)pageSize);
            
            productModel = FillingDataForPagination(productModel,currentPage,pageSize,totalPages);
            
            return new ResponseDto<AllInfoProductModel>()
            {
                SuccessMessage = SuccessMessage.ReviewsAreFound,
                Data = productModel
            };
        }
        catch (Exception e)
        {
            _logger.Error(e, e.Message);

            return new ResponseDto<AllInfoProductModel>()
            {
                ErrorMessage = ErrorMessage.ProductsAreNotFound,
                ErrorCode = (int)ErrorCode.ProductsAreNotFound
            };
        }
    }
}