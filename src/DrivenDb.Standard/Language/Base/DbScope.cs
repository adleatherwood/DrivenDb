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
using System.Collections.Generic;
using System.Data;

namespace DrivenDb.Base
{
   internal class DbScope : IDbScope
   {
      protected readonly IDbConnection m_Connection;
      protected readonly DbAccessor m_Accessor;
      protected readonly IDbTransaction m_Transaction;

      internal DbScope(IDb db, DbAccessor accessor)
      {
         m_Connection = db.CreateConnection();
         m_Accessor = accessor;

         m_Connection.Open();
         m_Transaction = m_Connection.BeginTransaction();
      }

      public IEnumerable<T> ReadValues<T>(string query, params object[] parameters)
      {
         return m_Accessor.ReadValues<T>(m_Connection, m_Transaction, query, parameters);
      }

      public T ReadValue<T>(string query, params object[] parameters)
      {
         return m_Accessor.ReadValue<T>(m_Connection, m_Transaction, query, parameters);
      }

      public IEnumerable<T> ReadAnonymous<T>(T model, string query, params object[] parameters)
      {
         return m_Accessor.ReadAnonymous(m_Connection, m_Transaction, model, query, parameters);
      }

      public IEnumerable<T> ReadType<T>(Func<T> factory, string query, params object[] parameters)
      {
         return m_Accessor.ReadType(m_Connection, m_Transaction, factory, query, parameters);
      }

      public IEnumerable<T> ReadType<T>(string query, params object[] parameters) where T : new()
      {
         return m_Accessor.ReadType<T>(m_Connection, m_Transaction, query, parameters);
      }

      public IEnumerable<T> ReadEntities<T>(string query, params object[] parameters) where T : IDbRecord, new()
      {
         return m_Accessor.ReadEntities<T>(m_Connection, m_Transaction, query, parameters);
      }

      public void WriteEntity<T>(T entity)
         where T : IDbEntity
      {
         m_Accessor.TransactEntity(m_Connection, m_Transaction, entity, true);
      }

      public void WriteEntity<T>(T entity, bool returnId)
         where T : IDbEntity
      {
         m_Accessor.TransactEntity(m_Connection, m_Transaction, entity, returnId);
      }

      public void WriteEntities<T>(IEnumerable<T> entities)
         where T : IDbEntity
      {
         m_Accessor.TransactEntities(m_Connection, m_Transaction, entities, true);
      }

      public void WriteEntities<T>(IEnumerable<T> entities, bool returnIds)
         where T : IDbEntity
      {
         m_Accessor.TransactEntities(m_Connection, m_Transaction, entities, returnIds);
      }

      public void Execute(string query, params object[] parameters)
      {
         m_Accessor.Execute(m_Connection, m_Transaction, query, parameters);
      }

      public void Commit()
      {
         m_Transaction.Commit();
      }

      public void Dispose()
      {
         m_Transaction.Dispose();
         m_Connection.Dispose();
      }
   }
}