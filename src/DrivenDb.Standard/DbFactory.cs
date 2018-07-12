/**************************************************************************************
 * Original Author : Anthony Leatherwood (adleatherwood@gmail.com)                              
 * Source Location : https://github.com/Fastlite/DrivenDb    
 *  
 * This source is subject to the Microsoft Public License.
 * Link: http://www.microsoft.com/en-us/openness/licenses.aspx
 *  
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
 **************************************************************************************/

using System;
using System.Data;
using DrivenDb.Base;
using DrivenDb.MsSql;
using DrivenDb.SqLite;
using DrivenDb.MySql;
using DrivenDb.Oracle;

namespace DrivenDb
{
   // TODO: review tests to make sure they are universal and clean up after themselves tryf
   public static class DbFactory
   {
      public static IDbAccessor CreateAccessor(DbAccessorType type, IDb db)
      {
         var setup = GetSetup(type, db);

         if (type == DbAccessorType.MsSql)
         {
            return new MsSqlAccessor((IMsSqlScripter)setup.Scripter, setup.Mapper, db);
         }

         return new DbAccessor(setup.Scripter, setup.Mapper, db);
      }

      public static IDbAccessor CreateAccessor(DbAccessorType type, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, AccessorExtension.None, connections));
      }

      public static IDbAccessor CreateAccessor(DbAccessorType type, AccessorExtension extensions, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, extensions, connections));
      }

      public static IDbAccessor CreateAccessor(Func<ISqlBuilder> builder, AccessorExtension extensions, Func<IDbConnection> connections)
      {

         var db = new Db(extensions, connections);
         
         return new DbAccessor(new DbScripter(db, builder), new DbMapper(db), db);
      }

      public static IDbAccessorSlim CreateSlimAccessor(DbAccessorType type, IDb db)
      {
         return CreateAccessor(type, db);
      }

      public static IDbAccessorSlim CreateSlimAccessor(DbAccessorType type, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, AccessorExtension.None, connections));
      }

      public static IDbAccessorSlim CreateSlimAccessor(DbAccessorType type, AccessorExtension extensions, Func<IDbConnection> connections)
      {
         return CreateAccessor(type, new Db(type, extensions, connections));
      }

      public static IDbAccessorSlim CreateSlimAccessor(Func<ISqlBuilder> builder, AccessorExtension extensions, Func<IDbConnection> connections)
      {
         var db = new Db(extensions, connections);
         
         return new DbAccessor(new DbScripter(db, builder), new DbMapper(db), new Db(extensions, connections));
      }

      internal static AccessorSetup GetSetup(DbAccessorType type, IDb db)
      {
         var mapper = new DbMapper(db);

         Func<ISqlBuilder> builders;

         switch (type)
         {
            case DbAccessorType.MsSql:
               {
                        IMsSqlBuilder msbuilders() => new MsSqlBuilder();

                        var msscripter = new MsSqlScripter(db, msbuilders);

                  return new AccessorSetup(msscripter, mapper);
               }
            case DbAccessorType.SqLite:
               {
                  builders = () => new SqLiteBuilder();
               }
               break;
            case DbAccessorType.MySql:
               {
                  builders = () => new MySqlBuilder();
               }
               break;
            case DbAccessorType.Oracle:
               {
                  builders = () => new OracleBuilder();
               }
               break;
            default:
               throw new InvalidOperationException(string.Format("Unsupported DbAccessorType value of '{0}'", type));
         }

         var scripter = new DbScripter(db, builders);

         return new AccessorSetup(scripter, mapper);
      }

      internal sealed class AccessorSetup
      {
         private readonly IDbScripter _scripter;
         private readonly IDbMapper _mapper;
         
         public AccessorSetup(IDbScripter scripter, IDbMapper mapper)
         {
            _scripter = scripter;
            _mapper = mapper;
         }

         public IDbScripter Scripter
         {
            get { return _scripter;}
         }

         public IDbMapper Mapper
         {
            get { return _mapper; }
         }
      }
   }
}