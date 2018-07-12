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
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace DrivenDb
{
   [DataContract]
   public abstract class DbEntity<T> : DbRecord<T>, IDbEntity<T>
      where T : DbEntity<T>, IDbEntity, INotifyPropertyChanged, new()
   {
      protected DbEntity()
      {
         Initialize();
      }

      public IDbEntity<T> Entity
      {
         get { return this; }
      }

      protected virtual void BeforeSerialization()
      {
      }

      [OnSerializing]
      private void OnSerializing(StreamingContext context)
      {
         BeforeSerialization();
      }

      protected virtual void AfterDeserialization()
      {
      }

      [OnDeserialized]
      private void OnDeserialized(StreamingContext context)
      {
         Initialize();
         AfterDeserialization();
      }

      private void Initialize()
      {
         m_Instance.PropertyChanged += (s, e) =>
            {
               if (m_Columns.ContainsKey(e.PropertyName))
               {
                  m_Changes.Add(e.PropertyName);
                  m_LastModified = DateTime.Now;

                  if (m_State != EntityState.New)
                  {
                     ChangeState(EntityState.Modified);
                  }
               }
            };
      }

      void IDbEntity.Delete()
      {
         if (m_State != EntityState.Deleted)
         {
            m_PreDeletedState = m_State;

            ChangeState(EntityState.Deleted);
         }
      }

      void IDbEntity.Undelete()
      {
         if (m_State == EntityState.Deleted)
         {
            m_State = m_PreDeletedState;

            ChangeState(m_PreDeletedState);
         }
      }

      public T ToNew()
      {
         var result = new T();

         result.Update(this.m_Instance, false);

         if (m_IdentityColumn.Value != null)
         {
            var property = m_Accessor.GetPropertyInfo(m_IdentityColumn.Key);

            var value = property.PropertyType.IsValueType
               ? Activator.CreateInstance(property.PropertyType)
               : null;

            result.Entity.SetProperty(m_IdentityColumn.Key, value);
            result.m_Changes.Remove(m_IdentityColumn.Key);
         }

         result.m_State = EntityState.New;

         return result;
      }

      public T ToUpdate()
      {
         var result = new T();

         result.Update(this.m_Instance, false);

         if (m_IdentityColumn.Value != null)
         {
            result.m_Changes.Remove(m_IdentityColumn.Key);
         }

         result.m_State = EntityState.Modified;

         return result;
      }

      public TOther ToMapped<TOther>()
         where TOther : new()
      {
         var result = new TOther();

         MapTo(result);

         return result;
      }

      public void MapTo<TOther>(TOther other)
      {
         var targets = other.GetType()
            .GetProperties();

         foreach (var target in targets)
         {
            if (target.CanWrite && m_Accessor.CanReadProperty(target.Name))
            {
               target.SetValue(other, m_Accessor.GetPropertyValue<object>(this.m_Instance, target.Name), null);
            }
         }
      }

      public void Update(T other, bool checkIdentity = true)
      {
         if (checkIdentity && !this.Entity.SameAs(other))
         {
            throw new InvalidDataException("Cannot update mismatched records");
         }

         var properties = m_Accessor.GetProperties();

         foreach (var property in properties)
         {
            if (m_Accessor.CanReadProperty(property.Name)
               && m_Accessor.CanWriteProperty(property.Name)
               )
            {
               Entity.SetProperty(property.Name, other.GetProperty(property.Name));
            }
         }

         m_LastModified = DateTime.Now;
      }

      public T Clone()
      {
         var result = new T();
         var properties = m_Accessor.GetProperties();

         foreach (var property in properties)
         {
            if (m_Accessor.CanReadProperty(property.Name)
               && m_Accessor.CanWriteProperty(property.Name)
               )
            {
               var value = m_Accessor.GetPropertyValue<object>(m_Instance, property.Name);

               m_Accessor.SetPropertyValue(result, property.Name, value);
            }
         }

         result.Reset();

         foreach (var change in m_Changes)
         {
            result.m_Changes.Add(change);
         }

         result.m_LastModified = this.m_LastModified;
         result.m_LastUpdated = this.m_LastUpdated;
         result.m_State = this.m_State;

         return result;
      }

      void IDbEntity<T>.Merge(T other, bool checkIdentity)
      {
         if (checkIdentity && !this.Entity.SameAs(other))
         {
            throw new InvalidDataException("Cannot update mismatched records");
         }

         var lastModified = m_LastModified;
         var lastUpdated = m_LastUpdated;
         var changes = new HashSet<string>(m_Changes);

         if (m_LastUpdated < other.LastUpdated)
         {
            var properties = m_Accessor.GetProperties();

            foreach (var property in properties)
            {
               if (!changes.Contains(property.Name))
               {
                  Entity.SetProperty(property.Name, other.GetProperty(property.Name));
               }
            }
         }

         foreach (var change in other.Changes)
         {
            if (!changes.Contains(change))
            {
               Entity.SetProperty(change, other.GetProperty(change));
               changes.Add(change);
            }
         }

         m_Changes = new HashSet<string>(changes);

         var state = other.State == EntityState.Deleted || other.State == EntityState.Modified
                      ? other.State
                      : m_State;

         ChangeState(state);

         m_LastModified = lastModified.HasValue && lastModified > other.LastModified
                             ? lastModified
                             : other.LastModified;

         m_LastUpdated = lastUpdated > other.LastUpdated
                            ? lastUpdated
                            : other.LastUpdated;
      }
   }
}