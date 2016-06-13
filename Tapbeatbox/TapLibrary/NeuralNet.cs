using System;
using System.Collections.Generic;
using System.Text;

namespace Tapbeatbox.TapLibrary
{
    public enum TrainingType
    {
        BackPropogation
    }

    public class NeuralFactor
    {
        #region Constructors

        public NeuralFactor(double weight)
        {
            m_weight = weight;
            m_lastDelta = m_delta = 0;
        }

        #endregion

        #region Member Variables

        private double m_weight, m_lastDelta, m_delta;

        #endregion

        #region Properties

        public double Weight
        {
            get { return m_weight; }
            set { m_weight = value; }
        }

        public double H_Vector
        {
            get { return m_delta; }
            set { m_delta = value; }
        }

        public double Last_H_Vector
        {
            get { return m_lastDelta; }
            //set { m_lastDelta = value; }
        }

        #endregion

        #region Methods

        public void ApplyWeightChange(ref double learningRate)
        {
            m_lastDelta = m_delta;
            m_weight += m_delta * learningRate;
        }

        public void ResetWeightChange()
        {
            m_lastDelta = m_delta = 0;
        }

        #endregion
    }

    public interface INeuronReceptor
    {
        Dictionary<INeuronSignal, NeuralFactor> Input { get; }
    }

    public interface INeuronSignal
    {
        double Output { get; set; }
    }

    public interface INeuron : INeuronSignal, INeuronReceptor
    {
        void Pulse(INeuralLayer layer);
        void ApplyLearning(INeuralLayer layer, ref double learningRate);
        void InitializeLearning(INeuralLayer layer);

        NeuralFactor Bias { get; set; }

        double Error { get; set; }
        double LastError { get; set; }
    }

    public interface INeuralLayer : IList<INeuron>
    {
        void Pulse(INeuralNet net);
        void ApplyLearning(INeuralNet net);
        void InitializeLearning(INeuralNet net);
    }

    public interface INeuralNet
    {
        INeuralLayer PerceptionLayer { get; }
        INeuralLayer HiddenLayer { get; }
        INeuralLayer OutputLayer { get; }

        double LearningRate { get; set; }

        void Pulse();
        void ApplyLearning();
        void InitializeLearning();
    }

    public class Neuron : INeuron
    {
        #region Constructors

        public Neuron(double bias)
        {
            m_bias = new NeuralFactor(bias);
            m_error = 0;
            m_input = new Dictionary<INeuronSignal, NeuralFactor>();
        }

        #endregion

        #region Member Variables

        private Dictionary<INeuronSignal, NeuralFactor> m_input;
        double m_output, m_error, m_lastError;
        NeuralFactor m_bias;

        #endregion

        #region INeuronSignal Members

        public double Output
        {
            get { return m_output; }
            set { m_output = value; }
        }

        #endregion

        #region INeuronReceptor Members

        public Dictionary<INeuronSignal, NeuralFactor> Input
        {
            get { return m_input; }
        }

        #endregion

        #region INeuron Members

        public void Pulse(INeuralLayer layer)
        {
            lock (this)
            {
                m_output = 0;

                foreach (KeyValuePair<INeuronSignal, NeuralFactor> item in m_input)
                    m_output += item.Key.Output * item.Value.Weight;

                m_output += m_bias.Weight;

                m_output = Sigmoid(m_output);
            }
        }

        public NeuralFactor Bias
        {
            get { return m_bias; }
            set { m_bias = value; }
        }

        public double Error
        {
            get { return m_error; }
            set
            {
                m_lastError = m_error;
                m_error = value;
            }
        }

        public void ApplyLearning(INeuralLayer layer, ref double learningRate)
        {
            foreach (KeyValuePair<INeuronSignal, NeuralFactor> m in m_input)
                m.Value.ApplyWeightChange(ref learningRate);

            m_bias.ApplyWeightChange(ref learningRate);
        }

        public void InitializeLearning(INeuralLayer layer)
        {
            foreach (KeyValuePair<INeuronSignal, NeuralFactor> m in m_input)
                m.Value.ResetWeightChange();

            m_bias.ResetWeightChange();
        }

        public double LastError
        {
            get { return m_lastError; }
            set { m_lastError = value; }
        }

        #endregion

        #region Private Static Utility Methods

        public static double Sigmoid(double value)
        {
            return 1 / (1 + Math.Exp(-value));
        }

        #endregion
    }

    public class NeuralLayer : INeuralLayer
    {
        #region Constructor

        public NeuralLayer()
        {
            m_neurons = new List<INeuron>();
        }

        #endregion

        #region Member Variables

        private List<INeuron> m_neurons;

        #endregion

        #region IList<INeuron> Members

        public int IndexOf(INeuron item)
        {
            return m_neurons.IndexOf(item);
        }

        public void Insert(int index, INeuron item)
        {
            m_neurons.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_neurons.RemoveAt(index);
        }

        public INeuron this[int index]
        {
            get { return m_neurons[index]; }
            set { m_neurons[index] = value; }
        }

        #endregion

        #region ICollection<INeuron> Members

        public void Add(INeuron item)
        {
            m_neurons.Add(item);
        }

        public void Clear()
        {
            m_neurons.Clear();
        }

        public bool Contains(INeuron item)
        {
            return m_neurons.Contains(item);
        }

        public void CopyTo(INeuron[] array, int arrayIndex)
        {
            m_neurons.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_neurons.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(INeuron item)
        {
            return m_neurons.Remove(item);
        }

        #endregion

        #region IEnumerable<INeuron> Members

        public IEnumerator<INeuron> GetEnumerator()
        {
            return m_neurons.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INeuralLayer Members

        public void Pulse(INeuralNet net)
        {
            foreach (INeuron n in m_neurons)
                n.Pulse(this);
        }

        public void ApplyLearning(INeuralNet net)
        {
            double learningRate = net.LearningRate;

            foreach (INeuron n in m_neurons)
                n.ApplyLearning(this, ref learningRate);
        }

        public void InitializeLearning(INeuralNet net)
        {
            foreach (INeuron n in m_neurons)
                n.InitializeLearning(this);
        }

        #endregion
    }

    public class NeuralNet : INeuralNet
    {
        #region Constructors

        public NeuralNet()
        {
            m_learningRate = 0.5;
        }

        #endregion

        #region Member Variables

        private INeuralLayer m_inputLayer;
        private INeuralLayer m_outputLayer;
        private INeuralLayer m_hiddenLayer;
        private double m_learningRate;

        #endregion

        #region INeuralNet Members

        public INeuralLayer PerceptionLayer
        {
            get { return m_inputLayer; }
        }

        public INeuralLayer HiddenLayer
        {
            get { return m_hiddenLayer; }
        }

        public INeuralLayer OutputLayer
        {
            get { return m_outputLayer; }
        }

        public double LearningRate
        {
            get { return m_learningRate; }
            set { m_learningRate = value; }
        }

        public void Pulse()
        {
            lock (this)
            {
                m_hiddenLayer.Pulse(this);
                m_outputLayer.Pulse(this);
            }
        }

        public void ApplyLearning()
        {
            lock (this)
            {
                m_hiddenLayer.ApplyLearning(this);
                m_outputLayer.ApplyLearning(this);
            }
        }

        public void InitializeLearning()
        {
            lock (this)
            {
                m_hiddenLayer.InitializeLearning(this);
                m_outputLayer.InitializeLearning(this);
            }
        }

        public void Train(double[][] inputs, double[][] expected, TrainingType type, int iterations, Components.TrainInfo trainInfo)
        {
            int i, j;

            switch (type)
            {
                case TrainingType.BackPropogation:

                    lock (this)
                    {

                        for (i = 0; i < iterations; i++)
                        {

                            InitializeLearning(); // set all weight changes to zero

                            for (j = 0; j < inputs.Length; j++)
                                BackPropogation_TrainingSession(this, inputs[j], expected[j]);

                            ApplyLearning(); // apply batch of cumlutive weight changes
                            trainInfo.Presentage = (double)i / iterations;
                        }

                    }
                    break;
                default:
                    throw new ArgumentException("Unexpected TrainingType");
            }
        }

        #endregion

        #region Methods

        public void Initialize(int randomSeed,
            int inputNeuronCount, int hiddenNeuronCount, int outputNeuronCount)
        {
            Initialize(this, randomSeed, inputNeuronCount, hiddenNeuronCount, outputNeuronCount);
        }

        public void PreparePerceptionLayerForPulse(double[] input)
        {
            PreparePerceptionLayerForPulse(this, input);
        }

        #region Private Static Utility Methods -----------------------------------------------

        private static void Initialize(NeuralNet net, int randomSeed,
            int inputNeuronCount, int hiddenNeuronCount, int outputNeuronCount)
        {

            #region Declarations

            int i, j;
            Random rand;

            #endregion

            #region Initialization

            rand = new Random(randomSeed);

            #endregion

            #region Execution

            net.m_inputLayer = new NeuralLayer();
            net.m_outputLayer = new NeuralLayer();
            net.m_hiddenLayer = new NeuralLayer();

            for (i = 0; i < inputNeuronCount; i++)
                net.m_inputLayer.Add(new Neuron(0));

            for (i = 0; i < outputNeuronCount; i++)
                net.m_outputLayer.Add(new Neuron(rand.NextDouble()));

            for (i = 0; i < hiddenNeuronCount; i++)
                net.m_hiddenLayer.Add(new Neuron(rand.NextDouble()));

            // wire-up input layer to hidden layer
            for (i = 0; i < net.m_hiddenLayer.Count; i++)
                for (j = 0; j < net.m_inputLayer.Count; j++)
                    net.m_hiddenLayer[i].Input.Add(net.m_inputLayer[j], new NeuralFactor(rand.NextDouble()));

            // wire-up output layer to hidden layer
            for (i = 0; i < net.m_outputLayer.Count; i++)
                for (j = 0; j < net.m_hiddenLayer.Count; j++)
                    net.m_outputLayer[i].Input.Add(net.HiddenLayer[j], new NeuralFactor(rand.NextDouble()));

            #endregion
        }

        private static void CalculateErrors(NeuralNet net, double[] desiredResults)
        {
            #region Declarations

            int i, j;
            double temp, error;
            INeuron outputNode, hiddenNode;

            #endregion

            #region Execution

            // Calcualte output error values 
            for (i = 0; i < net.m_outputLayer.Count; i++)
            {
                outputNode = net.m_outputLayer[i];
                temp = outputNode.Output;

                outputNode.Error = (desiredResults[i] - temp) * SigmoidDerivative(temp); //* temp * (1.0F - temp);
            }

            // calculate hidden layer error values
            for (i = 0; i < net.m_hiddenLayer.Count; i++)
            {
                hiddenNode = net.m_hiddenLayer[i];
                temp = hiddenNode.Output;

                error = 0;
                for (j = 0; j < net.m_outputLayer.Count; j++)
                {
                    outputNode = net.m_outputLayer[j];
                    error += (outputNode.Error * outputNode.Input[hiddenNode].Weight) * SigmoidDerivative(temp);// *(1.0F - temp);                   
                }

                hiddenNode.Error = error;

            }

            #endregion
        }

        private static double SigmoidDerivative(double value)
        {
            return value * (1.0F - value);
        }

        public static void PreparePerceptionLayerForPulse(NeuralNet net, double[] input)
        {
            #region Declarations

            int i;

            #endregion

            #region Execution

            if (input.Length != net.m_inputLayer.Count)
                throw new ArgumentException(string.Format("Expecting {0} inputs for this net", net.m_inputLayer.Count));

            // initialize data
            for (i = 0; i < net.m_inputLayer.Count; i++)
                net.m_inputLayer[i].Output = input[i];

            #endregion

        }

        public static void CalculateAndAppendTransformation(NeuralNet net)
        {
            #region Declarations

            int i, j;
            INeuron outputNode, inputNode, hiddenNode;

            #endregion

            #region Execution

            // adjust output layer weight change
            for (j = 0; j < net.m_outputLayer.Count; j++)
            {
                outputNode = net.m_outputLayer[j];

                for (i = 0; i < net.m_hiddenLayer.Count; i++)
                {
                    hiddenNode = net.m_hiddenLayer[i];
                    outputNode.Input[hiddenNode].H_Vector += outputNode.Error * hiddenNode.Output;
                }

                outputNode.Bias.H_Vector += outputNode.Error * outputNode.Bias.Weight;
            }

            // adjust hidden layer weight change
            for (j = 0; j < net.m_hiddenLayer.Count; j++)
            {
                hiddenNode = net.m_hiddenLayer[j];

                for (i = 0; i < net.m_inputLayer.Count; i++)
                {
                    inputNode = net.m_inputLayer[i];
                    hiddenNode.Input[inputNode].H_Vector += hiddenNode.Error * inputNode.Output;
                }

                hiddenNode.Bias.H_Vector += hiddenNode.Error * hiddenNode.Bias.Weight;
            }

            #endregion
        }


        #region Backprop

        public static void BackPropogation_TrainingSession(NeuralNet net, double[] input, double[] desiredResult)
        {
            PreparePerceptionLayerForPulse(net, input);
            net.Pulse();
            CalculateErrors(net, desiredResult);
            CalculateAndAppendTransformation(net);
        }

        #endregion

        #endregion Private Static Utility Methods -------------------------------------------
        
        #endregion

    }
}