using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories
{
    public class StockRepository: IStockRepository
    {
        private readonly ApplicationDbContext _db;
          
        public StockRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<Stock?> GetStockByBookId(int bookId)
                    =>await _db.Stocks.FirstOrDefaultAsync(s => s.BookId == bookId);

        public async Task ManageStock(StockDTO stockToManage)
        {
            var existingStock = await GetStockByBookId(stockToManage.BookId);
            if (existingStock is null) 
            { 
                var stock =new Stock 
                { 
                    BookId =stockToManage.BookId,
                    Quantity = stockToManage.Quantity
                    
                };
                _db.Stocks.Add(stock);
            }
            else
            {
                existingStock.Quantity = stockToManage.Quantity;
            }
            await _db.SaveChangesAsync();

        }
        public async Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "")
        {
            var stocks = await (from book in _db.Books
                                join stock in _db.Stocks
                                on book.Id equals stock.BookId
                                into book_stock
                                from bookstock in book_stock.DefaultIfEmpty()
                                where string.IsNullOrEmpty(sTerm) || book.BookName.ToLower().Contains(sTerm.ToLower())
                                select new StockDisplayModel
                                {
                                    BookId = book.Id,
                                    BookName =book.BookName,
                                    Quantity = bookstock == null ? 0 : bookstock.Quantity
                                }).ToListAsync();
            return stocks;
        }
        

    }
}
