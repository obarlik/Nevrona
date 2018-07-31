using System;
using System.Collections.Generic;
using System.Linq;

namespace Nevrona
{
    public class NeuralNetwork : List<Neuron>
    {
        public Neuron[] Input { get; }
        public Neuron[] Output { get; }


        public int GenomeLength { get; }
        public int[] NeuronCounts { get; }
        public int Generation { get; set; }


        public double Fitness { get; protected set; }


        public NeuralNetwork(bool randomInit, params int[] neuronCounts)
        {
            if (neuronCounts.Length < 2)
                throw new ArgumentException(
                    "At least one hidden layer needed!", 
                    "neuronCount");

            NeuronCounts = neuronCounts;

            Input = GenerateNeurons()
                    .Take(neuronCounts.First())
                    .ToArray();

            var prevLayer = Input;

            GenomeLength =
                neuronCounts
                .Select(
                    (c, i) => i == 0 ? 
                        1 : 
                        neuronCounts[i - 1] + 1)
                .Sum();

            foreach (var nc in neuronCounts.Skip(1)
                               .Take(neuronCounts.Length - 2))
            {
                prevLayer =
                    GenerateNeurons(n => n.FullConnect(prevLayer))
                    .Take(nc)
                    .ToArray();
            }

            Output = GenerateNeurons(n => n.FullConnect(prevLayer))
                     .Take(neuronCounts.Last())
                     .ToArray();            
        }


        IEnumerable<Neuron> GenerateNeurons(Action<Neuron> neuronInit = null)
        {
            while (true)
            {
                var n = new Neuron();

                if (neuronInit != null)
                    neuronInit(n);

                Add(n);

                yield return n;
            }
        }


        public NeuralNetwork(double[] dna, int[] neuronCount)
            : this(false, neuronCount)
        {
            DNA = dna;
        }


        public static IEnumerable<T[]> ArraySplit<T>(T[] data, int blockSz)
        {
            var sz = 0;
            var block = new T[blockSz];

            foreach (var d in data)
            {
                block[sz++] = d;

                if (sz == blockSz)
                {
                    sz = 0;
                    yield return block;
                }
            }
        }


        void Reset()
        {
            Fitness = double.MinValue;
            this.AsParallel().ForAll(n => n.Reset());
        }


        public void Run(double[] inputs)
        {
            Reset();
            Input.Zip(inputs, (n, v) => n.Input = v);

            foreach (var n in Output)
                n.Calculate();
        }


        public void UpdateFitness(double[] inputs, Func<NeuralNetwork, double> fitnessFunc)
        {
            Run(inputs);
            Fitness = fitnessFunc(this);
        }

    //    Output.Zip(ideals, (o, i) => -Math.Pow(o.Output.Value - i, 2.0)).Sum();
      
        
        public double[] DNA
        {
            get
            {
                return this
                       .SelectMany(
                            n => n.Select(w => w.Value)
                                 .Concat(new[] { n.Bias }))
                                 .ToArray();
            }

            set
            {
                if (value.Length != GenomeLength)
                    throw new ArgumentException("Invalid genome length!", "value");

                var i = 0;

                foreach (var n in this)
                {
                    foreach (var w in n.ToArray())
                        n[w.Key] = value[i++];

                    n.Bias = value[i++];
                }
            }
        }


        public IEnumerable<NeuralNetwork> Generate()
        {
            while (true)
                yield return new NeuralNetwork(true, NeuronCounts);
        }


        public IEnumerable<NeuralNetwork> CrossOver(NeuralNetwork partner)
        {
            var crossIndex = (int)(GenomeLength * Neuron.RandomRange(0.15, 0.85));

            var dna1 = DNA;
            var dna2 = partner.DNA;

            if (dna1.Length != dna2.Length)
                throw new ArgumentException("Genome mismatch!", "partner");

            yield return new NeuralNetwork(
                dna1.Take(crossIndex)
                .Concat(dna2.Skip(crossIndex))
                .ToArray(), NeuronCounts)
            {
                Generation = Math.Max(Generation, partner.Generation) + 1
            };

            yield return new NeuralNetwork(
                dna2.Take(crossIndex)
                .Concat(dna1.Skip(crossIndex))
                .ToArray(), NeuronCounts)
            {
                Generation = Math.Max(Generation, partner.Generation) + 1
            };
        }


        public IEnumerable<NeuralNetwork> Mutate()
        {
            var mutateIndex = (int)(GenomeLength * Neuron.RandomRange(0.15, 0.85));

            var dna = DNA;
            dna[mutateIndex] = Neuron.RandomRange(-0.1, 0.1);

            yield return new NeuralNetwork(dna, NeuronCounts)
            {
                Generation = Generation + 1
            };
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

        
        public static IEnumerable<NeuralNetwork> Offspring(
            IEnumerable<NeuralNetwork> population,
            double selectRatio = 0.5,
            double elitismRatio = 0.1,
            double reproduceRatio = 0.2,
            double mutateRatio = 0.01)
        {
            var ordered = 
                population
                .OrderByDescending(nn => nn.Fitness)
                .ToArray();

            var selected = 
                ordered
                .Take((int)(ordered.Length * selectRatio))
                .ToArray();

            var elites =
                selected
                .Take((int)(selected.Length * elitismRatio))
                .ToArray();

            foreach (var nn in elites)
                yield return nn;

            foreach (var nn in RandomSelect(elites.Length)
                               .Take((int)(elites.Length * mutateRatio))
                               .SelectMany(i => elites[i].Mutate()))
                yield return nn;

            var parents =
                RandomSelect(elites.Length)
                .Take((int)(elites.Length * reproduceRatio / 2.0) * 2)
                .Select(i => elites[i])
                .ToArray();

            var motherCount = parents.Length / 2;

            foreach (var nn in
                     parents.Take(motherCount)
                     .Zip(parents.Skip(motherCount),
                          (m, f) => m.CrossOver(f))
                     .SelectMany(c => c))
                yield return nn;

            var otherCount = selected.Length - elites.Length;

            parents =
                RandomSelect(otherCount)
                .Take((int)(otherCount * reproduceRatio))
                .Select(i => selected[elites.Length + i])
                .ToArray();

            motherCount = parents.Length / 2;

            foreach (var nn in
                     parents.Take(motherCount)
                     .Zip(parents.Skip(motherCount),
                          (m, f) => m.CrossOver(f))
                     .SelectMany(c => c))
                yield return nn;
        }

    }
}
