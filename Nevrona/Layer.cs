using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Nevrona
{
    [Serializable]
    public class Layer
    {
        [XmlIgnore]
        public int WeightCount
        {
            get { return Neurons[0].WeightCount; }
            set { Neurons.ForEach(n => n.WeightCount = value); }
        }
        

        public List<Neuron> Neurons { get; }
        

        [XmlIgnore]
        public int NeuronCount
        {
            get { return Neurons.Count; }

            set
            {
                while (Neurons.Count > 1 && Neurons.Count > value) Neurons.RemoveAt(Neurons.Count - 1);
                while (Neurons.Count < value) Neurons.Add(new Neuron(WeightCount));
            }
        }


        public Layer()
        {
            Neurons = new List<Neuron>();
        }


        public Layer(int weightCount) : this()
        {
            Neurons.Add(new Neuron(weightCount));
        }


        public Layer RandomizeWeights()
        {
            Neurons.ForEach(n => n.RandomizeWeights());

            return this;
        }


        public void Calculate(Layer prevLayer)
        {
            Neurons.AsParallel()
            .ForAll(n => n.Calculate(prevLayer));
        }
    }
}
