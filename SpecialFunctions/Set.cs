using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Msr.Mlas.SpecialFunctions
{ 
    public class Set<T> : IEnumerable<T> 
    {
        protected Dictionary<T, Ignore> Dictionary; 

        //Get our own copy of the Singleton to avoid having to make the GetInstance call
        static Ignore IgnoreGetInstance = Ignore.GetInstance();

        public Set()
        { 
            Dictionary = new Dictionary<T, Ignore>(); 
        }
 
        public Set(T element) : this()
        {
            AddNew(element);
        }

        public Set(IEnumerable<T> collection) : this() 
        { 
              AddNewOrOldRange(collection);
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
                return Dictionary.Count; 
            }
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
            return Dictionary.ContainsKey(element);
        } 
 
        public Set<T> Subtract(Set<T> featureStringSet)
        { 
            Set<T> result = Set<T>.GetInstance();
            foreach (T item1 in Dictionary.Keys)
            {
                if (!featureStringSet.Contains(item1))
                {
                    result.AddNew(item1); 
                } 
            }
            return result; 

        }

        //public static void UnionToFirst<T>(ref Set everyFeatureNameSet, Set remainingFeatureStringSet)
        //{
        //    foreach (T item1 in remainingFeatureStringSet.Keys) 
        //    { 
        //        everyFeatureNameSet[item1);
        //    } 
        //}



        public void Add(T element, bool checkThatNew)
        { 
            if (checkThatNew) 
            {
                Dictionary.Add(element, IgnoreGetInstance); 
            }
            else
            {
                Dictionary[element] = IgnoreGetInstance;
            }
        } 
 
        public virtual void AddNew(T element)
        { 
            Dictionary.Add(element, IgnoreGetInstance);
        }

        public virtual void AddNewOrOld(T element)
        {
            Dictionary[element] = IgnoreGetInstance; 
        } 

 
        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Dictionary.Keys.GetEnumerator();
        } 
 
        #endregion
 
        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Dictionary.Keys.GetEnumerator();
        } 
 
        #endregion
 
        public virtual void Remove(T element)
        {
            Dictionary.Remove(element);
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
            if (other.Count != Count) 
            { 
                return false;
            } 
            ///!!!!scan smaller??
            foreach (T element in this)
            {
                if (!other.Contains(element))
                {
                    return false; 
                } 
            }
            return true; 
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
            string s = "{" + SpecialFunctions.Join(",", this) + "}";
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
 
    }

    /// <summary>
    /// Implements GetHashCode as a running xor over the hash codes of the elements. Running time is the same as for Set, with the
    /// exception of AddNewOrOld, which now must check for existence of the element. This adds a minor constant time to this operation.
    /// </summary> 
    /// <typeparam name="T"></typeparam> 
    public class HashableSet<T> : Set<T>
    { 
        private int _hashCode;

        public HashableSet()
        {
            Dictionary = new Dictionary<T, Ignore>();
        } 
 
        public HashableSet(T element) : this()
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
            if (!Dictionary.ContainsKey(element)) 
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

    // Used when the class doesn't matter 
    public struct Ignore
    {
        static Ignore Singleton = new Ignore();
        public static Ignore GetInstance()
        {
            return Singleton; 
        } 
    }
 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
