using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace DrivenDb.Tests.Examples
{
    [TestClass]
   public class Examples : IDisposable
   {
      private readonly string m_Filename;
      private readonly IDbAccessor m_Accessor;

      public Examples()
      {
         m_Filename = Path.GetTempFileName();
         m_Accessor = DbFactory.CreateAccessor(
            DbAccessorType.SqLite,
            AccessorExtension.All,
            () => new SQLiteConnection(String.Format("Data Source={0};Version=3;New=True", m_Filename))
            );

         m_Accessor.Execute(CREATE_DATABASE_SCRIPT);
         //m_Accessor.Log = Console.Out;
      }

      [TestMethod]
      public void ReadValueFromDb()
      {
         var value = m_Accessor.ReadValue<long>("SELECT 7");

         //Console.WriteLine(String.Format("Value = '{0}'", value));
      }

        [TestMethod]
        public void ReadValuesFromDb()
      {
         var Ids = m_Accessor.ReadValues<long>("SELECT [Id] FROM [Thing1]");

         //foreach (var id in Ids)
         //{
         //   Console.WriteLine(String.Format("Id = '{0}'", id));
         //}
      }

        [TestMethod]
        public void ReadSingleClassFromDb()
      {
         var thing1 = m_Accessor.ReadEntity<Thing1>("SELECT * FROM [Thing1] WHERE Id = @0", 1);

         //Console.WriteLine(String.Format("Thing1: Id = '{0}', Name = '{1}', Value = '{2}'", thing1.Id, thing1.Name, thing1.Value));
      }

        [TestMethod]
        public void ReadSingleClassFromDb2()
      {
         var thing1s = m_Accessor.ReadEntities<Thing1>("SELECT * FROM [Thing1]");

         //foreach (var thing1 in thing1s)
         //{
         //   Console.WriteLine(String.Format("Thing1: Id = '{0}', Name = '{1}', Value = '{2}'", thing1.Id, thing1.Name, thing1.Value));
         //}
      }

        [TestMethod]
        public void ReadSingleClassFromDbByIdentity()
      {
         var thing1 = m_Accessor.ReadIdentity<Thing1, int>(1);

         //Console.WriteLine(String.Format("Thing1: Id = '{0}', Name = '{1}', Value = '{2}'", thing1.Id, thing1.Name, thing1.Value));
      }

        [TestMethod]
        public void ReadMultipleClassesFromDb()
      {
         var entities = m_Accessor.ReadEntities<Thing1, Thing2>(
            @"SELECT * FROM [Thing1] WHERE [Id] = @0;
              SELECT * FROM [Thing2] WHERE [Thing1Id] = @0;",
            1
            );

         var thing1 = entities.Set1.Single();

         //Console.WriteLine(String.Format("Thing1: Id = '{0}', Name = '{1}', Value = '{2}'", thing1.Id, thing1.Name, thing1.Value));

         //foreach (var thing2 in entities.Set2)
         //{
         //   Console.WriteLine(String.Format("Thing2: Id = '{0}', Thing1Id = '{1}', Name = '{2}', Value = '{3}'", thing2.Id, thing2.Thing1Id, thing2.Name, thing2.Value));
         //}
      }

      [TestMethod]
      public void ReadAnonymousClassFromDb()
      {
         var thing1s = m_Accessor.ReadAnonymous(new { Id = 0L, Name = "", Value = 0L },
            "SELECT * FROM [Thing1]"
            );

         //foreach (var thing1 in thing1s)
         //{
         //   Console.WriteLine(String.Format("Thing1: Id = '{0}', Name = '{1}', Value = '{2}'", thing1.Id, thing1.Name, thing1.Value));
         //}
      }

      [TestMethod]
      public void ReadRelatedClassFromDb()
      {
         var thing1 = m_Accessor.ReadIdentity<Thing1, int>(1);
         var thing2s = m_Accessor.ReadRelated<Thing1, Thing2>(thing1)
             .On(t1 => new { t1.Id }, t2 => new { t2.Thing1Id });

         //Console.WriteLine(String.Format("Thing1: Id = '{0}', Name = '{1}', Value = '{2}'", thing1.Id, thing1.Name, thing1.Value));

         //foreach (var thing2 in thing2s)
         //{
         //   Console.WriteLine(String.Format("Thing2: Id = '{0}', Thing1Id = '{1}', Name = '{2}', Value = '{3}'", thing2.Id, thing2.Thing1Id, thing2.Name, thing2.Value));
         //}
      }

      [TestMethod]
      public void EnumerationParameterInSql()
      {
         // using the 'AccessorExtension.AllowEnumerableParameters' option
         var thing1s = m_Accessor.ReadEntities<Thing1>("SELECT * FROM [Thing1] WHERE [Id] IN (@0)", new[] { 1L, 2L, 3L });

         //foreach (var thing1 in thing1s)
         //{
         //   Console.WriteLine(String.Format("Thing1: Id = '{0}', Name = '{1}', Value = '{2}'", thing1.Id, thing1.Name, thing1.Value));
         //}
      }

      [TestMethod]
      public void DateTimeParameterInSql()
      {
         // using the 'AccessorExtension.LimitDateParameters' option
         //var thing1s = m_Accessor.ReadEntities<Thing1>("SELECT * FROM [Thing1] WHERE [Created] > @0", DateTime.MinValue);

         //foreach (var thing1 in thing1s)
         //{
         //    Console.WriteLine(String.Format("Thing1: Id = '{0}', Name = '{1}', Value = '{2}'", thing1.Id, thing1.Name, thing1.Value));
         //}
      }

      [TestMethod]
      public void ExecuteSqlOnDb()
      {
         m_Accessor.Execute("DELETE FROM [Thing2] WHERE [Id] > @0", 50);
      }

      [TestMethod]
      public void WriteEntitiesToDb()
      {
         var thing2s = m_Accessor.ReadEntities<Thing2>("SELECT * FROM [Thing2]")
            .ToList();

         thing2s[0].Value = "one";
         thing2s[2].Entity.Delete();

         var gnu2 = new Thing2() { Thing1Id = 1, Name = "New Thing 2", Value = "Priceless" };

         thing2s.Add(gnu2);

         m_Accessor.WriteEntities(thing2s);

         //Console.WriteLine("Gnu2 Id = '{0}'", gnu2.Id);
      }

      [TestMethod]
      public void AffinityTest()
      {
         var numbers = m_Accessor.ReadEntities<NumbersTable>(
            @"SELECT * FROM [NumbersTable]"
            ).ToArray();

         Assert.AreEqual(3, numbers.Length);
         Assert.AreEqual(1, numbers[0].Number0);
         Assert.AreEqual(1, numbers[0].Number1);
         Assert.AreEqual(1, numbers[0].Number2);
         Assert.AreEqual(2, numbers[1].Number0);
         Assert.AreEqual(2, numbers[1].Number1);
         Assert.AreEqual(2, numbers[1].Number2);
         Assert.AreEqual(3, numbers[2].Number0);
         Assert.AreEqual(3, numbers[2].Number1);
         Assert.AreEqual(3, numbers[2].Number2);
      }

      private const string CREATE_DATABASE_SCRIPT = @"
         CREATE TABLE [Thing1]
         (
            [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
            [Name] TEXT NOT NULL,
            [Value] BIGINT NOT NULL
         );

         CREATE TABLE [Thing2]
         (
            [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
            [Thing1Id] NUMBER NOT NULL,
            [Name] TEXT NOT NULL,
            [Value] TEXT NOT NULL
         );

         INSERT INTO [Thing1] VALUES (NULL, 'Thing1 #1', 1);
         INSERT INTO [Thing1] VALUES (NULL, 'Thing1 #2', 2);
         INSERT INTO [Thing1] VALUES (NULL, 'Thing1 #3', 3);

         INSERT INTO [Thing2] VALUES (NULL, 0, 'Thing2 #1', 'One');
         INSERT INTO [Thing2] VALUES (NULL, 1, 'Thing2 #2', 'Two');
         INSERT INTO [Thing2] VALUES (NULL, 2, 'Thing2 #3', 'Three');
         INSERT INTO [Thing2] VALUES (NULL, 2, 'Thing2 #4', 'Four');
         INSERT INTO [Thing2] VALUES (NULL, 3, 'Thing2 #5', 'Five');
         INSERT INTO [Thing2] VALUES (NULL, 3, 'Thing2 #6', 'Six');

         CREATE TABLE [NumbersTable]
         (
            [Number0] SMALLINT,
            [Number1] INT,
            [Number2] BIGINT
         );

         INSERT INTO [NumbersTable] VALUES (1,1,1);
         INSERT INTO [NumbersTable] VALUES (2,2,2);
         INSERT INTO [NumbersTable] VALUES (3,3,3);
         ";

      [DbTable(Name = "NumbersTable")]
      private class NumbersTable : DbEntity<NumbersTable>, INotifyPropertyChanged
      {
         private short m_Number0;
         private int m_Number1;
         private long m_Number2;

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = true, Name = "Number0")]
         public short Number0
         {
            get { return m_Number0; }
            set
            {
               m_Number0 = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Number0"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = true, Name = "Number1")]
         public int Number1
         {
            get { return m_Number1; }
            set
            {
               m_Number1 = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Number1"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = true, Name = "Number2")]
         public long Number2
         {
            get { return m_Number2; }
            set
            {
               m_Number2 = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Number2"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged;
      }

      [DbTable(Name = "Thing1")]
      private class Thing1 : DbEntity<Thing1>, INotifyPropertyChanged
      {
         private long m_Id;
         private string m_Name;
         private long m_Value;

         [DbColumn(IsDbGenerated = true, IsPrimaryKey = true, Name = "Id")]
         public long Id
         {
            get { return m_Id; }
            set
            {
               m_Id = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Id"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "Name")]
         public string Name
         {
            get { return m_Name; }
            set
            {
               m_Name = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "Value")]
         public long Value
         {
            get { return m_Value; }
            set
            {
               m_Value = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged;
      }

      [DbTable(Name = "Thing2")]
      private class Thing2 : DbEntity<Thing2>, INotifyPropertyChanged
      {
         private long m_Id;
         private long m_Thing1Id;
         private string m_Name;
         private string m_Value;

         [DbColumn(IsDbGenerated = true, IsPrimaryKey = true, Name = "Id")]
         public long Id
         {
            get { return m_Id; }
            set
            {
               m_Id = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Id"));
            }
         }

         [DbColumn(IsDbGenerated = true, IsPrimaryKey = true, Name = "Thing1Id")]
         public long Thing1Id
         {
            get { return m_Thing1Id; }
            set
            {
               m_Thing1Id = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Thing1Id"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "Name")]
         public string Name
         {
            get { return m_Name; }
            set
            {
               m_Name = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "Value")]
         public string Value
         {
            get { return m_Value; }
            set
            {
               m_Value = value;
               PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged;
      }

      public void Dispose()
      {
         File.Delete(m_Filename);
      }
   }
}