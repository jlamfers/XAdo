using System;
using XAdo.Core.Interface;

namespace XAdo.Core
{
   class Atomic : IAtomic
   {
      private readonly Func<bool> _commitHandler;
      private readonly Func<bool> _rollbackHandler;
      private bool? _committed;

      public Atomic(Func<bool> commitHandler, Func<bool> rollbackHandler)
      {
         if (commitHandler == null) throw new ArgumentNullException("commitHandler");
         if (rollbackHandler == null) throw new ArgumentNullException("rollbackHandler");
         _commitHandler = commitHandler;
         _rollbackHandler = rollbackHandler;
      }

      public bool Commit()
      {
         if (!_committed.GetValueOrDefault(true))
         {
            throw new InvalidOperationException("Atomic operation was rollbacked and cannot be committed anymore.");
         }
         _committed = true;
         return _commitHandler();
      }

      public bool Rollback()
      {
         if (_committed.GetValueOrDefault(false))
         {
            throw new InvalidOperationException("Atomic operation was committed and cannot be rollbacked anymore.");
         }
         _committed = false;
         return _rollbackHandler();
      }
   }
}
