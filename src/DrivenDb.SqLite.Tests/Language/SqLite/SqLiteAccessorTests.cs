using System.Data;
using DrivenDb.Tests.Language.Interfaces;
using System;
using System.Data.SQLite;
using System.IO;

namespace DrivenDb.Tests.Language.SqLite
{
   public class SqLiteAccessorTests : IDbAccessorTests
   {
      protected override IDbAccessor CreateAccessor(out string key)
      {
         return CreateAccessor(out key, AccessorExtension.All);
      }

      protected override IDbAccessor CreateAccessor(out string key, AccessorExtension extensions)
      {
         var filename = key = Path.GetTempFileName();
         var accessor = DbFactory.CreateAccessor(
             DbAccessorType.SqLite, extensions, () => new SQLiteConnection(String.Format("Data Source={0};Version=3;New=True", filename))
             );

         accessor.Execute(@"
                CREATE TABLE MyTable
                (
                   [MyIdentity] INTEGER PRIMARY KEY AUTOINCREMENT,
                   [MyString] TEXT NULL,
                   [MyNumber] INTEGER NOT NULL
                );

                CREATE TABLE MyChildren
                (
                   [HisIdentity] INTEGER,
                   [MyString] TEXT NULL
                );

                CREATE TABLE MyFriend
                (
                   [MyIdentity] INTEGER PRIMARY KEY AUTOINCREMENT,
                   [MyString] TEXT NULL,
                   [MyNumber] INTEGER NOT NULL
                );

                INSERT INTO MyTable VALUES (null, 'One', 1);
                INSERT INTO MyTable VALUES (null, 'Two', 2);
                INSERT INTO MyTable VALUES (null, 'Three', 3);

                INSERT INTO MyChildren VALUES (1, 'Child 1/3');
                INSERT INTO MyChildren VALUES (1, 'Child 2/3');
                INSERT INTO MyChildren VALUES (1, 'Child 3/3');
                INSERT INTO MyChildren VALUES (3, 'Child 1/1');

                CREATE TABLE [NullableTest] (
                  [Value1] INT NULL
                );

                INSERT INTO [NullableTest] VALUES (null);
                INSERT INTO [NullableTest] VALUES (1);
                INSERT INTO [NullableTest] VALUES (null);
                INSERT INTO [NullableTest] VALUES (2);

                CREATE TABLE MyNopkTable
                (
                   [MyString] TEXT NULL,
                   [MyNumber] INTEGER NOT NULL
                );

                INSERT INTO MyNopkTable VALUES ('One', 1);
                INSERT INTO MyNopkTable VALUES ('Two', 2);
                INSERT INTO MyNopkTable VALUES ('Three', 3);
                ");

         //accessor.Log = Console.Out;

         return accessor;
      }

      protected override IDbDataParameter CreateParam<T>(string name, T value)
      {
         return new SQLiteParameter(name, value);
      }

      protected override void DestroyAccessor(string key)
      {
         File.Delete(key);
      }
   }
}