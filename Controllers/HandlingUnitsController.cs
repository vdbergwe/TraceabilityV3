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
