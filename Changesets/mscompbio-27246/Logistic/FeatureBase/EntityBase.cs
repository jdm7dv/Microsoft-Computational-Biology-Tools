using System; 
using System.Xml.Serialization;
using System.Diagnostics;
using System.Reflection;

namespace Msr.Adapt.LearningWorkbench
{ 
 	/// <summary> 
	/// Inherit from this class to define a feature on a feature on email, etc.
	/// </summary> 
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public abstract class Feature
    {
        private Feature[] _rgFeature = new Feature[] { };
        [XmlIgnore] 
        public virtual MethodInfo MainMethod 
        {
            get 
            {
                //Would be nice if this could be check at compile time
                // Also would be nice if multiple FeatureFunction's could be defined and it would pick the right one based on the types being passed
                MethodInfo aMethodInfo = this.GetType().GetMethod("FeatureFunction");
                Debug.Assert(aMethodInfo != null);// TODO raise error? or ignore?
                return aMethodInfo; 
            } 
        }
 
        [XmlElement]
        public Feature[] FeatureCollection
        {
            get
            {
                return _rgFeature; 
            } 
            set
            { 
                if (value == null)
                {
                    _rgFeature = new Feature[] { };
                }
                else
                { 
                    _rgFeature = value; 
                }
            } 
        }

        /// <summary>
        /// Unions two arrays of Feature objects, creating a new array of Feature objects
        /// </summary>
        //TODO would this be better with variable # of inputs? or one input of type Feature[][]? 
        static public Feature[] Union(Feature[] FeatureCollection1, Feature[] FeatureCollection2) 
        {
            Feature[] rg = new Feature[FeatureCollection1.Length + FeatureCollection2.Length]; 
            FeatureCollection1.CopyTo(rg, 0);
            FeatureCollection2.CopyTo(rg, FeatureCollection1.Length);
            return rg;
        }

        static private bool IsParams(ParameterInfo aParameterInfo) 
        { 
            object[] rgObj = aParameterInfo.GetCustomAttributes(typeof(ParamArrayAttribute), true);
            Debug.Assert(rgObj.Length == 0 || rgObj.Length == 1); // real assert 
            return rgObj.Length > 0;
        }

        public virtual object Evaluate(object entity)
        {
            Feature feature = this; 
            MethodInfo aMethodInfo = feature.MainMethod; 
            ParameterInfo[] rgParameterInfo = aMethodInfo.GetParameters();
 
            Debug.Assert(feature.FeatureCollection != null);
            int cFeatureCollection = feature.FeatureCollection.Length;

            //If this is a entityfeature, then the first parameter of the main method, takes the entity
            int cEntityParam = 0;
            if (feature is EntityFeature) 
            { 
                cEntityParam = 1;
                // If this is a entityparams, check that there is an argument for it and that it is type "params" 
                Debug.Assert(rgParameterInfo.Length > 0); //TODO raise error
                Debug.Assert(!IsParams(rgParameterInfo[0])); //TODO raise error
            }


 
            // See if the "params" keyword is being used 
            bool bVariableParam = (rgParameterInfo.Length > 0) && IsParams(rgParameterInfo[rgParameterInfo.Length - 1]);
 

            if (bVariableParam)
            {
                //!!! this assert doesn't work when zero features are given
                //Debug.Assert(cFeatureCollection >= rgParameterInfo.Length); //TODO raise error if the collection is too short
                //TODO check tthis earlier when type if given to manager? 
                Debug.Assert(rgParameterInfo[rgParameterInfo.Length - 1].ParameterType.IsArray); //TODO raise error 
                Debug.Assert(rgParameterInfo[rgParameterInfo.Length - 1].ParameterType.GetElementType() == typeof(object)); //TODO raise error - type must be array of objects
            } 
            else
            {
                if (cFeatureCollection + cEntityParam != rgParameterInfo.Length)
                {
                    throw new System.ArgumentException(string.Format("The '{0}' feature expects {1} feature input(s), but it was given {2}", this.GetType(), rgParameterInfo.Length - cEntityParam, cFeatureCollection));
                } 
 
            }
 
            object[] parameters = new object[rgParameterInfo.Length];


            // Evaluate the subfeatures
            for (int iParam = 0; iParam < parameters.Length; ++iParam)
            { 
                if (iParam < cEntityParam) 			//If this is a entityfeature, then the first parameter of the main method, takes the entity 
                {
                    Debug.Assert(iParam == 0); // real assert 
                    parameters[iParam] = entity;
                    Debug.Assert(!IsParams(rgParameterInfo[iParam])); // real assert
                }
                else
                {
                    if (!IsParams(rgParameterInfo[iParam])) // regular parameter 
                    { 
                        Feature featureSub = feature.FeatureCollection[iParam - cEntityParam];
                        parameters[iParam] = featureSub.Evaluate(entity); 
                    }
                    else // "params" parameter
                    {
                        Debug.Assert(iParam == parameters.Length - 1); // real assert
                        //TODO does it matter if object[] doesn't match the declared type for the params parameter?
                        object[] paramsparameters = new object[feature.FeatureCollection.Length - (iParam - cEntityParam)]; 
                        for (int iFeature = iParam - cEntityParam; iFeature < feature.FeatureCollection.Length; ++iFeature) 
                        {
                            Feature featureSub = feature.FeatureCollection[iFeature]; 
                            paramsparameters[iFeature - (iParam - cEntityParam)] = featureSub.Evaluate(entity);
                            // if paramsparameters was more typed, would check types here
                        }
                        parameters[iParam] = paramsparameters;
                    }
                } 
 
                // Check its type
                Type typeExpected = rgParameterInfo[iParam].ParameterType; 

                if (!typeExpected.IsInstanceOfType(parameters[iParam]))
                {
                    throw new System.ArgumentException(string.Format("Expected input of type '{0}' but got type '{1}'", typeExpected, parameters[iParam].GetType()));
                }
            } 
 
            // Call the main function and return
            try 
            {
                return aMethodInfo.Invoke(feature, parameters);
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                if (tie.InnerException != null) 
                { 
                    throw (tie.InnerException);
                } 
                return null;
            }
        }

    }
 
	/// <summary> 
	/// Inherit from this class on an email, etc.
	/// </summary> 
 	public abstract class EntityFeature : Feature
 	{
		protected EntityFeature()
 		{
			Debug.Assert(this.FeatureCollection != null); // real assert
		} 
 
	}
 
    public interface IHashableFeature
    {
    }


	 
} 

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source. 
// Copyright (c) Microsoft Corporation. All rights reserved.
