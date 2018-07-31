using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Nevrona
{
    [Serializable]
    public class NeuralNetwork : List<Layer>
    {
        public Layer Input { get { return this[0]; } }
        public Layer Output { get { return this[Count - 1]; } }


        public int[] NeuronCounts
        {
            get
            {
                return this.Select(l => l.Count)
                       .ToArray();
            }

            set
            {
                Clear();

                if (value == null)
                    return;

                if (value.Length < 3)
                    throw new ArgumentException(
                        "At least one hidden layer needed!",
                        "neuronCount");

                foreach (var nc in value)
                {
                    Add(new Layer()
                    {
                        NeuronCount = nc
                    });
                }
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
                return this.SelectMany(l => 
                        l.SelectMany(n => n))
                       .Count();
            }
        }


        public int Generation;


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
            NeuronCounts = neuronCounts;
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


        public void Run(double[] inputs)
        {
            Input.Zip(
                inputs, 
                (n, v) => n.Input = v);

            Layer layer = null;

            foreach (var l in this)
            {
                l.Calculate(layer);
                layer = l;
            }
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
                       .SelectMany(l => l.SelectMany(n => n))
                       .ToArray();
            }

            set
            {
                if (value.Length != GenomeLength)
                    throw new ArgumentException("Invalid genome length!", "value");

                var i = 0;

                foreach (var n in this.SelectMany(l => l))
                    foreach (var ni in Enumerable.Range(0, n.Count))
                        n[ni] = value[i++];
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
