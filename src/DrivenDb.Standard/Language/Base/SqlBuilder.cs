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
using System.Linq;
using System.Collections.Generic;

namespace DrivenDb.Base
{
   public abstract class SqlBuilder : ISqlBuilder
   {
      protected readonly List<Where> m_Wheres = new List<Where>();
      protected readonly List<Setter> m_Setters = new List<Setter>();
      protected readonly HashSet<string> m_Columns = new HashSet<string>();
      protected readonly List<GroupedWhere> m_Grouped = new List<GroupedWhere>();
      private readonly string m_IdOpen;
      private readonly string m_IdClose;
      private readonly string m_Parameter;
      protected readonly string m_Terminator;

      protected SqlBuilder(string idOpen, string idClose, string parameter, string terminator)
      {
         m_IdOpen = idOpen;
         m_IdClose = idClose;
         m_Parameter = parameter;
         m_Terminator = terminator;
      }

      public string Schema
      {
         get;
         set;
      }

      public string Table
      {
         get;
         set;
      }

      public void AddColumn(string name)
      {
         m_Columns.Add(name);
      }

      public void AddSetter(string column, int parameter)
      {
         m_Setters.Add(new Setter(column, parameter));
      }

      public void AddWhere(string column, int parameter)
      {
         m_Wheres.Add(new Where(column, parameter));
      }

      public void GroupWhere()
      {
         m_Grouped.Add(new GroupedWhere(m_Wheres.ToArray()));
         m_Wheres.Clear();
      }

      public abstract string ToInsert<T>(T entity, int index, bool returnId) where T : IDbEntity;

      public string ToSelect()
      {
         if (String.IsNullOrWhiteSpace(Table))
         {
            throw new InvalidOperationException("No table specified for select");
         }

         return String.Format("SELECT {0} FROM {1}{2}{3}", GetFormattedColumns(), GetFormattedTable(), GetWhere(), m_Terminator);
      }

      public string ToUpdate()
      {
         if (String.IsNullOrWhiteSpace(Table))
         {
            throw new InvalidOperationException("No table specified for update");
         }

         if (!m_Setters.Any())
         {
            throw new InvalidOperationException("No columns specified for update");
         }

         return String.Format("UPDATE {0} SET {1}{2}{3}", GetFormattedTable(), GetFormattedSetters(), GetWhere(), m_Terminator);
      }

      public string ToDelete()
      {
         if (String.IsNullOrWhiteSpace(Table))
         {
            throw new InvalidOperationException("No table specified for delete");
         }

         return String.Format("DELETE FROM {0}{1}{2}", GetFormattedTable(), GetWhere(), m_Terminator);
      }

      public virtual DateTime CorrectDateTime(DateTime dateTime)
      {
         return dateTime;
      }

      protected string GetWhere()
      {
         if (!m_Wheres.Any() && !m_Grouped.Any())
         {
            return null;
         }

         if (m_Wheres.Any() && !m_Grouped.Any())
         {
            return String.Format(" WHERE {0}", GetFormattedWheres());
         }

         if (!m_Wheres.Any() && m_Grouped.Any())
         {
            return String.Format(" WHERE {0}", GetFormattedGroupedWheres());
         }

         return String.Format(" WHERE {0} AND ({1})", GetFormattedWheres(), GetFormattedGroupedWheres());
      }

      protected string GetFormattedColumns()
      {
         if (m_Columns.Count == 0)
         {
            return "*";
         }

         return String.Join(", ", m_Columns.Select(c => String.Format("{0}{1}{2}", m_IdOpen, c, m_IdClose)));
      }

      protected string GetFormattedSetterColumns()
      {
         return String.Join(", ", m_Setters.Select(s => String.Format("{0}{1}{2}", m_IdOpen, s.Column, m_IdClose)));
      }

      protected string GetFormattedSetterValues()
      {
         return String.Join(", ", m_Setters.Select(s => String.Format("{0}{1}", m_Parameter, s.Parameter)));
      }

      protected string GetFormattedTable()
      {
         if (String.IsNullOrWhiteSpace(Schema))
         {
            return String.Format("{0}{2}{1}", m_IdOpen, m_IdClose, Table);
         }

         return String.Format("{0}{2}{1}.{0}{3}{1}", m_IdOpen, m_IdClose, Schema, Table);
      }

      protected string GetFormattedWheres()
      {
         return String.Join(" AND ", m_Wheres.Select(c => String.Format("{0}{1}{2} = {3}{4}", m_IdOpen, c.Column, m_IdClose, m_Parameter, c.Parameter)));
      }

      protected string GetFormattedGroupedWheres()
      {
         var groups = m_Grouped.Select(
             g => "(" +
                 String.Join(" AND ",
                     g.Wheres.Select(w => String.Format("{0}{1}{2} = {3}{4}", m_IdOpen, w.Column, m_IdClose, m_Parameter, w.Parameter))
                     ) + ")"
             );

         return String.Join(" OR ", groups);
      }

      protected string GetFormattedSetters()
      {
         return String.Join(", ", m_Setters.Select(c => String.Format("{0}{1}{2} = {3}{4}", m_IdOpen, c.Column, m_IdClose, m_Parameter, c.Parameter)));
      }

      protected class GroupedWhere
      {
         public GroupedWhere(IEnumerable<Where> wheres)
         {
            Wheres = wheres;
         }

         public IEnumerable<Where> Wheres
         {
            get;
            private set;
         }
      }

      protected class Where
      {
         public Where(string column, int parameter)
         {
            Column = column;
            Parameter = parameter;
         }

         public string Column
         {
            get;
            private set;
         }

         public int Parameter
         {
            get;
            private set;
         }
      }

      protected class Setter
      {
         public Setter(string column, int parameter)
         {
            Column = column;
            Parameter = parameter;
         }

         public string Column
         {
            get;
            private set;
         }

         public int Parameter
         {
            get;
            private set;
         }
      }
   }
}
