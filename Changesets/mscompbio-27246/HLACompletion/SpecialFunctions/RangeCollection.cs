using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;
using System.Text.RegularExpressions;
 
// !!!!  here is a bug on rangecollection to fix sometime 
//  Add these in this order 1,2,3,4,5,6,7,8,9,0 and then look at the result. It is 0,1-9, not the expected 0-9
 
namespace Msr.Mlas.SpecialFunctions
{
    [Serializable]
    public class RangeCollection
    {
        public RangeCollection Clone() 
        { 
            RangeCollection clone = new RangeCollection();
            clone._startItems = new List<int>(_startItems); 
            clone._itemToLength = new SortedDictionary<int, int>(_itemToLength);
            return clone;
        }


 		private static readonly Regex _rangeExpression = new Regex(@"^(?<begin>-?\d+)(-(?<end>-?\d+))?$", RegexOptions.Compiled); 
		private List<int> _startItems; 
		private SortedDictionary<int, int> _itemToLength;
 
        public RangeCollection()
        {
            _startItems = new List<int>();
            _itemToLength = new SortedDictionary<int, int>();
        }
 
		public static RangeCollection GetInstance() 
		{
			return new RangeCollection(); 
 		}

 		public static RangeCollection GetInstance(int singleItem)
		{
 			RangeCollection range = new RangeCollection();
			range.Add(singleItem); 
			return range; 
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
			ranges = ranges.Trim();
			RangeCollection aRangeCollection = GetInstance(); 

			aRangeCollection.AddRanges(ranges.Split(','));

			return aRangeCollection;
		}
 
 
 		public IEnumerable<KeyValuePair<int, int>> Collection
 		{ 
			get
 			{
				foreach (int item in _startItems)
				{
					int last = item + _itemToLength[item] - 1;
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
				return _startItems[0];
			} 
		} 

		public int LastElement //!!!Used to be MaxElement but it was returning one beyond the last (inclusive) element in the range. Because I didn't know if this was by design or a bug. I changed the name. 
		{
 			get
 			{
				return _startItems[_startItems.Count - 1] + _itemToLength[_startItems[_startItems.Count - 1]] - 1;
 			}
		} 
 

		public void Clear() 
		{
			_startItems.Clear();
			_itemToLength.Clear();
 		}

 		public int Count() 
		{ 
 			return Count(int.MinValue, int.MaxValue);
		} 

		public int Count(int min, int max)
		{
			int count = 0;
			foreach (int start in _startItems)
 			{ 
 				int stop = _itemToLength[start] + start - 1; 

				// truncate start and stop around max. 
 				int begin = Math.Max(start, min);
				int end = Math.Min(stop, max);
				int diff = Math.Max(0, end - begin + 1);

				count += diff;
			} 
 
			return count;
 
 		}

 		/// <summary>
		/// Returns the number of contiguous ranges in this collection. Useful for memory
 		/// consumption debugging.
		/// </summary> 
		public int EntryCount 
		{
			get { return _startItems.Count; } 
		}

        public void AddRangeCollection(RangeCollection rangeCollection)
        {
            TryAddRangeCollection(rangeCollection);
        } 
 
 		public bool TryAddRangeCollection(RangeCollection rangeCollection)
 		{ 
            bool allNew = true;
			foreach (KeyValuePair<int,int> startAndLast in rangeCollection.Collection)
 			{
				allNew &= TryAdd(startAndLast.Key, startAndLast.Value - startAndLast.Key + 1);
			}
            return allNew; 
		} 

		public void AddRanges(ICollection<string> ranges) 
		{
 			foreach (string range in ranges)
 			{
				AddRange(range);
 			}
		} 
 
		public void AddRange(string range)
		{ 
			SpecialFunctions.CheckCondition(_rangeExpression.IsMatch(range), range + " is not a valid range. Must be of the form m-n or m.");
			Match match = _rangeExpression.Match(range);
 			int begin = int.Parse(match.Groups["begin"].Value);
 			int end = match.Groups["end"].Value == "" ? begin : int.Parse(match.Groups["end"].Value);
			AddRange(begin, end);
 		} 
 
		private void AddRange(int begin, int end)
		{ 
			SpecialFunctions.CheckCondition(begin <= end, "Invalid range. Begin " + begin + " must be no greater than end " + end + ".");
			TryAdd(begin, end - begin + 1);
		}


 		public void Add(int item) 
 		{ 
			bool isOK = TryAdd(item);
 			//SpecialFunctions.CheckCondition(!isOK); 
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="item"></param> 
 		/// <returns>True if item was added. False if it already existed in the range.</returns> 
 		public bool TryAdd(int item)
		{ 
 			return TryAdd(item, 1);
		}

		public bool TryAdd(int item, int length)
		{
			SpecialFunctions.CheckCondition(length > 0, "Invalid range. Must have positive length."); 
			Debug.Assert(_startItems.Count == _itemToLength.Count); // real assert 
 			int indexOfMiss = ~_startItems.BinarySearch(item);
 			 
			int previous, end;

 			if (indexOfMiss < 0) //Hit a start
			{
				indexOfMiss = ~indexOfMiss;
				if (length <= _itemToLength[item]) 
				{ 
					return false;
 				} 
 				else
				{
 					_itemToLength[item] = length;
					indexOfMiss++;	  // indexOfMiss should point to the following range for the remainder of this method
					previous = item;
					end = item + length; 
				} 
			}
 			else if (indexOfMiss == 0) 
 			{
				_startItems.Insert(indexOfMiss, item);
 				_itemToLength.Add(item, length);
				previous = item;
				end = item + length;
				indexOfMiss++;		  // indexOfMiss should point to the following range for the remainder of this method 
				//return true; 
			}
 			else 
 			{
				 previous = _startItems[indexOfMiss - 1];
 				 end = previous + _itemToLength[previous];

				//if (item < end) //inside existing range
				//{ 
				//	return false; 
				//}
				 if (item <= end) 
 				 {
 					 int newLength = item - previous + length;
					 Debug.Assert(newLength > 0); // real assert
 					 if (newLength < _itemToLength[previous])
					 {
						 return false; 
					 } 
					 else
					 { 
 						 _itemToLength[previous] = newLength;
 						 end = previous + newLength;
					 }
 				 }
				 else // after previous range, not contiguous with previous range
				 { 
					 _startItems.Insert(indexOfMiss, item); 
					 _itemToLength.Add(item, length);
					 previous = item; 
 					 end = item + length;
 					 indexOfMiss++;
				 }
 			}

			if (indexOfMiss == _startItems.Count) 
			{ 
				return true;
			} 

			// collapse next range into this one
 			//!!loop till encorportate all possible ranges.
 			int next = _startItems[indexOfMiss];
			if (end >= next)
 			{ 
				int newEnd = Math.Max(end, next + _itemToLength[next]); 
				_itemToLength[previous] = newEnd - previous; //ItemToLength[previous] + ItemToLength[next];
				_itemToLength.Remove(next); 
				_startItems.RemoveAt(indexOfMiss);
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
			int indexOfMiss = _startItems.BinarySearch(item); 
			if (indexOfMiss >= 0) // item is the beginning of a range 
				return true;
 
 			indexOfMiss = ~indexOfMiss;

 			if (indexOfMiss == 0)   // item is before any of the ranges
				return false;

 			int previous = _startItems[indexOfMiss - 1]; 
			int end = previous + _itemToLength[previous]; 

			return item < end; // we already know it's greater than previous... 
		}

        public bool IsBetween(int low, int high)
        {
            int veryFirstItem = _startItems[0];
            Debug.Assert(_startItems.Count == _itemToLength.Count); 
            int veryLastItem = _startItems[_startItems.Count - 1] + _itemToLength[veryFirstItem] - 1; 
            bool isBetween = low <= veryFirstItem && veryFirstItem <= high;
            return isBetween; 
        }

        public bool IsEmpty
        {
            get
            { 
                return _startItems.Count == 0; 
            }
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
 			bool b = (_startItems.Count == 1) && (_startItems[0] == firstItem) && (_itemToLength[_startItems[0]] == lastItem - firstItem + 1); 
			return b;
 		}

		static public void Test()
		{
			RangeCollection aRangeCollection = RangeCollection.GetInstance(); 
			aRangeCollection.Add(0); 
			SpecialFunctions.CheckCondition("0" == aRangeCollection.ToString());
 			aRangeCollection.Add(1); 
 			SpecialFunctions.CheckCondition("0-1" == aRangeCollection.ToString());
			aRangeCollection.Add(4);
 			SpecialFunctions.CheckCondition("0-1,4" == aRangeCollection.ToString());
			aRangeCollection.Add(5);
			SpecialFunctions.CheckCondition("0-1,4-5" == aRangeCollection.ToString());
			aRangeCollection.Add(7); 
			SpecialFunctions.CheckCondition("0-1,4-5,7" == aRangeCollection.ToString()); 
			aRangeCollection.Add(2);
 			SpecialFunctions.CheckCondition("0-2,4-5,7" == aRangeCollection.ToString()); 
 			aRangeCollection.Add(3);
			SpecialFunctions.CheckCondition("0-5,7" == aRangeCollection.ToString());
 			aRangeCollection.Add(6);
			SpecialFunctions.CheckCondition("0-7" == aRangeCollection.ToString());
			aRangeCollection.Add(-10);
			SpecialFunctions.CheckCondition("-10,0-7" == aRangeCollection.ToString()); 
			aRangeCollection.Add(-5); 
			SpecialFunctions.CheckCondition("-10,-5,0-7" == aRangeCollection.ToString());
 
 			string range = "-10--5,-3,-2-1,1-5,7-12,13-15,14-16,20-25,22-23";
 			aRangeCollection = RangeCollection.Parse(range);
			Console.WriteLine(range);
 			Console.WriteLine(aRangeCollection);
			//Console.WriteLine(aRangeCollection.Contains(3));
			//Console.WriteLine(aRangeCollection.Contains(12)); 
			//Console.WriteLine(aRangeCollection.Contains(13)); 
			//Console.WriteLine(aRangeCollection.Contains(6));
 
			range = "1-5,0,4-10,-10--5,-12--3,15-20,12-21,-13";
 			aRangeCollection = RangeCollection.Parse(range);

 			Console.WriteLine(range);
			Console.WriteLine(aRangeCollection);
 			//Console.WriteLine(aRangeCollection.Contains(-12)); 
			//Console.WriteLine(aRangeCollection.Contains(-10)); 
			//Console.WriteLine(aRangeCollection.Contains(-7));
			//Console.WriteLine(aRangeCollection.Contains(-5)); 
			//Console.WriteLine(aRangeCollection.Contains(-4));
			//Console.WriteLine(aRangeCollection.Contains(0));
 			//Console.WriteLine(aRangeCollection.Contains(1));
 			//Console.WriteLine(aRangeCollection.Contains(-2));

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
 




 		public static RangeCollection GetInstance(RangeCollection rangeCollection)
		{ 
			return rangeCollection.Clone(); 
		}
 
	}

	public class RangeCollectionCollection : IEnumerable<RangeCollection>
 	{
 		private List<RangeCollection> _rangeCollections;
 
		//public IEnumerable<RangeCollection> RangeCollections 
 		//{
		//	get { return _rangeCollections; } 
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

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
