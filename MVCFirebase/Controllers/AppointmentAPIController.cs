using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using MVCFirebase.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace MVCFirebase.Controllers
{
    public class AppointmentAPIController : ApiController
    {
        string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/AppointmentAPI/GetAppointments")]
        //public GenericAPIResult GetAppointments(string cliniccode,string statussearch int pagenumber, int pagesize, DateTime date)
        public GenericAPIResult GetAppointments(string cliniccode, int pagenumber, int pagesize, DateTime date, DateTime updatedat)
        {
            //if (statussearch == null)
            //{
            //    statussearch = "";
            //}

            string strDate = date.ToString("dd-MMM-yyyy");
            string strUpdatedAt = updatedat.ToString("dd-MMM-yyyy");

            List<AppointmentAPI> patients = new List<AppointmentAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetAppointmentsAll", conn);
                    sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
                    //sqlComm.Parameters.Add(new SqlParameter("@StatusSearch", statussearch));
                    sqlComm.Parameters.Add(new SqlParameter("@Date", strDate));
                    sqlComm.Parameters.Add(new SqlParameter("@UpdatedAt", strUpdatedAt));
                    sqlComm.Parameters.Add(new SqlParameter("@PageNumber", pagenumber));
                    sqlComm.Parameters.Add(new SqlParameter("@PageSize", pagesize));

                    sqlComm.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader sdr = sqlComm.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            dynamic row = new ExpandoObject();
                            var dictionary = (IDictionary<string, object>)row;

                            for (int i = 0; i < sdr.FieldCount; i++)
                            {
                                if (sdr.GetName(i) == "bill_sms" || sdr.GetName(i) == "reminder_sms" || sdr.GetName(i) == "isCreated" || sdr.GetName(i) == "isSynced" || sdr.GetName(i) == "request_by_patient")
                                {
                                    // Convert 1 or 0 to boolean true or false
                                    bool fieldValue = Convert.ToInt32(sdr.GetValue(i)) == 1;
                                    dictionary.Add(sdr.GetName(i), fieldValue);
                                }
                                else
                                {
                                    dictionary.Add(sdr.GetName(i), sdr.GetValue(i));
                                }
                            }

                            dynamicDt.Add(row);
                        }

                    }
                    conn.Close();
                }
                result.message = "Appointment List fetched Successfully";
                result.statusCode = "200";
                result.error = "false";
                result.data = dynamicDt;
            }
            catch (Exception ex)
            {
                result.message = ex.Message;
                result.statusCode = "201";
                result.error = "true";
                result.data = dynamicDt;
            }

            return result;

        }

        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/AppointmentAPI/GetAppointmentsPatientWise")]
        //public GenericAPIResult GetAppointments(string cliniccode,string statussearch int pagenumber, int pagesize, DateTime date)
        public GenericAPIResult GetAppointmentsPatientWise(string cliniccode, string patientid, DateTime updatedat)
        {

            string strUpdatedAt = updatedat.ToString("dd-MMM-yyyy");
            List<AppointmentAPI> patients = new List<AppointmentAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    SqlCommand sqlComm = new SqlCommand("usp_GetAppointmentsPatientWise", conn);
                    sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
                    sqlComm.Parameters.Add(new SqlParameter("@PatientId", patientid));
                    sqlComm.Parameters.Add(new SqlParameter("@UpdatedAt", strUpdatedAt));


                    sqlComm.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader sdr = sqlComm.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            dynamic row = new ExpandoObject();
                            var dictionary = (IDictionary<string, object>)row;

                            for (int i = 0; i < sdr.FieldCount; i++)
                            {
                                if (sdr.GetName(i) == "bill_sms" || sdr.GetName(i) == "reminder_sms" || sdr.GetName(i) == "isCreated" || sdr.GetName(i) == "isSynced" || sdr.GetName(i) == "request_by_patient")
                                {
                                    // Convert 1 or 0 to boolean true or false
                                    bool fieldValue = Convert.ToInt32(sdr.GetValue(i)) == 1;
                                    dictionary.Add(sdr.GetName(i), fieldValue);
                                }
                                else
                                {
                                    dictionary.Add(sdr.GetName(i), sdr.GetValue(i));
                                }
                            }

                            dynamicDt.Add(row);
                        }

                    }
                    conn.Close();
                }
                result.message = "Appointment List for Patient " + patientid + " fetched Successfully";
                result.statusCode = "200";
                result.error = "false";
                result.data = dynamicDt;
            }
            catch (Exception ex)
            {
                result.message = ex.Message;
                result.statusCode = "201";
                result.error = "true";
                result.data = dynamicDt;
            }

            return result;

        }


        [JwtAuthorize(Roles = "user")]
        [HttpPost]
        [Route("api/AppointmentAPI/CreateAppointment")]
        public async Task<GenericAPIResult> CreateAppointment([FromBody] AppointmentAPI Obj)
        {
            GenericAPIResult result = new GenericAPIResult();
            var dynamicDt = new List<dynamic>();
            string errorcode = "";
            string statuscode = "";


            string msg = "";

            int maxId = 0;

            if (Obj is null)
            {
                msg = "Appointment Data is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (Obj.patient_id == "")
            {
                msg = "patient_id is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.patient == "")
            {
                msg = "patient is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicCode == "" || Obj.clinicCode is null)
            {

                msg = "clinicCode is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinic_id == "" || Obj.clinic_id is null)
            {
                msg = "clinic_id is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.status == "" || Obj.status is null)
            {
                msg = "status is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.severity == "" || Obj.severity is null)
            {
                msg = "severity is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.updatedAt.ToString() == "01-01-0001 00:00:00")
            {
                msg = "updatedAt is invalid";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.raisedDate.ToString() == "01-01-0001 00:00:00")
            {
                msg = "raisedDate is invalid";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicCode != "")
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    DataTable dtCC = new DataTable();
                    SqlCommand sqlCommCC = new SqlCommand("Select * from clinics where clinic_code = '" + Obj.clinicCode + "'", conn);
                    sqlCommCC.CommandType = CommandType.Text;
                    conn.Open();

                    dtCC.Load(sqlCommCC.ExecuteReader());
                    conn.Close();
                    if (dtCC.Rows.Count == 0)
                    {
                        msg = "Clinic Code " + Obj.clinicCode + " does not exists.";
                        statuscode = "201";
                        errorcode = "true";
                    }
                    else
                    {
                        DataTable dtPatient = new DataTable();
                        SqlCommand sqlCommPatient = new SqlCommand("Select * from patients where patient_id = '"+ Obj.patient_id + "' and clinicCode = '" + Obj.clinicCode + "'", conn);
                        sqlCommPatient.CommandType = CommandType.Text;
                        conn.Open();

                        dtPatient.Load(sqlCommPatient.ExecuteReader());
                        conn.Close();
                        if (dtPatient.Rows.Count == 0)
                        {
                            msg = "Patient Id " + Obj.patient_id + " does not exists.";
                            statuscode = "201";
                            errorcode = "true";
                        }
                        else
                        {
                            string documentidOfPatient = dtPatient.Rows[0]["id"].ToString();

                            DataTable dtappexist = new DataTable();
                            SqlCommand sqlCommappexist = new SqlCommand("Select * from appointments where clinicCode = '" + Obj.clinicCode + "' and patient_id = '" + Obj.patient_id + "' and CAST(raisedDate AS DATE) = '" + Obj.raisedDate.ToString("dd-MMM-yyyy") + "' and status = 'Waiting'", conn);

                            sqlCommappexist.CommandType = CommandType.Text;
                            conn.Open();

                            dtappexist.Load(sqlCommappexist.ExecuteReader());
                            conn.Close();
                            if (dtappexist.Rows.Count > 0)
                            {
                                msg = "Appointment for Patient " + Obj.patient_id + " in waiting already exists for today.";
                                statuscode = "201";
                                errorcode = "true";
                            }
                            else
                            {
                                try
                                {
                                    int countTokenNo = 0;
                                    SqlCommand cmdTokenNoExist = new SqlCommand("Select Count (*) from appointments where clinicCode = '" + Obj.clinicCode + "' and CAST(raisedDate AS DATE) = '" + Obj.raisedDate.ToString("dd-MMM-yyyy") + "' and Convert(int,token) = " + Convert.ToInt32(Obj.token), conn);

                                    conn.Open();

                                    cmdTokenNoExist.CommandType = CommandType.Text;
                                    countTokenNo = (int)cmdTokenNoExist.ExecuteScalar();

                                    conn.Close();

                                    if (countTokenNo > 0)
                                    {
                                        msg = "Token No " + Obj.token + " already assigned.";
                                        statuscode = "201";
                                        errorcode = "true";
                                    }
                                    else
                                    {
                                        int returnedTokenNo = 0;

                                        SqlCommand sqlCommTokenNo = new SqlCommand("usp_GetAppointmentNextTokenNo", conn);
                                        sqlCommTokenNo.Parameters.Add(new SqlParameter("@ClinicCode", Obj.clinicCode));
                                        sqlCommTokenNo.Parameters.Add(new SqlParameter("@Date", Obj.raisedDate));


                                        sqlCommTokenNo.CommandType = CommandType.StoredProcedure;
                                        conn.Open();

                                        using (SqlDataReader sdr = sqlCommTokenNo.ExecuteReader())
                                        {
                                            while (sdr.Read())
                                            {
                                                returnedTokenNo = sdr.GetInt32(0);
                                            }

                                        }
                                        conn.Close();
                                        if (Convert.ToInt32(Obj.token) < returnedTokenNo)
                                        {
                                            //tokenToBeSaved = returnedTokenNo;
                                            msg = "Token No can not be less than " + returnedTokenNo + ".";
                                            statuscode = "201";
                                            errorcode = "true";
                                        }
                                        else
                                        {

                                            DataTable dt = new DataTable();
                                            SqlCommand sqlComm = new SqlCommand("Select Max(isnull(Id,'0')) as Id from appointments", conn);
                                            sqlComm.CommandType = CommandType.Text;
                                            conn.Open();

                                            dt.Load(sqlComm.ExecuteReader());
                                            conn.Close();

                                            maxId = Convert.ToInt32(dt.Rows[0]["Id"].ToString()) + 1;

                                            using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                            "INSERT INTO appointments (" +
                                            "Id, documentId, days, fee, status, statusCashier, statusChemist, severity, patient_id, clinic_id, clinicCode, patient, bill_sms, reminder_sms, timeStamp, updatedAt, completionDate, completiondateCashier, completiondateChemist, raisedDate, token, referTo, medicineFee, medicineCost, modeofpayment, modeofpaymentChemist, isCreated, isSynced, createdBy, receptionist, chemist, cashier, doctor, request_by_patient" +
                                            ") VALUES (" +
                                            "@Id, @DocumentId, @Days, @Fee, @Status, @StatusCashier, @StatusChemist, @Severity, @PatientId, @ClinicId, @ClinicCode, @Patient, @BillSms, @ReminderSms, @TimeStamp, @UpdatedAt, @CompletionDate, @CompletiondateCashier, @CompletiondateChemist, @RaisedDate, @Token, @ReferTo, @MedicineFee, @MedicineCost, @ModeOfPayment, @ModeOfPaymentChemist, @IsCreated, @IsSynced, @CreatedBy, @Receptionist, @Chemist, @Cashier, @Doctor, @RequestByPatient)", conn))
                                            {
                                                sqlCommPatientInsert.CommandType = CommandType.Text;

                                                sqlCommPatientInsert.Parameters.AddWithValue("@Id", maxId);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@DocumentId", Obj.documentId ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Days", Obj.days ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Fee", Obj.fee ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Status", Obj.status ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@StatusCashier", Obj.statusCashier ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@StatusChemist", Obj.statusChemist ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Severity", Obj.severity ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@PatientId", Obj.patient_id ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@ClinicId", Obj.clinic_id ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@ClinicCode", Obj.clinicCode ?? (object)DBNull.Value);
                                                //sqlCommPatientInsert.Parameters.AddWithValue("@Patient", Obj.patient ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Patient", documentidOfPatient);
                                                
                                                sqlCommPatientInsert.Parameters.AddWithValue("@BillSms", Obj.bill_sms ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@ReminderSms", Obj.reminder_sms ?? (object)DBNull.Value);
                                                if (Obj.timeStamp is null || Obj.timeStamp.ToString() == "")
                                                {
                                                    sqlCommPatientInsert.Parameters.AddWithValue("@TimeStamp", DateTime.Now);
                                                }
                                                else
                                                {
                                                    sqlCommPatientInsert.Parameters.AddWithValue("@TimeStamp", Obj.timeStamp);
                                                }
                                                if (Obj.updatedAt is null || Obj.updatedAt.ToString() == "")
                                                {
                                                    sqlCommPatientInsert.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                                                }
                                                else
                                                {
                                                    sqlCommPatientInsert.Parameters.AddWithValue("@UpdatedAt", Obj.updatedAt);
                                                }


                                                sqlCommPatientInsert.Parameters.AddWithValue("@CompletionDate", Obj.completionDate ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@CompletiondateCashier", Obj.completiondateCashier ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@CompletiondateChemist", Obj.completiondateChemist ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@RaisedDate", Obj.raisedDate);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Token", Obj.token ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@ReferTo", Obj.referTo ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@MedicineFee", Obj.medicineFee ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@MedicineCost", Obj.medicineCost ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@ModeOfPayment", Obj.modeofpayment ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@ModeOfPaymentChemist", Obj.modeofpaymentChemist ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@IsCreated", Obj.isCreated ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@IsSynced", Obj.isSynced ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@CreatedBy", Obj.createdBy ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Receptionist", Obj.receptionist ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Chemist", Obj.chemist ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Cashier", Obj.cashier ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@Doctor", Obj.doctor ?? (object)DBNull.Value);
                                                sqlCommPatientInsert.Parameters.AddWithValue("@RequestByPatient", Obj.request_by_patient ?? (object)DBNull.Value);

                                                conn.Open();
                                                sqlCommPatientInsert.ExecuteNonQuery();
                                            }

                                            msg = "Appointment Successfully Created";


                                            statuscode = "200";
                                            errorcode = "false";
                                        }


                                    }




                                }
                                catch (Exception ex)
                                {
                                    msg = ex.Message;
                                    statuscode = "201";
                                    errorcode = "true";

                                }
                                finally
                                {
                                    conn.Close();
                                }
                            }


                        }
                    }
                    

                }

                #region Code to update Firebase Listener

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                try
                {
                    //Query Qref = db.Collection("clinics").WhereEqualTo("clinic_code", Obj.clinicCode).Limit(1);
                    //QuerySnapshot snapClinic = await Qref.GetSnapshotAsync();

                    //if (snapClinic.Count > 0)
                    //{
                    //    DocumentSnapshot docSnapClinic = snapClinic.Documents[0];
                    //    Clinic clinic = docSnapClinic.ConvertTo<Clinic>();



                    //    CollectionReference col1 = db.Collection("clinics").Document(docSnapClinic.Id).Collection("WebAPIResponse");

                    //    Dictionary<string, object> data1 = new Dictionary<string, object>
                    //    {
                    //        {"CollectionName" ,"Appointment" },
                    //        {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)},
                    //    };

                    //    await col1.Document().SetAsync(data1);


                    //}

                    CollectionReference col1 = db.Collection("WebAPIResponse");
                    // Specify the document ID 'GP-101'
                    DocumentReference doc1 = col1.Document(Obj.clinicCode);

                    // Delete the document if it exists
                    await doc1.DeleteAsync();

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"CollectionName" ,"Appointment Created" },
                            {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)},

                        };

                    // Set the data for the document with the specified ID
                    await doc1.SetAsync(data1);

                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                    statuscode = "201";
                    errorcode = "true";
                }

                #endregion
            }

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;

            return result; ;
            //  return msg;
        }

        [JwtAuthorize(Roles = "user")]
        [HttpPost]
        [Route("api/AppointmentAPI/UpdateAppointment")]
        public async Task<GenericAPIResult> UpdateAppointment([FromBody] AppointmentAPI Obj)
        {
            GenericAPIResult result = new GenericAPIResult();
            var dynamicDt = new List<dynamic>();
            string errorcode = "";
            string statuscode = "";


            string msg = "";

            int maxId = 0;

            if (Obj is null)
            {
                msg = "Appointment Data is Blank";
                statuscode = "201";
                errorcode = "true";
            }
            else if (Obj.Id == 0)
            {
                msg = "Appointment Id is Blank";
                statuscode = "201";
                errorcode = "true";

            }

            else if (Obj.patient_id == "")
            {
                msg = "patient_id is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicCode == "" || Obj.clinicCode is null)
            {

                msg = "clinicCode is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinic_id == "" || Obj.clinic_id is null)
            {
                msg = "clinic_id is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.status == "" || Obj.status is null)
            {
                msg = "status is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.severity == "" || Obj.severity is null)
            {
                msg = "severity is Blank";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.updatedAt.ToString() == "01-01-0001 00:00:00")
            {
                msg = "updatedAt is invalid";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.raisedDate.ToString() == "01-01-0001 00:00:00")
            {
                msg = "raisedDate is invalid";
                statuscode = "201";
                errorcode = "true";

            }
            else if (Obj.clinicCode != "")
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    DataTable dtCC = new DataTable();
                    SqlCommand sqlCommCC = new SqlCommand("Select * from clinics where clinic_code = '" + Obj.clinicCode + "'", conn);
                    sqlCommCC.CommandType = CommandType.Text;
                    conn.Open();

                    dtCC.Load(sqlCommCC.ExecuteReader());
                    conn.Close();
                    if (dtCC.Rows.Count == 0)
                    {
                        msg = "Clinic Code " + Obj.clinicCode + " does not exists.";
                        statuscode = "201";
                        errorcode = "true";
                    }
                    else
                    {
                        DataTable dtPatient = new DataTable();
                        SqlCommand sqlCommPatient = new SqlCommand("Select * from patients where patient_id = '" + Obj.patient_id + "' and cliniccode = '" + Obj.clinicCode + "'", conn);
                        sqlCommPatient.CommandType = CommandType.Text;
                        conn.Open();

                        dtPatient.Load(sqlCommPatient.ExecuteReader());
                        conn.Close();
                        if (dtPatient.Rows.Count == 0)
                        {
                            msg = "Patient Id " + Obj.patient_id + " does not exists.";
                            statuscode = "201";
                            errorcode = "true";
                        }
                        else
                        {
                            try
                            {
                                using (SqlCommand sqlCommPatientInsert = new SqlCommand(
                                "update appointments set Days=@Days,Fee=@Fee,status = @Status, statusCashier = @StatusCashier, statusChemist = @StatusChemist, " +
                                "bill_sms = @BillSms, reminder_sms=@ReminderSms, timeStamp= @TimeStamp, updatedAt=@UpdatedAt, completionDate=@CompletionDate, " +
                                "completiondateCashier=@CompletiondateCashier, completiondateChemist=@CompletiondateChemist, referTo=@ReferTo, medicineFee=@MedicineFee, " +
                                "medicineCost=@MedicineCost, modeofpayment=@ModeOfPayment, modeofpaymentChemist=@ModeOfPaymentChemist, isCreated=@IsCreated, isSynced=@IsSynced, " +
                                "receptionist=@Receptionist, chemist=@Chemist, cashier=@Cashier, doctor=@Doctor, request_by_patient=@RequestByPatient where Id = '" + Obj.Id+"'", conn))
                                {
                                    sqlCommPatientInsert.CommandType = CommandType.Text;

                                    sqlCommPatientInsert.Parameters.AddWithValue("@Id", maxId);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@DocumentId", Obj.documentId ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Days", Obj.days ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Fee", Obj.fee ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Status", Obj.status ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@StatusCashier", Obj.statusCashier ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@StatusChemist", Obj.statusChemist ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Severity", Obj.severity ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@PatientId", Obj.patient_id ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@ClinicId", Obj.clinic_id ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@ClinicCode", Obj.clinicCode ?? (object)DBNull.Value);
                                    //sqlCommPatientInsert.Parameters.AddWithValue("@Patient", Obj.patient ?? (object)DBNull.Value);
                                    //sqlCommPatientInsert.Parameters.AddWithValue("@Patient", documentid);

                                    sqlCommPatientInsert.Parameters.AddWithValue("@BillSms", Obj.bill_sms ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@ReminderSms", Obj.reminder_sms ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@TimeStamp", DateTime.Now);
                                    //sqlCommPatientInsert.Parameters.AddWithValue("@UpdatedAt", Obj.updatedAt ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@CompletionDate", Obj.completionDate ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@CompletiondateCashier", Obj.completiondateCashier ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@CompletiondateChemist", Obj.completiondateChemist ?? (object)DBNull.Value);
                                    //sqlCommPatientInsert.Parameters.AddWithValue("@RaisedDate", Obj.raisedDate);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Token", Obj.token ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@ReferTo", Obj.referTo ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@MedicineFee", Obj.medicineFee ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@MedicineCost", Obj.medicineCost ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@ModeOfPayment", Obj.modeofpayment ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@ModeOfPaymentChemist", Obj.modeofpaymentChemist ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@IsCreated", Obj.isCreated ?? (object)DBNull.Value); 
                                    sqlCommPatientInsert.Parameters.AddWithValue("@IsSynced", Obj.isSynced ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@CreatedBy", Obj.createdBy ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Receptionist", Obj.receptionist ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Chemist", Obj.chemist ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Cashier", Obj.cashier ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@Doctor", Obj.doctor ?? (object)DBNull.Value);
                                    sqlCommPatientInsert.Parameters.AddWithValue("@RequestByPatient", Obj.request_by_patient ?? (object)DBNull.Value);

                                    conn.Open();
                                    sqlCommPatientInsert.ExecuteNonQuery();
                                }

                                msg = "Appointment Successfully Updated.";


                                statuscode = "200";
                                errorcode = "false";

                            }
                            catch (Exception ex)
                            {
                                msg = ex.Message;
                                statuscode = "201";
                                errorcode = "true";

                            }
                            finally
                            {
                                conn.Close();
                            }


                        }
                    }


                }

                #region Code to update Firebase Listener

                string Path = AppDomain.CurrentDomain.BaseDirectory + @"greenpaperdev-firebase-adminsdk-8k2y5-fb46e63414.json";
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path);
                FirestoreDb db = FirestoreDb.Create("greenpaperdev");


                try
                {
                    //Query Qref = db.Collection("clinics").WhereEqualTo("clinic_code", Obj.clinicCode).Limit(1);
                    //QuerySnapshot snapClinic = await Qref.GetSnapshotAsync();

                    //if (snapClinic.Count > 0)
                    //{
                    //    DocumentSnapshot docSnapClinic = snapClinic.Documents[0];
                    //    Clinic clinic = docSnapClinic.ConvertTo<Clinic>();

                    //    CollectionReference col1 = db.Collection("clinics").Document(docSnapClinic.Id).Collection("WebAPIResponse");

                    //    Dictionary<string, object> data1 = new Dictionary<string, object>
                    //    {
                    //        {"CollectionName" ,"Appointment" },
                    //        {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)},
                    //    };

                    //    await col1.Document().SetAsync(data1);
                    //}

                    CollectionReference col1 = db.Collection("WebAPIResponse");
                    // Specify the document ID 'GP-101'
                    DocumentReference doc1 = col1.Document(Obj.clinicCode);

                    // Delete the document if it exists
                    await doc1.DeleteAsync();

                    Dictionary<string, object> data1 = new Dictionary<string, object>
                        {
                            {"CollectionName" ,"Appointment Updated" },
                            {"UpdatedAt" ,DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)},

                        };

                    // Set the data for the document with the specified ID
                    await doc1.SetAsync(data1);

                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                    statuscode = "201";
                    errorcode = "true";
                }

                #endregion
            }

            result.message = msg;
            result.statusCode = statuscode;
            result.error = errorcode;
            result.data = dynamicDt;

            return result; ;
            //  return msg;
        }


        [JwtAuthorize(Roles = "user")]
        [HttpGet]
        [Route("api/AppointmentAPI/GetAppointmentNextTokenNo")]
        public GenericAPIResult GetNextTokenNo(string cliniccode, DateTime date)
        {
            
            string strDate = date.ToString("dd-MMM-yyyy");

            List<AppointmentAPI> patients = new List<AppointmentAPI>();
            GenericAPIResult result = new GenericAPIResult();

            var dynamicDt = new List<dynamic>();

            string errorcode = "";
            string statuscode = "";
            string msg = "";

            try
            {

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    if (cliniccode == "" || cliniccode is null)
                    {
                        msg = "clinicCode is Blank";
                        statuscode = "201";
                        errorcode = "true";
                    }
                    else
                    {
                        DataTable dtCC = new DataTable();
                        SqlCommand sqlCommCC = new SqlCommand("Select * from clinics where clinic_code = '" + cliniccode + "'", conn);
                        sqlCommCC.CommandType = CommandType.Text;
                        conn.Open();

                        dtCC.Load(sqlCommCC.ExecuteReader());
                        conn.Close();
                        if (dtCC.Rows.Count == 0)
                        {
                            msg = "Clinic Code " + cliniccode + " does not exists.";
                            statuscode = "201";
                            errorcode = "true";
                        }
                        else
                        {
                            SqlCommand sqlComm = new SqlCommand("usp_GetAppointmentNextTokenNo", conn);
                            sqlComm.Parameters.Add(new SqlParameter("@ClinicCode", cliniccode));
                            sqlComm.Parameters.Add(new SqlParameter("@Date", strDate));


                            sqlComm.CommandType = CommandType.StoredProcedure;
                            conn.Open();

                            using (SqlDataReader sdr = sqlComm.ExecuteReader())
                            {
                                while (sdr.Read())
                                {
                                    dynamic row = new ExpandoObject();
                                    var dictionary = (IDictionary<string, object>)row;

                                    for (int i = 0; i < sdr.FieldCount; i++)
                                    {
                                        dictionary.Add(sdr.GetName(i), sdr.GetValue(i));
                                    }

                                    dynamicDt.Add(row);
                                }

                            }
                            conn.Close();
                            result.message = "Next Token Number fetched Successfully";
                            result.statusCode = "200";
                            result.error = "false";
                            result.data = dynamicDt;

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                result.message = ex.Message;
                result.statusCode = "201";
                result.error = "true";
                result.data = dynamicDt;
            }

            return result;

        }

    }
}