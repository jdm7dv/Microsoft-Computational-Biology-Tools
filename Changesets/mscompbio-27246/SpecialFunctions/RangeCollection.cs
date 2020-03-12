using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;

// !!!!  here is a bug on rangecollection to fix sometime 
//  Add these in this order 1,2,3,4,5,6,7,8,9,0 and then look at the result. It is 0,1-9, not the expected 0-9 

namespace Msr.Mlas.SpecialFunctions 
{
    public class RangeCollection
    {

        private List<int> StartItems;
        private SortedDictionary<int, int> ItemToLength; 
 
        private RangeCollection()
        { 
            StartItems = new List<int>();
            ItemToLength = new SortedDictionary<int, int>();
        }

        public static RangeCollection GetInstance()
        { 
            return new RangeCollection(); 
        }
 
        public static RangeCollection GetInstance(int begin, int end)
        {
            return Parse(begin + "-" + end);
        }

        /// <summary> 
        /// Parses strings of the form -10--5,-2-10,12-12 . Spaces are allowed, no other characters are. 
        /// If mergeOverlappingRanges, then, for example, 2-3,4-5 is represented
        /// as 2-5. Otherwise, they're maintained as separate ranges. The only difference is in the behavior of the ToString() call. 
        /// By extension, this will change how a RangeCollection is parsed into a RangeCollectionCollection using the latter's
        /// GetInstance(RangeCollection) initializer.
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static RangeCollection Parse(string ranges) 
        { 
            return Parse(ranges, true);
        } 

        public static RangeCollection Parse(string ranges, bool mergeOverlappingRanges)
        {
            RangeCollection aRangeCollection = GetInstance();

            int lastBegin = int.MaxValue; 
            int lastEnd = int.MinValue; 

            string[] contiguousRanges = ranges.Split(','); 
            foreach (string r in contiguousRanges)
            {
                string range = r;   // only do this cuz I need to be able to reassign it later and you can't with foreach iterators.

                bool beginIsNegative = false;
                bool endIsNegative = false; 
 
                if (range[0] == '-')
                { 
                    beginIsNegative = true;
                    range = range.Substring(1);
                }
                if (range.IndexOf("--") > 0)
                {
                    endIsNegative = true; 
                    range = range.Replace("--", "-"); 
                }
 
                string[] rangeBeginAndEnd = range.Split('-');

                int begin = int.Parse(rangeBeginAndEnd[0].Trim());
                if (beginIsNegative)
                    begin *= -1;
 
                int end; 
                if (rangeBeginAndEnd.Length == 1)
                { 
                    end = begin;
                    SpecialFunctions.CheckCondition(!endIsNegative, "Ill-formed Range. " + ranges);
                }
                else
                {
                    SpecialFunctions.CheckCondition(rangeBeginAndEnd.Length == 2, "Ill-formed Range. " + ranges); 
                    end = int.Parse(rangeBeginAndEnd[1].Trim()); 
                    if (endIsNegative)
                        end *= -1; 
                }

                SpecialFunctions.CheckCondition(end > lastEnd, "Ill-formed Range. " + ranges);
                SpecialFunctions.CheckCondition(end >= begin, range + " is not a valid range. End is before begin!");

                if (begin > lastEnd + 1 || !mergeOverlappingRanges) // this is the beginning of a new range 
                { 
                    aRangeCollection.StartItems.Add(begin);
                    aRangeCollection.ItemToLength.Add(begin, end - begin + 1); 
                }
                else // we're really just extending the previous range
                {
                    aRangeCollection.ItemToLength[lastBegin] = end - lastBegin + 1;
                }
 
                lastBegin = begin; 
                lastEnd = end;
            } 

            return aRangeCollection;
        }


        public IEnumerable<KeyValuePair<int, int>> Collection 
        { 
            get
            { 
                foreach (int item in StartItems)
                {
                    int last = item + ItemToLength[item] - 1;
                    yield return new KeyValuePair<int, int>(item, last);
                }
            } 
        } 

        /// <summary> 
        /// Returns an enumeration of the integer elements in this RangeCollection.
        /// </summary>
        public IEnumerable<int> Elements
        {
            get
            { 
                foreach (KeyValuePair<int, int> range in Collection) 
                {
                    for (int i = range.Key; i <= range.Value; i++) 
                    {
                        yield return i;
                    }
                }
            }
        } 
 
        public int FirstElement //Used to be MinElement
        { 
            get
            {
                return StartItems[0];
            }
        }
 
        public int LastElement //!!!Used to be MaxElement but it was returning one beyond the last (inclusive) element in the range. Because I didn't know if this was by design or a bug. I changed the name. 
        {
            get 
            {
                return StartItems[StartItems.Count - 1] + ItemToLength[StartItems[StartItems.Count - 1]] - 1;
            }
        }

        public int Count() 
        { 
            return Count(int.MinValue, int.MaxValue);
        } 

        public int Count(int min, int max)
        {
            int count = 0;
            foreach (int start in StartItems)
            { 
                int stop = ItemToLength[start] + start - 1; 

                // truncate start and stop around max. 
                int begin = Math.Max(start, min);
                int end = Math.Min(stop, max);
                int diff = Math.Max(0, end - begin + 1);

                count += diff;
            } 
 
            return count;
 
        }

        /// <summary>
        /// Returnst the number of contiguous ranges in this collection. Useful for memory
        /// consumption debugging.
        /// </summary> 
        public int EntryCount 
        {
            get { return StartItems.Count; } 
        }

        public void Add(int item)
        {
            bool isOK = TryAdd(item);
            //SpecialFunctions.CheckCondition(!isOK); 
        } 

        //!!!!Should this have a name more like "TryAddNewOrOld" ?? 
        public bool TryAdd(int item)
        {
            Debug.Assert(StartItems.Count == ItemToLength.Count); // real assert
            int indexOfMiss = ~StartItems.BinarySearch(item);
            if (indexOfMiss < 0) //Hit a start
            { 
                return false; 
            }
            if (indexOfMiss == 0) 
            {
                StartItems.Insert(indexOfMiss, item);
                ItemToLength.Add(item, 1);
                return true;
            }
 
            int previous = StartItems[indexOfMiss - 1]; 
            int end = previous + ItemToLength[previous];
 
            if (item < end)
            {
                return false;
            }
            else if (end == item)
            { 
                int length = item - previous + 1; 
                Debug.Assert(length > 0); // real assert
                ItemToLength[previous] = length; 
            }
            else
            {
                StartItems.Insert(indexOfMiss, item);
                ItemToLength.Add(item, 1);
                previous = item; 
                ++indexOfMiss; 
            }
            end = item + 1; 

            if (indexOfMiss == StartItems.Count)
            {
                return true;
            }
 
            int next = StartItems[indexOfMiss]; 
            if (end == next)
            { 
                ItemToLength[previous] = ItemToLength[previous] + ItemToLength[next];
                ItemToLength.Remove(next);
                StartItems.RemoveAt(indexOfMiss);
            }

#if DEBUG 
            foreach (KeyValuePair<KeyValuePair<int, int>, KeyValuePair<int, int>> previousStartAndLastAndNextStartAndLast in SpecialFunctions.Neighbors(Collection)) 
            {
                int previousStart = previousStartAndLastAndNextStartAndLast.Key.Key; 
                int previousLast = previousStartAndLastAndNextStartAndLast.Key.Value;
                int nextStart = previousStartAndLastAndNextStartAndLast.Value.Key;
                int nextLast = previousStartAndLastAndNextStartAndLast.Value.Value;

                Debug.Assert(previousLast < nextStart);
            } 
#endif 

            return true; 
        }

        /// <summary>
        /// Returns true iff item is within the ranges of this RangeCollection.
        /// </summary>
        public bool Contains(int item) 
        { 
            int indexOfMiss = StartItems.BinarySearch(item);
            if (indexOfMiss >= 0) // item is the beginning of a range 
                return true;

            indexOfMiss = ~indexOfMiss;

            if (indexOfMiss == 0)   // item is before any of the ranges
                return false; 
 
            int previous = StartItems[indexOfMiss - 1];
            int end = previous + ItemToLength[previous]; 

            return item < end; // we already know it's greater than previous...
        }

        public bool IsBetween(int low, int high)
        { 
            int veryFirstItem = StartItems[0]; 
            Debug.Assert(StartItems.Count == ItemToLength.Count);
            int veryLastItem = StartItems[StartItems.Count - 1] + ItemToLength[veryFirstItem] - 1; 
            bool isBetween = low <= veryFirstItem && veryFirstItem <= high;
            return isBetween;
        }

        public string ToString(string seperator1, string separator2)
        { 
            StringBuilder sb = new StringBuilder(); 
            foreach (KeyValuePair<int, int> startAndLast in Collection)
            { 
                if (sb.Length != 0)
                {
                    sb.Append(separator2);
                }
                if (startAndLast.Key == startAndLast.Value)
                { 
                    sb.Append(startAndLast.Key); 
                }
                else 
                {
                    sb.AppendFormat("{0}{1}{2}", startAndLast.Key, seperator1, startAndLast.Value);
                }
            }
            return sb.ToString();
        } 
 
        public override string ToString()
        { 
            return ToString("-", ",");
        }



        public bool IsComplete(int rowCount) 
        { 
            return IsComplete(0, rowCount - 1);
        } 
        public bool IsComplete(int firstItem, int lastItem)
        {
            bool b = (StartItems.Count == 1) && (StartItems[0] == firstItem) && (ItemToLength[StartItems[0]] == lastItem - firstItem + 1);
            return b;
        }
 
        static public void Test() 
        {
            RangeCollection aRangeCollection = RangeCollection.GetInstance(); 
            aRangeCollection.Add(0);
            SpecialFunctions.CheckCondition("0-0" == aRangeCollection.ToString());
            aRangeCollection.Add(1);
            SpecialFunctions.CheckCondition("0-1" == aRangeCollection.ToString());
            aRangeCollection.Add(4);
            SpecialFunctions.CheckCondition("0-1,4-4" == aRangeCollection.ToString()); 
            aRangeCollection.Add(5); 
            SpecialFunctions.CheckCondition("0-1,4-5" == aRangeCollection.ToString());
            aRangeCollection.Add(7); 
            SpecialFunctions.CheckCondition("0-1,4-5,7-7" == aRangeCollection.ToString());
            aRangeCollection.Add(2);
            SpecialFunctions.CheckCondition("0-2,4-5,7-7" == aRangeCollection.ToString());
            aRangeCollection.Add(3);
            SpecialFunctions.CheckCondition("0-5,7-7" == aRangeCollection.ToString());
            aRangeCollection.Add(6); 
            SpecialFunctions.CheckCondition("0-7" == aRangeCollection.ToString()); 
            aRangeCollection.Add(-10);
            SpecialFunctions.CheckCondition("-10--10,0-7" == aRangeCollection.ToString()); 
            aRangeCollection.Add(-5);
            SpecialFunctions.CheckCondition("-10--10,-5--5,0-7" == aRangeCollection.ToString());

            aRangeCollection = RangeCollection.Parse("1-5,7-12,13-14");

            Console.WriteLine(aRangeCollection); 
            Console.WriteLine(aRangeCollection.Contains(3)); 
            Console.WriteLine(aRangeCollection.Contains(12));
            Console.WriteLine(aRangeCollection.Contains(13)); 
            Console.WriteLine(aRangeCollection.Contains(6));

            aRangeCollection = RangeCollection.Parse("-10--5,-1-14");

            Console.WriteLine(aRangeCollection);
            Console.WriteLine(aRangeCollection.Contains(-12)); 
            Console.WriteLine(aRangeCollection.Contains(-10)); 
            Console.WriteLine(aRangeCollection.Contains(-7));
            Console.WriteLine(aRangeCollection.Contains(-5)); 
            Console.WriteLine(aRangeCollection.Contains(-4));
            Console.WriteLine(aRangeCollection.Contains(0));
            Console.WriteLine(aRangeCollection.Contains(1));
            Console.WriteLine(aRangeCollection.Contains(-2));

            Console.WriteLine("Count: " + aRangeCollection.Count()); 
            Console.WriteLine("Count -5 to 2: " + aRangeCollection.Count(-5, 2)); 

            RangeCollectionCollection rcc = RangeCollectionCollection.GetInstance(aRangeCollection); 
            Console.WriteLine(rcc);
            Console.WriteLine(rcc.GetContainingRangeCollection(-12));
            Console.WriteLine(rcc.GetContainingRangeCollection(-10));
            Console.WriteLine(rcc.GetContainingRangeCollection(-5));
            Console.WriteLine(rcc.GetContainingRangeCollection(3));
            Console.WriteLine(rcc.GetContainingRangeCollection(15)); 
        } 

 

    }

    public class RangeCollectionCollection : IEnumerable<RangeCollection>
    {
        private List<RangeCollection> _rangeCollections; 
 
        //public IEnumerable<RangeCollection> RangeCollections
        //{ 
        //    get { return _rangeCollections; }
        //}

        private RangeCollectionCollection() { }

        /// <summary> 
        /// A Collection of RangeCollections is constructed from a single RangeCollection by spliting it into 
        /// it's contiguous ranges.
        /// </summary> 
        public static RangeCollectionCollection GetInstance(RangeCollection rangeCollection)
        {
            RangeCollectionCollection rcc = new RangeCollectionCollection();

            rcc._rangeCollections = new List<RangeCollection>();
 
            foreach (string range in rangeCollection.ToString().Split(',')) 
            {
                rcc._rangeCollections.Add(RangeCollection.Parse(range)); 
            }

            return rcc;
        }

        /// <summary> 
        /// Returns the range collection that contains the given item, or null if none exist. 
        /// </summary>
        /// <param name="item"></param> 
        /// <returns></returns>
        public RangeCollection GetContainingRangeCollection(int item)
        {
            // note that by construction, at most one rc in _rangeCollections will contain the item.
            foreach (RangeCollection rc in _rangeCollections)
            { 
                if (rc.Contains(item)) 
                    return rc;
            } 

            return null;
        }

        public override string ToString()
        { 
            StringBuilder sb = new StringBuilder(); 
            foreach (RangeCollection rc in _rangeCollections)
            { 
                sb.Append(rc.ToString() + ";");
            }
            return sb.ToString();
        }

        #region IEnumerable<RangeCollection> Members 
 
        public IEnumerator<RangeCollection> GetEnumerator()
        { 
            foreach (RangeCollection rc in _rangeCollections)
                yield return rc;
        }

        #endregion
 
        #region IEnumerable Members 

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() 
        {
            foreach (RangeCollection rc in _rangeCollections)
                yield return rc;
        }

        #endregion 
    } 
}
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
