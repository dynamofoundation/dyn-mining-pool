using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace dyn_mining_pool
{
    public class Distributor
    {

        public class miningShare
        {
            public string wallet;
            public UInt64 shares;

            public miningShare(string iWallet, UInt64 iShares)
            {
                wallet = iWallet;
                shares = iShares;
            }

        }

        public void run()
        {

            while (!Global.Shutdown)
            {

                Int64 lastRun = (Int64)Convert.ToDouble(Database.GetSetting("last_payout_run"));
                Int64 unixNow = (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
                if (unixNow - lastRun > Global.secondsBetweenPayouts)
                {
                    Console.WriteLine("Running payout");
                    Database.UpdateSetting("last_payout_run", unixNow.ToString());

                    UInt64 walletBalance = getMiningWalletBalance();
                    if (walletBalance > 0)
                    {
                        UInt64 fee = (walletBalance * Global.feePercent) / 100;
                        sendMoney(Global.profitWallet, fee);
                        walletBalance -= fee;
                        List<miningShare> shares = Database.CountShares(unixNow);
                        UInt64 totalShares = 0;
                        foreach (miningShare s in shares)
                            totalShares += s.shares;
                        foreach (miningShare s in shares)
                        {
                            UInt64 payout = (walletBalance * s.shares) / totalShares;
                            sendMoney(s.wallet, payout);
                        }
                    }


                }

                Thread.Sleep(10000);
            }
        }


        public UInt64 getMiningWalletBalance()
        {
            var webrequest = (HttpWebRequest)WebRequest.Create(Global.FullNodeRPC);

            string postData = "{\"jsonrpc\": \"1.0\", \"id\": \"1\", \"method\": \"getbalance\", \"params\": [\"*\", 10]}";
            var data = Encoding.ASCII.GetBytes(postData);
            Console.WriteLine(postData);

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

            dynamic walletData = JsonConvert.DeserializeObject<dynamic>(submitResponse);

            UInt64 amount = (UInt64)(Convert.ToDecimal(walletData["result"]) * 100000000m);

            return amount;
        }


        public void sendMoney(string wallet, UInt64 amount)
        {

            decimal dAmount = (decimal)amount / 100000000m;

            var webrequest = (HttpWebRequest)WebRequest.Create(Global.FullNodeRPC);

            string postData = "{\"jsonrpc\": \"1.0\", \"id\": \"1\", \"method\": \"sendtoaddress\", \"params\": [\"" + wallet + "\", " + dAmount + ", \"\", \"\", true]}";
            var data = Encoding.ASCII.GetBytes(postData);
            Console.WriteLine(postData);

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
