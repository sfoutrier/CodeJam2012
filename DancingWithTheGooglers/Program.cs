using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace DancingWithTheGooglers
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = File.OpenText("input.txt");

            File.WriteAllLines("output.txt", 
                ScoreLine.ReadAllLines(inputFile)
                    .Select((x, i) => "Case #" + (i + 1) + ": " + x.FindNbWithAtLeastExpected()));
        }
    }

    class ScoreLine
    {
        public static IEnumerable<ScoreLine> ReadAllLines(StreamReader reader)
        {
            var nbLine = int.Parse(reader.ReadLine());
            string line;
            while (null != (line = reader.ReadLine()))
            {
                yield return new ScoreLine(line);
                --nbLine;
            }
            Debug.Assert(nbLine == 0);
        }

        public List<Tuple<int,int,int>> UsalScores { get; private set; }
        public int SurprisingScores { get; private set; }
        public int ExpectedScore { get; private set; }

        public ScoreLine(string line)
        {
            var splittedLine = line.Split(' ');
            int nbGooglers = int.Parse(splittedLine[0]);
            Debug.Assert(splittedLine.Length == nbGooglers + 3);

            SurprisingScores = int.Parse(splittedLine[1]);
            ExpectedScore = int.Parse(splittedLine[2]);
            UsalScores = new List<Tuple<int, int, int>>(
                Enumerable.Range(3, nbGooglers).Select(x => UsualScoresFromTotal(int.Parse(splittedLine[x]))));
        }

        private Tuple<int, int, int> UsualScoresFromTotal(int total)
        {
            int avg = total / 3;
            Debug.Assert(total >= 0 && total <= 30);
            switch (total % 3)
            {
                case 0:
                    return Tuple.Create(avg, avg, avg);
                case 1:
                    return Tuple.Create(avg + 1, avg, avg);
                case 2:
                    return Tuple.Create(avg + 1, avg + 1, avg);
            }

            throw new InvalidProgramException("This should never happen");
        }

        public int FindNbWithAtLeastExpected()
        {
            int founded = 0;
            int remainingSurprising = SurprisingScores;
            foreach (var currentScores in UsalScores)
            {
                if (currentScores.Item1 >= ExpectedScore)
                {
                    ++founded;
                }
                // we can't increase one of the 3 scores of more than 1 point
                else if (remainingSurprising > 0 && currentScores.Item1 >= ExpectedScore - 1)
                {
                    // only possible to change scores if we are in the case 0 or 2 of UsualScoresFromTotal
                    if (currentScores.Item1 == currentScores.Item2 && currentScores.Item2 > 0 && currentScores.Item1 < 10)
                    {
                        // then we could move 1 point form judge 2 to judge 1
                        ++founded;
                        --remainingSurprising;
                    }
                }
            }
            return founded;
        }
    }
}
