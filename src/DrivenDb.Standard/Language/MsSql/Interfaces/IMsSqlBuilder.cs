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

namespace DrivenDb.MsSql
{
   internal interface IMsSqlBuilder : ISqlBuilder
   {
      string ToCreateIdTable();
      string ToSelectIdTable();

      string ToInsertWithScopeIdentity(int index, bool returnId);

      //string ToInsertOutputDeleted<T>(T entity, int index, string[] columns)
      //   where T : IDbEntity;

      string ToUpdateOutputDeleted(int index, string[] columns);
      string ToDeleteOutputDeleted(int index, string[] columns);
   }
}