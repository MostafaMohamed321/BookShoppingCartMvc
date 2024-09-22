namespace BookShoppingCartMvcUI.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int bookId, int qty);
        Task<int> RemoveItem(int bookId);
        Task<ShoppingCart> GetUserCart();
        Task<int> GetCarItemCount(string UserId = "");
        Task<ShoppingCart> GetCart(string userId);
        Task<bool> DoCheCkOut(CheckoutModel model);

    }
}