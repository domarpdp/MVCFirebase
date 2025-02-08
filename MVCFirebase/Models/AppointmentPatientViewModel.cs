using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class AppointmentPatientViewModel
    {
        public List<Appointment> Appointments { get; set; }
        public List<Patient> Patients { get; set; }
    }
}