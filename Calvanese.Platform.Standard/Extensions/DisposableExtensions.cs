using System;
using System.Collections.Generic;
using System.Linq;

namespace Calvanese.Platform.WPF.Extensions {
    public static class DisposableExtensions {
        #region Methods (Disposal)

        public static void DisposeAll(this IEnumerable<IDisposable> disposables) {
            // Prepare a list of exceptions.
            List<Exception>? exceptions = null;

            // Dispose of all disposables.
            foreach (IDisposable disposable in disposables) {
                try {
                    // Dispose.
                    disposable.Dispose();
                }
                catch (Exception exception) {
                    // Create the list of exceptions.
                    exceptions ??= new List<Exception>();

                    // Add the exception.
                    exceptions.Add(exception);
                }
            }

            // Throw the aggregate of the exceptions.
            if (exceptions?.Any() == true) throw new AggregateException(exceptions);
        }

        #endregion
    }
}