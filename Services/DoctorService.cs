using HospitalAppointmentSystem.Models; // Needed to use the Doctor class

namespace HospitalAppointmentSystem.Services; // Namespace for helper/service classes

// Simple service that supplies the doctor data (acts like a mini database)
public class DoctorService
{
    // Returns the full list of doctors; in a real project this would come from a database
    public List<Doctor> GetAllDoctors()
    {
        // Create and return a new list with sample doctors using object initialisers
        return new List<Doctor>
        {
            new Doctor { DoctorId = 1, DoctorName = "Dr. Anil Sharma",  Specialization = "Cardiology",  Experience = 15, ConsultationFee = 800 },
            new Doctor { DoctorId = 2, DoctorName = "Dr. Meena Iyer",   Specialization = "Dermatology", Experience = 8,  ConsultationFee = 600 },
            new Doctor { DoctorId = 3, DoctorName = "Dr. Rahul Verma",  Specialization = "Neurology",   Experience = 12, ConsultationFee = 1000 },
            new Doctor { DoctorId = 4, DoctorName = "Dr. Sneha Reddy",  Specialization = "Pediatrics",  Experience = 6,  ConsultationFee = 500 },
            new Doctor { DoctorId = 5, DoctorName = "Dr. Kiran Patel",  Specialization = "Cardiology",  Experience = 20, ConsultationFee = 1200 },
            new Doctor { DoctorId = 6, DoctorName = "Dr. Farah Khan",   Specialization = "Orthopedics", Experience = 10, ConsultationFee = 700 }
        };
    }
}
