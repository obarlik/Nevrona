using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nevrona
{
    [Serializable]
    public class Neuron : List<double>
    {
        [NonSerialized]
        public double Input;


        [NonSerialized]
        public double Output;
        

        public int GenomeLength
        {
            get { return Count; }
        }


        public double Bias
        {
            get { return this.Last(); }
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
        
        
        public double Calculate(Layer previousLayer)
        {
            return Output = 
                Transfer(
                    (previousLayer != null ?
                     (Input = 
                        this
                        .Zip(previousLayer,
                            (w, n) => w * n.Output)
                        .Sum()) :
                        Input) + Bias);
        }


    }
}
