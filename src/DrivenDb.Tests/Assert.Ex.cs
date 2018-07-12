using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DrivenDb.Tests
{
    internal static class AssertX
    {
        public static void Throws<T>(Action a)
            where T : Exception
        {
            try
            {
                a();
            }
            catch(Exception e)
            {
                if (e.GetType() != typeof(T))
                    Assert.Fail();
            }
        }
    }
}
