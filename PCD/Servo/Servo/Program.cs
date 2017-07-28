using System;
using System.IO.Ports;
using System.Threading;

namespace Servo
{
    class Program
    {
        const int BaudNumber = 9600;
        static string SerialPortName = string.Empty;

        static void Main(string[] args)
        {
            try
            {
                EstablishSerialConnection();

                while (true)
                {
                    WriteToArduino(Console.ReadLine());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press any key.");

                Console.ReadKey();
            }
        }

        private static void EstablishSerialConnection()
        {
            Console.WriteLine("Establishing serial connection...");

            if(!FindArduinoSerialPort())
            {
                throw new Exception("Cannot establish serial connection");
            }

            Console.WriteLine("Connect setting = Serial Port Name : " + SerialPortName + " Baud Number : " + BaudNumber.ToString());
        }

        private static bool FindArduinoSerialPort()
        {
            string[] serialPorts = SerialPort.GetPortNames();

            for (int i = 0; i < serialPorts.Length; i++)
            {
                using (SerialPort serialPort = new SerialPort(serialPorts[i], BaudNumber))
                {
                    serialPort.Open();

                    while (!serialPort.IsOpen)
                    {
                        Thread.Sleep(25);
                    }

                    serialPort.Write("E");

                    Thread.Sleep(25);

                    if (ReadFromArduino(serialPort))
                    {
                        SerialPortName = serialPorts[i];

                        return true;
                    }
                }
            }

            return false;
        }

        private static void WriteToArduino(string input)
        {
            using (SerialPort serialPort = new SerialPort(SerialPortName, BaudNumber))
            {
                serialPort.Open();

                while (!serialPort.IsOpen)
                {
                    Thread.Sleep(25);
                }

                serialPort.Write(input);

                Thread.Sleep(25);

                ReadFromArduino(serialPort);
            }
        }

        private static bool ReadFromArduino(SerialPort serialPort)
        {
            for(int i = 0; i < 1000; i++)
            {
                string read = serialPort.ReadExisting();

                if (read == "E")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
