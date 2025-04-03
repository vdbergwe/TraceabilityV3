using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TraceabilityV3.Models
{
    public class UploadService
    {
        private readonly IDbContextFactory<TraceabilityEntities> _dbContextFactory;
        private readonly ICentralApiClient _apiClient;

        public UploadService(IDbContextFactory<TraceabilityEntities> dbContextFactory, ICentralApiClient apiClient)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        // Get pending records (IsUploaded == false)
        public async Task<List<HandlingUnit>> GetPendingRecordsAsync()
        {
            using (var db = _dbContextFactory.Create())
            {
                return await db.HandlingUnits
                    .Where(r => (r.IsUploaded ?? false) == false)
                    .ToListAsync();
            }
        }

        public async Task<List<HandlingUnitMovement>> GetPendingMovementsAsync()
        {
            using (var db = _dbContextFactory.Create())
            {
                return await db.HandlingUnitMovements
                    .Where(r => (r.IsUploaded ?? false) == false)
                    .ToListAsync();
            }
        }

        // Upload a single record
        public async Task ProcessUploadAsync(HandlingUnit record)
        {
            if (record == null) return;

            using (var db = _dbContextFactory.Create())
            {
                try
                {
                    var existingRecord = await db.HandlingUnits.FindAsync(record.SSCC);
                    if (existingRecord == null)
                    {
                        Debug.WriteLine($"Record {record.SSCC} not found in the database.");
                        return;
                    }

                    await _apiClient.UploadRecord(existingRecord); // Call to central API
                    existingRecord.IsUploaded = true;

                    db.Entry(existingRecord).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Upload failed for {record.SSCC}: {ex.Message}");
                }
            }
        }

        // Upload a single movement
        public async Task ProcessMovementAsync(HandlingUnitMovement record)
        {
            if (record == null) return;

            using (var db = _dbContextFactory.Create())
            {
                try
                {
                    var existingRecord = await db.HandlingUnitMovements.FindAsync(record.Id);
                    if (existingRecord == null)
                    {
                        Debug.WriteLine($"Movement {record.SSCC} not found in the database.");
                        return;
                    }

                    await _apiClient.UploadMovementRecord(existingRecord); // Call to central API
                    existingRecord.IsUploaded = true;

                    db.Entry(existingRecord).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Upload failed for movement {record.SSCC}: {ex.Message}");
                }
            }
        }

        // Upload all pending records
        public async Task ProcessPendingRecordsAsync()
        {
            var pendingRecords = await GetPendingRecordsAsync();
            foreach (var record in pendingRecords)
            {
                await ProcessUploadAsync(record);
            }
        }

        // Upload all pending movements
        public async Task ProcessPendingMovementsAsync()
        {
            var pendingMovements = await GetPendingMovementsAsync();
            foreach (var record in pendingMovements)
            {
                await ProcessMovementAsync(record);
            }
        }
    }
}
