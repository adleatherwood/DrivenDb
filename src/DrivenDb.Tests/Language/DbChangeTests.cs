using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DrivenDb.Tests.Language
{
    [TestClass]
    public class DbChangeTests
   {      
      [TestMethod]
      public void SerializationTest()
      {
         var change = new DbChange(
            //new object[] {1, 2L, "one", DateTime.Now.Date},
            DbChangeType.Updated,
            "TestTable",
            new string[] {"ColumnA", "ColumnB"},
            null
            );

         using (var memory = new MemoryStream())
         {
            SerializeDataContractXml(change, memory);

            memory.Position = 0;

            var result = DeserializeDataContractXml<DbChange>(memory);
            var columns = new HashSet<string>(new string[] {"ColumnA", "ColumnB"});

            var affected = result.AffectedColumns.ToList().TrueForAll(columns.Contains);

            Assert.AreEqual(result.ChangeType, DbChangeType.Updated);
            Assert.AreEqual(result.AffectedTable, "TestTable");
            Assert.IsTrue(affected);
            //Assert.AreEqual(result.Identity.Length, 4);
            //Assert.AreEqual(result.Identity[0], 1);
            //Assert.AreEqual(result.Identity[1], 2L);
            //Assert.AreEqual(result.Identity[2], "one");
            //Assert.AreEqual(result.Identity[3], DateTime.Now.Date);
         }
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
            instance = (T)deserializer.ReadObject(reader);
         }

         return instance;
      }
   }
}
