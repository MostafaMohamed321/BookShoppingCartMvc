using BookShoppingCartMvcUI.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class AdminOperationsController : Controller
    {
        private readonly IUserOrderRepository _userOrderRepository;
        public AdminOperationsController(IUserOrderRepository userOrderRepository)
        {
            _userOrderRepository = userOrderRepository;
        }
        public async Task<IActionResult> AllOrders()
        {
            var order = await _userOrderRepository.UserOrders(true);
            return View(order);
        }
        public async Task<IActionResult> TogglePaymentStatus(int orderId)
        {
            try
            {
                await _userOrderRepository.TogglePaymentStatus(orderId);
            }
            catch (Exception ex)
            {
            }
            return RedirectToAction("AllOrders");

        }


        public async Task<IActionResult> UpdateOrderStatus(int OrderId)
        {
            var order = await _userOrderRepository.GetOrderById(OrderId);
            if (order == null)
            {
                throw new Exception($"order with Id {OrderId} does not found");
            }
            var orderStatusList = (await _userOrderRepository.GetOrderStatuses()).Select(a =>
            {
                return new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.StatusName,
                    Selected = order.OrderStatusId == a.Id
                };



            });
            var data = new UpdateOrderStatusModel
            {
                OrderId = OrderId,
                OrderStatusId = order.OrderStatusId,
                OrderStatusList = orderStatusList

            };
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusModel data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    data.OrderStatusList = (await _userOrderRepository.GetOrderStatuses()).Select(orderStatus =>
                    {
                        return new SelectListItem
                        {
                            Value = orderStatus.Id.ToString(),
                            Text = orderStatus.StatusName,
                            Selected = orderStatus.Id == data.OrderStatusId
                        };

                    });
                    return View(data);

                }
                await _userOrderRepository.ChangeOrderStatus(data);
                TempData["msg"] = "Update Successfully";
            }
            catch (Exception ex)
            {
                ex = new Exception("someThing went wrong");

            }
            return RedirectToAction(nameof(UpdateOrderStatus), new { orderId = data.OrderId });

        }

        public async Task<IActionResult> Dashboard()
        {
            return View();
        }
    }
}
