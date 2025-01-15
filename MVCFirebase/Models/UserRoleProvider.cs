using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace MVCFirebase.Models
{
    public class UserRoleProvider: RoleProvider
    {

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }
        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }
        public override string[] GetRolesForUser(string username)
        {

            string[] webapiuser = { "user" };
            string[] webapiadmin = { "admin" };
            string[] emptystring = { };
            //string logFilePath = HttpContext.Current.Server.MapPath("~/App_Data/RoleLogs.txt"); // Adjust the path as needed
            string[] roles;

            if (username == "WEBAPIUSER")
            {
                roles = webapiuser;

            }
            else if (username == "WEBAPIADMIN")
            {
                roles = webapiadmin;

            }
            else
            {

                if (string.IsNullOrEmpty(username) || !username.Contains('|'))
                {
                    return new string[] { }; // Return empty array if username is invalid
                }

                try
                {
                    roles = username.Split('|')[2].Split('_');
                    return roles;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error in GetRolesForUser: " + ex.Message);
                    return new string[] { }; // Return empty roles on error
                }


                //if (HttpContext.Current.Session != null)//&& HttpContext.Current.Session["UserRoles"] != null
                //{
                //    //GlobalSessionVariables.ClinicMobileNumber = username.Split('|')[3];
                //    //GlobalSessionVariables.ClinicDocumentAutoId = username.Split('|')[4];
                //    //GlobalSessionVariables.ClinicCode = username.Split('|')[5];
                //    //GlobalSessionVariables.UserRoles = username.Split('|')[2];

                //    roles = username.Split('|')[2].Split('_');


                //    //return GlobalSessionVariables.UserRoles.Split(',');
                //}
                //else
                //{

                //    roles = emptystring;

                //}

            }

            // Log roles to the text file
            //LogRolesToFile(username, roles, logFilePath);

            return roles;




        }

        private void LogRolesToFile(string username, string[] roles, string logFilePath)
        {
            try
            {
                string roleString = string.Join(", ", roles);
                string logEntry = $"Username: {username}, Roles: {roleString}, Timestamp: {DateTime.Now}\n";

                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

                // Append the log entry to the file
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // Log exception if needed, but avoid throwing to ensure the main functionality isn't affected
                File.AppendAllText(logFilePath, $"Error logging roles for {username}: {ex.Message}\n");
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }
        public override bool IsUserInRole(string username, string roleName)
        {
            var roles = GetRolesForUser(username);

            return roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

        }
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }
        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}