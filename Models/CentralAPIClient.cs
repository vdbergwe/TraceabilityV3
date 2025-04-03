using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace TraceabilityV3.Models
{
    public class CentralAPIClient : ICentralApiClient 
    {
        private readonly HttpClient _httpClient;
        private TraceabilityEntities db = new TraceabilityEntities();

        public CentralAPIClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task UploadRecord(HandlingUnit record)
        {
            //try
            //{
            var ApiHU = new APIHandlingUnit
            {
                SSCC = record.SSCC,
                WERKS = record.WERKS,
                MATNR = record.MATNR,
                ScannedCode = record.ScannedCode,
                Created = record.Created,
                CreatedBy = record.CreatedBy,
                Batch = record.Batch,
                Horse = record.Horse,
                Status = record.Status,
                ChildServer = Environment.MachineName,
                zzSAPIntegration = false
            };

                var ZplLable = db.IssuedLabels.FirstOrDefault(il => il.SSCC == record.SSCC);
                var RegisterMovement = db.HandlingUnitMovements.FirstOrDefault(hu => hu.SSCC == record.SSCC && hu.Status == "Registered");

                if (RegisterMovement == null)
                {
                    LogToFile("Error: RegisterMovement is null. Cannot determine Waypoint.");
                    return;
                }

                if (ZplLable == null)
                {
                    LogToFile("Warning: ZplLable is null. Some label data might be missing.");
                    return;
                }

                var APIZTraceOut = new APIzTraceOut_SSCCUnit
                {
                    WERKS = ApiHU.WERKS,
                    MATNR = ApiHU.MATNR,
                    SSCC = ApiHU.SSCC,
                    WaypointId = double.TryParse(RegisterMovement?.Waypoint, out double waypointValue)
                        ? (int)Math.Round(waypointValue)
                        : 0,
                    Waypoint = RegisterMovement?.Device ?? "Unknown",
                    Created = ApiHU.Created,
                    CreatedTime = ApiHU.Created.HasValue
                        ? (ApiHU.Created.Value.TimeOfDay) // Extract only the time portion
                        : (TimeSpan?)null,
                    CreatedDate = ApiHU.Created,
                    CreatedBy = ApiHU.CreatedBy,
                    ActualWeight = ZplLable?.MaterialNetWt ?? 0m,
                    ScannedGTIN = ZplLable?.MaterialGTIN,
                    Label_GTIN = ZplLable?.MaterialGTIN,
                    Label_Description = ZplLable?.MaterialDescription,
                    Label_Country = ZplLable?.Country,
                    PData_DateCode = ZplLable?.JulianDate,
                    PData_DateTime = ZplLable.Created?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    //PData_DateTime = ZplLable?.Created.ToString(),
                    PData_BatchCode = ZplLable?.Batch,
                    PData_BestBefore = ZplLable?.BestBefore.ToString(),
                    PDet_PCode = ZplLable?.MATNR,
                    PDet_OrdUnits = ZplLable?.MaterialQtyLevel1.HasValue == true
                        ? (int)Math.Round(ZplLable.MaterialQtyLevel1.Value)
                        : 0,
                    PDet_ConsUnits = ZplLable?.MaterialQtyCON.HasValue == true
                        ? (int)Math.Round(ZplLable.MaterialQtyCON.Value)
                        : 0,
                    PDet_NetWt = ZplLable?.MaterialNetWt ?? 0m
                };

                var combinedData = new
                {
                    HandlingUnit = ApiHU,
                    TraceUnit = APIZTraceOut
                };

                string jsonData = JsonConvert.SerializeObject(combinedData);

                LogToFile($"Sending JSON:\n{jsonData}");

                using (var content = new StringContent(jsonData, Encoding.UTF8, "application/json"))
                using (var response = await _httpClient.PostAsync($"{Global.CentralURL}api/APIHandlingUnits/AutoUpdate", content))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        LogToFile($"Error: API responded with {response.StatusCode}\nResponse: {responseContent}");
                        return;
                    }

                    LogToFile($"Success: API responded with {response.StatusCode}\nResponse: {responseContent}");
                }
            //}
            //catch (Exception ex)
            //{
            //    LogToFile($"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            //}
        }

        /// <summary>
        /// Writes log data to a file located in the Models directory.
        /// </summary>
        private void LogToFile(string message)
        {
            try
            {
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "Log.txt");
                string logDirectory = Path.GetDirectoryName(logFilePath);

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logEntry = $"{DateTime.Now}: {message}\n{"-".PadLeft(80, '-')}\n";
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging Error: {ex.Message}");
            }
        }

        public async Task UploadMovementRecord(HandlingUnitMovement record)
        {
            var ApiMovementHU = new APIHandlingUnitMovement
            {
                SSCC = record.SSCC,
                Device = record.Device,
                MovementTime = record.MovementTime,                
                CreatedBy = record.CreatedBy,
                Type = record.Type,
                Status = record.Status,
                Horse = record.Horse,
                Source = record.Source,
                Destination = record.Destination,
                Waypoint = record.Waypoint,
                RejectID = record.RejectID,
                ChildServer = Environment.MachineName
            };
            var json = JsonConvert.SerializeObject(ApiMovementHU);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{Global.CentralURL}api/APIHandlingUnits/AutoMovementUpdate", content);

            response.EnsureSuccessStatusCode();
        }
    }
}