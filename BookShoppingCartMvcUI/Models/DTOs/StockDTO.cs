using System.ComponentModel.DataAnnotations;

namespace BookShoppingCartMvcUI.Models.DTOs
{
    public class StockDTO
    {
        public int BookId { get; set; }
        [Range(0, int.MaxValue,ErrorMessage ="Quantity must be a non_Negative Value")]
        public int Quantity { get; set; }
    }
}
