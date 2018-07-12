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
using System.Data;

namespace DrivenDb
{
   internal class Db : IDb
   {      
      private readonly DbAccessorType m_Type;
      private readonly AccessorExtension m_Extensions;
      private readonly Func<IDbConnection> m_Connections;

      public Db(AccessorExtension extensions, Func<IDbConnection> connections)
         : this(DbAccessorType.MsSql, extensions, connections)
      {         
      }

      public Db(DbAccessorType type, AccessorExtension extensions, Func<IDbConnection> connections)
      {         
         m_Type = type;
         m_Extensions = extensions;
         m_Connections = connections;

         switch(m_Type)
         {
            case DbAccessorType.MsSql:
               ParameterLimit = 1000;     // half of what is available
               ParameterNameLimit = 64;   // half of what is available
               break;
            default:
               ParameterLimit = 1000;     // using mssql's as a base
               ParameterNameLimit = 64;   // using mssql's as a base
               break;
         }
      }

      public DbAccessorType Type
      {
         get { return m_Type; }
      }

      public int ParameterLimit
      {
         get;
         private set;
      }

      public int ParameterNameLimit
      {
         get;
         private set;
      }

      public bool AllowEnumerableParameters
      {
         get { return (m_Extensions & AccessorExtension.AllowEnumerableParameters) == AccessorExtension.AllowEnumerableParameters; }
      }

      public bool LimitDateParameters
      {
         get { return (m_Extensions & AccessorExtension.LimitDateParameters) == AccessorExtension.LimitDateParameters; }
      }

      public bool AllowUnmappedColumns
      {
         get { return (m_Extensions & AccessorExtension.AllowUnmappedColumns) == AccessorExtension.AllowUnmappedColumns; }
      }

      public bool CaseInsensitiveColumnMapping
      {
         get { return (m_Extensions & AccessorExtension.CaseInsensitiveColumnMapping) == AccessorExtension.CaseInsensitiveColumnMapping; }
      }

      public bool PrivateMemberColumnMapping
      {
         get { return (m_Extensions & AccessorExtension.PrivateMemberColumnMapping) == AccessorExtension.PrivateMemberColumnMapping; }
      }

      public bool DefaultStringParametersToAnsiString
      {
         get { return (m_Extensions & AccessorExtension.DefaultStringParametersToAnsiString) == AccessorExtension.DefaultStringParametersToAnsiString; }
      }

      public bool SetAnsiNullsOff
      {
          get { return m_Type == DbAccessorType.MsSql; }
      }

        public IDbConnection CreateConnection()
      {
         return m_Connections();
      }
   }
}
