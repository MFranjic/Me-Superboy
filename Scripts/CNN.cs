using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class CNN : MonoBehaviour {

    public int originalResolution = 128;

    public bool initialization = false;
    public bool training = true;

    public string FirstConvolutionLayerFilters = "ConvLayer1Filters";
    public string SecondConvolutionLayerFilters = "ConvLayer2Filters";
    public string InputLayerFilters = "InputLayerFilters";
    public string FullyConnectedLayerWeights = "FCLayerWeights";
    public string OutputLayerWeights = "OutputLayerWeights";

    public bool Dalekozor = true;
    public bool Ljestve = false;
    public bool Laso = false;
    public bool Ormar = false;
    public bool Životinja = false;
    public bool Pijuk = false;

    public double LearningRate = 0.1;

    private int inputResolution = 32;
    private int convFiltersResolution = 5;
    private int poolFieldResolution = 2;

    private double[] goal;

    void Start() {

    }

    public double[] test(int[,] inputImage)
    {
        double[,] input = AdaptResolution(inputImage, originalResolution, inputResolution);
        int inputRes = inputResolution;                                                                                         // inputRes = 32

        /*Debug.Log(inputRes);
        string inp = "";
        for (int i = 0; i < inputRes; i++)
        {
            for (int j = 0; j < inputRes; j++)
            {
                Debug.Log(i + " " + j + " " + input[i, j]);
                inp += input[i, j] + " ";
            }
            inp += "End\n";
        }
        Debug.Log(inp);*/

        ConvolutionalLayer layer1 = new ConvolutionalLayer(input, inputRes, 1, 6, convFiltersResolution, FirstConvolutionLayerFilters, initialization);
        inputRes = inputRes - convFiltersResolution + 1;                                                                        // inputRes = 28

        MaxpoolLayer layer2 = new MaxpoolLayer(layer1.Convolution(), inputRes, 6, poolFieldResolution);
        inputRes /= 2;                                                                                                          // inputRes = 14

        ConvolutionalLayer layer3 = new ConvolutionalLayer(layer2.Subsample(), inputRes, 6, 16, convFiltersResolution, SecondConvolutionLayerFilters, initialization);
        inputRes = inputRes - convFiltersResolution + 1;                                                                        // inputRes = 10

        MaxpoolLayer layer4 = new MaxpoolLayer(layer3.Convolution(), inputRes, 16, poolFieldResolution);
        inputRes /= 2;                                                                                                          // inputRes = 5

        InputLayer layer5 = new InputLayer(layer4.Subsample(), inputRes, 72, InputLayerFilters, initialization);

        FullyConnectedLayer layer6 = new FullyConnectedLayer(layer5.Flatten(), 72, 50, FullyConnectedLayerWeights, initialization);

        FullyConnectedLayer final = new FullyConnectedLayer(layer6.Calculate(), 50, 6, OutputLayerWeights, initialization);
        //final.Calculate();
        double[] output = final.Calculate();
        SetGoal(output);
        double[] loss = LossFunction(output, goal);
        double[] results = GetResults(output);
        Debug.Log("Output: " + output[0] + ", " + output[1] + ", " + output[2]);
        Debug.Log("Loss: " + loss[0] + ", " + loss[1] + ", " + loss[2]);
        Debug.Log("Rezultat: " + results[0].ToString("#.000") + "%, " + results[1].ToString("#.000") + "%, " + results[2].ToString("#.000") + "%");

        if (training)
        {
            double[] dx = final.Backpropagation(loss, LearningRate);
            dx = layer6.Backpropagation(dx, LearningRate);
            double[,,] convdx = layer5.Backpropagation(dx, LearningRate);
            convdx = layer4.Backpropagation(convdx);
            convdx = layer3.Backpropagation(convdx, LearningRate);
            convdx = layer2.Backpropagation(convdx);
            convdx = layer1.Backpropagation(convdx, LearningRate);
        }
        return output;
    }

    private double[] GetResults(double[] results)
    {
        int resultSize = 3;
        double[] percentages = new double[resultSize];
        for (int i = 0; i < resultSize; i++)
        {
            percentages[i] = results[i] / 1.7159 * 100;
            if (percentages[i] < 0)
            {
                percentages[i] = 0;
            }
        }
        return percentages;
    }

    private void Backpropagation(double loss)
    {

    }

    private double[] LossFunction(double[] output, double[] goal)
    {
        double[] lossDerivative = new double[3];
        double loss = 0;
        for (int i = 0; i < 3; i++)
        {
            //Debug.Log(Math.Pow(output[i] - goal[i], 2));
            loss += Math.Pow(output[i] - goal[i], 2) / 2;
            lossDerivative[i] = output[i] - goal[i];
        }
        return lossDerivative;
    }

    private void SetGoal(double[] output)
    {
        double max = 0;
        for (int i = 0; i < 3; i++)
        {
            max += output[i];
        }
        if (Dalekozor)
            goal = new double[] { 1.7159, 0.0, 0.0 };
        else if (Ljestve)
            goal = new double[] { 0.0, 1.7159, 0.0 };
        else if (Laso)
            goal = new double[] { 0.0, 0.0, 1.7159 };
    }

    private double[,] AdaptResolution(int[,] input, int originalResolution, int newResolution)
    {
        int stride = originalResolution / newResolution;
        double[,] newInput = new double[newResolution, newResolution];

        for (int i = 0; i < newResolution; i++)
        {
            for (int j = 0; j < newResolution; j++)
            {
                newInput[i, j] = -0.1;
                for (int x = 0; x < stride; x++)
                {
                    for (int y = 0; y < stride; y++)
                    {
                        if (input[i * stride + x, j * stride + y] == 1)
                            newInput[i, j] = 1.17;
                        //newInput[i, j] += (double)input[i * stride + x, j * stride + y];
                    }
                }
            }
        }
        return newInput;
    }

    private class Filter                    // or neuron, kernel
    {
        public int resolution;
        public double [,,] parameters;           // or weights
        public int depth;
        public int[] inputNums;

        private int inputSize;
        private int resultSize;

        public Filter(string filePath, bool initialization, int resolution, int depth, int[] inputNums, int inputSize, int resultSize)             // file-writing constructor
        {
            this.inputNums = inputNums;
            parameters = new double[depth, resolution, resolution];
            this.depth = depth;
            this.resolution = resolution;
            this.inputSize = inputSize;
            this.resultSize = resultSize;

            System.Random randomWeight = new System.Random();
            StreamWriter writer = new StreamWriter(filePath, true);
            string input = "";
            for (int d = 0; d < depth; d++)
            {
                for (int i = 0; i < resolution; i++)                
                {
                    for (int j = 0; j < resolution; j++)
                    {
                        //parameters[d, i, j] = (randomWeight.NextDouble() * (double)(inputSize - resultSize) + (double)resultSize) * Math.Sqrt(1.0 / (double)inputSize);
                        parameters[d, i, j] = (randomWeight.NextDouble() * 2.0 - 1.0) * Math.Sqrt(1.0 / (double)inputSize);
                        if (j < resolution - 1)
                            input += parameters[d, i, j].ToString("0.00000") + " ";
                        else
                            input += parameters[d, i, j].ToString("0.00000");
                    }
                    if (i < resolution - 1)
                        input += "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||";
                    else
                        input += "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||-----------------------------------------------------------------";
                }
                if (d < depth - 1)
                    input += "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++";
            }
            writer.WriteLine(input);
            writer.Close();
            AssetDatabase.ImportAsset(filePath);
        }

        public Filter(string data, int resolution, int depth, int[] inputNums)                                      // file-reading constructor
        {
            this.inputNums = inputNums;
            parameters = new double[depth, resolution, resolution];
            this.depth = depth;
            this.resolution = resolution;

            string[] filters = data.Split(new string[] { "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++" }, StringSplitOptions.None);
            for (int d = 0; d < depth; d++)
            {
                string[] rows = filters[d].Split(new string[] { "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||" }, StringSplitOptions.None);
                for (int i = 0; i < resolution; i++)  
                {
                    string[] weights = rows[i].Split(' ');
                    for (int j = 0; j < resolution; j++)
                    {
                        parameters[d, i, j] = Convert.ToDouble(weights[j]);
                    }
                }
            }
        }

        public string toString()
        {
            string input = "";
            for (int d = 0; d < depth; d++)
            {
                for (int i = 0; i < resolution; i++)
                {
                    for (int j = 0; j < resolution; j++)
                    {
                        if (j < resolution - 1)
                            input += parameters[d, i, j].ToString("0.00000") + " ";
                        else
                            input += parameters[d, i, j].ToString("0.00000");
                    }
                    if (i < resolution - 1)
                        input += "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||";
                    else
                        input += "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||-----------------------------------------------------------------";
                }
                if (d < depth - 1)
                    input += "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++";
            }
            input += "\n";
            return input;
        }
    }

    private class MaxpoolLayer
    {
        private int stride;                         // receptive field resolution = stride = 2
        private double[,,] inputs;
        private int inputsResolution;
        private int inputDepth;
        private double[,,] results;

        private double[,,] dh;
        private double[,,] dx;

        public MaxpoolLayer(double[,,] inputs, int inputsResolution, int inputDepth, int poolFieldResolution)
        {
            this.inputs = inputs;
            this.inputsResolution = inputsResolution;
            this.inputDepth = inputDepth;
            this.stride = poolFieldResolution;
        }

        public double[,,] Subsample()
        {
            results = generateResults();
            return results;
        }

        public double[,,] Backpropagation(double[,,] dh)
        {
            this.dh = dh;

            dx = new double[inputDepth, inputsResolution, inputsResolution];
            for (int d = 0; d < inputDepth; d++)
            {
                for (int x = 0; x < inputsResolution / 2; x ++)
                {
                    for (int y = 0; y < inputsResolution / 2; y++)
                    {
                        if (inputs[d, x * 2, y * 2] == results[d, x, y])
                            dx[d, x * 2, y * 2] = dh[d, x, y];
                        else if (inputs[d, x * 2 + 1, y * 2] == results[d, x, y])
                            dx[d, x * 2 + 1, y * 2] = dh[d, x, y];
                        else if (inputs[d, x * 2, y * 2 + 1] == results[d, x, y])
                            dx[d, x * 2, y * 2 + 1] = dh[d, x, y];
                        else if (inputs[d, x * 2 + 1, y * 2 + 1] == results[d, x, y])
                            dx[d, x * 2 + 1, y * 2 + 1] = dh[d, x, y];
                    }
                }
            }

            return dx;
        }

        private double[,,] generateResults()
        {
            double[,,] results = new double[inputDepth, inputsResolution / stride, inputsResolution / stride];

            for(int d = 0; d < inputDepth; d++)
            {
                double[,] input = new double[inputsResolution, inputsResolution];
                for (int i = 0; i < inputsResolution; i++)
                {
                    for (int j = 0; j < inputsResolution; j++)
                    {
                        input[i, j] = inputs[d, i, j];
                    }
                }

                for (int i = 0; i < inputsResolution / stride; i++)
                {
                    for (int j = 0; j < inputsResolution / stride; j++)
                    {
                        results[d, i, j] = findMax(input, i*stride, j*stride, stride);
                    }
                }
            }
            return results;
        }

        private double findMax (double[,] featureMap, int x, int y, int stride) 
        {
            double max = 0;
            for(int i = 0; i < stride; i++)
            {
                for (int j = 0; j < stride; j++)
                {
                    if (featureMap[x + i, y + j] > max)
                        max = featureMap[x + i, y + j];
                }
            }
            return max;
        }
    }

    private class ConvolutionalLayer
    {
        public int receptiveFieldX = 0;         // left coo
        public int receptiveFieldY = 0;         // top coo

        private int stride;
        private double[,,] input;
        private int inputDim;
        private int numInputs;
        private int filterDim;
        private int numFilters;

        private string filePath;

        private double[,,] dh;
        private double[,,] dx;

        private LinkedList<Filter> filters;
        private double[,,] results;

        public ConvolutionalLayer(double[,] input, int inputDim, int numInputs, int numFilters, int filterDim, string filtersFileName, bool initialization)         // 1st conv layer
        {
            this.input = new double[1, inputDim, inputDim];
            for (int i = 0; i < inputDim; i++)
            {
                for (int j = 0; j < inputDim; j++)
                {
                    this.input[0, i, j] = input[i, j];
                }
            }
            this.inputDim = inputDim;
            this.numInputs = numInputs;
            this.numFilters = numFilters;
            this.filterDim = filterDim;

            filters = new LinkedList<Filter>();
            filePath = "Assets/Resources/" + filtersFileName + ".txt";

            if (initialization)
            {
                for (int i = 0; i < numFilters; i++)
                {
                    filters.AddLast(new Filter(filePath, initialization, filterDim, 1, new int[] { 0 }, inputDim, inputDim - filterDim + 1));
                }
            }
            else
            {
                StreamReader reader = new StreamReader(filePath);
                String output = reader.ReadToEnd();
                String[] filtersData = output.Split('\n');
                for (int i = 0; i < numFilters; i++)
                {                  
                    filters.AddLast(new Filter(filtersData[i], filterDim, 1, new int[] { 0 }));
                }
            }
        }

        public ConvolutionalLayer(double[,,] input, int inputDim, int numInputs, int numFilters, int filterDim, string filtersFileName, bool initialization)        // 2nd conv layer
        {
            this.input = input;
            this.inputDim = inputDim;
            this.numInputs = numInputs;
            this.numFilters = numFilters;
            this.filterDim = filterDim;

            filters = new LinkedList<Filter>();
            filePath = "Assets/Resources/" + filtersFileName + ".txt";

            if(initialization)
            {
                filters.AddLast(new Filter(filePath, initialization, filterDim, 6, new int[] { 0, 1, 2, 3, 4, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 0, 1, 2, 3 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 1, 2, 3, 4 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 2, 3, 4, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 0, 3, 4, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 0, 1, 4, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 0, 1, 2, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 0, 1, 3, 4 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 1, 2, 4, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 4, new int[] { 0, 2, 3, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 3, new int[] { 0, 1, 2 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 3, new int[] { 1, 2, 3 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 3, new int[] { 2, 3, 4 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 3, new int[] { 3, 4, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 3, new int[] { 0, 4, 5 }, inputDim, inputDim - filterDim + 1));
                filters.AddLast(new Filter(filePath, initialization, filterDim, 3, new int[] { 0, 1, 5 }, inputDim, inputDim - filterDim + 1));
            }
            else
            {
                StreamReader reader = new StreamReader(filePath);
                String output = reader.ReadToEnd();
                String[] filtersData = output.Split('\n');

                filters.AddLast(new Filter(filtersData[0], filterDim, 6, new int[] { 0, 1, 2, 3, 4, 5 }));
                filters.AddLast(new Filter(filtersData[1], filterDim, 4, new int[] { 0, 1, 2, 3 }));
                filters.AddLast(new Filter(filtersData[2], filterDim, 4, new int[] { 1, 2, 3, 4 }));
                filters.AddLast(new Filter(filtersData[3], filterDim, 4, new int[] { 2, 3, 4, 5 }));
                filters.AddLast(new Filter(filtersData[4], filterDim, 4, new int[] { 0, 3, 4, 5 }));
                filters.AddLast(new Filter(filtersData[5], filterDim, 4, new int[] { 0, 1, 4, 5 }));
                filters.AddLast(new Filter(filtersData[6], filterDim, 4, new int[] { 0, 1, 2, 5 }));
                filters.AddLast(new Filter(filtersData[7], filterDim, 4, new int[] { 0, 1, 3, 4 }));
                filters.AddLast(new Filter(filtersData[8], filterDim, 4, new int[] { 1, 2, 4, 5 }));
                filters.AddLast(new Filter(filtersData[9], filterDim, 4, new int[] { 0, 2, 3, 5 }));
                filters.AddLast(new Filter(filtersData[10], filterDim, 3, new int[] { 0, 1, 2 }));
                filters.AddLast(new Filter(filtersData[11], filterDim, 3, new int[] { 1, 2, 3 }));
                filters.AddLast(new Filter(filtersData[12], filterDim, 3, new int[] { 2, 3, 4 }));
                filters.AddLast(new Filter(filtersData[13], filterDim, 3, new int[] { 3, 4, 5 }));
                filters.AddLast(new Filter(filtersData[14], filterDim, 3, new int[] { 0, 4, 5 }));
                filters.AddLast(new Filter(filtersData[15], filterDim, 3, new int[] { 0, 1, 5 }));
            }
        }

        public double[,,] Convolution()
        {
            results = getResults();
            return results;
        }

        public double[,,] Backpropagation (double[,,] dh, double learningRate)
        {
            this.dh = dh;

            dx = new double[numInputs, inputDim, inputDim];
            int dhNum = 0;
            for (int inputs = 0; inputs < numInputs; inputs++)
            {
                dhNum = 0;
                foreach (Filter filter in filters)
                {
                    for (int d = 0; d < filter.depth; d++)
                    {
                        if (filter.inputNums[d] == inputs)
                        {
                            double[,] partdx = invertConvolution(filter, d, dh, dhNum);
                            for (int i = 0; i < inputDim; i++)
                            {
                                for (int j = 0; j < inputDim; j++)
                                {
                                    dx[inputs, i, j] += partdx[i, j];
                                }
                            }
                        }
                    }
                    dhNum++;
                }               
            }

            dhNum = 0;
            foreach (Filter filter in filters)
            {
                for (int d = 0; d < filter.depth; d++)
                {
                    double[,] filterConv = filterConvolution(dh, dhNum, filter.inputNums[d], learningRate);
                    for (int i = 0; i < filterDim; i++)
                    {
                        for (int j = 0; j < filterDim; j++)
                        {
                            filter.parameters[d, i, j] += filterConv[i, j];
                        }
                    }
                }
                dhNum++;
            }

            updateFilters();

            return dx;
        }

        private double[,] invertConvolution(Filter filter, int depth, double[,,] dh, int dhNum)
        {
            double[,] partdx = new double[inputDim, inputDim];

            double[,] tempDh = new double[inputDim + filterDim - 1, inputDim + filterDim - 1];
            for (int i = 0; i < inputDim - filterDim + 1; i++)
            {
                for (int j = 0; j < inputDim - filterDim + 1; j++)
                {
                    tempDh[i + filterDim - 1, j + filterDim - 1] = dh[dhNum, i, j];
                }
            }
            double[,] tempFilter = new double[filterDim, filterDim];
            for (int i = 0; i < filterDim; i++)
            {
                for (int j = 0; j < filterDim; j++)
                {
                    tempFilter[filterDim - 1 - i, filterDim - 1 - j] = filter.parameters[depth, i, j];
                }
            }

            // multiplication
            for (int i = 0; i < inputDim; i++)
            {
                for (int j = 0; j < inputDim; j++)
                {
                    for (int x = 0; x < filterDim; x++)
                    {
                        for (int y = 0; y < filterDim; y++)
                        {
                            partdx[i, j] += tempDh[i + x, j + y] * tempFilter[x, y];
                        }
                    }
                }
            }
            return partdx;
        }

        private double[,] filterConvolution(double[,,] dh, int dhNum, int inputNum, double learningRate)
        {
            double[,] partfilter = new double[filterDim, filterDim];

            for (int i = 0; i < filterDim; i++)
            {
                for (int j = 0; j < filterDim; j++)
                {
                    for (int x = 0; x < inputDim - filterDim + 1; x++)
                    {
                        for (int y = 0; y < inputDim - filterDim + 1; y++)
                        {
                            partfilter[i, j] -= learningRate * input[inputNum, x + i, y + j] * dh[dhNum, x, y];
                        }
                    }
                }
            }

            return partfilter;
        }

        private double[,,] getResults()
        {
            double[,,] results = new double[numFilters, inputDim - filterDim + 1, inputDim - filterDim + 1];

            int d = 0;
            foreach (Filter filter in filters)
            {               
                double[,] result = generateResult(input, inputDim, filter, filterDim);

                for (int i = 0; i < inputDim - filterDim + 1; i++)
                {
                    for (int j = 0; j < inputDim - filterDim + 1; j++)
                    {
                        results[d, i, j] = result[i, j];
                    }
                }              
                d++;
            }
            return results;
        }

        private void updateFilters()
        {
            StreamWriter writer = new StreamWriter(filePath, false);

            string input = "";
            foreach(Filter filter in filters)
            {
                input += filter.toString();
            }
            writer.WriteLine(input);
            writer.Close();
            AssetDatabase.ImportAsset(filePath);
        }

        private double[,] generateResult(double[,,] input, int inputDim, Filter filter, int filterDim)
        {
            double[,] result = new double[inputDim - filterDim + 1, inputDim - filterDim + 1];
            double[,,] inputPart = new double[filter.depth, filterDim, filterDim];

            for (int i = 0; i < inputDim - filterDim + 1; i++)
            {
                for (int j = 0; j < inputDim - filterDim + 1; j++)
                {
                    for (int d = 0; d < filter.depth; d++)
                    {
                        for (int x = 0; x < filterDim; x++)
                        {
                            for (int y = 0; y < filterDim; y++)
                            {
                                inputPart[d, x, y] = input[filter.inputNums[d], i + x, j + y];
                            }
                        }
                        
                    }
                    result[i, j] = multiplication(inputPart, filter.parameters, filterDim, filter.depth);
                    /*if (result[i, j] < 0)
                    {
                        result[i, j] = 0;                                                   // RELU as activation function
                    }*/
                    //result[i, j] = Math.Tanh(result[i, j]);                               // Tanh as activation function
                    result[i, j] = 1.7159 * Math.Tanh((2.0 / 3.0) * result[i, j]);        // 1.7159 Tanh as activation function
                }
            }
            return result;
        }

        private double multiplication(double[,,] input, double[,,] filter, int dim, int depth)
        {
            double result = 0;

            for (int d = 0; d < depth; d++)
            {
                for (int i = 0; i < dim; i++)
                {
                    for (int j = 0; j < dim; j++)
                    {
                        result += input[d, i, j] * filter[d, i, j];    
                    }
                }
            }
            return result;
        }
    }

    private class InputLayer
    {
        private double[,,] input;
        private int inputDim;
        private int filterDim;
        private int numFilters;

        private LinkedList<Filter> filters;
        private double[] results;

        private string filePath;

        private double[] dh;
        private double[,,] dx;

        public InputLayer(double [,,] input, int inputDim, int numFilters, string filtersFileName, bool initialization)
        {
            this.input = input;
            this.inputDim = inputDim;
            this.filterDim = inputDim;
            this.numFilters = numFilters;

            filters = new LinkedList<Filter>();
            this.filePath = "Assets/Resources/" + filtersFileName + ".txt";

            if (initialization)
            {
                for (int i = 0; i < numFilters; i++)
                {
                    filters.AddLast(new Filter(filePath, initialization, filterDim, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, inputDim, numFilters));
                }
            }
            else
            {
                StreamReader reader = new StreamReader(filePath);
                String output = reader.ReadToEnd();
                String[] filtersData = output.Split('\n');

                for (int i = 0; i < numFilters; i++)
                {
                    filters.AddLast(new Filter(filtersData[i], filterDim, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));
                }
            }
        }

        public double[,,] Backpropagation(double[] dh, double learningRate)
        {
            this.dh = dh;

            dx = new double[16, inputDim, inputDim];

            int filterNum = 0;
            foreach (Filter filter in filters)
            {
                for (int d = 0; d < 16; d++)
                { 
                    for (int i = 0; i < inputDim; i++)
                    {
                        for (int j = 0; j < inputDim; j++)
                        {
                            dx[d, i, j] += dh[filterNum] * filter.parameters[0, i, j];
                        }
                    }
                }
                filterNum++;
            }

            filterNum = 0;
            foreach (Filter filter in filters)
            {
                for (int d = 0; d < 16; d++)
                {
                    for (int i = 0; i < inputDim; i++)
                    {
                        for (int j = 0; j < inputDim; j++)
                        {
                            filter.parameters[0, i, j] -= learningRate * dh[filterNum] * input[d, i, j];
                        }
                    }
                }
                filterNum++;
            }
            //Debug.Log("Updating filters");
            updateFilters();

            return dx;
        }

        public double[] Flatten()
        {
            results = flattenInput();
            return results;
        }

        private void updateFilters()
        {
            StreamWriter writer = new StreamWriter(filePath, false);

            string input = "";
            foreach (Filter filter in filters)
            {
                input += filter.toString();
            }
            writer.WriteLine(input);
            writer.Close();
            AssetDatabase.ImportAsset(filePath);
        }

        private double[] flattenInput()
        {
            double[] results = new double[numFilters];
            int filterNum = 0;
            string temp = "";
            foreach(Filter filter in filters)
            {
                results[filterNum] = 0;
                for (int d = 0; d < 16; d++)
                {
                    for (int i = 0; i < inputDim; i++)
                    {
                        for (int j = 0; j < inputDim; j++)
                        {
                            results[filterNum] += input[d, i, j] * filter.parameters[0, i, j];
                        }
                    }
                }
                /*if (results[filterNum] < 0)
                {
                    results[filterNum] = 0;                                                     // RELU as activation function
                }*/
                //results[filterNum] = Math.Tanh(results[filterNum]);                           // Tanh as activation function
                results[filterNum] = 1.7159 * Math.Tanh((2.0 / 3.0) * results[filterNum]);      // 1.7159 Tanh as activation function
                temp += results[filterNum] + " ";
                filterNum++;        
                
            }
            //Debug.Log(temp);
            return results;
        }
    }

    private class FullyConnectedLayer
    {
        private double[] input;
        private int inputSize;
        private double[] bias;
        private double[] results;
        private int resultSize;
        private double[,] weights;

        private string filePath;

        private double[] dh;
        private double[] dx;

        public FullyConnectedLayer(double[] input, int inputSize, int resultSize, string filtersFileName, bool initialization)
        {
            this.input = input;
            this.inputSize = inputSize;
            this.resultSize = resultSize;
            this.bias = new double[resultSize];

            this.filePath = "Assets/Resources/" + filtersFileName + ".txt";
            if (initialization)
            {
                weights = initializeWeights();
            }
            else
            {
                weights = fetchWeights();
            }
        }

        public double[] Backpropagation(double[] dh, double learningRate)
        {
            this.dh = dh;

            dx = new double[inputSize];
            for (int i = 0; i < inputSize; i++)
            {
                dx[i] = 0;
                for (int j = 0; j < resultSize; j++)
                {
                    dx[i] += dh[j] * weights[j, i];
                }
            }            

            for (int i = 0; i < resultSize; i++)                        
            {
                for (int j = 0; j < inputSize; j++)
                {
                    weights[i, j] -= learningRate * dh[i] * input[j];               // weights update                  
                }
                bias[i] -= learningRate * dh[i] * 1.0;                              // bias update
            }
            updateWeights();

            return dx;
        }

        public double[] Calculate()
        {
            results = doCalculation();
            return results;
        }

        private double[,] fetchWeights()
        {
            double[,] weights = new double[resultSize, inputSize];
            StreamReader reader = new StreamReader(filePath);
            string output = reader.ReadToEnd();
            string[] rows = output.Split(new string[] { "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||" }, StringSplitOptions.None);

            for (int i = 0; i < resultSize; i++)
            {
                string[] weightsData = rows[i].Split(' ');
                for (int j = 0; j < inputSize; j++)
                {
                    weights[i, j] = Convert.ToDouble(weightsData[j]);
                }
                bias[i] = Convert.ToDouble(weightsData[inputSize]);
            }
            reader.Close();
            return weights;
        }

        private double[,] initializeWeights()
        {
            double[,] weights = new double[resultSize, inputSize];
            StreamWriter writer = new StreamWriter(filePath, true);
            System.Random randomBias = new System.Random();
            System.Random randomWeight = new System.Random();

            string input = "";       
            for (int i = 0; i < resultSize; i++)
            {
                for (int j = 0; j < inputSize; j++)
                {
                    //weights[i, j] = (randomWeight.NextDouble() * (double)(inputSize - resultSize) + (double)resultSize) * Math.Sqrt(1.0 / (double)inputSize);
                    weights[i, j] = (randomWeight.NextDouble() * 2.0 - 1.0) * Math.Sqrt(1.0 / (double)inputSize);
                    input += weights[i, j].ToString("0.00000") + " ";                    
                }
                bias[i] = randomBias.NextDouble();
                input += bias[i].ToString("0.00000");
                if (i < resultSize - 1)
                    input += "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||";
            }
            writer.WriteLine(input);
            writer.Close();
            AssetDatabase.ImportAsset(filePath);
            return weights;
        }

        private void updateWeights()
        {
            StreamWriter writer = new StreamWriter(filePath, false);

            string input = "";
            for (int i = 0; i < resultSize; i++)
            {
                for (int j = 0; j < inputSize; j++)
                {
                    input += weights[i, j].ToString("0.00000") + " ";
                }
                input += bias[i].ToString("0.00000");
                if (i < resultSize - 1)
                    input += "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||";
            }
            writer.WriteLine(input);
            writer.Close();
            AssetDatabase.ImportAsset(filePath);
        }

        private double[] doCalculation()
        {
            double[] results = new double[resultSize];
            string temp = "";
            double sum = 0;
            for (int i = 0; i < resultSize; i++)
            {
                results[i] = 0;
                for (int j = 0; j < inputSize; j++)
                {
                    results[i] += input[j] * weights[i, j];                   
                }
                results[i] += bias[i];
                /*if (results[i] < 0)
                {
                    results[i] = 0;                                             // RELU as activation function
                }*/
                //results[i] = Math.Tanh(results[i]);                           // Tanh as activation function
                results[i] = 1.7159 * Math.Tanh((2.0 / 3.0) * results[i]);      // 1.7159 Tanh as activation function
                temp += results[i] + " ";
                sum += results[i];
            }

            //Debug.Log(temp);
            return results;
        }
    }

    /*Filter filterCurve1 = new Filter(filter1circle, filtersResolution);
    layer1.addFilter(filterCurve1);
    Filter filterCurve2 = new Filter(filter2circle, filtersResolution);
    layer1.addFilter(filterCurve2);
    Filter filterCurve3 = new Filter(filter3circle, filtersResolution);
    layer1.addFilter(filterCurve3);
    Filter filterCurve4 = new Filter(filter4circle, filtersResolution);
    layer1.addFilter(filterCurve4);
    layer1.Convolution();

    LinkedList<int[,]> results = layer1.getResults();*/

    /*private int[,] filter1circle = new int[,]    {{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10 },
                                                  { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10 },
                                                  { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 0, 0 },
                                                  { 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0, 0, 0 },
                                                  { 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 0, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                  { 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }};

    private int[,] filter2circle = new int[,]  {{ 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 10, 10 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10 } };

    private int[,] filter3circle = new int[,]  {{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 10, 10, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 10, 10, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };

    private int[,] filter4circle = new int[,]  {{ 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 10, 10, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 10, 10, 10, 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 10, 10, 10, 10, 10, 0, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 10, 10, 10, 10, 0, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10, 0 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10 },
                                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 10 } };*/

}