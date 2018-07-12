/**************************************************************************************
 * Original Author : Anthony Leatherwood (adleatherwood@gmail.com)
 * Source Location : https://github.com/Fastlite/DrivenDb
 *
 * This source is subject to the Microsoft Public License.
 * Link: http://www.microsoft.com/en-us/openness/licenses.aspx
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 **************************************************************************************/

using System.Windows;
using DrivenDb;
using DrivenDbConsole.Contracts;
using DrivenDbConsole.Contracts.MsSql;
using DrivenDbConsole.Contracts.SqLite;
using DrivenDbConsole.Generator;
using DrivenDbConsole.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DrivenDbConsole
{
   internal class MainWindowViewModel : INotifyPropertyChanged
   {
      public MainWindowViewModel()
      {
         CString = "Integrated Security=SSPI;Persist Security Info=False;User ID=sa;Initial Catalog=AdventureWorksLT2008;Data Source=.";
         AppName = "MyApp";
         Namespace = "MyApp.Contracts";
         TableFilter = "%";
         ReadOnlyTableFilter = "%";

         Generate = new RelayCommand(o => OnGenerate(), o => CanGenerate());
         Clip = new RelayCommand(o => OnClip(), o => CanClip());

         AccessorTypes = Enum.GetValues(typeof(DbAccessorType)).Cast<DbAccessorType>().Select(v => v.ToString());

         UseLinq = false;
         LessChanges = false;
         WriteSchema = false;
         ScriptDefaults = false;
         UseUnspecified = true;
      }

      public IEnumerable<string> AccessorTypes
      {
         get;
         private set;
      }

      public string SelectedAccessorType
      {
         get;
         set;
      }

      public string CString
      {
         get;
         set;
      }

      public string AppName
      {
         get;
         set;
      }

      public string Namespace
      {
         get;
         set;
      }

      public string TableFilter
      {
         get;
         set;
      }

      public string ReadOnlyTableFilter
      {
         get;
         set;
      }

      public bool UseLinq
      {
         get;
         set;
      }

      public bool WriteSchema
      {
         get;
         set;
      }

      public bool LessChanges
      {
         get;
         set;
      }

      public bool ScriptDefaults
      {
         get;
         set;
      }

      public bool UseUnspecified
      {
         get;
         set;
      }

      public string Output
      {
         get;
         private set;
      }

      public ICommand Generate
      {
         get;
         private set;
      }

      public ICommand Clip
      {
         get;
         private set;
      }

      public bool CanGenerate()
      {
         return !String.IsNullOrWhiteSpace(CString)
            && !String.IsNullOrWhiteSpace(AppName)
            && !String.IsNullOrWhiteSpace(Namespace)
            && !String.IsNullOrWhiteSpace(SelectedAccessorType) && (SelectedAccessorType == "MsSql" || SelectedAccessorType == "SqLite")
            && (!String.IsNullOrWhiteSpace(TableFilter) || !String.IsNullOrWhiteSpace(ReadOnlyTableFilter));
      }

      public void OnGenerate()
      {
         try
         {
            IInfoExtractor extractor = null;

            switch (SelectedAccessorType)
            {
               case "MsSql":
                  extractor = new MsSqlInfoExtractor(CString);
                  break;

               case "SqLite":
                  extractor = new SqLiteInfoExtractor(CString);
                  break;
            }

            if (extractor != null)
            {
               var writables = (TableFilter ?? "").Split(',')
                  .Where(f => !String.IsNullOrWhiteSpace(f))
                  .Select(f => f.Trim())
                  .ToArray();

               var readables = (ReadOnlyTableFilter ?? "").Split(',')
                  .Where(f => !String.IsNullOrWhiteSpace(f))
                  .Select(f => f.Trim())
                  .ToArray();

               var writableInfo = extractor.GetDatabaseMetadata(writables);
               var readableInfo = extractor.GetDatabaseMetadata(readables);
               var fileWriter = new FileWriter();
               var entityWriter = new EntityWriter();
               var typeWriter = new TypeWriter();

               var output = fileWriter.WriteHeader(Namespace);
               output += entityWriter.Execute(AppName, Namespace, UseLinq, LessChanges, WriteSchema, ScriptDefaults, UseUnspecified, writableInfo);
               output += typeWriter.Execute(readableInfo);
               output += fileWriter.WriteFooter();

               Output = output;

               OnPropertyChanged("Output");
            }
         }
         catch (Exception e)
         {
            Output = e.StackTrace;
            OnPropertyChanged("Output");
         }
      }

      public bool CanClip()
      {
         return !String.IsNullOrWhiteSpace(Output);
      }

      public void OnClip()
      {
         try
         {
            Clipboard.SetText(Output);
         }
         catch (COMException)
         {
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      private void OnPropertyChanged(string name)
      {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
   }
}