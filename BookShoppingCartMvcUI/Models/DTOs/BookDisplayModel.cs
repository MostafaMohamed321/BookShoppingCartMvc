namespace BookShoppingCartMvcUI.Models.DTOs
{

    // this class => mean a View Model 
    public class BookDisplayModel
    {
        public IEnumerable<Book> Books { get; set; } 

        public IEnumerable<Genre> Genres { get ; set; }

        public string STerm { get; set; } = "";

        public int GenreId { get; set; } = 0;
    }
}
