using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TraceabilityV3;
using System.Web.UI;
using PagedList;
using TraceabilityV3.Models;


namespace TraceabilityV3.Controllers
{
    public class HandlingUnitsController : Controller
    {
        private TraceabilityEntities db = new TraceabilityEntities();
        private OMD_DatawarehouseEntities db2 = new OMD_DatawarehouseEntities();

        // GET: HandlingUnits
        public async Task<ActionResult> Index(int page = 1, 
                                              int pageSize = 10,                  
                                              string sortOrder = null, 
                                              string sortDirection = null, 
                                              string SSCC = null,
                                              string WERKS = null,
                                              string MATNR = null,
                                              string ScannedCode = null,
                                              string CreatedFrom = null,
                                              string CreatedTo = null,
                                              string CreatedBy = null,
                                              string Batch = null,
                                              string Horse = null,
                                              string Status = null,
                                              int MaxRecords = 100)
        {
            // Track the start time
            var startTime = DateTime.Now;

            // Default Values where required
            if (string.IsNullOrEmpty(CreatedTo))
            {
                CreatedTo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (string.IsNullOrEmpty(CreatedFrom))
            {
                CreatedFrom = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "SSCC";
            }

            if (string.IsNullOrEmpty(sortDirection))
            {
                sortDirection = "asc";
            }

            // Convert string to DateTime for comparison
            DateTime? createdFromDate = string.IsNullOrEmpty(CreatedFrom) ? (DateTime?)null : DateTime.Parse(CreatedFrom);
            DateTime? createdToDate = string.IsNullOrEmpty(CreatedTo) ? (DateTime?)null : DateTime.Parse(CreatedTo);

            // Retrieve data from SQL Database
            var handlingUnits = db.HandlingUnits.AsQueryable();

            // Filter data on datetime values
            handlingUnits = handlingUnits.Where(h =>
                       (!createdFromDate.HasValue || h.Created >= createdFromDate) &&
                       (!createdToDate.HasValue || h.Created <= createdToDate));

            // Filter Logic (AND logic applies)
            if (!string.IsNullOrEmpty(SSCC))
                handlingUnits = handlingUnits.Where(h => h.SSCC.Contains(SSCC));
            if (!string.IsNullOrEmpty(WERKS))
                handlingUnits = handlingUnits.Where(h => h.WERKS.Contains(WERKS));
            if (!string.IsNullOrEmpty(MATNR))
                handlingUnits = handlingUnits.Where(h => h.MATNR.Contains(MATNR));
            if (!string.IsNullOrEmpty(ScannedCode))
                handlingUnits = handlingUnits.Where(h => h.ScannedCode.Contains(ScannedCode));
            if (!string.IsNullOrEmpty(CreatedBy))
                handlingUnits = handlingUnits.Where(h => h.CreatedBy.Contains(CreatedBy));
            if (!string.IsNullOrEmpty(Batch))
                handlingUnits = handlingUnits.Where(h => h.Batch.ToString().Contains(Batch));
            if (!string.IsNullOrEmpty(Horse))
                handlingUnits = handlingUnits.Where(h => h.Horse.Contains(Horse));
            if (!string.IsNullOrEmpty(Status))
                handlingUnits = handlingUnits.Where(h => h.Status.Contains(Status));

            // Take Maximum Records
            handlingUnits = handlingUnits.Take(MaxRecords);

            // Sort the data
            switch (sortOrder)
            {
                case "SSCC":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.SSCC)
                        : handlingUnits.OrderByDescending(h => h.SSCC);
                    break;
                case "WERKS":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.WERKS)
                        : handlingUnits.OrderByDescending(h => h.WERKS);
                    break;
                case "MATNR":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.MATNR)
                        : handlingUnits.OrderByDescending(h => h.MATNR);
                    break;
                case "ScannedCode":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.ScannedCode)
                        : handlingUnits.OrderByDescending(h => h.ScannedCode);
                    break;
                case "Created":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.Created)
                        : handlingUnits.OrderByDescending(h => h.Created);
                    break;
                case "CreatedBy":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.CreatedBy)
                        : handlingUnits.OrderByDescending(h => h.CreatedBy);
                    break;
                case "Batch":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.Batch)
                        : handlingUnits.OrderByDescending(h => h.Batch);
                    break;
                case "Horse":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.Horse)
                        : handlingUnits.OrderByDescending(h => h.Horse);
                    break;
                case "Status":
                    handlingUnits = sortDirection == "asc"
                        ? handlingUnits.OrderBy(h => h.Status)
                        : handlingUnits.OrderByDescending(h => h.Status);
                    break;
                default:
                    handlingUnits = handlingUnits.OrderBy(h => h.SSCC);
                    break;
            }

            // Paginate the data
            var pagedHandlingUnits = handlingUnits.ToPagedList(page, pageSize);

            //Count total Records
            ViewBag.ResultCount = handlingUnits.Count();

            // Track the end time and calculate duration
            var endTime = DateTime.Now;
            var timeTaken = endTime - startTime;

            // Pass the time taken to the ViewBag
            ViewBag.TimeTaken = timeTaken.TotalMilliseconds;

            // Return the view with the data
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDirection = sortDirection;
            ViewBag.CreatedFrom = CreatedFrom;
            ViewBag.CreatedTo = CreatedTo;
            return View(pagedHandlingUnits);
        }

        public ActionResult RejectReason(string id)
        {
            string reason;
            string reasonId = db.HandlingUnits
                .Where(a => a.SSCC == id)
                .Select(a => a.Status)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(reasonId) && reasonId.Length >= 2)
            {
                reasonId = reasonId.Substring(reasonId.Length - 2);
                reason = db.RejectReasons
                    .Where(f => f.ReasonID == reasonId)
                    .Select(f => f.Name)
                    .FirstOrDefault();
            }
            else
            {
                reason = "";
            }

            ViewBag.RejectReason = reason;
            return PartialView();
        }

        public ActionResult ProductionReport(string FromTime, string ToTime, bool Detail = false)
        {
            DateTime? fromTimeValue = null;
            DateTime? toTimeValue = null;

            if (string.IsNullOrEmpty(FromTime) || string.IsNullOrEmpty(ToTime))
            {
                var now = DateTime.Now.TimeOfDay;
                if (now >= new TimeSpan(8, 30, 1) && now <= new TimeSpan(16, 30, 0))
                {
                    fromTimeValue = DateTime.Today.Add(new TimeSpan(8, 0, 0));
                    toTimeValue = DateTime.Today.Add(new TimeSpan(15, 59, 59));
                }
                else if ((now >= new TimeSpan(16, 30, 1) && now <= new TimeSpan(23, 59, 59)) || now <= new TimeSpan(0, 30, 0))
                {
                    fromTimeValue = DateTime.Today.Add(new TimeSpan(16, 0, 0));
                    toTimeValue = DateTime.Today.Add(new TimeSpan(23, 59, 59));
                }
                else if (now >= new TimeSpan(0, 30, 1) && now <= new TimeSpan(8, 30, 0))
                {
                    fromTimeValue = DateTime.Today.Add(new TimeSpan(0, 0, 0));
                    toTimeValue = DateTime.Today.Add(new TimeSpan(7, 59, 59));
                }
            }
            else
            {
                fromTimeValue = DateTime.Parse(FromTime);
                toTimeValue = DateTime.Parse(ToTime);
            }

            // Fetch handling unit history with selected fields only.
            var handlingUnitsHistory = db.HandlingUnits
                                    .Where(a => a.Created >= fromTimeValue && a.Created <= toTimeValue)
                                    .Join(
                                        db.SAPMaterials, // No filtering inside Join
                                        hu => new { hu.MATNR, hu.WERKS },  // Joining on both MATNR and WERKS
                                        sm => new { sm.MATNR, sm.WERKS },  // Ensuring WERKS match
                                        (hu, sm) => new
                                        {
                                            hu.SSCC,
                                            hu.Created,
                                            hu.MATNR,
                                            hu.Status,
                                            Description = sm.MAKTX
                                        })
                                    .OrderByDescending(a => a.Created)
                                    .AsNoTracking()
                                    .ToList();


            var handlingUnitSSCCs = handlingUnitsHistory.Select(a => a.SSCC).ToList();

            // Fetch Handling Units Movements
            var handling_units_ToSap = (from hu in db.HandlingUnitMovements
                                        join huHist in db.HandlingUnits on hu.SSCC equals huHist.SSCC
                                        where huHist.Created >= fromTimeValue &&
                                              huHist.Created <= toTimeValue &&
                                              hu.Status.Contains("Warehouse")
                                        select new
                                        {
                                            hu.SSCC,
                                            hu.MovementTime
                                        })
                                        .AsNoTracking()
                                        .ToList();

            // Filter: select handling unit SSCCs from movements where the created time is after toTimeValue.
            var remove_units = handling_units_ToSap
                .Where(a => a.MovementTime >= toTimeValue)
                .Select(a => a.SSCC)
                .ToHashSet();

            // Remove units from history.
            var updated_handling_Units_history = handlingUnitsHistory
                .Where(a => !remove_units.Contains(a.SSCC))
                .ToList();

            // Fetch all Product data for the distinct products in the updated history.
            var productIds = updated_handling_Units_history.Select(a => a.MATNR).Distinct().ToList();
            var productDetails = db.SAPMaterials
                .Where(p => productIds.Contains(p.MATNR) && p.WERKS == "1110")
                .Select(p => new
                {
                    p.SYSID,
                    p.MANDT,
                    p.WERKS,
                    p.MATNR,
                    p.MAKTX,
                    p.NetWt_HU
                })
                .AsNoTracking()
                .ToDictionary(p => p.MATNR);

            // Group data and build view models.
            var handlingUnitsData = updated_handling_Units_history
                .GroupBy(a => a.MATNR)
                .Select(g => new HandlingUnitsViewModel
                {
                    ProductId = Convert.ToInt32(g.Key),
                    PLU = productDetails.ContainsKey(g.Key) ? productDetails[g.Key].MATNR : "",
                    Description = productDetails.ContainsKey(g.Key) ? productDetails[g.Key].MAKTX : "",
                    TotalScanned = g.Count(),
                    TotalRejected = g.Count(a => a.Status.Contains("Rejected")),
                    TotalPending = g.Count(a => !a.Status.Contains("Rejected") && !a.Status.Contains("Warehouse")),
                    TotalAccepted = g.Count(a => a.Status.Contains("Warehouse")),
                    TotalKG = Convert.ToInt32(g.Count(a => !a.Status.Contains("Rejected")) *
                               (productDetails.ContainsKey(g.Key) ? productDetails[g.Key].NetWt_HU : 0) / 1000),
                    ProductItems = g.Select(a => new HandlingUnitsItemViewModel
                    {
                        SSCC = a.SSCC,
                        Created = a.Created,
                        BatchDescription = a.Description,
                        RejectReason = a.Status.Contains("Rejected") ? "Rejected" : ""
                    }).ToList()
                })
                .ToList();

            // Prepare ViewBag data.
            ViewBag.ViewModel = handlingUnitsData.OrderBy(a => a.PLU);
            ViewBag.TotalScanned = handlingUnitsData.Sum(a => a.TotalScanned);
            ViewBag.TotalRejected = handlingUnitsData.Sum(a => a.TotalRejected);
            ViewBag.TotalAccepted = handlingUnitsData.Sum(a => a.TotalAccepted);
            ViewBag.TotalPending = handlingUnitsData.Sum(a => a.TotalPending);
            ViewBag.TotalWeight = Convert.ToInt32(handlingUnitsData.Sum(h => h.TotalKG));
            ViewBag.Products = updated_handling_Units_history.Select(a => a.MATNR).Distinct();
            ViewBag.ToTime = toTimeValue.Value.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.FromTime = fromTimeValue.Value.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.DisplayDetail = Detail.ToString();

            return PartialView(handlingUnitsData);
        }

        public ActionResult BinReport(string FromTime, string ToTime, bool Detail = false)
        {
            DateTime? fromTimeValue = null;
            DateTime? toTimeValue = null;

            if (string.IsNullOrEmpty(FromTime) || string.IsNullOrEmpty(ToTime))
            {
                var now = DateTime.Now.TimeOfDay;
                if (now >= new TimeSpan(8, 30, 1) && now <= new TimeSpan(16, 30, 0))
                {
                    fromTimeValue = DateTime.Today.Add(new TimeSpan(8, 0, 0));
                    toTimeValue = DateTime.Today.Add(new TimeSpan(15, 59, 59));
                }
                else if ((now >= new TimeSpan(16, 30, 1) && now <= new TimeSpan(23, 59, 59)) || now <= new TimeSpan(0, 30, 0))
                {
                    fromTimeValue = DateTime.Today.Add(new TimeSpan(16, 0, 0));
                    toTimeValue = DateTime.Today.Add(new TimeSpan(23, 59, 59));
                }
                else if (now >= new TimeSpan(0, 30, 1) && now <= new TimeSpan(8, 30, 0))
                {
                    fromTimeValue = DateTime.Today.Add(new TimeSpan(0, 0, 0));
                    toTimeValue = DateTime.Today.Add(new TimeSpan(7, 59, 59));
                }
            }
            else
            {
                fromTimeValue = DateTime.Parse(FromTime);
                toTimeValue = DateTime.Parse(ToTime);
            }

            // Fetch handling unit history with selected fields only.
            var handlingUnitsHistory = db.HandlingUnits
                                    .Where(a => a.Created >= fromTimeValue && a.Created <= toTimeValue)
                                    .Join(
                                        db.SAPMaterials, // No filtering inside Join
                                        hu => new { hu.MATNR, hu.WERKS },  // Joining on both MATNR and WERKS
                                        sm => new { sm.MATNR, sm.WERKS },  // Ensuring WERKS match
                                        (hu, sm) => new
                                        {
                                            hu.SSCC,
                                            hu.Created,
                                            hu.MATNR,
                                            hu.Status,
                                            Description = sm.MAKTX
                                        })
                                    .OrderByDescending(a => a.Created)
                                    .AsNoTracking()
                                    .ToList();


            var handlingUnitSSCCs = handlingUnitsHistory.Select(a => a.SSCC).ToList();

            // Fetch Handling Units Movements
            var handling_units_ToSap = (from hu in db.HandlingUnitMovements
                                        join huHist in db.HandlingUnits on hu.SSCC equals huHist.SSCC
                                        where huHist.Created >= fromTimeValue &&
                                              huHist.Created <= toTimeValue &&
                                              hu.Status.Contains("Warehouse")
                                        select new
                                        {
                                            hu.SSCC,
                                            hu.MovementTime
                                        })
                                        .AsNoTracking()
                                        .ToList();

            // Filter: select handling unit SSCCs from movements where the created time is after toTimeValue.
            var remove_units = handling_units_ToSap
                .Where(a => a.MovementTime >= toTimeValue)
                .Select(a => a.SSCC)
                .ToHashSet();

            // Remove units from history.
            var updated_handling_Units_history = handlingUnitsHistory
                .Where(a => !remove_units.Contains(a.SSCC))
                .ToList();

            // Fetch all Product data for the distinct products in the updated history.
            var productIds = updated_handling_Units_history.Select(a => a.MATNR).Distinct().ToList();
            var productDetails = db.SAPMaterials
                .Where(p => productIds.Contains(p.MATNR) && p.WERKS == "1110")
                .Select(p => new
                {
                    p.SYSID,
                    p.MANDT,
                    p.WERKS,
                    p.MATNR,
                    p.MAKTX,
                    p.NetWt_HU
                })
                .AsNoTracking()
                .ToDictionary(p => p.MATNR);

            // Group data and build view models.
            var handlingUnitsData = updated_handling_Units_history
                .GroupBy(a => a.MATNR)
                .Select(g => new HandlingUnitsViewModel
                {
                    ProductId = Convert.ToInt32(g.Key),
                    PLU = productDetails.ContainsKey(g.Key) ? productDetails[g.Key].MATNR : "",
                    Description = productDetails.ContainsKey(g.Key) ? productDetails[g.Key].MAKTX : "",
                    TotalScanned = g.Count(),
                    TotalRejected = g.Count(a => a.Status.Contains("Rejected")),
                    TotalPending = g.Count(a => !a.Status.Contains("Rejected") && !a.Status.Contains("Warehouse")),
                    TotalAccepted = g.Count(a => a.Status.Contains("Warehouse")),
                    TotalKG = Convert.ToInt32(g.Count(a => !a.Status.Contains("Rejected")) *
                               (productDetails.ContainsKey(g.Key) ? productDetails[g.Key].NetWt_HU : 0) / 1000),
                    ProductItems = g.Select(a => new HandlingUnitsItemViewModel
                    {
                        SSCC = a.SSCC,
                        Created = a.Created,
                        BatchDescription = a.Description,
                        RejectReason = a.Status.Contains("Rejected") ? "Rejected" : ""
                    }).ToList()
                })
                .ToList();

            // Prepare ViewBag data.
            ViewBag.ViewModel = handlingUnitsData.OrderBy(a => a.PLU);
            ViewBag.TotalScanned = handlingUnitsData.Sum(a => a.TotalScanned);
            ViewBag.TotalRejected = handlingUnitsData.Sum(a => a.TotalRejected);
            ViewBag.TotalAccepted = handlingUnitsData.Sum(a => a.TotalAccepted);
            ViewBag.TotalPending = handlingUnitsData.Sum(a => a.TotalPending);
            ViewBag.TotalWeight = Convert.ToInt32(handlingUnitsData.Sum(h => h.TotalKG));
            ViewBag.Products = updated_handling_Units_history.Select(a => a.MATNR).Distinct();
            ViewBag.ToTime = toTimeValue.Value.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.FromTime = fromTimeValue.Value.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.DisplayDetail = Detail.ToString();

            return PartialView(handlingUnitsData);
        }

        public void ImportHandlingUnits()
        {
            const int batchSize = 5000; // Process in chunks to manage memory and database load
            var productsFromDb2 = db2.Products
                .ToDictionary(p => p.Id, p => new { p.WERKS, p.MATNR });
            var existingSsccs = new HashSet<string>(db.HandlingUnits.Select(h => h.SSCC));

            int totalRecords = db2.Handling_Units.Count();
            int processedRecords = 0;

            while (processedRecords < totalRecords)
            {
                // Step 1: Fetch handling units in batches
                var handlingUnitsFromDb2 = db2.Handling_Units
                    .OrderBy(h => h.SSCC) // Ensure consistent ordering for pagination
                    .Skip(processedRecords)
                    .Take(batchSize)
                    .ToList();

                var handlingUnitsToInsert = new List<HandlingUnit>();

                // Step 2: Process each batch
                foreach (var unit in handlingUnitsFromDb2)
                {
                    // Skip existing SSCCs
                    if (existingSsccs.Contains(unit.SSCC))
                       continue;
                    

                    if (handlingUnitsToInsert.Where(a => a.SSCC == unit.SSCC).Count() > 0)
                        continue;

                    // Lookup WERKS and MATNR from the dictionary
                    if (unit.Product.HasValue && productsFromDb2.TryGetValue(unit.Product.Value, out var productData))
                    {
                        var handlingUnit = new HandlingUnit
                        {
                            SSCC = unit.SSCC,
                            WERKS = productData.WERKS,
                            MATNR = productData.MATNR,
                            ScannedCode = unit.ScannedCode,
                            Created = unit.Created,
                            CreatedBy = unit.CreatedBy,
                            Batch = unit.Batch.ToString(),
                            Horse = unit.Horse,
                            Status = unit.Status
                        };
                        handlingUnitsToInsert.Add(handlingUnit);
                    }
                }

                // Step 3: Insert new records in a batch
                if (handlingUnitsToInsert.Any())
                {
                    db.HandlingUnits.AddRange(handlingUnitsToInsert); 
                    db.SaveChanges();
                }

                // Update the processed record count
                processedRecords += handlingUnitsFromDb2.Count;
            }

        }



        // GET: HandlingUnits/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HandlingUnit handlingUnit = await db.HandlingUnits.FindAsync(id);
            if (handlingUnit == null)
            {
                return HttpNotFound();
            }
            return View(handlingUnit);
        }

        // GET: HandlingUnits/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HandlingUnits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SSCC,WERKS,MATNR,ScannedCode,Created,CreatedBy,Batch,Horse,Status")] HandlingUnit handlingUnit)
        {
            if (ModelState.IsValid)
            {
                db.HandlingUnits.Add(handlingUnit);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(handlingUnit);
        }

        // GET: HandlingUnits/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HandlingUnit handlingUnit = await db.HandlingUnits.FindAsync(id);
            if (handlingUnit == null)
            {
                return HttpNotFound();
            }
            return View(handlingUnit);
        }

        // POST: HandlingUnits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SSCC,WERKS,MATNR,ScannedCode,Created,CreatedBy,Batch,Horse,Status")] HandlingUnit handlingUnit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(handlingUnit).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(handlingUnit);
        }

        // GET: HandlingUnits/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HandlingUnit handlingUnit = await db.HandlingUnits.FindAsync(id);
            if (handlingUnit == null)
            {
                return HttpNotFound();
            }
            return View(handlingUnit);
        }

        // POST: HandlingUnits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            HandlingUnit handlingUnit = await db.HandlingUnits.FindAsync(id);
            db.HandlingUnits.Remove(handlingUnit);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
