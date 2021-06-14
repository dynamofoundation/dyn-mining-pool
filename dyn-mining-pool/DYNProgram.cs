using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace dyn_mining_pool
{
    public class DYNProgram
    {



        public static string CalcHash(string blockHex, string strProgram)
        {

            string headerHex = blockHex.Substring(0, 160);

            byte[] header = StringToByteArray(headerHex);
            for (int i = 0; i < 16; i++)
            {
                byte tmp = header[4+i];
                header[4+i] = header[4+31 - i];
                header[4+31 - i] = tmp;
            }


            string merkleRoot = headerHex.Substring(72,64);
            string hashPrev = Global.PrevBlockHash;  //todo get from header

            SHA256 sha = SHA256.Create();
            byte[] result = sha.ComputeHash(header);

            uint[] iResult = ConvertToInt(result);

            byte[] res1 = ConvertToBytes(iResult);

            /*
            string shaHex = BitConverter.ToString(result).Replace("-", "");
            Console.WriteLine(shaHex);
            shaHex = BitConverter.ToString(res1).Replace("-", "");
            Console.WriteLine(shaHex);
            */



            string[] lines = strProgram.Split("\n");

            int line_ptr = 0;
            int loop_counter = 0;
            uint memory_size = 0;
            uint[] memPool =  null;

            while (line_ptr < lines.Length)
            {
                string[] tokens = lines[line_ptr].Split(" ");
                if (tokens[0] == "ADD")
                {
                    iResult = SwapIntBELE(iResult);
                    uint[] arg1 = ConvertHexToIntLE(tokens[1]);
                    for (int i = 0; i < 8; i++)
                        iResult[i] += arg1[i];
                    iResult = SwapIntBELE(iResult);
                }
                else if (tokens[0] == "XOR")
                {
                    uint[] arg1 = ConvertHexToIntLE(tokens[1]);
                    iResult = SwapIntBELE(iResult);
                    for (int i = 0; i < 8; i++)
                        iResult[i] ^= arg1[i];
                    iResult = SwapIntBELE(iResult);
                }
                else if (tokens[0] == "SHA2")
                {
                    //multiple rounds
                    if (tokens.Length == 2)
                    {
                        loop_counter = Int32.Parse(tokens[1]);
                        for (int i = 0; i < loop_counter; i++)
                        {
                            sha.Initialize();
                            byte[] bResult = ConvertToBytes(iResult);
                            byte[] tmp = sha.ComputeHash(bResult);
                            iResult = ConvertToInt(tmp);
                        }

                    }
                    //single round
                    else
                    {
                        sha.Initialize();
                        byte[] bResult = ConvertToBytes(iResult);
                        byte[] tmp = sha.ComputeHash(bResult);
                        iResult = ConvertToInt(tmp);

                    }
                }
                else if (tokens[0] == "MEMGEN")
                {
                    memory_size = (uint)Int32.Parse(tokens[2]);
                    memPool = new uint[memory_size * 8];
                    for ( int i = 0; i < memory_size; i++ )
                    {
                        if (tokens[1] == "SHA2")
                        {
                            sha.Initialize();
                            byte[] bResult = ConvertToBytes(iResult);
                            byte[] output = sha.ComputeHash(bResult);
                            iResult = ConvertToInt(output);
                            uint[] val = SwapIntBELE(iResult);
                            for (int j = 0; j < 8; j++)
                                memPool[i * 8 + j] = val[j];
                        }
                    }

                }
                else if (tokens[0] == "MEMADD")
                {
                    uint[] arg1 = ConvertHexToIntLE(tokens[1]);
                    for ( int i = 0; i < memory_size; i++)
                        for (int j = 0; j < 8; j++)
                            memPool[i*8+j] += arg1[j];

                }
                else if (tokens[0] == "MEMXOR")
                {
                    uint[] arg1 = ConvertHexToIntLE(tokens[1]);
                    for (int i = 0; i < memory_size; i++)
                        for (int j = 0; j < 8; j++)
                            memPool[i * 8 + j] ^= arg1[j];

                }
                else if (tokens[0] == "READMEM")
                {
                    uint index = 0;
                    if (tokens[1] == "MERKLE")
                    {
                        byte[] bMerkle = StringToByteArray(merkleRoot);
                        uint[] iMerkle = ConvertToInt(bMerkle);
                        index = iMerkle[7] % memory_size;
                        for (int i = 0; i < 8; i++)
                            iResult[i] = memPool[index * 8 + i];
                        iResult = SwapIntBELE(iResult);
                    }
                    else if (tokens[1] == "HASHPREV")
                    {
                        byte[] bHashPrev = StringToByteArray(hashPrev);
                        index = bHashPrev[0] % memory_size;
                        for (int i = 0; i < 8; i++)
                            iResult[i] = memPool[index * 8 + i];
                        iResult = SwapIntBELE(iResult);
                    }
                }

                /*
                byte[] res2 = ConvertToBytes(iResult);
                shaHex = BitConverter.ToString(res2).Replace("-", "");
                Console.WriteLine(line_ptr + "   " + shaHex);
                */


                line_ptr++;
            }


            byte[] bHashresult = ConvertToBytes(iResult);
            return BitConverter.ToString(bHashresult).Replace("-", "");
        }


        public static byte[] ConvertToBytes (uint[] data)
        {
            byte[] bResult = new byte[32];
            for ( int i = 0; i < 8; i++)
            {
                bResult[i * 4] = (byte)(data[i] >> 24);
                bResult[i * 4 + 1] = (byte)((data[i] & 0x00FF0000) >> 16);
                bResult[i * 4 + 2] = (byte)((data[i] & 0x0000FF00) >> 8);
                bResult[i * 4 + 3] = (byte)(data[i] & 0x000000FF);
            }

            return bResult;

        }

        public static uint[] SwapIntBELE(uint[] data)
        {
            uint[] iResult = new uint[8];
            for (int i = 0; i < 8; i++)
            {
                byte b1 = (byte)(data[i] >> 24);
                byte b2 = (byte)((data[i] & 0x00FF0000) >> 16);
                byte b3 = (byte)((data[i] & 0x0000FF00) >> 8);
                byte b4 = (byte)(data[i] & 0x000000FF);
                iResult[i] = ((uint)b4 << 24) + ((uint)b3 << 16) + ((uint)b2 << 8) + (uint)b1;
            }
            return iResult;
        }

        public static uint[] ConvertToInt (byte[] data)
        {
            uint[] iResult = new uint[8];
            for (int i = 0; i < 8; i++)
                iResult[i] = ((uint)data[i * 4] << 24) + ((uint)data[i * 4 + 1] << 16) + ((uint)data[i * 4 + 2] << 8) + (uint)data[i * 4 + 3];
            return iResult;
        }

        public static uint[] ConvertToIntLE(byte[] data)
        {
            uint[] iResult = new uint[8];
            for (int i = 0; i < 8; i++)
                iResult[i] = ((uint)data[i * 4 + 3] << 24) + ((uint)data[i * 4 + 2] << 16) + ((uint)data[i * 4 + 1] << 8) + (uint)data[i * 4];
            return iResult;
        }


        public static uint[] ConvertHexToIntLE(string data)
        {
            byte[] bdata = new byte[32];
            for ( int i = 0; i < 32; i++)
                bdata[i] = (byte)((GetHexVal(data[i << 1]) << 4) + (GetHexVal(data[(i << 1) + 1])));

            uint[] result = new uint[8];
            for ( int i = 0; i < 8; i++)
            {
                result[i] = ((uint)bdata[i * 4 + 3] << 24) + ((uint)bdata[i * 4 + 2] << 16) + ((uint)bdata[i * 4 + 1] << 8) + (uint)bdata[i * 4];
            }
            return result;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }
    }
}
