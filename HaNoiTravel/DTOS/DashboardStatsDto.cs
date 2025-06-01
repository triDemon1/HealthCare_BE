namespace HaNoiTravel.DTOS
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int LastMonthUsers { get; set; }
        public int TodayBookings { get; set; }
        public int YesterdayBookings { get; set; } 
        public decimal MonthlyRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; } 

        public string TopServiceName { get; set; }
        public int TopServiceBookings { get; set; }
    }
}
