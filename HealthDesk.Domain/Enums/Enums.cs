namespace HealthDesk.Domain.Enums;

public enum AppointmentStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3
}

public enum BloodGroup
{
    APositive,
    ANegative,
    BPositive,
    BNegative,
    OPositive,
    ONegative,
    ABPositive,
    ABNegative
}

public enum ShiftType
{
    Morning = 1,
    Evening = 2,
    Night = 3
}
