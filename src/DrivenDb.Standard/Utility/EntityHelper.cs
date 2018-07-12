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
using System.Linq.Expressions;

namespace DrivenDb.Utility
{
   internal static class EntityHelper
   {
      public static Func<T, int> GetHashCode<T>(IEnumerable<string> members)
      {
         var funcs = new List<Func<T, int>>();

         foreach (var member in members)
         {
            funcs.Add(GetHashCode<T>(member));
         }

         return (t) =>
            {
               var first = true;
               var result = 0;

               funcs.ForEach(f =>
                  {
                     if (first)
                     {
                        result = f(t);
                        first = false;
                     }
                     else
                     {
                        result = CombineHashCodes(f(t), result);
                     }
                  });

               return result;
            };
      }

      public static Func<T, T, bool> Equals<T>(IEnumerable<string> members)
      {
         var funcs = new List<Func<T, T, bool>>();

         foreach (var member in members)
         {
            funcs.Add(Equals<T>(member));
         }

         return (t1, t2) => funcs.TrueForAll(f => f(t1, t2));
      }

      public static Func<T, T, int> CompareTo<T>(IEnumerable<string> members)
      {
         var funcs = new List<Func<T, T, int>>();

         foreach (var member in members)
         {
            funcs.Add(CompareTo<T>(member));
         }

         return (t1, t2) =>
         {
            foreach (var func in funcs)
            {
               var result = func(t1, t2);

               if (result != 0)
               {
                  return result;
               }
            }

            return 0;
         };
      }

      private static Func<T, T, bool> Equals<T>(string member)
      {
         var propertyInfo = typeof(T).GetProperty(member);

         if (!propertyInfo.PropertyType.Name.Contains("Nullable"))
         {            
            var methodInfo = propertyInfo.PropertyType.GetMethod("Equals", new Type[] {propertyInfo.PropertyType});

            var type = typeof (T);
            var source1 = Expression.Parameter(type, "source1");
            var source2 = Expression.Parameter(type, "source2");
            var property1 = Expression.Property(source1, member);
            var property2 = Expression.Property(source2, member);
            var call = Expression.Call(property1, methodInfo, property2);

            return Expression.Lambda<Func<T, T, bool>>(call, source1, source2).Compile();
         }

         return (a, b) => false;
      }

      private static Func<T, T, int> CompareTo<T>(string member)
      {
         var propertyInfo = typeof(T).GetProperty(member);
         var methodInfo = propertyInfo.PropertyType.GetMethod("CompareTo", new Type[] { propertyInfo.PropertyType });

         if (methodInfo != null)
         {
            var type = typeof(T);
            var source1 = Expression.Parameter(type, "source1");
            var source2 = Expression.Parameter(type, "source2");
            var property1 = Expression.Property(source1, member);
            var property2 = Expression.Property(source2, member);
            var call = Expression.Call(property1, methodInfo, property2);

            return Expression.Lambda<Func<T, T, int>>(call, source1, source2).Compile();
         }

         return (a, b) => -1;
      }

      private static Func<T, int> GetHashCode<T>(string member)
      {
         var type = typeof(T);
         var source = Expression.Parameter(type, "source");
         var property = Expression.Property(source, member);
         var call = Expression.Call(property, "GetHashCode", null);

         return Expression.Lambda<Func<T, int>>(call, source).Compile();
      }

      internal static int CombineHashCodes(int h1, int h2)
      {
         return (((h1 << 5) + h1) ^ h2);
      }
   }
}