using System; 
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Xml; 
using System.Text; 

namespace Msr.Adapt.LearningWorkbench 
{
 	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class FeatureSerializer
	{ 
		//Hashtable _rgSerializer; 
 		XmlSerializer _xmlSerializer;
 		Type _typeRoot; 

		public FeatureSerializer(string[] nameOfAssemblyCollection) : this(typeof(OuterTempClass),typeof(OuterTempClass),"Feature",false,nameOfAssemblyCollection)
 		{
		}
		public FeatureSerializer(Type[] featureTypeCollection) : this(typeof(OuterTempClass),typeof(OuterTempClass),"Feature",false,featureTypeCollection)
		{ 
		} 
		public FeatureSerializer(Type rootType, Type parentType, string fieldName, bool asArray, string[] nameOfAssemblyCollection)
 		{ 
 			Type[] rgFeatureTypeAsArray = CreateTypeCollection(nameOfAssemblyCollection, new Type[]{});
			Init(rootType, parentType, fieldName, asArray, rgFeatureTypeAsArray);
 		}

		public static Type[] CreateTypeCollection(string[] nameOfAssemblyCollection, Type[] initialTypeCollection)
		{ 
 
			// Check that type are feature objects
			foreach (Type aType in initialTypeCollection) 
			{
 				Debug.Assert(aType.IsSubclassOf(typeof(Feature))); //TODO raise error
 			}


			ArrayList rgFeatureTypeAsArraylist = new ArrayList(initialTypeCollection); 
 
 			foreach (string sNameOfAssembly in nameOfAssemblyCollection)
			{ 
				Assembly anAssembly = Assembly.Load(sNameOfAssembly);
				Module[] rgModule = anAssembly.GetModules();
				foreach (Module aModule in rgModule)
				{
 					Type[] rgType = aModule.GetTypes();
 					foreach (Type aType in rgType) 
					{ 
 						if (aType.IsSubclassOf(typeof(Feature)))
						{ 
							rgFeatureTypeAsArraylist.Add(aType);
						}
					}
				}
 			}
 
 
 			Type[] rgFeatureTypeAsArray = new Type[rgFeatureTypeAsArraylist.Count];
			rgFeatureTypeAsArraylist.CopyTo(rgFeatureTypeAsArray); 
 			return rgFeatureTypeAsArray;

		}
		public FeatureSerializer(Type rootType, Type parentType, string fieldName, bool asArray, Type[] featureTypeCollection)
		{
			Init(rootType, parentType, fieldName, asArray,  featureTypeCollection); 
		} 

 		public static bool WillSerialize(string[] nameOfAssemblyCollection, bool throwExceptionIfFalse) 
 		{
			foreach (string sNameOfAssembly in nameOfAssemblyCollection)
 			{
				try
				{
					Assembly anAssembly = Assembly.Load(sNameOfAssembly); 
				} 
				catch(System.IO.FileNotFoundException e)
 				{ 
 					if (throwExceptionIfFalse)
					{
 						InvalidOperationException ioe = new InvalidOperationException(string.Format("Can't find assembly: {0}.", sNameOfAssembly),e);
						throw(ioe);
					}
					return false; 
				} 
			}
 
 			return WillSerialize(CreateTypeCollection(nameOfAssemblyCollection, new Type[]{}), throwExceptionIfFalse);
 		}
		public static bool WillSerialize(Type[] featureTypeCollection, bool throwExceptionIfFalse)
 		{
			// See if we can create an object and serialize it
			foreach (Type aType in featureTypeCollection) 
			{ 
				object anObject;
				try 
 				{
 					anObject = Activator.CreateInstance(aType);
				}
 				catch (System.MissingMethodException e)
				{
					if (throwExceptionIfFalse) 
					{ 
						InvalidOperationException ioe = new InvalidOperationException(string.Format("Can't serialize the type '{0}' because of missing method.", aType.Name),e);
						throw(ioe); 
 					}
 					return false;
				}

 				Debug.WriteLine("XmlSerializer(" + anObject.GetType().ToString() + ")");
                //XmlSerializer aXmlSerializer = new XmlSerializer(aType); 
				XmlSerializer aXmlSerializer = new XmlSerializer(aType, "VirusCount"); 

				StringWriter aStringWriter = new StringWriter(); 
				try
				{
					aXmlSerializer.Serialize(aStringWriter, anObject);
 				}
 				catch(System.InvalidOperationException e)
				{ 
 					if (throwExceptionIfFalse) 
					{
						InvalidOperationException ioe = new InvalidOperationException(string.Format("Can't serialize the type '{0}'.", aType.Name),e); 
						throw(ioe);
					}
					return false;
 				}
 				aStringWriter.Close();
			} 
 			return true; 
		}
		private void Init(Type rootType, Type parentType, string fieldName, bool AsArray, Type[] featureTypeCollection) 
		{
			_typeRoot = rootType;

			// Create the XmlElements
 			XmlAttributes xmlAttributesForElements = new XmlAttributes();
 			XmlAttributes xmlAttributesForArrays = new XmlAttributes(); 
			foreach (Type aType in featureTypeCollection) 
 			{
				// Check that this type has the right base type 
				if (!aType.IsSubclassOf(typeof(Feature)))
				{
					Debug.Assert(false);//TODO raise error
				}
 				xmlAttributesForElements.XmlElements.Add(new XmlElementAttribute(aType.Name, aType));
 				xmlAttributesForArrays.XmlArrayItems.Add(new XmlArrayItemAttribute(aType.Name, aType)); 
			} 

 			//Create the Override 
			XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();
			xmlAttributeOverrides.Add(parentType, fieldName, AsArray?xmlAttributesForArrays:xmlAttributesForElements);
			const string sFeatureFieldName = "FeatureCollection";
			if (typeof(Feature) != parentType || fieldName !=  sFeatureFieldName)
			{
 				xmlAttributeOverrides.Add(typeof(Feature), sFeatureFieldName, xmlAttributesForElements); 
 			} 

			try 
 			{
				_xmlSerializer = new XmlSerializer(_typeRoot, xmlAttributeOverrides, featureTypeCollection, null,null);
			}
			catch(InvalidOperationException ioe)
			{
				throw(new InvalidOperationException(string.Format("Can't create XmlSerializer for {0}",_typeRoot.Name),ioe)); 
 			} 

 			_xmlSerializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute); 
			_xmlSerializer.UnknownElement +=new XmlElementEventHandler(Serializer_UnknownElement);
 			_xmlSerializer.UnknownNode += new XmlNodeEventHandler(Serializer_UnknownNode);
			_xmlSerializer.UnreferencedObject += new UnreferencedObjectEventHandler(Serializer_UnreferencedObject);
		}

 
 
		private void Serializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
		{ 
			Debug.WriteLine("UnreferencedObject:");
 			Debug.WriteLine("ID: " + e.UnreferencedId);
 			Debug.WriteLine("UnreferencedObject: " + e.UnreferencedObject);
			Debug.Assert(false); //TODO raise exception or not depending on settings (?)
 		}
 
		private void Serializer_UnknownNode(object sender, XmlNodeEventArgs e) 
		{
			//TODO raise exception or not depending on settings (?) 
			throw new System.ArgumentException(string.Format("XML contains an undefined node: {0}", e.Name));
		}

 		private void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
 		{
			Debug.WriteLine("Unknown Element"); 
 			Debug.WriteLine("\t" + e.Element.Name + " " + e.Element.InnerXml); 
			Debug.WriteLine("\t LineNumber: " + e.LineNumber);
			Debug.WriteLine("\t LinePosition: " + e.LinePosition); 
			Debug.Assert(false); //TODO raise exception or not depending on settings (?)
		}

		private void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
 		{
 			Debug.WriteLine("Unknown Attribute"); 
			Debug.WriteLine("\t" + e.Attr.Name + " " + e.Attr.InnerXml); 
 			Debug.WriteLine("\t LineNumber: " + e.LineNumber);
			Debug.WriteLine("\t LinePosition: " + e.LinePosition); 
			Debug.Assert(false); //TODO raise exception or not depending on settings (?)
		}



		//TODO doesn't check type's very much 
		public string ToXml(object outer) 
 		{
 			// If this is the special case of a bare feature, wrap it in an outer class 
			if (_typeRoot == typeof(OuterTempClass))
 			{
				outer = new OuterTempClass((Feature) outer);
			}

 
			//TODO 
			// Debug.Assert(KnownFeature(feature), string.Format("Feature {0} is not known to the {1}", feature.GetType().Name, this.GetType().Name)); //TODO make exception
			StringWriter aStringWriter = new StringWriter(); 
 			try
 			{
				_xmlSerializer.Serialize(aStringWriter, outer);
 			}
			catch(System.InvalidOperationException e)
			{ 
				//!!! output 	e.innerException._message	"The type Msr.Adapt.HighLevelFeatures.WeightIf was not expected. Use the XmlInclude or SoapInclude attribute to specify types that are not known statically."	string 

                string message = string.Format("Can't serialize an instance of {0}.", outer.GetType().Name); 
                if (null != e.InnerException)
                {
                    message += " " + e.InnerException.Message;
                }
                InvalidOperationException ioe = new InvalidOperationException(message,e);
				throw(ioe); 
			} 
 			string s = aStringWriter.ToString();
 			aStringWriter.Close(); 

			if (_typeRoot != typeof(OuterTempClass))
 			{
				return s;
			}
			else 
			{ 
				//TODO: Waining
 				// this makes assumptions about the form of the serialization 
 				//						@"<?xml version="1.0" encoding="utf-16"?>
				//<OuterTempClass xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
 				//  <HasString>
				//    <Subject />
				//    <Text>out</Text>
				//  </HasString> 
				//</OuterTempClass>" 

 
				string[] rgsXml = s.Split('\n');
 				Debug.Assert(rgsXml[1].StartsWith("<OuterTempClass ") && rgsXml[rgsXml.Length-1] == "</OuterTempClass>"); // real assert
 				StringBuilder aStringBuilder = new StringBuilder();
				for (int iLine = 2; iLine < rgsXml.Length - 1; ++iLine)
 				{
					aStringBuilder.Append(rgsXml[iLine].Trim()); 
				} 
				return aStringBuilder.ToString();
			} 
		}

 		//		/// <summary>
 		//		/// Tells if a feature is of types know to the function manager
		//		/// </summary>
 		//		public bool KnownFeature(Feature feature) 
		//		{ 
		//			if (!_rgSerializer.ContainsKey(feature.GetType()))
		//			{ 
		//				return false;
		//			}
 		//
 		//			//if (feature.FeatureCollection != null)
		//			{
 		//				//Debug.Assert(feature.FeatureCollection.Length > 0, "FeatureCollections must not be zero length arrays (instead, make them null)"); //TODO raise exception 
		//				foreach (Feature featureSub in feature.FeatureCollection) 
		//				{
		//					if (!KnownFeature(featureSub)) 
		//					{
		//						return false;
 		//					}
 		//				}
		//			}
 		// 
		// 
		//			return true;
		//		} 
		//
		public object FromXml(string xml)
 		{
            bool bOuter = xml.StartsWith("<OuterTempClass");
 			StringReader aStringReader;
 
            if (bOuter) 
            {
                aStringReader = new StringReader(xml); 
            }
            else
            {
                aStringReader = new StringReader(String.Format("<OuterTempClass>{0}</OuterTempClass>", xml));
            }
			object objectOuter = _xmlSerializer.Deserialize(aStringReader); //!!!if this fails, raise a message suggesting that the xml may have a syntax error 
 			aStringReader.Close(); 

            if (bOuter) 
            {
                //TODO check that it is of type objectOuter
                return objectOuter;
            }
            else
            { 
                return ((OuterTempClass)objectOuter).Feature; 
            }
		} 

        public object FromXmlStreamReader(StreamReader streamReader)
        {
                string xml = streamReader.ReadToEnd();
                return FromXml(xml);
        } 
 

        public object FromXmlFile(string fileName) 
        {
            using (StreamReader streamReader = File.OpenText(fileName))
            {
                string xml = streamReader.ReadToEnd();
                return FromXml(xml);
            } 
        } 
    }
 

	[Serializable]
	[XmlRoot(Namespace="", IsNullable=false)]
	public class OuterTempClass
	{
 		public OuterTempClass() : this(null) 
 		{ 
		}
 	 
		public OuterTempClass(Feature feature)
		{
			Feature = feature;
		}

		public Feature Feature; 
 	} 

 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
