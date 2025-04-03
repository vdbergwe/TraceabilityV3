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
    public class APIDevicesController : ApiController
    {
        private TraceabilityEntities db = new TraceabilityEntities();

        // GET: api/APIDevices
        public IQueryable<Device> GetDevices()
        {
            return db.Devices;
        }

        // GET: api/APIDevices/5        
        [ResponseType(typeof(Device))]
        public async Task<IHttpActionResult> GetDevice(string Description)
        {
            Device device = await db.Devices.FirstOrDefaultAsync(a => a.Description == Description);
            if (device == null)
            {
                return Ok();
            }

            return Ok(device);
        }

        [HttpPost]
        [ResponseType(typeof(void))]
        [Route("api/APIDevices/UpdateDevice")]
        public async Task<IHttpActionResult> UpdateDevice([FromUri] string DeviceName, [FromUri] string PrinterType, [FromUri] string PrinterIP)
        { 
            var device = await db.Devices.Where(d => d.Description == DeviceName).FirstOrDefaultAsync();

            device.PrinterIP = PrinterIP;
            device.PrinterType = PrinterType;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Entry(device).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(device.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // PUT: api/APIDevices/5
        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutDevice(int Id, Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (Id != device.Id)
            {
                return BadRequest();
            }

            db.Entry(device).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(device.Id))
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

        // POST: api/APIDevices
        [ResponseType(typeof(Device))]
        public async Task<IHttpActionResult> PostDevice(Device device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Devices.Add(device);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = device.Id }, device);
        }

        // DELETE: api/APIDevices/5
        [ResponseType(typeof(Device))]
        public async Task<IHttpActionResult> DeleteDevice(int id)
        {
            Device device = await db.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            db.Devices.Remove(device);
            await db.SaveChangesAsync();

            return Ok(device);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DeviceExists(int id)
        {
            return db.Devices.Count(e => e.Id == id) > 0;
        }
    }
}