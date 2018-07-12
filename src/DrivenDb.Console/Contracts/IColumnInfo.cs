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

namespace DrivenDbConsole.Contracts
{
   internal interface IColumnInfo
   {
      string Name
      {
         get;
      }

      string SqlType
      {
         get;
      }

      string ClrType
      {
         get;
      }

      bool IsNullable
      {
         get;
      }

      bool IsPrimaryKey
      {
         get;
      }

      bool IsIdentity
      {
         get;
      }

      bool HasDefault
      {
         get;
      }

      string DefaultValue
      {
         get;
      }

      int OrdinalPosition
      {
         get;
      }

      bool IsReadonly
      {
         get;
         set;
      }
   }
}