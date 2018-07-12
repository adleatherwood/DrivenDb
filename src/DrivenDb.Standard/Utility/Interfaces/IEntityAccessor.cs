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
using System.Reflection;

namespace DrivenDb.Utility.Interfaces
{
   internal interface IEntityAccessor
   {
      bool HasProperty(string name);
      bool CanReadProperty(string name);
      bool CanWriteProperty(string name);
      Type GetType();
      IEnumerable<PropertyInfo> GetProperties();
      PropertyInfo GetPropertyInfo(string name);
   }
}