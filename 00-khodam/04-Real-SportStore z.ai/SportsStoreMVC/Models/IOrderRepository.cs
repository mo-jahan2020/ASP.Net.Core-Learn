namespace SportsStore.Models {

    /// <summary>
    /// اینترفیس Repository سفارشات
    /// </summary>
    public interface IOrderRepository {

        // لیست سفارشات به همراه خطوط و محصولات هر خط
        IQueryable<Order> Orders { get; }

        // ذخیره یک سفارش (جدید یا موجود)
        void SaveOrder(Order order);
    }
}
