﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace Nevrona
{
    [Serializable]
    public class Population 
    {
        public int Generation;

        public double SelectionRate = 0.5;
        public double ElitismRate = 0.1;
        public double ReproductionRate = 0.3;
        public double MutationRate = 0.1;


        public int Size { get; set; }


        [XmlIgnore]
        public int[] NeuronCounts
        {
            get { return NeuralNetworks.First().NeuronCounts; }
        }


        public NeuralNetwork Train(
            IEnumerable<double[]> inputs,
            Func<NeuralNetwork, IEnumerable<double[]>, double> fitness)
        {
            NeuralNetworks.AsParallel()
            .ForAll(nn => nn.UpdateFitness(inputs, fitness));

            Offspring();

            return NeuralNetworks
                   .OrderByDescending(nn => nn.Fitness)
                   .ThenByDescending(nn => nn.Generation)
                   .First();
        }


        public List<NeuralNetwork> NeuralNetworks { get; }


        public Population()
        {
            NeuralNetworks = new List<NeuralNetwork>();
        }


        public Population(int size, params int[] neuronCounts)
            : this()
        {
            Size = size;
            
            NeuralNetworks
            .AddRange(
                Enumerable.Range(0, Size)
                .Select(i => new NeuralNetwork(neuronCounts)
                             .RandomizeWeights()));
        }


        public Population(IEnumerable<double[]> members, params int[] neuronCounts)
            : this()
        {
            NeuralNetworks
            .AddRange(members
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
                NeuralNetworks
                .OrderByDescending(nn => nn.Fitness)
                .ThenByDescending(nn => nn.Generation)
                .Take((int)(NeuralNetworks.Count * SelectionRate))
                .ToArray();

            NeuralNetworks.Clear();

            NeuralNetworks.AddRange(selected);

            NeuralNetworks
            .AddRange(
                RandomSelect(selected.Length)
                .Take((int)(selected.Length * MutationRate))
                .AsParallel()
                .Select(i => selected[i].Mutate(Generation)));

            var elites =
                selected
                .Take((int)(selected.Length * ElitismRate))
                .ToArray();

            var parents =
                RandomSelect(elites.Length)
                .Take((int)(elites.Length * ReproductionRate / 2.0) * 2)
                .AsParallel()
                .Select(i => elites[i])
                .ToArray();

            var motherCount = parents.Length / 2;

            NeuralNetworks
            .AddRange(
                parents.Take(motherCount).AsParallel()
                .Zip(parents.Skip(motherCount).AsParallel(),
                    (m, f) => m.CrossOver(f, Generation))
                .SelectMany(c => c));

            var otherCount = selected.Length - elites.Length;

            NeuralNetworks
            .AddRange(
                RandomSelect(otherCount)
                .Take((int)(otherCount * MutationRate))
                .AsParallel()
                .Select(i => selected[elites.Length + i]
                             .Mutate(Generation)));

            parents =
                RandomSelect(otherCount)
                .Take((int)(otherCount * ReproductionRate))
                .AsParallel()
                .Select(i => selected[elites.Length + i])
                .ToArray();

            motherCount = parents.Length / 2;

            NeuralNetworks
            .AddRange(
                parents.Take(motherCount).AsParallel()
                .Zip(parents.Skip(motherCount).AsParallel(),
                    (m, f) => m.CrossOver(f, Generation))
                .SelectMany(c => c));

            if (Size > NeuralNetworks.Count)
            {
                NeuralNetworks.AddRange(
                    Enumerable.Range(0, Size - NeuralNetworks.Count)
                    .AsParallel()
                    .Select(i => new NeuralNetwork(NeuronCounts)
                    {
                        Generation = Generation
                    })
                );
            }
        }


        public void SaveToFile(string fileName)
        {
            using (var fs = File.Create(fileName))
            {
                new XmlSerializer(GetType())
                .Serialize(fs, this);
            }
        }


        public static Population FromFile(string fileName)
        {
            if (File.Exists(fileName))
                using (var fs = File.OpenRead(fileName))
                {
                    return (Population)new XmlSerializer(typeof(Population))
                           .Deserialize(fs);
                }
            else
                return null;
        }
    }
}
