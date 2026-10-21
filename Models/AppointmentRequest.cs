namespace HospitalAppointmentSystem.Models; // Same namespace as the other models

// AppointmentRequest: holds the data posted by the appointment form on the Details page
public class AppointmentRequest
{
    public int DoctorId { get; set; } // Filled from the HIDDEN FIELD of the form

    public DateTime AppointmentDate { get; set; } // Date chosen by the patient in the form

    public string Reason { get; set; } = string.Empty; // Reason for the visit typed by the patient
}
