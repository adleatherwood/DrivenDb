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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using DrivenDb.Utility;

// ReSharper disable StaticFieldInGenericType
namespace DrivenDb
{
   public delegate void StateChangedEvent(EntityState previous, EntityState current);

   [DataContract]
   public abstract class DbRecord<T> : IDbRecord<T>
      where T : IDbRecord
   {
      protected static readonly EntityAccessor<T> m_Accessor = new EntityAccessor<T>(true);
      private static readonly Func<T, T, int> m_CompareTo;
      private static readonly Func<T, T, bool> m_Equals;
      private static readonly Func<T, int> m_Hasher;
      private static readonly bool? m_IsIdentity32;

      protected static readonly DbTableAttribute m_Table;
      protected static readonly DbSequenceAttribute m_Sequence;
      protected static readonly KeyValuePair<string, DbColumnAttribute> m_IdentityColumn;
      protected static readonly KeyValuePair<string, DbColumnAttribute>[] m_PrimaryColumns;
      protected static readonly IDictionary<string, DbColumnAttribute> m_Columns;

      static DbRecord()
      {
         var type = typeof(T);

         m_Table = GetTableAttribute(type);
         m_Sequence = GetSequenceAttribute(type);
         m_Columns = GetColumnAttributes(type);

         m_PrimaryColumns = m_Columns
            .Where(c => c.Value.IsPrimaryKey)
            .ToArray();

         m_IdentityColumn = m_Columns
            .FirstOrDefault(c => c.Value.IsDbGenerated);

         if (m_IdentityColumn.Value != null)
         {
            m_IsIdentity32 = m_Accessor.GetPropertyInfo(m_IdentityColumn.Key).PropertyType == typeof(int);
         }

         m_CompareTo = EntityHelper.CompareTo<T>(m_PrimaryColumns.Select(p => p.Key).ToArray()); // optional case sensitive?
         m_Equals = EntityHelper.Equals<T>(m_PrimaryColumns.Select(p => p.Key).ToArray()); // optional case sensitive?
         m_Hasher = EntityHelper.GetHashCode<T>(m_PrimaryColumns.Select(p => p.Key).ToArray());
      }

      public event StateChangedEvent StateChanged;

      [DataMember]
      protected HashSet<string> m_Changes = new HashSet<string>();

      [DataMember]
      protected DateTime? m_LastModified;

      [DataMember]
      protected DateTime? m_LastUpdated;

      [DataMember]
      protected EntityState m_PreDeletedState;

      [DataMember]
      protected EntityState m_State;

      protected T m_Instance;
      protected Lazy<int> m_Hash;

      protected DbRecord()
      {
         Initialize();
      }

      public IDbRecord Record
      {
         get { return this; }
      }

      protected void ChangeState(EntityState state)
      {
         var previous = m_State;

         m_State = state;

         if (previous != state && StateChanged != null)
         {
            StateChanged(previous, state);
         }
      }

      [OnDeserialized]
      private void OnDeserialized(StreamingContext context)
      {
         Initialize();
      }

      private void Initialize()
      {
         m_Instance = (T) (object) this;
         m_Hash = new Lazy<int>(() => m_Hasher(m_Instance));
      }

      private static DbTableAttribute GetTableAttribute(Type type)
      {
         return type.GetCustomAttributes(true)
            .Where(a => a.GetType().FullName == "DrivenDb.DbTableAttribute"
                     || a.GetType().FullName == "System.Data.Linq.Mapping.TableAttribute")
            .OrderBy(a => a.GetType().FullName)
            .Select(a =>
               {
                  var dbTable = a as DbTableAttribute;

                  if (dbTable != null)
                  {
                     return dbTable;
                  }

                  var nameProperty = a.GetType()
                     .GetProperty("Name");

                  var schema = default(string);
                  var name = default(string);

                  if (nameProperty != null)
                  {
                     name = (string) nameProperty
                        .GetGetMethod(true)
                        .Invoke(a, null);

                     var split = name.Split('.');

                     if (split.Count() == 2)
                     {
                        schema = split[0].Replace("[", "").Replace("]", "");
                        name = split[1].Replace("[", "").Replace("]", "");
                     }
                  }

                  return new DbTableAttribute()
                     {
                        Schema = schema,
                        Name = name,
                     };
               })
            .First();
      }

      private static DbSequenceAttribute GetSequenceAttribute(Type type)
      {
         return type.GetCustomAttributes(true)
            .OfType<DbSequenceAttribute>()
            .SingleOrDefault();
      }

      private static IDictionary<string, DbColumnAttribute> GetColumnAttributes(Type type)
      {
         var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
         var columns = new Dictionary<string, DbColumnAttribute>();

         foreach (var property in properties)
         {
            if (property.Name == "Record" || property.Name == "Entity")
            {
               continue;
            }

            var found = property.GetCustomAttributes(true)
               .Where(a => a.GetType().FullName == "DrivenDb.DbColumnAttribute"
                        || a.GetType().FullName == "System.Data.Linq.Mapping.ColumnAttribute")
               .OrderBy(a => a.GetType().FullName)
               .FirstOrDefault();

            if (found != null)
            {
               var dbColumn = found as DbColumnAttribute;

               if (dbColumn == null)
               {
                  var foundType = found.GetType();
                  var generatedProperty = foundType.GetProperty("IsDbGenerated");
                  var primaryProperty = foundType.GetProperty("IsPrimaryKey");
                  var nameProperty = foundType.GetProperty("Name");

                  dbColumn = new DbColumnAttribute()
                     {
                        IsDbGenerated = (bool) generatedProperty.GetGetMethod(true).Invoke(found, null),
                        IsPrimaryKey = (bool) primaryProperty.GetGetMethod(true).Invoke(found, null),
                        Name = (string) nameProperty.GetGetMethod(true).Invoke(found, null),
                     };
               }

               dbColumn.Name = dbColumn.Name ?? property.Name;

               columns.Add(property.Name, dbColumn);
            }
         }

         return columns;
      }

      int IDbRecord.IdentityHash
      {
         get { return m_Hash.Value; }
      }

      object[] IDbRecord.PrimaryKey
      {
         get
         {
            var result = new List<object>();

            foreach (var column in m_PrimaryColumns)
            {
               result.Add(m_Accessor.GetPropertyValue<object>(m_Instance, column.Key));
            }

            return result.ToArray();
         }
      }

      DateTime? IDbRecord.LastUpdated
      {
         get { return m_LastUpdated; }
      }

      DateTime? IDbRecord.LastModified
      {
         get { return m_LastModified; }
      }

      EntityState IDbRecord.State
      {
         get { return m_State; }
      }

      [Obsolete("Please get the schema information from the Table or TableOverride properties.")]
      string IDbRecord.Schema
      {
         get { return m_Table.Schema; }
      }

      DbTableAttribute IDbRecord.Table
      {
         get { return m_Table; }
      }

      DbSequenceAttribute IDbRecord.Sequence
      {
         get { return m_Sequence; }
      }

      DbTableAttribute IDbRecord.TableOverride
      {
         get;
         set;
      }

      DbColumnAttribute IDbRecord.IdentityColumn
      {
         get { return m_IdentityColumn.Value; }
      }

      DbColumnAttribute[] IDbRecord.PrimaryColumns
      {
         get { return m_PrimaryColumns.Select(c => c.Value).ToArray(); }
      }

      IDictionary<string, DbColumnAttribute> IDbRecord.Columns
      {
         get { return m_Columns; }
      }

      IEnumerable<string> IDbRecord.Changes
      {
         get { return m_Changes; }
      }

      void IDbRecord.SetIdentity(long identity)
      {
         if (m_IdentityColumn.Value != null && m_IsIdentity32.HasValue)
         {
            if (m_IsIdentity32.Value)
            {
               m_Accessor.SetPropertyValue(m_Instance, m_IdentityColumn.Key, (int) identity);
            }
            else
            {
               m_Accessor.SetPropertyValue(m_Instance, m_IdentityColumn.Key, identity);
            }
         }
      }

      object IDbRecord.GetProperty(string property)
      {
         return m_Accessor.GetPropertyValue<object>(m_Instance, property);
      }

      void IDbRecord.SetProperty(string property, object value)
      {
         m_Accessor.SetPropertyValue(m_Instance, property, value);
      }

      void IDbRecord.Reset()
      {
         ChangeState(EntityState.Current);

         m_LastModified = null;
         m_LastUpdated = DateTime.Now;
         m_Changes.Clear();
      }

      bool IDbRecord<T>.SameAs(T other)
      {
         return m_Hash.Value == other.IdentityHash && m_Equals(m_Instance, other);
      }

      bool IEquatable<T>.Equals(T other)
      {
         var value1 = m_State == EntityState.New && this.m_Hash.Value == default(int)
            ? this.GetHashCode()
            : m_Hash.Value;

         var value2 = m_State == EntityState.New && this.m_Hash.Value == default(int)
            ? other.GetHashCode()
            : other.IdentityHash;

         return value1 == value2 && m_Equals(m_Instance, other);
      }

      int IComparable<T>.CompareTo(T other)
      {
         return m_CompareTo(this.m_Instance, other);
      }
   }
}