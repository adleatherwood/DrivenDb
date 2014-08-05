﻿/**************************************************************************************
 * Original Author : Anthony Leatherwood (adleatherwood@gmail.com)                              
 * Source Location : http://drivendb.codeplex.com     
 *  
 * This source is subject to the Microsoft Public License.
 * Link: http://drivendb.codeplex.com/license
 *  
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
 **************************************************************************************/

using System;
using System.Linq;
using DrivenDb.Base;

namespace DrivenDb.MsSql
{
   class MsSqlBuilder : SqlBuilder, IMsSqlBuilder
   {
      private static readonly DateTime SQLMIN = new DateTime(1753, 1, 1);
      private static readonly DateTime SQLMAX = new DateTime(9999, 12, 31, 23, 59, 59);
      private static readonly TimeSpan ZERO = new TimeSpan(0);

      public MsSqlBuilder() : base("[", "]", "@", ";")
      {
      }

      public override string ToInsert<T>(T entity, int index, bool returnId)
      {
         var output = returnId && entity.IdentityColumn != null
                         ? String.Format(" OUTPUT {0}, INSERTED.{1}", index, entity.IdentityColumn.Name)
                         : null;

         return String.Format("INSERT INTO {0} ({1}){2} VALUES ({3});", GetFormattedTable(), GetFormattedSetterColumns(), output, GetFormattedSetterValues());
      }

      public string ToInsertWithScopeIdentity(int index, bool returnId)
      {
         var identity = returnId
                           ? String.Format("SELECT {0}, CAST(SCOPE_IDENTITY() AS  BIGINT);", index)
                           : null;

         return String.Format("INSERT INTO {0} ({1}) VALUES ({2}); {3}", GetFormattedTable(), GetFormattedSetterColumns(), GetFormattedSetterValues(), identity);
      }

      //public string ToInsertOutputDeleted<T>(T entity, int index, string[] columns)
      //    where T : IDbEntity
      //{
      //   var deleted = String.Join(",", columns.Select(c => "DELETED." + c));
      //   var output = entity.IdentityColumn != null
      //                   ? String.Format(" OUTPUT {0}, INSERTED.{1}, {2}", index, entity.IdentityColumn.Name, deleted)
      //                   : null;

      //   return String.Format("INSERT INTO {0} ({1}){2} VALUES ({3});", GetFormattedTable(), GetFormattedSetterColumns(), output, GetFormattedSetterValues());
      //}

      public string ToUpdateOutputDeleted(int index, string[] columns)         
      {
         if (String.IsNullOrWhiteSpace(Table))
         {
            throw new InvalidOperationException("No table specified for update");
         }

         if (!m_Setters.Any())
         {
            throw new InvalidOperationException("No columns specified for update");
         }

         var deleted = String.Join(",", columns.Select(c => "DELETED." + c));
         var output = String.Format(" OUTPUT {0},{1}", index, deleted);

         return String.Format("UPDATE {0} SET {1}{2}{3}{4}", GetFormattedTable(), GetFormattedSetters(), output, GetWhere(), m_Terminator);
      }

      public string ToDeleteOutputDeleted(int index, string[] columns)
      {
         if (String.IsNullOrWhiteSpace(Table))
         {
            throw new InvalidOperationException("No table specified for delete");
         }

         var deleted = String.Join(",", columns.Select(c => "DELETED." + c));
         var output = String.Format(" OUTPUT {0},{1}", index, deleted);

         return String.Format("DELETE FROM {0}{1}{2}{3}", GetFormattedTable(), output, GetWhere(), m_Terminator);
      }

      public override DateTime CorrectDateTime(DateTime dateTime)
      {
         var result = dateTime;

         result = (result - SQLMIN) > ZERO ? result : SQLMIN;
         result = (SQLMAX - result) > ZERO ? result : SQLMAX;

         return result;
      }
   }
}
