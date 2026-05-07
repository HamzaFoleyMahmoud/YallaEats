namespace YallaEat.Models.ViewModels
{
    public class SalesPerformanceViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
