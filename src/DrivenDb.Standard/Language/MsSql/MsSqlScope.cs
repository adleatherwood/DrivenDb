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
using DrivenDb.MsSql;
using System.Collections.Generic;

namespace DrivenDb.Language.MsSql
{
   internal sealed class MsSqlScope : DbScope, IMsSqlScope
   {
      private readonly IMsSqlScripter m_Scripter;

      internal MsSqlScope(IDb db, MsSqlAccessor accessor, IMsSqlScripter scripter)
         : base(db, accessor)
      {
         m_Scripter = scripter;
      }

      public void WriteEntityUsingScopeIdentity<T>(T entity) where T : IDbEntity, new()
      {
         WriteEntitiesUsingScopeIdentity(new[] { entity });
      }

      public void WriteEntitiesUsingScopeIdentity<T>(IEnumerable<T> entities) where T : IDbEntity, new()
      {
         m_Accessor.WriteEntities(m_Connection, m_Transaction, entities,
                       (c, i, e) => m_Scripter.ScriptInsertWithScopeIdentity<T>(c, e, i, true)
                       , null
                       , null
                       , null
                       , true
            );
      }
   }
}