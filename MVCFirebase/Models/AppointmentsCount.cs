using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class AppointmentsCount
    {
        //TodayAppointmentsCount,AllPatientsCount,WaitingAppointmentsCounts,CompletedAppointmentsCounts
        public string AllPatientsCount { get; set; }
        public string TodayAppointmentsCount { get; set; }

        public string WaitingAppointmentsCounts { get; set; }

        public string CompletedAppointmentsCounts { get; set; }
    }
}