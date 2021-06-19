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

            { "result":109.00000000,"error":null,"id":"1"}

            return 0;
        }


        public void sendMoney(string wallet, UInt64 amount)
        {

        }
    }
}
