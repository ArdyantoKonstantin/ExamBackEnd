namespace ExamBackEnd.Entities
{
    public class Restaurant
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public List<FoodItem> FoodItems { get; set; } = new List<FoodItem>();

        public List<Cart> Carts { get; set; } = new List<Cart>();

        public DateTimeOffset CreatedAt { set; get; }

    }
}