using System.ComponentModel.DataAnnotations;
using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;

namespace WebStoreMVC.Models;

public class AllInfoProductModel
{
    public IQueryable<UserReview> UserReviews { get; set; }
    
    public double MiddleRateAmount { get; set; }
    public int RatingAmount { get; set; }
    public int OneStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int FiveStarCount { get; set; }
    
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int StartedPage { get; set; }
    public int EndedPage { get; set; }
    
    public Product ProductInfo { get; set; }
    
}