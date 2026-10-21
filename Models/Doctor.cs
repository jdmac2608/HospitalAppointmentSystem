namespace HospitalAppointmentSystem.Models; // Namespace that groups all model classes of the project

// Doctor model: represents one doctor record shown on the website
public class Doctor
{
    public int DoctorId { get; set; } // Unique id of the doctor (used in query string and hidden field)

    public string DoctorName { get; set; } = string.Empty; // Full name of the doctor (initialised to empty to avoid null)

    public string Specialization { get; set; } = string.Empty; // Medical field, e.g. Cardiology

    public int Experience { get; set; } // Years of experience

    public decimal ConsultationFee { get; set; } // Base consultation fee in rupees (without service charge)
}
