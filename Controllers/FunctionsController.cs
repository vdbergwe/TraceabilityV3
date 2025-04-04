using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.UI;
using System.Threading;
using System.Globalization;
using TraceabilityV3.Models;

namespace TraceabilityV3.Controllers
{
    public class FunctionsController : Controller
    {
        private TraceabilityEntities db = new TraceabilityEntities();

        // GET: Functions
        public ActionResult Index()
        {
            return View();
        }

        public class SSCCCentral
        {
            public string NumberBank { get; set; }
            public int Range { get; set; }
            public string LastIssued { get; set; }
            public DateTime Created { get; set; }
            public string Reserved { get; set; }
            public string Released { get; set; }
            public string Depleted { get; set; }
            public string ChildServer { get; set; }
            public string Status { get; set; }
        }


        public async Task<string> UpdateSSCCCentral(string NumberBank, string LastNumber)
        {
            // Send HTTPS Get to update Central SSCC Number Bank
            using (var client = new HttpClient())
            {
                string CentralURL = $"https://rclstrace01.tsb.co.za:4443/api/APISSCCCentrals/UpdateSSCCCentral/{NumberBank}/{LastNumber}";

                try
                {
                    // Retrieve the latest SSCC data from the central server
                    var response = await client.GetAsync(CentralURL);
                    Console.WriteLine($"Response Status Code: {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        return "";
                    }

                    // Deserialize the JSON response (adjust for single object, not a list)
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    SSCCCentral receivedData = JsonConvert.DeserializeObject<SSCCCentral>(jsonResponse); // Changed from List<SSCCCentral> to SSCCCentral

                    if (receivedData == null)
                    {
                        return "";
                    }

                    // Validate if received data matches the sent data
                    if (NumberBank == receivedData.NumberBank &&
                        LastNumber == receivedData.LastIssued)
                    {
                        return ""; // Data matches, no further action needed
                    }
                    else
                    {
                        return ""; // Data doesn't match
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "";
                }
            }
        }


        public async Task<string> GenerateSSCC(string WaypointId)
        {
            // Get Current SSCC Bank
            var SSCCNumberBank = db.SSCCNumberBanks
                    .Where(s => s.Status == "Active" && s.Device == WaypointId)
                    .OrderByDescending(s => s.NumberBank)
                    .FirstOrDefault();
            int MaxNumberRange = 0;

            if (SSCCNumberBank != null)
            {
                MaxNumberRange = Convert.ToInt32(SSCCNumberBank.Range);
            }


            // Ensure SSCCNumberBank exists and is not Depleted
            if (SSCCNumberBank != null && int.TryParse(SSCCNumberBank.LastIssued, out int lastIssuedNumber) && lastIssuedNumber < MaxNumberRange)
            {
                int NextNumber = Convert.ToInt32(SSCCNumberBank.LastIssued) + 1;

                int RangeDigits = SSCCNumberBank.Range.Length;

                // Construct the base SSCC
                string baseSSCC = $"{SSCCNumberBank.NumberBank}{NextNumber.ToString("D"+RangeDigits)}";
                int weightedSum = 0;
                int length = baseSSCC.Length;

                // Compute weighted sum manually
                for (int i = 2; i < length; i++) // Start at index 2
                {
                    int digit = baseSSCC[i] - '0';
                    weightedSum += digit * ((i % 2 == 0) ? 3 : 1);
                }

                // Compute check digit using direct modulo arithmetic
                int checkDigit = (10 - (weightedSum % 10)) % 10;

                // Concatenate final SSCC
                string finalSSCC = baseSSCC + checkDigit;                

                // Update SSCCNumberBank
                SSCCNumberBank.LastIssued = NextNumber.ToString();

                if (NextNumber > (MaxNumberRange - 1))
                {
                    SSCCNumberBank.Status = "Depleted";
                    SSCCNumberBank.Depleted = DateTime.Now;
                }

                db.Entry(SSCCNumberBank).State = EntityState.Modified;
                await db.SaveChangesAsync();

                await UpdateSSCCCentral(SSCCNumberBank.NumberBank,SSCCNumberBank.LastIssued);

                // Ensure the database query is awaited to prevent threading issues
                bool exists = await db.HandlingUnits
                                      .Where(h => h.SSCC == finalSSCC)
                                      .AnyAsync();

                // Return the result based on whether the record exists
                if (exists)
                {
                    return "False";
                }
                else
                {
                    return finalSSCC;
                }
            }
            else
            {
                int counter = 0;
                bool result = false;

                // Send HTTP request to obtain new SSCCNumberBank from Central Server
                while (!result && counter < 3)  // Limit the loop to 3 attempts
                {
                    counter++;
                    var response = await RequestNewSSCCBankFromServer(WaypointId);

                    // Attempt to parse the response to a boolean
                    if (bool.TryParse(response, out result) && result)
                    {
                        return "True";  
                    }

                    Thread.Sleep(500);  
                }

                return "False";  // Return "False" if not successful
            }           
            
        }

        public async Task<string> RequestNewSSCCBankFromServer(string WaypointId)
        {
            using (var client = new HttpClient())
            {
                string serverName = Dns.GetHostName(); // Get the server name
                string CentralURL = $"https://rclstrace01.tsb.co.za:4443/api/APISSCCCentrals/GenerateSSCC/{serverName}/{WaypointId}";
                try
                {
                    var response = await client.GetAsync(CentralURL);
                    Console.WriteLine($"Response Status Code: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        // Deserialize the JSON response
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // Deserialize JSON response into a List of  objects
                        List<SSCCBank> responseData = JsonConvert.DeserializeObject<List<SSCCBank>>(jsonResponse);

                        // Ensure the list is not empty before accessing the first item
                        if (responseData != null && responseData.Count > 0)
                        {
                            SSCCBank firstItem = responseData[0]; // Get the first object in the list
                                                                        // Map the response data to SSCCNumberBank model
                            var newSSCCBank = new SSCCNumberBank
                            {
                                NumberBank = firstItem.AD + firstItem.ED + firstItem.GS1 + firstItem.Bank,
                                Range = firstItem.Range,
                                LastIssued = firstItem.LastIssued,
                                Status = firstItem.Status,
                                Device = firstItem.ChildDevice,
                                Created = firstItem.Created
                            };

                            // Add the new SSCCNumberBank to the database and save changes
                            db.SSCCNumberBanks.Add(newSSCCBank);
                            await db.SaveChangesAsync();

                            return "True";
                        }
                       
                    }
                    else
                    {
                        return "False";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return "False";
            }
        }

        public async Task<HandlingUnit> CreateHandlingUnit(string ScannedValue, string WaypointId, string UserId, string SSCC)
        {
            string Waypoint = db.Devices
                .Where(d => d.Description == WaypointId)
                .Select(d => d.Waypoint)
                .FirstOrDefault().ToString();

            string WERKS = db.Waypoints
                .Where(wp => wp.Id.ToString() == Waypoint)
                .Select(wp => wp.Location)
                .FirstOrDefault().ToString();

            var CurrentMaterial = db.SAPMaterials
                .Where(sm => sm.GTIN_HU == ScannedValue ||
                       sm.GTIN_Level1 == ScannedValue ||
                       sm.GTIN_Level2 == ScannedValue ||
                       sm.GTIN_CON == ScannedValue)
                .Where(sm => sm.WERKS == WERKS)
                .AsNoTracking()
                .FirstOrDefault();

            // Get the current year in four digits
            string year = DateTime.Now.Year.ToString();

            // Get the current Julian day of the year (001-365/366)
            string julianDay = DateTime.Now.DayOfYear.ToString("D3");

            // Determine the plant code based on WERKS using a traditional switch
            string plantCode;
            switch (WERKS)
            {
                case "1110":
                    plantCode = "M";
                    break;
                case "1132":
                    plantCode = "P";
                    break;
                case "5001":
                    plantCode = "SZ";
                    break;
                default:
                    throw new ArgumentException("Invalid WERKS value");
            }

            // Generate the batch number
            string Batch = $"{year}{julianDay}{plantCode}{WERKS}";

            int CurrentWaypointId = Convert.ToInt32(db.Devices.Where(d => d.Description == WaypointId).Select(d => d.Waypoint).FirstOrDefault());
            var CurrentWaypoint = db.Waypoints.Where(wp => wp.Id == CurrentWaypointId).FirstOrDefault();

            // Generate a new HandlingUnit 
            var newHandlingUnit = new HandlingUnit
            {
                SSCC = SSCC,
                WERKS = WERKS,
                MATNR = CurrentMaterial.MATNR,
                ScannedCode = ScannedValue,
                Created = DateTime.Now,
                CreatedBy = UserId,
                Batch = Batch,
                Horse = "",
                Status = "Registered"
            };

            // Generate new Handling Unit Movement
            var HandlingUnitMovement = new HandlingUnitMovement
            {
                SSCC = SSCC,
                Device = WaypointId,
                MovementTime = DateTime.Now,
                CreatedBy = UserId,
                Type = "PAS",
                Status = "Registered",
                Horse = "",
                Source = CurrentWaypoint.Description,
                Destination = "Inspection",
                Waypoint = CurrentWaypoint.Id.ToString(),
                RejectID = ""
            };

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.HandlingUnits.Add(newHandlingUnit);
                    db.HandlingUnitMovements.Add(HandlingUnitMovement);

                    await db.SaveChangesAsync();

                    // Commit the transaction if both entries are saved successfully
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback in case of an error
                    transaction.Rollback();
                    Console.WriteLine($"Error saving handling unit: {ex.Message}");
                    throw; // Rethrow to propagate the exception
                }
            }

            return newHandlingUnit;
        }

        public string GenerateZPL(string WaypointId, string SSCC)
        {
            string BusinessUnit = "RCL Foods Sugar and Milling (PTY) LTD";
            string Country = "South Africa";

            // Get the current year in two-digit format
            string YearCode = DateTime.Now.ToString("yy");

            // Get the current Julian day of the year (001-365/366)
            string DateCode = DateTime.Now.DayOfYear.ToString("D3");

            // Combine them to get the full Julian date code
            string JulianDateCode = YearCode + DateCode;

            string dateString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // Calculate ExpiryDate
            DateTime ExpiryDate = DateTime.Now.AddYears(3);
            string BestBefore = ExpiryDate.ToString("yyyy/MM/dd");

            // Fetch data
            var data = (from hu in db.HandlingUnits
                        join d in db.Devices on hu.SSCC equals SSCC
                        join wp in db.Waypoints on d.Waypoint equals wp.Id
                        join sm in db.SAPMaterials on hu.MATNR equals sm.MATNR
                        where hu.SSCC == SSCC && d.Description == WaypointId
                        select new
                        {
                            hu.Batch,
                            hu.MATNR,
                            hu.SSCC,
                            WERKS = wp.Location.ToString(),
                            Material = sm.MAKTX,
                            MaterialGTIN = sm.GTIN_HU,
                            MaterialQtyLevel1 = sm.HUQty_Level1,
                            MaterialQtyCON = sm.HUQty_CON,
                            MaterialNetWt = sm.NetWt_HU
                        }).FirstOrDefault();

            if (data == null)
            {
                return string.Empty;
            }
           
            var result = new
            {
                MaterialQtyLevel1 = (int)data.MaterialQtyLevel1,  
                MaterialQtyCON = (int)data.MaterialQtyCON,  
                MaterialNetWt = (int)data.MaterialNetWt  
            };

            int LabelCount = 1;
            int LabelNo = 0;

            List<string> LabelLines = new List<string> { "^XA\n" };

            while (LabelNo < LabelCount)
            {
                LabelNo++;

                LabelLines.Add("^FX Start of label number " + LabelNo + " for SSCC " + SSCC + "\n");
                LabelLines.Add("^XA\n");

                // Business Unit & Country
                LabelLines.Add("^CF0,47^FB790,2,0,C,0^FO10,40^FD" + BusinessUnit + "^FS\n");
                LabelLines.Add("^CF0,40^FB790,2,0,C,0^FO10,105^FD" + Country + "^FS\n");

                // Underline for Company and Country
                LabelLines.Add("^FO10,165^GB810,2,2,B,0^FS\n");

                // QR Code for SSCC
                LabelLines.Add("^FO40,205^BXN,18,200,,,,,1^FD" + SSCC + "^FS\n");

                // Production Data
                LabelLines.Add("^CF0,35^FB403,1,0,C,0^FO340,210^FDPRODUCTION DATA:\\&^FS\n");
                LabelLines.Add("^CF0,28\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,260^FDDate Code:\\&^FS^FO500,260^FD" + JulianDateCode + "^FS\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,295^FDDate/Time:\\&^FS^FO500,295 ^FD" + dateString + "^FS\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,330^FDPlant:\\&^FS^FO500,330 ^FD" + data.WERKS + "^FS\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,365^FDBatch Code:\\&^FS^FO500,365  ^FD" + data.Batch + "^FS\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,400^FDBest Before:\\&^FS ^FO500,400 ^FD" + BestBefore + "^FS\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,435^FDSSCC:\\&^FS ^FO500,435 ^FD" + SSCC + "^FS\n");

                // Underline for SSCC and Production Data
                LabelLines.Add("^FO10,520^GB810,2,2,B,0^FS\n");

                // Material Information
                LabelLines.Add("^CF0,105^FB785,1,0,C,0^FO30,560^FD" + data.MATNR.TrimStart('0') + "\\&^FS\n");
                
                // Determine font size based on string length
                string fontSize = data.Material.Length > 28 ? "68" : "85";  // Set font size to 60 if over 28 characters, otherwise 85

                LabelLines.Add($"^CF0,{fontSize}^FB785,2,0,C,0^FO30,680^FD{data.Material}^FS");

                // Underline for Description
                LabelLines.Add("^FO10,880^GB810,2,2,B,0^FS\n");

                // QR Code for Pallet GTIN
                LabelLines.Add("^FO60,955^BXN,8,200,,,,,1^FD" + data.MaterialGTIN + "^FS\n");

                // Product Details Section
                LabelLines.Add("^CF0,35^FB403,1,0,C,0^FO295,930^FDPRODUCT DETAILS:\\&^FS\n");
                LabelLines.Add("^CF0,28\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,975^FDProduct HU GTIN:^FS^FO500,975^FD" + data.MaterialGTIN + "^FS\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,1010^FDOrder Units:^FS^FO500,1010  ^FD" + result.MaterialQtyLevel1 + "^FS\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,1045^FDConsumer Units:^FS ^FO500,1045^FD" + result.MaterialQtyCON + "^FS\n");
                LabelLines.Add("^FB240,1,0,R,0^FO250,1080^FDNet Weight:^FS^FO500,1080^FD" + result.MaterialNetWt + "^FS\n");

                // Underline for Unique Label ID
                LabelLines.Add("^FO10,1130^GB810,2,2,B,0^FS\n");

                // SSCC Label ID
                LabelLines.Add("^FO90,1140^FDSSCC Label ID: ^FS\n");
                LabelLines.Add("^FO290,1140^FD" + SSCC + "-" + LabelNo + "-" + LabelCount + "^FS\n");

                // Bottom Label Group Indicators
                LabelLines.Add("^FO1,1168^GB810,3,3,B,0^FS\n");
                LabelLines.Add("^FO1,1168^GB203,40,21,B,0^FS\n");
                LabelLines.Add("^FO204,1168^GB203,40,3,B,0^FS\n");
                LabelLines.Add("^FO407,1168^GB203,40,3,B,0^FS\n");
                LabelLines.Add("^FO610,1168^GB203,40,3,B,0^FS\n");

                // End of Label
                LabelLines.Add("^XZ\n");
                LabelLines.Add("^FX End of label number " + LabelNo + "\n");
                LabelLines.Add("\n");
            }

            try
            {
                var ThisLabel = new IssuedLabel
                {
                    SSCC = SSCC,
                    BusinessUnit = BusinessUnit,
                    Country = Country,
                    JulianDate = JulianDateCode,
                    DateString = DateTime.Parse(dateString),
                    WERKS = data.WERKS,
                    Batch = data.Batch,
                    BestBefore = DateTime.Parse(BestBefore),
                    MATNR = data.MATNR,
                    MaterialDescription = data.Material,
                    MaterialGTIN = data.MaterialGTIN,
                    MaterialQtyLevel1 = data.MaterialQtyLevel1,
                    MaterialQtyCON = data.MaterialQtyCON,
                    MaterialNetWt = data.MaterialNetWt,  
                    ZPL = string.Join("", LabelLines),
                    Created = DateTime.Now
                };

                db.IssuedLabels.Add(ThisLabel);
                db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());  
            };            

            return string.Join("", LabelLines);
        }

        public class SSCCBank
        {
            public string AD { get; set; }
            public string ED { get; set; }
            public string GS1 { get; set; }
            public string Bank { get; set; }
            public string Range { get; set; }
            public string LastIssued { get; set; }
            public Nullable<System.DateTime> Created { get; set; }
            public Nullable<System.DateTime> Reserved { get; set; }
            public Nullable<System.DateTime> Released { get; set; }
            public Nullable<System.DateTime> Depleted { get; set; }
            public string ChildServer { get; set; }
            public string ChildDevice { get; set; }
            public string Status { get; set; }
        }




    }
}