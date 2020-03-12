using System; 
using System.Collections.Generic;
using System.Text;

namespace VirusCount.PhyloTree
{
    public interface IMessage { } 
 
    /// <summary>
    /// A wrapper around the traditional double[] to allow polymorphism with the Gaussian message. 
    /// </summary>
    public class MessageDiscrete : IMessage
    {
        public double[] P;

        protected MessageDiscrete(double[] p) 
        { 
            P = p;
        } 

        public static MessageDiscrete GetInstance(double[] p)
        {
            return new MessageDiscrete(p);
        }
    } 
 

    /// <summary>
    /// This will change when Gaussian is implemented
    /// </summary>
    public class MessageGaussian : IMessage
    { 
        private MessageGaussian()
        {
        }

        private MessageGaussian(double logK, double a, double v)
        { 
            LogK = logK; 
            A = a;
            V = v; 
        }

        static public MessageGaussian GetInstance(double logK, double a, double v)
        {
            return new MessageGaussian(logK, a, v);
        } 
 
        public readonly double LogK;     // porportionality constant
        public readonly double A; 
        public readonly double V;

    }


} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
