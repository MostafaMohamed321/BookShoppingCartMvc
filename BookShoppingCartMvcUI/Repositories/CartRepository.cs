using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<int> AddItem(int bookId, int qty)
        {
            string userId = GetUserId();

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User not Logged-in");

                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                       UserId = userId

                    };
                    _db.ShoppingCarts.Add(cart);

                }
                _db.SaveChanges();


                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _db.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price
                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();

            }
            catch (Exception ex)
            {
                
            }
            var cartItemCount =await GetCarItemCount(userId);
            return cartItemCount;
        }
        public async Task<int> RemoveItem(int bookId)
        {
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("UserId is not Logged-in");

                var cart = await GetCart(userId);
                if (cart is null)
                    throw new UnauthorizedAccessException("Invalid cart");
                
                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                    throw new UnauthorizedAccessException("no item in Cart");

                else if (cartItem.Quantity == 1)
                  _db.CartDetails.Remove(cartItem);
                
                else
                  cartItem.Quantity = cartItem.Quantity - 1;
                
                _db.SaveChanges();
               

            }
            catch (Exception ex)
            {
            }
            var cartItemCount= await GetCarItemCount(userId);
            return cartItemCount;
        }
        public async Task<ShoppingCart> GetUserCart()
        {
                var UserId = GetUserId();
            if (UserId == null)
                throw new UnauthorizedAccessException("Invalid UserId");
            var ShoppingCart = await _db.ShoppingCarts.Include(a=>a.CartDetails)
                                                      .ThenInclude(a=>a.Book)
                                                      .ThenInclude(a=>a.Stock)
                                                      .Include(a => a.CartDetails)
                                                      .ThenInclude(a => a.Book)
                                                      .ThenInclude(a=>a.Genre)
                                                      .Where(a=>a.UserId == UserId)
                                                      .FirstOrDefaultAsync();
                      return ShoppingCart;


        }
        public async Task<int> GetCarItemCount(string UserId = "")
        {
            if (!string.IsNullOrEmpty(UserId))
            { 
                UserId = GetUserId();

            }
            var data = await (from cart in _db.ShoppingCarts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              select new { cartDetail.Id }).ToListAsync();
            return data.Count;
        }
        public async Task<bool> DoCheCkOut(CheckoutModel model)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("UserId Is not Logged-In ");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new UnauthorizedAccessException("Invalid Cart ");
                var cartDetails = _db.CartDetails.Where(A => A.ShoppingCartId == cart.Id).ToList();
                if (cartDetails.Count == 0)
                    throw new UnauthorizedAccessException("Cart Is Empty");
                var pendingRecord = _db.orderStatuses.FirstOrDefault(x => x.StatusName == "Pending");
                if (pendingRecord is null)
                    throw new UnauthorizedAccessException("order status does not have Pending Status");

               
                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.Now,
                    Name =model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid =false,
                    OrderStatusId = pendingRecord.Id

                };
                   _db.Orders.Add(order);
                   _db.SaveChanges();
                   foreach(var item in cartDetails)
                   {
                         var orderDetails = new OrderDetail
                         {
                             BookId = item.BookId,
                             OrderId =order.Id,
                             Quantity = item.Quantity,
                             UnitPrice =item.UnitPrice,

                         };
                        _db.OrderDetails.Add(orderDetails);
                        var stock =await _db.Stocks.FirstOrDefaultAsync(a=>a.BookId == item.BookId);
                        if (stock == null)
                        {
                            throw new UnauthorizedAccessException($"Only {stock.Quantity} are Available");   
                        }
                        stock.Quantity -=item.Quantity;
                   }
                  // _db.SaveChanges();

                    _db.CartDetails.RemoveRange(cartDetails);
                    _db.SaveChanges();
                    transaction.Commit();
                    return true;
            }       
            catch (Exception ex) 
            { 
                return false;
            }
        }
        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart =await _db.ShoppingCarts.FirstOrDefaultAsync(x=>x.UserId == userId);
           return cart; 
        }
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

    }
}
