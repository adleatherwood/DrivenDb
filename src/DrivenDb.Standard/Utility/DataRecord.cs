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
using System.Data;
using System.Linq;

namespace DrivenDb.Utility
{
   class DataRecord : IDataRecord
   {
      private readonly Dictionary<string,object> m_Dictionary = new Dictionary<string, object>();
      private readonly string[] m_Names;
      private readonly object[] m_Values;

      public DataRecord(IEnumerable<string> names, IEnumerable<object> values)
      {
         m_Names = names.ToArray();
         m_Values = values.ToArray();

         for(var i = 0; i < m_Names.Length; i++)
         {
            m_Dictionary.Add(m_Names[i], m_Values[i]);
         }
      }

      public string GetName(int i)
      {
         return m_Names[i];
      }

      public string GetDataTypeName(int i)
      {
         return m_Values.GetType().Name;
      }

      public Type GetFieldType(int i)
      {
         return m_Values.GetType();
      }

      public object GetValue(int i)
      {
         return m_Values[i];
      }

      public int GetValues(object[] values)
      {
         m_Values.CopyTo(values, 0);
         return m_Values.Length;
      }

      public int GetOrdinal(string name)
      {         
         for(var i = 0; i < m_Names.Length; i++)
         {
            if (m_Names[i] == name)
            {
               return i;
            }
         }

         return -1;
      }

      public bool GetBoolean(int i)
      {
         return Convert.ToBoolean(m_Values[i]);
      }

      public byte GetByte(int i)
      {
         return Convert.ToByte(m_Values[i]);
      }

      public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
      {
         throw new NotSupportedException();
      }

      public char GetChar(int i)
      {
         return Convert.ToChar(m_Values[i]);
      }

      public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
      {
         throw new NotSupportedException();
      }

      public Guid GetGuid(int i)
      {
         return Guid.Parse((string) m_Values[i]);
      }

      public short GetInt16(int i)
      {
         return Convert.ToInt16(m_Values[i]);
      }

      public int GetInt32(int i)
      {
         return Convert.ToInt32(m_Values[i]);
      }

      public long GetInt64(int i)
      {
         return Convert.ToInt64(m_Values[i]);
      }

      public float GetFloat(int i)
      {
         return Convert.ToSingle(m_Values[i]);
      }

      public double GetDouble(int i)
      {
         return Convert.ToDouble(m_Values[i]);
      }

      public string GetString(int i)
      {
         return Convert.ToString(m_Values[i]);
      }

      public decimal GetDecimal(int i)
      {
         return Convert.ToDecimal(m_Values[i]);
      }

      public DateTime GetDateTime(int i)
      {
         return Convert.ToDateTime(m_Values[i]);
      }

      public IDataReader GetData(int i)
      {
         throw new NotSupportedException();
      }

      public bool IsDBNull(int i)
      {
         return DBNull.Value.Equals(m_Values[i]);
      }

      public int FieldCount
      {
         get { return m_Names.Length; }
      }

      object IDataRecord.this[int i]
      {
         get { return m_Values[i]; }
      }

      object IDataRecord.this[string name]
      {
         get { return m_Dictionary[name]; }
      }
   }
}
