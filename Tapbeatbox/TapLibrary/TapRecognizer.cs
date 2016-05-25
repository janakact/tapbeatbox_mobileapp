using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapbeatbox.TapLibrary
{
    public class TapRecognizer
    {
        private List<ToneSlot> listOfSlots;
        NeuralNet net;

        double high, mid, low;

        public TapRecognizer(List<ToneSlot> slots)
        {
            listOfSlots = slots;

            high = .99;
            low = .01;
            mid = .5;


            net = new NeuralNet();
            
        }


        //Get the maximum value
        public int recognizeTheSlot(double[] parms)
        {

            System.Diagnostics.Debug.WriteLine("Start Recognition");
            for (int i=0; i<Constant.parmCount; i++)
                net.PerceptionLayer[i].Output = parms[i];


            net.Pulse();

            int ret = 0;
            for(int i=1; i<Constant.parmCount; i++)
            {
                if (net.OutputLayer[i].Output > net.OutputLayer[ret].Output)
                    ret = i;
            }

            System.Diagnostics.Debug.WriteLine("Finished Recognition");
            return ret;
        }

        public void train()
        {
            //Initialize the nural net
            net.Initialize(1, Constant.parmCount, Constant.parmCount, listOfSlots.Count);

            List<double[]> inputList = new List<double[]>(); //All the inpust and outputs
            List<double[]> outputList = new List<double[]>();
            #region Setup Training Data
            foreach(ToneSlot t in listOfSlots)
            {
                double[] outputValues = new double[listOfSlots.Count]; //Each slot contain a set of data. 
                outputValues[t.ID] = high; // For all those data , output will be same. the slot id
                foreach(double[] d in t.trainingDataSet)
                {
                    outputList.Add(outputValues);
                    inputList.Add(d);
                }
            }

            double[][] input = inputList.ToArray<double[]>();
            double[][] output = outputList.ToArray<double[]>();
            #endregion

            #region TrainingSection
            System.Diagnostics.Debug.WriteLine("Start Training");
            net.Train(input, output, TrainingType.BackPropogation, Constant.neuralNetFeedCount);
            System.Diagnostics.Debug.WriteLine("Finished Training");
            #endregion

        }



    }
}
