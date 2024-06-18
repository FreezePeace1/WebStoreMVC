using WebStoreMVC.DAL.Context;
using WebStoreMVC.Dtos;
using WebStoreMVC.Services.Interfaces;

namespace WebStoreMVC.Application.Services;

public class CartService : ICartService
{
    private readonly WebStoreContext _dbcontext;

    public CartService(WebStoreContext dbcontext)
    {
        _dbcontext = dbcontext;
    }
    
    public ResponseDto Index()
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto> Add(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto> Decrease(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseDto> Remove(int id)
    {
        throw new NotImplementedException();
    }

    public ResponseDto Clear()
    {
        throw new NotImplementedException();
    }
}