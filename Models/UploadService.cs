using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TraceabilityV3.Models
{
    public class UploadService
    {
        private readonly TraceabilityEntities _db = new TraceabilityEntities();
        private readonly ICentralApiClient _apiClient;

        public UploadService(TraceabilityEntities db, ICentralApiClient apiClient)
        {
            _db = db;
            _apiClient = apiClient;
        }

        // Get pending records (IsUploaded == false)
        public IQueryable<HandlingUnit> GetPendingRecords()
        {
            return _db.HandlingUnits.Where(r => r.IsUploaded != true);
        }

        // Get pending records (IsUploaded == false)
        public IQueryable<HandlingUnitMovement> GetPendingMovements()
        {
            return _db.HandlingUnitMovements.Where(r => r.IsUploaded != true);
        }

        // Upload a single record
        public async Task ProcessUploadAsync(HandlingUnit record)
        {
            try
            {
                // Ensure the entity has the most up-to-date data
                await _db.Entry(record).ReloadAsync();

                await _apiClient.UploadRecord(record);  // Call to central API
                record.IsUploaded = true;

                _db.Entry(record).State = EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the error (or implement retry logic)
                System.Diagnostics.Debug.WriteLine($"Upload failed for {record.SSCC}: {ex.Message}");
            }
        }

        // Upload a single record
        public async Task ProcessMovementAsync(HandlingUnitMovement record)
        {
            try
            {
                // Ensure the entity has the most up-to-date data
                await _db.Entry(record).ReloadAsync();

                await _apiClient.UploadMovementRecord(record);  // Call to central API
                record.IsUploaded = true;

                _db.Entry(record).State = EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the error (or implement retry logic)
                System.Diagnostics.Debug.WriteLine($"Upload failed for {record.SSCC}: {ex.Message}");
            }
        }

        // Upload all pending records
        public async Task ProcessPendingRecordsAsync()
        {
            var pendingRecords = GetPendingRecords().ToList();
            foreach (var record in pendingRecords)
            {
                await ProcessUploadAsync(record);
            }
        }

        // Upload all pending movements
        public async Task ProcessPendingMovementsAsync()
        {
            var pendingMovements = GetPendingMovements().ToList();
            foreach (var record in pendingMovements)
            {
                await ProcessMovementAsync(record);
            }
        }
    }
}