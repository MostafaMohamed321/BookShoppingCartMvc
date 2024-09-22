using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMvcUI.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;
        public CartController(ICartRepository cartRepo) 
        {
            _cartRepo = cartRepo;
        }
        public async Task<IActionResult> AddItem( int bookId , int qty =1,int redirect =0) 
        {
            var cartCount =await _cartRepo.AddItem(bookId,qty);
            if (redirect == 0)
             return Ok(cartCount);
            
            return RedirectToAction("GetUserCart");
        }       
        public async Task<IActionResult> RemoveItem(int bookId) 
        { 
            var cartCount = await _cartRepo.RemoveItem(bookId);
            return RedirectToAction("GetUserCart");
             
        }
        public async Task<IActionResult> GetUserCart(int bookId) 
        {
            var Cart = await _cartRepo.GetUserCart();
            return View(Cart);
        }
       
        public async Task<IActionResult> GetTotalItemInCart(int bookId)
        { 
            int cartItem = await _cartRepo.GetCarItemCount();
            return Ok(cartItem);
        
        }
        public IActionResult CheckOut()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CheckOut(CheckoutModel model)
        {
            if (!ModelState.IsValid) 
                return View(model);
            bool check = await _cartRepo.DoCheCkOut(model);
            if (!check)
                return RedirectToAction(nameof(OrderFailure));
           
            return RedirectToAction(nameof(OrderSuccess));
        }
        public IActionResult OrderSuccess()
        {
            return View();
        }
        public IActionResult OrderFailure()
        {
            return View();
        }








    }
}
