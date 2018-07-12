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

using DrivenDb.Base;
using DrivenDb.Language.MsSql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DrivenDb.MsSql
{
   internal class MsSqlAccessor : DbAccessor, IMsSqlAccessor
   {
      protected new readonly IMsSqlScripter m_Scripter;
      protected new readonly IDb m_Db;

      public MsSqlAccessor(IMsSqlScripter scripter, IDbMapper mapper, IDb db)
         : base(scripter, mapper, db)
      {
         m_Scripter = scripter;
         m_Db = db;
      }

      public void WriteEntityUsingScopeIdentity<T>(T entity)
         where T : IDbEntity, new()
      {
         WriteEntitiesUsingScopeIdentity(new[] { entity });
      }

      public void WriteEntitiesUsingScopeIdentity<T>(IEnumerable<T> entities)
         where T : IDbEntity, new()
      {
         WriteEntities(null, null, entities,
                       (c, i, e) => m_Scripter.ScriptInsertWithScopeIdentity<T>(c, e, i, true)
                       , null
                       , null
                       , null
                       , true
            );
      }

      [Obsolete("Method fails if a trigger exists on the target table.")]
      public Tuple<T, D> WriteEntityAndOutputDeleted<T, D>(T entity, D deleted)
         where T : IDbEntity, new()
         where D : class
      {
         return WriteEntitiesAndOutputDeleted(new[] { entity }, deleted).First();
      }

      [Obsolete("Method fails if a trigger exists on the target table.")]
      public IEnumerable<Tuple<T, D>> WriteEntitiesAndOutputDeleted<T, D>(IEnumerable<T> entities, D deleted)
         where T : IDbEntity, new()
         where D : class
      {
         if (!(typeof(D).FullName ?? "").Contains("AnonymousType"))
         {
            throw new NotSupportedException("Only anonymous types can be used to specify deleted output");
         }

         var columns = typeof(D).GetProperties().Select(p => p.Name).ToArray();
         var output = new List<Tuple<T, D>>();
         var constructor = typeof(D).GetConstructors()[0];

         WriteEntities(null, null, entities,
                       (c, i, e) => m_Scripter.ScriptInsert(c, i, e, true),
                       (c, i, e) => m_Scripter.ScriptUpdateOutputDeleted(c, i, e, columns),
                       (c, i, e) => m_Scripter.ScriptDeleteOutputDeleted(c, i, e, columns),
                       (i, e, a) => output.Add(new Tuple<T, D>(e, (D) constructor.Invoke(a.Skip(a.Length - columns.Length).ToArray())))
                       , true
            );

         return output;
      }

      IMsSqlScope IMsSqlAccessor.CreateScope()
      {
         return new MsSqlScope(m_Db, this, m_Scripter);
      }
   }
}