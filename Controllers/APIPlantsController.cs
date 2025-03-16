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
    public class APIPlantsController : ApiController
    {
        private TraceabilityEntities db = new TraceabilityEntities();

        // GET: api/APIPlants
        public IQueryable<Plant> GetPlants()
        {
            return db.Plants;
        }

        // GET: api/APIPlants/5
        [ResponseType(typeof(Plant))]
        public async Task<IHttpActionResult> GetPlant(string id)
        {
            Plant plant = await db.Plants.FindAsync(id);
            if (plant == null)
            {
                return NotFound();
            }

            return Ok(plant);
        }

        // PUT: api/APIPlants/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutPlant(string id, Plant plant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != plant.Werks)
            {
                return BadRequest();
            }

            db.Entry(plant).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlantExists(id))
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

        // POST: api/APIPlants
        [ResponseType(typeof(Plant))]
        public async Task<IHttpActionResult> PostPlant(Plant plant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Plants.Add(plant);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PlantExists(plant.Werks))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = plant.Werks }, plant);
        }

        // DELETE: api/APIPlants/5
        [ResponseType(typeof(Plant))]
        public async Task<IHttpActionResult> DeletePlant(string id)
        {
            Plant plant = await db.Plants.FindAsync(id);
            if (plant == null)
            {
                return NotFound();
            }

            db.Plants.Remove(plant);
            await db.SaveChangesAsync();

            return Ok(plant);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PlantExists(string id)
        {
            return db.Plants.Count(e => e.Werks == id) > 0;
        }
    }
}