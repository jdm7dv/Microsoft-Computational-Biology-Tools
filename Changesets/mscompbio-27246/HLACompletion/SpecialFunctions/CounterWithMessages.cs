using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{ 
    public class CounterWithMessages 
    {
        private CounterWithMessages() 
        {
        }

        private string FormatString;
        private int MessageInterval;
        public int Index { get; private set; } 
        private int? CountOrNull; 

        public static CounterWithMessages GetInstance(string formatString, int messageInterval, int? totalCountOrNull) 
        {
            CounterWithMessages counter = new CounterWithMessages();
            counter.FormatString = formatString;
            counter.MessageInterval = messageInterval;
            counter.Index = -1;
            counter.CountOrNull = totalCountOrNull; 
            return counter; 
        }
 
        public void Increment()
        {
            lock (this)
            {
                ++Index;
                if (Index % MessageInterval == 0) 
                { 
                    if (null == CountOrNull)
                    { 
                        Console.WriteLine(FormatString, Index);
                    }
                    else
                    {
                        Console.WriteLine(FormatString, Index, CountOrNull.Value);
                    } 
                } 
            }
        } 
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
