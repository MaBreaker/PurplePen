using System;
using System.Net.Http;
using System.Threading;

namespace PurplePen.Livelox
{
    public class LiveloxApiCall<T> : IAbortable, IDisposable
    {
        public LiveloxApiRequestContext RequestContext { get; set; }
        public HttpResponseMessage Response { get; set; }
        public T Result { get; set; }
        public Exception Exception { get; set; }
        public LiveloxApiClient Client { get; set; }
        public bool TimedOut { get; private set; }

        private bool isDisposed = false;

        public Action<LiveloxApiCall<T>> Callback { get; set; }

        public bool Success => Exception == null;

        public CancellationTokenSource CancellationSource { get; set; } = new CancellationTokenSource();

        ~LiveloxApiCall()
        {
            Dispose(false);
        }

        public void Abort()
        {
            Client?.Abort();
            CancellationSource?.Cancel();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                // free managed resources
                CancellationSource?.Dispose();
                CancellationSource = null;
            }

            isDisposed = true;
        }

        // Dispose managed resources.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void RegisterTimeout(TimeSpan timeout)
        {
            CancellationSource?.CancelAfter(timeout);
        }

        // Called when the cancellation token fires due to timeout.
        public void MarkTimedOut()
        {
            TimedOut = true;
        }
    }
}
