using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nevrona
{
    public class Neuron : List<double>
    {
        Layer _Layer;

        public Layer Layer
        {
            get { return _Layer; }

            set
            {
                _Layer = value;

                Clear();
                Add(0.0);

                if (value != null)
                    AddRange(new double[value.Count]);
            }
        }
        

        public double Input;
        public double? Output;


        public double Bias
        {
            get { return this[Count - 1]; }
            set { this[Count - 1] = value; }
        }


        public int GenomeLength
        {
            get { return Count; }
        }


        static Random rnd = new Random();


        public static double RandomRange(double min, double max)
        {
            return min + (max - min) * rnd.NextDouble();
        }


        public Neuron()
        {
            Add(0.0);
        }


        public Neuron RandomizeWeights()
        {
            for (var i = 0; i < Count; i++)
                this[i] = RandomRange(-0.1, 0.1);
            
            return this;
        }


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
                      Layer.PreviousLayer != null ?
                        (Input = this
                                 .Zip(Layer.PreviousLayer, 
                                      (w, n) => w * n.Calculate())
                                 .Sum() + Bias) :
                        Input + Bias)).Value;
        }


    }
}
