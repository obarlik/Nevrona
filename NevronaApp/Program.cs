using Nevrona;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NevronaApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var population = new Population(2, 3, 4, 1);

            population.SaveToFile("test.xml");

            var pop2 = Population.FromFile("test.xml");

            pop2.SaveToFile("test2.xml");

            var t1 = File.ReadAllText("test.xml");
            var t2 = File.ReadAllText("test2.xml");

            if (t1 != t2)
                throw new Exception("Farklı!");
        }
    }
}
