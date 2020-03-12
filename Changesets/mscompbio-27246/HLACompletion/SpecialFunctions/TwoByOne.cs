using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Msr.Mlas.SpecialFunctions
{ 
    public class TwoByOne 
    {
        private TwoByOne() 
        {
        }

        public static TwoByOne GetInstance()
        {
            TwoByOne twoByOne = new TwoByOne(); 
            twoByOne.Total = 0; 
            twoByOne.TrueCount = 0;
            return twoByOne; 
        }

        public static TwoByOne GetInstance<T>(IEnumerable<T> tEnum, Predicate<T> predicate)
        {
            TwoByOne twoByOne = new TwoByOne();
            twoByOne.Total = 0; 
            twoByOne.TrueCount = 0; 
            foreach (T t in tEnum)
            { 
                twoByOne.Add(predicate(t));
            }
            return twoByOne;
        }

        public void Add(bool? testOrNull) 
        { 
            if (testOrNull.HasValue)
            { 
                Add(testOrNull.Value);
            }
        }

        public void Add(bool test)
        { 
            if (test) 
            {
                ++TrueCount; 
            }
            ++Total;
        }

        public int Total { get; private set; }
        public int TrueCount { get; private set; } 
        public double Freq 
        {
            get 
            {
                return (double)TrueCount / (double)Total;
            }
        }

        public double CompFreq 
        { 
            get
            { 
                return 1.0 - Freq;
            }
        }
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
