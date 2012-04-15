using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace HallOfMirrors
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = File.OpenText("input.txt");
            int verbose = 0;
            if (args.Length > 0 &&
                !int.TryParse(args[0], out verbose))
                verbose = 0;

            File.WriteAllLines("output.txt", 
                Hall.ReadAllHalls(inputFile, verbose)
                    .Select((x, i) => "Case #" + (i + 1) + ": " + x.CountReflects()));
        }
    }

    class Hall
    {
        static DumpForm _form;

        public static IEnumerable<Hall> ReadAllHalls(StreamReader reader, int verbose = 0)
        {
            if (verbose >= 2)
            {
                _form = new DumpForm();
                _form.Show();
            }

            var nbHalls = int.Parse(reader.ReadLine());
            for(int i=0; i< nbHalls; ++i)
                yield return new Hall(reader, verbose);
        }

        #region Members
        public enum Square
        {
            Empty,
            Mirror,
            Me,
        }

        private Square[,] _hall;
        private int _height;
        private int _width;

        private int _verbose;
        public int Distance { get; private set; }

        public struct Position
        {
            public int X;
            public int Y;

            public Position(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        public Position Me { get; private set; }
        #endregion

        public Hall(StreamReader reader, int verbose = 0)
        {
            _verbose = verbose;
            var firstLine = reader.ReadLine().Split(' ');
            _height = int.Parse(firstLine[0]);
            _width = int.Parse(firstLine[1]);
            Distance = int.Parse(firstLine[2]);

            _hall = new Square[_height, _width];
            if (_verbose >= 1)
                Console.WriteLine("Distance max = " + Distance);

            for (int i = 0; i < _height; ++i)
            {
                string curLine = reader.ReadLine();
                if(_verbose >= 1)
                    Console.WriteLine(curLine);

                for (int j = 0; j < _width; j++)
                {
                    switch(curLine[j])
                    {
                        case '#':
                            _hall[i, j] = Square.Mirror;
                            break;
                        case '.':
                            _hall[i, j] = Square.Empty;
                            break;
                        case 'X':
                            _hall[i, j] = Square.Me;
                            Me = new Position(i, j);
                            break;
                    }
                }
            }
        }

        public int CountReflects()
        {
            int directions = 0;
            int nbReflects = 0;
            foreach (var vector in GetAllVectors())
            {
                ++directions;
                if (CastRay(vector))
                    ++nbReflects;
            }
            if(_verbose >= 1) 
                Console.WriteLine("Iterated " + directions + " initial directions");
            return nbReflects;
        }

        #region The stuff for computing all possible initial directions
        // finds all the directions the ray of light could take
        public IEnumerable<Vector> GetAllVectors()
        {
            foreach (var vector in GetVectorInQuarterPlane())
            {
                yield return vector;
                var rotated = vector.Rotate();
                yield return rotated;
                rotated = rotated.Rotate();
                yield return rotated;
                rotated = rotated.Rotate();
                yield return rotated;
            }
        }

        public IEnumerable<Vector> GetVectorInQuarterPlane()
        {
            yield return new Vector(1, 0);
            if (Distance > 1)
            {
                yield return new Vector(1, 1);
                for (int i = 1; 1 + i * i < Distance * Distance; ++i)
                {
                    for (int j = 1; j < i && j * j + i * i < Distance * Distance; ++j)
                    {
                        // I don't want cast several time the same direction with a constant mutilplier
                        if (Pgcd(i, j) == 1)
                        {
                            yield return new Vector(j, i);
                            yield return new Vector(i, j);
                        }
                    }
                }
            }
        }

        private int Pgcd(int a, int b)
        {
            if (b > a)
                return Pgcd(b, a);

            int c = a % b;
            if (c == 0)
                return b;
            return Pgcd(b, c);
        }
        #endregion

        #region Raytracing engine
        public bool CastRay(Vector direction)
        {
            if(_verbose >= 1)
                Console.WriteLine("Initial direction : " + direction.X + ", " + direction.Y);

            int vectorApplyed = 0;
            int x = Me.X;
            int y = Me.Y;
            int absDirX = Math.Abs(direction.X);
            int absDirY = Math.Abs(direction.Y);
            int signDirX = Math.Sign(direction.X);
            int signDirY = Math.Sign(direction.Y);

            Action<int, int> dump = null;

            if (absDirX >= absDirY)
            {
                if (_verbose >= 4)
                    dump = (a, b) => Dump(a, b);
                Func<int, int, Square> hall = (a, b) => _hall[a, b];

                while (direction.GetNormSquare(++vectorApplyed) <= Distance * Distance
                    && MoveUnit(absDirX, ref signDirX, absDirY, ref signDirY, ref x, ref y, hall, dump))
                {
                    if (_verbose >= 2 && _verbose <= 4)
                    {
                        Dump(x, y);
                        Console.WriteLine("Distance = " + Math.Sqrt(direction.GetNormSquare(vectorApplyed)));
                    }

                    if (Me.X == x && Me.Y == y)
                    {
                        if (_verbose >= 1)
                            Console.WriteLine("reflected");
                        return true;
                    }
                }
            }
            else
            {
                if (_verbose >= 4)
                    dump = (a, b) => Dump(b, a);
                Func<int, int, Square> hall = (a, b) => _hall[b, a];

                while (direction.GetNormSquare(++vectorApplyed) <= Distance * Distance
                    && MoveUnit(absDirY, ref signDirY, absDirX, ref signDirX, ref y, ref x, hall, dump))
                {
                    if (_verbose >= 2 && _verbose <= 4)
                    {
                        Dump(x, y);
                        Console.WriteLine("Distance = " + Math.Sqrt(direction.GetNormSquare(vectorApplyed)));
                    }

                    if (Me.X == x && Me.Y == y)
                    {
                        if (_verbose >= 1)
                            Console.WriteLine("reflected");
                        return true;
                    }
                }
            }

            if (_verbose >= 1)
                Console.WriteLine("lost in the mist");
            return false;
        }

        private void Dump(int x, int y)
        {
            if (_verbose >= 3)
            {
                StringBuilder bld = new StringBuilder();
                for (int i = 0; i < _height; ++i)
                {
                    for (int j = 0; j < _width; ++j)
                    {
                        if (i == x && j == y)
                            bld.Append(" o ");
                        else
                            switch (_hall[i, j])
                            {
                                case Square.Empty:
                                    bld.Append(" . ");
                                    break;
                                case Square.Me:
                                    bld.Append(" X ");
                                    break;
                                case Square.Mirror:
                                    bld.Append(" # ");
                                    break;
                            }
                    }
                    bld.AppendLine();
                }
                _form.SetContent(bld.ToString());
                _form.Refresh();
                _form.Focus();
                System.Threading.Thread.Sleep(50);
            }
            else
            {
                Console.WriteLine("=>");
                for (int i = 0; i < _height; ++i)
                {
                    for (int j = 0; j < _width; ++j)
                    {
                        if (i == x && j == y)
                            Console.Write('o');
                        else
                            switch (_hall[i, j])
                            {
                                case Square.Empty:
                                    Console.Write('.');
                                    break;
                                case Square.Me:
                                    Console.Write('X');
                                    break;
                                case Square.Mirror:
                                    Console.Write('#');
                                    break;
                            }
                    }
                    Console.WriteLine();
                }
            }
        }

        private bool MoveUnit(int absDirX, ref int signDirX, int absDirY, ref int signDirY, ref int x, ref int y, Func<int, int, Square> hall, Action<int, int> dump)
        {
            int j = 0;
            for (int i = 0; i < absDirX; ++i)
            {
                int deltaX = absDirX * (2 * j + 1);
                int deltaY = absDirY * (2 * i + 1);
                if (deltaY > deltaX)
                {
                    ++j;
                    // we increase Y first
                    y += signDirY;
                    if (hall(x, y) == Square.Mirror)
                    {
                        signDirY = -signDirY;
                        y += signDirY;
                    }

                    x += signDirX;
                    if (hall(x, y) == Square.Mirror)
                    {
                        signDirX = -signDirX;
                        x += signDirX;
                    }
                }
                else if (deltaY == deltaX)
                {
                    ++j;
                    // we are on a corner, increase x and y at the same time
                    if (hall(x + signDirX, y + signDirY) != Square.Mirror)
                    {
                        // cross the corner
                        y += signDirY;
                        x += signDirX;
                    }
                    else
                    {
                        if (hall(x + signDirX, y) == Square.Mirror)
                        {
                            if (hall(x, y + signDirY) == Square.Mirror)
                            {
                                // flip both
                                signDirX = -signDirX;
                                signDirY = -signDirY;
                            }
                            else
                            {
                                // flip x
                                signDirX = -signDirX;
                                y += signDirY;
                            }
                        }
                        else if (hall(x, y + signDirY) == Square.Mirror)
                        {
                            // flip y
                            signDirY = -signDirY;
                            x += signDirX;
                        }
                        else
                        {
                            // no other mirror, the ray is destroyed
                            if (_verbose >= 1)
                                Console.WriteLine("ray destroyed on the corner");

                            return false;
                        }
                    }
                }
                else
                {
                    // we only increase x
                    x += signDirX;
                    if (hall(x, y) == Square.Mirror)
                    {
                        signDirX = -signDirX;
                        x += signDirX;
                    }
                }
                dump(x, y);
            }

            return true;
        }
        #endregion
    }

    [DebuggerDisplay("Vector({X}, {Y})")]
    class Vector
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int GetNormSquare(int applyed)
        {
            return applyed * applyed * X * X + applyed * applyed * Y * Y; 
        }

        public Vector(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector Rotate()
        {
            return new Vector(Y, -X);
        }
    }
}
