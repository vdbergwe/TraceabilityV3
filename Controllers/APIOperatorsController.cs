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
    public class APIOperatorsController : ApiController
    {
        private TraceabilityEntities db = new TraceabilityEntities();

        // GET: api/APIOperators
        public IQueryable<Operator> GetOperators()
        {
            return db.Operators;
        }

        // GET: api/APIOperators/5
        [ResponseType(typeof(Operator))]
        public async Task<IHttpActionResult> GetOperator(string id)
        {
            Operator @operator = await db.Operators.FindAsync(id);
            if (@operator == null)
            {
                return NotFound();
            }

            return Ok(@operator);
        }

        // PUT: api/APIOperators/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutOperator(string id, Operator @operator)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != @operator.EmployeeId)
            {
                return BadRequest();
            }

            db.Entry(@operator).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperatorExists(id))
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

        // POST: api/APIOperators
        [ResponseType(typeof(Operator))]
        public async Task<IHttpActionResult> PostOperator(Operator @operator)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Operators.Add(@operator);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OperatorExists(@operator.EmployeeId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = @operator.EmployeeId }, @operator);
        }

        // DELETE: api/APIOperators/5
        [ResponseType(typeof(Operator))]
        public async Task<IHttpActionResult> DeleteOperator(string id)
        {
            Operator @operator = await db.Operators.FindAsync(id);
            if (@operator == null)
            {
                return NotFound();
            }

            db.Operators.Remove(@operator);
            await db.SaveChangesAsync();

            return Ok(@operator);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OperatorExists(string id)
        {
            return db.Operators.Count(e => e.EmployeeId == id) > 0;
        }
    }
}