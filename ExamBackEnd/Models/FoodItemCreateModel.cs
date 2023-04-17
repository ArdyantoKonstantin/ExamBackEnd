namespace ExamBackEnd.Models
{
    public class FoodItemCreateModel
    {
        public string? Name { get; set; }

        public decimal Price { get; set; }

        public string? RestaurantId { get; set; }
    }
}
