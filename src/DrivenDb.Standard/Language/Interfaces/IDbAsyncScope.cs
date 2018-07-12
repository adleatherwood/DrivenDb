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
using System.Threading.Tasks;

namespace DrivenDb
{
   public interface IDbAsyncScope : IDisposable
   {
      Task<IEnumerable<T>> ReadValuesAsync<T>(string query, params object[] parameters);

      Task<T> ReadValueAsync<T>(string query, params object[] parameters);

      Task<IEnumerable<T>> ReadAnonymousAsync<T>(T model, string query, params object[] parameters);

      Task<IEnumerable<T>> ReadTypeAsync<T>(Func<T> factory, string query, params object[] parameters);

      Task<IEnumerable<T>> ReadTypeAsync<T>(string query, params object[] parameters)
         where T : new();

      void Commit();

      Task ExecuteAsync(string query, params object[] parameters);
   }
}