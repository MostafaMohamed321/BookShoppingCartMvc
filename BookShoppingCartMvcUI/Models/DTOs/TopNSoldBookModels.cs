namespace BookShoppingCartMvcUI.Models.DTOs
{
    public record TopNSoldBookModel(string BookName,string AuthorName,int TotalUintSold);
    
    public record TopNSoldBookVm(DateTime StartDate ,DateTime EndDate,IEnumerable<TopNSoldBookModel> TopNSoldBooks);





}
