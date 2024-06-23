using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Test3.Data;
using Test3.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Test3.Controllers
{
    public class OrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDurableClient _durableClient;

        public OrderItemsController(ApplicationDbContext context, IDurableClient durableClient)
        {
            _context = context;
            _durableClient = durableClient;
        }

        // GET: OrderItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OrderItems.Include(o => o.Order).Include(o => o.Product);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItems = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderItemID == id);
            if (orderItems == null)
            {
                return NotFound();
            }

            return View(orderItems);
        }

        // GET: OrderItems/Create
        public IActionResult Create()
        {
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderId", "OrderId");
            ViewData["ProductID"] = new SelectList(_context.Product, "ProductID", "ProductID");
            return View();
        }

        // POST: OrderItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderItemID,OrderID,ProductID,Price")] OrderItems orderItems)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderItems);
                await _context.SaveChangesAsync();

                var order = await _context.Order.FindAsync(orderItems.OrderID);
                var product = await _context.Product.FindAsync(orderItems.ProductID);

                var orderDetails = new OrderDetails
                {
                    OrderId = orderItems.OrderID,
                    ProductId = orderItems.ProductID,
                    Quantity = 1, // Assuming a quantity of 1, adjust as needed
                    Price = (decimal)orderItems.Price,
                    UserName = order?.userName
                };

                string instanceId = await _durableClient.StartNewAsync("OrderProcessingOrchestrator", orderDetails);

                return RedirectToAction(nameof(OrderStatus), new { instanceId });
            }
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderId", "OrderId", orderItems.OrderID);
            ViewData["ProductID"] = new SelectList(_context.Product, "ProductID", "ProductID", orderItems.ProductID);
            return View(orderItems);
        }

        // GET: OrderItems/OrderStatus
        public async Task<IActionResult> OrderStatus(string instanceId)
        {
            var status = await _durableClient.GetStatusAsync(instanceId);
            return View(status);
        }

        // GET: OrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItems = await _context.OrderItems.FindAsync(id);
            if (orderItems == null)
            {
                return NotFound();
            }
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderId", "OrderId", orderItems.OrderID);
            ViewData["ProductID"] = new SelectList(_context.Product, "ProductID", "ProductID", orderItems.ProductID);
            return View(orderItems);
        }

        // POST: OrderItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderItemID,OrderID,ProductID,Price")] OrderItems orderItems)
        {
            if (id != orderItems.OrderItemID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderItems);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemsExists(orderItems.OrderItemID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderID"] = new SelectList(_context.Order, "OrderId", "OrderId", orderItems.OrderID);
            ViewData["ProductID"] = new SelectList(_context.Product, "ProductID", "ProductID", orderItems.ProductID);
            return View(orderItems);
        }

        // GET: OrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderItems = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderItemID == id);
            if (orderItems == null)
            {
                return NotFound();
            }

            return View(orderItems);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItems = await _context.OrderItems.FindAsync(id);
            if (orderItems != null)
            {
                _context.OrderItems.Remove(orderItems);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemsExists(int id)
        {
            return _context.OrderItems.Any(e => e.OrderItemID == id);
        }
    }
}
