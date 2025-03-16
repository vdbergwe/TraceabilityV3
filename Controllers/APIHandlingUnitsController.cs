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
                string emailApiUrl = $"https://localhost:44314/Email/SendEmail?from={from}&to={to}&subject={subject}&body={body}";


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

        // GET: api/APIHandlingUnits/PendingHandlingUnits/WaypointId
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/APIHandlingUnits/PendingHandlingUnits/{WaypointId}")]
        public async Task<IHttpActionResult> PendingHandlingUnits ( string WaypointId)
        {
            try
            {
                // Join HandlingUnits with HandlingUnitMovements and SAPMaterials based on SSCC and MATNR
                var handlingUnits = await (from hu in db.HandlingUnits
                                           join hum in db.HandlingUnitMovements on hu.SSCC equals hum.SSCC
                                           join sm in db.SAPMaterials on hu.MATNR equals sm.MATNR
                                           where (hu.Status == "Accepted" || hu.Status == "Registered")
                                                 && hum.Device == WaypointId
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

                // Update the status to "Accepted"
                handlingUnit.Status = "Accepted";
                handlingUnit.IsUploaded = false;
                db.Entry(handlingUnit).State = EntityState.Modified;

                int CurrentWaypointId = Convert.ToInt32(db.Devices.Where(d => d.Description == WaypointId).Select(d => d.Waypoint).FirstOrDefault());
                var CurrentWaypoint = db.Waypoints.Where(wp => wp.Id == CurrentWaypointId).FirstOrDefault();
                var NextWaypoint = db.Waypoints.Where(wp => wp.Id == CurrentWaypoint.ToWaypoint).FirstOrDefault();

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

                // Fetch the HandlingUnit from the database using Entity Framework (EF)
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
                handlingUnit.Status = "Rejected";
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