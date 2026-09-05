namespace KasiCash.Models
{
    public class DashboardViewModel
    {
        public string BusinessName { get; set; } = string.Empty;

        public decimal TodaySales { get; set; }

        public int TodayTransactions { get; set; }

        public decimal OutstandingCredit { get; set; }

        public int OutstandingCustomers { get; set; }

        public int TotalStockItems { get; set; }

        public int ProductCount { get; set; }

        public int LowStockItems { get; set; }

        public decimal TotalSales { get; set; }

        public decimal CashSales { get; set; }

        public decimal CreditSales { get; set; }

        public List<Sale> RecentSales { get; set; } = new();

        public List<Product> LowStockProducts { get; set; } = new();
    }
}