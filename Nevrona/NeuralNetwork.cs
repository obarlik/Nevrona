using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Nevrona
{
    [Serializable]
    public class NeuralNetwork 
    {
        [XmlIgnore]
        public Layer Input { get { return Layers.First(); } }


        [XmlIgnore]
        public Layer Output { get { return Layers.Last(); } }
                
        public List<Layer> Layers { get; }


        [XmlAttribute]
        public int Generation { get; set; }


        [XmlIgnore]
        public double Fitness
        {
            get;
            protected set;
        }


        [XmlIgnore]
        public int[] NeuronCounts
        {
            get
            {
                return Layers.Select(l => l.Neurons.Count)
                       .ToArray();
            }

            set
            {
                Layers.Clear();

                if (value == null)
                    return;

                if (value.Length < 3)
                    throw new ArgumentException(
                        "At least one hidden layer needed!",
                        "neuronCount");

                Layer previousLayer = null;

                foreach (var nc in value)
                {
                    Layers.Add(previousLayer =
                        new Layer(previousLayer == null ? 0 : previousLayer.Neurons.Count)
                        {
                            NeuronCount = nc
                        });
                }
            }
        }


        [XmlIgnore]
        public int GenomeLength
        {
            get
            {
                return Layers.SelectMany(l =>
                        l.Neurons.SelectMany(n => n.Weights))
                       .Count();
            }
        }


        public NeuralNetwork RandomizeWeights()
        {
            Layers.ForEach(l => l.RandomizeWeights());

            return this;
        }



        public NeuralNetwork()
        {
            Layers = new List<Layer>();
        }


        public NeuralNetwork(double[] dna, int[] neuronCount)
            : this(neuronCount)
        {
            DNA = dna;
        }


        public NeuralNetwork(int[] neuronCounts)
            : this()
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


        public double[] Run(double[] inputs)
        {
            Input.Neurons.Zip(
                inputs,
                (n, v) => n.Input = v);

            Layer layer = null;

            Layers.ForEach(l =>
            {
                l.Calculate(layer);
                layer = l;
            });

            return Output.Neurons
                   .Select(n => n.Output)
                   .ToArray();
        }


        public void UpdateFitness(IEnumerable<double[]> inputs, Func<NeuralNetwork, IEnumerable<double[]>, double> fitnessFunc)
        {
            Fitness = fitnessFunc(this, inputs.Select(input => Run(input)));
        }


    //    Output.Zip(ideals, (o, i) => -Math.Pow(o.Output.Value - i, 2.0)).Sum();
      
        [XmlIgnore]
        public double[] DNA
        {
            get
            {
                return Layers
                       .SelectMany(l => l.Neurons.SelectMany(n => n.Weights))
                       .ToArray();
            }

            set
            {
                if (value.Length != GenomeLength)
                    throw new ArgumentException("Invalid genome length!", "value");

                var i = 0;

                foreach (var n in Layers.SelectMany(l => l.Neurons))
                    foreach (var ni in Enumerable.Range(0, n.Weights.Count))
                        n.Weights[ni] = value[i++];
            }
        }


        public IEnumerable<NeuralNetwork> Generate()
        {
            while (true)
                yield return new NeuralNetwork(NeuronCounts);
        }


        public IEnumerable<NeuralNetwork> CrossOver(NeuralNetwork partner, int generation)
        {
            var crossIndex = (int)(GenomeLength * Neuron.RandomRange(0.15, 0.85));

            var dna1 = DNA;
            var dna2 = partner.DNA;

            if (dna1.Length != dna2.Length)
                throw new ArgumentException("Genome mismatch!", "partner");

            yield return
                new NeuralNetwork(
                    dna1.Take(crossIndex)
                    .Concat(dna2.Skip(crossIndex))
                    .ToArray(), NeuronCounts)
                {
                    Generation = generation
                };

            yield return new NeuralNetwork(
                dna2.Take(crossIndex)
                .Concat(dna1.Skip(crossIndex))
                .ToArray(), NeuronCounts)
            {
                Generation = generation
            };
        }


        public NeuralNetwork Mutate(int generation)
        {
            var mutateIndex = (int)(GenomeLength * Neuron.RandomRange(0.15, 0.85));

            var dna = DNA;
            dna[mutateIndex] = Neuron.RandomRange(-0.1, 0.1);

            return new NeuralNetwork(dna, NeuronCounts)
            {
                Generation = generation
            };
        }

    }
}
