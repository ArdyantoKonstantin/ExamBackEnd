namespace ExamBackEnd.Models
{
    public class FoodItemDataGridItem
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public decimal Price { get; set; }

        public string? RestaurantName { get; set; }

        public DateTimeOffset CreatedAt { set; get; }
    }
}
