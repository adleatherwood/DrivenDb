using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using DrivenDb.Base;

namespace DrivenDb.Language.Base
{
   internal class DbAsyncScope : DbScope, IDbAsyncScope
   {
      protected new readonly DbConnection m_Connection;
      protected new readonly DbAsyncAccessor m_Accessor;
      protected new readonly DbTransaction m_Transaction;

      internal DbAsyncScope(IDb db, DbAsyncAccessor accessor)
         : base(db, accessor)
      {
         m_Connection = (DbConnection)db.CreateConnection();
         m_Accessor = accessor;

         m_Connection.Open();
         m_Transaction = m_Connection.BeginTransaction();
      }

      public async Task<IEnumerable<T>> ReadValuesAsync<T>(string query, params object[] parameters)
      {
         return await m_Accessor.ReadValuesAsync<T>(m_Connection, m_Transaction, query, parameters);
      }

      public async Task<T> ReadValueAsync<T>(string query, params object[] parameters)
      {
         return await m_Accessor.ReadValueAsync<T>(m_Connection, m_Transaction, query, parameters);
      }

      public async Task<IEnumerable<T>> ReadAnonymousAsync<T>(T model, string query, params object[] parameters)
      {
         return await m_Accessor.ReadAnonymousAsync(m_Connection, m_Transaction, model, query, parameters);
      }

      public async Task<IEnumerable<T>> ReadTypeAsync<T>(Func<T> factory, string query, params object[] parameters)
      {
         return await m_Accessor.ReadTypeAsync(m_Connection, m_Transaction, factory, query, parameters);
      }

      public async Task<IEnumerable<T>> ReadTypeAsync<T>(string query, params object[] parameters) where T : new()
      {
         return await m_Accessor.ReadTypeAsync<T>(m_Connection, m_Transaction, query, parameters);
      }

      public async Task ExecuteAsync(string query, params object[] parameters)
      {
         await m_Accessor.ExecuteAsync(m_Connection, m_Transaction, query, parameters);
      }
   }
}
