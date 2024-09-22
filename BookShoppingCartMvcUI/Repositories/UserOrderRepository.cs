using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BookShoppingCartMvcUI.Repositories
{
    public class UserOrderRepository :IUserOrderRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        public UserOrderRepository(ApplicationDbContext db, IHttpContextAccessor contextAccessor , UserManager<IdentityUser> userManager)
        { 
            _db = db;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        public async Task ChangeOrderStatus(UpdateOrderStatusModel model)
        {
            var order = await _db.Orders.FindAsync(model.OrderId);
            if (order == null)
                throw new InvalidOperationException($"order with Id {model.OrderId}");

            order.OrderStatusId = model.OrderStatusId;

            await _db.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderById(int orderId)
        {
            return await _db.Orders.FindAsync(orderId);
        }

        public async Task<IEnumerable<OrderStatus>> GetOrderStatuses()
        {
           return await _db.orderStatuses.ToListAsync();
           
        }

        public async Task TogglePaymentStatus(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null)
                            throw new ArgumentException($"order With Id :{orderId} does not found");

            order.IsPaid =!order.IsPaid;
            await _db.SaveChangesAsync();
            
        }

        public async Task<IEnumerable<Order>> UserOrders(bool getAll = false)
        {
            var orders = _db.Orders.Include(x => x.OrderStatus)
                                         .Include(x => x.OrderDetail)
                                         .ThenInclude(x => x.Book)
                                         .ThenInclude(x => x.Genre).AsQueryable();
            if (!getAll) 
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("UserId not Logged-In ");
                orders.Where(a=>a.UserId == userId);
                return await orders.ToListAsync();
            }
            return await orders.ToListAsync();
        }

        private  string GetUserId()
        {
            var principal = _contextAccessor.HttpContext.User;
            string userId =_userManager.GetUserId(principal);
            return userId;



        }
    }
}
