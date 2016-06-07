using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Tapbeatbox.TapLibrary
{
    public class NetworkClient
    {
        public static async Task send(DataSet dataSet)
        {
            //Where we are posting to: 
            Uri theUri = new Uri(Constant.networkURI);
            System.Diagnostics.Debug.WriteLine("URI Created");

            //Create an Http client and set the headers we want 
            HttpClient aClient = new HttpClient();
            aClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            aClient.DefaultRequestHeaders.Add("X-ZUMO-INSTALLATION-ID", "8bc6aea9-864a-44fc-9b4b-87ec64e123bd");
            aClient.DefaultRequestHeaders.Add("X-ZUMO-APPLICATION", "OabcWgaGVdIXpqwbMTdBQcxyrOpeXa20");
            aClient.DefaultRequestHeaders.Host = theUri.Host;
            System.Diagnostics.Debug.WriteLine("Client Created");

            ////Class that will be serialized into Json and posted 
            //TodoItem2 todoItem2 = new TodoItem2();

            ////Set some values 
            //todoItem2.username = "ddd";
            //todoItem2.password = "123";
            //System.Diagnostics.Debug.WriteLine("Objects Created");


            


            //Create a Json Serializer for our type 
            DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(typeof(DataSet));
            System.Diagnostics.Debug.WriteLine("jsoSer Created");

            // use the serializer to write the object to a MemoryStream 
            MemoryStream ms = new MemoryStream();
            jsonSer.WriteObject(ms, dataSet);
            ms.Position = 0;

            System.Diagnostics.Debug.WriteLine("Memory Stream Created");

            //use a Stream reader to construct the StringContent (Json) 
            StreamReader sr = new StreamReader(ms);
            // Note if the JSON is simple enough you could ignore the 5 lines above that do the serialization and construct it yourself 
            // then pass it as the first argument to the StringContent constructor 
            StringContent theContent = new StringContent(sr.ReadToEnd(), System.Text.Encoding.UTF8, "application/json");
            System.Diagnostics.Debug.WriteLine("Content:" + theContent);

            //Post the data 
            HttpResponseMessage aResponse = await aClient.PostAsync(theUri, theContent);

            System.Diagnostics.Debug.WriteLine("Post Sent");

            if (aResponse.IsSuccessStatusCode)
            { 
                System.Diagnostics.Debug.WriteLine("Success: " + aResponse.Content.ToString());
                // return aResponse.Content.ToString();
            }
            else
            {
                // show the response status code 
                String failureMsg = "HTTP Status: " + aResponse.StatusCode.ToString() + " – Reason: " + aResponse.ReasonPhrase;
                System.Diagnostics.Debug.WriteLine("Failed"+failureMsg);
               // return failureMsg;
            }
        }
    }


    [DataContract]
    public class TodoItem2
    {
        //[DataMember(Name = "slotId")]
        //public int slotId { get; set; }

        [DataMember(Name = "username")]
        public string username { get; set; }

        [DataMember(Name = "password")]
        public string password { get; set; }
    }

   
}
