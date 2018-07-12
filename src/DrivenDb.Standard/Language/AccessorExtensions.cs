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

namespace DrivenDb
{
   [Flags]
   public enum AccessorExtension
   {
      None = 0,
      AllowEnumerableParameters = 1,
      LimitDateParameters = 2,
      AllowUnmappedColumns = 4,
      CaseInsensitiveColumnMapping = 8,
      PrivateMemberColumnMapping = 16,
      DefaultStringParametersToAnsiString = 32,
      
      Common = AllowEnumerableParameters
         | LimitDateParameters
         | AllowUnmappedColumns
         | CaseInsensitiveColumnMapping,
         
      All = AllowEnumerableParameters
         | LimitDateParameters
         | AllowUnmappedColumns
         | CaseInsensitiveColumnMapping
         | PrivateMemberColumnMapping
         | DefaultStringParametersToAnsiString
   }
}
