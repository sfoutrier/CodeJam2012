using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SpeakingInTongues
{
    class Program
    {
        static void Main(string[] args)
        {
            var translastor = new Translator();
            // begin to learn
            translastor.LearnFrom("a zoo", "y qee");
            translastor.LearnFrom("our language is impossible to understand", "ejp mysljylc kd kxveddknmc re jsicpdrysi");
            translastor.LearnFrom("there are twenty six factorial possibilities", "rbcpc ypc rtcsra dkh wyfrepkym veddknkmkrkcd");
            translastor.LearnFrom("so it is okay if you want to just give up", "de kr kd eoya kw aej tysr re ujdr lkgc jv");

            translastor.FillBlanks();

            var inputFile = File.OpenText("input.txt");

            File.WriteAllLines("output.txt", 
                translastor.TranslateAll(inputFile)
                    .Select((original, i) => "Case #" + (i + 1) + ": " + original));
        }
    }

    class Translator
    {
        private Dictionary<char, char> _charMapping = new Dictionary<char,char>(26);

        public void LearnFrom(IEnumerable<char> english, IEnumerable<char> googlerese)
        {
            foreach (var tuple in googlerese.Zip(english, (x, y) => Tuple.Create(x, y)))
            {
                _charMapping[tuple.Item1] = tuple.Item2;
            }    
        }

        private IEnumerable<char> GetAllChars()
        {
            for (int i = 0; i < 26; ++i)
                yield return (char)('a' + i);
        }

        public void FillBlanks()
        {
            var englishChars = new HashSet<char>(GetAllChars());
            var googlereseChars = new HashSet<char>(GetAllChars());
            foreach (var mapping in _charMapping)
            {
                englishChars.Remove(mapping.Value);
                googlereseChars.Remove(mapping.Key);
            }

            // this will randomly fill the blanks ... unique result if only 1 missing char
            LearnFrom(englishChars, googlereseChars);
        }

        public string Translate(string line)
        {
            char o;
            return new string(line.Select(c => 
            {
                if(_charMapping.TryGetValue(c, out o))
                    return o;
                return c;
            }).ToArray());
        }

        public IEnumerable<string> TranslateAll(StreamReader reader)
        {
            var nbLine = int.Parse(reader.ReadLine());
            string line;
            for (int i = 0; i < nbLine; ++i)
            {
                line = reader.ReadLine();
                yield return Translate(line);
            }
        }
    }
}
