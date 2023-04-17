namespace ExamBackEnd.Models
{
    public class AddToCartModel
    {
        public string? RestaurantId { get; set; }
        public string? FoodItemId { get; set; }
        public int Qty { get; set; }
    }
}
