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
using System.Data.SqlClient;
using System.Linq;

namespace DrivenDbConsole.Contracts.MsSql
{
   internal class MsSqlInfoExtractor : IInfoExtractor
   {
      #region STATIC CONSTRUCTOR ------------------------------------------------------------------

      private static readonly Dictionary<string, string> _types;

      static MsSqlInfoExtractor()
      {
         //
         // sql data type -> clr data type
         //
         _types = new Dictionary<string, string>()
            {
               {"bigint", "long"},
               {"bigint?", "long?"},
               {"bit", "bool"},
               {"bit?", "bool?"},
               {"char", "string"},
               {"char?", "string"},
               {"date", "DateTime"},
               {"date?", "DateTime?"},
               {"datetime", "DateTime"},
               {"datetime?", "DateTime?"},
               {"datetime2", "DateTime"},
               {"datetime2?", "DateTime?"},
               {"decimal", "decimal"},
               {"decimal?", "decimal?"},
               {"float", "double"},
               {"float?", "double?"},
               {"image", "byte[]"},
               {"image?", "byte[]"},
               {"int", "int"},
               {"int?", "int?"},
               {"money", "decimal"},
               {"money?", "decimal?"},
               {"nchar", "string"},
               {"nchar?", "string"},
               {"numeric", "decimal"},
               {"numeric?", "decimal?"},
               {"nvarchar", "string"},
               {"nvarchar?", "string"},
               {"real", "single"},
               {"real?", "single?"},
               {"smalldatetime", "DateTime"},
               {"smalldatetime?", "DateTime?"},
               {"smallint", "short"},
               {"smallint?", "short?"},
               {"text", "string"},
               {"text?", "string"},
               {"timestamp", "byte[]"},
               {"timestamp?", "byte[]"},
               {"time", "TimeSpan"},
               {"time?", "TimeSpan?"},
               {"tinyint", "byte"},
               {"tinyint?", "byte?"},
               {"uniqueidentifier", "Guid"},
               {"uniqueidentifier?", "Guid?"},
               {"varbinary", "byte[]"},
               {"varbinary?", "byte[]"},
               {"varchar", "string"},
               {"varchar?", "string"},
               {"xml", "string"},
               {"xml?", "string"},
            };
      }

      #endregion STATIC CONSTRUCTOR ------------------------------------------------------------------

      #region PUBLIC CONTRUCTOR -------------------------------------------------------------------

      private readonly string _cstring;
      private readonly IDbAccessor _model;

      public MsSqlInfoExtractor(string cstring)
      {
         _cstring = cstring;
         _model = DbFactory.CreateAccessor(DbAccessorType.MsSql, () => new SqlConnection(_cstring));
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

      private IEnumerable<Tuple<string, string, bool>> GetTableNames(IEnumerable<string> tableExpressions)
      {
         if (!tableExpressions.Any())
         {
            return new Tuple<string, string, bool>[0];
         }

         var likes = String.Join(" ", tableExpressions.Select(i => String.Format("(t.TABLE_SCHEMA = '{0}' AND t.TABLE_NAME LIKE '{1}') OR", Split(i))));
         var result = _model.ReadAnonymous(new { Schema = "", Table = "", HasTriggers = false },
            String.Format(
               @"SELECT t.TABLE_SCHEMA, t.TABLE_NAME, ISNULL(tr.HasTriggers, 0) AS [HasTriggers]
                  FROM INFORMATION_SCHEMA.TABLES t 
                  LEFT JOIN (SELECT parent_id, CONVERT(BIT, COUNT(*)) AS [HasTriggers] FROM sys.triggers GROUP BY parent_id) tr ON tr.parent_id = object_id(t.TABLE_SCHEMA + '.' + t.TABLE_NAME, 'U')
                  WHERE {0} AND t.TABLE_TYPE = 'BASE TABLE' 
                  ORDER BY t.TABLE_NAME", likes.Substring(0, likes.Length - 2))
            );

         return result.Select(i => new Tuple<string, string, bool>(i.Schema, i.Table, i.HasTriggers));
      }

      private static object[] Split(string s)
      {
         var split = s.Split('.');

         if (split.Length == 1)
         {
            return new object[] { "dbo", split[0] };
         }
         else
         {
            return split;
         }
      }

      private IEnumerable<ITableInfo> GetTableDefs(IEnumerable<Tuple<string, string, bool>> tables)
      {
         var result = new List<ITableInfo>();

         foreach (var table in tables)
         {
            var gnu = GetTableDef(table.Item1, table.Item2, table.Item3);
            result.Add(gnu);
         }

         return result;
      }

      private ITableInfo GetTableDef(string schemaName, string tableName, bool hasTriggers)
      {
         return new TableInfo(schemaName, tableName, hasTriggers, GetFieldDefs(schemaName, tableName));
      }

      private IEnumerable<IColumnInfo> GetFieldDefs(string schemaName, string tableName)
      {
         var result = new List<IColumnInfo>();
         var fields = _model.ReadAnonymous(new { ColumnName = "", DataType = "", IsNullable = "", ColumnDefault = "", OrdinalPosition = 0 },
            @"SELECT c.COLUMN_NAME, c.DATA_TYPE, c.IS_NULLABLE, c.COLUMN_DEFAULT, c.ORDINAL_POSITION
               FROM INFORMATION_SCHEMA.COLUMNS c
              WHERE c.TABLE_SCHEMA = @0
                AND c.TABLE_NAME = @1
              ORDER BY c.ORDINAL_POSITION"
            , schemaName
            , tableName
            );

         var pknames = _model.ReadValues<string>(
            @"SELECT c.name
                FROM sys.index_columns ic  
                JOIN sys.indexes i ON i.index_id = ic.index_id AND i.object_id = ic.object_id AND i.is_primary_key = 1
                JOIN sys.tables t ON i.object_id = t.object_id  
                JOIN sys.schemas s ON s.schema_id = t.schema_id  
                JOIN sys.columns c ON ic.column_id = c.column_id AND c.object_id = t.object_id
               WHERE t.name = @1
                 AND s.name = @0"
            , schemaName
            , tableName
            );

         var idnames = _model.ReadValues<string>(
            @"SELECT ic.name
                FROM sys.identity_columns ic
                JOIN sys.tables t ON ic.object_id = t.object_id
                JOIN sys.schemas s ON s.schema_id = t.schema_id
               WHERE t.Name = @1
                 AND s.name = @0"
            , schemaName
            , tableName
            );

         var cpnames = _model.ReadValues<string>(
            @"SELECT cc.name
                FROM sys.computed_columns cc  
                JOIN sys.tables t ON cc.object_id = t.object_id
                JOIN sys.schemas s ON s.schema_id = t.schema_id  
               WHERE t.name = @1
                 AND s.name = @0

               UNION 

              SELECT c.name
                FROM sys.columns c
                JOIN sys.tables t ON c.object_id = t.object_id
                JOIN sys.schemas s ON s.schema_id = t.schema_id  
               WHERE t.name = @1
                 AND s.name = @0
                 AND c.system_type_id = 189 -- timestamp"
            , schemaName
            , tableName
            );

         var primaries = new HashSet<string>(pknames);
         var identities = new HashSet<string>(idnames);
         var computeds = new HashSet<string>(cpnames);

         foreach (var field in fields)
         {
            var sqlType = field.IsNullable == "YES" ? field.DataType + "?" : field.DataType;

            string clrType;

            if (!_types.TryGetValue(sqlType, out clrType))
            {
               continue;
            }

            var gnu = new MsSqlColumnInfo(field.ColumnName, field.DataType, clrType, field.ColumnDefault, field.IsNullable == "YES", primaries.Contains(field.ColumnName), identities.Contains(field.ColumnName), field.OrdinalPosition, computeds.Contains(field.ColumnName));

            result.Add(gnu);
         }

         return result;
      }

      #endregion PRIVATE METHODS ---------------------------------------------------------------------
   }
}