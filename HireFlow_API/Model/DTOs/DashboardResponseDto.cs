namespace HireFlow_API.Model.DTOs
{
    public class DashboardResponseDto
    {
        public DashboardStatsDto Stats { get; set; }
        public List<RecentApplicationDto> RecentApplications { get; set; }
        public List<UpcomingInterviewDto> UpcomingInterviews { get; set; }
    }

    public class DashboardStatsDto
    {
        public int OpenPositions { get; set; }
        public int Applications { get; set; }
        public int Interviews { get; set; }
        public int NewHires { get; set; }
    }

    public class RecentApplicationDto
    {
        public string Candidate { get; set; }
        public string Position { get; set; }
        public DateTime AppliedOn { get; set; }
        public string Status { get; set; }
        public int Score { get; set; }
    }

    public class UpcomingInterviewDto
    {
        public DateTime Date { get; set; }
        public string TimeRange { get; set; }
        public string Position { get; set; }
        public string Candidate { get; set; }
    }

}
