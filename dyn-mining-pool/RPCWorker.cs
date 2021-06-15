﻿using Newtonsoft.Json;
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
            try
            {
                HttpListenerRequest request = context.Request;

                StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string text = reader.ReadToEnd();

                dynamic rpcData = JsonConvert.DeserializeObject<dynamic>(text);

                string strResponse = "";

                string method = rpcData.method;

                Global.updateRand((uint)text.Length);

                if (method == "gethashfunction")
                {
                    Global.updateRand(37);
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
                    Global.updateRand(43);

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
                    Global.CurrBlockTarget = target;
                    string newTarget = target.Substring(2) + "00";
                    block.result.target = newTarget;
                    Global.CurrPoolTarget = newTarget;

                    strResponse = JsonConvert.SerializeObject(block);

                    Global.PrevBlockHash = block.result.previousblockhash;

                }

                else if (method == "submitblock")
                {
                    strResponse = "{\"result\":\"ok\",\"error\":null,\"id\":0}";

                    Global.updateRand(71);

                    string blockHex = rpcData["params"][0];
                    string hashA = DYNProgram.CalcHash(blockHex, Global.AlgoProgram);
                    Console.WriteLine(hashA);
                    string nativeTarget = Global.CurrPoolTarget;
                    byte[] bHashA = DYNProgram.StringToByteArray(hashA);
                    byte[] bNativeTarget = DYNProgram.StringToByteArray(nativeTarget);

                    bool ok = false;
                    bool done = false;
                    int i = 0;
                    while ((!ok) && (i < 32) && (!done))
                        if (bHashA[i] < bNativeTarget[i])
                            ok = true;
                        else if (bHashA[i] == bNativeTarget[i])
                            i++;
                        else
                            done = true;

                    Global.updateRand((uint)i);

                    //they gave us a good hash - add it to their tally and submit if it meets the network hashrate
                    if (ok)
                    {

                        //check if we should submit
                        nativeTarget = Global.CurrBlockTarget;
                        Console.WriteLine(Global.CurrBlockTarget);
                        bNativeTarget = DYNProgram.StringToByteArray(nativeTarget);
                        i = 0;
                        ok = false;
                        done = false;
                        while ((!ok) && (i < 32) && (!done))
                            if (bHashA[i] < bNativeTarget[i])
                                ok = true;
                            else if (bHashA[i] == bNativeTarget[i])
                                i++;
                            else
                                done = true;


                        if (ok)
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

                            string submitResponse = new StreamReader(webresponse.GetResponseStream()).ReadToEnd();
                            Console.WriteLine(submitResponse);




                        }


                    }

                }
                else if (method == "getpooldata")
                {
                    Global.updateRand(103);

                    dynamic poolData = new System.Dynamic.ExpandoObject();
                    poolData.walletAddr = Global.walletAddr;
                    poolData.nonce = (uint)(Global.randomNum(31) * DateTime.UnixEpoch.Ticks);

                    strResponse = JsonConvert.SerializeObject(poolData);

                }




                HttpListenerResponse response = context.Response;

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strResponse);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                uint sum = 0;
                for (int i = 0; i < buffer.Length; i++)
                    sum += buffer[i];

                Global.updateRand(sum);
            }
            catch(Exception e)
            {

            }
        }
    }
}
