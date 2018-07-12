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

namespace DrivenDb.Utility
{
   internal class DisposableCollection : IDisposable
   {
      private readonly List<IDisposable> m_Disposables = new List<IDisposable>();
      private bool m_IsDisposed = false;

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      ~DisposableCollection()
      {
         Dispose(false);
      }

      public void Add(params IDisposable[] disposables)
      {
         if (disposables != null && disposables.Length > 0)
         {
            m_Disposables.AddRange(disposables);
         }
      }

      private void Dispose(bool disposing)
      {
         if (!m_IsDisposed)
         {
            if (disposing)
            {
               foreach (var disposable in m_Disposables)
               {
                  disposable.Dispose();
               }
            }
         }

         m_IsDisposed = true;
      }
   }
}