using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using URGLibrary;
using System.IO;
using System.IO.Ports;
using System.Threading;

class GetPCD
{
    const int ArduinoEndAngle = 180;

    const int Scans = 4;

    const int URGRotationAngle = 270;
    const int URGEndStep = 1080;

    static SerialPort ArduinoSerialPort;

    static void Main(string[] args)
    {
        try
        {
            EstablishSerialConnection();

            WriteToFile(VectorMultiplexer(GetRGBLibrary.GetRGBLibrary.RGBMultiplexer(GetCulledMagnitudes(GetCalibratedMagnitudes(GetMagnitudes())), ArduinoEndAngle, URGEndStep), GetUnitVectors()));
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
        string serialPortName = "COM4";
        int baudNumber = 9600;

        Console.WriteLine("Connect setting = Serial Port Name : " + serialPortName + " Baud Number : " + baudNumber.ToString());

        ArduinoSerialPort = new SerialPort(serialPortName, baudNumber);
        ArduinoSerialPort.Open();
    }

    private static double[,] GetCulledMagnitudes(double[,] calibratedMagnitudes)
    {
        double[,] culledMagnitudes = new double[ArduinoEndAngle + 1, URGEndStep + 1];
        double averageMagnitude = 0;

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            for (int j = 0; j < URGEndStep + 1; j++)
            {
                averageMagnitude += calibratedMagnitudes[i, j];
            }
        }

        averageMagnitude /= ((ArduinoEndAngle + 1) + (URGEndStep + 1));

        double maxMagnitude = averageMagnitude * 2;

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            for (int j = 0; j < URGEndStep + 1; j++)
            {
                if(calibratedMagnitudes[i, j] <= maxMagnitude)
                {
                    culledMagnitudes[i, j] = calibratedMagnitudes[i, j];
                }
                else
                {
                    culledMagnitudes[i, j] = 0;
                }
            }
        }

        // show culled magnitudes data
        Console.WriteLine();
        Console.WriteLine("Culled Magnitudes Data: ");

        Console.WriteLine(culledMagnitudes[0, 0] + " " + culledMagnitudes[0, URGEndStep / 3] + " " + culledMagnitudes[0, URGEndStep / 2] + " " + culledMagnitudes[0, URGEndStep - (URGEndStep / 3)] + " " + culledMagnitudes[0, URGEndStep]);
        Console.WriteLine(culledMagnitudes[ArduinoEndAngle / 3, 0] + " " + culledMagnitudes[ArduinoEndAngle / 3, URGEndStep / 3] + " " + culledMagnitudes[ArduinoEndAngle / 3, URGEndStep / 2] + " " + culledMagnitudes[ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)] + " " + culledMagnitudes[ArduinoEndAngle / 3, URGEndStep]);
        Console.WriteLine(culledMagnitudes[ArduinoEndAngle / 2, 0] + " " + culledMagnitudes[ArduinoEndAngle / 2, URGEndStep / 3] + " " + culledMagnitudes[ArduinoEndAngle / 2, URGEndStep / 2] + " " + culledMagnitudes[ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)] + " " + culledMagnitudes[ArduinoEndAngle / 2, URGEndStep]);
        Console.WriteLine(culledMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), 0] + " " + culledMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3] + " " + culledMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2] + " " + culledMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)] + " " + culledMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep]);
        Console.WriteLine(culledMagnitudes[ArduinoEndAngle, 0] + " " + culledMagnitudes[ArduinoEndAngle, URGEndStep / 3] + " " + culledMagnitudes[ArduinoEndAngle, URGEndStep / 2] + " " + culledMagnitudes[ArduinoEndAngle, URGEndStep - (URGEndStep / 3)] + " " + culledMagnitudes[ArduinoEndAngle, URGEndStep]);

        return culledMagnitudes;
    }

    private static double[,] GetCalibratedMagnitudes(double[,] magnitudes)
    {
        double[,] calibratedMagnitudes = new double[ArduinoEndAngle + 1, URGEndStep + 1];
        double[,] normalisedMagnitudes = GetNormalisedMagnitudes(magnitudes);

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            for (int j = 0; j < URGEndStep + 1; j++)
            {
                calibratedMagnitudes[i, j] = normalisedMagnitudes[i, j] * (100 / 0.913974829505042);
            }
        }

        // show calibrated magnitudes data
        Console.WriteLine();
        Console.WriteLine("Calibrated Magnitudes Data: ");

        Console.WriteLine(calibratedMagnitudes[0, 0] + " " + calibratedMagnitudes[0, URGEndStep / 3] + " " + calibratedMagnitudes[0, URGEndStep / 2] + " " + calibratedMagnitudes[0, URGEndStep - (URGEndStep / 3)] + " " + calibratedMagnitudes[0, URGEndStep]);
        Console.WriteLine(calibratedMagnitudes[ArduinoEndAngle / 3, 0] + " " + calibratedMagnitudes[ArduinoEndAngle / 3, URGEndStep / 3] + " " + calibratedMagnitudes[ArduinoEndAngle / 3, URGEndStep / 2] + " " + calibratedMagnitudes[ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)] + " " + calibratedMagnitudes[ArduinoEndAngle / 3, URGEndStep]);
        Console.WriteLine(calibratedMagnitudes[ArduinoEndAngle / 2, 0] + " " + calibratedMagnitudes[ArduinoEndAngle / 2, URGEndStep / 3] + " " + calibratedMagnitudes[ArduinoEndAngle / 2, URGEndStep / 2] + " " + calibratedMagnitudes[ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)] + " " + calibratedMagnitudes[ArduinoEndAngle / 2, URGEndStep]);
        Console.WriteLine(calibratedMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), 0] + " " + calibratedMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3] + " " + calibratedMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2] + " " + calibratedMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)] + " " + calibratedMagnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep]);
        Console.WriteLine(calibratedMagnitudes[ArduinoEndAngle, 0] + " " + calibratedMagnitudes[ArduinoEndAngle, URGEndStep / 3] + " " + calibratedMagnitudes[ArduinoEndAngle, URGEndStep / 2] + " " + calibratedMagnitudes[ArduinoEndAngle, URGEndStep - (URGEndStep / 3)] + " " + calibratedMagnitudes[ArduinoEndAngle, URGEndStep]);

        return calibratedMagnitudes;
    }

    private static double[,] GetNormalisedMagnitudes(double[,] magnitudes)
    {
        double[,] normalisedMagnitudes = new double[ArduinoEndAngle + 1, URGEndStep + 1];
        double highestMagnitude = magnitudes[0, 0];

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            for (int j = 1; j < URGEndStep + 1; j++)
            {
                if (magnitudes[i, j] > highestMagnitude)
                {
                    highestMagnitude = magnitudes[i, j];
                }
            }
        }

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            for (int j = 0; j < URGEndStep + 1; j++)
            {
                normalisedMagnitudes[i, j] = magnitudes[i, j] / highestMagnitude;
            }
        }

        return normalisedMagnitudes;
    }

    private static double[,] GetMagnitudes()
    {
        double[,] magnitudes = new double[ArduinoEndAngle + 1, URGEndStep + 1];

        EstablishURGConnection(out string ipAddress, out int portNumber);

        TcpClient tcpClient = new TcpClient();
        tcpClient.Connect(ipAddress, portNumber);
        NetworkStream networkStream = tcpClient.GetStream();

        WriteMagnitudes(networkStream, URGLibrary.URGLibrary.SCIP2());
        ReadMagnitudes(networkStream); // ignore echo back

        WriteMagnitudes(networkStream, URGLibrary.URGLibrary.MD(0, URGEndStep));
        ReadMagnitudes(networkStream);  // ignore echo back

        magnitudes = GetLeftWindingMagnitudes(networkStream);

        WriteMagnitudes(networkStream, URGLibrary.URGLibrary.QT());    // stop measurement mode
        ReadMagnitudes(networkStream); // ignore echo back

        networkStream.Close();
        tcpClient.Close();

        // show magnitudes data
        Console.WriteLine();
        Console.WriteLine("Magnitudes Data: ");

        Console.WriteLine(magnitudes[0, 0] + " " + magnitudes[0, URGEndStep / 3] + " " + magnitudes[0, URGEndStep / 2] + " " + magnitudes[0, URGEndStep - (URGEndStep / 3)] + " " + magnitudes[0, URGEndStep]);
        Console.WriteLine(magnitudes[ArduinoEndAngle / 3, 0] + " " + magnitudes[ArduinoEndAngle / 3, URGEndStep / 3] + " " + magnitudes[ArduinoEndAngle / 3, URGEndStep / 2] + " " + magnitudes[ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)] + " " + magnitudes[ArduinoEndAngle / 3, URGEndStep]);
        Console.WriteLine(magnitudes[ArduinoEndAngle / 2, 0] + " " + magnitudes[ArduinoEndAngle / 2, URGEndStep / 3] + " " + magnitudes[ArduinoEndAngle / 2, URGEndStep / 2] + " " + magnitudes[ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)] + " " + magnitudes[ArduinoEndAngle / 2, URGEndStep]);
        Console.WriteLine(magnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), 0] + " " + magnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3] + " " + magnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2] + " " + magnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)] + " " + magnitudes[ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep]);
        Console.WriteLine(magnitudes[ArduinoEndAngle, 0] + " " + magnitudes[ArduinoEndAngle, URGEndStep / 3] + " " + magnitudes[ArduinoEndAngle, URGEndStep / 2] + " " + magnitudes[ArduinoEndAngle, URGEndStep - (URGEndStep / 3)] + " " + magnitudes[ArduinoEndAngle, URGEndStep]);

        return magnitudes;
    }

    /// <summary>
    /// get connection information from user.
    /// </summary>
    private static void EstablishURGConnection(out string ipAdderss, out int portNumber)
    {
        ipAdderss = "192.168.1.11";
        portNumber = 10940;

        Console.WriteLine("Connect setting = IP Address : " + ipAdderss + " Port number : " + portNumber.ToString());
    }

    /// <summary>
    /// Read to "\n\n" from NetworkStream
    /// </summary>
    /// <returns>receive data</returns>
    private static string ReadMagnitudes(NetworkStream networkStream)
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
    /// write data
    /// </summary>
    private static bool WriteMagnitudes(NetworkStream networkStream, string data)
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

    private static double[,] GetLeftWindingMagnitudes(NetworkStream networkStream)
    {
        double[,] leftWindingMagnitudes = new double[ArduinoEndAngle + 1, URGEndStep + 1];

        Console.WriteLine();
        Console.WriteLine("Resetting Servo...");

        ArduinoSerialPort.WriteLine("10");

        Thread.Sleep(1000);

        Console.WriteLine();
        Console.WriteLine("Step Data: ");

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            if (i <= 9)
            {
                ArduinoSerialPort.WriteLine("1" + i);
            }
            else
            {
                if (i <= 99)
                {
                    ArduinoSerialPort.WriteLine("2" + i);
                }
                else
                {
                    ArduinoSerialPort.WriteLine("3" + i);
                }
            }

            Console.WriteLine(i + "/" + ArduinoEndAngle);

            double[,] distances = new double[Scans + 1, URGEndStep + 1];

            for (int j = 0; j < Scans; j++)
            {
                List<long> distance = new List<long>();
                long timeStamp = 0;
                string recieveData = ReadMagnitudes(networkStream);

                if (!URGReader.MD(recieveData, ref timeStamp, ref distance))
                {
                    Console.WriteLine(recieveData);

                    break;
                }

                if (distance.Count == 0)
                {
                    Console.WriteLine(recieveData);

                    continue;
                }

                for (int k = 0; k < URGEndStep + 1; k++)
                {
                    distances[j, k] = distance[k];
                }
            }

            double[] magnitude = new double[URGEndStep + 1];

            for (int j = 0; j < Scans + 1; j++)
            {
                for (int k = 0; k < URGEndStep + 1; k++)
                {
                    magnitude[k] += distances[j, k];
                }
            }

            for (int j = 0; j < URGEndStep + 1; j++)
            {
                magnitude[j] /= Scans;

                leftWindingMagnitudes[i, j] = magnitude[j];
            }
        }

        return GetRightWindingMagnitudes(networkStream, leftWindingMagnitudes);
    }

    private static double[,] GetRightWindingMagnitudes(NetworkStream networkStream, double[,] leftWindingMagnitudes)
    {
        double[,] magnitudes = new double[ArduinoEndAngle + 1, URGEndStep + 1];
        double[,] rightWindingMagnitudes = new double[ArduinoEndAngle + 1, URGEndStep + 1];

        Console.WriteLine();
        Console.WriteLine("Resetting Servo...");

        ArduinoSerialPort.WriteLine("3180");

        Thread.Sleep(1000);

        Console.WriteLine();
        Console.WriteLine("Step Data: ");

        for (int i = ArduinoEndAngle; i > -1; i--)
        {
            if (i <= 9)
            {
                ArduinoSerialPort.WriteLine("1" + i);
            }
            else
            {
                if (i <= 99)
                {
                    ArduinoSerialPort.WriteLine("2" + i);
                }
                else
                {
                    ArduinoSerialPort.WriteLine("3" + i);
                }
            }

            Console.WriteLine(i + "/" + ArduinoEndAngle);

            double[,] distances = new double[Scans + 1, URGEndStep + 1];

            for (int j = 0; j < Scans; j++)
            {
                List<long> distance = new List<long>();
                long timeStamp = 0;
                string recieveData = ReadMagnitudes(networkStream);

                if (!URGReader.MD(recieveData, ref timeStamp, ref distance))
                {
                    Console.WriteLine(recieveData);

                    break;
                }

                if (distance.Count == 0)
                {
                    Console.WriteLine(recieveData);

                    continue;
                }

                for (int k = 0; k < URGEndStep + 1; k++)
                {
                    distances[j, k] = distance[k];
                }
            }

            double[] magnitude = new double[URGEndStep + 1];

            for (int j = 0; j < Scans + 1; j++)
            {
                for (int k = 0; k < URGEndStep + 1; k++)
                {
                    magnitude[k] += distances[j, k];
                }
            }

            for (int j = 0; j < URGEndStep + 1; j++)
            {
                magnitude[j] /= Scans;

                rightWindingMagnitudes[i, j] = magnitude[j];
            }
        }

        for (int i = ArduinoEndAngle; i > -1; i--)
        {
            for (int j = 0; j < URGEndStep + 1; j++)
            {
                magnitudes[i, j] = (leftWindingMagnitudes[i, j] + rightWindingMagnitudes[i, j]) / 2;
            }
        }

        ResetServo();

        return magnitudes;
    }

    private static void ResetServo()
    {
        Console.WriteLine();
        Console.WriteLine("Resetting Servo...");

        ArduinoSerialPort.WriteLine("10");

        Thread.Sleep(1000);
    }

    private static double[,,] GetUnitVectors()
    {
        double[,,] unitVectors = new double[3, ArduinoEndAngle + 1, URGEndStep + 1];

        double urgAngleInAStep = URGRotationAngle / (double)URGEndStep;

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            double arduinoCurrentAngle = i * (Math.PI / 180);

            for (int j = 0; j < URGEndStep + 1; j++)
            {
                double urgCurrentAngle = ((j * urgAngleInAStep) - 45) * (Math.PI / 180);

                unitVectors[0, i, j] = Math.Cos(arduinoCurrentAngle) * Math.Cos(urgCurrentAngle);
                unitVectors[1, i, j] = Math.Sin(urgCurrentAngle);
                unitVectors[2, i, j] = Math.Sin(arduinoCurrentAngle) * Math.Cos(urgCurrentAngle);
            }
        }

        // show unit vector data
        Console.WriteLine();
        Console.WriteLine("Unit Vector Data: ");

        Console.WriteLine(unitVectors[0, 0, 0] + " " + unitVectors[1, 0, 0] + " " + unitVectors[2, 0, 0]);
        Console.WriteLine(unitVectors[0, 0, URGEndStep / 3] + " " + unitVectors[1, 0, URGEndStep / 3] + " " + unitVectors[2, 0, URGEndStep / 3]);
        Console.WriteLine(unitVectors[0, 0, URGEndStep / 2] + " " + unitVectors[1, 0, URGEndStep / 2] + " " + unitVectors[2, 0, URGEndStep / 2]);
        Console.WriteLine(unitVectors[0, 0, URGEndStep - (URGEndStep / 3)] + " " + unitVectors[1, 0, URGEndStep - (URGEndStep / 3)] + " " + unitVectors[2, 0, URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(unitVectors[0, 0, URGEndStep] + " " + unitVectors[1, 0, URGEndStep] + " " + unitVectors[2, 0, URGEndStep]);

        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 3, 0] + " " + unitVectors[1, ArduinoEndAngle / 3, 0] + " " + unitVectors[2, ArduinoEndAngle / 3, 0]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 3, URGEndStep / 3] + " " + unitVectors[1, ArduinoEndAngle / 3, URGEndStep / 3] + " " + unitVectors[2, ArduinoEndAngle / 3, URGEndStep / 3]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 3, URGEndStep / 2] + " " + unitVectors[1, ArduinoEndAngle / 3, URGEndStep / 2] + " " + unitVectors[2, ArduinoEndAngle / 3, URGEndStep / 2]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)] + " " + unitVectors[1, ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)] + " " + unitVectors[2, ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 3, URGEndStep] + " " + unitVectors[1, ArduinoEndAngle / 3, URGEndStep] + " " + unitVectors[2, ArduinoEndAngle / 3, URGEndStep]);

        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 2, 0] + " " + unitVectors[1, ArduinoEndAngle / 2, 0] + " " + unitVectors[2, ArduinoEndAngle / 2, 0]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 2, URGEndStep / 3] + " " + unitVectors[1, ArduinoEndAngle / 2, URGEndStep / 3] + " " + unitVectors[2, ArduinoEndAngle / 2, URGEndStep / 3]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 2, URGEndStep / 2] + " " + unitVectors[1, ArduinoEndAngle / 2, URGEndStep / 2] + " " + unitVectors[2, ArduinoEndAngle / 2, URGEndStep / 2]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)] + " " + unitVectors[1, ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)] + " " + unitVectors[2, ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle / 2, URGEndStep] + " " + unitVectors[1, ArduinoEndAngle / 2, URGEndStep] + " " + unitVectors[2, ArduinoEndAngle / 2, URGEndStep]);

        Console.WriteLine(unitVectors[0, ArduinoEndAngle - (ArduinoEndAngle / 3), 0] + " " + unitVectors[1, ArduinoEndAngle - (ArduinoEndAngle / 3), 0] + " " + unitVectors[2, ArduinoEndAngle - (ArduinoEndAngle / 3), 0]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3] + " " + unitVectors[1, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3] + " " + unitVectors[2, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2] + " " + unitVectors[1, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2] + " " + unitVectors[2, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)] + " " + unitVectors[1, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)] + " " + unitVectors[2, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep] + " " + unitVectors[1, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep] + " " + unitVectors[2, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep]);

        Console.WriteLine(unitVectors[0, ArduinoEndAngle, 0] + " " + unitVectors[1, ArduinoEndAngle, 0] + " " + unitVectors[2, ArduinoEndAngle, 0]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle, URGEndStep / 3] + " " + unitVectors[1, ArduinoEndAngle, URGEndStep / 3] + " " + unitVectors[2, ArduinoEndAngle, URGEndStep / 3]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle, URGEndStep / 2] + " " + unitVectors[1, ArduinoEndAngle, URGEndStep / 2] + " " + unitVectors[2, ArduinoEndAngle, URGEndStep / 2]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle, URGEndStep - (URGEndStep / 3)] + " " + unitVectors[1, ArduinoEndAngle, URGEndStep - (URGEndStep / 3)] + " " + unitVectors[2, ArduinoEndAngle, URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(unitVectors[0, ArduinoEndAngle, URGEndStep] + " " + unitVectors[1, ArduinoEndAngle, URGEndStep] + " " + unitVectors[2, ArduinoEndAngle, URGEndStep]);

        return unitVectors;
    }

    private static double[,,] VectorMultiplexer(double[,,] magnitudeRGB, double[,,] unitVectors)
    {
        double[,,] vectorRGB = new double[4, ArduinoEndAngle + 1, URGEndStep + 1];

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            for (int j = 0; j < URGEndStep + 1; j++)
            {
                vectorRGB[0, i, j] = unitVectors[0, i, j] * magnitudeRGB[0, i, j];
                vectorRGB[1, i, j] = unitVectors[1, i, j] * magnitudeRGB[0, i, j];
                vectorRGB[2, i, j] = unitVectors[2, i, j] * magnitudeRGB[0, i, j];
                vectorRGB[3, i, j] = magnitudeRGB[1, i, j];
            }
        }

        // show vector data
        Console.WriteLine();
        Console.WriteLine("Vector Data: ");

        Console.WriteLine(vectorRGB[0, 0, 0] + " " + vectorRGB[1, 0, 0] + " " + vectorRGB[2, 0, 0]);
        Console.WriteLine(vectorRGB[0, 0, URGEndStep / 3] + " " + vectorRGB[1, 0, URGEndStep / 3] + " " + vectorRGB[2, 0, URGEndStep / 3]);
        Console.WriteLine(vectorRGB[0, 0, URGEndStep / 2] + " " + vectorRGB[1, 0, URGEndStep / 2] + " " + vectorRGB[2, 0, URGEndStep / 2]);
        Console.WriteLine(vectorRGB[0, 0, URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[1, 0, URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[2, 0, URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(vectorRGB[0, 0, URGEndStep] + " " + vectorRGB[1, 0, URGEndStep] + " " + vectorRGB[2, 0, URGEndStep]);

        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 3, 0] + " " + vectorRGB[1, ArduinoEndAngle / 3, 0] + " " + vectorRGB[2, ArduinoEndAngle / 3, 0]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 3, URGEndStep / 3] + " " + vectorRGB[1, ArduinoEndAngle / 3, URGEndStep / 3] + " " + vectorRGB[2, ArduinoEndAngle / 3, URGEndStep / 3]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 3, URGEndStep / 2] + " " + vectorRGB[1, ArduinoEndAngle / 3, URGEndStep / 2] + " " + vectorRGB[2, ArduinoEndAngle / 3, URGEndStep / 2]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[1, ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[2, ArduinoEndAngle / 3, URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 3, URGEndStep] + " " + vectorRGB[1, ArduinoEndAngle / 3, URGEndStep] + " " + vectorRGB[2, ArduinoEndAngle / 3, URGEndStep]);

        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 2, 0] + " " + vectorRGB[1, ArduinoEndAngle / 2, 0] + " " + vectorRGB[2, ArduinoEndAngle / 2, 0]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 2, URGEndStep / 3] + " " + vectorRGB[1, ArduinoEndAngle / 2, URGEndStep / 3] + " " + vectorRGB[2, ArduinoEndAngle / 2, URGEndStep / 3]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 2, URGEndStep / 2] + " " + vectorRGB[1, ArduinoEndAngle / 2, URGEndStep / 2] + " " + vectorRGB[2, ArduinoEndAngle / 2, URGEndStep / 2]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[1, ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[2, ArduinoEndAngle / 2, URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle / 2, URGEndStep] + " " + vectorRGB[1, ArduinoEndAngle / 2, URGEndStep] + " " + vectorRGB[2, ArduinoEndAngle / 2, URGEndStep]);

        Console.WriteLine(vectorRGB[0, ArduinoEndAngle - (ArduinoEndAngle / 3), 0] + " " + vectorRGB[1, ArduinoEndAngle - (ArduinoEndAngle / 3), 0] + " " + vectorRGB[2, ArduinoEndAngle - (ArduinoEndAngle / 3), 0]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3] + " " + vectorRGB[1, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3] + " " + vectorRGB[2, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 3]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2] + " " + vectorRGB[1, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2] + " " + vectorRGB[2, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep / 2]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[1, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[2, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep] + " " + vectorRGB[1, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep] + " " + vectorRGB[2, ArduinoEndAngle - (ArduinoEndAngle / 3), URGEndStep]);

        Console.WriteLine(vectorRGB[0, ArduinoEndAngle, 0] + " " + vectorRGB[1, ArduinoEndAngle, 0] + " " + vectorRGB[2, ArduinoEndAngle, 0]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle, URGEndStep / 3] + " " + vectorRGB[1, ArduinoEndAngle, URGEndStep / 3] + " " + vectorRGB[2, ArduinoEndAngle, URGEndStep / 3]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle, URGEndStep / 2] + " " + vectorRGB[1, ArduinoEndAngle, URGEndStep / 2] + " " + vectorRGB[2, ArduinoEndAngle, URGEndStep / 2]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle, URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[1, ArduinoEndAngle, URGEndStep - (URGEndStep / 3)] + " " + vectorRGB[2, ArduinoEndAngle, URGEndStep - (URGEndStep / 3)]);
        Console.WriteLine(vectorRGB[0, ArduinoEndAngle, URGEndStep] + " " + vectorRGB[1, ArduinoEndAngle, URGEndStep] + " " + vectorRGB[2, ArduinoEndAngle, URGEndStep]);

        return vectorRGB;
    }

    private static void WriteToFile(double[,,] vectorRGB)
    {
        Console.WriteLine();
        Console.WriteLine("Writing data to file...");

        DirectoryInfo directoryInfo = Directory.CreateDirectory(Directory.GetCurrentDirectory().ToString() + "\\Scan_" + DateTime.Now.Ticks.ToString());

        WriteToPCDFile(vectorRGB, directoryInfo);
        WriteToToMeshFile(vectorRGB, directoryInfo);
    }

    private static void WriteToPCDFile(double[,,] vectorRGB, DirectoryInfo directoryInfo)
    {
        int length = URGEndStep * ArduinoEndAngle;

        using (StreamWriter streamWriter = File.CreateText(directoryInfo  + "\\PDC.txt"))
        {
            streamWriter.WriteLine("# .PCD v.7 - Point Cloud Data file format");
            streamWriter.WriteLine("VERSION .7");
            streamWriter.WriteLine("FIELDS x y z rgb");
            streamWriter.WriteLine("SIZE 8 8 8 8");
            streamWriter.WriteLine("TYPE F F F F");
            streamWriter.WriteLine("COUNT 1 1 1 1");
            streamWriter.WriteLine("WIDTH " + length);
            streamWriter.WriteLine("HEIGHT 1");
            streamWriter.WriteLine("VIEWPOINT 0 0 0 1 0 0 0");
            streamWriter.WriteLine("POINTS " + length);
            streamWriter.WriteLine("DATA ascii");

            for (int i = 0; i < ArduinoEndAngle + 1; i++)
            {
                for (int j = 0; j < URGEndStep + 1; j++)
                {
                    streamWriter.WriteLine(vectorRGB[0, i, j] + " " + vectorRGB[1, i, j] + " " + vectorRGB[2, i, j] + " " + vectorRGB[3, i, j]);
                }
            }
        }
    }

    private static void WriteToToMeshFile(double[,,] vectors, DirectoryInfo directoryInfo)
    {
        using (StreamWriter streamWriter = File.CreateText(directoryInfo + "\\To_Mesh.txt"))
        {
            for (int i = 0; i < ArduinoEndAngle + 1; i++)
            {
                for (int j = 0; j < URGEndStep + 1; j++)
                {
                    streamWriter.WriteLine(vectors[0, i, j] + ";" + vectors[1, i, j] + ";" + vectors[2, i, j]);
                }
            }

            FixMesh(streamWriter, vectors);
        }
    }

    private static void FixMesh(StreamWriter streamWriter, double[,,] vectors)
    {
        AddBaseMash(streamWriter, vectors);
        InterpolateMesh(streamWriter, vectors);
    }

    private static void AddBaseMash(StreamWriter streamWriter, double[,,] vectors)
    {
        double interpolationFactor = (URGEndStep / URGRotationAngle) * (360 - URGRotationAngle);

        double lowestPoint = vectors[1, 0, 0];

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            if (vectors[1, i, 0] < lowestPoint)
            {
                lowestPoint = vectors[1, i, 0];
            }
            else
            {
                if (vectors[1, i, URGEndStep] < lowestPoint)
                {
                    lowestPoint = vectors[1, i, URGEndStep];
                }
            }
        }

        for (int i = 0; i < ArduinoEndAngle + 1; i++)
        {
            double firstXOffset = (vectors[0, i, 0] - vectors[0, i, URGEndStep]) / interpolationFactor;
            double firstYOffset = (vectors[1, i, 0] - lowestPoint) / (interpolationFactor / 4);
            double firstZOffset = (vectors[2, i, 0] - vectors[2, i, URGEndStep]) / interpolationFactor;
            double lastXOffset = (vectors[0, i, URGEndStep] - vectors[0, i, 0]) / interpolationFactor;
            double lastYOffset = (vectors[1, i, URGEndStep] - lowestPoint) / (interpolationFactor / 4);
            double lastZOffset = (vectors[2, i, URGEndStep] - vectors[2, i, 0]) / interpolationFactor;

            for (int j = 0; j < (interpolationFactor / 4) + 1; j++)
            {
                double firstX = vectors[0, i, 0] - (firstXOffset * j);
                double firstY = vectors[1, i, 0] - (firstYOffset * j);
                double firstZ = vectors[2, i, 0] - (firstZOffset * j);
                double lastX = vectors[0, i, URGEndStep] - (lastXOffset * j);
                double lastY = vectors[1, i, URGEndStep] - (lastYOffset * j);
                double lastZ = vectors[2, i, URGEndStep] - (lastZOffset * j);

                streamWriter.WriteLine(firstX + ";" + firstY + ";" + firstZ);
                streamWriter.WriteLine(lastX + ";" + lastY + ";" + lastZ);
            }

            for (int j = (int)(interpolationFactor / 4); j < (interpolationFactor / 2) + 1; j++)
            {
                double firstX = vectors[0, i, 0] - (firstXOffset * j);
                double firstZ = vectors[2, i, 0] - (firstZOffset * j);
                double lastX = vectors[0, i, URGEndStep] - (lastXOffset * j);
                double lastZ = vectors[2, i, URGEndStep] - (lastZOffset * j);

                streamWriter.WriteLine(firstX + ";" + lowestPoint + ";" + firstZ);
                streamWriter.WriteLine(lastX + ";" + lowestPoint + ";" + lastZ);
            }
        }
    }

    private static void InterpolateMesh(StreamWriter streamWriter, double[,,] vectors)
    {
        double interpolationFactor = ((URGEndStep / URGRotationAngle) * 360) / ArduinoEndAngle;

        for (int i = 1; i < ArduinoEndAngle + 1; i++)
        {
            for (int j = 0; j < URGEndStep + 1; j++)
            {
                double xOffset = (vectors[0, i - 1, j] - vectors[0, i, j]) / interpolationFactor;
                double yOffset = (vectors[1, i - 1, j] - vectors[1, i, j]) / interpolationFactor;
                double zOffset = (vectors[2, i - 1, j] - vectors[2, i, j]) / interpolationFactor;

                for (int k = 0; k < interpolationFactor + 1; k++)
                {
                    double x = vectors[0, i - 1, j] - (xOffset * k);
                    double y = vectors[1, i - 1, j] - (yOffset * k);
                    double z = vectors[2, i - 1, j] - (zOffset * k);

                    streamWriter.WriteLine(x + ";" + y + ";" + z);
                }
            }
        }
    }
}