﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fastlite.DrivenDb.Tests.Framework
{
   [TestClass]
   public class DBRecordCollectionTests
   {
      [TestMethod]
      public void DBRecordCollection_IteratesSuppliedValues()
      {
         var values = new List<DbRecord<string>>()
            {
               new DbRecord<string>("", new[] {"n"}, new object[] {"a"}),
               new DbRecord<string>("", new[] {"n"}, new object[] {"b"}),
               new DbRecord<string>("", new[] {"n"}, new object[] {"c"}),
            };

         var sut = new DbRecordList<string>(values);

         Assert.AreEqual(3, sut.Count);         
      }
   }
}
