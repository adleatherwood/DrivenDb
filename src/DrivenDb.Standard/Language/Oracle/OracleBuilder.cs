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
using DrivenDb.Base;

namespace DrivenDb.Oracle
{
    internal class OracleBuilder : SqlBuilder
    {
        private static readonly DateTime SQLMIN = DateTime.MinValue;
        private static readonly DateTime SQLMAX = new DateTime(9999, 12, 31, 0, 0, 0);
        private static readonly TimeSpan ZERO = new TimeSpan(0);

        public OracleBuilder()
            : base(null, null, "@", ";")
        {
        }

        public override string ToInsert<T>(T entity, int index, bool returnId)
        {
            var identityColumn = returnId && entity.IdentityColumn != null
                ? entity.IdentityColumn.Name + ", "
                : null;

            var identityValue = identityColumn != null
               ? String.Format("{0}.nextval, ", entity.Sequence.Name)
               : "";

            var identitySelect = identityColumn != null
               ? String.Format("SELECT {0}, {1}.currval FROM dual;", index, entity.Sequence.Name)
               : null;

            return String.Format("INSERT INTO {0} ({1}{2}) VALUES ({3}{4}); {5}", GetFormattedTable(), identityColumn, GetFormattedSetterColumns(), identityValue, GetFormattedSetterValues(), identitySelect);
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
