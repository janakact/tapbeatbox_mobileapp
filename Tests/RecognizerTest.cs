using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tapbeatbox.TapLibrary;

namespace Tests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public int TestRecognizeMethod()
        {
            TapRecognizer recognizer = new TapRecognizer(null);
            double[] parms = new double[]{ 2.3, 1.2,3.4};
            Assert.AreEqual(recognizer.recognizeTheSlot(parms), 2);
            Assert.AreEqual(0, 1);
            return 0;
        }


    }
}
