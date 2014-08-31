﻿using System.ComponentModel;
using System.Runtime.Serialization;
using Fastlite.DrivenDb.Core.Contracts;
using Fastlite.DrivenDb.Core.Contracts.Attributes;

namespace Fastlite.DrivenDb.Data.Tests.Base.Tables
{
   [DataContract]
   [DbTable(Name = "MyTable")]
   internal partial class MyTablePartial : DbEntity<MyTablePartial>, INotifyPropertyChanged
   {
      [DataMember]
      private long m_MyIdentity;

      [DataMember]
      private string m_MyString;

      [DataMember]
      private long m_MyNumber;

      [DbColumn(IsDbGenerated = true, IsPrimaryKey = true, Name = "MyIdentity")]
      public long MyIdentity
      {
         get { return m_MyIdentity; }
         set { m_MyIdentity = value; }
      }

      [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "MyString")]
      public string MyString
      {
         get { return m_MyString; }
         set { m_MyString = value; }
      }

      [DbColumn(IsDbGenerated = false, IsPrimaryKey = false, Name = "MyNumber")]
      public long MyNumber
      {
         get { return m_MyNumber; }
         set { m_MyNumber = value; }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged(string propertyName)
      {
         PropertyChangedEventHandler handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
