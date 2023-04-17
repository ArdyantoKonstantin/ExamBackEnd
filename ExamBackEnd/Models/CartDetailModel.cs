namespace ExamBackEnd.Models
{
    public class CartDetailModel
    {
        public string? Id { get; set; }
        public string? FoodItemId { get; set; } = null!;
        public string? FoodItemName { get; set; } = null!;
        public string? RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }
}
