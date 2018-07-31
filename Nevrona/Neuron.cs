using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nevrona
{
    public class Neuron : Dictionary<Neuron, double>
    {
        static Random rnd = new Random();


        public static double RandomRange(double min, double max)
        {
            return min + (max - min) * rnd.NextDouble();
        }


        public Neuron()
        {
        }


        public Neuron(Neuron[] inputLayer, bool randomInit = true)
        {
            FullConnect(inputLayer);

            if (randomInit)
                RandomizeWeights();
        }


        public void FullConnect(Neuron[] inputLayer)
        {
            Clear();

            foreach (var n in inputLayer)
                this[n] = 0.0;
        }


        public void RandomizeWeights()
        {
            foreach (var p in this)
                this[p.Key] = RandomRange(-0.1, 0.1);

            Bias = RandomRange(-0.1, 0.1);
        }


        public double Input;
        public double? Output;
        public double Bias;


        double Transfer(double x)
        {
            var v = Math.Exp(2.0 * x);

            return (v - 1.0) / (v + 1.0);
        }


        public void Reset()
        {
            Output = null;
        }


        public double Calculate()
        {
            return Output ??
                  (Output = Transfer(
                      this.Any() ?
                        (Input = this
                                 .Select(w => w.Key.Calculate() * w.Value)
                                 .Sum() + Bias) :
                        Input)).Value;
        }
    }
}
