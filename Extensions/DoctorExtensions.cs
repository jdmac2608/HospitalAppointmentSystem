using HospitalAppointmentSystem.Models; // Needed so we can use the Doctor class

namespace HospitalAppointmentSystem.Extensions; // Namespace for all extension methods

// Extension methods MUST be written inside a static, non-generic, top-level class
public static class DoctorExtensions
{
    // Fixed service charge (Rs. 100) added to every consultation; const = value never changes
    private const decimal ServiceCharge = 100m;

    // 'this Doctor doctor' makes this an EXTENSION METHOD, so it can be called as doctor.CalculateTotalCharge()
    public static decimal CalculateTotalCharge(this Doctor doctor)
    {
        // Total charge = doctor's consultation fee + Rs. 100 service charge
        return doctor.ConsultationFee + ServiceCharge;
    }
}
