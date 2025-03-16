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
using PagedList;
using System.Data.SqlClient;
using System.Data.Entity.Validation;

namespace TraceabilityV3.Controllers
{
    public class SAPMaterialsController : Controller
    {
        private TraceabilityEntities db = new TraceabilityEntities();
        private zzSAPIntegrationTSH_WBEntities db2 = new zzSAPIntegrationTSH_WBEntities();

        // GET: SAPMaterials
        public async Task<ActionResult> Index(int page = 1,
                                              int pageSize = 10,
                                              string sortOrder = null,
                                              string sortDirection = null,
                                              string SYSID = null,
                                              string MANDT = null,
                                              string WERKS = null,
                                              string MATNR = null,
                                              string MAKTX = null,
                                              string GTIN_CON = null,
                                              string GTIN_HU = null,
                                              string NetWt_HU = null,
                                              string HUQty_CON = null,
                                              string SAPUOM_CON = null,
                                              string SAPUOM_Base = null,
                                              string ProductType = null,
                                              int MaxRecords = 100)
        {

            // Track the start time
            var startTime = DateTime.Now;

            // Retrieve data from SQL Database
            var Materials = db.SAPMaterials.AsQueryable();           

            // Filter Logic (AND logic applies)
            if (!string.IsNullOrEmpty(SYSID))
                Materials = Materials.Where(h => h.SYSID.Contains(SYSID));
            if (!string.IsNullOrEmpty(MANDT))
                Materials = Materials.Where(h => h.MANDT.Contains(MANDT));
            if (!string.IsNullOrEmpty(WERKS))
                Materials = Materials.Where(h => h.WERKS.Contains(WERKS));
            if (!string.IsNullOrEmpty(MATNR))
                Materials = Materials.Where(h => h.MATNR.Contains(MATNR));
            if (!string.IsNullOrEmpty(MAKTX))
                Materials = Materials.Where(h => h.MAKTX.Contains(MAKTX));
            if (!string.IsNullOrEmpty(GTIN_CON))
                Materials = Materials.Where(h => h.GTIN_CON.Contains(GTIN_CON));
            if (!string.IsNullOrEmpty(GTIN_HU))
                Materials = Materials.Where(h => h.GTIN_HU.Contains(GTIN_HU));
            if (!string.IsNullOrEmpty(NetWt_HU))
                Materials = Materials.Where(h => h.NetWt_HU.ToString().Contains(NetWt_HU));
            if (!string.IsNullOrEmpty(HUQty_CON))
                Materials = Materials.Where(h => h.HUQty_CON.ToString().Contains(HUQty_CON));
            if (!string.IsNullOrEmpty(SAPUOM_CON))
                Materials = Materials.Where(h => h.SAPUOM_CON.Contains(SAPUOM_CON));
            if (!string.IsNullOrEmpty(SAPUOM_Base))
                Materials = Materials.Where(h => h.SAPUOM_Base.Contains(SAPUOM_Base));
            if (!string.IsNullOrEmpty(ProductType))
                Materials = Materials.Where(h => h.ProductType.Contains(ProductType));


            // Take Maximum Records
            Materials = Materials.Take(MaxRecords);

            // Sort the data
            switch (sortOrder)
            {
                case "SYSID":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.SYSID)
                        : Materials.OrderByDescending(h => h.SYSID);
                    break;
                case "MANDT":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.MANDT)
                        : Materials.OrderByDescending(h => h.MANDT);
                    break;
                case "WERKS":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.WERKS)
                        : Materials.OrderByDescending(h => h.WERKS);
                    break;
                case "MATNR":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.MATNR)
                        : Materials.OrderByDescending(h => h.MATNR);
                    break;
                case "MAKTX":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.MAKTX)
                        : Materials.OrderByDescending(h => h.MAKTX);
                    break;
                case "GTIN_CON":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.GTIN_CON)
                        : Materials.OrderByDescending(h => h.GTIN_CON);
                    break;
                case "GTIN_HU":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.GTIN_HU)
                        : Materials.OrderByDescending(h => h.GTIN_HU);
                    break;
                case "NetWt_HU":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.NetWt_HU)
                        : Materials.OrderByDescending(h => h.NetWt_HU);
                    break;
                case "HUQty_CON":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.HUQty_CON)
                        : Materials.OrderByDescending(h => h.HUQty_CON);
                    break;
                case "SAPUOM_CON":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.SAPUOM_CON)
                        : Materials.OrderByDescending(h => h.SAPUOM_CON);
                    break;
                case "SAPUOM_Base":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.SAPUOM_Base)
                        : Materials.OrderByDescending(h => h.SAPUOM_Base);
                    break;
                case "ProductType":
                    Materials = sortDirection == "asc"
                        ? Materials.OrderBy(h => h.ProductType)
                        : Materials.OrderByDescending(h => h.ProductType);
                    break;
                default:
                    Materials = Materials.OrderBy(h => h.MATNR);
                    break;
            }

            // Paginate the data
            var pagedMaterials = Materials.ToPagedList(page, pageSize);

            //Count total Records
            ViewBag.ResultCount = Materials.Count();

            // Track the end time and calculate duration
            var endTime = DateTime.Now;
            var timeTaken = endTime - startTime;

            // Pass the time taken to the ViewBag
            ViewBag.TimeTaken = timeTaken.TotalMilliseconds;

            // Return the view with the data
            ViewBag.SortOrder = sortOrder;
            ViewBag.SortDirection = sortDirection;


            return View(pagedMaterials);
        }

        public void ImportSAPMaterials()
        {          
                // Fetch all records from the source table
                var sourceMaterials = db2.zTraceIn_SAPMaterials.ToList();

                foreach (var sourceMaterial in sourceMaterials)
                {
                    // Check if the material exists in the destination database
                    var destinationMaterial = db.SAPMaterials
                        .FirstOrDefault(m => m.SYSID == sourceMaterial.SYSID &&
                                             m.MANDT == sourceMaterial.MANDT &&
                                             m.WERKS == sourceMaterial.WERKS &&
                                             m.MATNR == sourceMaterial.MATNR);

                    if (destinationMaterial != null)
                    {
                        // Check if the column `Locked` is true
                        if (destinationMaterial.Locked ?? false) 
                        {
                            // Skip import for locked rows
                            continue;
                        }

                        // Check for changes and update
                        if (!AreMaterialsEqual(destinationMaterial, sourceMaterial))
                        {
                            UpdateMaterial(destinationMaterial, sourceMaterial);
                            destinationMaterial.DateLastUpdated = DateTime.Now;
                        }
                    }
                    else
                    {
                        // Insert new material
                        var newMaterial = CloneMaterial(sourceMaterial);
                        newMaterial.DateFirstAdded = DateTime.Now;
                        db.SAPMaterials.Add(newMaterial);
                    }
                }

                // Save changes to the destination database
                db.SaveChanges();            
        }

        private bool AreMaterialsEqual(SAPMaterial dest, zTraceIn_SAPMaterials source)
        {
            // Compare all fields to check for changes
            return dest.MAKTX == source.MAKTX &&
                   dest.GTIN_CON == source.GTIN_CON &&
                   dest.GTIN_HU == source.GTIN_HU &&
                   dest.GTIN_Level1 == source.GTIN_Level1 &&
                   dest.GTIN_Level2 == source.GTIN_Level2 &&
                   dest.NetWt_CON == source.NetWt_CON &&
                   dest.NetWt_HU == source.NetWt_HU &&
                   dest.NetWt_Level1 == source.NetWt_Level1 &&
                   dest.NetWt_Level2 == source.NetWt_Level2 &&
                   dest.TareWt_CON == source.TareWt_CON &&
                   dest.TareWt_HU == source.TareWt_HU &&
                   dest.TareWt_Level1 == source.TareWt_Level1 &&
                   dest.TareWt_Level2 == source.TareWt_Level2 &&
                   dest.HUQty_CON == source.HUQty_CON &&
                   dest.HUQty_Level1 == source.HUQty_Level1 &&
                   dest.HUQty_Level2 == source.HUQty_Level2 &&
                   dest.SAPUOM_CON == source.SAPUOM_CON &&
                   dest.SAPUOM_HU == source.SAPUOM_HU &&
                   dest.SAPUOM_Level1 == source.SAPUOM_Level1 &&
                   dest.SAPUOM_Level2 == source.SAPUOM_Level2 &&
                   dest.SAPUOM_Base == source.SAPUOM_Base &&
                   dest.SAPUOM_Sales == source.SAPUOM_Sales &&
                   dest.QCUOM == source.QCUOM &&
                   dest.WtCatUOM == source.WtCatUOM &&
                   dest.ProductType == source.ProductType &&
                   dest.ProductWtCat == source.ProductWtCat &&
                   dest.BulkFlag == source.BulkFlag &&
                   dest.Retired == source.Retired &&
                   dest.RetirementDate == source.RetirementDate &&
                   dest.GTIN_SalesUOM == source.GTIN_SalesUOM;
        }

        private void UpdateMaterial(SAPMaterial dest, zTraceIn_SAPMaterials source)
        {
            dest.MAKTX = source.MAKTX;
            dest.GTIN_CON = source.GTIN_CON;
            dest.GTIN_HU = source.GTIN_HU;
            dest.GTIN_Level1 = source.GTIN_Level1;
            dest.GTIN_Level2 = source.GTIN_Level2;
            dest.NetWt_CON = source.NetWt_CON;
            dest.NetWt_HU = source.NetWt_HU;
            dest.NetWt_Level1 = source.NetWt_Level1;
            dest.NetWt_Level2 = source.NetWt_Level2;
            dest.TareWt_CON = source.TareWt_CON;
            dest.TareWt_HU = source.TareWt_HU;
            dest.TareWt_Level1 = source.TareWt_Level1;
            dest.TareWt_Level2 = source.TareWt_Level2;
            dest.HUQty_CON = source.HUQty_CON;
            dest.HUQty_Level1 = source.HUQty_Level1;
            dest.HUQty_Level2 = source.HUQty_Level2;
            dest.SAPUOM_CON = source.SAPUOM_CON;
            dest.SAPUOM_HU = source.SAPUOM_HU;
            dest.SAPUOM_Level1 = source.SAPUOM_Level1;
            dest.SAPUOM_Level2 = source.SAPUOM_Level2;
            dest.SAPUOM_Base = source.SAPUOM_Base;
            dest.SAPUOM_Sales = source.SAPUOM_Sales;
            dest.QCUOM = source.QCUOM;
            dest.WtCatUOM = source.WtCatUOM;
            dest.ProductType = source.ProductType;
            dest.ProductWtCat = source.ProductWtCat;
            dest.BulkFlag = source.BulkFlag;
            dest.Retired = source.Retired;
            dest.RetirementDate = source.RetirementDate;
            dest.GTIN_SalesUOM = source.GTIN_SalesUOM;
        }

        private SAPMaterial CloneMaterial(zTraceIn_SAPMaterials source)
        {
            return new SAPMaterial
            {
                SYSID = source.SYSID,
                MANDT = source.MANDT,
                WERKS = source.WERKS,
                MATNR = source.MATNR,
                MAKTX = source.MAKTX,
                GTIN_CON = source.GTIN_CON,
                GTIN_HU = source.GTIN_HU,
                GTIN_Level1 = source.GTIN_Level1,
                GTIN_Level2 = source.GTIN_Level2,
                NetWt_CON = source.NetWt_CON,
                NetWt_HU = source.NetWt_HU,
                NetWt_Level1 = source.NetWt_Level1,
                NetWt_Level2 = source.NetWt_Level2,
                TareWt_CON = source.TareWt_CON,
                TareWt_HU = source.TareWt_HU,
                TareWt_Level1 = source.TareWt_Level1,
                TareWt_Level2 = source.TareWt_Level2,
                HUQty_CON = source.HUQty_CON,
                HUQty_Level1 = source.HUQty_Level1,
                HUQty_Level2 = source.HUQty_Level2,
                SAPUOM_CON = source.SAPUOM_CON,
                SAPUOM_HU = source.SAPUOM_HU,
                SAPUOM_Level1 = source.SAPUOM_Level1,
                SAPUOM_Level2 = source.SAPUOM_Level2,
                SAPUOM_Base = source.SAPUOM_Base,
                SAPUOM_Sales = source.SAPUOM_Sales,
                QCUOM = source.QCUOM,
                WtCatUOM = source.WtCatUOM,
                ProductType = source.ProductType,
                ProductWtCat = source.ProductWtCat,
                BulkFlag = source.BulkFlag,
                Retired = source.Retired,
                RetirementDate = source.RetirementDate,
                GTIN_SalesUOM = source.GTIN_SalesUOM
            };
        }

            // GET: SAPMaterials/Details/5
            public async Task<ActionResult> Details(string SYSID, string MANDT, string WERKS, string MATNR)
        {
            if (SYSID == null || MANDT == null || WERKS == null || MATNR == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SAPMaterial sAPMaterial = await db.SAPMaterials.FindAsync(SYSID, MANDT, WERKS, MATNR);
            if (sAPMaterial == null)
            {
                return HttpNotFound();
            }
            return View(sAPMaterial);
        }

        // GET: SAPMaterials/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SAPMaterials/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SYSID,MANDT,WERKS,MATNR,MAKTX,GTIN_CON,GTIN_HU,GTIN_Level1,GTIN_Level2,NetWt_CON,NetWt_HU,NetWt_Level1,NetWt_Level2,TareWt_CON,TareWt_HU,TareWt_Level1,TareWt_Level2,HUQty_CON,HUQty_Level1,HUQty_Level2,SAPUOM_CON,SAPUOM_HU,SAPUOM_Level1,SAPUOM_Level2,SAPUOM_Base,SAPUOM_Sales,QCUOM,WtCatUOM,ProductType,ProductWtCat,BulkFlag,DateFirstAdded,DateLastUpdated,ReadStatus,Retired,RetirementDate,GTIN_SalesUOM,Locked")] SAPMaterial sAPMaterial)
        {
            if (ModelState.IsValid)
            {
                db.SAPMaterials.Add(sAPMaterial);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(sAPMaterial);
        }

        // GET: SAPMaterials/Edit/5
        public async Task<ActionResult> Edit(string SYSID, string MANDT, string WERKS, string MATNR)
        {
            if (SYSID == null || MANDT == null || WERKS == null || MATNR == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SAPMaterial sAPMaterial = await db.SAPMaterials.FindAsync(SYSID,MANDT,WERKS, MATNR);
            if (sAPMaterial == null)
            {
                return HttpNotFound();
            }
            return View(sAPMaterial);
        }

        // POST: SAPMaterials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SYSID,MANDT,WERKS,MATNR,MAKTX,GTIN_CON,GTIN_HU,GTIN_Level1,GTIN_Level2,NetWt_CON,NetWt_HU,NetWt_Level1,NetWt_Level2,TareWt_CON,TareWt_HU,TareWt_Level1,TareWt_Level2,HUQty_CON,HUQty_Level1,HUQty_Level2,SAPUOM_CON,SAPUOM_HU,SAPUOM_Level1,SAPUOM_Level2,SAPUOM_Base,SAPUOM_Sales,QCUOM,WtCatUOM,ProductType,ProductWtCat,BulkFlag,DateFirstAdded,DateLastUpdated,ReadStatus,Retired,RetirementDate,GTIN_SalesUOM,Locked")] SAPMaterial sAPMaterial)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(sAPMaterial).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    // Log validation errors
                    foreach (var validationError in ex.EntityValidationErrors)
                    {
                        foreach (var error in validationError.ValidationErrors)
                        {
                            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        }
                    }

                    // Optionally log the full exception (e.g., using a logging framework like Serilog or NLog)
                    Console.WriteLine(ex);

                    // Provide feedback to the user
                    ViewBag.ErrorMessage = "An error occurred while saving the changes. Please review the form for errors.";
                }
                catch (Exception ex)
                {
                    // Log the generic exception
                    Console.WriteLine(ex);

                    // Provide feedback to the user
                    ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                }
            }

            // Return the view with the current data and any validation messages
            return View(sAPMaterial);
        }

        // GET: SAPMaterials/Delete/5
        public async Task<ActionResult> Delete(string SYSID, string MANDT, string WERKS, string MATNR)
        {
            if (SYSID == null || MANDT == null || WERKS == null || MATNR == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SAPMaterial sAPMaterial = await db.SAPMaterials.FindAsync(SYSID, MANDT, WERKS, MATNR);
            if (sAPMaterial == null)
            {
                return HttpNotFound();
            }
            return View(sAPMaterial);
        }

        // POST: SAPMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string SYSID, string MANDT, string WERKS, string MATNR)
        {
            SAPMaterial sAPMaterial = await db.SAPMaterials.FindAsync(SYSID, MANDT, WERKS, MATNR);
            db.SAPMaterials.Remove(sAPMaterial);
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
