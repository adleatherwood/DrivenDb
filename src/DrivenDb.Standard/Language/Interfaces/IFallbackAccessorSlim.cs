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
   public interface IFallbackAccessorSlim
   {
      IEnumerable<T> ReadValues<T>(string query, params object[] parameters);

      T ReadValue<T>(string query, params object[] parameters);

      IEnumerable<T> ReadAnonymous<T>(T model, string query, params object[] parameters);

      IEnumerable<T> ReadType<T>(string query, params object[] parameters)
         where T : new();

      IEnumerable<T> ReadType<T>(Func<T> factory, string query, params object[] parameters);

      T ReadEntity<T>(string query, params object[] parameters)
         where T : IDbRecord, new();

      IEnumerable<T> ReadEntities<T>(string query, params object[] parameters)
         where T : IDbRecord, new();
   }
}