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

using DrivenDbConsole.Contracts;
using System.Collections.Generic;
using System.Text;

namespace DrivenDbConsole.Generator
{
   internal class TypeWriter
   {
      private readonly StringBuilder m_Output = new StringBuilder();

      public string Execute(IDatabaseInfo database)
      {         
         WriteTables(database.Tables);

         return m_Output.ToString();
      }
      
      private void WriteTables(IEnumerable<ITableInfo> tableDefs)
      {
         foreach (var table in tableDefs)
         {
            //
            // DECLARATION
            //
            WriteLine("\t[DataContract]");
            
            WriteLine("\tpublic partial class " + table.Name + "ReadOnly");
            WriteLine("\t{");

            //
            // MEMBERS
            //
            WriteLine("");

            foreach (var field in table.Fields)
            {
               WriteLine("\t\t[DataMember]");
               WriteLine("\t\tprivate " + field.ClrType + " _" + field.Name + ";");
            }

            WriteLine("");

            //
            // PROPERTIES
            //
            foreach (var field in table.Fields)
            {               
               WriteLine("\t\tpublic " + field.ClrType + " " + field.Name);
               WriteLine("\t\t{");
               WriteLine("\t\t\tget { return _" + field.Name + "; }");
               WriteLine("\t\t}");
               WriteLine("");
            }

            //
            // CLOSE
            //
            WriteLine("\t}");
            WriteLine("");
         }
      }

      private void WriteLine(string text)
      {
         m_Output.AppendLine(text);
      }
   }
}