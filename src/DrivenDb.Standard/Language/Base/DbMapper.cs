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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrivenDb.Utility;

namespace DrivenDb.Base
{
   internal class DbMapper : IDbMapper
   {      
#region --- STATIC --------------------------------------------------------------------------

      private static readonly Dictionary<RuntimeTypeHandle, MethodInfo> m_Methods;
      private static readonly MethodInfo m_IsDbNull = typeof (IDataRecord).GetMethod("IsDBNull");
      private static readonly MethodInfo m_GetValue = typeof (IDataRecord).GetMethod("GetValue");

      static DbMapper()
      {
         m_Methods = new Dictionary<RuntimeTypeHandle, MethodInfo>();

         // non-nullable
         m_Methods[typeof (char).TypeHandle] = typeof (IDataRecord).GetMethod("GetChar");
         m_Methods[typeof (byte).TypeHandle] = typeof (IDataRecord).GetMethod("GetByte");
         m_Methods[typeof (byte[]).TypeHandle] = typeof(IDataRecord).GetMethod("GetBytes");
         m_Methods[typeof (short).TypeHandle] = typeof (IDataRecord).GetMethod("GetInt16");
         m_Methods[typeof (int).TypeHandle] = typeof (IDataRecord).GetMethod("GetInt32");
         m_Methods[typeof (long).TypeHandle] = typeof (IDataRecord).GetMethod("GetInt64");
         m_Methods[typeof (float).TypeHandle] = typeof (IDataRecord).GetMethod("GetFloat");
         m_Methods[typeof (double).TypeHandle] = typeof (IDataRecord).GetMethod("GetDouble");
         m_Methods[typeof (decimal).TypeHandle] = typeof (IDataRecord).GetMethod("GetDecimal");
         m_Methods[typeof (bool).TypeHandle] = typeof (IDataRecord).GetMethod("GetBoolean");
         m_Methods[typeof (string).TypeHandle] = typeof (IDataRecord).GetMethod("GetString");
         m_Methods[typeof (Guid).TypeHandle] = typeof (IDataRecord).GetMethod("GetGuid");
         m_Methods[typeof (DateTime).TypeHandle] = typeof (IDataRecord).GetMethod("GetDateTime");
         m_Methods[typeof (TimeSpan).TypeHandle] = typeof (IDataRecord).GetMethod("GetDateTime");

         // nullable
         m_Methods[typeof (byte?).TypeHandle] = typeof (IDataRecord).GetMethod("GetByte");
         m_Methods[typeof (short?).TypeHandle] = typeof (IDataRecord).GetMethod("GetInt16");
         m_Methods[typeof (int?).TypeHandle] = typeof (IDataRecord).GetMethod("GetInt32");
         m_Methods[typeof (long?).TypeHandle] = typeof (IDataRecord).GetMethod("GetInt64");
         m_Methods[typeof (float?).TypeHandle] = typeof (IDataRecord).GetMethod("GetFloat");
         m_Methods[typeof (double?).TypeHandle] = typeof (IDataRecord).GetMethod("GetDouble");
         m_Methods[typeof (decimal?).TypeHandle] = typeof (IDataRecord).GetMethod("GetDecimal");
         m_Methods[typeof (bool?).TypeHandle] = typeof (IDataRecord).GetMethod("GetBoolean");
         m_Methods[typeof (char?).TypeHandle] = typeof (IDataRecord).GetMethod("GetChar");
         m_Methods[typeof (Guid?).TypeHandle] = typeof (IDataRecord).GetMethod("GetGuid");
         m_Methods[typeof (DateTime?).TypeHandle] = typeof (IDataRecord).GetMethod("GetDateTime");
         m_Methods[typeof (TimeSpan?).TypeHandle] = typeof (IDataRecord).GetMethod("GetDateTime");
      }

#endregion
#region --- PRIVATE -------------------------------------------------------------------------

      private readonly ConcurrentDictionary<Identity, CacheInfo> m_Cache = new ConcurrentDictionary<Identity, CacheInfo>();
      private readonly ConcurrentDictionary<Type, AnonActivator> m_Activators = new ConcurrentDictionary<Type, AnonActivator>();
      private readonly IDb m_Db;

      public DbMapper(IDb db)
      {
         m_Db = db;
      }

#endregion
#region --- PUBLIC --------------------------------------------------------------------------

      public T MapValue<T>(IDataReader reader)
      {
         var isNullable = Equals(default(T), null);

         if (reader.Read())
         {
            if (reader.IsDBNull(0))
            {
               if (!isNullable)
               {
                  throw new InvalidCastException("Unable to convert DbNull value to no-nullable value");
               }

               return default(T);
            }

            return (T) reader.GetValue(0);
         }

         return default(T);
      }

      public string MapValue(IDataReader reader)
      {
         if (reader.Read())
         {
            return reader.GetString(0);
         }

         return null;
      }

      public IEnumerable<T> MapValues<T>(IDataReader reader)
      {
         var result = new List<T>();
         var isNullable = Equals(default(T), null);

         while (reader.Read())
         {
            if (reader.IsDBNull(0))
            {
               if (!isNullable)
               {
                  throw new InvalidCastException("Unable to convert DbNull value to no-nullable value");
               }
               
               result.Add(default(T));
            }
            else
            {
               result.Add((T) reader.GetValue(0));
            }
         }

         return result;
      }

      public T MapEntity<T>(string query, IDataReader reader)
         where T : IDbRecord, new()
      {
         return MapEntities<T>(query, reader).SingleOrDefault();
      }

      public IEnumerable<T> MapEntities<T>(string query, IDataReader reader)
         where T : IDbRecord, new()
      {
         var mapper = GetDeserializer<T>(query, reader);
         var result = new List<T>();

         while (reader.Read())
         {
            var gnu = new T();

            mapper(reader, gnu);

            gnu.Reset();

            result.Add(gnu);
         }

         return result;
      }

      public IEnumerable<T> ParallelMapEntities<T>(string query, IDataReader reader)
         where T : IDbRecord, new()
      {
         var mapper = GetDeserializer<T>(query, reader);
         var names = new List<String>();

         for (var i = 0; i < reader.FieldCount; i++)
         {
            names.Add(reader.GetName(i));
         }

         var holders = new List<DataHolder<T>>();

         while (reader.Read())
         {
            var values = new object[reader.FieldCount];

            reader.GetValues(values);
            holders.Add(new DataHolder<T>() {DataRecord = new DataRecord(names, values)});
         }

         Parallel.ForEach(holders, h =>
            {
               var gnu = new T();

               mapper(h.DataRecord, gnu);

               h.Entity = gnu;
               h.Entity.Reset();
            });
         
         return holders.Select(h => h.Entity).ToArray();
      }

      public IEnumerable<T> MapType<T>(string query, IDataReader reader)
         where T : new()
      {         
         return MapType<T>(query, reader, () => new T());
      }

      public IEnumerable<T> MapType<T>(string query, IDataReader reader, Func<T> factory)
      {
         var mapper = GetDeserializer<T>(query, reader);
         var result = new List<T>();

         while (reader.Read())
         {
            var gnu = factory();

            mapper(reader, gnu);

            result.Add(gnu);
         }

         return result;
      }

      public IEnumerable<T> MapAnonymous<T>(T model, string query, IDataReader reader)
      {
         var type = typeof(T);

         AnonActivator info;

         if (!m_Activators.TryGetValue(type, out info))
            {
               m_Activators.AddOrUpdate(type, new AnonActivator(type), (t, a) => new AnonActivator(type));
               info = m_Activators[type];
            }
      
         var result = new List<T>();
         var values = new object[info.ParamCount];

         while (reader.Read())
         {
            reader.GetValues(values);

            for (var i = 0; i < values.Length; i++)
            {
               if (values[i] == DBNull.Value)
               {
                  values[i] = null;
               }
            }

            var gnu = info.New<T>(values);

            result.Add(gnu);
         }

         return result;
      }

#endregion

#region --- PRIVATE -------------------------------------------------------------------------

      public Action<IDataRecord, T> GetDeserializer<T>(string query, IDataRecord reader)
      {
         var identity = new Identity(query, typeof (T));

         CacheInfo info;

         if (!m_Cache.TryGetValue(identity, out info))
         {
            info = new CacheInfo();
         }

         if (info.Deserializer == null)
         {
            info.Deserializer = BuildMapper<T>(reader);
            m_Cache[identity] = info;
         }

         return (Action<IDataRecord, T>)info.Deserializer;
      }

      private Action<IDataRecord, T> BuildMapper<T>(IDataRecord reader)
      {
         var fields = BuildFieldDictionary<T>();
         var properties = BuildPropertyDictionary<T>();
         
         //
         // build expression 
         //
         var assignments = new List<Expression>();
         var record = Expression.Parameter(typeof (IDataRecord), "record");
         var entity = Expression.Parameter(typeof (T), "output");

         for (var i = 0; i < reader.FieldCount; i++)
         {            
            var columnName = reader.GetName(i);
            var columnType = reader.GetFieldType(i);

            MethodInfo method;

            if (!m_Methods.TryGetValue(columnType.TypeHandle, out method))
            {
               throw new InvalidDataException("No IDataRecord access method defined for column '" + columnName + "', type '" + columnType.Name + "' in DbMapper.");
            }

            ConditionalExpression assignment = null;
            PropertyInfo property = null;
            FieldInfo field = null;

            if (properties.TryGetValue(columnName, out property))
            {
               var target = Expression.Property(entity, property.Name);
               assignment = BuildAssignmentExpression(record, columnType, method, i, property.PropertyType, target);                              
            }
            else if (fields.TryGetValue(columnName, out field))
            {
               var target = Expression.Field(entity, field.Name);
               assignment = BuildAssignmentExpression(record, columnType, method, i, field.FieldType, target);
            }
            
            if (assignment == null && !m_Db.AllowUnmappedColumns)
            {
               throw new InactiveExtensionException(AccessorExtension.AllowUnmappedColumns.ToString(), "Unable to map column '" + columnName + "', type '" + columnType.Name + "' on target type '" + typeof(T).Name + "'.");
            }
            else if (assignment != null)
            {
               assignments.Add(assignment);
            }
         }

         var block = Expression.Block(assignments.ToArray());

         return Expression.Lambda<Action<IDataRecord, T>>(block, record, entity).Compile();
      }

      private static ConditionalExpression BuildAssignmentExpression(ParameterExpression record, Type columnType, MethodInfo method, int columnIndex, Type targetType, MemberExpression target)
      {
         if (targetType == typeof(TimeSpan) || Nullable.GetUnderlyingType(targetType) == typeof(TimeSpan))
         {
            var value = Expression.Variable(typeof (object));

            return Expression.IfThen(
               Expression.Not(Expression.Call(record, m_IsDbNull, Expression.Constant(columnIndex, typeof (int)))),
               Expression.Block(new[] {value},
                  Expression.Assign(value, Expression.Call(record, m_GetValue, Expression.Constant(columnIndex, typeof(int)))),
                  Expression.Assign(target, Expression.Convert(value, targetType))
                  ));
         }
         else if (method.Name == "GetBytes")
         {
            var lzero = Expression.Constant(0L);
            var izero = Expression.Constant(0);
            var imax = Expression.Constant(1024);
            var lmax = Expression.Constant(1024L);
            var lred = Expression.Variable(typeof (long));
            var loffset = Expression.Variable(typeof (long));
            var buffer = Expression.Variable(typeof (byte[]));
            var memory = Expression.Variable(typeof (MemoryStream));
            var breaker = Expression.Label();

            return Expression.IfThen(
               Expression.Not(Expression.Call(record, m_IsDbNull, Expression.Constant(columnIndex, typeof (int)))),
               Expression.Block(new [] {lred, loffset, buffer, memory},
                  Expression.Assign(memory, Expression.New(typeof (MemoryStream).GetConstructors()[0])),
                  Expression.Assign(buffer, Expression.New(typeof (byte[]).GetConstructors()[0], Expression.Constant(1024))),
                  Expression.Assign(loffset, lzero),
                  Expression.TryFinally(
                     Expression.Block(
                        Expression.Loop(
                           Expression.Block(                              
                              Expression.Assign(lred, Expression.Call(record, method, Expression.Constant(columnIndex), loffset, buffer, izero, imax)),
                              Expression.Call(memory, typeof (MemoryStream).GetMethod("Write", new[] {typeof(byte[]), typeof(int), typeof(int) }), buffer, izero, Expression.Convert(lred, typeof (int))),
                              Expression.AddAssign(loffset, lred),
                              Expression.IfThen(Expression.NotEqual(lred, lmax), Expression.Break(breaker))
                              ),
                           breaker
                           ),
                        Expression.Assign(Expression.Property(memory, typeof (MemoryStream).GetProperty("Position")), lzero),
                        Expression.Assign(target, Expression.Call(memory, typeof (MemoryStream).GetMethod("ToArray")))
                        ),
                     Expression.Call(memory, typeof (MemoryStream).GetMethod("Dispose"))
                     )
                  )
               );
         }
         else if (targetType == columnType)
         {
            return Expression.IfThen(
               Expression.Not(Expression.Call(record, m_IsDbNull, Expression.Constant(columnIndex, typeof(int)))),
               Expression.Assign(target, Expression.Call(record, method, Expression.Constant(columnIndex, typeof(int))))
               );
         }
         else if (targetType.IsEnum || Nullable.GetUnderlyingType(targetType) == columnType)
         {
            return Expression.IfThen(
               Expression.Not(Expression.Call(record, m_IsDbNull, Expression.Constant(columnIndex, typeof(int)))),
               Expression.Assign(target, Expression.Convert(Expression.Call(record, method, Expression.Constant(columnIndex, typeof(int))), targetType))
               );
         }

         return null;
      }

      private Dictionary<string, FieldInfo> BuildFieldDictionary<T>()
      {
         var fields = m_Db.CaseInsensitiveColumnMapping
                         ? new Dictionary<string, FieldInfo>(StringComparer.CurrentCultureIgnoreCase)
                         : new Dictionary<string, FieldInfo>();

         var bindings = m_Db.PrivateMemberColumnMapping
                           ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                           : BindingFlags.Public | BindingFlags.Instance;

         foreach (var field in typeof(T).GetFields(bindings))
         {
            var attribute = field.GetCustomAttributes(typeof(DbColumnAttribute), true)
               .Cast<DbColumnAttribute>()
               .SingleOrDefault();

            if (attribute != null)
            {
               fields.Add(attribute.Name ?? field.Name, field);
            }
            else
            {
               fields.Add(field.Name, field);
            }
         }

         return fields;
      }

      private Dictionary<string, PropertyInfo>  BuildPropertyDictionary<T>()
      {
         var properties = m_Db.CaseInsensitiveColumnMapping
                             ? new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase)
                             : new Dictionary<string, PropertyInfo>();

         var bindings = m_Db.PrivateMemberColumnMapping
                           ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                           : BindingFlags.Public | BindingFlags.Instance;

         foreach (var property in typeof(T).GetProperties(bindings))
         {
            var attribute = property.GetCustomAttributes(typeof(DbColumnAttribute), true)
               .Cast<DbColumnAttribute>()
               .SingleOrDefault();

            if (attribute != null)
            {
               properties.Add(attribute.Name ?? property.Name, property);               
            }
            else
            {
               properties.Add(property.Name, property);
            }
         }

         return properties;
      }

#endregion

#region --- NESTED --------------------------------------------------------------------------

      private class CacheInfo
      {
         public object Deserializer
         {
            get;
            set;
         }
      }

      private class Identity : IEquatable<Identity>
      {
         private static readonly Regex m_Select = new Regex(@"(select).*?(from)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
         private readonly int m_HashCode;
         private readonly string m_Sql;
         private readonly Type m_Type;

         internal Identity(string sql, Type type)
         {
            m_Sql = GetQuerySelects(sql);
            m_Type = type;

            unchecked
            {
               m_HashCode = 17; 
               m_HashCode = m_HashCode * 23 + (m_Sql == null ? 0 : m_Sql.GetHashCode());
               m_HashCode = m_HashCode * 23 + (type == null ? 0 : type.GetHashCode());
            }
         }

         public bool Equals(Identity other)
         {
            return other != null
                   && m_Type == other.m_Type
                   && m_Sql == other.m_Sql;
         }

         public override bool Equals(object obj)
         {
            return Equals(obj as Identity);
         }

         public override int GetHashCode()
         {
            return m_HashCode;
         }

         private static string GetQuerySelects(string query)
         {
            var result = "";

            foreach (Match match in m_Select.Matches(query))
            {
               result += match.Value + Environment.NewLine;
            }

            return result;
         }
      }

      private struct AnonActivator
      {
         private readonly ConstructorInfo m_Ctor;
         
         internal AnonActivator(Type type)
         {
            m_Ctor = type.GetConstructors()[0];
            ParamCount = m_Ctor.GetParameters().Length;  
         }

         public readonly int ParamCount;

         public T New<T>(params object[] args)
         {
            return (T)m_Ctor.Invoke(args);            
         }
      }

#endregion
   }
}