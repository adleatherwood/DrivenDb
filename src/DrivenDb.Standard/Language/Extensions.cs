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

namespace DrivenDb
{
   internal static class Extensions
   {
      public static void ForEach<T>(this IEnumerable<T> instance, Action<T> action)
      {
         if (instance == null)
         {
            return;
         }

         foreach (var item in instance)
         {
            action(item);
         }
      }

      public static void ForEach<T>(this IEnumerable<T> instance, Action<T, int> action)
      {
         if (instance == null)
         {
            return;
         }

         var i = 0;
         foreach (var item in instance)
         {
            action(item, i);
            i++;
         }
      }
   }
}
