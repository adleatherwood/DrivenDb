using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DrivenDb.MsSql.Tests.Language.MsSql
{
   [DataContract]
   [DbTable(Name = "TimeTable")]
   public partial class TimeTable : DbEntity<TimeTable>, INotifyPropertyChanged
   {

      [DataMember]
      private DateTime m_PartyDate;
      [DataMember]
      private DateTime m_PartyDateTime;
      [DataMember]
      private DateTime m_PartyDateTime2;
      [DataMember]
      private TimeSpan m_PartyTime;
      [DataMember]
      private TimeSpan? m_PartyTime2;

      public TimeTable()
      {
      }

      [DbColumn(Name = "PartyDate", IsPrimaryKey = false, IsDbGenerated = false)]
      public DateTime PartyDate
      {
         get { return m_PartyDate; }
         set
         {
            m_PartyDate = value;
            PropertyChanged(this, new PropertyChangedEventArgs("PartyDate"));
         }
      }

      [DbColumn(Name = "PartyDateTime", IsPrimaryKey = false, IsDbGenerated = false)]
      public DateTime PartyDateTime
      {
         get { return m_PartyDateTime; }
         set
         {
            m_PartyDateTime = value;
            PropertyChanged(this, new PropertyChangedEventArgs("PartyDateTime"));
         }
      }

      [DbColumn(Name = "PartyDateTime2", IsPrimaryKey = false, IsDbGenerated = false)]
      public DateTime PartyDateTime2
      {
         get { return m_PartyDateTime2; }
         set
         {
            m_PartyDateTime2 = value;
            PropertyChanged(this, new PropertyChangedEventArgs("PartyDateTime2"));
         }
      }

      [DbColumn(Name = "PartyTime", IsPrimaryKey = false, IsDbGenerated = false)]
      public TimeSpan PartyTime
      {
         get { return m_PartyTime; }
         set
         {
            m_PartyTime = value;
            PropertyChanged(this, new PropertyChangedEventArgs("PartyTime"));
         }
      }

      [DbColumn(Name = "PartyTime2", IsPrimaryKey = false, IsDbGenerated = false)]
      public TimeSpan? PartyTime2
      {
         get { return m_PartyTime2; }
         set
         {
            m_PartyTime2 = value;
            PropertyChanged(this, new PropertyChangedEventArgs("PartyTime2"));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
