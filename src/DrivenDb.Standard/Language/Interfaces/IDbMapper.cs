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
using System.Data;

namespace DrivenDb
{
   public interface IDbMapper
   {
      T MapValue<T>(IDataReader reader);
      
      IEnumerable<T> MapValues<T>(IDataReader reader);
      
      string MapValue(IDataReader reader);

      T MapEntity<T>(string query, IDataReader reader)
         where T : IDbRecord, new();

      IEnumerable<T> MapEntities<T>(string query, IDataReader reader)
         where T : IDbRecord, new();

      IEnumerable<T> MapAnonymous<T>(T model, string query, IDataReader reader);

      IEnumerable<T> ParallelMapEntities<T>(string query, IDataReader reader)
         where T : IDbRecord, new();

      IEnumerable<T> MapType<T>(string query, IDataReader reader)
         where T : new();

      IEnumerable<T> MapType<T>(string query, IDataReader reader, Func<T> factory);
   }
}