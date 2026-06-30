namespace EfficientWallet.Domain
{
    public class ExchangeRate
    {
        public Guid Id { get; set; }
        public DateOnly RatesDate { get; set; }
        public string Currency { get; set; } = default!;
        public decimal Rate { get; set; }
        public string BaseCurrency { get; set; } = "EUR";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
