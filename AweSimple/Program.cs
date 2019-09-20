using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace AweSimple
{
    class Program
    {
        static void TestSpeed()
        {
            try
            {
                byte[] inputBuffer = new byte[1024];

                Stopwatch stopWatch = new Stopwatch();

                using (AweStream.AweStream aweStream = new AweStream.AweStream(1024 * 1024 * 100))
                {
                    //Map
                    aweStream.Map();
                    stopWatch.Start();

                    //Copy one bytes
                    //Use unsafe pointer
                    for (int i = 0; i < 1024 * 1024 * 100; i++)
                    {
                        unsafe
                        {
                            aweStream.LpMemory[i] = 1;
                        }
                    }

                    stopWatch.Stop();

                    Console.WriteLine(stopWatch.ElapsedMilliseconds);

                    aweStream.Position = 0;

                    
                    //Block copy
                    stopWatch.Reset();
                    stopWatch.Start();

                    for (int i = 0; i < 1024 * 100; i++)
                    {
                        unsafe
                        {
                            aweStream.Write(inputBuffer, 0, 1024);
                        }
                    }

                    stopWatch.Stop();

                    Console.WriteLine(stopWatch.ElapsedMilliseconds);

                    //UnMap
                    aweStream.UnMap();
                }

                byte[] ManageBuf = new byte[1024 * 1024 * 100];

                stopWatch.Reset();
                stopWatch.Start();

                for (int i = 0; i < 1024 * 1024 * 100; i++)
                {
                    ManageBuf[i] = 1;
                }

                stopWatch.Stop();

                Console.WriteLine(stopWatch.ElapsedMilliseconds);

                stopWatch.Reset();
                stopWatch.Start();

                for (int i = 0; i < 1024 * 100; i++)
                {
                    Array.Copy(inputBuffer, 0, ManageBuf, i * 1024, 1024);

                }

                stopWatch.Stop();

                Console.WriteLine(stopWatch.ElapsedMilliseconds);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        }

        static void Main(string[] args)
        {
            TestSpeed();

            string input = Console.ReadLine();
            List<AweStream.AweStream> aweStreams = new List<AweStream.AweStream>();

            while (true)
            {
                switch (input)
                {
                    case "a":
                        try
                        {
                            AweStream.AweStream aweStream = new AweStream.AweStream(1024 * 1024 * 100);
                            aweStreams.Add(aweStream);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        break;
                    case "m":
                        foreach (AweStream.AweStream aweStream in aweStreams)
                        {
                            try
                            {
                                aweStream.Map();
                                Console.WriteLine("Map ok!");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        break;

                    case "u":
                        foreach (AweStream.AweStream aweStream in aweStreams)
                        {
                            try
                            {
                                aweStream.UnMap();
                                Console.WriteLine("UnMap ok!");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        break;

                    case "d":
                        foreach (AweStream.AweStream aweStream in aweStreams)
                        {
                            try
                            {
                                aweStream.Dispose();
                                Console.WriteLine("Dispose ok!");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        aweStreams = new List<AweStream.AweStream>();

                        break;

                    case "x":
                        return;
                }

                input = Console.ReadLine();
            }
        }
    }
}
