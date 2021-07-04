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

        public class pendingPayout
        {
            public string wallet;
            public UInt64 amount;

            public pendingPayout(string iWallet, UInt64 iAmount)
            {
                wallet = iWallet;
                amount = iAmount;
            }

        }


        public void run()
        {

            while (!Global.Shutdown)
            {

                Int64 lastRun = (Int64)Convert.ToDouble(Database.GetSetting("last_payout_run"));
                Int64 unixNow = (Int64)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
                if (unixNow - lastRun > Global.SecondsBetweenPayouts())
                {
                    Console.WriteLine("Running payout");

                    try
                    {
                        if (getMiningWalletBalance() > 0)
                        {
                            UInt64 walletBalance = getMiningWalletBalance() - Database.pendingPayouts();
                            if (walletBalance > 1000)
                            {
                                UInt64 fee = (walletBalance * Global.FeePercent()) / 100;
                                sendMoney(Global.ProfitWallet(), fee);
                                walletBalance -= fee;
                                List<miningShare> shares = Database.CountShares(unixNow);
                                UInt64 totalShares = 0;
                                foreach (miningShare s in shares)
                                    totalShares += s.shares;
                                foreach (miningShare s in shares)
                                {
                                    UInt64 payout = (walletBalance * s.shares) / totalShares;
                                    if (payout >= Global.MinPayout() * 100000000)
                                        sendMoney(s.wallet, payout);
                                    else
                                        Database.SavePendingPayout(s.wallet, payout);
                                }

                                List<pendingPayout> pending = Database.GetPendingPayouts();
                                foreach (pendingPayout p in pending)
                                {
                                    if (p.amount > Global.MinPayout() * 100000000)
                                    {
                                        sendMoney(p.wallet, p.amount);
                                        Database.DeletePendingPayout(p.wallet);
                                    }
                                }
                                ClearPendingRewards();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in running payout, retrying in 10 seconds.");
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }

                    Database.UpdateSetting("last_payout_run", unixNow.ToString());


                }

                Thread.Sleep(10000);
            }
        }


        public UInt64 getMiningWalletBalance()
        {
            var webrequest = (HttpWebRequest)WebRequest.Create(Global.FullNodeRPC());

            string postData = "{\"jsonrpc\": \"1.0\", \"id\": \"1\", \"method\": \"getbalance\", \"params\": [\"*\", 10]}";
            var data = Encoding.ASCII.GetBytes(postData);

            webrequest.Method = "POST";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.ContentLength = data.Length;

            var username = Global.FullNodeUser();
            var password = Global.FullNodePass();
            string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            webrequest.Headers.Add("Authorization", "Basic " + encoded);


            using (var stream = webrequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }


            var webresponse = (HttpWebResponse)webrequest.GetResponse();

            string submitResponse = new StreamReader(webresponse.GetResponseStream()).ReadToEnd();

            dynamic walletData = JsonConvert.DeserializeObject<dynamic>(submitResponse);

            UInt64 amount = (UInt64)(Convert.ToDecimal(walletData["result"]) * 100000000m);

            return amount;
        }

        public void sendMoney(string wallet, UInt64 amount)
        {

            decimal dAmount = (decimal)amount / 100000000m;

            var webrequest = (HttpWebRequest)WebRequest.Create(Global.FullNodeRPC());

            string postData = "{\"jsonrpc\": \"1.0\", \"id\": \"1\", \"method\": \"sendtoaddress\", \"params\": [\"" + wallet + "\", " + dAmount + ", \"\", \"\", true]}";
            var data = Encoding.ASCII.GetBytes(postData);
            Console.WriteLine(postData);

            webrequest.Method = "POST";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.ContentLength = data.Length;

            var username = Global.FullNodeUser();
            var password = Global.FullNodePass();
            string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            webrequest.Headers.Add("Authorization", "Basic " + encoded);


            using (var stream = webrequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var webresponse = (HttpWebResponse)webrequest.GetResponse();

            string submitResponse = new StreamReader(webresponse.GetResponseStream()).ReadToEnd();

            Database.SavePayout(wallet, amount);

        }
    }
}
