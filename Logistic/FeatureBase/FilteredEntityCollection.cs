using System; 
using System.Collections;
using System.Diagnostics;

namespace Msr.Adapt.LearningWorkbench
{
 	/// <summary> 
	/// Creates a new entity collection containing all the entitys the given entityCollection that pass the filter test. 
	/// Does not actually enumerate entitys until it is asked to,
	/// </summary> 
	public class FilteredEntityCollection : IEnumerable
	{
 		internal IEnumerable _enumerableEntityCollection;
 		internal Feature _featureKeep;
		internal Feature _featureChange;
 		internal long _iMaxNumberEntities; 
 
		public FilteredEntityCollection(Feature featureKeep, IEnumerable entityCollection) : this(entityCollection, featureKeep, null, long.MaxValue)
		{ 
		}

		public FilteredEntityCollection(IEnumerable entityCollection, Feature featureKeep, Feature featureChange, long maxNumberEntities)
		{
 			_featureKeep = featureKeep;
 			_featureChange = featureChange; 
			_iMaxNumberEntities = maxNumberEntities; 
 			Debug.Assert(featureKeep == null || featureKeep.MainMethod.ReturnType == typeof(bool)); //TODO raise error
			_enumerableEntityCollection = entityCollection;	 
		}

		public IEnumerator GetEnumerator()
		{
			return new EnumeratorFilteredEntityCollection(this);
 		} 
 	} 

	 
 	class EnumeratorFilteredEntityCollection: IEnumerator
	{
		FilteredEntityCollection _filteredEntityCollection;
		IEnumerator _enumerator;
		Object _current = null;
		long _iNumberEntities = 0; 
 
 		public EnumeratorFilteredEntityCollection(FilteredEntityCollection filteredEntityCollection)
 		{ 
			_filteredEntityCollection = filteredEntityCollection;
 			_enumerator = filteredEntityCollection._enumerableEntityCollection.GetEnumerator();
		}


		// we implement Current once, because we work for all types of entitys 
		object IEnumerator.Current 
		{
			get 
 			{
 				return _current;
			}
 		}

		public void Reset() 
		{ 
			_enumerator.Reset();
		} 

		//TODO do not assume that the EmailPath is a NPFolder
 		public bool MoveNext()
 		{
			while(_iNumberEntities < _filteredEntityCollection._iMaxNumberEntities && _enumerator.MoveNext())
 			{ 
				if (_filteredEntityCollection._featureKeep == null || (bool) _filteredEntityCollection._featureKeep.Evaluate(_enumerator.Current)) 
				{
					if (_filteredEntityCollection._featureChange != null) 
					{
						_current = _filteredEntityCollection._featureChange.Evaluate(_enumerator.Current);
 					}
 					else
					{
 						_current = _enumerator.Current; 
					} 
					++_iNumberEntities;
					return true; 
				}
			}

 			return false;
 		}
	} 
 

 	/// <summary> 
	/// Creates a new collection with no more than the max number of items
	/// Does not actually enumerate entitys until it is asked to,
	/// </summary>
	public class LimitedSizeCollection : IEnumerable
	{
 		internal IEnumerable _enumerableCollection; 
 		internal long _iMaxNumber; 

		public LimitedSizeCollection(IEnumerable enumerable, long maxNumber) 
 		{
			_iMaxNumber = maxNumber;
			_enumerableCollection = enumerable;	
		}

		public IEnumerator GetEnumerator() 
		{ 
 			return new EnumeratorLimitedSizeCollection(this);
 		} 
	}

 	
	class EnumeratorLimitedSizeCollection: IEnumerator
	{
		LimitedSizeCollection _LimitedSizeCollection; 
		IEnumerator _enumerator; 
		long _iNumber = 0;
 
 		public EnumeratorLimitedSizeCollection(LimitedSizeCollection LimitedSizeCollection)
 		{
			_LimitedSizeCollection = LimitedSizeCollection;
 			_enumerator = LimitedSizeCollection._enumerableCollection.GetEnumerator();
		}
 
 
		// we implement Current once, because we work for all types of entitys
		object IEnumerator.Current 
		{
			get
 			{
 				return _enumerator.Current;
			}
 		} 
 
		public void Reset()
		{ 
			_enumerator.Reset();
		}

		//TODO do not assume that the EmailPath is a NPFolder
 		public bool MoveNext()
 		{ 
			if(_iNumber < _LimitedSizeCollection._iMaxNumber && _enumerator.MoveNext()) 
 			{
				++_iNumber; 
				return true;
			}
			return false;
		}
 	}
} 
 
// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved. 
