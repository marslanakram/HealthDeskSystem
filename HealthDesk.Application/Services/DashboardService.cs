using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Application.Interfaces;
using HealthDesk.Domain.Enums;

namespace HealthDesk.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DashboardService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    //public async Task<DashboardDto> GetAdminDashboardAsync()
    //{
    //    var today = DateTime.Today;
    //    var monthStart = new DateTime(today.Year, today.Month, 1);

    //    var todaysAppts = await _uow.Appointments.GetTodaysAppointmentsAsync();
    //    var recentAppts = await _uow.Appointments.GetByDateAsync(today);

    //    return new DashboardDto
    //    {
    //        TotalPatients = await _uow.Patients.CountAsync(),
    //        TotalDoctors = await _uow.Doctors.CountAsync(),
    //        TotalDepartments = await _uow.Departments.CountAsync(),
    //        TotalAppointmentsToday = await _uow.Appointments
    //                                        .CountAsync(a => a.AppointmentDate.Date == today),
    //        TotalAppointmentsThisMonth = await _uow.Appointments
    //                                        .CountAsync(a => a.AppointmentDate >= monthStart),
    //        PendingAppointments = await _uow.Appointments
    //                                        .CountAsync(a => a.Status == AppointmentStatus.Pending),
    //        CompletedAppointments = await _uow.Appointments
    //                                        .CountAsync(a => a.Status == AppointmentStatus.Completed),
    //        TodaysAppointments = _mapper.Map<IEnumerable<AppointmentDto>>(todaysAppts),
    //        RecentAppointments = _mapper.Map<IEnumerable<AppointmentDto>>(recentAppts)
    //    };
    //}

    public async Task<DashboardDto> GetAdminDashboardAsync()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var todaysAppts = await _uow.Appointments
            .GetTodaysAppointmentsAsync();

        var recentAppts = await _uow.Appointments
            .FindAsync(a => a.AppointmentDate >= today.AddDays(-7));

        var paidThisMonth = await _uow.Appointments
            .FindAsync(a => a.IsPaid &&
                            a.AppointmentDate >= monthStart);

        var revenue = paidThisMonth
            .Where(a => a.Fee.HasValue)
            .Sum(a => a.Fee!.Value);

        return new DashboardDto
        {
            TotalPatients =
                await _uow.Patients.CountAsync(),
            TotalDoctors =
                await _uow.Doctors.CountAsync(),
            TotalDepartments =
                await _uow.Departments.CountAsync(),
            TotalAppointmentsToday =
                await _uow.Appointments.CountAsync(
                    a => a.AppointmentDate.Date == today),
            TotalAppointmentsThisMonth =
                await _uow.Appointments.CountAsync(
                    a => a.AppointmentDate >= monthStart),
            PendingAppointments =
                await _uow.Appointments.CountAsync(
                    a => a.Status == AppointmentStatus.Pending),
            CompletedAppointments =
                await _uow.Appointments.CountAsync(
                    a => a.Status == AppointmentStatus.Completed),
            RevenueThisMonth = revenue,
            TodaysAppointments =
                _mapper.Map<IEnumerable<AppointmentDto>>(todaysAppts),
            RecentAppointments =
                _mapper.Map<IEnumerable<AppointmentDto>>(
                    recentAppts.OrderByDescending(
                        a => a.AppointmentDate).Take(10))
        };
    }

    public async Task<DashboardDto> GetDoctorDashboardAsync(int doctorId)
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var todaysAppts = await _uow.Appointments
            .FindAsync(a => a.DoctorId == doctorId && a.AppointmentDate.Date == today);

        return new DashboardDto
        {
            TotalAppointmentsToday = todaysAppts.Count(),
            TotalAppointmentsThisMonth = await _uow.Appointments
                                            .CountAsync(a => a.DoctorId == doctorId
                                                          && a.AppointmentDate >= monthStart),
            PendingAppointments = await _uow.Appointments
                                            .CountAsync(a => a.DoctorId == doctorId
                                                          && a.Status == AppointmentStatus.Pending),
            CompletedAppointments = await _uow.Appointments
                                            .CountAsync(a => a.DoctorId == doctorId
                                                          && a.Status == AppointmentStatus.Completed),
            TodaysAppointments = _mapper.Map<IEnumerable<AppointmentDto>>(todaysAppts)
        };
    }

    public async Task<DashboardDto> GetPatientDashboardAsync(int patientId)
    {
        var appointments = await _uow.Appointments.GetByPatientAsync(patientId);

        return new DashboardDto
        {
            TotalAppointmentsThisMonth = appointments
                .Count(a => a.AppointmentDate.Month == DateTime.Today.Month),
            PendingAppointments = appointments
                .Count(a => a.Status == AppointmentStatus.Pending),
            CompletedAppointments = appointments
                .Count(a => a.Status == AppointmentStatus.Completed),
            RecentAppointments = _mapper.Map<IEnumerable<AppointmentDto>>(
                appointments.Take(5))
        };
    }
}
