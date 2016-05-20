using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Tapbeatbox.TapLibrary;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task NetworkClientTest()
        {
            System.Diagnostics.Debug.WriteLine("Starting Test");
           
            NetworkClient nt = new NetworkClient();

            DataSet dataSet = new DataSet();
            dataSet.setId = new Random().Next(30000,300000);
            dataSet.deviceModel = "Unit Test";


            for (int i = 0; i < 10; i++)
            {
                Data d = new Data();
                d.time = i;
                d.x = i * 2;
                d.y = i * 3;
                d.z = i * i;
                dataSet.dataList.Add(d);
            }

            System.Diagnostics.Debug.WriteLine("Call Send");


            await  NetworkClient.send(dataSet);
            System.Diagnostics.Debug.WriteLine("Done");

        }
    }
}
