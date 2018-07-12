using System.ComponentModel;
using System.Runtime.Serialization;

namespace DrivenDb.MsSql.Tests.Language.MsSql
{
   [DataContract]
   [DbTable(Name = "ImageTable")]
   public partial class ImageTable : DbEntity<ImageTable>, INotifyPropertyChanged
   {

      [DataMember]
      private int m_Id;
      [DataMember]
      private byte[] m_Test;

      public ImageTable()
      {
      }

      [DbColumn(Name = "Id", IsPrimaryKey = false, IsDbGenerated = true)]
      public int Id
      {
         get { return m_Id; }
         set { m_Id = value; }
      }

      [DbColumn(Name = "Test", IsPrimaryKey = false, IsDbGenerated = false)]
      public byte[] Test
      {
         get { return m_Test; }
         set
         {
            m_Test = value;
            PropertyChanged(this, new PropertyChangedEventArgs("Test"));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
