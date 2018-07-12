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
using System.Collections.Generic;
using System.Linq;

namespace DrivenDb.Base
{
   internal class EntityJoiner<P, C> : IWhereJoinable<P, C>, IEqualsJoinable<C>, IOnJoiner<P, C>
      where P : IDbRecord, new()
      where C : IDbRecord, new()
   {
      //private const int TEN_MINUTES = 600;

      private readonly int _timeout;
      private readonly IDb m_Db;
      private readonly IEnumerable<P> m_Instances;
      private readonly IDbMapper m_Mapper;
      private readonly IDbScripter m_Scripter;
      private string[] m_CKey;
      private string[] m_PKey;

      public EntityJoiner(int timeout, IDb db, IDbScripter scripter, IDbMapper mapper, IEnumerable<P> instances)
      {
         _timeout = timeout;
         m_Db = db;
         m_Scripter = scripter;
         m_Mapper = mapper;
         m_Instances = instances;
      }

      public IEnumerable<C> On<PK, CK>(Func<P, PK> pkey, Func<C, CK> ckey)
      {
         m_PKey = GetSymbols(pkey(new P()));
         m_CKey = GetSymbols(ckey(new C()));

         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = _timeout;

            m_Scripter.ScriptRelatedSelect<P, C>(m_Instances, command, m_PKey, m_CKey);

            //Debug.WriteLine(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapEntities<C>(command.CommandText, reader);
            }
         }
      }

      public IEnumerable<C> Equals<CK>(Func<C, CK> key)
      {
         m_CKey = GetSymbols(key(new C()));

         using (var connection = m_Db.CreateConnection())
         using (var command = connection.CreateCommand())
         {
            connection.Open();
            command.CommandTimeout = _timeout;

            m_Scripter.ScriptRelatedSelect<P, C>(m_Instances, command, m_PKey, m_CKey);

            //Debug.WriteLine(command.CommandText);

            using (var reader = command.ExecuteReader())
            {
               return m_Mapper.MapEntities<C>(command.CommandText, reader);
            }
         }
      }

      public IEqualsJoinable<C> Where<PK>(Func<P, PK> key)
      {
         m_PKey = GetSymbols(key(new P()));
         return this;
      }

      private static string[] GetSymbols<T>(T instance)
      {
         return instance.GetType().GetProperties().Select(p => p.Name).ToArray();
      }
   }
}