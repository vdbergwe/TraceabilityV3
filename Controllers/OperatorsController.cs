using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TraceabilityV3;
using BCrypt.Net;

namespace TraceabilityV3.Controllers
{
    public class OperatorsController : Controller
    {
        private TraceabilityEntities db = new TraceabilityEntities();

        // GET: Operators
        public ActionResult Index(int page = 1,
                                    int pageSize = 10,
                                    string sortOrder = null,
                                    string sortDirection = null,
                                    string EmployeeId = null,
                                    string FirstName = null,
                                    string Code = null,
                                    string WaypointID = null,                                    
                                    int MaxRecords = 100)
        {
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(Operator));

            // Track the start time
            var startTime = DateTime.Now;

            // Retrieve data from SQL Database
            var Operators = db.Operators.AsQueryable();

            // Filter Logic (AND logic applies)
            if (!string.IsNullOrEmpty(EmployeeId))
                Operators = Operators.Where(h => h.EmployeeId.Contains(EmployeeId));
            if (!string.IsNullOrEmpty(FirstName))
                Operators = Operators.Where(h => h.FirstName.Contains(FirstName));
            if (!string.IsNullOrEmpty(Code))
                Operators = Operators.Where(h => h.Code.Contains(Code));
            if (!string.IsNullOrEmpty(WaypointID))
                Operators = Operators.Where(h => h.WaypointID.ToString().Contains(WaypointID));

            // Take Maximum Records
            Operators = Operators.Take(MaxRecords);

            // Sort the data
            switch (sortOrder)
            {
                case "EmployeeId":
                    Operators = sortDirection == "asc"
                        ? Operators.OrderBy(h => h.EmployeeId)
                        : Operators.OrderByDescending(h => h.EmployeeId);
                    break;
                case "FirstName":
                    Operators = sortDirection == "asc"
                        ? Operators.OrderBy(h => h.FirstName)
                        : Operators.OrderByDescending(h => h.FirstName);
                    break;
                case "Code":
                    Operators = sortDirection == "asc"
                        ? Operators.OrderBy(h => h.Code)
                        : Operators.OrderByDescending(h => h.Code);
                    break;
                case "WaypointID":
                    Operators = sortDirection == "asc"
                        ? Operators.OrderBy(h => h.WaypointID)
                        : Operators.OrderByDescending(h => h.WaypointID);
                    break;
           
          
                default:
                    Operators = Operators.OrderBy(h => h.EmployeeId);
                    break;
            }

            // Paginate the data
            var pagedMaterials = Operators.ToPagedList(page, pageSize);

            //Count total Operators
            ViewBag.ResultCount = Operators.Count();

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

        // GET: Operators/Details/5
        public ActionResult Details(string EmployeeId)
        {
            if (EmployeeId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Operator @operator = db.Operators.Find(EmployeeId);
            if (@operator == null)
            {
                return HttpNotFound();
            }
            return View(@operator);
        }

        // GET: Operators/Create
        public ActionResult Create()
        {
            var waypoints = db.Waypoints.Select(w => new {w.Id, w.Description}).ToList();

            ViewBag.WaypointList = new SelectList(waypoints, "Id", "Description");

            return View();
        }

        // POST: Operators/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmployeeId,FirstName,Code,WaypointID")] Operator @operator)
        {
            @operator.Code = BCrypt.Net.BCrypt.HashPassword(@operator.Code + "salt");
            if (ModelState.IsValid)
            {
                db.Operators.Add(@operator);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(@operator);
        }

        // GET: Operators/Edit/5
        public ActionResult Edit(string EmployeeId)
        {
            if (EmployeeId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Operator @operator = db.Operators.Find(EmployeeId);
            if (@operator == null)
            {
                return HttpNotFound();
            }
            return View(@operator);
        }

        // POST: Operators/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmployeeId,FirstName,Code,WaypointID")] Operator @operator)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@operator).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(@operator);
        }

        // GET: Operators/Delete/5
        public ActionResult Delete(string EmployeeId)
        {
            if (EmployeeId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Operator @operator = db.Operators.Find(EmployeeId);
            if (@operator == null)
            {
                return HttpNotFound();
            }
            return View(@operator);
        }

        // POST: Operators/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string EmployeeId)
        {
            Operator @operator = db.Operators.Find(EmployeeId);
            db.Operators.Remove(@operator);
            db.SaveChanges();
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
