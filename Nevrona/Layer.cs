using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nevrona
{
    [Serializable]
    public class Layer : List<Neuron>
    {
        public Layer()
        {
        }


        public int NeuronCount
        {
            get { return Count; }

            set
            {
                while (Count > value) RemoveAt(Count - 1);
                while (Count < value) Add(new Neuron());
            }
        }

        
        public Layer RandomizeWeights()
        {
            ForEach(n => n.RandomizeWeights());

            return this;
        }


        public void Calculate(Layer prevLayer)
        {
            this.AsParallel()
            .ForAll(n => n.Calculate(prevLayer));
        }
    }
}
