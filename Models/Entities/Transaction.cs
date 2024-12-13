namespace pweb_eas.Models.Entities
{
    public class Transaction
    {
        public required Guid Id { get; set; }
        public required Guid UserId { get; set; }
        public User? User { get; set; }
        public string? Name { get; set; }
        public required string Type { get; set; }
        public required decimal Amount { get; set; }
        public required string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
