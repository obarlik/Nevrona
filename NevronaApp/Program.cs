using Nevrona;
using System;
using System.Collections.Generic;
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
        }
    }
}
