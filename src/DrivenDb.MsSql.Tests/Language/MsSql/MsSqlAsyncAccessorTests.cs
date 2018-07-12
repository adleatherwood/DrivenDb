using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DrivenDb.Async.Tests
{
   [TestClass]
   public class MsSqlAsyncAccessorTests : MsSqlAccessorTests
   {
      [TestMethod]
      public void ExecuteAsyncTest()
      {
         string filename;

         var accessor = (IDbAsyncAccessor)CreateAccessor(out filename);
         
         accessor.ExecuteAsync("UPDATE MyTable SET MyString = 'testeroo'")
            .GetAwaiter()
            .GetResult();

         var entities = accessor.ReadEntities<MyTable>("SELECT * FROM MyTable");

         Assert.IsTrue(entities.All(e => e.MyString == "testeroo"));

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void ReadValuesAsyncTest()
      {
         var filename = "";
         var accessor = (IDbAsyncAccessor)CreateAccessor(out filename);

         var actual = accessor.ReadValuesAsync<string>(@"SELECT [MyString] FROM [MyTable]")
            .GetAwaiter()
            .GetResult()
            .ToArray();

         Assert.AreEqual(3, actual.Length);
         Assert.IsTrue(actual.Contains("One"));
         Assert.IsTrue(actual.Contains("Two"));
         Assert.IsTrue(actual.Contains("Three"));

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void ReadValueAsyncTest()
      {
         var filename = "";
         var accessor = (IDbAsyncAccessor)CreateAccessor(out filename);

         var actual = accessor.ReadValueAsync<string>(@"SELECT [MyString] FROM [MyTable] ORDER BY [MyIdentity]")
            .GetAwaiter()
            .GetResult();

         Assert.AreEqual("One", actual);

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void ReadAnonymousAsyncTest()
      {
         var filename = "";
         var accessor = (IDbAsyncAccessor)CreateAccessor(out filename);

         var actual = accessor.ReadAnonymousAsync(new { MyIdentity = 0L, MyString = ""}, @"SELECT [MyIdentity], [MyString] FROM [MyTable] ORDER BY [MyIdentity]")
            .GetAwaiter()
            .GetResult()
            .ToArray();

         var one = actual[0];
         var two = actual[1];
         var three = actual[2];

         Assert.AreEqual(3, actual.Length);

         Assert.AreEqual(1, one.MyIdentity);
         Assert.AreEqual("One", one.MyString);

         Assert.AreEqual(2, two.MyIdentity);
         Assert.AreEqual("Two", two.MyString);

         Assert.AreEqual(3, three.MyIdentity);
         Assert.AreEqual("Three", three.MyString);

         DestroyAccessor(filename);
      }

      protected override IDbAccessor CreateAccessor(AccessorExtension extensions)
      {
         return DbAsyncFactory.CreateAccessor(
            DbAccessorType.MsSql, extensions,
            () => new SqlConnection(TEST_CSTRING)
            );
      }
   }
}
