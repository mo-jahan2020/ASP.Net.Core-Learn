namespace SportsStore.Models.ViewModels {

    /// <summary>
    /// ViewModel برای لیست سفارشات با Pagination
    /// </summary>
    public class OrdersListViewModel {
        public IEnumerable<Order> Orders { get; set; }
            = Enumerable.Empty<Order>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
    }
}
