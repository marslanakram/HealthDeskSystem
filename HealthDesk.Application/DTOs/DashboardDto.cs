namespace HealthDesk.Application.DTOs;

public class DashboardDto
{
    // ── Stats Cards ───────────────────────────────────────────────
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointmentsToday { get; set; }
    public int TotalAppointmentsThisMonth { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int TotalDepartments { get; set; }
    public decimal RevenueThisMonth { get; set; }

    // ── Recent Data ───────────────────────────────────────────────
    public IEnumerable<AppointmentDto> TodaysAppointments { get; set; }
        = new List<AppointmentDto>();

    public IEnumerable<AppointmentDto> RecentAppointments { get; set; }
        = new List<AppointmentDto>();
}
