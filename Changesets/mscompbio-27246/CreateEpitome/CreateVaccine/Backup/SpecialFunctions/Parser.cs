using System; 
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Msr.Mlas.SpecialFunctions 
{ 
 	public class Parser
	{ 
        /// <summary>
        /// This method should be updated to use the rest of the methods in this class.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="type"></param>
        /// <returns></returns> 
        public static object Parse(string field, Type type) 
        {
            if (type.Equals(typeof(string))) 
            {
                return field;
            }
            if (type.Equals(typeof(int)))
            {
                return int.Parse(field); 
            } 
            if (type.Equals(typeof(double)))
            { 
                return double.Parse(field);
            }
            if (type.Equals(typeof(bool)))
            {
                return bool.Parse(field);
            } 
            if (type.Equals(typeof(DateTime))) 
            {
                return DateTime.Parse(field); 
            }
            SpecialFunctions.CheckCondition(false, "Don't know how to parse type " + type.Name);
            return null;
        }

		/// <summary> 
		/// Will parse s into T, provided T has a Parse(string) or TryParse(string s, out T t) method defined, or is one of the magical 
		/// special cases we've implemented (including ICollection (comma delimited), Nullable and Enums).
		/// </summary> 
 		/// <typeparam name="T"></typeparam>
 		/// <param name="s"></param>
		/// <returns></returns>
 		public static T Parse<T>(string s)
		{
			T t; 
			if (TryParse(s, out t)) 
			{
				return t; 
 			}
 			else
			{
 				throw new ArgumentException(string.Format("Could not parse {0} into an instance of type {1}", s, typeof(T)));
			}
		} 
 
		/// <summary>
		/// Will parse s into T, provided T has a Parse(string) or TryParse(string s, out T t) method defined, or is one of the magical 
		/// special cases we've implemented (including ICollection (comma delimited), Nullable and Enums).
 		/// </summary>
 		/// <typeparam name="T"></typeparam>
		/// <param name="s"></param>
 		/// <returns></returns>
		public static bool TryParse<T>(string s, out T t) 
		{ 
			Type type = typeof(T);
			if (s is T) 
			{
 				return StringTryParse(s, out t);
 			}
			else if (type.IsEnum)
 			{
				return EnumTryParse(s, out t); 
			} 
            //else if (type.IsGenericType)
            //{ 
            //    if (type.FindInterfaces(Module.FilterTypeNameIgnoreCase, "ICollection*").Length > 0)
            //    {
            //        return CollectionsTryParse(s, out t);
            //    }
            //    else if (type.Name.StartsWith("Nullable"))
            //    { 
            //        return NullableTryParse(s, out t); 
            //    }
            //} 
			
 			return GenericTryParse(s, out t);
 		}

		private static bool NullableTryParse<T>(string s, out T t)
 		{ 
			t = default(T); 
			if (string.IsNullOrEmpty(s) || s.Equals("null", StringComparison.CurrentCultureIgnoreCase))
			{ 
				return true;
			}

 			Type type = typeof(T);
 			Type underlyingType = type.GetGenericArguments()[0];
			//underlyingType.TypeInitializer 
 			MethodInfo tryParse = typeof(Parser).GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static); 
			MethodInfo genericTryParse = tryParse.MakeGenericMethod(underlyingType);
 
			object[] args = new object[] { s, Activator.CreateInstance(underlyingType) };

			bool success = (bool)genericTryParse.Invoke(null, args);
			if (success)
			{
 				t = (T)args[1]; 
 			} 
			return success;
 		} 

		private static bool StringTryParse<T>(string s, out T t)
		{
			t = (T)(object)s;
			return true;
		} 
 
 		private static bool CollectionsTryParse<T>(string s, out T t)
 		{ 
			Type type = typeof(T);
 			Type genericType = type.GetGenericArguments()[0];

			MethodInfo collectionTryParse = typeof(Parser).GetMethod("GenericCollectionsTryParse", BindingFlags.NonPublic | BindingFlags.Static);
			MethodInfo genericCollectionTryParse = collectionTryParse.MakeGenericMethod(type, genericType);
			t = default(T); 
			object[] args = new object[] { s, t }; 

			bool success = (bool)genericCollectionTryParse.Invoke(null, args); 
 			if (success)
 			{
				t = (T)args[1];
 			}
			return success;
		} 
 
		private static bool GenericCollectionsTryParse<T,S>(string s, out T t) where T : ICollection<S>, new()
		{ 
			t = new T();

 			foreach (string itemAsString in s.Split(','))
 			{
				S item;
 				if (TryParse<S>(itemAsString, out item)) 
				{ 
					t.Add(item);
				} 
				else
				{
 					t = default(T);
 					return false;
				}
 			} 
			return true; 
		}
 
		private static bool EnumTryParse<T>(string s, out T t)
		{
			int i;
 			if (int.TryParse(s, out i))
 			{
				t = (T)(object)i; 
 				return true; 
			}
 
			try
			{
				t = (T)Enum.Parse(typeof(T), s, true);
				return true;
 			}
 			catch (ArgumentException) 
			{ 
 			}
			t = default(T); 
			return false;
		}

		//private static bool NullableTryParse<T>(string s, out T t) where T:System.Nullable
		//{
 		//	if (string.IsNullOrEmpty(s) || s.Equals("null", StringComparison.CurrentCultureIgnoreCase)) 
 		//	{ 
		//		return null;
 		//	} 


		//}

		private static bool GenericTryParse<T>(string s, out T t)
		{ 
			// now the general one. 
			bool success = false;
 			t = default(T); 
 			Type type = typeof(T);

			MethodInfo tryParse = type.GetMethod("TryParse", new Type[] { typeof(string), type.MakeByRefType() });

 			if (tryParse != null)
			{ 
				object[] args = new object[] { s, t }; 

				success = (bool)tryParse.Invoke(null, args); 

				if (success)
				{
 					t = (T)args[1];
 				}
			} 
 			else 
			{
				MethodInfo parse = type.GetMethod("Parse", new Type[] { typeof(string) }); 
				SpecialFunctions.CheckCondition(parse != null, string.Format("Cannot parse type {0}. It does not have a TryParse or Parse method defined", typeof(T)));

				try
				{
 					object[] args = new object[] { s };
 					t = (T)parse.Invoke(null, args); 
					success = true; 
 				}
				catch { } 
			}

			return success;
		}
	}
} 

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL)
// Copyright (c) Microsoft Corporation. All rights reserved.
