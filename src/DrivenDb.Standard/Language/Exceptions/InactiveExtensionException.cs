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
    public class InactiveExtensionException : Exception
    {
        public InactiveExtensionException(string extensionName)
            : base(String.Format("DrivenDb extension '{0}' is inactive.", extensionName))
        {
        }

        public InactiveExtensionException(string extensionName, string message)
           : base(String.Format("DrivenDb extension '{0}' is inactive.  {1}", extensionName, message))
        {
        }
    }
}
