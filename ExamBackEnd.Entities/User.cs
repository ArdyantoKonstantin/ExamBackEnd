namespace ExamBackEnd.Entities
{
    public class User
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Password { get; set; }

        public string? Email { get; set; }

        public List<Cart> Carts { get; set; } = new List<Cart>();

        public DateTimeOffset CreatedAt { set; get; }
    }
}