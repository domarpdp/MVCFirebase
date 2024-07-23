using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCFirebase.Models
{
    public class ClinicAPI
    {
        [Key]
        public int Id { get; set; }


        public string documentId { get; set; }
        public string clinicname { get; set; }


        public string clinicaddress { get; set; }

        public string clinicsector { get; set; }

        public string clinicstate { get; set; }


        public string cliniccity { get; set; }

        public string clinicmobilenumber { get; set; }



        public string mobilenumber { get; set; }


        public string name { get; set; }


        public string email { get; set; }



        public string idproof { get; set; }


        public string clinicemail { get; set; }


        public string clinicwebsite { get; set; }



        public string clinicstreet { get; set; }
        public string clinicpincode { get; set; }

        public string clinicadvertisement { get; set; }

        public string selectidproofimage { get; set; }

        public string userId { get; set; }

        public string selected_plan { get; set; }

        public string logo { get; set; }
        public DateTime? created_on { get; set; }

        public DateTime? subscription_start_date { get; set; }

        public DateTime? subscription_end_date { get; set; }

        public string clinic_code { get; set; }

        public string registerd_by_number { get; set; }

        public bool? free_trail_available { get; set; }

        public bool? free_sms_available { get; set; }

        public bool? clinic_info_completed { get; set; }

        public bool? is_using_free_trial { get; set; }

        public DateTime? free_trial_taken_date { get; set; }

    }
}