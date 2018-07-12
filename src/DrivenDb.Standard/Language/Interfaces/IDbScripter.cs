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

using System.Collections.Generic;
using System.Data;

namespace DrivenDb
{
   public interface IDbScripter
   {
      void InsertWriteInitializer(IDbCommand command);

      void AppendWriteFinalizer(IDbCommand command);

      void ScriptExecute(IDbCommand command, string query, params object[] parameters);

      void ScriptIdentitySelect<T>(IDbCommand command, params object[] parameters)
         where T : IDbRecord, new();

      void ScriptArray<T>(IDbCommand command, string query, params object[] parameters)
         where T : struct;

      void ScriptSelect(IDbCommand command, string query, params object[] parameters);

      void ScriptInsert<T>(IDbCommand command, int index, T entity, bool returnId)
         where T : IDbEntity;

      void ScriptUpdate<T>(IDbCommand command, int index, T entity)
         where T : IDbEntity;

      void ScriptDelete<T>(IDbCommand command, int index, T entity)
         where T : IDbEntity;

      void ScriptRelatedSelect<P, C>(IEnumerable<P> parents, IDbCommand command, string[] pkey, string[] ckey)
         where P : IDbRecord
         where C : IDbRecord, new();
   }
}