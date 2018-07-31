using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nevrona
{
    [Serializable]
    public class Population : List<NeuralNetwork>
    {
        public int Generation;

        public double SelectionRate = 0.5;
        public double ElitismRate = 0.1;
        public double ReproductionRate = 0.2;
        public double MutationRate = 0.01;


        public Population()
        {
        }


        public Population(int size, params int[] neuronCounts)
        {
            AddRange(Enumerable.Range(0, size)
                     .Select(i => new NeuralNetwork(neuronCounts)
                                  .RandomizeWeights()));
        }


        public Population(IEnumerable<double[]> members, params int[] neuronCounts)
        {
            AddRange(members
                     .Select(dna => new NeuralNetwork(dna, neuronCounts)));
        }


        public static IEnumerable<int> RandomSelect(int count)
        {
            var resultSet = Enumerable.Range(0, count)
                            .ToList();

            while (count > 0)
            {
                var i = (int)Neuron.RandomRange(0, count--);
                yield return resultSet[i];

                resultSet.RemoveAt(i);
            }
        }


        public void Offspring()
        {
            ++Generation;
            
            var selected =
                this.OrderByDescending(nn => nn.Fitness)
                .Take((int)(Count * SelectionRate))
                .ToArray();

            Clear();

            var elites =
                selected
                .Take((int)(selected.Length * ElitismRate))
                .ToArray();

            AddRange(elites);

            AddRange(
                RandomSelect(elites.Length)
                .Take((int)(elites.Length * MutationRate))
                .Select(i => elites[i].Mutate(c => c.Generation = Generation)));

            var parents =
                RandomSelect(elites.Length)
                .Take((int)(elites.Length * ReproductionRate / 2.0) * 2)
                .Select(i => elites[i])
                .ToArray();

            var motherCount = parents.Length / 2;

            AddRange(
                parents.Take(motherCount)
                .Zip(parents.Skip(motherCount),
                    (m, f) => m.CrossOver(f, c => c.Generation = Generation))
                .SelectMany(c => c));

            var otherCount = selected.Length - elites.Length;

            AddRange(
                RandomSelect(otherCount)
                .Take((int)(otherCount * MutationRate))
                .Select(i => selected[elites.Length + i]
                             .Mutate(c => c.Generation = Generation)));

            parents =
                RandomSelect(otherCount)
                .Take((int)(otherCount * ReproductionRate))
                .Select(i => selected[elites.Length + i])
                .ToArray();

            motherCount = parents.Length / 2;

            AddRange(
                parents.Take(motherCount)
                .Zip(parents.Skip(motherCount),
                    (m, f) => m.CrossOver(f, c => c.Generation = Generation))
                .SelectMany(c => c));
        }

    }
}
