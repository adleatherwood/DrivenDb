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
    public class MissingResultException : Exception
    {
        public MissingResultException(int expected, int found)
            : base(String.Format("'{0}' result sets where expected but only '{1}' {2} found", expected, found, found == 1 ? "was" : "were"))
        {
            ExpectedResultCount = expected;
            FoundResultCount = found;
        }

        public int ExpectedResultCount
        {
            get;
            private set;
        }

        public int FoundResultCount
        {
            get;
            private set;
        }
    }
}
