namespace SupportManagement.Application.DTOs;

public record DashboardSummaryDto(
    int TotalTickets,
    int OpenTickets,
    int ClosedTickets,
    int ResolvedTickets,
    int OverdueTickets,
    int SlaBreachedTickets,
    int ReopenedTickets,
    double AverageFirstResponseMinutes,
    double AverageResolutionMinutes,
    List<CategoryTrendDto> CategoryTrends,
    List<AgentPerformanceDto> AgentPerformance
);

public record CategoryTrendDto(string CategoryName, int Count);
public record AgentPerformanceDto(string AgentName, int AssignedCount, int ResolvedCount);
public record SlaComplianceDto(int TotalTickets, int CompliantTickets, double ComplianceRate);
