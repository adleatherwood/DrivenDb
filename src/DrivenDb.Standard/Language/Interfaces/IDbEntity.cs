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

namespace DrivenDb
{
   public interface IDbEntity : IDbRecord
   {
      void Delete();
      void Undelete();
   }

   public interface IDbEntity<T> : IDbEntity, IDbRecord<T>
      where T : IDbEntity
   {
      T Clone();

      void Update(T other, bool checkIdentity = true);
      void Merge(T other, bool checkIdentity = true);
   }
}