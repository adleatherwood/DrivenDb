using System;
using System.Collections.Generic;
using DrivenDb.Base;

namespace DrivenDb.MsSql
{
   internal class MsSqlAsyncAccessor : DbAsyncAccessor, IMsSqlAccessor
   {
      private readonly MsSqlAccessor _sqlAccessor;

      public MsSqlAsyncAccessor(IMsSqlScripter scripter, IDbMapper mapper, IDb db)
         : base(scripter, mapper, db)
      {
         _sqlAccessor = new MsSqlAccessor(scripter, mapper, db);
      }

      public void WriteEntityUsingScopeIdentity<T>(T entity) 
         where T : IDbEntity, new()
      {
         _sqlAccessor.WriteEntityUsingScopeIdentity<T>(entity);
      }

      public void WriteEntitiesUsingScopeIdentity<T>(IEnumerable<T> entities) 
         where T : IDbEntity, new()
      {
         _sqlAccessor.WriteEntitiesUsingScopeIdentity<T>(entities);
      }

      [Obsolete("Method fails if a trigger exists on the target table.")]
      public Tuple<T, D> WriteEntityAndOutputDeleted<T, D>(T entity, D deleted) 
         where T : IDbEntity, new() 
         where D : class
      {
         return _sqlAccessor.WriteEntityAndOutputDeleted(entity, deleted);
      }

      [Obsolete("Method fails if a trigger exists on the target table.")]
      public IEnumerable<Tuple<T, D>> WriteEntitiesAndOutputDeleted<T, D>(IEnumerable<T> entities, D deleted) 
         where T : IDbEntity, new() 
         where D : class
      {
         return _sqlAccessor.WriteEntitiesAndOutputDeleted(entities, deleted);
      }

      IMsSqlScope IMsSqlAccessor.CreateScope()
      {
         return ((IMsSqlAccessor) _sqlAccessor).CreateScope();
      }
   }
}
