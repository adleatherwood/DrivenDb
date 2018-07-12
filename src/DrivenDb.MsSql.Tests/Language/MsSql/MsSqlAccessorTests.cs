using System.Data;
using DrivenDb.MsSql;
using DrivenDb.MsSql.Tests.Language.MsSql;
using DrivenDb.Tests.Language.Interfaces;
using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DrivenDb
{
    [TestClass]
   public class MsSqlAccessorTests : IDbAccessorTests
   {
      [TestMethod]
      public void NullablePrimaryKeysCanUpdate()
      {
         var filename = "";
         var accessor = CreateAccessor(out filename);

         var one = new NullableKeyTest()
            {
               A = 1,
               B = null,
               Value = "one"
            };

         accessor.WriteEntity(one);

         one.Value = "two";

         accessor.WriteEntity(one);

         var two = accessor.ReadEntities<NullableKeyTest>("SELECT * FROM [NullableKeyTest]")
            .Single();

         Assert.AreEqual(1, two.A);
         Assert.IsNull(two.B);
         Assert.AreEqual("two", two.Value);

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void SchemaOverrideCanBeSwitchedAtRuntime()
      {
         var filename = "";
         var accessor = (IMsSqlAccessor) CreateAccessor(out filename);

         var schema1s = accessor.ReadEntities<SchemaTable>(
            @"SELECT * FROM [one].[SchemaTable1]"
            ).ToArray();

         var schema2s = schema1s.Select(s => s.ToNew())
            .ToArray();

         schema2s.ForEach(s => s.Record.TableOverride = new DbTableAttribute() {HasTriggers = false, Schema = "two", Name = "SchemaTable2"});         
         accessor.WriteEntities(schema2s);

         var actual = accessor.ReadEntities<SchemaTable>(
            @"SELECT * FROM [two].[SchemaTable2]"
            ).ToArray();
         
         Assert.AreEqual("one", actual[0].Text);
         Assert.AreEqual("two", actual[1].Text);
         Assert.AreEqual("three", actual[2].Text);

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void WriteTransactionWithScopeIdentityTest()
      {
         var filename = "";
         var accessor = (IMsSqlAccessor) CreateAccessor(out filename);

         var entities = new MyTable[]
            {
               new MyTable() {MyNumber = 2, MyString = "2"},
               new MyTable() {MyNumber = 3, MyString = "3"},
               new MyTable() {MyNumber = 4, MyString = "4"},
            };

         using (var scope = accessor.CreateScope())
         {
            scope.WriteEntitiesUsingScopeIdentity(entities);
            scope.Commit();
         }

         Assert.AreEqual(4, entities[0].MyIdentity);
         Assert.AreEqual(5, entities[1].MyIdentity);
         Assert.AreEqual(6, entities[2].MyIdentity);

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void ReadEntitysWithTimespanColumnSucceeds()
      {
         var filename = "";
         var accessor = (IMsSqlAccessor) CreateAccessor(out filename);

         var actual = accessor.ReadEntities<TimeTable>(
            @"SELECT TOP 1 * FROM [TimeTable]"
            ).Single();

         var expected = new DateTime(1972, 8, 2, 6, 5, 33);

         Assert.AreEqual(expected.Date, actual.PartyDate.Date);
         Assert.AreEqual(expected.Date, actual.PartyDateTime.Date);
         Assert.AreEqual(expected.Date, actual.PartyDateTime2.Date);

         Assert.AreEqual(expected.TimeOfDay, actual.PartyTime);
         Assert.AreEqual(null, actual.PartyTime2);
         Assert.AreEqual(expected.TimeOfDay, actual.PartyDateTime.TimeOfDay);
         Assert.AreEqual(expected.TimeOfDay, actual.PartyDateTime2.TimeOfDay);

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void WriteEntitiesWithScopeIdentityTest()
      {
         var filename = "";
         var accessor = (IMsSqlAccessor) CreateAccessor(out filename);

         var entities = new MyTable[]
            {
               new MyTable() {MyNumber = 2, MyString = "2"},
               new MyTable() {MyNumber = 3, MyString = "3"},
               new MyTable() {MyNumber = 4, MyString = "4"},
            };

         accessor.WriteEntitiesUsingScopeIdentity(entities);

         Assert.AreEqual(4, entities[0].MyIdentity);
         Assert.AreEqual(5, entities[1].MyIdentity);
         Assert.AreEqual(6, entities[2].MyIdentity);

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void VarbinaryTest()
      {
         var filename = "";
         var accessor = CreateAccessor(out filename);

         var gnu = new VarbinaryTest()
            {
               Value1 = GetBytes("This is a test")
            };

         accessor.WriteEntity(gnu);

         var existing = accessor.ReadEntities<VarbinaryTest>("SELECT * FROM [VarbinaryTest]");

         Assert.AreEqual(1, existing.Count());

         var first = existing.First();

         Assert.IsNull(first.Value2);
         Assert.IsNull(first.Value3);

         var value = GetString(first.Value1);

         Assert.AreEqual("This is a test", value);

         DestroyAccessor(filename);
      }
      
      [TestMethod]
      public void ImageTest()
      {
         var filename = "";
         var accessor = CreateAccessor(out filename);

         var gnu = new ImageTable()
            {
               Test = GetBytes("This is a test")
            };

         accessor.WriteEntity(gnu);

         var existing = accessor.ReadEntities<ImageTable>("SELECT * FROM [ImageTable]");

         Assert.AreEqual(1, existing.Count());

         var first = existing.First();
         var value = GetString(first.Test);

         Assert.AreEqual("This is a test", value);

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void TextTest()
      {
         var filename = "";
         var accessor = CreateAccessor(out filename);

         var gnu = new TextTable()
            {
               Test = "This is a test"
            };

         accessor.WriteEntity(gnu);

         var existing = accessor.ReadEntities<TextTable>("SELECT * FROM [TextTable]");

         Assert.AreEqual(1, existing.Count());

         var first = existing.First();
         var value = first.Test;

         Assert.AreEqual("This is a test", value);

         DestroyAccessor(filename);
      }

      [TestMethod]
      public void WriteEntitiesWithOutputTest()
      {
         var filename = "";
         var accessor = (IMsSqlAccessor) CreateAccessor(out filename);

         var entities = accessor.ReadEntities<MyTable>("SELECT * FROM [MyTable]")
            .ToList();

         entities[0].MyNumber = 100;
         entities[0].MyString = "100";
         entities[1].MyNumber = 200;
         entities[1].MyString = "200";
         entities[2].Entity.Delete();

         var gnu = new MyTable()
            {
               MyNumber = 400,
               MyString = "400"
            };

         entities.Add(gnu);

         var changes = accessor.WriteEntitiesAndOutputDeleted(entities, new { MyNumber = 0L, MyString = "" }).ToArray();

         Assert.AreEqual(changes[0].Item2.MyNumber, 1);
         Assert.AreEqual(changes[0].Item2.MyString, "One");
         Assert.AreEqual(changes[1].Item2.MyNumber, 2);
         Assert.AreEqual(changes[1].Item2.MyString, "Two");
         Assert.AreEqual(changes[2].Item2.MyNumber, 3);
         Assert.AreEqual(changes[2].Item2.MyString, "Three");

         DestroyAccessor(filename);
      }

      private static byte[] GetBytes(string str)
      {
         byte[] bytes = new byte[str.Length * sizeof(char)];
         System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
         return bytes;
      }

      private static string GetString(byte[] bytes)
      {
         char[] chars = new char[bytes.Length / sizeof(char)];
         System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
         return new string(chars);
      }

      protected const string MASTER_CSTRING = @"Integrated Security=SSPI;Initial Catalog=master;Data Source=localhost";
      protected const string TEST_CSTRING = @"Integrated Security=SSPI;Initial Catalog=DrivenDbTest;Data Source=localhost";

      protected override IDbAccessor CreateAccessor(out string key)
      {
         return CreateAccessor(out key, AccessorExtension.All);
      }

      protected virtual IDbAccessor CreateAccessor(AccessorExtension extensions)
      {
         return DbFactory.CreateAccessor(
            DbAccessorType.MsSql, extensions,
            () => new SqlConnection(TEST_CSTRING)
            );
      }

      protected override IDbAccessor CreateAccessor(out string key, AccessorExtension extensions)
      {
         SqlConnection.ClearAllPools();

         key = null;

         var master = DbFactory.CreateAccessor(
              DbAccessorType.MsSql, extensions,
              () => new SqlConnection(MASTER_CSTRING)
              );

         master.Execute(@"
               IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'DrivenDbTest')
               BEGIN
                  DROP DATABASE DrivenDbTest
               END

               CREATE DATABASE [DrivenDbTest]"
            );

         var accessor = CreateAccessor(extensions);

         accessor.Execute(@"CREATE SCHEMA [one]");
         accessor.Execute(@"CREATE SCHEMA [two]");

         accessor.Execute(@"                                
                CREATE TABLE [MyTable]
                (
                   [MyIdentity] BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
                   [MyString] TEXT NULL,
                   [MyNumber] BIGINT NOT NULL
                );

                CREATE TABLE MyChildren
                (
                   [HisIdentity] BIGINT,
                   [MyString] TEXT NULL
                );

                CREATE TABLE MyFriend
                (
                   [MyIdentity] BIGINT IDENTITY(1,1)PRIMARY KEY CLUSTERED,
                   [MyString] TEXT NULL,
                   [MyNumber] BIGINT NOT NULL
                );

                INSERT INTO MyTable VALUES ('One', 1);
                INSERT INTO MyTable VALUES ('Two', 2);
                INSERT INTO MyTable VALUES ('Three', 3);

                INSERT INTO MyChildren VALUES (1, 'Child 1/3');
                INSERT INTO MyChildren VALUES (1, 'Child 2/3');
                INSERT INTO MyChildren VALUES (1, 'Child 3/3');
                INSERT INTO MyChildren VALUES (3, 'Child 1/1');

                CREATE TABLE MyBigTable
                (
                   Id BIGINT IDENTITY(1,1) PRIMARY KEY CLUSTERED,
                   Property1    VARCHAR(50) NULL,
                   Property2    VARCHAR(50) NULL,
                   Property3    VARCHAR(50) NULL,
                   Property4    VARCHAR(50) NULL,
                   Property5    VARCHAR(50) NULL,
                   Property6    VARCHAR(50) NULL,
                   Property7    VARCHAR(50) NULL,
                   Property8    VARCHAR(50) NULL,
                   Property9    VARCHAR(50) NULL,
                   Property10   VARCHAR(50) NULL,
                   Property11   VARCHAR(50) NULL,
                   Property12   VARCHAR(50) NULL
                );

               CREATE TABLE [VarbinaryTest](
                  [Id] [int] IDENTITY(1,1) NOT NULL,
                  [Value1] [varbinary](50) NOT NULL,
                  [Value2] [varbinary](50) NULL,
                  [Value3] [varchar](50) NULL,
                CONSTRAINT [PK_VarbinaryTest] PRIMARY KEY CLUSTERED
               (
                  [Id] ASC
               )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
               ) ON [PRIMARY]
               CREATE TABLE [NullableTest] (
                  [Value1] INT NULL
               );

               INSERT INTO [NullableTest] VALUES (null);
               INSERT INTO [NullableTest] VALUES (1);
               INSERT INTO [NullableTest] VALUES (null);
               INSERT INTO [NullableTest] VALUES (2);

               CREATE TABLE [MyNopkTable]
               (
                   [MyString] TEXT NULL,
                   [MyNumber] BIGINT NOT NULL
               );

               INSERT INTO MyNopkTable VALUES ('One', 1);
               INSERT INTO MyNopkTable VALUES ('Two', 2);
               INSERT INTO MyNopkTable VALUES ('Three', 3);

               CREATE TABLE [TextTable]
               (
                  [Id] INT IDENTITY(1,1) NOT NULL,
                  [Test] TEXT
               )

               CREATE TABLE [ImageTable]
               (
                  [Id] INT IDENTITY(1,1) NOT NULL,
                  [Test] IMAGE
               )

               CREATE TABLE [TimeTable](
                  [PartyDate] [date] NOT NULL,
                  [PartyTime] [time](7) NOT NULL,
                  [PartyTime2] [time](7) NULL,
                  [PartyDateTime] [datetime] NOT NULL,
                  [PartyDateTime2] [datetime2](7) NOT NULL
               )

               INSERT INTO [dbo].[TimeTable] VALUES ('1972-08-02', '06:05:33', NULL, '1972-08-02 06:05:33', '1972-08-02 06:05:33')

               CREATE TABLE [one].[SchemaTable1]
               (
                  [Id] INT IDENTITY(1,1) NOT NULL,
                  [Text] VARCHAR(50)
               )

               INSERT INTO [one].[SchemaTable1] ([Text]) VALUES ('one')
               INSERT INTO [one].[SchemaTable1] ([Text]) VALUES ('two')
               INSERT INTO [one].[SchemaTable1] ([Text]) VALUES ('three')

               CREATE TABLE [two].[SchemaTable2]
               (
                  [Id] INT IDENTITY(1,1) NOT NULL,
                  [Text] VARCHAR(50)
               )

               CREATE TABLE [dbo].[NullableKeyTest](
                  [A] [int] NOT NULL,
                  [B] [int] NULL,
                  [Value] [varchar](50) NULL
               ) ON [PRIMARY]
               ");

         return accessor;
      }

      protected override IDbDataParameter CreateParam<T>(string name, T value)
      {
         return new SqlParameter(name, value);
      }

      protected override void DestroyAccessor(string key)
      {
         SqlConnection.ClearAllPools();

         var accessor = DbFactory.CreateAccessor(
             DbAccessorType.MsSql, () => new SqlConnection(MASTER_CSTRING)
             );

         accessor.Execute("DROP DATABASE [DrivenDbTest]");
      }

      [DbTable(HasTriggers = false, Name = "SchemaTable1", Schema = "one")]
      private sealed class SchemaTable
         : DbEntity<SchemaTable>
         , INotifyPropertyChanged
      {
         private int _id;
         private string _text;

         [DbColumn(IsDbGenerated = true, IsPrimaryKey = true, Name = "Id")]
         public int Id
         {
            get { return _id; }
            set
            {
               _id = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Id"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "Text")]
         public string Text
         {
            get { return _text; }
            set
            {
               _text = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged = delegate {};
      }

      [DataContract]
      [DbTable(Schema = "dbo", Name = "NullableKeyTest")]
      public partial class NullableKeyTest : DbEntity<NullableKeyTest>, INotifyPropertyChanged
      {

         [DataMember]
         private int? _A;
         [DataMember]
         private int? _B;
         [DataMember]
         private string _Value;

         public NullableKeyTest()
         {
         }

         [DbColumn(Name = "A", IsPrimaryKey = true, IsDbGenerated = false)]
         public int? A
         {
            get { return _A; }
            set
            {
               BeforeAChanged(ref value);
               _A = value;
               AfterAChanged(value);
               PropertyChanged(this, new PropertyChangedEventArgs("A"));
            }
         }

         [DbColumn(Name = "B", IsPrimaryKey = true, IsDbGenerated = false)]
         public int? B
         {
            get { return _B; }
            set
            {
               BeforeBChanged(ref value);
               _B = value;
               AfterBChanged(value);
               PropertyChanged(this, new PropertyChangedEventArgs("B"));
            }
         }

         [DbColumn(Name = "Value", IsPrimaryKey = false, IsDbGenerated = false)]
         public string Value
         {
            get { return _Value; }
            set
            {
               
               _Value = value;               
               PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged;

         partial void BeforeAChanged(ref int? value);
         partial void BeforeBChanged(ref int? value);

         partial void AfterAChanged(int? value);
         partial void AfterBChanged(int? value);

         partial void OnSerialization();
         partial void OnDeserialization();

         protected override void BeforeSerialization()
         {
            OnSerialization();
         }

         protected override void AfterDeserialization()
         {
            OnDeserialization();
         }
      }
   }
}