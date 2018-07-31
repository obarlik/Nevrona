using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nevrona
{
    public class NeuralNetwork : List<Layer>
    {
        public Layer Input { get { return this[0]; } }
        public Layer Output { get { return this[Count - 1]; } }


        public int[] NeuronCounts
        {
            get
            {
                return this.Select(l => l.Count).ToArray();

            }

            set
            {
                Clear();

                if (value == null || value.Length < 3)
                    return;

                Add(new Layer()
                {
                    NeuronCount = value[0]
                });

                var prevLayer = Input;

                foreach (var nc in value.Skip(1)
                                   .Take(value.Length - 2))
                {
                    Add(prevLayer = new Layer()
                    {
                        NeuronCount = nc,
                        PreviousLayer = prevLayer
                    });
                }

                Add(new Layer()
                {
                    NeuronCount = value.Last(),
                    PreviousLayer = prevLayer
                });
            }
        }


        public NeuralNetwork RandomizeWeights()
        {
            ForEach(l => l.RandomizeWeights());

            return this;
        }


        public int GenomeLength
        {
            get
            {
                return Input.NeuronCount
                     + this.Skip(1).Sum(l => l.NeuronCount * (l.PreviousLayer.Count + 1));
            }
        }


        public int Generation { get; set; }


        public double Fitness { get; protected set; }
        

        public NeuralNetwork()
        {
        }


        public NeuralNetwork(double[] dna, int[] neuronCount)
            : this(neuronCount)
        {
            DNA = dna;
        }


        public NeuralNetwork(int[] neuronCounts)
        {
            if (neuronCounts.Length < 2)
                throw new ArgumentException(
                    "At least one hidden layer needed!",
                    "neuronCount");

            NeuronCounts = neuronCounts;
        }


        public Layer AddLayer()
        {
            var layer = new Layer();

            if (this.Any())
                layer.PreviousLayer = this.Last();

            Add(layer);

            return layer;
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


        public void Reset()
        {
            Fitness = double.MinValue;
            this.AsParallel().ForAll(l => l.Reset());
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
                           l => l.SelectMany(
                               n => n.Concat(new[] { n.Bias })))
                       .ToArray();
            }

            set
            {
                if (value.Length != GenomeLength)
                    throw new ArgumentException("Invalid genome length!", "value");

                var i = 0;

                foreach (var l in this)
                {
                    foreach (var n in l)
                    {
                        foreach (var ni in Enumerable.Range(0, n.Count))
                            n[ni] = value[i++];

                        n.Bias = value[i++];
                    }
                }
            }
        }


        public IEnumerable<NeuralNetwork> Generate()
        {
            while (true)
                yield return new NeuralNetwork(NeuronCounts);
        }


        public IEnumerable<NeuralNetwork> CrossOver(NeuralNetwork partner, Action<NeuralNetwork> init = null)
        {
            var crossIndex = (int)(GenomeLength * Neuron.RandomRange(0.15, 0.85));

            var dna1 = DNA;
            var dna2 = partner.DNA;

            if (dna1.Length != dna2.Length)
                throw new ArgumentException("Genome mismatch!", "partner");

            var nn = new NeuralNetwork(
                dna1.Take(crossIndex)
                .Concat(dna2.Skip(crossIndex))
                .ToArray(), NeuronCounts)
            {
                Generation = Math.Max(Generation, partner.Generation) + 1
            };

            if (init != null)
                init(nn);

            yield return nn;

            nn = new NeuralNetwork(
                dna2.Take(crossIndex)
                .Concat(dna1.Skip(crossIndex))
                .ToArray(), NeuronCounts);
            
            if (init != null)
                init(nn);

            yield return nn;
        }


        public NeuralNetwork Mutate(Action<NeuralNetwork> init = null)
        {
            var mutateIndex = (int)(GenomeLength * Neuron.RandomRange(0.15, 0.85));

            var dna = DNA;
            dna[mutateIndex] = Neuron.RandomRange(-0.1, 0.1);

            var nn = new NeuralNetwork(dna, NeuronCounts);

            if (init != null)
                init(nn);            

            return nn;
        }

    }
}
