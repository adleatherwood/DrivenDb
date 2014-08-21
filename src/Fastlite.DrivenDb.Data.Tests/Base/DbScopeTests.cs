﻿using System.Linq;
using Fastlite.DrivenDb.Data.Tests.Base.Infrastructure;
using Fastlite.DrivenDb.Data.Tests.Base.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fastlite.DrivenDb.Data.Tests.Base
{
   public abstract class DbScopeTests : DbTestClass
   {
      [TestMethod]
      public void DbScope_CommitsData()
      {
         using (var fixture = CreateFixture())
         {
            var accessor = fixture.CreateAccessor()
               .WithAllExtensions()
               .Build();

            var gnu4 = new MyTable()
            {
               MyNumber = 4,
               MyString = "Four"
            };
            var gnu5 = new MyTable()
            {
               MyNumber = 5,
               MyString = "Five"
            };
            var gnu6 = new MyTable()
            {
               MyNumber = 6,
               MyString = "Six"
            };

            using (var scope = accessor.CreateScope())
            {
               scope.WriteEntity(gnu4);
               scope.WriteEntities(new[] { gnu5, gnu6 });
               scope.Commit();
            }

            var committed = accessor.ReadEntities<MyTable>(
               @"SELECT * FROM MyTable"
               );

            Assert.AreEqual(6, committed.Count());
         }
      }

      [TestMethod]
      public void DbScope_RollsbackData()
      {
         using (var fixture = CreateFixture())
         {
            var accessor = fixture.CreateAccessor()
               .WithAllExtensions()
               .Build();

            var gnu4 = new MyTable()
            {
               MyNumber = 4,
               MyString = "Four"
            };
            var gnu5 = new MyTable()
            {
               MyNumber = 5,
               MyString = "Five"
            };
            var gnu6 = new MyTable()
            {
               MyNumber = 6,
               MyString = "Six"
            };

            using (var scope = accessor.CreateScope())
            {
               scope.WriteEntity(gnu4);
               scope.WriteEntities(new[] { gnu5, gnu6 });
            }

            var committed = accessor.ReadEntities<MyTable>(
               @"SELECT * FROM MyTable"
               );

            Assert.AreEqual(3, committed.Count());
         }
      }

      [TestMethod]
      public void DbScope_AvoidsExecutionWhenAllEntitiesAreCurrent()
      {
         using (var fixture = CreateFixture())
         {
            var accessor = fixture.CreateAccessor()
               .WithAllExtensions()
               .Build();

            var entities = accessor.ReadEntities<MyTable>("SELECT * FROM MyTable");

            using (var scope = accessor.CreateScope())
            {
               scope.WriteEntities(entities);
               scope.Commit();
            }
         }
      }

      [TestMethod]
      public void DbScope_ExecuteCommitsSuccessfully()
      {
         using (var fixture = CreateFixture())
         {
            var accessor = fixture.CreateAccessor()
               .WithAllExtensions()
               .Build();

            using (var scope = accessor.CreateScope())
            {
               scope.Execute("UPDATE MyTable SET MyString = 'testeroo'");
               scope.Execute("UPDATE MyTable SET MyNumber = 555");
               scope.Commit();
            }

            var entities = accessor.ReadEntities<MyTable>("SELECT * FROM MyTable");

            Assert.IsTrue(entities.All(e => e.MyNumber == 555));
            Assert.IsTrue(entities.All(e => e.MyString == "testeroo"));
         }
      }

      [TestMethod]
      public void DbScope_ExecuteRollsbackSuccessfully()
      {
         using (var fixture = CreateFixture())
         {
            var accessor = fixture.CreateAccessor()
               .WithAllExtensions()
               .Build();

            using (var scope = accessor.CreateScope())
            {
               scope.Execute("UPDATE MyTable SET MyString = 'testeroo'");
               scope.Execute("UPDATE MyTable SET MyNumber = 555");
            }

            var entities = accessor.ReadEntities<MyTable>("SELECT * FROM MyTable");

            Assert.IsTrue(entities.All(e => e.MyNumber != 555));
            Assert.IsTrue(entities.All(e => e.MyString != "testeroo"));
         }
      }
   }
}