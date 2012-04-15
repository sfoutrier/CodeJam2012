using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace RecycledNumbers
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = File.OpenText("input.txt");

            File.WriteAllLines("output.txt",
                CycleCounter.ReadAllLines(inputFile)
                    .Select((x, i) => "Case #" + (i + 1) + ": " + x));

            //Console.ReadLine();
        }

        static class CycleCounter
        {
            public static IEnumerable<int> ReadAllLines(StreamReader reader)
            {
                var nbLine = int.Parse(reader.ReadLine());
                string line;
                while (null != (line = reader.ReadLine()))
                {
                    if (line.Length > 0)
                    {
                        var splittedLine = line.Split(' ');
                        yield return CountCyclesBetween(int.Parse(splittedLine[0]), int.Parse(splittedLine[1]));
                        --nbLine;
                    }
                }
                Debug.Assert(nbLine == 0);
            }

            public static IEnumerable<int> GetDigits(int input)
            {
                while (input > 0)
                {
                    yield return input % 10;
                    input /= 10;
                }
            }

            public static int ToNumber(IEnumerable<int> digits)
            {
                int result = 0;
                foreach (int i in digits.Reverse())
                {
                    result *= 10;
                    result += i;
                }
                return result;
            }

            public static IEnumerable<IEnumerable<int>> AllCycles(IEnumerable<int> digits)
            {
                var digitsArray = digits.ToArray();
                for (int i = 0; i < digitsArray.Length; ++i)
                    // remove numbers with a leading 0
                    if (digitsArray[(i - 1 + digitsArray.Length) % digitsArray.Length] != 0)
                        yield return Enumerable.Concat(digitsArray.Skip(i), digitsArray.Take(i));
            }

            public static int[] AllCyclesBetween(int input, int min, int max)
            {
                var cycles = AllCycles(GetDigits(input))
                    .Select(ToNumber)
                    .Where(x => x <= max && x >= min)
                    .Distinct()
                    .ToArray();

                return cycles;
            }

            public static int CountCyclesBetween(int min, int max)
            {
                int foundCyclesPairs = 0;
                var alreadyFoundNumbers = new HashSet<int>();
                for (int i = min; i <= max; ++i)
                {
                    if (!alreadyFoundNumbers.Contains(i))
                    {
                        var cycles = AllCyclesBetween(i, min, max);
                        foundCyclesPairs += cycles.Length * (cycles.Length - 1) / 2;

                        if (cycles.Length > 1)
                        {
                            foreach (int foundNumber in cycles)
                            {
                                alreadyFoundNumbers.Add(foundNumber);
                            }
                            //Console.WriteLine(cycles.Aggregate(string.Empty, (x, y) => x + " " + y));
                        }
                    }
                }
                return foundCyclesPairs;
            }
        }
    }
}
