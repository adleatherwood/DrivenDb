using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace DrivenDb
{
   public interface IDbAsyncAccessor : IDbAccessor
   {
      Task ExecuteAsync(string query, params object[] parameters);
      Task<IEnumerable<T>> ReadValuesAsync<T>(string query, params object[] parameters);
      Task<IEnumerable<T>> ReadValuesAsync<T>(DbConnection connection, DbTransaction transaction, string query, params object[] parameters);
      Task<T> ReadValueAsync<T>(string query, params object[] parameters);
      Task<T> ReadValueAsync<T>(DbConnection connection, DbTransaction transaction, string query, params object[] parameters);
      Task<IEnumerable<T>> ReadAnonymousAsync<T>(T model, string query, params object[] parameters);
      Task<IEnumerable<T>> ReadAnonymousAsync<T>(DbConnection connection, DbTransaction transaction, T model, string query, params object[] parameters);

      Task<IEnumerable<T>> ReadTypeAsync<T>(string query, params object[] parameters)
         where T : new();

      Task<IEnumerable<T>> ReadTypeAsync<T>(DbConnection connection, DbTransaction transaction, string query, params object[] parameters)
         where T : new();

      Task<DbSet<T1, T2>> ReadTypeAsync<T1, T2>(string query, params object[] parameters)
         where T1 : new()
         where T2 : new();

      Task<DbSet<T1, T2, T3>> ReadTypeAsync<T1, T2, T3>(string query, params object[] parameters)
         where T1 : new()
         where T2 : new()
         where T3 : new();

      Task<DbSet<T1, T2, T3, T4>> ReadTypeAsync<T1, T2, T3, T4>(string query, params object[] parameters)
         where T1 : new()
         where T2 : new()
         where T3 : new()
         where T4 : new();

      Task<DbSet<T1, T2, T3, T4, T5>> ReadTypeAsync<T1, T2, T3, T4, T5>(string query, params object[] parameters)
         where T1 : new()
         where T2 : new()
         where T3 : new()
         where T4 : new()
         where T5 : new();

      Task<IEnumerable<T>> ReadTypeAsync<T>(Func<T> factory, string query, params object[] parameters);
      Task<IEnumerable<T>> ReadTypeAsync<T>(DbConnection connection, DbTransaction transaction, Func<T> factory, string query, params object[] parameters);
      IDbAsyncScope CreateAsyncScope();
   }
}
