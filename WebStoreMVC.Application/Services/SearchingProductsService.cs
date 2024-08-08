using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using WebStoreMVC.Application.Resources;
using WebStoreMVC.DAL.Context;
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
            if (temp.Contains('#') || _context.Products.Any(p => p.Hashtags.ToLower().Contains(temp)))
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

    private ProductSearchingModel FillingDataForPagination(ProductSearchingModel productSearchingModel,int currentPage,int pageSize,int totalPages,string searchString)
    {
        productSearchingModel.CurrentPage = currentPage;
        productSearchingModel.PageSize = pageSize;
        productSearchingModel.TotalPages = totalPages;
        productSearchingModel.Products = productSearchingModel.Products.OrderBy(x => x.CategoryId).Skip((currentPage - 1) * pageSize).Take(pageSize);
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

    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    public async Task<ResponseDto<ProductSearchingModel>> SearchingProducts(string searchString = "",int currentPage = 1)
    {
        // Исключение, которые приводят сразу к выводу ошибки 
        /*if (searchString == "")
        {
            return new ResponseDto<List<Product>>()
            {
                ErrorMessage = ErrorMessage.ProductsAreNotFound,
                ErrorCode = (int)ErrorCode.ProductsAreNotFound
            };
        }*/

        ProductSearchingModel productSearchingModel = new ProductSearchingModel();
        if (searchString != "")
        {
            searchString = StringTransformation(searchString);
            string[] words = StringDistribution(searchString);

            string hashtag = words[0];
            string hashtag2 = words[1];
            string productName = words[2];
            
            productSearchingModel.Products = _context.Products;
        
            if (hashtag != "" && hashtag2 != "" && productName != "")
            {
                productSearchingModel.Products = productSearchingModel.Products.Where(u =>( u.Hashtags.ToLower().Contains(hashtag) && u.Hashtags.ToLower().Contains(hashtag2) && u.ProductName.ToLower().Contains(productName)));

            }
            else if (hashtag != "" && hashtag2 != "" && productName == "")
            {
                productSearchingModel.Products = productSearchingModel.Products.Where(u => u.Hashtags.ToLower().Contains(hashtag) && u.Hashtags.ToLower().Contains(hashtag2));

            }
            else if (hashtag != "" || productName != "")
            {
                productSearchingModel.Products = productSearchingModel.Products.Where(u => u.Hashtags.ToLower().Contains(hashtag) && u.ProductName.ToLower().Contains(productName));
            }
            else if (hashtag != "" || productName != "")
            {
                productSearchingModel.Products = productSearchingModel.Products.Where(u => u.Hashtags.ToLower().Contains(searchString) || u.ProductName.ToLower().Contains(searchString));   
            }

            if (!productSearchingModel.Products.Any())
            {
                return new ResponseDto<ProductSearchingModel>()
                {
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

        productSearchingModel = FillingDataForPagination(productSearchingModel, currentPage, pageSize, totalPages, searchString);
        
        return new ResponseDto<ProductSearchingModel>()
        {
            Data =  productSearchingModel
        };
    }
}