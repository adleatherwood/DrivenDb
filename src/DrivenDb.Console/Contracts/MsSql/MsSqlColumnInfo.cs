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

using DrivenDbConsole.Contracts.Base;
using System;

namespace DrivenDbConsole.Contracts.MsSql
{
   internal class MsSqlColumnInfo : ColumnInfo
   {
      public MsSqlColumnInfo(string name, string sqlType, string clrType, string defaultValue, bool isNullable, bool isPrimaryKey, bool isIdentity, int ordinalPosition, bool isReadonly)
         : base(name, sqlType, clrType, defaultValue, isNullable, isPrimaryKey, isIdentity, ordinalPosition, isReadonly)
      {
         switch ((defaultValue ?? "").ToLower())
         {
            case "(newid())":
               DefaultValue = "Guid.NewGuid()";
               break;

            case "(getdate())":
               DefaultValue = "DateTime.Now";
               break;

            case "(null)":
               DefaultValue = "null";
               break;

            default:
               DefaultValue = defaultValue;
               break;
         }

         if (DefaultValue != null)
         {
            if (DefaultValue.Contains("'"))
            {
               DefaultValue = DefaultValue.Replace("'", "\"");
            }

            if (sqlType.ToLower() == "bit")
            {
               if (DefaultValue == "null")
                  DefaultValue = "null";
               else if (DefaultValue == "((0))")
                  DefaultValue = "false";
               else
                  DefaultValue = "true";
            }

            if (DefaultValue.Contains("(("))
            {
               DefaultValue = DefaultValue.Replace("((", "");
               DefaultValue = DefaultValue.Replace("))", "");
            }

            if (sqlType.ToLower() == "money")
            {
               DefaultValue += "m";
            }
         }
      }
   }
}