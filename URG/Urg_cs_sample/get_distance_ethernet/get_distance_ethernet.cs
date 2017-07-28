using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using SCIP_library;

class get_distance_ethernet
{
    const int StartStep = 0;
    const int EndStep = 1080;
    const int Step = 3;

    static void Main(string[] args)
    {
        try
        {
            double[] lengths = GetLengths();
            double[,] unitVectors = GetUnitVectors();
            double[,] vectors = GetVectors(lengths, unitVectors);

            WritePCDFile();
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

    private static void WritePCDFile()
    {
        
    }

    private static double[,] GetVectors(double[] lengths, double[,] unitVectors)
    {
        double[,] vectors = new double[3, EndStep + 1];

        for (int i = 0; i < EndStep + 1; i++)
        {
            vectors[0, i] = lengths[i] * unitVectors[0, i];
            vectors[1, i] = lengths[i] * unitVectors[1, i];
            vectors[2, i] = lengths[i] * unitVectors[2, i];
        }

        // show unit vector data
        Console.WriteLine();
        Console.WriteLine(vectors[0, 0] + " " + vectors[1, 0] + " " + vectors[2, 0]);
        Console.WriteLine(vectors[0, EndStep / 3] + " " + vectors[1, EndStep / 3] + " " + vectors[2, EndStep / 3]);
        Console.WriteLine(vectors[0, EndStep / 2] + " " + vectors[1, EndStep / 2] + " " + vectors[2, EndStep / 2]);
        Console.WriteLine(vectors[0, EndStep - (EndStep / 3)] + " " + vectors[1, EndStep - (EndStep / 3)] + " " + vectors[2, EndStep - (EndStep / 3)]);
        Console.WriteLine(vectors[0, EndStep] + " " + vectors[1, EndStep] + " " + vectors[2, EndStep]);

        return vectors;
    }

    private static double[,] GetUnitVectors()
    {
        double[,] unitVectors = new double[3, EndStep + 1];

        double angleInAStep = 270 / ((double)EndStep + 1);

        for(int i = 0; i < EndStep + 1; i++)
        {
            double currentAngle = (i * angleInAStep) * (Math.PI / 180);

            unitVectors[0, i] = Math.Cos(currentAngle) * Math.Cos(0);
            unitVectors[1, i] = Math.Sin(currentAngle) * Math.Cos(0);
            unitVectors[2, i] = Math.Sin(0);
        }

        // show unit vector data
        Console.WriteLine();
        Console.WriteLine(unitVectors[0, 0] + " " + unitVectors[1, 0] + " " + unitVectors[2, 0]);
        Console.WriteLine(unitVectors[0, EndStep / 3] + " " + unitVectors[1, EndStep / 3] + " " + unitVectors[2, EndStep / 3]);
        Console.WriteLine(unitVectors[0, EndStep / 2] + " " + unitVectors[1, EndStep / 2] + " " + unitVectors[2, EndStep / 2]);
        Console.WriteLine(unitVectors[0, EndStep - (EndStep / 3)] + " " + unitVectors[1, EndStep - (EndStep / 3)] + " " + unitVectors[2, EndStep - (EndStep / 3)]);
        Console.WriteLine(unitVectors[0, EndStep] + " " + unitVectors[1, EndStep] + " " + unitVectors[2, EndStep]);

        return unitVectors;
    }

    private static double[] GetLengths()
    {
        double[] lengths = new double[EndStep + 1];

        string ip_address;
        int port_number;

        get_connect_information(out ip_address, out port_number);

        TcpClient urg = new TcpClient();
        urg.Connect(ip_address, port_number);
        NetworkStream stream = urg.GetStream();

        write(stream, SCIP_Writer.SCIP2());
        read_line(stream); // ignore echo back

        write(stream, SCIP_Writer.MD(StartStep, EndStep));
        read_line(stream);  // ignore echo back

        List<long> distances = new List<long>();
        long time_stamp = 0;

        Console.WriteLine();

        for (int i = 0; i < Step; i++)
        {
            string receive_data = read_line(stream);

            if (!SCIP_Reader.MD(receive_data, ref time_stamp, ref distances))
            {
                Console.WriteLine(receive_data);

                break;
            }
            if (distances.Count == 0)
            {
                Console.WriteLine(receive_data);

                continue;
            }

            // show distance data
            Console.WriteLine("time stamp: " + time_stamp.ToString() + " distances : " + distances[0].ToString() + " " + distances[EndStep / 3].ToString() + " " + distances[EndStep / 2].ToString() + " " + distances[EndStep - (EndStep / 3)].ToString() + " " + distances[EndStep].ToString());

            if (i > 0)
            {
                for (int j = 0; j < EndStep + 1; j++)
                {
                    lengths[j] = (lengths[j] + distances[j]) / 2;
                }
            }
            else
            {
                for (int j = 0; j < EndStep + 1; j++)
                {
                    lengths[j] = distances[j];
                }
            }
        }

        // show length data
        Console.WriteLine();
        Console.WriteLine("time stamp: " + time_stamp.ToString() + " lengths : " + lengths[0].ToString() + " " + lengths[EndStep / 3].ToString() + " " + lengths[EndStep / 2].ToString() + " " + lengths[(EndStep / 2) + (EndStep / 3)].ToString() + " " + lengths[EndStep].ToString());

        write(stream, SCIP_Writer.QT());    // stop measurement mode
        read_line(stream); // ignore echo back

        stream.Close();
        urg.Close();

        return lengths;
    }

    /// <summary>
    /// get connection information from user.
    /// </summary>

    private static void get_connect_information(out string ip_address, out int port_number)
    {
        ip_address = "192.168.1.11";
        port_number = 10940;

        Console.WriteLine("Connect setting = IP Address : " + ip_address + " Port number : " + port_number.ToString());
    }

    /// <summary>
    /// Read to "\n\n" from NetworkStream
    /// </summary>
    /// <returns>receive data</returns>
    
    static string read_line(NetworkStream stream)
    {
        if (stream.CanRead)
        {
            StringBuilder sb = new StringBuilder();

            bool is_NL = false;
            bool is_NL2 = false;

            while (!is_NL2)
            {
                char buf = (char)stream.ReadByte();

                if (buf == '\n')
                {
                    if (is_NL)
                    {
                        is_NL2 = true;
                    }
                    else
                    {
                        is_NL = true;
                    }
                }
                else
                {
                    is_NL = false;
                }

                sb.Append(buf);
            }

            return sb.ToString();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// write data
    /// </summary>
    
    static bool write(NetworkStream stream, string data)
    {
        if (stream.CanWrite)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            stream.Write(buffer, 0, buffer.Length);

            return true;
        }
        else
        {
            return false;
        }
    }
}