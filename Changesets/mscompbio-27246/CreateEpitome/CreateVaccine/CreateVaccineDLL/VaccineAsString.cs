using System; 
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions; 
using Msr.Mlas.SpecialFunctions; 

namespace VirusCount 
{
 	/// <summary>
	/// Summary description for VaccineAsString.
	/// </summary>
	public class VaccineAsString
	{ 
        public string String; 
        public int NumberOfComponents;
        public int TotalNumberOfAminoAcids; 
        public string Name;



		//!!!could be nice to have a version that writes to a stream and to a string
 		public static void DebugReportCollection(VaccineAsString[] vaccineAsStringCollection) 
 		{ 
			for(int iVaccineAsString = 0; iVaccineAsString < vaccineAsStringCollection.Length; ++iVaccineAsString)
 			{ 
				VaccineAsString aVaccineAsString = vaccineAsStringCollection[iVaccineAsString];
				Debug.WriteLine(string.Format("{0}\t{1}",iVaccineAsString, aVaccineAsString.String));
			}
		}

	 
 
 		private VaccineAsString()
 		{ 
		}


 		private string Border;

		static public VaccineAsString GetInstance(int maximumWindowLength, string name) 
		{ 
			VaccineAsString aVaccineAsString = new VaccineAsString();
			aVaccineAsString.Border = new string('_', (int) maximumWindowLength); //!!!Using _____ of varying length now seems not so good. How about using newlines? 

			aVaccineAsString.String = string.Format("{0}",aVaccineAsString.Border);
 			aVaccineAsString.NumberOfComponents = 0;
 			aVaccineAsString.TotalNumberOfAminoAcids = 0;
			aVaccineAsString.Name = name;
 
 			return aVaccineAsString; 
		}
 
		public void AddComponent(PatchPattern patchPattern)
		{
			AddComponent(patchPattern.FullRealization());
		}

 		public void AddComponent(string componentAsString) 
 		{ 
			String = string.Format("{0}{1}{2}", String, componentAsString, Border);
 			++NumberOfComponents; 
			TotalNumberOfAminoAcids += componentAsString.Length;
		}

		public void SetComponents(HashSet<Component> componentList)
		{
			NumberOfComponents = componentList.Count; 
 			TotalNumberOfAminoAcids = 0; 
 			StringBuilder sb = new StringBuilder(Border);
            Component[] componentArray = new Component[NumberOfComponents]; 
            componentList.CopyTo(componentArray, 0);
            Array.Sort(componentArray, delegate(Component c0, Component c1) {return c0.CreationOrder.CompareTo(c1.CreationOrder);});

            foreach (Component component in componentArray)
			{
 				TotalNumberOfAminoAcids += component.FullLength; 
				sb.Append(component.PatchPattern().FullRealization()); 
				sb.Append(Border);
			} 
			String = sb.ToString();
		}


 		static Regex UnderscoresPattern = new Regex("_+");
 		internal string[] NiceStringCollection() 
		{ 
 			string[] components = UnderscoresPattern.Split(String.Trim('_'));
			return components; 
		}
		internal string NiceString()
		{
			string niceString = string.Join(",", NiceStringCollection());
 			return niceString;
 		} 
 
		internal static VaccineAsString GetInstanceFromNice(string name, string nice)
 		{ 
			string[] components = nice.Split(',');
			int max = 0;
			foreach (string component in components)
			{
				max = Math.Max(max, component.Length);
 			} 
 
 			VaccineAsString aVaccineAsString = VaccineAsString.GetInstance(max, name);
 
			foreach (string component in components)
 			{
				aVaccineAsString.AddComponent(component);
			}
			return aVaccineAsString;
		} 
    } 
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
