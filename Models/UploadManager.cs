using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;

namespace TraceabilityV3.Models
{
    public class UploadManager : IDisposable
    {
        private readonly UploadService _uploadService;
        private readonly Subject<Unit> _newRecordSubject = new Subject<Unit>();
        private readonly Subject<Unit> _newMovementSubject = new Subject<Unit>();
        private readonly IDisposable _mergedSubscription;

        public UploadManager(UploadService uploadService)
        {
            _uploadService = uploadService;

            // Observable that ticks every minute
            var timerObservable = Observable.Interval(TimeSpan.FromMinutes(1))
                                            .Select(_ => Unit.Default);

            // Merge timer with manual triggers
            var uploadTrigger = Observable.Merge(timerObservable, _newRecordSubject, _newMovementSubject);


            // Subscribe to triggers (runs on a background thread)
            _mergedSubscription = uploadTrigger
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(async _ =>
                {
                    await _uploadService.ProcessPendingRecordsAsync();
                    await _uploadService.ProcessPendingMovementsAsync();
                });
        }

        // Trigger upload manually when new data is added
        public void NotifyNewRecord()
        {
            _newRecordSubject.OnNext(Unit.Default);
        }

        // Trigger upload manually for movements
        public void NotifyNewMovement()
        {
            _newMovementSubject.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _mergedSubscription.Dispose();
            _newRecordSubject.Dispose();
            _newMovementSubject.Dispose();
        }

    }
}