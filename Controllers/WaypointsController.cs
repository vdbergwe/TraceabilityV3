using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TraceabilityV3;

namespace TraceabilityV3.Controllers
{
    public class WaypointsController : Controller
    {
        private TraceabilityEntities db = new TraceabilityEntities();

        // GET: Waypoints
        public ActionResult Index()
        {
            return View(db.Waypoints.ToList());
        }

        // GET: Waypoints/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waypoint waypoint = db.Waypoints.Find(id);
            if (waypoint == null)
            {
                return HttpNotFound();
            }
            return View(waypoint);
        }

        // GET: Waypoints/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Waypoints/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Description,Type,Status,UOM,LoadCapcity,HasPrinter,HasScanner,Location,ToWaypoint,VariableWaypoint")] Waypoint waypoint)
        {
            if (ModelState.IsValid)
            {
                db.Waypoints.Add(waypoint);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(waypoint);
        }

        // GET: Waypoints/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waypoint waypoint = db.Waypoints.Find(id);
            if (waypoint == null)
            {
                return HttpNotFound();
            }
            return View(waypoint);
        }

        // POST: Waypoints/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,Type,Status,UOM,LoadCapcity,HasPrinter,HasScanner,Location,ToWaypoint,VariableWaypoint")] Waypoint waypoint)
        {
            if (ModelState.IsValid)
            {
                db.Entry(waypoint).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(waypoint);
        }

        // GET: Waypoints/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Waypoint waypoint = db.Waypoints.Find(id);
            if (waypoint == null)
            {
                return HttpNotFound();
            }
            return View(waypoint);
        }

        // POST: Waypoints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Waypoint waypoint = db.Waypoints.Find(id);
            db.Waypoints.Remove(waypoint);
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
