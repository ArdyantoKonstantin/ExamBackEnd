using ExamBackEnd.Entities;

namespace ExamBackEnd.Models
{
    public class AddToCartDetailModel
    {
        public string? CartId { get; set; }

        public string? FoodItemId { get; set; } = null!;

        public int Qty { get; set; }
    }
}
