using System.ComponentModel;
using System.Runtime.Serialization;

namespace DrivenDb.MsSql.Tests.Language.MsSql
{
   [DataContract]
   [DbTable(Name = "VarbinaryTest")]
   public partial class VarbinaryTest : DbEntity<VarbinaryTest>, INotifyPropertyChanged
   {
      [DataMember]
      private int m_Id;
      [DataMember]
      private byte[] m_Value1;
      [DataMember]
      private byte[] m_Value2;
      [DataMember]
      private string m_Value3;

      [DbColumn(Name = "Id", IsPrimaryKey = true, IsDbGenerated = true)]
      public int Id
      {
         get { return m_Id; }
         set { m_Id = value; }
      }

      [DbColumn(Name = "Value1", IsPrimaryKey = false, IsDbGenerated = false)]
      public byte[] Value1
      {
         get { return m_Value1; }
         set
         {
            if (value == m_Value1) return;
            m_Value1 = value;
            PropertyChanged(this, new PropertyChangedEventArgs("Value1"));
         }
      }

      [DbColumn(Name = "Value2", IsPrimaryKey = false, IsDbGenerated = false)]
      public byte[] Value2
      {
         get { return m_Value2; }
         set
         {
            if (value == m_Value2) return;
            m_Value2 = value;
            PropertyChanged(this, new PropertyChangedEventArgs("Value2"));
         }
      }

      [DbColumn(Name = "Value3", IsPrimaryKey = false, IsDbGenerated = false)]
      public string Value3
      {
         get { return m_Value3; }
         set
         {
            if (value == m_Value3) return;
            m_Value3 = value;
            PropertyChanged(this, new PropertyChangedEventArgs("Value3"));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
