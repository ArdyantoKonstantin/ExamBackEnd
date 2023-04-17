using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamBackEnd.Entities;
using ExamBackEnd.Models;

namespace ExamBackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FoodItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FoodItems
        [HttpGet]
        public async Task<ActionResult<List<FoodItemDataGridItem>>> GetFoodItems()
        {
          if (_context.FoodItems == null)
          {
              return NotFound();
          }
            return await _context.FoodItems.AsNoTracking().Select(Q => new FoodItemDataGridItem
            {
                Id = Q.Id,
                Name = Q.Name,
                Price = Q.Price,
                CreatedAt = Q.CreatedAt,
                RestaurantName = Q.Restaurant.Name
            }).ToListAsync();
        }

        // GET: api/FoodItems/5
        [HttpGet("{id}", Name = "FindFoodByRestaurantID")]
        public async Task<ActionResult<List<FoodItemDataGridItem>>> GetFoodItem(string id)
        {
          if (_context.FoodItems == null)
          {
              return NotFound();
          }
            var foodItem = await _context.FoodItems
                .AsNoTracking()
                .Where(Q => Q.RestaurantId == id)
                .Select(Q => new FoodItemDataGridItem
                {
                    Id = Q.Id,
                    Name = Q.Name,
                    Price = Q.Price,
                    RestaurantName = Q.Restaurant.Name,
                    CreatedAt = Q.CreatedAt
                })
                .ToListAsync();

            if (foodItem == null)
            {
                return NotFound();
            }

            return foodItem;
        }

        // PUT: api/FoodItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFoodItem(string id, FoodItem foodItem)
        {
            if (id != foodItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(foodItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodItemExists(id))
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

        // POST: api/FoodItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost(Name = "CreateFood")]
        public async Task<ActionResult<FoodItem>> PostFoodItem(FoodItemCreateModel foodItem)
        {
          if (_context.FoodItems == null)
          {
              return Problem("Entity set 'ApplicationDbContext.FoodItems'  is null.");
          }
            var insert = new FoodItem
            {
                Id = Ulid.NewUlid().ToString(),
                Name = foodItem.Name,
                Price = foodItem.Price,
                RestaurantId = foodItem.RestaurantId,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            _context.FoodItems.Add(insert);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (FoodItemExists(insert.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return insert;
        }

        // DELETE: api/FoodItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodItem(string id)
        {
            if (_context.FoodItems == null)
            {
                return NotFound();
            }
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            _context.FoodItems.Remove(foodItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FoodItemExists(string id)
        {
            return (_context.FoodItems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
