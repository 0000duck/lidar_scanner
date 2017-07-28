using System;

namespace GetRGBLibrary
{
    public class GetRGBLibrary
    {
        public static double[,,] RGBMultiplexer(double[,] Magnitudes, int ArduinoEndStep, int URGEndStep)
        {
            double[,,] magnitudeRGB = new double[2, ArduinoEndStep + 1, URGEndStep + 1];

            double maximumMagnitude = Magnitudes[0, 0];
            double highestMagnitude = Magnitudes[0, 0];

            for (int i = 0; i < ArduinoEndStep + 1; i++)
            {
                for (int j = 1; j < URGEndStep + 1; j++)
                {
                    if (Magnitudes[i, j] != maximumMagnitude)
                    {
                        if (Magnitudes[i, j] > maximumMagnitude)
                        {
                            maximumMagnitude = Magnitudes[i, j];
                        }
                        else
                        {
                            if (Magnitudes[i, j] > highestMagnitude)
                            {
                                highestMagnitude = Magnitudes[i, j];
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < ArduinoEndStep + 1; i++)
            {
                for (int j = 0; j < URGEndStep + 1; j++)
                {
                    byte r = 0;
                    byte g = 0;
                    byte b = 0;

                    if (Magnitudes[i, j] != maximumMagnitude)
                    {
                        double lengthPercentage = (Magnitudes[i, j] / highestMagnitude) * 100;

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
                    }

                    magnitudeRGB[0, i, j] = Magnitudes[i, j];
                    magnitudeRGB[1, i, j] = r << 16 | g << 8 | b;
                }
            }

            // show rgb data
            Console.WriteLine();
            Console.WriteLine("RGB Data: ");

            Console.WriteLine(magnitudeRGB[1, 0, 0] + " " + magnitudeRGB[1, 0, URGEndStep / 3] + " " + magnitudeRGB[1, 0, URGEndStep / 2] + " " + magnitudeRGB[1, 0, URGEndStep - (URGEndStep / 3)] + " " + magnitudeRGB[1, 0, URGEndStep]);
            Console.WriteLine(magnitudeRGB[1, ArduinoEndStep / 3, 0] + " " + magnitudeRGB[1, ArduinoEndStep / 3, URGEndStep / 3] + " " + magnitudeRGB[1, ArduinoEndStep / 3, URGEndStep / 2] + " " + magnitudeRGB[1, ArduinoEndStep / 3, URGEndStep - (URGEndStep / 3)] + " " + magnitudeRGB[1, ArduinoEndStep / 3, URGEndStep]);
            Console.WriteLine(magnitudeRGB[1, ArduinoEndStep / 2, 0] + " " + magnitudeRGB[1, ArduinoEndStep / 2, URGEndStep / 3] + " " + magnitudeRGB[1, ArduinoEndStep / 2, URGEndStep / 2] + " " + magnitudeRGB[1, ArduinoEndStep / 2, URGEndStep - (URGEndStep / 3)] + " " + magnitudeRGB[1, ArduinoEndStep / 2, URGEndStep]);
            Console.WriteLine(magnitudeRGB[1, ArduinoEndStep - (ArduinoEndStep / 3), 0] + " " + magnitudeRGB[1, ArduinoEndStep - (ArduinoEndStep / 3), URGEndStep / 3] + " " + magnitudeRGB[1, ArduinoEndStep - (ArduinoEndStep / 3), URGEndStep / 2] + " " + magnitudeRGB[1, ArduinoEndStep - (ArduinoEndStep / 3), URGEndStep - (URGEndStep / 3)] + " " + magnitudeRGB[1, ArduinoEndStep - (ArduinoEndStep / 3), URGEndStep]);
            Console.WriteLine(magnitudeRGB[1, ArduinoEndStep, 0] + " " + magnitudeRGB[1, ArduinoEndStep, URGEndStep / 3] + " " + magnitudeRGB[1, ArduinoEndStep, URGEndStep / 2] + " " + magnitudeRGB[1, ArduinoEndStep, URGEndStep - (URGEndStep / 3)] + " " + magnitudeRGB[1, ArduinoEndStep, URGEndStep]);

            return magnitudeRGB;
        }
    }
}
