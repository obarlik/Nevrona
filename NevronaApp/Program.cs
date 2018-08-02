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


        static string NormalString(string s)
        {
            return new string(s.Select(c => NormalChar(c)).ToArray());
        }


        static string NegativeText(string s)
        {
            return new string(
                TextToNumber(s)
                .Select(n => Alpha[
                    (int)((1.0 - n * 10 - 1.0 / (Alpha.Length - 1)) 
                       * (Alpha.Length - 1))])
                .ToArray());
        }


        static double[] TextToNumber(string s, int minLength = 0, int maxLength = int.MaxValue)
        {
            var r =
                s.Select(c => (double)Math.Max(Alpha.IndexOf(NormalChar(c)), 0) / (Alpha.Length * 10))
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
                new Population(1000, 20, 50, 10, 1);

            NeuralNetwork champ = null;

            var data = File.ReadAllLines("TrainData.txt");
                
            var trainSet = data
                    .Take(3)
                    .SelectMany(s =>
                    {
                        var s2 = NegativeText(s);
                        var n = TextToNumber(s, 10, 10);
                        var n2 = TextToNumber(s2, 10, 10);

                        return new[]
                        {
                            new
                            {
                                Text = s,
                                Text2 = s,
                                Inputs = n.Concat(n).ToArray(),
                                Ideal = 0.3
                            },
                            new
                            {
                                Text = s,
                                Text2 = s2,
                                Inputs = n.Concat(n2).ToArray(),
                                Ideal = -0.3
                            },
                            new
                            {
                                Text = s,
                                Text2 = s2,
                                Inputs = n2.Concat(n).ToArray(),
                                Ideal = -0.3
                            },
                        };
                    })
                    .ToArray();

            var quit = false;
            var rnd = new Random();
            var fitness = 0.0;

            for (var iter = 0; iter < 100; iter++)
            {
                champ = population.Train(
                    trainSet.Select(d => d.Inputs).ToArray(),
                    (nn, o) => 1.0 / trainSet.AsParallel()
                               .Zip(o.AsParallel(), 
                                          (d, r) => 
                                            ((d.Ideal * r[0]) > 0.0) ?
                                                0 :
                                                -Math.Abs(r[0])).Sum());

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

                if (champ != null && champ.Run(input)[0] < 0.25)
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
                        (o.First()[0] >= 0.35 && "YE".Contains(choice))
                     || (o.First()[0] <= -0.35 && "NH".Contains(choice)) ?
                            Math.Abs(o.First()[0]) :
                           -Math.Abs(o.First()[0]));

                Console.WriteLine("Fitness: {0:0.000}", champ.Fitness);
            }

            population.SaveToFile(PopulationFile);
        }
    }
}
