using System.Data;
using MySql.Data.MySqlClient;
using System;
using DrivenDb.Tests.Language.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DrivenDb.Tests.Language.MySql
{
   [TestClass]
   public class MySqlAccessorTests : IDbAccessorTests
   {
      protected override IDbAccessor CreateAccessor(out string key)
      {
         return CreateAccessor(out key, AccessorExtension.All);
      }

      protected override IDbAccessor CreateAccessor(out string key, AccessorExtension extensions)
      {
         key = null;

         var accessor = DbFactory.CreateAccessor(
             DbAccessorType.MySql, extensions, () => new MySqlConnection("server=localhost;uid=root;pwd=;")
             );

         accessor.Execute(@"
                DROP DATABASE IF EXISTS DrivenDbTest;

                CREATE DATABASE DrivenDbTest;

                USE DrivenDbTest;

                CREATE TABLE MyTable
                (
                   MyIdentity BIGINT NOT NULL AUTO_INCREMENT,
                   MyString   VARCHAR(100) NULL,
                   MyNumber   BIGINT NOT NULL,
                   PRIMARY KEY(MyIdentity)
                );

                CREATE TABLE MyChildren
                (
                   HisIdentity BIGINT,
                   MyString    VARCHAR(100) NULL
                );

                CREATE TABLE MyFriend
                (
                   MyIdentity BIGINT NOT NULL AUTO_INCREMENT,
                   MyString   VARCHAR(100) NULL,
                   MyNumber   BIGINT NOT NULL,
                   PRIMARY KEY (MyIdentity)
                );

                INSERT INTO MyTable VALUES (null, 'One', 1);
                INSERT INTO MyTable VALUES (null, 'Two', 2);
                INSERT INTO MyTable VALUES (null, 'Three', 3);

                INSERT INTO MyChildren VALUES (1, 'Child 1/3');
                INSERT INTO MyChildren VALUES (1, 'Child 2/3');
                INSERT INTO MyChildren VALUES (1, 'Child 3/3');
                INSERT INTO MyChildren VALUES (3, 'Child 1/1');

                CREATE TABLE NullableTest (
                  Value1 INT NULL
                );

               INSERT INTO NullableTest VALUES (null);
               INSERT INTO NullableTest VALUES (1);
               INSERT INTO NullableTest VALUES (null);
               INSERT INTO NullableTest VALUES (2);

               CREATE TABLE MyNopkTable
               (
                   MyString   VARCHAR(100) NULL,
                   MyNumber   BIGINT NOT NULL
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
         return new MySqlParameter(name, value);
      }

      protected override void DestroyAccessor(string key)
      {
         var accessor = DbFactory.CreateAccessor(
             DbAccessorType.MySql, () => new MySqlConnection("Server=localhost;User Id=root;Password=")
             );

         accessor.Execute("DROP DATABASE DrivenDbTest");
      }
   }
}