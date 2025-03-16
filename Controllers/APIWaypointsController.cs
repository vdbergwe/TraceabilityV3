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
using TraceabilityV3;

namespace TraceabilityV3.Controllers
{
    public class APIWaypointsController : ApiController
    {
        private TraceabilityEntities db = new TraceabilityEntities();

        // GET: api/APIWaypoints
        public IQueryable<Waypoint> GetWaypoints()
        {
            return db.Waypoints;
        }

        // GET: api/APIWaypoints/5
        [ResponseType(typeof(Waypoint))]
        public async Task<IHttpActionResult> GetWaypoint(int id)
        {
            Waypoint waypoint = await db.Waypoints.FindAsync(id);
            if (waypoint == null)
            {
                return NotFound();
            }

            return Ok(waypoint);
        }

        // PUT: api/APIWaypoints/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutWaypoint(int id, Waypoint waypoint)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != waypoint.Id)
            {
                return BadRequest();
            }

            db.Entry(waypoint).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WaypointExists(id))
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

        // POST: api/APIWaypoints
        [ResponseType(typeof(Waypoint))]
        public async Task<IHttpActionResult> PostWaypoint(Waypoint waypoint)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Waypoints.Add(waypoint);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = waypoint.Id }, waypoint);
        }

        // DELETE: api/APIWaypoints/5
        [ResponseType(typeof(Waypoint))]
        public async Task<IHttpActionResult> DeleteWaypoint(int id)
        {
            Waypoint waypoint = await db.Waypoints.FindAsync(id);
            if (waypoint == null)
            {
                return NotFound();
            }

            db.Waypoints.Remove(waypoint);
            await db.SaveChangesAsync();

            return Ok(waypoint);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool WaypointExists(int id)
        {
            return db.Waypoints.Count(e => e.Id == id) > 0;
        }
    }
}