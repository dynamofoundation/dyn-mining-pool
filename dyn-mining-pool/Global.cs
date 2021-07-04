using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dyn_mining_pool
{
    public class Global
    {

        public static bool Shutdown = false;

        public static string AlgoProgram;
        public static string PrevBlockHash;
        public static string CurrPoolTarget;
        public static string CurrBlockTarget;


        /*
        public static string PoolListenerEndpoint = "http://10.1.0.29:6434/";

        public static string FullNodeRPC = "http://10.1.0.90:6433/";
        public static string FullNodeUser = "user";
        public static string FullNodePass = "123456";
        public static string DatabaseLocation = @"C:\pool\mining_pool.db";


        public static uint feePercent = 2;

        public static int secondsBetweenPayouts = 60;//* 60; //60 * 60 * 24;     //one day
        public static UInt64 minPayout = 10;

        public static string miningWallet = "dy1qnyyut8z689gm8zq2mem2dxn086kpwnhdxt3ex8";
        public static string profitWallet = "dy1qqsyj5s9t8eqtzn9x8twfnelxj7am9q9dntt55y";
        */

        public static string PoolListenerEndpoint() {
            return settings["PoolListenerEndpoint"];
        }

        public static string FullNodeRPC()
        {
            return settings["FullNodeRPC"];
        }

        public static string FullNodeUser()
        {
            return settings["FullNodeUser"];
        }

        public static string FullNodePass()
        {
            return settings["FullNodePass"];
        }
        public static string DatabaseLocation()
        {
            return settings["DatabaseLocation"];
        }



        public static uint FeePercent()
        {
            return Convert.ToUInt32(settings["FeePercent"]);
        }


        public static int SecondsBetweenPayouts()
        {
            return Convert.ToInt32(settings["SecondsBetweenPayouts"]);
        }

        public static UInt64 MinPayout()
        {
            return Convert.ToUInt64(settings["MinPayout"]);
        }


        public static string MiningWallet()
        {
            return settings["MiningWallet"];
        }
    
    public static string ProfitWallet()
        {
            return settings["ProfitWallet"];
        }


        public static Dictionary<string, string> settings = new Dictionary<string, string>();


        public static uint randSeed;
        public static Object randLock = new Object();
        public static uint RandomNum(uint x)
        {
            lock (randLock)
            {
                randSeed += x;
                return randSeed;
            }
        }

        public static void UpdateRand(uint x)
        {
            lock (randLock)
            {
                randSeed += x;
            }
        }

        public static void LoadSettings()
        {
            using (StreamReader r = new StreamReader("settings.txt"))
            {
                string json = r.ReadToEnd();
                settings = JsonConvert.DeserializeObject<Dictionary<string,string>>(json);
            }
        }

    }
}
