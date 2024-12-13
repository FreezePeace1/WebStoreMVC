using Microsoft.EntityFrameworkCore;
using WebStoreMVC.Domain.Entities;

namespace WebStoreMVC.Models;

public static class ProductExtension
{
    public static IQueryable<Product> Filter(this IQueryable<Product> products, ProductFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.Category))
        {
            products = products.Where(x => x.Category.CategoryName == filter.Category);
        }

        if (!string.IsNullOrEmpty(filter.Color))
        {
            products = products.Where(x => x.Color.ColorName == filter.Color);
        }

        if (!string.IsNullOrEmpty(filter.Manufacturer))
        {
            products = products.Where(x => x.Manufacturer.ManufacturerName == filter.Manufacturer);
        }

        if (filter.MinPrice != null || filter.MinPrice > 0)
        {
            products = products.Where(x => x.Price >= filter.MinPrice);
        }

        if (filter.MaxPrice != null || filter.MaxPrice > products.Min(x => x.Price))
        {
            products = products.Where(x => x.Price <= filter.MaxPrice);
        }

        return products;
    }
}