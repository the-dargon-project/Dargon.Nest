using System;
using Dargon.Nest.Daemon.Hatchlings;
using Dargon.Nest.Daemon.Updating;
using Dargon.Nest.Eggs;
using Dargon.Nest.Eggxecutor;
using Fody.Constructors;
using Nito.AsyncEx;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ItzWarty.Collections;

namespace Dargon.Nest.Daemon {
   public class ExeggutorSynchronization {
      private readonly AsyncReaderWriterLock multinestAccessLock = new AsyncReaderWriterLock();

      public Task<IDisposable> TakeSharedLockAsync() => multinestAccessLock.ReaderLockAsync();
      public Task<IDisposable> TakeSharedLockAsync(CancellationToken token) => multinestAccessLock.ReaderLockAsync(token);

      public Task<IDisposable> TakeExclusiveLockAsync() => multinestAccessLock.WriterLockAsync();
      public Task<IDisposable> TakeExclusiveLockAsync(CancellationToken token) => multinestAccessLock.WriterLockAsync(token);
   }
}
