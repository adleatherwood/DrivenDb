using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DrivenDb.Tests.Language
{
    [TestClass]
   public class DbEntityTests
   {
      [TestMethod]
      public void ColumnsWithSpacesAtTheEndOfTheirNamesStillGetACompiledSetter()
      {
         var sut = new MySpacedIndentityClass();

         sut.Entity.SetIdentity(1);
         Assert.IsTrue(true);
      }

      [TestMethod]
      public void BeforeAfterSerialized_WorksProperly()
      {
         var before = new MySerializationClass()
            {
               MyNumber = 1
            };

         using (var memory = new MemoryStream())
         {
            SerializeDataContractXml(before, memory);

            memory.Position = 0;

            var after = DeserializeDataContractXml<MySerializationClass>(memory);

            Assert.AreEqual(3, after.MyNumber);
         }
      }

      [TestMethod]
      public void ToMapped_CopiesPropertiesByName()
      {
         var source = new MyTable() { MyString = "3" };
         var target = new MyTable2();

         source.MapTo(target);

         Assert.AreEqual("3", target.MyString);
      }

      [TestMethod]
      public void ToUpdate_CopiesPropertiesAndSetsStateToModified()
      {
         var source = new MyTable() { MyNumber = 1, MyIdentity = 2, MyString = "3" };

         source.Entity.Reset();

         var target = source.ToUpdate();

         Assert.AreEqual(1, target.MyNumber);
         Assert.AreEqual(2, target.MyIdentity);
         Assert.AreEqual("3", target.MyString);
         Assert.AreEqual(EntityState.Modified, target.Entity.State);
      }

      [TestMethod]
      public void ToNew_CopiesPropertiesAndSetsStateToNew()
      {
         var source = new MyTable() { MyNumber = 1, MyIdentity = 2, MyString = "3" };

         source.Entity.Reset();

         var target = source.ToNew();

         Assert.AreEqual(1, target.MyNumber);
         Assert.AreEqual(0, target.MyIdentity);
         Assert.AreEqual("3", target.MyString);
         Assert.AreEqual(EntityState.New, target.Entity.State);
      }

      // OBSOLETE
      //[TestMethod]
      //public void State_CanBeModifiedManually()
      //{
      //   var table = new MyTable();

      //   table.Entity.State = EntityState.Modified;

      //   Assert.AreEqual(EntityState.Modified, table.Entity.State);
      //}

      [TestMethod]
      public void EqualityComparisonProplemSolved()
      {
         var list1 = new List<MyTable>()
            {
               new MyTable() {MyNumber = 1},
               new MyTable() {MyNumber = 2},
               new MyTable() {MyNumber = 2},
               new MyTable() {MyNumber = 1},
            };

         var list2 = list1.Where(o => o.MyNumber == 2)
            .ToList();

         foreach (var item in list2)
         {
            var contains = list1.Contains(item);

            Assert.IsTrue(contains);

            var removed = list1.Remove(item);

            Assert.IsTrue(removed);
         }
      }

      [TestMethod]
      public void Update_CopiesAllPropertiesProperly()
      {
         var instance1 = new MyTable();

         instance1.MyIdentity = 1;
         instance1.MyNumber = 2;
         instance1.MyString = "3";

         var instance2 = new MyTable();

         instance2.MyIdentity = 1;

         Thread.Sleep(100);

         instance2.Update(instance1, true);

         Assert.AreEqual(instance1.MyIdentity, instance2.MyIdentity);
         Assert.AreEqual(instance1.MyNumber, instance2.MyNumber);
         Assert.AreEqual(instance1.MyString, instance2.MyString);
         Assert.IsTrue(instance2.Entity.LastModified > instance1.Entity.LastModified);
      }

      [TestMethod]
      public void Update_ThrowsOnIdentityCheck()
      {
         var instance1 = new MyTable();

         instance1.MyIdentity = 1;

         var instance2 = new MyTable();

         instance2.MyIdentity = 2;

         AssertX.Throws<InvalidDataException>(
            () => instance2.Update(instance1, true)
            );
      }

      [TestMethod]
      public void Clone_CopiesAllPropertiesProperly()
      {
         var instance1 = new MyTable();

         instance1.MyIdentity = 1;
         instance1.MyNumber = 2;
         instance1.MyString = "3";

         var instance2 = instance1.Clone();

         Assert.AreEqual(instance1.MyIdentity, instance2.MyIdentity);
         Assert.AreEqual(instance1.MyNumber, instance2.MyNumber);
         Assert.AreEqual(instance1.MyString, instance2.MyString);
      }

      [TestMethod]
      public void Clone_CopiesStateProperly()
      {
         var instance1 = new MyTable();

         instance1.Entity.Reset();

         Assert.AreEqual(EntityState.Current, instance1.Entity.State);

         instance1.MyNumber = 2;

         Assert.AreEqual(EntityState.Modified, instance1.Entity.State);

         var instance2 = instance1.Clone();

         Assert.AreEqual(EntityState.Modified, instance2.Entity.State);
      }

      [TestMethod]
      public void Clone_CopiesChangesProperly()
      {
         var instance1 = new MyTable();

         instance1.MyNumber = 2;

         var instance2 = instance1.Clone();

         Assert.AreEqual(1, instance2.Entity.Changes.Count());
         Assert.IsTrue(instance2.Entity.Changes.Contains("MyNumber"));
      }

      [TestMethod]
      public void Clone_CopiesTimestampsProperly()
      {
         var instance1 = new MyTable();

         instance1.Entity.Reset();
         instance1.MyNumber = 2;

         var instance2 = instance1.Clone();

         Assert.AreEqual(instance1.Entity.LastModified, instance2.Entity.LastModified);
         Assert.AreEqual(instance1.Entity.LastUpdated, instance2.Entity.LastUpdated);
      }

      [TestMethod]
      public void InitialStateTest()
      {
         var instance = new MyTable();

         Assert.AreEqual(instance.Entity.State, EntityState.New);
         Assert.AreEqual(instance.Entity.Columns.Count, 3);
         Assert.AreEqual(instance.Entity.PrimaryColumns.Length, 1);
         Assert.AreEqual(instance.Entity.PrimaryColumns[0].Name, "MyIdentity");
         Assert.AreEqual(instance.Entity.IdentityColumn.Name, "MyIdentity");
         Assert.AreEqual(instance.Entity.IdentityColumn.Name, "MyIdentity");
         Assert.AreEqual(instance.Entity.Changes.Count(), 0);
      }

      [TestMethod]
      public void ChangeTrackingTest()
      {
         var instance = new MyTable();

         instance.MyString = "test";

         Assert.AreEqual(instance.Entity.State, EntityState.New);
         Assert.AreEqual(instance.Entity.Changes.Count(), 1);
         Assert.AreEqual(instance.Entity.Changes.First(), "MyString");

         instance.Entity.SetIdentity(1);
         instance.Entity.Reset();

         Assert.AreEqual(instance.Entity.State, EntityState.Current);
         Assert.AreEqual(instance.Entity.Changes.Count(), 0);

         instance.MyString = "test";

         Assert.AreEqual(instance.Entity.Changes.Count(), 1);
         Assert.AreEqual(instance.Entity.Changes.First(), "MyString");
      }

      [TestMethod]
      public void IdentityComparisonTest()
      {
         var instance1 = new MyTable();
         var instance2 = new MyTable();

         instance1.Entity.SetIdentity(1);
         instance2.Entity.SetIdentity(1);
         instance1.Entity.Reset();
         instance2.Entity.Reset();

         Assert.AreEqual(instance1.Entity.State, EntityState.Current);
         Assert.AreEqual(instance1.Entity.State, EntityState.Current);
         Assert.AreEqual(instance1.Entity.Changes.Count(), 0);
         Assert.IsTrue(instance1.Entity.SameAs(instance2));
         Assert.IsTrue(instance1.Entity.SameAs(instance2));

         instance2.Entity.SetIdentity(2);

         Assert.AreEqual(instance1.Entity.State, EntityState.Current);
         Assert.AreEqual(instance1.Entity.Changes.Count(), 0);
         Assert.IsFalse(instance1.Entity.SameAs(instance2));
         Assert.IsFalse(instance1.Entity.SameAs(instance2));
      }

      [TestMethod]
      public void DeleteTest()
      {
         var instance = new MyTable();

         instance.Entity.Delete();

         Assert.AreEqual(instance.Entity.State, EntityState.Deleted);
      }

      [TestMethod]
      public void PrimaryKeyTest()
      {
         var instance1 = new MyTable();
         var instance2 = new MyTable();

         instance1.Entity.SetIdentity(1);
         instance2.Entity.SetIdentity(2);
         instance1.Entity.Reset();
         instance2.Entity.Reset();

         Assert.AreEqual(instance1.Entity.PrimaryKey.Length, 1);
         Assert.AreEqual((int) instance1.Entity.PrimaryKey[0], 1);
         Assert.AreEqual(instance2.Entity.PrimaryKey.Length, 1);
         Assert.AreEqual((int) instance2.Entity.PrimaryKey[0], 2);
         Assert.AreNotEqual(instance1, instance2);
      }

      [TestMethod]
      public void PrimaryKeyTest2()
      {
         var instance1 = new MyTable2();
         var instance2 = new MyTable2();

         instance1.MyKey1 = 1;
         instance1.MyKey2 = 2;

         instance2.MyKey1 = 3;
         instance2.MyKey2 = 4;

         Assert.AreEqual(instance1.Entity.PrimaryKey.Length, 2);
         Assert.AreEqual((int) instance1.Entity.PrimaryKey[0], 1);
         Assert.AreEqual((int) instance1.Entity.PrimaryKey[1], 2);

         Assert.AreEqual(instance2.Entity.PrimaryKey.Length, 2);
         Assert.AreEqual((int) instance2.Entity.PrimaryKey[0], 3);
         Assert.AreEqual((int) instance2.Entity.PrimaryKey[1], 4);

         Assert.AreNotEqual(instance1, instance2);
      }

      [TestMethod]
      public void MergeTest()
      {
         var instance1 = new MyTable();
         var instance2 = new MyTable();

         instance1.MyString = "test";
         instance2.Entity.Merge(instance1);

         Assert.IsTrue(instance1.Entity.LastModified.HasValue);
         Assert.IsTrue(instance2.Entity.LastModified.HasValue);
         Assert.AreEqual(instance1.Entity.LastModified.Value, instance2.Entity.LastModified.Value);

         instance1.Entity.SetIdentity(1);
         instance2.Entity.SetIdentity(1);
         instance1.Entity.Reset();
         instance2.Entity.Reset();
         instance2.Entity.Merge(instance1);

         Assert.AreEqual(instance2.MyIdentity, 1);
         Assert.AreEqual(instance2.MyString, "test");
         Assert.IsTrue(instance1.Entity.SameAs(instance2));
         Assert.IsFalse(instance1.Entity.LastModified.HasValue);
         Assert.IsFalse(instance2.Entity.LastModified.HasValue);
      }

      [TestMethod]
      public void MergeTest2()
      {
         var instance1 = new MyTable();
         var instance2 = new MyTable();

         instance1.Entity.SetIdentity(1);
         instance2.Entity.SetIdentity(1);
         instance1.Entity.Reset();
         instance2.Entity.Reset();

         instance1.MyString = "test";
         instance2.MyNumber = 5;

         Assert.IsTrue(instance1.Entity.LastModified.HasValue);
         Assert.AreEqual(instance1.Entity.Changes.Count(), 1);
         Assert.IsTrue(instance2.Entity.LastModified.HasValue);
         Assert.AreEqual(instance2.Entity.Changes.Count(), 1);

         instance2.Entity.Merge(instance1);

         Assert.AreEqual(instance1.Entity.Changes.Count(), 1);
         Assert.AreEqual(instance2.Entity.Changes.Count(), 2);
         Assert.AreEqual(instance2.Entity.State, instance1.Entity.State);

         instance1.Entity.Reset();
         instance2.Entity.Reset();
         instance2.MyNumber = 5;
         instance2.Entity.Merge(instance1);

         Assert.AreEqual(instance2.Entity.Changes.Count(), 1);
         Assert.AreEqual(instance2.Entity.State, EntityState.Modified);
         Assert.AreNotEqual(instance2.Entity.State, instance1.Entity.State);

         instance1.Entity.Delete();
         instance2.Entity.Merge(instance1);

         Assert.AreEqual(instance2.Entity.State, EntityState.Deleted);
      }

      [TestMethod]
      public void MergeWithoutPrimaryKeysSucceeds()
      {
         var instance1 = new MyNopkTable();
         var instance2 = new MyNopkTable();

         instance1.MyNumber = 999;
         instance2.MyString = "test";

         instance2.Entity.Merge(instance1);

         Assert.AreEqual(instance2.MyNumber, 999);
         Assert.AreEqual(instance2.MyString, "test");
      }

      [TestMethod]
      public void MergeWithIdentityCheckAndDifferentIdentitiesFails()
      {
         var instance1 = new MyTable();
         var instance2 = new MyTable();

         instance1.Entity.SetIdentity(1);
         instance2.Entity.SetIdentity(2);

         instance1.Entity.Reset();
         instance2.Entity.Reset();

         instance1.MyNumber = 999;
         instance2.MyString = "test";

         AssertX.Throws<InvalidDataException>(() =>
            {
               instance2.Entity.Merge(instance1);
            });
      }

      [TestMethod]
      public void MergeWithoutIdentityCheckAndDifferentIdentitiesSucceeds()
      {
         var instance1 = new MyTable();
         var instance2 = new MyTable();

         instance1.Entity.SetIdentity(1);
         instance2.Entity.SetIdentity(2);

         instance1.Entity.Reset();
         instance2.Entity.Reset();

         instance1.MyNumber = 999;
         instance2.MyString = "test";

         instance2.Entity.Merge(instance1, false);

         Assert.AreEqual(instance2.MyNumber, 999);
         Assert.AreEqual(instance2.MyString, "test");
      }

      private static void SerializeDataContractXml<T>(T instance, Stream stream)
      {
         using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, new UTF8Encoding(), false))
         {
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(writer, instance);
         }
      }

      private static T DeserializeDataContractXml<T>(Stream stream)
      {
         T instance;

         using (var reader = XmlDictionaryReader.CreateDictionaryReader(XmlReader.Create(stream)))
         {
            var deserializer = new DataContractSerializer(typeof(T));
            instance = (T) deserializer.ReadObject(reader);
         }

         return instance;
      }

      #region --- NESTED --------------------------------------------------------------------------

      [DbTable(Name = "MyTable")]
      private class MyTable : DbEntity<MyTable>, INotifyPropertyChanged
      {
         private int m_MyIdentity;
         private string m_MyString;
         private int m_MyNumber;

         [DbColumn(IsDbGenerated = true, IsPrimaryKey = true, Name = "MyIdentity")]
         public int MyIdentity
         {
            get { return m_MyIdentity; }
            set
            {
               m_MyIdentity = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyIdentity"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "MyString")]
         public string MyString
         {
            get { return m_MyString; }
            set
            {
               m_MyString = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyString"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "MyNumber")]
         public int MyNumber
         {
            get { return m_MyNumber; }
            set
            {
               m_MyNumber = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyNumber"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged;
      }

      [DbTable(Name = "MyTable2")]
      private class MyTable2 : DbEntity<MyTable2>, INotifyPropertyChanged
      {
         private int m_MyKey1;
         private int m_MyKey2;
         private string m_MyString;

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = true, Name = "MyKey1")]
         public int MyKey1
         {
            get { return m_MyKey1; }
            set
            {
               m_MyKey1 = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyKey1"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = true, Name = "MyKey2")]
         public int MyKey2
         {
            get { return m_MyKey2; }
            set
            {
               m_MyKey2 = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyKey2"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "MyString")]
         public string MyString
         {
            get { return m_MyString; }
            set
            {
               m_MyString = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyString"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged;      }

      [DbTable(Name = "MyNopkTable")]
      private class MyNopkTable : DbEntity<MyNopkTable>, INotifyPropertyChanged
      {
         private string m_MyString;
         private int m_MyNumber;

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "MyString")]
         public string MyString
         {
            get { return m_MyString; }
            set
            {
               m_MyString = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyString"));
            }
         }

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "MyNumber")]
         public int MyNumber
         {
            get { return m_MyNumber; }
            set
            {
               m_MyNumber = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyNumber"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged;
      }

      [DbTable(Name = "MyTable")]
      [DataContract]
      private class MySerializationClass : DbEntity<MySerializationClass>, INotifyPropertyChanged
      {
         [DataMember]
         private int m_MyNumber;

         [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "MyNumber")]
         public int MyNumber
         {
            get { return m_MyNumber; }
            set
            {
               m_MyNumber = value;
               PropertyChanged(this, new PropertyChangedEventArgs("MyNumber"));
            }
         }

         public event PropertyChangedEventHandler PropertyChanged;

         protected override void BeforeSerialization()
         {
            if (m_MyNumber == 1)
            {
               m_MyNumber = 2;
            }
         }

         protected override void AfterDeserialization()
         {
            if (m_MyNumber == 2)
            {
               m_MyNumber = 3;
            }
         }
      }

      [DbTable(Name = "MyTable")]
      [DataContract]
      private class MySpacedIndentityClass : DbEntity<MySpacedIndentityClass>, INotifyPropertyChanged
      {
         [DataMember]
         private int m_MyNumber;

         [DbColumn(IsDbGenerated = true, IsPrimaryKey = true, Name = "MyIdentity ")] // note: space on the end is on purpose
         public int MyIndentity
         {
            get { return m_MyNumber; }
            set { m_MyNumber = value; }
         }

         private void IngoreThis()
         {
            PropertyChanged(null, null);
         }



         public event PropertyChangedEventHandler PropertyChanged;
      }

      #endregion --- NESTED --------------------------------------------------------------------------
   }
}