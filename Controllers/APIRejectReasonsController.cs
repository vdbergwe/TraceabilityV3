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
    public class APIRejectReasonsController : ApiController
    {
        private TraceabilityEntities db = new TraceabilityEntities();

        // GET: api/APIRejectReasons
        public IQueryable<RejectReason> GetRejectReasons()
        {
            return db.RejectReasons;
        }

        // GET: api/APIRejectReasons/5
        [ResponseType(typeof(RejectReason))]
        public async Task<IHttpActionResult> GetRejectReason(string id)
        {
            RejectReason rejectReason = await db.RejectReasons.FindAsync(id);
            if (rejectReason == null)
            {
                return NotFound();
            }

            return Ok(rejectReason);
        }

        // PUT: api/APIRejectReasons/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutRejectReason(string id, RejectReason rejectReason)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != rejectReason.ReasonID)
            {
                return BadRequest();
            }

            db.Entry(rejectReason).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RejectReasonExists(id))
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

        // POST: api/APIRejectReasons
        [ResponseType(typeof(RejectReason))]
        public async Task<IHttpActionResult> PostRejectReason(RejectReason rejectReason)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.RejectReasons.Add(rejectReason);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (RejectReasonExists(rejectReason.ReasonID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = rejectReason.ReasonID }, rejectReason);
        }

        // DELETE: api/APIRejectReasons/5
        [ResponseType(typeof(RejectReason))]
        public async Task<IHttpActionResult> DeleteRejectReason(string id)
        {
            RejectReason rejectReason = await db.RejectReasons.FindAsync(id);
            if (rejectReason == null)
            {
                return NotFound();
            }

            db.RejectReasons.Remove(rejectReason);
            await db.SaveChangesAsync();

            return Ok(rejectReason);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool RejectReasonExists(string id)
        {
            return db.RejectReasons.Count(e => e.ReasonID == id) > 0;
        }
    }
}