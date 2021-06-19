using System;
using System.Collections.Generic;
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
    }
}
