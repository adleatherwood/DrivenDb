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

namespace DrivenDb
{
   public interface ISqlBuilder
   {
      string Schema
      {
         get;
         set;
      }

      string Table
      {
         get;
         set;
      }
      
      void AddColumn(string name);
      void AddSetter(string column, int parameter);
      void AddWhere(string column, int parameter);
      void GroupWhere();

      string ToInsert<T>(T entity, int index, bool returnId) where T : IDbEntity;
      string ToSelect();
      string ToUpdate();
      string ToDelete();

      DateTime CorrectDateTime(DateTime dateTime);
   }
}
