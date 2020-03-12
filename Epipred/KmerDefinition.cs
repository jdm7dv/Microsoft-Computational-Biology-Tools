using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;

namespace VirusCount
{ 
 	/// <summary> 
	/// Summary description for KmerDefinition.
	/// </summary> 
	public class KmerDefinition
	{
		public override string ToString()
 		{
 			return string.Format("{0}{1}{2}", BeforeMerCount, EpitopeMerCount, AfterMerCount);
		} 
 
 		static public KmerDefinition GetInstance()
		{ 
			return GetInstance(7,9,6);
		}
		static public KmerDefinition GetInstance(int b, int e, int a)
		{
 			KmerDefinition aKmerDefinition = new KmerDefinition();
 			aKmerDefinition.BeforeMerCount = b; 
			aKmerDefinition.EpitopeMerCount = e; 
 			aKmerDefinition.AfterMerCount = a;
			aKmerDefinition.NumberOfEAt1stPosition = 1; 

			Debug.Assert(aKmerDefinition.NumberOfEAt1stPosition == 1 || aKmerDefinition.BeforeMerCount == 0); //!!! these two cases are the only ones tested

			return aKmerDefinition;
		}
 
		private KmerDefinition() 
 		{
 		} 

		public int BeforeMerCount;
 		public int EpitopeMerCount;
		public int AfterMerCount;
		public int NumberOfEAt1stPosition;
		public int FullMerCount 
		{ 
			get
 			{ 
 				return BeforeMerCount + EpitopeMerCount + AfterMerCount;
			}
 		}

		public string PositionName(int iPos, int iZeroPos)
		{ 
			int iDiff = iPos - iZeroPos; 
			if (iDiff < BeforeMerCount)
			{ 
 				return string.Format("B{0}", iDiff - 6);
 			}
			else
 			{
				iDiff -= BeforeMerCount;
				if (iDiff < EpitopeMerCount) 
				{ 
					Debug.Assert(NumberOfEAt1stPosition == 1); //!!!need code for other cases
					return string.Format("E{0}", iDiff + 1); 
 				}
 				else
				{
 					iDiff -= EpitopeMerCount;
					Debug.Assert(iDiff < AfterMerCount);
					return string.Format("A{0}", iDiff + 1); 
				} 
			}
		} 

        public int StringPosition(char chBEA, int iOffset)
        {
            int iIndexInString;
            switch (chBEA)
            { 
                case 'B': 
                    iIndexInString = BeforeMerCount + iOffset;
                    break; 
                case 'E':
                    iIndexInString = BeforeMerCount + iOffset - 1;
                    break;
                case 'A':
                    iIndexInString = BeforeMerCount + EpitopeMerCount + iOffset - 1;
                    break; 
                default: 
                    Debug.Fail("ERRor"); //!!!
                    iIndexInString = int.MinValue; 
                    break;

            }
            return iIndexInString;
        }
 
 

 	} 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
