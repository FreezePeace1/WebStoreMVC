using WebStoreMVC.Domain.Entities;
using WebStoreMVC.Dtos;
using WebStoreMVC.Models;

namespace WebStoreMVC.Services.Interfaces;

public interface IReviewService
{
    /*public Task<ResponseDto<ShowReviewsModel>> ShowReviews(int productId);*/
    public Task<ResponseDto> PostReview(PostReviewDto reviewDto,int productId);
}