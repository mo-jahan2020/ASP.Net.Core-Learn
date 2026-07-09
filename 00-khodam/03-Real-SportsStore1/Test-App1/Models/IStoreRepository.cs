namespace Test_App1.Models
{
    public interface IStoreRepository 
    {
        IQueryable<Product> Products { get; }
        Task<IEnumerable<Product>> GetPagedProductsAsync(int pageNumber, int pageSize);
        Task<int> GetTotalProducesCountAsync();
        //IEnumerable<Product> Products { get; }
        void AddProduct(Product product);
        void SaveProduct(Product product);
        void DeleteProduct(int productId);
        
    } 
}
