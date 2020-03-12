using System; 
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Msr.Mlas.SpecialFunctions;

namespace Msr.Mlas.SpecialFunctions 
{ 
    /// <summary>
    /// Assigns serial number of objects 
    /// </summary>
    public class SerialNumbers<T>
    {
        private SerialNumbers()
        {
        } 
 
        static public SerialNumbers<T> GetInstance()
        { 
            SerialNumbers<T> aSerialNumbers = new SerialNumbers<T>();
            aSerialNumbers.ItemToSerialNumber = new Dictionary<T, int>();
            aSerialNumbers.SerialNumberToItem = new Dictionary<int, T>();
            return aSerialNumbers;
        }
 
        private Dictionary<T, int> ItemToSerialNumber; 
        private Dictionary<int, T> SerialNumberToItem;
 
        public int GetNewOrOld(T item)
        {
            if (!ItemToSerialNumber.ContainsKey(item))
            {
                int serialNumber = ItemToSerialNumber.Count;
                Debug.Assert(serialNumber == SerialNumberToItem.Count); // real assert 
                ItemToSerialNumber.Add(item, serialNumber); 
                SerialNumberToItem.Add(serialNumber, item);
                return serialNumber; 
            }
            else
            {
                return ItemToSerialNumber[item];
            }
        } 
 
        public int GetOld(T item)
        { 
            SpecialFunctions.CheckCondition(ItemToSerialNumber.ContainsKey(item));
            return ItemToSerialNumber[item];
        }


        public int Last 
        { 
            get
            { 
                return SerialNumberToItem.Count - 1;
            }
        }

        public IEnumerable<KeyValuePair<int, T>> Mapping()
        { 
            return SerialNumberToItem; 
        }
 
        public bool TryGetOld(T item, out int serialNumber)
        {
            return ItemToSerialNumber.TryGetValue(item, out serialNumber);
        }

        public int Count 
        { 
            get
            { 
                return ItemToSerialNumber.Count;
            }
        }

        public T GetItem(int iFeature)
        { 
            return SerialNumberToItem[iFeature]; 
        }
 
        public IEnumerable<T> Items()
        {
            return SerialNumberToItem.Values;
            //foreach (T t in SerialNumberToItem.Values)
            //{
            //    yield return t; 
            //} 
        }
    } 
}

// Microsoft Research, Machine Learning and Applied Statistics Group, Shared Source.
// Copyright (c) Microsoft Corporation. All rights reserved.
