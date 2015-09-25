﻿/**************************************************************************************
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
      private readonly IValueJoiner m_Joiner;
      protected readonly Func<ISqlBuilder> m_Builders;
      protected bool m_HasScriptedTriggeredEntity;

      public DbScripter(IDb db, IValueJoiner joiner, Func<ISqlBuilder> builders)
      {
         m_Db = db;
         m_Builders = builders;
         m_Joiner = joiner;
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

      private IEnumerable<object> ManipulateQuery(out string outputQuery, int index, string inputQuery, object[] parameters)
      {
         var values = new List<object>();
         var prefix = index > -1 ? ToPrefix(index) : null;
         var decrement = 0;

         if (index > -1)
         {
            inputQuery = m_Indexer.Replace(inputQuery, "@" + prefix + "$1$2");
         }

         for (var i = 0; parameters != null && i < parameters.Length; i++)
         {
            var name = "@" + prefix + i;
            var gnuname = "@" + prefix + (i - decrement);

            if (decrement > 0)
            {
               inputQuery = Regex.Replace(inputQuery, name + "($|[^0-9])", gnuname + "$1");
            }

            //
            // convert convertable parameters
            //
            var convertable = parameters[i] as IParamConvertible;

            if (convertable != null)
            {
               parameters[i] = convertable.ToParameterValue();
            }

            //
            // swap null for DBNull.Value
            //
            if (parameters[i] == null)
            {
               values.Add(DBNull.Value);
               continue;
            }

            //
            // pass through IDbDataParameters
            //
            var dbparameter = parameters[i] as IDbDataParameter;

            if (dbparameter != null)
            {
               values.Add(dbparameter);
               continue;
            }

            //
            // if ienumerable, replace parameter with a delimited list
            //
            var sparameter = parameters[i] as string;
            var bparameter = parameters[i] as byte[];

            if (sparameter == null && bparameter == null)
            {
               var enumerable = parameters[i] as IEnumerable;

               if (enumerable != null)
               {
                  if (!m_Db.AllowEnumerableParameters)
                  {
                     throw new InactiveExtensionException("AllowEnumerableParameters");
                  }

                  if (!enumerable.GetEnumerator().MoveNext())
                  {
                     throw new ArgumentException("Empty enumeration parameter not allowed");
                  }

                  inputQuery = Regex.Replace(inputQuery, "(" + gnuname + ")($|[^0-9])", m_Joiner.Join(enumerable) + "$2");
                  decrement++;
                  continue;
               }
            }

            //
            // if datetime, parameter check min/max values
            //                        
            if (parameters[i] is DateTime)
            {
               var dateTime = (DateTime)parameters[i];

               if (m_Db.LimitDateParameters)
                  values.Add(CorrectDateTime(dateTime));
               else
                  values.Add(dateTime);

               continue;
            }

            //
            // if datetimeoffset, parameter check min/max values
            //
            if (parameters[i] is DateTimeOffset)
            {
               var dateTime = ((DateTimeOffset)parameters[i]).DateTime;
               
               if (m_Db.LimitDateParameters)
                  values.Add(CorrectDateTime(dateTime));
               else
                  values.Add(dateTime);

               continue;
            }

            values.Add(parameters[i]);
         }

         outputQuery = inputQuery;

         return values;
      }

      private void AppendParameters(IDbCommand command, int index, IEnumerable<object> parameters)
      {
         var i = -1;
         var prefix = index > -1 ? ToPrefix(index) : null;

         foreach (var parameter in parameters)
         {
            i++;

            if (parameter is IDbDataParameter)
            {
               command.Parameters.Add(parameter);
            }
            else
            {
               var gnu = command.CreateParameter();

               gnu.ParameterName = "@" + prefix + i;
               gnu.Value = parameter;

               if (m_Db.DefaultStringParametersToAnsiString && parameter is string)               
               {
                  gnu.DbType = DbType.AnsiString;
               }

               command.Parameters.Add(gnu);
            }
         }
      }

      private static string ToPrefix(int index)
      {
         return index + "_";         
      }
   }
}
