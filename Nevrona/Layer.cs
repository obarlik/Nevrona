using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nevrona
{
    public class Layer : List<Neuron>
    {
        public Layer PreviousLayer { get; set; }

        public Layer()
        {
        }


        public int NeuronCount
        {
            get { return Count; }

            set
            {
                while (Count > value) RemoveAt(Count - 1);
                while (Count < value) AddNeuron();
            }
        }


        public void Reset()
        {
            ForEach(n => n.Reset());
        }


        public Layer RandomizeWeights()
        {
            ForEach(n => n.RandomizeWeights());

            return this;
        }


        public Neuron AddNeuron()
        {
            var neuron = new Neuron() { Layer = this };
            Add(neuron);
            return neuron;
        }

    }
}
