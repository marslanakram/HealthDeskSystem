using AutoMapper;
using HealthDesk.Application.DTOs;
using HealthDesk.Domain.Entities;

namespace HealthDesk.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Patient Mappings ──────────────────────────────────────
        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.IsActive,
                opt => opt.MapFrom(src => src.User != null && src.User.IsActive));

        CreateMap<CreatePatientDto, Patient>();
        CreateMap<UpdatePatientDto, Patient>();

        // ── Doctor Mappings ───────────────────────────────────────
        CreateMap<Doctor, DoctorDto>()
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty));

        CreateMap<CreateDoctorDto, Doctor>();
        CreateMap<UpdateDoctorDto, Doctor>();

        // ── Appointment Mappings ──────────────────────────────────
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.Patient != null ? src.Patient.FullName : string.Empty))
            .ForMember(dest => dest.DoctorName,
                opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.FullName : string.Empty))
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Doctor != null && src.Doctor.Department != null
                    ? src.Doctor.Department.Name : string.Empty))
            .ForMember(dest => dest.HasPrescription,
                opt => opt.MapFrom(src => src.Prescription != null));

        CreateMap<CreateAppointmentDto, Appointment>();
        CreateMap<UpdateAppointmentDto, Appointment>();

        // ── Prescription Mappings ─────────────────────────────────
        CreateMap<Prescription, PrescriptionDto>()
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.Patient != null ? src.Patient.FullName : string.Empty))
            .ForMember(dest => dest.DoctorName,
                opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.FullName : string.Empty));

        CreateMap<CreatePrescriptionDto, Prescription>();

        // ── Department Mappings ───────────────────────────────────
        CreateMap<Department, DepartmentDto>()
            .ForMember(dest => dest.DoctorCount,
                opt => opt.MapFrom(src => src.Doctors != null ? src.Doctors.Count : 0));

        CreateMap<CreateDepartmentDto, Department>();
        CreateMap<UpdateDepartmentDto, Department>();
        // ── AuditLog Mappings ─────────────────────────────────────────
        //CreateMap<AuditLog, AuditLogDto>();
        CreateMap<AuditLog, AuditLogDto>()
       .ForMember(dest => dest.Timestamp,
       opt => opt.MapFrom(src =>
           TimeZoneInfo.ConvertTimeFromUtc(
               src.Timestamp,
               TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")
           )
       ));
    }
}
