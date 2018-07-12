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

using System.Data;

namespace DrivenDb
{
    public interface IDb
    {
        bool AllowEnumerableParameters
        {
            get;
        }
        
        bool LimitDateParameters
        {
            get;
        }

       int ParameterLimit
       {
          get;
       }

       int ParameterNameLimit
       {
          get;
       }

       bool AllowUnmappedColumns
       {
          get;
       }

       bool CaseInsensitiveColumnMapping
       {
          get;
       }

       bool PrivateMemberColumnMapping
       {
          get;
       }

       bool DefaultStringParametersToAnsiString
       {
          get;
       }

       bool SetAnsiNullsOff
       {
           get; 
       }

       IDbConnection CreateConnection();
    }
}
