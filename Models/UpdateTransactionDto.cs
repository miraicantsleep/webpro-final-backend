namespace pweb_eas.Models
{
    public class UpdateTransactionDto
    {
        public string? Name { get; set; }
        public required string Type { get; set; }
        public required decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}