

using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace BookShoppingCartMvcUI.Repositories
{
    public class HomeRepository: IHomeRepository
    {
        private readonly ApplicationDbContext _db;
        public HomeRepository(ApplicationDbContext db)

        { 
            _db = db;

        }
        public async Task<IEnumerable<Genre>> Genres()
        {
            return await _db.Genres.ToListAsync();

        }
        public async Task<IEnumerable<Book>> GetBooks(string sterm= "",int genreId = 0)
        {
            sterm = sterm.ToLower();
            var books = await (from book in _db.Books
                         join genre in _db.Genres
                         on book.GenreId equals genre.Id
                         join stock in _db.Stocks
                         on book.Id equals stock.BookId
                         into book_stock
                         from bookstock in book_stock.DefaultIfEmpty()
                         where string.IsNullOrWhiteSpace(sterm) ||(book !=null && book.BookName.ToLower().StartsWith(sterm))

                         select new Book
                         {
                             Id = book.Id,
                             Image = book.Image,
                             AuthorName = book.AuthorName,
                             BookName = book.BookName,
                             GenreId = book.GenreId,
                             Price = book.Price,
                             GenreName = genre.GenreName,
                             Quantity = bookstock.Quantity ==null ?0: bookstock.Quantity
                         }).ToListAsync();
                       if (genreId > 0)
                       {

                            books = books.Where(a=>a.GenreId==genreId).ToList();
                       }
          
            return books;




        }
    }
}
