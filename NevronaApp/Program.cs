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
        const string PopulationFile = "LastPopulation.xml";

        static void Main(string[] args)
        {   
            var population = 
                Population.FromFile(PopulationFile) ??
                new Population(20, 50, 150, 10, 1);

            var alpha = "abcçdefgğhıijklmnoöpqrsştuüvwxyz";

            var data = File.ReadAllLines("TrainData.txt")
                       .Select(l => l.Select(c => (double)alpha.IndexOf(c)/(alpha.Length*10)))

            population.Train();

            population.SaveToFile(PopulationFile);
        }
    }
}
