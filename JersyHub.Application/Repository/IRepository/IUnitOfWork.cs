namespace JersyHub.Application.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IOrderDeatilRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }

        IInventoryRepository Inventory { get; }

        void Save();
    }
}
