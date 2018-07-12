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

using System.Collections.Generic;

namespace DrivenDb
{
   public class DbSet<T1>
   {
      public DbSet(IEnumerable<T1> set1)
      {
         Set1 = set1;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2)
      {
         Set1 = set1;
         Set2 = set2;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2, T3>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2, IEnumerable<T3> set3)
      {
         Set1 = set1;
         Set2 = set2;
         Set3 = set3;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }

      public IEnumerable<T3> Set3
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2, T3, T4>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2, IEnumerable<T3> set3, IEnumerable<T4> set4)
      {
         Set1 = set1;
         Set2 = set2;
         Set3 = set3;
         Set4 = set4;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }

      public IEnumerable<T3> Set3
      {
         get;
         private set;
      }

      public IEnumerable<T4> Set4
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2, T3, T4, T5>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2, IEnumerable<T3> set3, IEnumerable<T4> set4, IEnumerable<T5> set5)
      {
         Set1 = set1;
         Set2 = set2;
         Set3 = set3;
         Set4 = set4;
         Set5 = set5;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }

      public IEnumerable<T3> Set3
      {
         get;
         private set;
      }

      public IEnumerable<T4> Set4
      {
         get;
         private set;
      }

      public IEnumerable<T5> Set5
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2, T3, T4, T5, T6>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2, IEnumerable<T3> set3, IEnumerable<T4> set4, IEnumerable<T5> set5, IEnumerable<T6> set6)
      {
         Set1 = set1;
         Set2 = set2;
         Set3 = set3;
         Set4 = set4;
         Set5 = set5;
         Set6 = set6;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }

      public IEnumerable<T3> Set3
      {
         get;
         private set;
      }

      public IEnumerable<T4> Set4
      {
         get;
         private set;
      }

      public IEnumerable<T5> Set5
      {
         get;
         private set;
      }

      public IEnumerable<T6> Set6
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2, T3, T4, T5, T6, T7>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2, IEnumerable<T3> set3, IEnumerable<T4> set4, IEnumerable<T5> set5, IEnumerable<T6> set6, IEnumerable<T7> set7)
      {
         Set1 = set1;
         Set2 = set2;
         Set3 = set3;
         Set4 = set4;
         Set5 = set5;
         Set6 = set6;
         Set7 = set7;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }

      public IEnumerable<T3> Set3
      {
         get;
         private set;
      }

      public IEnumerable<T4> Set4
      {
         get;
         private set;
      }

      public IEnumerable<T5> Set5
      {
         get;
         private set;
      }

      public IEnumerable<T6> Set6
      {
         get;
         private set;
      }

      public IEnumerable<T7> Set7
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2, T3, T4, T5, T6, T7, T8>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2, IEnumerable<T3> set3, IEnumerable<T4> set4, IEnumerable<T5> set5, IEnumerable<T6> set6, IEnumerable<T7> set7, IEnumerable<T8> set8)
      {
         Set1 = set1;
         Set2 = set2;
         Set3 = set3;
         Set4 = set4;
         Set5 = set5;
         Set6 = set6;
         Set7 = set7;
         Set8 = set8;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }

      public IEnumerable<T3> Set3
      {
         get;
         private set;
      }

      public IEnumerable<T4> Set4
      {
         get;
         private set;
      }

      public IEnumerable<T5> Set5
      {
         get;
         private set;
      }

      public IEnumerable<T6> Set6
      {
         get;
         private set;
      }

      public IEnumerable<T7> Set7
      {
         get;
         private set;
      }

      public IEnumerable<T8> Set8
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2, T3, T4, T5, T6, T7, T8, T9>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2, IEnumerable<T3> set3, IEnumerable<T4> set4, IEnumerable<T5> set5, IEnumerable<T6> set6, IEnumerable<T7> set7, IEnumerable<T8> set8, IEnumerable<T9> set9)
      {
         Set1 = set1;
         Set2 = set2;
         Set3 = set3;
         Set4 = set4;
         Set5 = set5;
         Set6 = set6;
         Set7 = set7;
         Set8 = set8;
         Set9 = set9;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }

      public IEnumerable<T3> Set3
      {
         get;
         private set;
      }

      public IEnumerable<T4> Set4
      {
         get;
         private set;
      }

      public IEnumerable<T5> Set5
      {
         get;
         private set;
      }

      public IEnumerable<T6> Set6
      {
         get;
         private set;
      }

      public IEnumerable<T7> Set7
      {
         get;
         private set;
      }

      public IEnumerable<T8> Set8
      {
         get;
         private set;
      }

      public IEnumerable<T9> Set9
      {
         get;
         private set;
      }
   }

   public class DbSet<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
   {
      public DbSet(IEnumerable<T1> set1, IEnumerable<T2> set2, IEnumerable<T3> set3, IEnumerable<T4> set4, IEnumerable<T5> set5, IEnumerable<T6> set6, IEnumerable<T7> set7, IEnumerable<T8> set8, IEnumerable<T9> set9, IEnumerable<T10> set10)
      {
         Set1 = set1;
         Set2 = set2;
         Set3 = set3;
         Set4 = set4;
         Set5 = set5;
         Set6 = set6;
         Set7 = set7;
         Set8 = set8;
         Set9 = set9;
         Set10 = set10;
      }

      public IEnumerable<T1> Set1
      {
         get;
         private set;
      }

      public IEnumerable<T2> Set2
      {
         get;
         private set;
      }

      public IEnumerable<T3> Set3
      {
         get;
         private set;
      }

      public IEnumerable<T4> Set4
      {
         get;
         private set;
      }

      public IEnumerable<T5> Set5
      {
         get;
         private set;
      }

      public IEnumerable<T6> Set6
      {
         get;
         private set;
      }

      public IEnumerable<T7> Set7
      {
         get;
         private set;
      }

      public IEnumerable<T8> Set8
      {
         get;
         private set;
      }

      public IEnumerable<T9> Set9
      {
         get;
         private set;
      }

      public IEnumerable<T10> Set10
      {
         get;
         private set;
      }
   }
}