using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Nevrona
{
    [Serializable]
    public class Neuron
    {
        [XmlIgnore]
        public double Input { get; set; }


        [XmlIgnore]
        public double Output { get; set; }


        public List<double> Weights { get; }


        [XmlIgnore]
        public int GenomeLength
        {
            get { return Weights.Count; }
        }


        [XmlIgnore]
        public double Bias
        {
            get { return Weights.Last(); }
        }


        [XmlIgnore]
        public int WeightCount
        {
            get { return Weights.Count - 1; }
            set
            {
                while (Weights.Count > 1 && Weights.Count > value + 1) Weights.RemoveAt(Weights.Count - 2);
                while (Weights.Count < value + 1) Weights.Insert(Weights.Count - 1, 0.0);
            }
        }


        static Random rnd = new Random();


        public static double RandomRange(double min, double max)
        {
            return min + (max - min) * rnd.NextDouble();
        }


        public Neuron()
        {
            Weights = new List<double>();
        }


        public Neuron(int weightCount) : this()
        {
            Weights.AddRange(
                Enumerable.Range(0, weightCount + 1)
                .Select(i => 0.0));
        }


        public Neuron RandomizeWeights()
        {
            for (var i = 0; i < Weights.Count; i++)
                Weights[i] = RandomRange(-0.1, 0.1);
            
            return this;
        }


        double Transfer(double x)
        {
            var v = Math.Exp(2.0 * x);

            return (v - 1.0) / (v + 1.0);
        }
        
        
        public double Calculate(Layer previousLayer)
        {
            return Output = 
                Transfer(
                    (previousLayer != null ?
                     (Input =
                        Weights
                        .Zip(previousLayer.Neurons,
                            (w, n) => w * n.Output)
                        .Sum()) :
                        Input) + Bias);
        }


    }
}
