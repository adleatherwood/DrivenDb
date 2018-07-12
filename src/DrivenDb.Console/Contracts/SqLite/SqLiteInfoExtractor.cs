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

using DrivenDb;
using DrivenDbConsole.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace DrivenDbConsole.Contracts.SqLite
{
   internal class SqLiteInfoExtractor : IInfoExtractor
   {
      #region PUBLIC CONTRUCTOR -------------------------------------------------------------------

      private readonly string _cstring;
      private readonly IDbAccessor _model;

      public SqLiteInfoExtractor(string cstring)
      {
         _cstring = cstring;
         _model = DbFactory.CreateAccessor(DbAccessorType.SqLite, () => new SQLiteConnection(_cstring));
      }

      #endregion PUBLIC CONTRUCTOR -------------------------------------------------------------------

      #region PUBLIC METHODS ----------------------------------------------------------------------

      //public IDatabaseInfo GetDatabaseMetadata(string tableExpression)
      //{
      //   var tableNames = GetTableNames(new[] { tableExpression });
      //   var tables = GetTableDefs(tableNames);

      //   return new DatabaseInfo(tables);
      //}

      public IDatabaseInfo GetDatabaseMetadata(IEnumerable<string> tableExpressions)
      {
         var tableNames = GetTableNames(tableExpressions);
         var tables = GetTableDefs(tableNames);

         return new DatabaseInfo(tables);
      }

      #endregion PUBLIC METHODS ----------------------------------------------------------------------

      #region PRIVATE METHODS ---------------------------------------------------------------------

      private IEnumerable<Tuple<string, string>> GetTableNames(IEnumerable<string> tableExpressions)
      {
         if (!tableExpressions.Any())
         {
            return new Tuple<string, string>[0];
         }

         var likes = String.Join(" ", tableExpressions.Select(i => String.Format("[name] LIKE '{0}' OR", i)));
         var result = _model.ReadAnonymous(new { Schema = "", Table = "" },
            String.Format(@"SELECT '' AS 'Schema', [name] AS 'Table' FROM [sqlite_master] WHERE {0} AND [type] = 'table'", likes.Substring(0, likes.Length - 2))
            );

         return result.Select(i => new Tuple<string, string>(i.Schema, i.Table));
      }

      private IEnumerable<ITableInfo> GetTableDefs(IEnumerable<Tuple<string, string>> tableNames)
      {
         var result = new List<ITableInfo>();

         foreach (var tableName in tableNames)
         {
            var gnu = GetTableDef(tableName.Item1, tableName.Item2);
            result.Add(gnu);
         }

         return result;
      }

      private ITableInfo GetTableDef(string schemaName, string tableName)
      {
         // TODO support for SQLite triggers
         return new TableInfo(schemaName, tableName, false, GetFieldDefs(tableName));
      }

      private IEnumerable<IColumnInfo> GetFieldDefs(string tableName)
      {
         var result = new List<IColumnInfo>();
         var ordinal = 0;
         var fields = _model.ReadAnonymous(new { cid = 0L, name = "", type = "", notnull = 0L, dflt_value = "", pk = 0L },
            String.Format(@"PRAGMA table_info('{0}')", tableName)
            )
            .Select(a => new { ColumnName = a.name, DataType = a.type.ToLower(), IsNullable = a.notnull == 0, ColumnDefault = "", OrdinalPosition = ordinal++, IsPrimaryKey = a.pk == 1 });

         var pknames = fields.Where(f => f.IsPrimaryKey)
            .Select(f => f.ColumnName);

         var idnames = fields.Where(f => f.IsPrimaryKey && f.DataType.ToLower() == "integer")
            .Select(f => f.ColumnName);

         var primaries = new HashSet<string>(pknames);
         var identities = new HashSet<string>(idnames);

         foreach (var field in fields)
         {
            string clrType;

            if (
                  field.DataType.IndexOf("int", StringComparison.CurrentCultureIgnoreCase) >= 0
               )
            {
               if (
                     field.DataType.IndexOf("small", StringComparison.CurrentCultureIgnoreCase) >= 0
                  || field.DataType.IndexOf("tiny", StringComparison.CurrentCultureIgnoreCase) >= 0
                  )
               {
                  clrType = "short";
               }
               else if
                  (
                     field.DataType.IndexOf("big", StringComparison.CurrentCultureIgnoreCase) >= 0
                  || field.DataType.IndexOf("8", StringComparison.CurrentCultureIgnoreCase) >= 0
                  )
               {
                  clrType = "long";
               }
               else
               {
                  clrType = "int";
               }
            }
            else if
               (
                  field.DataType.IndexOf("char", StringComparison.CurrentCultureIgnoreCase) >= 0
               || field.DataType.IndexOf("text", StringComparison.CurrentCultureIgnoreCase) >= 0
               || field.DataType.IndexOf("clob", StringComparison.CurrentCultureIgnoreCase) >= 0
               )
            {
               clrType = "string";
            }
            else if
               (
                  field.DataType.IndexOf("blob", StringComparison.CurrentCultureIgnoreCase) >= 0
               )
            {
               clrType = "byte[]";
            }
            else if
               (
                  field.DataType.IndexOf("real", StringComparison.CurrentCultureIgnoreCase) >= 0
               || field.DataType.IndexOf("double", StringComparison.CurrentCultureIgnoreCase) >= 0
               )
            {
               clrType = "double";
            }
            else if
               (
                  field.DataType.IndexOf("float", StringComparison.CurrentCultureIgnoreCase) >= 0
               )
            {
               clrType = "float";
            }
            else if
               (
                  field.DataType.IndexOf("date", StringComparison.CurrentCultureIgnoreCase) >= 0
               || field.DataType.IndexOf("time", StringComparison.CurrentCultureIgnoreCase) >= 0
               )
            {
               clrType = "DateTime";
            }
            else if
               (
                  field.DataType.IndexOf("bool", StringComparison.CurrentCultureIgnoreCase) >= 0
               )
            {
               clrType = "bool";
            }
            else
            {
               clrType = "decimal";
            }

            if (clrType != "byte[]" && clrType != "string" && !field.IsPrimaryKey && field.IsNullable)
            {
               clrType += "?";
            }

            var gnu = new ColumnInfo(field.ColumnName, field.DataType, clrType, field.ColumnDefault, field.IsNullable, primaries.Contains(field.ColumnName), identities.Contains(field.ColumnName), field.OrdinalPosition, false);

            result.Add(gnu);
         }

         return result;
      }

      #endregion PRIVATE METHODS ---------------------------------------------------------------------
   }
}