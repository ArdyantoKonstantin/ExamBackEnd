namespace ExamBackEnd.Entities
{
    public class Cart
    {
        public string? Id { get; set; }

        public string? UserId { get; set; }

        public User User { get; set; } = null!;

        public string? RestaurantId { get; set; }

        public Restaurant Restaurant { get; set; } = null!;

        public List<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

        public DateTimeOffset CreatedAt { set; get; }
    }
}