using System; 
using System.Diagnostics;
using System.Collections;
using System.IO; //todo: is this still needed?
using System.Windows.Forms; //todo: is this still needed? also Reference to Windows.Forms.dll

namespace Msr.Adapt.LearningWorkbench 
{ 
 	/// <summary>
	/// This class score retreival results. 
	///
	/// Many of the methods are defined in
	///   http://trec.nist.gov/pubs/trec4/appendices/A/appendixa.ps.gz
	///	  D. K. Harman. 1996. Appendix a, evaluation techniques and measures.
 	///	  In D. K. Harman, editor, Proceedings of the Fourth Text REtrieval Conference
 	///	  (TREC-4), pages A6--A14. NIST Special Publication 500-236. 
 
	/// </summary>
 	public class RetrievalScoring 
	{
		bool[] _rgbRelevent;
		int _cReleventItems;
		double[] _rgrScore;
		
 		/// <summary> 
 		/// ReleventCollection is a boolean array that tells if the ith item that would be 
		/// return is relevent. Must be sorted by return order
 		/// </summary> 
		/// <param name="ReleventCollection"></param>
		public RetrievalScoring(bool[] ReleventCollection)
		{
			_rgbRelevent = ReleventCollection;
			_rgrScore = null; //TODO  would it be better to always require this or to never require it?
 			_cReleventItems = NumberRelevent(ReleventCollection.Length); 
 		} 

		//TODO would it be better to have one array of a pair? 
 		/// <summary>
		/// Does not need to be sorted
		/// </summary>
		/// <param name="ReleventCollection"></param>
		/// <param name="ScoreCollection"></param>
		public RetrievalScoring(bool[] ReleventCollection, double[] ScoreCollection) 
 		{ 
 			Debug.Assert(ReleventCollection.Length == ScoreCollection.Length); //TODO raise error must be the same length
            _rgbRelevent = ReleventCollection; 
			_rgrScore = ScoreCollection;
 			Array.Sort(_rgrScore,_rgbRelevent, new LargeDoubleFirst());

			_cReleventItems = NumberRelevent(ReleventCollection.Length);
		}
 
		public int NumberRelevent() 
		{
			return _cReleventItems; 
 		}

 		public int NumberRelevent(int NumberReturned)
		{
 			int iReleventItems = 0;
			for(int i = 0; i < NumberReturned; ++i) 
			{ 
				bool bRelevent = _rgbRelevent[i];
				if (bRelevent) 
				{
 					++iReleventItems;
 				}
			}
 			return iReleventItems;
		} 
 
		public double Recall(int NumberReturned)
		{ 
			int iReleventReturned = NumberRelevent(NumberReturned);
			return (double) iReleventReturned / (double)  _cReleventItems;
 		}

 		public double Precision(int NumberReturned)
		{ 
 			int iReleventReturned = NumberRelevent(NumberReturned); 
			return (double) iReleventReturned / (double)  NumberReturned;
		} 

		public SortedList PrecisionAtNCutoffValues(int NumberCutoffValues)
		{
			double rOldPrecision = 0.0;
 			int iCutoff = 0;
 			SortedList rgPrecisions = new SortedList(NumberCutoffValues); 
 
			int iReleventItems = 0;
 			for(int i = 0; i < _rgbRelevent.Length; ++i) 
			{
				bool bRelevent = _rgbRelevent[i];
				if (bRelevent)
				{
					++iReleventItems;
 					double rRecall = (double) iReleventItems / (double) _cReleventItems; 
 					double rPrecision = (double) iReleventItems / (double)  (i + 1); 

					Debug.WriteLine(string.Format("recall={0}, precision={1}", rRecall, rPrecision)); 
 					do
					{
						double rRecallCutoff = (double)iCutoff / (double) (NumberCutoffValues - 1);
						if (rRecallCutoff > rRecall)
						{
							break; 
 						} 
 						rgPrecisions.Add(rRecallCutoff, (rRecallCutoff < rRecall)?rOldPrecision:rPrecision);
							 
 						++iCutoff;
					} while(iCutoff < NumberCutoffValues);
					rOldPrecision = rPrecision;
				}
			}
 
			return rgPrecisions; 
 		}
 		public SortedList InterpolatingRecallPrecision(int NumberCutoffValues) 
		{
 			int iCutoff = 0;
			SortedList rgPrecisions = new SortedList(NumberCutoffValues);

			int iReleventItems = 0;
			for(int i = 0; i < _rgbRelevent.Length; ++i) 
			{ 
				bool bRelevent = _rgbRelevent[i];
 				if (bRelevent) 
 				{
					++iReleventItems;
 					double rRecall = (double) iReleventItems / (double) _cReleventItems;
					double rPrecision = (double) iReleventItems / (double)  (i + 1);
					do
					{ 
						double rRecallCutoff = (double)iCutoff / (double) (NumberCutoffValues - 1); 
						if (rRecallCutoff > rRecall)
 						{ 
 							break;
						}
 						rgPrecisions.Add(rRecallCutoff,rPrecision);
						++iCutoff;
					} while(iCutoff < NumberCutoffValues);
				} 
			} 

			return rgPrecisions; 
 		}
 		
		public double NoninterpolatedAveragePrecision()
 		{
			int iReleventItems = 0;
			double rTotalPrecision = 0.0; 
			for(int i = 0; i < _rgbRelevent.Length; ++i) 
			{
				bool bRelevent = _rgbRelevent[i]; 
 				if (bRelevent)
 				{
					++iReleventItems;
 					double rPrecision = (double) iReleventItems / (double)  (i + 1);
					rTotalPrecision += rPrecision;
				} 
			} 

			if (iReleventItems == 0) 
			{
 				return 0.0;
 			}
			else
 			{
				double rAnswer = rTotalPrecision / (double) iReleventItems; 
				return rAnswer; 
			}
		} 

		public static void TestClass()
 		{
 			bool[] rgbReturn = new bool[]{true,true,false,true,false,false,true,false,false,false};
			RetrievalScoring aRetrievalScoring = new RetrievalScoring(rgbReturn);
 			Debug.Assert(Math.Abs(aRetrievalScoring.NoninterpolatedAveragePrecision() - 0.83) < .01); 
 
			rgbReturn = new bool[20];
			rgbReturn[3] = true; 
			rgbReturn[8] = true;
			rgbReturn[19] = true;
			aRetrievalScoring = new RetrievalScoring(rgbReturn);
 			SortedList rgr = aRetrievalScoring.InterpolatingRecallPrecision(11);
 			double[] rgrAnswer = new double[]{0.25,0.25,0.25,0.25, 0.22,0.22,0.22, 0.15,0.15,0.15,0.15};
			Debug.Assert(rgr.Count == rgrAnswer.Length); 
 			for(int i = 0; i < rgr.Count; ++i) 
			{
				Debug.Assert(Math.Abs((double) rgr.GetByIndex(i) - rgrAnswer[i]) < .01); 
			}

			//			rgr = aRetrievalScoring.PrecisionAtNCutoffValues(11);
			//			rgrAnswer = new double[]{0.0,0.0,0.0,0.25, 0.25,0.25,0.22, 0.15,0.15,0.15,0.15};
 			//			Debug.Assert(rgr.Count == rgrAnswer.Length);
 			//			for(int i = 0; i < rgr.Count; ++i) 
			//			{ 
 			//				Debug.Assert(Math.Abs((double) rgr.GetByIndex(i) - rgrAnswer[i]) < .01);
			//			}			 
		}


		/// <summary>
		/// For a nice reference on ROC curves see http://www.anaesthetist.com/mnm/stats/roc/
		/// and http://www.medcalc.be/manual/mpage06-13a.html 
 		/// </summary> 
 		/// <param name="sFileName"></param>
		public void Roc() 
 		{
			int[] rgcTotal = new int[]{_rgbRelevent.Length - _cReleventItems, _cReleventItems};
			int[] rgcSoFar = new int[]{0,0};

			//Plot FPF by TPF
		      //   false positive fraction 
			  //   true positive fraction 
 			//TODO there should be some score here, too
 			Debug.WriteLine(string.Format("{0},{1}","false positive fraction", "true positive fraction")); 
			for(int i = 0; i < _rgbRelevent.Length; ++i)
 			{
				bool b = _rgbRelevent[i];
				++rgcSoFar[b?1:0];
				//If the cutoff is after this point, what is the false positive fraction?
				double rFPF = (double) rgcSoFar[0] / rgcTotal[0]; 
				double rTPF = (double) rgcSoFar[1] / rgcTotal[1]; 
 				Debug.WriteLine(string.Format("{0},{1}",rFPF,rTPF));
 			} 

//			Debug.Assert(_rgrScore != null); //TODO raise error: the score array is needed
//
//
//			
// 
//			int minGroup = 1; 
//
//			//TODO what if already exists? 
//			//TODO use "Using"
//			StreamWriter aStreamWriter = null;
//			try
//			{
//				aStreamWriter = new StreamWriter(sFileName);
//			} 
//			catch (System.IO.IOException ioException) 
//			{
//				MessageBox.Show(ioException.Message); 
//				return;
//			}
//
//
//		// count outs and ins
//			int[] rgcTotal = new int[2]{_rgbRelevent.Length - _cReleventItems, _cReleventItems}; 
// 
//			for (int iClass = 0; iClass < rgcTotal.Length; ++iClass)
//			{ 
//				aStreamWriter.WriteLine("{0},{1}",rgcTotal[iClass],iClass==0?"Not Relevent":"Relevent");
//			}
//
//			aStreamWriter.WriteLine();
//
//			aStreamWriter.WriteLine("Thresh,% R cat S,% S cat R,cnt R cat S,cnt S cat R"); 
// 
//			int [] rgc = new int[_rgbRelevent.Length]; // inits to all zeros
//			int cGroup = 0; 
//			double rScore;
//			for (int iScore = 0; iScore < rgScores2.Length;) // note: don't increment here
//			{
//				rScore = rgScores2[iScore];
//				++cGroup;
//				// increment the correct count of elements passed. (note dupe code below) 
//				++rgc[rgClassIndexes2[iScore]]; 
//				while (++iScore < rgScores2.Length && rScore == rgScores2[iScore])
//				{ 
//					++cGroup;
//					// increment the correct count of elements passed. (note dupe code of above)
//					++rgc[rgClassIndexes2[iScore]];
//				}
//		
//				// note that AllScores is sorted so zero comes first 
//				if (iScore < rgScores2.Length && (cGroup >= minGroup)) 
//				{
//					aStreamWriter.WriteLine("{0},{1},{2},{3},{4}",rScore, ((double)(rgcTotal[0]-rgc[0]))/rgcTotal[0], ((double)rgc[1])/rgcTotal[1], rgcTotal[0]-rgc[0], rgc[1]); 
//					cGroup = 0;
//				}
//			}
//
//			aStreamWriter.Close();
		} 
 	} 

	public class LargeDoubleFirst: IComparer 
	{
		public int Compare(object ob0, object ob1)
		{
			return ((double)ob1).CompareTo(ob0);
 		}
 	} 
 
}
 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
