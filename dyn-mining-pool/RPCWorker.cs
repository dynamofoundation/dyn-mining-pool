using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;

namespace dyn_mining_pool
{
    public class RPCWorker
    {
        public HttpListenerContext context;
        static readonly HttpClient client = new HttpClient();



        public void run()
        {
            HttpListenerRequest request = context.Request;

            StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
            string text = reader.ReadToEnd();

            dynamic rpcData = JsonConvert.DeserializeObject<dynamic>(text);

            string strResponse = "";

            string method = rpcData.method;
            if (method == "gethashfunction")
            {
                var webrequest = (HttpWebRequest)WebRequest.Create(Global.FullNodeRPC);

                var postData = text;
                var data = Encoding.ASCII.GetBytes(postData);

                webrequest.Method = "POST";
                webrequest.ContentType = "application/x-www-form-urlencoded";
                webrequest.ContentLength = data.Length;

                var username = Global.FullNodeUser;
                var password = Global.FullNodePass;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                webrequest.Headers.Add("Authorization", "Basic " + encoded);


                using (var stream = webrequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var webresponse = (HttpWebResponse)webrequest.GetResponse();

                strResponse = new StreamReader(webresponse.GetResponseStream()).ReadToEnd();

                dynamic progData = JsonConvert.DeserializeObject<dynamic>(strResponse);

                Global.AlgoProgram = progData.result[0].program;


            }
            else if (method == "getblocktemplate")
            {
                var webrequest = (HttpWebRequest)WebRequest.Create(Global.FullNodeRPC);

                var postData = text;
                var data = Encoding.ASCII.GetBytes(postData);

                webrequest.Method = "POST";
                webrequest.ContentType = "application/x-www-form-urlencoded";
                webrequest.ContentLength = data.Length;

                var username = Global.FullNodeUser;
                var password = Global.FullNodePass;
                string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                webrequest.Headers.Add("Authorization", "Basic " + encoded);


                using (var stream = webrequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var webresponse = (HttpWebResponse)webrequest.GetResponse();

                string blockResponse = new StreamReader(webresponse.GetResponseStream()).ReadToEnd();

                dynamic block = JsonConvert.DeserializeObject<dynamic>(blockResponse);

                string target = block.result.target;
                string newTarget = target.Substring(4) + "0000";
                block.result.target = newTarget;

                strResponse = JsonConvert.SerializeObject(block);

                Global.PrevBlockHash = block.result.previousblockhash;

            }

            else if (method == "submitblock")
            {

                string blockHex = rpcData["params"][0];
                DYNProgram.CalcHash(blockHex, Global.AlgoProgram);

            }



            HttpListenerResponse response = context.Response;

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strResponse);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
