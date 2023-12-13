using WebMVC.Models;

namespace WebMVC.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts();
    }
}