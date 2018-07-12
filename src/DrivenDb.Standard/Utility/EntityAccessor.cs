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
using System.Linq.Expressions;
using System.Reflection;
using DrivenDb.Utility.Interfaces;

namespace DrivenDb.Utility
{
   public class EntityAccessor<T> : IEntityAccessor
   {
      private readonly Type m_GenericType;
      private readonly Type m_ObjectType;
      private readonly Dictionary<string, Func<T, object>> m_Getters;      
      private readonly Dictionary<string, PropertyInfo> m_Properties;
      private readonly Dictionary<string, Action<T, object>> m_Setters;

      public EntityAccessor(bool caseSensitive = true)
      {
         m_GenericType = typeof (T);
         m_ObjectType = typeof (object);

         m_Getters = new Dictionary<string, Func<T, object>>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
         m_Setters = new Dictionary<string, Action<T, object>>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
         m_Properties = m_GenericType.GetProperties().ToDictionary(p => p.Name, caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

         CompileGetters();
         CompileSetters();
      }

      #region --- PUBLIC --------------------------------------------------------------------------

      public bool HasProperty(string name)
      {
         return m_Getters.ContainsKey(name) || m_Setters.ContainsKey(name);
      }

      public bool CanReadProperty(string name)
      {
         return m_Getters.ContainsKey(name);
      }

      public bool CanWriteProperty(string name)
      {
         return m_Setters.ContainsKey(name);
      }

      public new Type GetType()
      {
         return typeof (T);
      }

      public IEnumerable<PropertyInfo> GetProperties()
      {
         return m_Properties.Values;
      }

      public PropertyInfo GetPropertyInfo(string name)
      {
         PropertyInfo property;

         if (!m_Properties.TryGetValue(name, out property))
         {
            throw new MissingFieldException(m_GenericType.Name, name);
         }

         return property;
      }

      public TValue GetPropertyValue<TValue>(T instance, string name)
      {
         Func<T, object> getter;

         if (!m_Getters.TryGetValue(name, out getter))
         {
            throw new MissingFieldException(m_GenericType.Name, name);
         }

         return (TValue) getter(instance);
      }

      public void SetPropertyValue<TValue>(T instance, string name, TValue value)
      {
         Action<T, object> setter;

         if (!m_Setters.TryGetValue(name, out setter))
         {
            throw new MissingFieldException(m_GenericType.Name, name);
         }

         setter(instance, value);
      }

      #endregion
      #region --- PRIVATE -------------------------------------------------------------------------

      private void CompileGetters()
      {
         foreach (var property in GetProperties().Where(p => p.CanRead))
         {
            var parameter = Expression.Parameter(m_GenericType);
            var compiled = Expression.Lambda<Func<T, object>>(
               Expression.Convert(
                  Expression.Property(
                     parameter, property.Name
                     ), m_ObjectType
                  ), parameter
               ).Compile();

            m_Getters.Add(property.Name, compiled);
         }
      }

      private void CompileSetters()
      {
         foreach (var property in GetProperties().Where(p => p.CanWrite))
         {
            var setter = property.GetSetMethod();

            if (setter != null)
            {
               var parameter1 = Expression.Parameter(m_GenericType);
               var parameter2 = Expression.Parameter(m_ObjectType);
               var compiled = Expression.Lambda<Action<T, object>>(
                  Expression.Call(parameter1, setter, Expression.Convert(parameter2, property.PropertyType)
                     ), parameter1, parameter2
                  ).Compile();

               m_Setters.Add(property.Name, compiled);
            }
         }
      }

      #endregion
   }
}