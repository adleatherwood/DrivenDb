using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using DrivenDb.Language.Base;

namespace DrivenDb.Base
{
   internal class DbAsyncAccessor : DbAccessor, IDbAsyncAccessor
   {
      public DbAsyncAccessor(IDbScripter scripter, IDbMapper mapper, IDb db)
         : base(scripter, mapper, db)
      {
      }

      internal async Task ExecuteAsync(DbConnection connection, DbTransaction transaction, string query, params object[] parameters)
      {
         using (var command = connection.CreateCommand())
         {
            if (connection.State != ConnectionState.Open)
            {
               connection.Open();
            }

            command.CommandTimeout = CommandTimeout;
            command.Transaction = transaction;

            m_Scripter.ScriptExecute(command, query, parameters);

            LogMessage(command.CommandText);

            await command.ExecuteNonQueryAsync();
         }
      }

      public async Task ExecuteAsync(string query, params object[] parameters)
      {
         using (var connection = GetAsyncableConnection())
         using (var command = connection.CreateCommand())
         {
            if (connection.State != ConnectionState.Open)
            {
               connection.Open();
            }

            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptExecute(command, query, parameters);

            LogMessage(command.CommandText);

            await command.ExecuteNonQueryAsync();
         }
      }

      public async Task<IEnumerable<T>> ReadValuesAsync<T>(string query, params object[] parameters)
      {
         using (var connection = GetAsyncableConnection())
         {
            connection.Open();

            return await ReadValuesAsync<T>(connection, null, query, parameters);
         }
      }

      public async Task<IEnumerable<T>> ReadValuesAsync<T>(DbConnection connection, DbTransaction transaction, string query, params object[] parameters)
      {
         using (var command = connection.CreateCommand())
         {
            if (transaction != null)
            {
               command.Transaction = transaction;
            }

            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               return m_Mapper.MapValues<T>(reader);
            }
         }
      }

      public async Task<T> ReadValueAsync<T>(string query, params object[] parameters)
      {
         using (var connection = GetAsyncableConnection())
         {
            connection.Open();

            return await ReadValueAsync<T>(connection, null, query, parameters);
         }
      }

      public async Task<T> ReadValueAsync<T>(DbConnection connection, DbTransaction transaction, string query, params object[] parameters)
      {
         using (var command = connection.CreateCommand())
         {
            if (transaction != null)
            {
               command.Transaction = transaction;
            }

            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               return m_Mapper.MapValue<T>(reader);
            }
         }
      }

      public async Task<IEnumerable<T>> ReadAnonymousAsync<T>(T model, string query, params object[] parameters)
      {
         using (var connection = GetAsyncableConnection())
         {
            connection.Open();

            return await ReadAnonymousAsync(connection, null, model, query, parameters);
         }
      }

      public async Task<IEnumerable<T>> ReadAnonymousAsync<T>(DbConnection connection, DbTransaction transaction, T model, string query, params object[] parameters)
      {
         using (var command = connection.CreateCommand())
         {
            if (transaction != null)
            {
               command.Transaction = transaction;
            }

            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               return m_Mapper.MapAnonymous(model, command.CommandText, reader);
            }
         }
      }

      public async Task<IEnumerable<T>> ReadTypeAsync<T>(string query, params object[] parameters)
         where T : new()
      {
         using (var connection = GetAsyncableConnection())
         {
            connection.Open();

            return await ReadTypeAsync<T>(connection, null, query, parameters);
         }
      }

      public async Task<IEnumerable<T>> ReadTypeAsync<T>(DbConnection connection, DbTransaction transaction, string query, params object[] parameters)
         where T : new()
      {
         using (var command = connection.CreateCommand())
         {
            if (transaction != null)
            {
               command.Transaction = transaction;
            }

            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               return m_Mapper.MapType<T>(command.CommandText, reader);
            }
         }
      }

      public async Task<DbSet<T1, T2>> ReadTypeAsync<T1, T2>(string query, params object[] parameters)
         where T1 : new()
         where T2 : new()
      {
         using (var connection = GetAsyncableConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               var set1 = m_Mapper.MapType<T1>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 1);
               var set2 = m_Mapper.MapType<T2>(command.CommandText, reader);

               return new DbSet<T1, T2>(set1, set2);
            }
         }
      }

      public async Task<DbSet<T1, T2, T3>> ReadTypeAsync<T1, T2, T3>(string query, params object[] parameters)
         where T1 : new()
         where T2 : new()
         where T3 : new()
      {
         using (var connection = GetAsyncableConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               var set1 = m_Mapper.MapType<T1>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 1);
               var set2 = m_Mapper.MapType<T2>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 2);
               var set3 = m_Mapper.MapType<T3>(command.CommandText, reader);

               return new DbSet<T1, T2, T3>(set1, set2, set3);
            }
         }
      }

      public async Task<DbSet<T1, T2, T3, T4>> ReadTypeAsync<T1, T2, T3, T4>(string query, params object[] parameters)
         where T1 : new()
         where T2 : new()
         where T3 : new()
         where T4 : new()
      {
         using (var connection = GetAsyncableConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               var set1 = m_Mapper.MapType<T1>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 1);
               var set2 = m_Mapper.MapType<T2>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 2);
               var set3 = m_Mapper.MapType<T3>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 3);
               var set4 = m_Mapper.MapType<T4>(command.CommandText, reader);

               return new DbSet<T1, T2, T3, T4>(set1, set2, set3, set4);
            }
         }
      }

      public async Task<DbSet<T1, T2, T3, T4, T5>> ReadTypeAsync<T1, T2, T3, T4, T5>(string query, params object[] parameters)
         where T1 : new()
         where T2 : new()
         where T3 : new()
         where T4 : new()
         where T5 : new()
      {
         using (var connection = GetAsyncableConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               var set1 = m_Mapper.MapType<T1>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 1);
               var set2 = m_Mapper.MapType<T2>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 2);
               var set3 = m_Mapper.MapType<T3>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 3);
               var set4 = m_Mapper.MapType<T4>(command.CommandText, reader);
               if (!reader.NextResult()) throw new MissingResultException(5, 4);
               var set5 = m_Mapper.MapType<T5>(command.CommandText, reader);

               return new DbSet<T1, T2, T3, T4, T5>(set1, set2, set3, set4, set5);
            }
         }
      }

      public async Task<IEnumerable<T>> ReadTypeAsync<T>(Func<T> factory, string query, params object[] parameters)
      {
         using (var connection = GetAsyncableConnection())
         {
            connection.Open();

            return await ReadTypeAsync(connection, null, factory, query, parameters);
         }
      }

      public async Task<IEnumerable<T>> ReadTypeAsync<T>(DbConnection connection, DbTransaction transaction, Func<T> factory, string query, params object[] parameters)
      {
         using (var command = connection.CreateCommand())
         {
            if (transaction != null)
            {
               command.Transaction = transaction;
            }

            command.CommandTimeout = CommandTimeout;

            m_Scripter.ScriptSelect(command, query, parameters);

            LogMessage(command.CommandText);

            using (var reader = await command.ExecuteReaderAsync())
            {
               return m_Mapper.MapType(command.CommandText, reader, factory);
            }
         }
      }

      public IDbAsyncScope CreateAsyncScope()
      {
         return new DbAsyncScope(m_Db, this);
      }

      private DbConnection GetAsyncableConnection()
      {
         var connection = (DbConnection) m_Db.CreateConnection();

         return connection;
      }
   }
}
