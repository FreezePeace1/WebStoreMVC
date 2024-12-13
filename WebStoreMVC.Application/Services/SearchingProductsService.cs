using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Domain.Enum;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Services;

public class SearchingProductsService : ISearchingProductsService
{
    private readonly WebStoreContext _context;

    public SearchingProductsService(WebStoreContext context)
    {
        _context = context;
    }


    //Приведение строки к нормальному виду
    private string StringTransformation(string searchString)
    {
        // Преобразования строки к тому или иному виду
        searchString = searchString.Trim();
        searchString = searchString.ToLower();
        searchString = Regex.Replace(searchString, @"\s+", " ");

        //Исключение повторяющихся слов
        HashSet<string> str = new HashSet<string>();
        string[] data = searchString.Split(' ');
        for (int i = 0; i < data.Length; i++)
        {
            str.Add(data[i]);
        }

        searchString = string.Join(" ", str);

        return searchString;
    }

    //Распределение слов для условий
    private string[] StringDistribution(string searchString)
    {
        int spaceCounter = 0;

        for (int i = 0; i < searchString.Length; i++)
        {
            if (searchString[i].Equals(' '))
            {
                spaceCounter++;
            }
        }

        string hashtag = "";
        string hashtag2 = "";
        string productName = "";
        int hashtagsCounter = 0;

        while (spaceCounter >= 0)
        {
            string temp = searchString.Split()[spaceCounter];
            if (_context.Colors.Any(p => p.ColorName.ToLower().Contains(temp))
                || _context.Manufacturers.Any(p => p.ManufacturerName.ToLower().Contains(temp)))
            {
                hashtagsCounter++;
                if (hashtagsCounter <= 1)
                {
                    hashtag = temp;
                }
                else if (hashtagsCounter > 1)
                {
                    hashtag2 = temp;
                }
            }
            else if (_context.Products.Any(p => p.ProductName.ToLower().Contains(temp)))
            {
                productName = temp;
            }

            spaceCounter--;
        }

        string[] words = { hashtag, hashtag2, productName };

        return words;
    }


    private ProductSearchingModel FillingDataForPagination(ProductSearchingModel productSearchingModel, int currentPage,
        int pageSize, int totalPages, string searchString)
    {
        productSearchingModel.CurrentPage = currentPage;
        productSearchingModel.PageSize = pageSize;
        productSearchingModel.TotalPages = totalPages;
        productSearchingModel.Products = productSearchingModel.Products.Skip((currentPage - 1) * pageSize).Take(pageSize);
        productSearchingModel.SearchString = searchString;

        productSearchingModel.StartedPage = productSearchingModel.CurrentPage - 5;
        productSearchingModel.EndedPage = productSearchingModel.CurrentPage + 4;

        if (productSearchingModel.StartedPage <= 0)
        {
            productSearchingModel.EndedPage = productSearchingModel.EndedPage
                                              - (productSearchingModel.StartedPage - 1);
            productSearchingModel.StartedPage = 1;
        }

        if (productSearchingModel.EndedPage > productSearchingModel.TotalPages)
        {
            productSearchingModel.EndedPage = productSearchingModel.TotalPages;
            if (productSearchingModel.EndedPage > 10)
            {
                productSearchingModel.StartedPage = productSearchingModel.EndedPage - 9;
            }
        }

        return productSearchingModel;
    }
    
    public async Task<ResponseDto<ProductSearchingModel>> SearchingProducts(string searchString = "",
        int currentPage = 1)
    {

        var colors = await _context.Colors.ToListAsync();
        var categories = await _context.Categories.ToListAsync();
        var manufacturers = await _context.Manufacturers.ToListAsync();
        
        ProductSearchingModel productSearchingModel = new ProductSearchingModel();
        productSearchingModel.Colors = colors;
        productSearchingModel.Categories = categories;
        productSearchingModel.Manufacturers = manufacturers;
        var maxPrice = await _context.Products.OrderByDescending(x => x.Price).FirstOrDefaultAsync();
        var minPrice = await _context.Products.OrderByDescending(x => x.Price).LastAsync();
        /*productSearchingModel.MaxPrice = productFilter?.MaxPrice ?? maxPrice?.Price ?? 9999999;
        productSearchingModel.MinPrice = productFilter?.MinPrice ?? minPrice.Price;*/
        
        if (searchString != "")
        {
            searchString = StringTransformation(searchString);
            string[] words = StringDistribution(searchString);

            string hashtag = words[0];
            string hashtag2 = words[1];
            string productName = words[2];

            productSearchingModel.Products = _context.Products;

            Color? colorId = new Color();
            Manufacturer? manufacturerId = new Manufacturer();
            if (hashtag != "")
            {
                colorId = await _context.Colors.FirstOrDefaultAsync(x => x.ColorName.ToLower().Contains(hashtag));
                manufacturerId = await _context.Manufacturers.FirstOrDefaultAsync(x => x.ManufacturerName.ToLower().Contains(hashtag));
            }

            if (hashtag2 != "")
            {
                colorId = await _context.Colors.FirstOrDefaultAsync(x => x.ColorName.ToLower().Contains(hashtag2));
                manufacturerId = await _context.Manufacturers.FirstOrDefaultAsync(x => x.ManufacturerName.ToLower().Contains(hashtag2));
            }
            
            if (colorId != null && manufacturerId != null && productName != "")
            {
                productSearchingModel.Products = productSearchingModel.Products.Where(u =>( u.ColorId == colorId.Id
                    && u.ManufacturerId == manufacturerId.Id && u.ProductName.ToLower().Contains(productName)));

            }
            else if (colorId != null && manufacturerId != null && productName == "")
            {
                productSearchingModel.Products = productSearchingModel.Products.Where(u => u.ColorId == colorId.Id && u.ManufacturerId == manufacturerId.Id);

            }
            else if (manufacturerId != null || productName != "")
            {
                productSearchingModel.Products = productSearchingModel.Products.Where(u => u.ManufacturerId == manufacturerId.Id && u.ProductName.ToLower().Contains(productName));
            }
            else if (colorId != null || productName != "")
            {
                productSearchingModel.Products = productSearchingModel.Products.Where(u => u.ColorId == colorId.Id || u.ProductName.ToLower().Contains(searchString));
            }
            
            
            if (!productSearchingModel.Products.Any())
            {
                return new ResponseDto<ProductSearchingModel>()
                {
                    Data = productSearchingModel,
                    ErrorMessage = ErrorMessage.ProductsAreNotFound,
                    ErrorCode = (int)ErrorCode.ProductsAreNotFound
                };
            }
        }
        else
        {
            productSearchingModel.Products = _context.Products;
        }

        int totalProducts = productSearchingModel.Products.Count();
        int pageSize = 15;
        int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

        productSearchingModel =
            FillingDataForPagination(productSearchingModel, currentPage, pageSize, totalPages, searchString);

        return new ResponseDto<ProductSearchingModel>()
        {
            Data = productSearchingModel
        };
    }
}