using WebMVC.Models;

namespace WebMVC.Services
{
    public interface IProductService
    {
        List<Product> GetProducts();
    }
}