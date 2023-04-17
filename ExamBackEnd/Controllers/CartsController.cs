using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamBackEnd.Entities;
using ExamBackEnd.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

namespace ExamBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Carts
        [HttpGet]
        [Authorize("api")]
        public async Task<ActionResult<decimal>> GetCarts(string id)
        {
            
            if (_context.Carts == null)
          {
              return NotFound();
          }
            decimal TotalPrice = 0;
            var userId = User.FindFirst(Claims.Subject)?.Value ?? throw new InvalidOperationException("User Id Not Found");
            var carts = await (from x in _context.Carts
                               join y in _context.CartDetails
                               on x.Id equals y.CartId
                               where x.UserId == userId && x.RestaurantId == id
                               select new CartDetailModel
                               {
                                   Id = x.Id,
                                   FoodItemId = y.FoodItemId,
                                   FoodItemName = y.FoodItem.Name,
                                   RestaurantId = id,
                                   RestaurantName = x.Restaurant.Name,
                                   Qty = y.Qty,
                                   Price = y.Qty * y.FoodItem.Price
                               }).ToListAsync();

            foreach(var item in carts)
            {
                TotalPrice += item.Price * item.Qty;
            }
            return TotalPrice;
        }

        // GET: api/Carts/5
        [HttpGet("{id}")]
        [Authorize("api")]
        public async Task<ActionResult<List<CartDetailModel>>> GetCart(string id)
        {

          if (_context.Carts == null)
          {
              return NotFound();
            }
            var userId = User.FindFirst(Claims.Subject)?.Value ?? throw new InvalidOperationException("User Id Not Found");
            var carts = await (from x in _context.Carts
                         join y in _context.CartDetails
                         on x.Id equals y.CartId
                         where x.UserId == userId && x.RestaurantId == id
                         select new CartDetailModel
                         {
                             Id = x.Id,
                             FoodItemId = y.FoodItemId,
                             FoodItemName = y.FoodItem.Name,
                             RestaurantId = id,
                             RestaurantName = x.Restaurant.Name,
                             Qty = y.Qty,
                             Price = y.Qty * y.FoodItem.Price
                         }).ToListAsync();

            if (carts == null)
            {
                return NotFound();
            }

            return carts;
        }

        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCart(string id, Cart cart)
        {
            if (id != cart.Id)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Carts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost(Name = "AddToCart")]
        [Authorize("api")]
        public async Task<ActionResult<bool>> PostCart(AddToCartModel cart)
        {
          if (_context.Carts == null)
          {
              return Problem("Entity set 'ApplicationDbContext.Carts'  is null.");
          }

            var userId = User.FindFirst(Claims.Subject)?.Value ?? throw new InvalidOperationException("User Id Not Found");
            var carts = await _context.Carts.ToListAsync();
            
            var existing = await _context.Carts
                .Where(Q => Q.RestaurantId == cart.RestaurantId && Q.UserId == userId)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                var searchForDetail = await _context.CartDetails.Where(Q => Q.CartId == existing.Id && Q.FoodItemId == cart.FoodItemId).FirstOrDefaultAsync();
                if(searchForDetail != null)
                {
                    searchForDetail.Qty += cart.Qty;
                }
                else
                {
                    var insertToCartDetails = new CartDetail
                    {
                        Id = Ulid.NewUlid().ToString(),
                        CartId = existing.Id,
                        FoodItemId = cart.FoodItemId,
                        Qty = cart.Qty
                    };

                    _context.CartDetails.Add(insertToCartDetails);
                }
            }
            else
            {

                var insertToCart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    RestaurantId = cart.RestaurantId,
                    Id = Ulid.NewUlid().ToString()
                };
                _context.Carts.Add(insertToCart);

                await _context.SaveChangesAsync();

                var insertToCartDetails = new CartDetail
                {
                    Id = Ulid.NewUlid().ToString(),
                    CartId = insertToCart.Id,
                    FoodItemId = cart.FoodItemId,
                    Qty = cart.Qty
                };

                _context.CartDetails.Add(insertToCartDetails);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // DELETE: api/Carts/5
        [HttpDelete("{id}", Name = "DeleteCart")]
        public async Task<IActionResult> DeleteCart(string id)
        {
            if (_context.Carts == null)
            {
                return NotFound();
            }
            var userId = User.FindFirst(Claims.Subject)?.Value ?? throw new InvalidOperationException("User Id Not Found");
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(string id)
        {
            return (_context.Carts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
