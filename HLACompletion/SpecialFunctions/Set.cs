using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Msr.Mlas.SpecialFunctions
{ 
 
    public class Set<T> : IEnumerable<T>
    { 
        protected HashSet<T> HashSet;

        public Set()
        {
            HashSet = new HashSet<T>();
        } 
 
        public Set(IEqualityComparer<T> comparer)
        { 
            HashSet = new HashSet<T>(comparer);
        }

        public Set(T element)
            : this()
        { 
            AddNew(element); 
        }
 
        public Set(IEnumerable<T> collection)
            : this()
        {
            AddNewOrOldRange(collection);
        }
 
        public Set(T element, IEqualityComparer<T> comparer) 
            : this(comparer)
        { 
            AddNew(element);
        }

        public Set<TOutput> ConvertAllNew<TOutput>(Converter<T, TOutput> converter)
        {
            Set<TOutput> outSet = Set<TOutput>.GetInstance(); 
            foreach (T t in this) 
            {
                outSet.AddNew(converter(t)); 
            }
            return outSet;
        }

        static public Set<T> GetInstance()
        { 
            return new Set<T>(); 
        }
 
        static public Set<T> GetInstance(T element)
        {
            return new Set<T>(element);
        }

        static public Set<T> GetInstance(IEnumerable<T> collection) 
        { 
            return new Set<T>(collection);
        } 

        static public Set<T> GetInstance(IEnumerable<T> collection, Predicate<T> predicate)
        {
            Set<T> set = new Set<T>();
            foreach (T t in collection)
            { 
                if (predicate(t)) 
                {
                    set.AddNewOrOld(t); 
                }
            }
            return set;
        }

        static public Set<T> GetInstance<T1>(IEnumerable<T1> collection, Converter<T1, T> converter) 
        { 
            Set<T> set = new Set<T>();
            foreach (T1 t1 in collection) 
            {
                set.AddNewOrOld(converter(t1));
            }
            return set;
        }
 
 

        static public Set<T> GetInstance(params T[] elementParams) 
        {
            return new Set<T>(elementParams);
        }
        //!!!fix argument names

        public int Count 
        { 
            get
            { 
                return HashSet.Count;
            }
        }

        public void Clear()
        { 
            this.HashSet.Clear(); 
        }
 
        public bool IsSubsetOf(Set<T> other)
        {
            foreach (T t in this)
            {
                if (!other.Contains(t))
                { 
                    return false; 
                }
            } 
            return true;
        }

        //!!!change T to <T> and fix argument names
        public Set<T> Intersection(Set<T> other)
        { 
            if (Count < other.Count) 
            {
                return IntersectionInternal(other); 
            }
            else
            {
                return other.IntersectionInternal(this);
            }
        } 
 
        private Set<T> IntersectionInternal(Set<T> other)
        { 
            Set<T> intersection = Set<T>.GetInstance();
            foreach (T item1 in this)
            {
                if (other.Contains(item1))
                {
                    intersection.AddNew(item1); 
                } 
            }
            return intersection; 
        }

        public int IntersectionCount(Set<T> set2)
        {
            if (Count < set2.Count)
            { 
                return IntersectionCountInternal(set2); 
            }
            else 
            {
                return set2.IntersectionCountInternal(this);
            }
        }

        private int IntersectionCountInternal(Set<T> set2) 
        { 
            int count = 0;
            foreach (T item1 in this) 
            {
                if (set2.Contains(item1))
                {
                    ++count;
                }
            } 
            return count; 
        }
 

        public bool Contains(T element)
        {
            return HashSet.Contains(element);
        }
 
        public Set<T> Subtract(Set<T> featureStringSet) 
        {
            Set<T> result = Set<T>.GetInstance(); 
            foreach (T item1 in HashSet)
            {
                if (!featureStringSet.Contains(item1))
                {
                    result.AddNew(item1);
                } 
            } 
            return result;
 
        }

        public void Add(T element, bool checkThatNew)
        {
            if (checkThatNew && HashSet.Contains(element))
            { 
                throw new ArgumentException("Set already contrains element: " + element.ToString()); 
            }
            HashSet.Add(element); 
        }

        public virtual void AddNew(T element)
        {
            Add(element, true);
        } 
 
        public virtual void AddNewOrOld(T element)
        { 
            HashSet.Add(element);
        }


        #region IEnumerable<T> Members
 
        IEnumerator<T> IEnumerable<T>.GetEnumerator() 
        {
            return HashSet.GetEnumerator(); 
        }

        #endregion

        #region IEnumerable Members
 
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() 
        {
            return HashSet.GetEnumerator(); 
        }

        #endregion

        public virtual void Remove(T element)
        { 
            HashSet.Remove(element); 
        }
 

        public virtual bool KeysEqual<Tv2>(IDictionary<T, Tv2> set2)
        {
            if (Count != set2.Count)
            {
                return false; 
            } 

            //!!!scan smaller? 
            foreach (T key in this)
            {
                //Debug.Assert(set1[key]); // real assert - all values must be "true"
                if (!set2.ContainsKey(key))
                {
                    return false; 
                } 
                else
                { 
                    // Debug.Assert(set2[key]); // real assert - all values must be "true"
                }
            }
            return true;
        }
 
        public override bool Equals(object obj) 
        {
            Set<T> other = obj as Set<T>; 
            if (other == null)
            {
                return false;
            }
            return Equals(other);
        } 
 
        public bool Equals(Set<T> other)
        { 
            return HashSet.SetEquals(other.HashSet);
        }

        public override int GetHashCode()
        {
            Debug.Fail("Need code"); 
            return base.GetHashCode(); 
        }
 

        public virtual Set<T> Clone()
        {
            return GetInstance(this);
        }
 
        public override string ToString() 
        {
            string s = "{" + this.StringJoin(",") + "}"; 
            return s;
        }
        public T[] ToArray()
        {
            T[] result = new T[Count];
            int idx = 0; 
            foreach (T item in this) 
            {
                result[idx++] = item; 
            }
            return result;
        }

        public void AddNewOrOldRange(IEnumerable<T> set)
        { 
            foreach (T item in set) 
            {
                AddNewOrOld(item); 
            }
        }

        public void AddNewRange(IEnumerable<T> set)
        {
            foreach (T item in set) 
            { 
                AddNew(item);
            } 
        }
        public Set<T> Union(Set<T> set)
        {
            Set<T> union = Clone();
            union.AddNewOrOldRange(set);
            return union; 
        } 

        public Set<T> Union(T element) 
        {
            Set<T> union = Clone();
            union.AddNewOrOld(element);
            return union;
        }
 
        public Set<T> SubtractElement(T element) 
        {
            Set<T> set = Clone(); 
            set.Remove(element);
            return set;
        }

        public double Cosine(Set<T> set2)
        { 
            double cosine = (double)IntersectionCount(set2) / (Math.Sqrt(Count) * Math.Sqrt(set2.Count)); 
            return cosine;
        } 




        public void RemoveIfPresent(T element)
        { 
            if (Contains(element)) 
            {
                Remove(element); 
            }
        }

        public T AnyElement()
        {
            foreach (T t in this) 
            { 
                return t;
            } 
            SpecialFunctions.CheckCondition(false, "An empty set has no elements");
            return default(T);
        }

        public bool IntersectionIsEmpty(Set<T> set2)
        { 
            if (Count < set2.Count) 
            {
                return IntersectionIsEmptyInternal(set2); 
            }
            else
            {
                return set2.IntersectionIsEmptyInternal(this);
            }
        } 
 
        private bool IntersectionIsEmptyInternal(Set<T> set2)
        { 
            foreach (T item1 in this)
            {
                if (set2.Contains(item1))
                {
                    return false;
                } 
            } 
            return true;
        } 

        static public Set<T> Parse(string s)
        {
            Set<T> result;
            if (!TryParse(s, out result))
            { 
                throw new FormatException(string.Format(@"Can't parse ""{0}"" as set", s)); 
            }
            return result; 
        }

        /// <summary>
        /// Works with or without the outer "{}"
        /// </summary>
        /// <typeparam name="T"></typeparam> 
        /// <param name="s"></param> 
        /// <param name="outputSet"></param>
        /// <returns></returns> 
        static public bool TryParse<T1>(string s, out Set<T1> outputSet)
        {
            if (s.Length >= 2 && s[0] == '{' && s[s.Length-1] == '}')
            {
                s = s.Substring(1, s.Length - 2);
            } 
 
            List<T1> outputList;
            if (!SpecialFunctions.TryParse(s, out outputList)) 
            {
                outputSet = null;
                return false;
            }

            //!!!but what if this gets an exception??? 
            outputSet = Set<T1>.GetInstance(outputList); 
            return true;
        } 


    }

    /// <summary>
    /// Running time is the same as for Set, with the 
    /// exception of AddNewOrOld, which now must check for existence of the element. This adds a minor constant time to this operation. 
    /// </summary>
    /// <typeparam name="T"></typeparam> 
    public class HashableSet<T> : Set<T>
    {
        private int _hashCode;

        public HashableSet() :base()
        { 
 
        }
 
        public HashableSet(T element)
            : this()
        {
            AddNew(element);
        }
 
        public HashableSet(IEnumerable<T> collection) 
            : this()
        { 
            foreach (T element in collection)
            {
                AddNew(element);
            }
        }
 
        new static public HashableSet<T> GetInstance() 
        {
            return new HashableSet<T>(); 
        }

        new static public HashableSet<T> GetInstance(T element)
        {
            return new HashableSet<T>(element);
        } 
 
        new static public HashableSet<T> GetInstance(IEnumerable<T> collection)
        { 
            return new HashableSet<T>(collection);
        }

        public override void AddNew(T element)
        {
            base.AddNew(element); 
            _hashCode ^= element.GetHashCode(); 
        }
 
        public override void AddNewOrOld(T element)
        {
            if (!base.HashSet.Contains(element))
                _hashCode ^= element.GetHashCode();
            base.AddNewOrOld(element);
        } 
 
        public override void Remove(T element)
        { 
            base.Remove(element);
            _hashCode ^= element.GetHashCode();
        }

        public override Set<T> Clone()
        { 
            return GetInstance(this); 
        }
 
        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj) 
        { 
            return this.GetHashCode() == obj.GetHashCode() && base.Equals(obj);
        } 


    }

    public static class HashSetExtensions
    { 
        public static void AddNew<T>(this HashSet<T> hashSet, T element) 
        {
            bool didAdd = hashSet.Add(element); 
            if (!didAdd)
            {
                throw new ArgumentException("Set already contains element: " + element.ToString());
            }
        }
 
        public static void AddNewRange<T>(this HashSet<T> hashSet, IEnumerable<T> iEnumerable) 
        {
            foreach (T t in iEnumerable) 
            {
                hashSet.AddNew(t);
            }
        }

        public static void AddNewOrOldRange<T>(this HashSet<T> hashSet, IEnumerable<T> iEnumerable) 
        { 
            foreach (T t in iEnumerable)
            { 
                hashSet.Add(t);
            }
        }


        //!!!Is SetEquals, for which the 2nd argument is IEnumerable, really as fast as an algorithm that knows they don't have duplicate values, checks length, and then checks that each element of one is in the other? 
        [Obsolete("Use 'SetEquals' instead")] 
        public static bool IsEqualTo<T>(this HashSet<T> hashSet1, HashSet<T> hashSet2)
        { 
            return hashSet1.SetEquals(hashSet2);
        }

        public static HashSet<T> GetInstanceFromNewRange<T>(IEnumerable<T> iEnumerable)
        {
            HashSet<T> hashSet = new HashSet<T>(); 
            hashSet.AddNewRange(iEnumerable); 
            return hashSet;
        } 
    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
