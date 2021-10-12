using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FormulierFunctions.Models;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace FormulierFunctions
{
    public class RegistrationFunction
    {
        [FunctionName("AddRegistrationSQL")]
        public async Task<IActionResult> AddRegistrationSQL(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/registrations")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var json = await new StreamReader(req.Body).ReadToEndAsync();
                var registration = JsonConvert.DeserializeObject<Registration>(json);

                string connectionString = Environment.GetEnvironmentVariable("SQLSERVER");

                string guid = Guid.NewGuid().ToString();
                registration.RegistrationId = guid;

                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();

                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "INSERT INTO Registraties VALUES(@RegistrationId,@FirstName,@LastName,@Email,@Zipcode,@Age,@IsFirstTimer)";
                        sqlCommand.Parameters.AddWithValue("@RegistrationId", registration.RegistrationId);
                        sqlCommand.Parameters.AddWithValue("@FirstName", registration.firstName);
                        sqlCommand.Parameters.AddWithValue("@LastName", registration.lastName);
                        sqlCommand.Parameters.AddWithValue("@Email", registration.email);
                        sqlCommand.Parameters.AddWithValue("@Zipcode", registration.zipcode);
                        sqlCommand.Parameters.AddWithValue("@Age", registration.age);
                        sqlCommand.Parameters.AddWithValue("@IsFirstTimer", registration.isFirstTimer);

                        await sqlCommand.ExecuteNonQueryAsync();

                    }
                }

                return new OkObjectResult(registration);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
            }
        }
        [FunctionName("SelectRegistrationSQL")]
        public async Task<IActionResult> SelectRegistrationSQL(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/registrations")] HttpRequest req,
            ILogger log)
        {
            try
            {
                string connectionString = Environment.GetEnvironmentVariable("SQLSERVER");

                List<Registration> registrations = new List<Registration>();

                using (SqlConnection sqlConnection = new SqlConnection())
                {
                    await sqlConnection.OpenAsync();

                    using(SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "SELECT * FROM Registraties";
                        var reader = await sqlCommand.ExecuteReaderAsync();

                        while(await reader.ReadAsync())
                        {
                            registrations.Add(new Registration()
                            {
                                age = int.Parse(reader["Age"].ToString()),
                                email = reader["EMail"].ToString(),
                                firstName = reader["FirstName"].ToString(),
                                lastName = reader["LastName"].ToString(),
                                isFirstTimer = bool.Parse(reader["IsFirstTimer"].ToString()),
                                RegistrationId = reader["RegistratieId"].ToString(),
                                zipcode = reader["Zipcode"].ToString()
                            });
                        }
                    }
                    return new OkObjectResult(registrations);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
            }
        }
    }
}
