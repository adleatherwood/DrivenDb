using System;
using System.Data;
using DrivenDb.Base;
using DrivenDb.MsSql;

namespace DrivenDb
{
   public static class DbAsyncFactory
   {
      public static IDbAsyncAccessor CreateAccessor(DbAccessorType type, IDb db)
      {
         var setup = DbFactory.GetSetup(type, db);

         if (type == DbAccessorType.MsSql)
         {
            return new MsSqlAsyncAccessor((IMsSqlScripter)setup.Scripter, setup.Mapper, db);
         }

         return new DbAsyncAccessor(setup.Scripter, setup.Mapper, db);
      }

      public static IDbAsyncAccessor CreateAccessor(DbAccessorType type, AccessorExtension extensions, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, extensions, connections));
      }
   }
}
