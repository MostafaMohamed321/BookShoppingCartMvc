namespace BookShoppingCartMvcUI.Repositories
{
    public interface IStockRepository
    {
        Task ManageStock(StockDTO stockToManage);
        Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "");
        Task<Stock?> GetStockByBookId(int bookId);
    }
}