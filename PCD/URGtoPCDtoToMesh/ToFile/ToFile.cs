using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading;

namespace ToFile
{
    class ToFile
    {
        const int ArduinoEndAngle = 180;

        const int Scans = 4;

        const int URGRotationAngle = 270;
        const int URGEndStep = 1080;

        const int Delay = 100;
        const int Second = 1000;

        static string SerialPortName = string.Empty;
        static int BaudRate = 9600;

        static string IPAddress = string.Empty;
        static int PortNumber = 0;

        static void Main(string[] args)
        {
            try
            {
                Setup();

                Loop();
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

        private static void Setup()
        {
            Initialise();

            EstablishSerialConnection();

            URGLibrary.URGLibrary.EstablishURGConnection(out IPAddress, out PortNumber);
        }

        private static void Initialise()
        {
            Console.WriteLine("Initialising...");

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine((i + 1) + "/3");

                Thread.Sleep(Second);
            }
        }

        private static void EstablishSerialConnection()
        {
            while (!FindArduinoSerialPort())
            {
                Thread.Sleep(Delay);
            }

            Thread.Sleep(Delay);

            Console.WriteLine();
            Console.WriteLine("Connect setting = Serial Port Name : " + SerialPortName + " Baud Number : " + BaudRate.ToString());
        }

        private static bool FindArduinoSerialPort()
        {
            string[] serialPortNames = SerialPort.GetPortNames();

            for (int i = 0; i < serialPortNames.Length; i++) 
            {
                try
                {
                    using (SerialPort serialPort = new SerialPort(serialPortNames[i], BaudRate))
                    {
                        EstablishHardSerialPort(serialPort);

                        if (ToReadEndFromArduino(serialPort))
                        {
                            SerialPortName = serialPortNames[i];

                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }

        private static void EstablishHardSerialPort(SerialPort serialPort)
        {
            serialPort.DtrEnable = false;

            serialPort.Open();

            while (!serialPort.IsOpen)
            {
                Thread.Sleep(Delay);
            }

            Thread.Sleep(Delay);

            ClearSerialPortBuffer(serialPort);

            Thread.Sleep(Delay);
        }

        private static void ClearSerialPortBuffer(SerialPort serialPort)
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }

        private static bool ToReadEndFromArduino(SerialPort serialPort)
        {
            serialPort.Write("E");

            Thread.Sleep(Delay);

            return ReadEndFromArduino(serialPort);
        }

        private static bool ReadEndFromArduino(SerialPort serialPort)
        {
            try
            {
                serialPort.ReadTimeout = 100;

                char[] serialPortCharArray = serialPort.ReadExisting().ToCharArray();

                for (int i = 0; i < serialPortCharArray.Length; i++)
                {
                    if (serialPortCharArray[i] == 'E')
                    {
                        return true;
                    }
                    else
                    {
                        continue;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static void Loop()
        {
            while (true)
            {
                Console.WriteLine();
                Console.Write("Press button to start scan... ");

                while (!ReadStartFromArduino())
                {
                    Thread.Sleep(Delay);
                }

                Thread.Sleep(Delay);

                Console.WriteLine("Done!");

                Console.WriteLine();
                Console.WriteLine("Starting scan...");

                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine((i + 1) + "/10");

                    Thread.Sleep(Second);
                }

                WriteToFile(VectorMultiplexer(RGBMultiplexer(GetCulledMagnitudes(GetCalibratedMagnitudes(GetMagnitudes()))), GetUnitVectors()));
            }
        }

        private static bool ReadStartFromArduino()
        {
            using (SerialPort serialPort = new SerialPort(SerialPortName, BaudRate))
            {
                EstablishSoftSerialPort(serialPort);

                try
                {
                    serialPort.ReadTimeout = 100;

                    char[] serialPortCharArray = serialPort.ReadExisting().ToCharArray();

                    for (int i = 0; i < serialPortCharArray.Length; i++)
                    {
                        if (serialPortCharArray[i] == 'S')
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        private static void EstablishSoftSerialPort(SerialPort serialPort)
        {
            serialPort.DtrEnable = false;

            serialPort.Open();

            while (!serialPort.IsOpen)
            {
                Thread.Sleep(Delay);
            }

            Thread.Sleep(Delay);
        }

        private static void WriteToFile(double[,,] vectorsRGB)
        {
            Console.WriteLine();
            Console.Write("Writing data to file... ");

            DirectoryInfo directoryInfo = Directory.CreateDirectory(Directory.GetCurrentDirectory().ToString() + Path.DirectorySeparatorChar + "Scans" +  Path.DirectorySeparatorChar + "Scan_" + DateTime.Now.Ticks.ToString());

            WriteToPCDFile(vectorsRGB, directoryInfo);
            WriteToToMeshFile(vectorsRGB, directoryInfo);
            ToMesh(directoryInfo);

            Console.WriteLine("Done!");
        }

        private static void WriteToPCDFile(double[,,] vectorsRGB, DirectoryInfo directoryInfo)
        {
            int length = vectorsRGB.GetLength(vectorsRGB.Rank - 1) * vectorsRGB.GetLength(vectorsRGB.Rank - 2);

            using (StreamWriter streamWriter = File.CreateText(directoryInfo.FullName.ToString() + Path.DirectorySeparatorChar + "PDC.txt"))
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

                for (int i = 0; i < vectorsRGB.GetLength(vectorsRGB.Rank - 2); i++)
                {
                    for (int j = 0; j < vectorsRGB.GetLength(vectorsRGB.Rank - 1); j++)
                    {
                        streamWriter.WriteLine(vectorsRGB[0, i, j] + " " + vectorsRGB[1, i, j] + " " + vectorsRGB[2, i, j] + " " + vectorsRGB[3, i, j]);
                    }
                }
            }
        }

        private static void WriteToToMeshFile(double[,,] vectors, DirectoryInfo directoryInfo)
        {
            using (StreamWriter streamWriter = File.CreateText(directoryInfo.FullName.ToString() + Path.DirectorySeparatorChar + "To_Mesh.txt"))
            {
                for (int i = 0; i < vectors.GetLength(vectors.Rank - 2); i++)
                {
                    for (int j = 0; j < vectors.GetLength(vectors.Rank - 1); j++)
                    {
                        streamWriter.WriteLine(vectors[0, i, j] + ";" + vectors[1, i, j] + ";" + vectors[2, i, j]);
                    }
                }

                FixPoints(streamWriter, vectors);
            }
        }

        private static void FixPoints(StreamWriter streamWriter, double[,,] vectors)
        {
            AddBasePoints(streamWriter, vectors);
            InterpolatePoints(streamWriter, vectors);
        }

        private static void AddBasePoints(StreamWriter streamWriter, double[,,] vectors)
        {
            double interpolationFactor = (vectors.GetLength(vectors.Rank - 1) / URGRotationAngle) * (360 - URGRotationAngle);

            double lowestPoint = vectors[1, 0, 0];

            for (int i = 0; i < vectors.GetLength(vectors.Rank - 2); i++)
            {
                if (vectors[1, i, 0] < lowestPoint)
                {
                    lowestPoint = vectors[1, i, 0];
                }

                if (vectors[1, i, vectors.GetLength(vectors.Rank - 1) - 1] < lowestPoint)
                {
                    lowestPoint = vectors[1, i, vectors.GetLength(vectors.Rank - 1) - 1];
                }
            }

            for (int i = 0; i < vectors.GetLength(vectors.Rank - 2); i++)
            {
                double firstXOffset = (vectors[0, i, 0] - vectors[0, i, vectors.GetLength(vectors.Rank - 1) - 1]) / interpolationFactor;
                double firstYOffset = (vectors[1, i, 0] - lowestPoint) / (interpolationFactor / 4);
                double firstZOffset = (vectors[2, i, 0] - vectors[2, i, vectors.GetLength(vectors.Rank - 1) - 1]) / interpolationFactor;
                double lastXOffset = (vectors[0, i, vectors.GetLength(vectors.Rank - 1) - 1] - vectors[0, i, 0]) / interpolationFactor;
                double lastYOffset = (vectors[1, i, vectors.GetLength(vectors.Rank - 1) - 1] - lowestPoint) / (interpolationFactor / 4);
                double lastZOffset = (vectors[2, i, vectors.GetLength(vectors.Rank - 1) - 1] - vectors[2, i, 0]) / interpolationFactor;

                for (int j = 0; j < (interpolationFactor / 4); j++)
                {
                    double firstX = vectors[0, i, 0] - (firstXOffset * j);
                    double firstY = vectors[1, i, 0] - (firstYOffset * j);
                    double firstZ = vectors[2, i, 0] - (firstZOffset * j);
                    double lastX = vectors[0, i, vectors.GetLength(vectors.Rank - 1) - 1] - (lastXOffset * j);
                    double lastY = vectors[1, i, vectors.GetLength(vectors.Rank - 1) - 1] - (lastYOffset * j);
                    double lastZ = vectors[2, i, vectors.GetLength(vectors.Rank - 1) - 1] - (lastZOffset * j);

                    streamWriter.WriteLine(firstX + ";" + firstY + ";" + firstZ);
                    streamWriter.WriteLine(lastX + ";" + lastY + ";" + lastZ);
                }

                for (int j = (int)(interpolationFactor / 4); j < (interpolationFactor / 2); j++)
                {
                    double firstX = vectors[0, i, 0] - (firstXOffset * j);
                    double firstZ = vectors[2, i, 0] - (firstZOffset * j);
                    double lastX = vectors[0, i, vectors.GetLength(vectors.Rank - 1) - 1] - (lastXOffset * j);
                    double lastZ = vectors[2, i, vectors.GetLength(vectors.Rank - 1) - 1] - (lastZOffset * j);

                    streamWriter.WriteLine(firstX + ";" + lowestPoint + ";" + firstZ);
                    streamWriter.WriteLine(lastX + ";" + lowestPoint + ";" + lastZ);
                }
            }
        }

        private static void InterpolatePoints(StreamWriter streamWriter, double[,,] vectors)
        {
            double interpolationFactor = ((vectors.GetLength(vectors.Rank - 1) / URGRotationAngle) * 360) / vectors.GetLength(vectors.Rank - 2);

            for (int i = 1; i < vectors.GetLength(vectors.Rank - 2); i++)
            {
                for (int j = 0; j < vectors.GetLength(vectors.Rank - 1); j++)
                {
                    double xOffset = (vectors[0, i - 1, j] - vectors[0, i, j]) / interpolationFactor;
                    double yOffset = (vectors[1, i - 1, j] - vectors[1, i, j]) / interpolationFactor;
                    double zOffset = (vectors[2, i - 1, j] - vectors[2, i, j]) / interpolationFactor;

                    for (int k = 0; k < interpolationFactor; k++)
                    {
                        double x = vectors[0, i - 1, j] - (xOffset * k);
                        double y = vectors[1, i - 1, j] - (yOffset * k);
                        double z = vectors[2, i - 1, j] - (zOffset * k);

                        streamWriter.WriteLine(x + ";" + y + ";" + z);
                    }
                }
            }
        }

        private static void ToMesh(DirectoryInfo directoryInfo)
        {
            switch(Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:

                    LinuxToMesh(directoryInfo);

                    break;

                default:

                    WindowsToMesh(directoryInfo);

                    break;
            }
        }

        private static void LinuxToMesh(DirectoryInfo directoryInfo)
        {
            throw new NotImplementedException();
        }

        private static void WindowsToMesh(DirectoryInfo directoryInfo)
        {
            Process.Start(Directory.GetCurrentDirectory().ToString() + Path.DirectorySeparatorChar + "MeshLab" + Path.DirectorySeparatorChar + "meshlabserver.exe", "-i " + directoryInfo.FullName.ToString() + Path.DirectorySeparatorChar + "To_Mesh.txt" + " -o " + directoryInfo.FullName.ToString() + Path.DirectorySeparatorChar + "Mesh.obj" + " -m vc vn -s MeshLabScript");
        }

        private static double[,,] VectorMultiplexer(double[,,] magnitudesRGB, double[,,] unitVectors)
        {
            double[,,] vectorsRGB = new double[4, unitVectors.GetLength(unitVectors.Rank - 2), unitVectors.GetLength(unitVectors.Rank - 1)];

            for (int i = 0; i < unitVectors.GetLength(unitVectors.Rank - 2); i++)
            {
                for (int j = 0; j < unitVectors.GetLength(unitVectors.Rank - 1); j++)
                {
                    vectorsRGB[0, i, j] = unitVectors[0, i, j] * magnitudesRGB[0, i, j];
                    vectorsRGB[1, i, j] = unitVectors[1, i, j] * magnitudesRGB[0, i, j];
                    vectorsRGB[2, i, j] = unitVectors[2, i, j] * magnitudesRGB[0, i, j];
                    vectorsRGB[3, i, j] = magnitudesRGB[1, i, j];
                }
            }

            // show vector data
            Console.WriteLine();
            Console.WriteLine("Vector Data: ");

            Console.WriteLine(vectorsRGB[0, 0, 0] + " " + vectorsRGB[1, 0, 0] + " " + vectorsRGB[2, 0, 0]);
            Console.WriteLine(vectorsRGB[0, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[1, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[2, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3]);
            Console.WriteLine(vectorsRGB[0, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[1, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[2, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2]);
            Console.WriteLine(vectorsRGB[0, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[1, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[2, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)]);
            Console.WriteLine(vectorsRGB[0, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[1, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[2, 0, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1]);

            Console.WriteLine();

            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 3, 0] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 3, 0] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 3, 0]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 3, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1]);

            Console.WriteLine();

            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 2, 0] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 2, 0] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 2, 0]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) / 2, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1]);

            Console.WriteLine();

            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), 0] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), 0] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), 0]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - (unitVectors.GetLength(unitVectors.Rank - 2) / 3), vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1]);

            Console.WriteLine();

            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - 1, 0] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - 1, 0] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - 1, 0]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 2]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - (vectorsRGB.GetLength(vectorsRGB.Rank - 1) / 3)]);
            Console.WriteLine(vectorsRGB[0, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[1, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1] + " " + vectorsRGB[2, unitVectors.GetLength(unitVectors.Rank - 2) - 1, vectorsRGB.GetLength(vectorsRGB.Rank - 1) - 1]);

            return vectorsRGB;
        }

        public static double[,,] RGBMultiplexer(double[,] magnitudes)
        {
            double[,,] magnitudesRGB = new double[2, magnitudes.GetLength(magnitudes.Rank - 2), magnitudes.GetLength(magnitudes.Rank - 1)];

            double highestMagnitude = magnitudes[0, 0];

            for (int i = 0; i < magnitudes.GetLength(magnitudes.Rank - 2); i++)
            {
                for (int j = 0; j < magnitudes.GetLength(magnitudes.Rank - 1); j++)
                {
                    if (magnitudes[i, j] > highestMagnitude)
                    {
                        highestMagnitude = magnitudes[i, j];
                    }
                }
            }

            for (int i = 0; i < magnitudes.GetLength(magnitudes.Rank - 2); i++)
            {
                for (int j = 0; j < magnitudes.GetLength(magnitudes.Rank - 1); j++)
                {
                    byte r = 0;
                    byte g = 0;
                    byte b = 0;

                    double lengthPercentage = (magnitudes[i, j] / highestMagnitude) * 100;

                    if (lengthPercentage <= 50)
                    {
                        b = (byte)((255 / 100) * lengthPercentage);
                        g = (byte)(255 - b);
                    }
                    else
                    {
                        g = (byte)((255 / 100) * lengthPercentage);
                        r = (byte)(255 - b);
                    }

                    magnitudesRGB[0, i, j] = magnitudes[i, j];
                    magnitudesRGB[1, i, j] = r << 16 | g << 8 | b;
                }
            }

            // show rgb data
            Console.WriteLine();
            Console.WriteLine("RGB Data: ");

            Console.WriteLine(magnitudesRGB[1, 0, 0] + " " + magnitudesRGB[1, 0, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3] + " " + magnitudesRGB[1, 0, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 2] + " " + magnitudesRGB[1, 0, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3)] + " " + magnitudesRGB[1, 0, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - 1]);
            Console.WriteLine(magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3, 0] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 2] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3)] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - 1]);
            Console.WriteLine(magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 2, 0] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 2, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 2, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 2] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 2, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3)] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 2, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - 1]);
            Console.WriteLine(magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3), 0] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3), magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3), magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 2] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3), magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3)] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) / 3), magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - 1]);
            Console.WriteLine(magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - 1, 0] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - 1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - 1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 2] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - 1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - (magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) / 3)] + " " + magnitudesRGB[1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 2) - 1, magnitudesRGB.GetLength(magnitudesRGB.Rank - 1) - 1]);

            return magnitudesRGB;
        }

        private static double[,] GetCulledMagnitudes(double[,] calibratedMagnitudes)
        {
            double[,] culledMagnitudes = new double[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2), calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1)];
            double averageMagnitude = 0;

            for (int i = 0; i < calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2); i++)
            {
                for (int j = 0; j < calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1); j++)
                {
                    averageMagnitude += calibratedMagnitudes[i, j];
                }
            }

            averageMagnitude /= (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) * calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1));

            double maximumMagnitude = averageMagnitude * 2;

            for (int i = 0; i < calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2); i++)
            {
                for (int j = 0; j < calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1); j++)
                {
                    if (calibratedMagnitudes[i, j] <= maximumMagnitude)
                    {
                        culledMagnitudes[i, j] = calibratedMagnitudes[i, j];
                    }
                    else
                    {
                        culledMagnitudes[i, j] = 0;
                    }
                }
            }

            // show average magnitude data
            Console.WriteLine();
            Console.WriteLine("Average Magnitude: " + averageMagnitude);

            return culledMagnitudes;
        }

        private static double[,] GetCalibratedMagnitudes(double[,] magnitudes)
        {
            double[,] calibratedMagnitudes = new double[magnitudes.GetLength(magnitudes.Rank - 2), magnitudes.GetLength(magnitudes.Rank - 1)];

            for (int i = 0; i < magnitudes.GetLength(magnitudes.Rank - 2); i++)
            {
                for (int j = 0; j < magnitudes.GetLength(magnitudes.Rank - 1); j++)
                {
                    calibratedMagnitudes[i, j] = magnitudes[i, j] * (100 / 96.8753378292046);
                }
            }

            // show calibrated magnitudes data
            Console.WriteLine();
            Console.WriteLine("Calibrated Magnitudes Data: ");

            Console.WriteLine(calibratedMagnitudes[0, 0] + " " + calibratedMagnitudes[0, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3] + " " + calibratedMagnitudes[0, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 2] + " " + calibratedMagnitudes[0, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3)] + " " + calibratedMagnitudes[0, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - 1]);
            Console.WriteLine(calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3, 0] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 2] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3)] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - 1]);
            Console.WriteLine(calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 2, 0] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 2, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 2, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 2] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 2, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3)] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 2, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - 1]);
            Console.WriteLine(calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3), 0] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3), calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3), calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 2] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3), calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3)] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) / 3), calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - 1]);
            Console.WriteLine(calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - 1, 0] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - 1, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - 1, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 2] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - 1, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - (calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) / 3)] + " " + calibratedMagnitudes[calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 2) - 1, calibratedMagnitudes.GetLength(calibratedMagnitudes.Rank - 1) - 1]);

            return calibratedMagnitudes;
        }

        private static double[,] GetMagnitudes()
        {
            double[,] magnitudes = new double[ArduinoEndAngle, URGEndStep];

            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.Connect(IPAddress, PortNumber);

                using (NetworkStream networkStream = tcpClient.GetStream())
                {
                    // ignore echo back
                    URGLibrary.URGLibrary.WriteMagnitudes(networkStream, URGLibrary.URGLibrary.SCIP2());
                    URGLibrary.URGLibrary.ReadMagnitudes(networkStream);

                    // ignore echo back
                    URGLibrary.URGLibrary.WriteMagnitudes(networkStream, URGLibrary.URGLibrary.MD(0, magnitudes.GetLength(magnitudes.Rank - 1)));
                    URGLibrary.URGLibrary.ReadMagnitudes(networkStream);

                    magnitudes = GetLeftWindingMagnitudes(networkStream);

                    // stop measurement mode
                    // ignore echo back
                    URGLibrary.URGLibrary.WriteMagnitudes(networkStream, URGLibrary.URGLibrary.QT());
                    URGLibrary.URGLibrary.ReadMagnitudes(networkStream);
                }
            }

            // show magnitudes data
            Console.WriteLine();
            Console.WriteLine("Magnitudes Data: ");

            Console.WriteLine(magnitudes[0, 0] + " " + magnitudes[0, magnitudes.GetLength(magnitudes.Rank - 1) / 3] + " " + magnitudes[0, magnitudes.GetLength(magnitudes.Rank - 1) / 2] + " " + magnitudes[0, magnitudes.GetLength(magnitudes.Rank - 1) - (magnitudes.GetLength(magnitudes.Rank - 1) / 3)] + " " + magnitudes[0, magnitudes.GetLength(magnitudes.Rank - 1) - 1]);
            Console.WriteLine(magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 3, 0] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 3, magnitudes.GetLength(magnitudes.Rank - 1) / 3] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 3, magnitudes.GetLength(magnitudes.Rank - 1) / 2] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 3, magnitudes.GetLength(magnitudes.Rank - 1) - (magnitudes.GetLength(magnitudes.Rank - 1) / 3)] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 3, magnitudes.GetLength(magnitudes.Rank - 1) - 1]);
            Console.WriteLine(magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 2, 0] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 2, magnitudes.GetLength(magnitudes.Rank - 1) / 3] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 2, magnitudes.GetLength(magnitudes.Rank - 1) / 2] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 2, magnitudes.GetLength(magnitudes.Rank - 1) - (magnitudes.GetLength(magnitudes.Rank - 1) / 3)] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) / 2, magnitudes.GetLength(magnitudes.Rank - 1) - 1]);
            Console.WriteLine(magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - (magnitudes.GetLength(magnitudes.Rank - 2) / 3), 0] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - (magnitudes.GetLength(magnitudes.Rank - 2) / 3), magnitudes.GetLength(magnitudes.Rank - 1) / 3] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - (magnitudes.GetLength(magnitudes.Rank - 2) / 3), magnitudes.GetLength(magnitudes.Rank - 1) / 2] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - (magnitudes.GetLength(magnitudes.Rank - 2) / 3), magnitudes.GetLength(magnitudes.Rank - 1) - (magnitudes.GetLength(magnitudes.Rank - 1) / 3)] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - (magnitudes.GetLength(magnitudes.Rank - 2) / 3), magnitudes.GetLength(magnitudes.Rank - 1) - 1]);
            Console.WriteLine(magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - 1, 0] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - 1, magnitudes.GetLength(magnitudes.Rank - 1) / 3] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - 1, magnitudes.GetLength(magnitudes.Rank - 1) / 2] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - 1, magnitudes.GetLength(magnitudes.Rank - 1) - (magnitudes.GetLength(magnitudes.Rank - 1) / 3)] + " " + magnitudes[magnitudes.GetLength(magnitudes.Rank - 2) - 1, magnitudes.GetLength(magnitudes.Rank - 1) - 1]);

            return magnitudes;
        }

        private static double[,] GetLeftWindingMagnitudes(NetworkStream networkStream)
        {
            double[,] leftWindingMagnitudes = new double[ArduinoEndAngle, URGEndStep];

            Console.WriteLine();
            Console.WriteLine("Step Data: ");

            for (int i = 0; i < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2); i++)
            {
                MoveServo(i);

                Console.WriteLine(i + "/" + leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2));

                double[,] distances = new double[Scans, leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1)];

                for (int j = 0; j < Scans; j++)
                {
                    List<long> distance = new List<long>();

                    long timeStamp = 0;
                    string recieveData = URGLibrary.URGLibrary.ReadMagnitudes(networkStream);

                    if (!URGLibrary.URGLibrary.MD(recieveData, ref timeStamp, ref distance))
                    {
                        Console.WriteLine(recieveData);

                        break;
                    }

                    if (distance.Count == 0)
                    {
                        Console.WriteLine(recieveData);

                        continue;
                    }

                    for (int k = 0; k < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); k++)
                    {
                        distances[j, k] = distance[k];
                    }
                }

                double[] magnitudes = new double[leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1)];

                for (int j = 0; j < Scans; j++)
                {
                    for (int k = 0; k < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); k++)
                    {
                        magnitudes[k] += distances[j, k];
                    }
                }

                for (int j = 0; j < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); j++)
                {
                    magnitudes[j] /= Scans;

                    leftWindingMagnitudes[i, j] = magnitudes[j];
                }
            }

            return GetRightWindingMagnitudes(networkStream, leftWindingMagnitudes);
        }

        private static void MoveServo(int angle)
        {
            using (SerialPort serialPort = new SerialPort(SerialPortName, BaudRate))
            {
                EstablishHardSerialPort(serialPort);

                if (angle <= 9)
                {
                    serialPort.WriteLine("A1" + angle);
                }
                else
                {
                    if (angle <= 99)
                    {
                        serialPort.WriteLine("A2" + angle);
                    }
                    else
                    {
                        serialPort.WriteLine("A3" + angle);
                    }
                }

                Thread.Sleep(Delay);

                ReadEndFromArduino(serialPort);
            }
        }

        private static double[,] GetRightWindingMagnitudes(NetworkStream networkStream, double[,] leftWindingMagnitudes)
        {
            double[,] averageMagnitudes = new double[leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2), leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1)];
            double[,] rightWindingMagnitudes = new double[leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2), leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1)];

            for (int i = 0; i < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2); i++)
            {
                MoveServo(leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2) - i);

                Console.WriteLine((leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2) - i) + "/" + leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2));

                double[,] distances = new double[Scans, leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1)];

                for (int j = 0; j < Scans; j++)
                {
                    List<long> distance = new List<long>();

                    long timeStamp = 0;
                    string recieveData = URGLibrary.URGLibrary.ReadMagnitudes(networkStream);

                    if (!URGLibrary.URGLibrary.MD(recieveData, ref timeStamp, ref distance))
                    {
                        Console.WriteLine(recieveData);

                        break;
                    }

                    if (distance.Count == 0)
                    {
                        Console.WriteLine(recieveData);

                        continue;
                    }

                    for (int k = 0; k < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); k++)
                    {
                        distances[j, k] = distance[k];
                    }
                }

                double[] magnitudes = new double[leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1)];

                for (int j = 0; j < Scans; j++)
                {
                    for (int k = 0; k < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); k++)
                    {
                        magnitudes[k] += distances[j, k];
                    }
                }

                for (int j = 0; j < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); j++)
                {
                    magnitudes[j] /= Scans;

                    rightWindingMagnitudes[i, j] = magnitudes[j];
                }
            }

            rightWindingMagnitudes = ReverseMagnitudes(rightWindingMagnitudes);

            for (int i = 0; i < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); i++)
            {
                averageMagnitudes[0, i] = leftWindingMagnitudes[0, i];
            }

            for (int i = 1; i < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2) - 1; i++)
            {
                for (int j = 0; j < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); j++)
                {
                    averageMagnitudes[i, j] = (leftWindingMagnitudes[i, j] + rightWindingMagnitudes[i, j]) / 2;
                }
            }

            for (int i = 0; i < leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 1); i++)
            {
                averageMagnitudes[leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2) - 1, i] = rightWindingMagnitudes[leftWindingMagnitudes.GetLength(leftWindingMagnitudes.Rank - 2) - 1, i];
            }

            return averageMagnitudes;
        }

        private static double[,] ReverseMagnitudes(double[,] magnitudes)
        {
            double[,] reversedMagnitudes = new double[magnitudes.GetLength(magnitudes.Rank - 2), magnitudes.GetLength(magnitudes.Rank - 1)];

            for(int i = 0; i < magnitudes.GetLength(magnitudes.Rank - 2); i++)
            {
                for (int j = 0; j < magnitudes.GetLength(magnitudes.Rank - 1); j++)
                {
                    reversedMagnitudes[i, j] = magnitudes[i, (magnitudes.GetLength(magnitudes.Rank - 1) - 1) - j];
                }
            }

            return reversedMagnitudes;
        }

        private static double[,,] GetUnitVectors()
        {
            double[,,] unitVectors = new double[3, ArduinoEndAngle, URGEndStep];

            double urgAngleInAStep = URGRotationAngle / (double)unitVectors.GetLength(unitVectors.Rank - 1);

            for (int i = 0; i < unitVectors.GetLength(unitVectors.Rank - 2); i++)
            {
                double arduinoCurrentAngle = i * (Math.PI / 180);

                for (int j = 0; j < unitVectors.GetLength(unitVectors.Rank - 1); j++)
                {
                    double urgCurrentAngle = ((j * urgAngleInAStep) - 45) * (Math.PI / 180);

                    unitVectors[0, i, j] = Math.Cos(arduinoCurrentAngle) * Math.Cos(urgCurrentAngle);
                    unitVectors[1, i, j] = Math.Sin(urgCurrentAngle);
                    unitVectors[2, i, j] = Math.Sin(arduinoCurrentAngle) * Math.Cos(urgCurrentAngle);
                }
            }

            return unitVectors;
        }
    }
}