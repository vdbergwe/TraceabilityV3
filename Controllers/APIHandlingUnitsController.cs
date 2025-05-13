using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TraceabilityV3;
using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.Models;
using System.Data.SqlTypes;
using BinaryKits.Zpl.Label.Elements;
using System.Xml.Linq;
using System.Drawing;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Ajax.Utilities;
using System.IO;
using TraceabilityV3.Models;
using System.Web;

namespace TraceabilityV3.Controllers
{
    public class APIHandlingUnitsController : ApiController
    {
        private TraceabilityEntities db = new TraceabilityEntities();
        FunctionsController functionsController = new FunctionsController();
        static TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");

        public class WaypointDto
        {
            public string Description { get; set; }
            public string Status { get; set; }
        }

        private readonly UploadManager _uploadManager;
        private static readonly HttpClient httpClient = new HttpClient();

        // Labelary API endpoint. Adjust resolution and label dimensions as needed.
        private const string LabelaryUrl = "http://api.labelary.com/v1/printers/8dpmm/labels/4x6/0/";
         

        // GET: api/APIHandlingUnits
        public IQueryable<HandlingUnit> GetHandlingUnits()
        {
            return db.HandlingUnits;
        }

        // GET: api/APIHandlingUnits/5
        [ResponseType(typeof(HandlingUnit))]
        public IHttpActionResult GetHandlingUnit(string id)
        {
            HandlingUnit handlingUnit = db.HandlingUnits.Find(id);
            if (handlingUnit == null)
            {
                return NotFound();
            }

            return Ok(handlingUnit);
        }

        // GET: api/APIHandlingUnits/SendEmail/{WaypointId}/{UserId}/{Message}
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/SendEmail/{WaypointId}/{UserId}/{Message}")]
        public async Task<IHttpActionResult> SendEmail(string WaypointId,string UserId, String Message)
        {
            try
            {
                // URL encode the from, to, subject, and body
                string from = "traceability@rclfoods.com";
                string to = "werner.vanderberg@rclfoods.com";
                string subject = "Support Requested at: " + WaypointId + " - by: " + UserId;
                string body = Message;

                // Build the URL to call the EmailService API
                string emailApiUrl = $"https://rclpgstrace01.tsb.co.za:5443/Email/SendEmail?from={from}&to={to}&subject={subject}&body={body}";


                // Call the Email API using HttpClient
                HttpResponseMessage response = await httpClient.GetAsync(emailApiUrl);

                // Check if the Email API responded successfully
                if (response.IsSuccessStatusCode)
                {
                    // If successful, return a success message
                    return Ok("Email sent successfully.");
                }
                else
                {
                    // If the response indicates failure, return the error
                    return InternalServerError(new Exception("Failed to send email via Email API."));
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a general error
                return InternalServerError(ex);
            }
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/CheckWaypoint/{device}/{operatorId}")]
        public async Task<IHttpActionResult> CheckWaypoint(string device, string operatorId)
        {
            // Get the current waypointId for the device
            int? waypointId = db.Devices
                .Where(d => d.Description == device)
                .Select(d => d.Waypoint)
                .FirstOrDefault();

            if (waypointId == null)
                return NotFound();

            // Load the current waypoint
            var callingWaypoint = await db.Waypoints.FindAsync(waypointId);

            if (callingWaypoint == null)
                return NotFound();

            // Get all variable waypoints at the same location
            var vWaypoints = db.Waypoints
                .Where(w => w.VariableWaypoint == true && w.Location == callingWaypoint.Location)
                .Select(w => new WaypointDto
                {
                    Description = w.Description,
                    Status = w.Status
                })
                .AsNoTracking()
                .ToList();

            return Ok(vWaypoints);
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/ChangeWaypoint/{Device}/{operatorId}")]
        public async Task<IHttpActionResult> ChangeWaypoint(string Device, string operatorId)
        {
            try
            {
                if (string.IsNullOrEmpty(Device) || string.IsNullOrEmpty(operatorId))
                {
                    return BadRequest("Invalid input parameters.");
                }

                List<WaypointDto> result = null;

                // Get the current waypoint ID for the device
                int? waypointId = db.Devices
                    .Where(d => d.Description == Device)
                    .Select(d => d.Waypoint)
                    .FirstOrDefault();

                if (waypointId == null)
                {
                    return BadRequest("Device not associated with a waypoint.");
                }

                // Load the current waypoint and its toWaypoint
                var callingWaypoint = db.Waypoints.Find(waypointId);
                var currentToWaypoint = db.Waypoints.Find(callingWaypoint.ToWaypoint);

                if (currentToWaypoint == null || !(currentToWaypoint.VariableWaypoint ?? false))

                {
                    return BadRequest("Current 'toWaypoint' is invalid or not a variable waypoint.");
                }

                // Look for an available open variable waypoint at the same location (excluding the current one)
                var availableOpenWaypoint = db.Waypoints
                    .Where(w => w.VariableWaypoint == true
                                && w.Status == "Open"
                                && w.Location == callingWaypoint.Location)
                    .FirstOrDefault();

                if (availableOpenWaypoint == null)
                {
                    return BadRequest("NO OPEN BINS");
                }

                // Get all handling units in the current toWaypoint
                var handlingUnitsToMove = db.HandlingUnits
                    .Where(h => h.Status == "To " + currentToWaypoint.Description)
                    .ToList();

                bool binWasClosed = false;

                // Get the next waypoint to move HUs to
                var nextWaypoint = db.Waypoints.Find(currentToWaypoint.ToWaypoint);

                if (nextWaypoint == null)
                {
                    return BadRequest("Next waypoint is not defined.");
                }

                // If there are handling units, transfer and close current bin
                if (handlingUnitsToMove.Count > 0)
                {
                    foreach (var hu in handlingUnitsToMove)
                    {
                        await ConfirmHandlingUnitInternal(hu.SSCC, currentToWaypoint.Id.ToString(), operatorId);                       
                    }

                    currentToWaypoint.Status = "Closed";
                    db.Entry(currentToWaypoint).State = EntityState.Modified;
                    binWasClosed = true;
                }

                // If the bin was closed, assign a new one
                if (binWasClosed)
                {
                    callingWaypoint.ToWaypoint = availableOpenWaypoint.Id;
                    availableOpenWaypoint.Status = "In Use";

                    db.Entry(availableOpenWaypoint).State = EntityState.Modified;
                    db.Entry(callingWaypoint).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                // Return updated list of variable waypoints at the same location
                result = db.Waypoints
                    .Where(w => w.VariableWaypoint == true && w.Location == callingWaypoint.Location)
                    .Select(w => new WaypointDto
                    {
                        Description = w.Description,
                        Status = w.Status
                    })
                    .AsNoTracking()
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/APIHandlingUnits/PendingHandlingUnits/WaypointId
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/PendingHandlingUnits/{Device}")]
        public async Task<IHttpActionResult> PendingHandlingUnits (string Device)
        {
            int? WaypointId = db.Devices.Where(d => d.Description == Device).Select(d => d.Waypoint).FirstOrDefault();
            var CurrentWaypoint = db.Waypoints.Find(WaypointId);
            string Status1 = string.Empty;
            string Status2 = string.Empty;
            string InitiatingWaypointId = string.Empty;
            if (CurrentWaypoint.Type == "PAS")
            {
                Status1 = "Accepted";
                Status2 = "Registered";
                InitiatingWaypointId = CurrentWaypoint.Id.ToString();
            }
            if (CurrentWaypoint.Type == "GUS")
            {
                Status1 = "To " + CurrentWaypoint.Description;
                Status2 = "Accepted at " + CurrentWaypoint.Description;
                InitiatingWaypointId = db.Waypoints.Where(w => w.ToWaypoint == CurrentWaypoint.Id).Select(w => w.Id).FirstOrDefault().ToString();
                var iWaypoint = db.Waypoints
                                .Where(w => w.ToWaypoint == CurrentWaypoint.Id)
                                .FirstOrDefault();

                if (iWaypoint?.VariableWaypoint == true)
                {
                    while (iWaypoint != null && iWaypoint.Status != "Closed")
                    {
                        iWaypoint = db.Waypoints
                            .Where(w => w.ToWaypoint == CurrentWaypoint.Id && w.Status == "Closed")
                            .FirstOrDefault();

                        if (iWaypoint == null)
                        {
                            Console.WriteLine("No closed waypoint found!");
                            break;
                        }
                    }
                }
                InitiatingWaypointId = iWaypoint.Id.ToString();
            }

            if (WaypointId != null)
            {
                try
                {
                    // Join HandlingUnits with HandlingUnitMovements and SAPMaterials based on SSCC and MATNR
                    var handlingUnits = await (from hu in db.HandlingUnits
                                               join hum in db.HandlingUnitMovements on hu.SSCC equals hum.SSCC
                                               join sm in db.SAPMaterials on hu.MATNR equals sm.MATNR
                                               where (hu.Status == Status1 || hu.Status == Status2)
                                                     && hum.Waypoint == InitiatingWaypointId
                                               select new
                                               {
                                                   SSCC = hu.SSCC,
                                                   Product = sm.MAKTX, // Fetch the MAKTX (Product) from SAPMaterials table
                                                   Status = hu.Status
                                               }).Distinct().ToListAsync();

                    // Check if any handling units are found
                    if (handlingUnits == null || handlingUnits.Count == 0)
                    {
                        return NotFound(); // Return 404 if no matching handling units are found
                    }

                    // Return the list of handling units
                    return Ok(handlingUnits);
                }
                catch (Exception ex)
                {
                    // Log the error and return a server error
                    return InternalServerError(ex);
                }
            }
            else
                { return NotFound(); }
           
        }

        // GET: api/APIHandlingUnits/HandlingUnitHistory/WaypointId
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/HandlingUnitHistory/{Device}")]
        public async Task<IHttpActionResult> HandlingUnitHistory(string Device)
        {
            int? WaypointId = db.Devices.Where(d => d.Description == Device).Select(d => d.Waypoint).FirstOrDefault();
            var CurrentWaypoint = db.Waypoints.Find(WaypointId);
            var NextWaypoint = db.Waypoints.Find(CurrentWaypoint.ToWaypoint);
            string Status1 = "To " + NextWaypoint.Description;
            
            if (WaypointId != null)
            {
                try
                {
                    var handlingUnits = await (from hum in db.HandlingUnitMovements
                                               join hu in db.HandlingUnits on hum.SSCC equals hu.SSCC
                                               join sm in db.SAPMaterials on hu.MATNR equals sm.MATNR
                                               where hum.Waypoint == CurrentWaypoint.Id.ToString() && hum.Status == Status1
                                               group new { hum, hu, sm } by hum.SSCC into g
                                               let latestMovement = g.OrderByDescending(h => h.hum.MovementTime).FirstOrDefault()
                                               orderby latestMovement.hum.MovementTime descending
                                               select new
                                               {
                                                   SSCC = g.Key,
                                                   Product = latestMovement.sm.MAKTX,
                                                   Status = latestMovement.hu.Status,
                                                   MovementTime = latestMovement.hum.MovementTime
                                               })
                          .Take(12)
                          .AsNoTracking()
                          .ToListAsync();

                    if (handlingUnits == null || handlingUnits.Count == 0)
                    {
                        return NotFound(); 
                    }
                                        
                    return Ok(handlingUnits.OrderBy(h => h.SSCC));
                }
                catch (Exception ex)
                {                    
                    return InternalServerError(ex);
                }
            }
            else
            { return NotFound(); }

        }

        // GET: api/APIHandlingUnits/AcceptHandlingUnit/SSCC/WaypointId/UserId
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/AcceptHandlingUnit/{SSCC}/{WaypointId}/{UserId}")]
        public async Task<IHttpActionResult> AcceptHandlingUnit(string SSCC, string WaypointId, string UserId)
        {
            try
            {
                // Validate if the SSCC is in the correct format or exists in the database
                if (string.IsNullOrEmpty(SSCC))
                {
                    return Ok("False"); // Return False if SSCC is invalid
                }

                // Fetch the HandlingUnit from the database using Entity Framework (EF)
                var handlingUnit = await db.HandlingUnits
                    .FirstOrDefaultAsync(h => h.SSCC == SSCC);

                // Check if handling unit exists
                if (handlingUnit == null)
                {
                    return Ok("False"); // Return False if handling unit is not found
                }

                int CurrentWaypointId = Convert.ToInt32(db.Devices.Where(d => d.Description == WaypointId).Select(d => d.Waypoint).FirstOrDefault());
                var CurrentWaypoint = db.Waypoints.Where(wp => wp.Id == CurrentWaypointId).FirstOrDefault();
                var NextWaypoint = db.Waypoints.Where(wp => wp.Id == CurrentWaypoint.ToWaypoint).FirstOrDefault();

                // Update the status to "Accepted"
                handlingUnit.Status = "Accepted";
                if (CurrentWaypoint.Type != "PAS")
                {
                    handlingUnit.Status = "Accepted at " + CurrentWaypoint.Description;
                }
                handlingUnit.IsUploaded = false;
                db.Entry(handlingUnit).State = EntityState.Modified;

                

                // Update HU Movement
                var HandlingUnitMovement = new HandlingUnitMovement
                {
                    SSCC = SSCC,
                    Device = WaypointId,
                    MovementTime = DateTime.Now,
                    CreatedBy = UserId,
                    Type = CurrentWaypoint.Type,
                    Status = "Accepted",
                    Horse = "",
                    Source = CurrentWaypoint.Description,
                    Destination = NextWaypoint.Description,
                    Waypoint = CurrentWaypoint.Id.ToString(),
                    RejectID = "",
                };

                db.HandlingUnitMovements.Add(HandlingUnitMovement);

                // Save the changes to the database
                int changes = await db.SaveChangesAsync();

                if (changes > 0)
                {
                    return Ok("True"); // Return True if update was successful
                }
                else
                {
                    return Ok("False"); // Return False if update failed (no changes saved)
                }
            }
            catch (Exception ex)
            {
                // Return False if an error occurred
                Console.WriteLine(ex.ToString());
                return Ok("False");
            }
        }

        private async Task<bool> ConfirmHandlingUnitInternal(string SSCC, string WaypointId, string UserId)
        {
            if (string.IsNullOrEmpty(SSCC))
                return false;

            var handlingUnit = await db.HandlingUnits.FirstOrDefaultAsync(h => h.SSCC == SSCC);
            if (handlingUnit == null)
                return false;

            var CurrentWaypoint = db.Waypoints.Find(Convert.ToInt32(WaypointId));
            var NextWaypoint = db.Waypoints.FirstOrDefault(wp => wp.Id == CurrentWaypoint.ToWaypoint);

            handlingUnit.Status = "To " + NextWaypoint.Description;
            handlingUnit.IsUploaded = false;
            db.Entry(handlingUnit).State = EntityState.Modified;

            var HandlingUnitMovement = new HandlingUnitMovement
            {
                SSCC = SSCC,
                Device = WaypointId,
                MovementTime = DateTime.Now,
                CreatedBy = UserId,
                Type = CurrentWaypoint.Type,
                Status = "To " + NextWaypoint.Description,
                Horse = "",
                Source = CurrentWaypoint.Description,
                Destination = NextWaypoint.Description,
                Waypoint = CurrentWaypoint.Id.ToString(),
                RejectID = ""
            };

            db.HandlingUnitMovements.Add(HandlingUnitMovement);

            int changes = await db.SaveChangesAsync();
            return changes > 0;
        }


        // GET: api/APIHandlingUnits/ConfirmHandlingUnit/SSCC/WaypointId/UserId
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/ConfirmHandlingUnits/{SSCC}/{WaypointId}/{UserId}")]
        public async Task<IHttpActionResult> ConfirmHandlingUnits(string SSCC, string WaypointId, string UserId)
        {
            try
            {
                // Validate if the SSCC is in the correct format or exists in the database
                if (string.IsNullOrEmpty(SSCC))
                {
                    return Ok("False"); // Return False if SSCC is invalid
                }

                // Fetch the HandlingUnit from the database
                var handlingUnit = await db.HandlingUnits
                    .FirstOrDefaultAsync(h => h.SSCC == SSCC);

                // Check if handling unit exists
                if (handlingUnit == null)
                {
                    return Ok("False"); // Return False if handling unit is not found
                }

                // Update the status to Next Waypoint
                int CurrentWaypointId = Convert.ToInt32(db.Devices.Where(d => d.Description == WaypointId).Select(d => d.Waypoint).FirstOrDefault());
                var CurrentWaypoint = db.Waypoints.Where(wp => wp.Id == CurrentWaypointId).FirstOrDefault();
                var NextWaypoint = db.Waypoints.Where(wp => wp.Id == CurrentWaypoint.ToWaypoint).FirstOrDefault();
                
                handlingUnit.Status = "To " + NextWaypoint.Description;
                handlingUnit.IsUploaded = false;
                db.Entry(handlingUnit).State = EntityState.Modified;

                // Update HU Movement
                var HandlingUnitMovement = new HandlingUnitMovement
                {
                    SSCC = SSCC,
                    Device = WaypointId,
                    MovementTime = DateTime.Now,
                    CreatedBy = UserId,
                    Type = CurrentWaypoint.Type,
                    Status = "To " + NextWaypoint.Description,
                    Horse = "",
                    Source = CurrentWaypoint.Description,
                    Destination = NextWaypoint.Description,
                    Waypoint = CurrentWaypoint.Id.ToString(),
                    RejectID = ""
                };

                db.HandlingUnitMovements.Add(HandlingUnitMovement);
                int changes = await db.SaveChangesAsync();


                var iWaypoint = db.Waypoints
                            .Where(w => w.ToWaypoint == CurrentWaypoint.Id)
                            .FirstOrDefault();

                if (iWaypoint?.VariableWaypoint == true)
                {
                    while (iWaypoint != null && iWaypoint.Status != "Closed")
                    {
                        iWaypoint = db.Waypoints
                            .Where(w => w.ToWaypoint == CurrentWaypoint.Id && w.Status == "Closed")
                            .FirstOrDefault();

                        if (iWaypoint == null)
                        {
                            Console.WriteLine("No closed waypoint found!");
                            break;
                        }
                    }

                    if (iWaypoint != null)
                    {
                        if (db.HandlingUnits.Where(h => h.Status == "Accepted at " + CurrentWaypoint.Description || h.Status == "To " + CurrentWaypoint.Description)
                       .AsNoTracking().Count() == 0)
                        {
                            iWaypoint.Status = "Open";
                            db.Entry(iWaypoint).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                }

                

                if (changes > 0)
                {
                    return Ok("True"); 
                }
                else
                {
                    return Ok("False"); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Ok("False");
            }
        }

        // GET: api/APIHandlingUnits/RejectHandlingUnit/SSCC/WaypointId/UserId
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/RejectHandlingUnit/{SSCC}/{WaypointId}/{UserId}/{RejectId}")]
        public async Task<IHttpActionResult> RejectHandlingUnit(string SSCC, string WaypointId, string UserId, string RejectId)
        {
            try
            {
                // Validate if the SSCC is in the correct format or exists in the database
                if (string.IsNullOrEmpty(SSCC))
                {
                    return Ok("False"); // Return False if SSCC is invalid
                }

                // Fetch the HandlingUnit from the database using Entity Framework (EF)
                var handlingUnit = await db.HandlingUnits
                    .FirstOrDefaultAsync(h => h.SSCC == SSCC);

                // Check if handling unit exists
                if (handlingUnit == null)
                {
                    return Ok("False"); // Return False if handling unit is not found
                }

                // Update the status to "Rejected"
                handlingUnit.Status = "Rejected " + RejectId;
                handlingUnit.IsUploaded = false;
                db.Entry(handlingUnit).State = EntityState.Modified;

                int CurrentWaypointId = Convert.ToInt32(db.Devices.Where(d => d.Description == WaypointId).Select(d => d.Waypoint).FirstOrDefault());
                var CurrentWaypoint = db.Waypoints.Where(wp => wp.Id == CurrentWaypointId).FirstOrDefault();

                // Update HU Movement
                var HandlingUnitMovement = new HandlingUnitMovement
                {
                    SSCC = SSCC,
                    Device = WaypointId,
                    MovementTime = DateTime.Now,
                    CreatedBy = UserId,
                    Type = CurrentWaypoint.Type,
                    Status = "Rejected",
                    Horse = "",
                    Source = CurrentWaypoint.Description,
                    Destination = "Rejects",
                    Waypoint = CurrentWaypoint.Id.ToString(),
                    RejectID = RejectId
                };

                db.HandlingUnitMovements.Add(HandlingUnitMovement);


                // Save the changes to the database
                int changes = await db.SaveChangesAsync();

                if (changes > 0)
                {
                    return Ok("True"); // Return True if update was successful
                }
                else
                {
                    return Ok("False"); // Return False if update failed (no changes saved)
                }
            }
            catch (Exception ex)
            {
                // Return False if an error occurred
                Console.WriteLine(ex.ToString());
                return Ok("False");
            }
        }

        // GET: api/APIHandlingUnits/NewHandlingUnit/ScannedValue/WaypointId/UserId
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/GetZPL/{waypointID}/{SSCC}")]
        public async Task<string> GetZPL(string waypointID, string SSCC)
        {
            string newLabel = functionsController.GenerateZPL(waypointID, SSCC);
            return newLabel;
        }

        // GET: api/APIHandlingUnits/NewHandlingUnit/ScannedValue/WaypointId/UserId
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/NewHandlingUnit/{ScannedValue}/{WaypointId}/{UserId}")]
        public async Task<IHttpActionResult> GetNewHandlingUnit(string ScannedValue, string WaypointId, string UserId)
        {
            int counter = 0;
            bool result = false;
            string NextSSCC = string.Empty;

            if (!db.SAPMaterials
                .Where(sm => sm.GTIN_CON == ScannedValue ||
                             sm.GTIN_HU == ScannedValue ||
                             sm.GTIN_Level1 == ScannedValue ||
                             sm.GTIN_Level2 == ScannedValue)
                .Any())
            {
                return NotFound();
            }

            while (counter < 3)
            {
                counter++;

                // Call the function to get the result (it could be true, false, or a 20-digit string in string format)
                NextSSCC = await functionsController.GenerateSSCC(WaypointId);

                // Try to parse the response as a boolean
                if (bool.TryParse(NextSSCC, out bool booleanResult))
                {
                    result = booleanResult;
                    if (result)
                    {
                        // If result is true, loop one more time
                        continue;
                    }
                }

                // Exit the loop if none of the conditions are met
                break;
            }  
            
            Console.WriteLine(NextSSCC);

            var newHandlingUnit = await functionsController.CreateHandlingUnit(ScannedValue, WaypointId, UserId, NextSSCC);            
            
            string description = db.SAPMaterials
                .Where(sm => sm.MATNR == newHandlingUnit.MATNR)
                .Select(sm => sm.MAKTX)
                .FirstOrDefault();

            string newLabel = functionsController.GenerateZPL(WaypointId,NextSSCC);

            // Get Labelary Image
            byte[] imageBytes = await GetLabelImageAsync(newLabel);

            // Save to a file (Testing only)
            // System.IO.File.WriteAllBytes("c:\\Transfer\\label.png", imageBytes);

            string base64Image = ConvertToBase64(imageBytes);

            // Construct response object
            var response = new
            {
                HandlingUnit = newHandlingUnit,                
                Product = description,
                Label = newLabel,
                Base64Image = base64Image
            };

            return Ok(response);
        }

        public async Task<byte[]> GetLabelImageAsync(string zpl)
        {
            using (var client = new HttpClient())
            {

                client.Timeout = TimeSpan.FromMilliseconds(2000);

                // Clear headers and set correct Accept header
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));

                // Convert ZPL to ASCII bytes
                byte[] zplBytes = Encoding.ASCII.GetBytes(zpl);

                using (var content = new ByteArrayContent(zplBytes))
                {
                    // Explicit Content-Type for Labelary
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    try
                    {
                        // Post request to Labelary
                        HttpResponseMessage response = await client.PostAsync(LabelaryUrl, content);
                    

                        if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsByteArrayAsync();
                        }
                        else
                        {
                            Console.WriteLine($"Error: {response.StatusCode}");
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("Request timed out.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Content\Images\NoImage.png");

                    return File.ReadAllBytes(imagePath);
                }
            }
        }


        static string ConvertToBase64(byte[] imageData)
        {
            return Convert.ToBase64String(imageData);
        }

        // PUT: api/APIHandlingUnits/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutHandlingUnit(string id, HandlingUnit handlingUnit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != handlingUnit.SSCC)
            {
                return BadRequest();
            }

            db.Entry(handlingUnit).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HandlingUnitExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/APIHandlingUnits
        [ResponseType(typeof(HandlingUnit))]
        public IHttpActionResult PostHandlingUnit(HandlingUnit handlingUnit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.HandlingUnits.Add(handlingUnit);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (HandlingUnitExists(handlingUnit.SSCC))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = handlingUnit.SSCC }, handlingUnit);
        }

        // DELETE: api/APIHandlingUnits/5
        [ResponseType(typeof(HandlingUnit))]
        public IHttpActionResult DeleteHandlingUnit(string id)
        {
            HandlingUnit handlingUnit = db.HandlingUnits.Find(id);
            if (handlingUnit == null)
            {
                return NotFound();
            }

            db.HandlingUnits.Remove(handlingUnit);
            db.SaveChanges();

            return Ok(handlingUnit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool HandlingUnitExists(string id)
        {
            return db.HandlingUnits.Count(e => e.SSCC == id) > 0;
        }
    }
}