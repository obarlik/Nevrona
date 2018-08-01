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

        const string Alpha    = "_abcdefghijklmnopqrstuvwxyz";
        
        static char NormalChar(char c)
        {
            switch(c)
            {
                case 'Ç':
                case 'ç':
                    return 'c';

                case 'Ğ':
                case 'ğ':
                    return 'g';

                case 'İ':
                case 'ı':
                    return 'i';

                case 'Ö':
                case 'ö':
                    return 'o';

                case 'Ş':
                case 'ş':
                    return 's';

                case 'Ü':
                case 'ü':
                    return 'u';                    
            }
            
            return char.ToLowerInvariant(c);
        }


        static double[] TextToNumber(string s, int minLength = 0, int maxLength = int.MaxValue)
        {
            var r =
                s.Select(c => (double)Alpha.IndexOf(NormalChar(c)) / (Alpha.Length * 10))
                .Select(d => d < 0 ? 0 : d)
                .ToArray();

            return 
                r.Length < minLength ?
                    r.Concat(Enumerable.Range(0, minLength - r.Length)
                             .Select(i => 0.0))
                    .ToArray() :
                r.Length > maxLength ?
                    r.Take(maxLength).ToArray() :
                r;
        }


        static string NumberToText(double[] n)
        {
            return new string(
                n.Select(d =>
                {
                    var i = (int)(d * (Alpha.Length * 10));
                    return i < 0 ? ' ' : Alpha[i];
                })
                .ToArray());
        }



        static void Main(string[] args)
        {   
            var population = 
                Population.FromFile(PopulationFile) ??
                new Population(500, 20, 50, 10, 1);

            NeuralNetwork champ = null;
            var data = File.ReadAllLines("TrainData.txt");

            var quit = false;
            var rnd = new Random();
            var fitness = 0.0;

            while (fitness < 0.6)
            {
                var s = data[rnd.Next(data.Length)];

                var d = TextToNumber(s, 10, 10);

                champ = population.Train(
                    new[] { d.Concat(d).ToArray() },
                    (nn, o) => o.First()[0]);

                fitness = champ.Fitness;

                Console.WriteLine("Generation: {0}    Fitness: {1}", champ.Generation, fitness);
            }

            while (!quit)
            {
                var s1 = data[rnd.Next(data.Length)];
                var s2 = data[rnd.Next(data.Length)];

                var d1 = TextToNumber(s1, 10, 10);
                var d2 = TextToNumber(s2, 10, 10);

                var input = d1.Concat(d2).ToArray();

                if (champ != null && champ.Run(input)[0] < 0.5)
                    continue;

                Console.Write("'{0}' ~ '{1}' ? ", s1, s2);

                var choice = '\0';

                while (!"YNEHQ".Contains(choice = char.ToUpper(Console.ReadKey().KeyChar))) ;

                Console.WriteLine();

                if (choice == 'Q')
                {
                    Console.WriteLine("Quiting...");
                    break;
                }

                
                champ = population.Train(
                    new[] { d1.Concat(d2).ToArray() },
                    (nn, o) =>
                        (o.First()[0] > 0.5 && "YE".Contains(choice))
                     || (o.First()[0] < -0.5 && "NH".Contains(choice)) ?
                            Math.Abs(o.First()[0]) :
                           -Math.Abs(o.First()[0]));

                Console.WriteLine("Fitness: {0:0.000}", champ.Fitness);
            }

            population.SaveToFile(PopulationFile);
        }
    }
}
