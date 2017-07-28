using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace URGLibrary
{
    public class URGLibrary
    {
        static string IPAddress = "192.168.1.11";
        static int PortNumber = 10940;

        /// <summary>
        /// get connection information from user.
        /// </summary>
        public static void EstablishURGConnection(out string ipAdderss, out int portNumber)
        {
            ipAdderss = IPAddress;
            portNumber = PortNumber;

            Console.WriteLine("Connect setting = IP Address : " + ipAdderss + " Port number : " + portNumber.ToString());
        }

        /// <summary>
        /// Create MD command
        /// </summary>
        /// <param name="start">measurement start step</param>
        /// <param name="end">measurement end step</param>
        /// <param name="grouping">grouping step number</param>
        /// <param name="skips">skip scan number</param>
        /// <param name="scans">get scan numbar</param>
        /// <returns>created command</returns>
        public static string MD(int start, int end, int grouping = 1, int skips = 0, int scans = 0)
        {
            return "MD" + start.ToString("D4") + end.ToString("D4") + grouping.ToString("D2") + skips.ToString("D1") + scans.ToString("D2") + "\n";
        }

        public static string SCIP2()
        {
            return "SCIP2.0" + "\n";
        }

        public static string QT()
        {
            return "QT\n";
        }

        /// <summary>
        /// read MD command
        /// </summary>
        /// <param name="getCommand">received command</param>
        /// <param name="timeStamp">timestamp data</param>
        /// <param name="distances">distance data</param>
        /// <returns>is successful</returns>
        public static bool MD(string getCommand, ref long timeStamp, ref List<long> distances)
        {
            distances.Clear();

            string[] splitCommand = getCommand.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (!splitCommand[0].StartsWith("MD"))
            {
                return false;
            }

            if (splitCommand[1].StartsWith("00"))
            {
                return true;
            }
            else
            {
                if (splitCommand[1].StartsWith("99"))
                {
                    timeStamp = Decode(splitCommand[2], 4);
                    MagnitudeData(splitCommand, 3, ref distances);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// write data
        /// </summary>
        public static bool WriteMagnitudes(NetworkStream networkStream, string data)
        {
            if (networkStream.CanWrite)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                networkStream.Write(buffer, 0, buffer.Length);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Read to "\n\n" from NetworkStream
        /// </summary>
        /// <returns>receive data</returns>
        public static string ReadMagnitudes(NetworkStream networkStream)
        {
            if (networkStream.CanRead)
            {
                StringBuilder stringBuilder = new StringBuilder();

                bool isNL = false;
                bool isNL2 = false;

                while (!isNL2)
                {
                    char buffer = (char)networkStream.ReadByte();

                    if (buffer == '\n')
                    {
                        if (isNL)
                        {
                            isNL2 = true;
                        }
                        else
                        {
                            isNL = true;
                        }
                    }
                    else
                    {
                        isNL = false;
                    }

                    stringBuilder.Append(buffer);
                }

                return stringBuilder.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// read magnitude data
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="startLine"></param>
        /// <returns></returns>
        public static bool MagnitudeData(string[] lines, int startLine, ref List<long> distances)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = startLine; i < lines.Length; ++i)
            {
                stringBuilder.Append(lines[i].Substring(0, lines[i].Length - 1));
            }

            return DecodeArray(stringBuilder.ToString(), 3, ref distances);
        }

        /// <summary>
        /// decode part of string 
        /// </summary>
        /// <param name="data">encoded string</param>
        /// <param name="size">encode size</param>
        /// <param name="offset">decode start position</param>
        /// <returns>decode result</returns>
        public static long Decode(string data, int size, int offset = 0)
        {
            long value = 0;

            for (int i = 0; i < size; ++i)
            {
                value <<= 6;
                value |= (long)data[offset + i] - 0x30;
            }

            return value;
        }

        /// <summary>
        /// decode multiple data
        /// </summary>
        /// <param name="data">encoded string</param>
        /// <param name="size">encode size</param>
        /// <returns>decode result</returns>
        public static bool DecodeArray(string data, int size, ref List<long> decodedData)
        {
            for (int pos = 0; pos <= data.Length - size; pos += size)
            {
                decodedData.Add(Decode(data, size, pos));
            }

            return true;
        }
    }
}