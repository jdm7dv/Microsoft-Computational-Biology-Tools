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
            SerialNumbers<T> serialNumbers = new SerialNumbers<T>();
            serialNumbers.ItemToSerialNumber = new Dictionary<T, int>();
            serialNumbers.ItemList = new List<T>();
            return serialNumbers;
        }
 
        static public SerialNumbers<T> GetInstance(IEnumerable<T> list) 
        {
            SerialNumbers<T> serialNumbers = GetInstance(); 
            foreach (T item in list)
            {
                serialNumbers.GetNewOrOld(item);
            }
            return serialNumbers;
        } 
 

        public Dictionary<T, int> ItemToSerialNumber; 
        public List<T> ItemList;

        public int GetNewOrOld(T item)
        {
            if (!ItemToSerialNumber.ContainsKey(item))
            { 
                Debug.Assert(ItemToSerialNumber.Count == ItemList.Count); // real assert 
                int serialNumber = ItemToSerialNumber.Count;
                Debug.Assert(serialNumber == ItemList.Count); // real assert 
                ItemToSerialNumber.Add(item, serialNumber);
                ItemList.Add(item);
                return serialNumber;
            }
            else
            { 
                int serialNumber = ItemToSerialNumber[item]; 
                return serialNumber;
            } 
        }

        public int GetNew(T item)
        {
            SpecialFunctions.CheckCondition(!ItemToSerialNumber.ContainsKey(item), "item seen more than once. " + item.ToString());
            Debug.Assert(ItemToSerialNumber.Count == ItemList.Count); // real assert 
            int serialNumber = ItemToSerialNumber.Count; 
            Debug.Assert(serialNumber == ItemList.Count); // real assert
            ItemToSerialNumber.Add(item, serialNumber); 
            ItemList.Add(item);
            return serialNumber;
        }


        public int GetOld(T item) 
        { 
            SpecialFunctions.CheckCondition(ItemToSerialNumber.ContainsKey(item), "Expected to have seen " + item + " before.");
            return ItemToSerialNumber[item]; 
        }


        public int Last
        {
            get 
            { 
                return ItemList.Count - 1;
            } 
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
 
        public T GetItem(int serialNumber) 
        {
            return ItemList[serialNumber]; 
        }


        public static SerialNumbers<string> ReadStringFeaturesFromFile(string featureFileName)
        {
            SerialNumbers<string> featureSerialNumbers = SerialNumbers<string>.GetInstance(); 
            foreach (string feature in SpecialFunctions.ReadEachLine(featureFileName)) 
            {
                featureSerialNumbers.GetNew(feature); 
            }
            return featureSerialNumbers;
        }

        /// <summary>
        /// Write the items in order to a file 
        /// </summary> 
        public void Save(string fileName)
        { 
            SpecialFunctions.WriteEachLine(ItemList, fileName);
        }

    }
}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
