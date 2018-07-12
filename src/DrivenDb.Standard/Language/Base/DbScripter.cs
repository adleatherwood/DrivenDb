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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DrivenDb.Language.Interfaces;

namespace DrivenDb.Base
{
   internal class DbScripter : IDbScripter
   {
      private static readonly Regex m_Indexer = new Regex(@"@([0-9]+)($|[^0-9])");
      private readonly IDb m_Db;
      protected readonly Func<ISqlBuilder> m_Builders;
      protected bool m_HasScriptedTriggeredEntity;

      public DbScripter(IDb db, Func<ISqlBuilder> builders)
      {
         m_Db = db;
         m_Builders = builders;
      }

      protected DateTime CorrectDateTime(DateTime dateTime)
      {
         return m_Builders().CorrectDateTime(dateTime);
      }

      public virtual void InsertWriteInitializer(IDbCommand command)
      {

      }

      public virtual void AppendWriteFinalizer(IDbCommand command)
      {

      }

      public void ScriptExecute(IDbCommand command, string query, params object[] parameters)
      {
         AppendQuery(command, -1, query, parameters);
      }

      public void ScriptArray<T>(IDbCommand command, string query, params object[] parameters)
          where T : struct
      {
         AppendQuery(command, -1, query, parameters);
      }

      public void ScriptSelect(IDbCommand command, string query, params object[] parameters)
      {
         AppendQuery(command, -1, query, parameters);
      }

      public void ScriptIdentitySelect<T>(IDbCommand command, params object[] parameters)
          where T : IDbRecord, new()
      {
         var metadata = new T();

         if (parameters == null || metadata.PrimaryColumns == null || metadata.PrimaryColumns.Length != parameters.Length)
         {
            throw new InvalidOperationException("Primary key columns and parameter counts do not match");
         }

         var count = 0;
         var builder = m_Builders();

         builder.Schema = (metadata.TableOverride ?? metadata.Table).Schema;
         builder.Table = (metadata.TableOverride ?? metadata.Table).Name;

         metadata.Columns.Values.ForEach(
             c => builder.AddColumn(c.Name)
             );

         metadata.PrimaryColumns.ForEach(
             c => builder.AddWhere(c.Name, count++)
             );

         AppendQuery(command, -1, builder.ToSelect(), parameters);
      }

      public void ScriptUpdate<T>(IDbCommand command, int index, T entity)
          where T : IDbEntity
      {
         var builder = m_Builders();
         var parameters = InitializeBuilderForUpdate(builder, entity);

         AppendQuery(command, index, builder.ToUpdate(), parameters);
      }

      protected IEnumerable<object> InitializeBuilderForUpdate<T>(ISqlBuilder builder, T entity) where T : IDbEntity
      {
         var metadata = entity;

         if (metadata.PrimaryColumns == null || metadata.PrimaryColumns.Length == 0)
         {
            throw new InvalidDataException("No primary key defined.");
         }

         var count = 0;

         builder.Schema = (metadata.TableOverride ?? metadata.Table).Schema;
         builder.Table = (metadata.TableOverride ?? metadata.Table).Name;

         // ReSharper disable AccessToModifiedClosure
         metadata.Changes.ForEach(
            c => builder.AddSetter(c, count++)
            );

         metadata.PrimaryColumns.ForEach(
            k => builder.AddWhere(k.Name, count++)
            );
         // ReSharper restore AccessToModifiedClosure

         return entity.Changes.Select(entity.GetProperty)
            .Concat(
               entity.PrimaryColumns
                  .Select(p => entity.GetProperty(p.Name.Trim())) // todo: this trim here is a fix to a sql column have training space in it's name.  it's not optimal, but no other solution is presenting itself to me.
            );
      }

      public void ScriptDelete<T>(IDbCommand command, int index, T entity)
          where T : IDbEntity
      {
         var builder = m_Builders();
         var parameters = InitializeBuilderForDelete(builder, entity);

         AppendQuery(command, index, builder.ToDelete(), parameters);
      }

      protected static IEnumerable<object> InitializeBuilderForDelete<T>(ISqlBuilder builder, T entity) where T : IDbEntity
      {
         var metadata = entity;

         if (metadata.PrimaryColumns == null || metadata.PrimaryColumns.Length == 0)
         {
            throw new InvalidDataException("No primary key defined.");
         }

         var count = 0;

         builder.Schema = (metadata.TableOverride ?? metadata.Table).Schema;
         builder.Table = (metadata.TableOverride ?? metadata.Table).Name;

         metadata.PrimaryColumns.ForEach(
            k => builder.AddWhere(k.Name, count++)
            );

         var parameters = entity.PrimaryColumns.Select(
            p => entity.GetProperty(p.Name.Trim()) // todo: this trim here is a fix to a sql column have training space in it's name.  it's not optimal, but no other solution is presenting itself to me.
            );
         return parameters;
      }

      public void ScriptRelatedSelect<P, C>(IEnumerable<P> parents, IDbCommand command, string[] pkey, string[] ckey)
         where P : IDbRecord
         where C : IDbRecord, new()
      {
         var metadata = new C();

         if (pkey == null || pkey.Length == 0 || ckey == null || ckey.Length == 0 || pkey.Length != ckey.Length)
         {
            throw new InvalidOperationException("Invalid number of joining fields defined");
         }

         var count = 0;
         var builder = m_Builders();

         builder.Schema = (metadata.TableOverride ?? metadata.Table).Schema;
         builder.Table = (metadata.TableOverride ?? metadata.Table).Name;

         metadata.Columns.Values.ForEach(c => builder.AddColumn(c.Name));

         parents.ForEach(
             p =>
             {
                ckey.ForEach(c => builder.AddWhere(c, count++));
                builder.GroupWhere();
             });

         var parameters = parents.SelectMany(
             p => pkey.Select(p.GetProperty)
             );

         AppendQuery(command, -1, builder.ToSelect(), parameters);
      }

      public void ScriptInsert<T>(IDbCommand command, int index, T entity, bool returnId)
         where T : IDbEntity
      {
         var builder = m_Builders();
         var parameters = InitializeBuilderForInsert(builder, entity);

         if (entity.Table.HasTriggers)
         {
            m_HasScriptedTriggeredEntity = true;
         }

         AppendQuery(command, index, builder.ToInsert(entity, index, returnId), parameters);
      }

      protected static IEnumerable<object> InitializeBuilderForInsert<T>(ISqlBuilder builder, T entity) where T : IDbEntity
      {
         var count = 0;
         var metadata = entity;

         builder.Schema = (metadata.TableOverride ?? metadata.Table).Schema;
         builder.Table = (metadata.TableOverride ?? metadata.Table).Name;

         entity.Changes.ForEach(c => builder.AddSetter(c, count++));

         var parameters = entity.Changes.Select(
            entity.GetProperty
            );
         return parameters;
      }

      protected void AppendQuery(IDbCommand command, int index, string query, IEnumerable<object> parameters)
      {
         var gnuquery = "";
         var gnuparameters = ManipulateQuery(out gnuquery, index, query, (parameters ?? new object[0]).ToArray());

         command.CommandText += gnuquery + Environment.NewLine;

         AppendParameters(command, index, gnuparameters);
      }

      private IEnumerable<object> ManipulateQuery(out string outputQuery, int queryIndex, string inputQuery, object[] parameters)
      {
         var values = new List<object>();

         outputQuery = ReplaceParameterNames(
            AddPrefixIfNeeded(queryIndex, inputQuery),
            queryIndex,
            AccumulateParametersAndBuildIndexMaps(parameters, values));

         return values;
      }

      private static string ReplaceParameterNames(string inputQuery, int queryIndex, IEnumerable<IndexMap> substitutionList)
      {
         var prefix = ToPrefix(queryIndex);

         // Replace in reverse order to not double replace any enumerable parameter replacements that might clash with original
         // parameter names. e.g. In '... [Foo] IN (@0) AND [Bar] = @1', if the enumerable for `@0` has at least two items,
         // replacement for `@1` needs to happen first or else the result will be '... [Foo] IN (@0,@2) AND [Bar] = @2' instead
         // of '... [Foo] IN (@0,@1) AND [Bar] = @2' as intended.
         substitutionList.GroupBy(item => item.OriginalIndex).Where(IsMappedToDifferentName).Reverse().ForEach(group =>
         {
            inputQuery = Regex.Replace(
               inputQuery,
               GetParameterName(prefix, group.Key) + "($|[^0-9])",
               GetParameterName(prefix, group.ToList()) + "$1");
         });

         return inputQuery;
      }

      private static bool IsMappedToDifferentName(IGrouping<int, IndexMap> grouping)
      {
         return grouping.Count() > 1 || grouping.Key != grouping.First().NewIndex;
      }

      private struct IndexMap
      {
         public int OriginalIndex;
         public int NewIndex;
      }

      private IEnumerable<IndexMap> AccumulateParametersAndBuildIndexMaps(IEnumerable<object> parameters, ICollection<object> values)
      {
         var substitutionList = new List<IndexMap>();

         parameters.ForEach((parameter, index) =>
         {
            if (IsEnumerableParameter(parameter))
            {
               // IsEnumerableParameter already ensures that `parameter` is not null if we hit this branch
               // ReSharper disable once AssignNullToNotNullAttribute
               var enumerable = (parameter as IEnumerable).Cast<object>().ToList();

               substitutionList.AddRange(HandleEnumerableParameter(enumerable, index, values));
            }
            else
            {
               substitutionList.Add(HandleParameter(parameter, index, values));
            }
         });

         return substitutionList;
      }

      private IndexMap HandleParameter(object parameter, int index, ICollection<object> values)
      {
         values.Add(GetParameterValue(parameter));
         return new IndexMap()
         {
            OriginalIndex = index,
            NewIndex = values.Count - 1
         };
      }

      private IEnumerable<IndexMap> HandleEnumerableParameter(List<object> parameter, int index,
         ICollection<object> values)
      {
         GuardAgainstEnumerableParametersNotAllowed();
         GuardAgainstEmptyEnumerableParameters(parameter);

         return parameter.Select(enumParam => HandleParameter(enumParam, index, values));
      }

      private void GuardAgainstEnumerableParametersNotAllowed()
      {
         if (!m_Db.AllowEnumerableParameters)
         {
            throw new InactiveExtensionException("AllowEnumerableParameters");
         }
      }

      private static void GuardAgainstEmptyEnumerableParameters<T>(IEnumerable<T> enumerable)
      {
         if (!enumerable.Any())
         {
            throw new ArgumentException("Empty enumeration parameter not allowed");
         }
      }

      private object GetParameterValue(object parameter)
      {
         //
         // convert convertable parameters
         //
         var convertable = parameter as IParamConvertible;
         if (convertable != null)
         {
            return convertable.ToParameterValue();
         }

         //
         // swap null for DBNull.Value
         //
         if (parameter == null)
         {
            return DBNull.Value;
         }

         //
         // pass through IDbDataParameters
         //
         var dbparameter = parameter as IDbDataParameter;
         if (dbparameter != null)
         {
            return dbparameter;
         }

         //
         // if datetime, parameter check min/max values
         //
         if (parameter is DateTime)
         {
            var dateTime = (DateTime)parameter;
            return m_Db.LimitDateParameters ? CorrectDateTime(dateTime) : dateTime;
         }

         //
         // if datetimeoffset, parameter check min/max values
         //
         if (parameter is DateTimeOffset)
         {
            var dateTime = ((DateTimeOffset)parameter).DateTime;
            return m_Db.LimitDateParameters ? CorrectDateTime(dateTime) : dateTime;
         }

         return parameter;
      }

      private static bool IsEnumerableParameter(object parameter)
      {
         return (parameter as string) == null
            && (parameter as byte[]) == null
            && (parameter as IEnumerable) != null;
      }

      private void AppendParameters(IDbCommand command, int index, IEnumerable<object> parameters)
      {
         var prefix = ToPrefix(index);

         parameters.ForEach((parameter, i) =>
         {
            if (parameter is IDbDataParameter)
            {
               command.Parameters.Add(parameter);
            }
            else
            {
               var gnu = command.CreateParameter();

               gnu.ParameterName = GetParameterName(prefix, i);
               gnu.Value = parameter;

               if (m_Db.DefaultStringParametersToAnsiString && parameter is string)
               {
                  gnu.DbType = DbType.AnsiString;
               }

               command.Parameters.Add(gnu);
            }
         });
      }

      private static string AddPrefixIfNeeded(int index, string inputQuery)
      {
         return index > -1
            ? m_Indexer.Replace(inputQuery, "@" + ToPrefix(index) + "$1$2")
            : inputQuery;
      }

      private static string GetParameterName(string prefix, int parameterIndex)
      {
         return "@" + prefix + parameterIndex;
      }

      private static string GetParameterName(string prefix, IEnumerable<IndexMap> parameterIndexes)
      {
         return string.Join(",", parameterIndexes.Select(indexMap => GetParameterName(prefix, indexMap.NewIndex)));
      }

      private static string ToPrefix(int index)
      {
         return index > -1 ? index + "_" : null;
      }
   }
}
