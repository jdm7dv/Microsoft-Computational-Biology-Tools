using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msr.Mlas.SpecialFunctions; 
using System.Diagnostics; 
using System.Text.RegularExpressions;
 

namespace Msr.Linkdis
{
    public class HlaMsr1Factory// : HlaFactory
    {
        private HlaMsr1Factory() 
        { 
        }
 
        public static void UnitTest()
        {
            HlaMsr1Factory hlaMsr1Factory = HlaMsr1Factory.GetFactory(new int[]{4,4,4});
            HashSet<string> warningSet = new HashSet<string>();
            SpecialFunctions.CheckCondition(hlaMsr1Factory.GetGroundOrAbstractInstance("B*1234/B23", ref warningSet).ToString() == "B1234/B23");
            hlaMsr1Factory.GetGroundOrAbstractInstance("BND", ref warningSet); 
            Hla hla5 = hlaMsr1Factory.GetGroundOrAbstractInstance("B", ref warningSet); 
            SpecialFunctions.CheckCondition(hla5.ToString() == "B");
            Hla hla3 = hlaMsr1Factory.GetGroundOrAbstractInstance("C1234", ref warningSet); 
            SpecialFunctions.CheckCondition(hla3.ToString() == "C1234");
            SpecialFunctions.CheckCondition(hlaMsr1Factory.GetGroundOrAbstractInstance("B*1234", ref warningSet).ToString() == "B1234");
            SpecialFunctions.CheckCondition(hlaMsr1Factory.GetGroundOrAbstractInstance("C1234/C12/C", ref warningSet).ToString() == "C");
            Hla hla7 = hlaMsr1Factory.GetGroundOrAbstractInstance("C1234N", ref warningSet);
            SpecialFunctions.CheckCondition(hla7.ToString() == "C1234");
            Hla hla6 = hlaMsr1Factory.GetGroundOrAbstractInstance("C12", ref warningSet); 
            SpecialFunctions.CheckCondition(hla6.ToString() == "C12"); 
            Hla hla1 = hlaMsr1Factory.GetGroundOrAbstractInstance("A74", ref warningSet);
            SpecialFunctions.CheckCondition(hla1.ToString() == "A74"); 
            Hla hla2 = hlaMsr1Factory.GetGroundOrAbstractInstance("A7412", ref warningSet);
            SpecialFunctions.CheckCondition(hla2.ToString() == "A74");
            SpecialFunctions.CheckCondition(hla6.IsMoreGeneralThan(hla7));// assert
            SpecialFunctions.CheckCondition(!hla6.IsMoreGeneralThan(hla2));// assert
            SpecialFunctions.CheckCondition(hlaMsr1Factory.GetGroundOrAbstractInstance("A12345678", ref warningSet).ToString() == "A1234");
            SpecialFunctions.CheckCondition(hlaMsr1Factory.GetGroundOrAbstractInstance("B95", ref warningSet).ToString() == "B15"); 
            SpecialFunctions.CheckCondition(hlaMsr1Factory.GetGroundOrAbstractInstance("B15/B9512", ref warningSet).ToString() == "B15"); 
            Hla hla4 = hlaMsr1Factory.GetGroundOrAbstractInstance("A02/A9299", ref warningSet);
            SpecialFunctions.CheckCondition(hla4.ToString() == "A02"); 
            SpecialFunctions.CheckCondition(hla3.IsGround); // assert
            SpecialFunctions.CheckCondition(hla1.IsGround); // assert
            SpecialFunctions.CheckCondition(!hla4.IsGround); // assert
            SpecialFunctions.CheckCondition(!hla5.IsGround); // assert
            SpecialFunctions.CheckCondition(hlaMsr1Factory.GetGroundOrAbstractInstance("A1200", ref warningSet).ToString() == "A12");
            SpecialFunctions.CheckCondition(!hlaMsr1Factory.GetGroundOrAbstractInstance("B*1234/B23", ref warningSet).IsGround); 
        } 
        public HlaMsr1 GetGroundInstance(string name, ref HashSet<string> warningSet)
        { 
            HlaMsr1 hla = (HlaMsr1) GetGroundOrAbstractInstance(name, ref warningSet);
            SpecialFunctions.CheckCondition(hla.IsGround, "Hla is not ground. " + name);
            return hla;
        }

        static Regex classSplitRegex = new Regex("[ABCW*]+"); 
        public static HashSet<string> TrimToTwoList = new HashSet<string> {"C17", "C18", "A74" }; 
        public static Dictionary<string, string> MoreThan100 = new Dictionary<string, string> { { "B95", "15" }, { "A92", "02" } };
 
        public static int SortTermsAlpha(KeyValuePair<string,string> kv1, KeyValuePair<string,string> kv2)
        {
            if (null == kv1.Key)
            {
                return 1;
            } 
            if (null == kv2.Key) 
            {
                return -1; 
            }
            int compare = kv1.Key.CompareTo(kv2.Key);
            if (0 != compare)
            {
                return compare;
            } 
            return kv1.Value.CompareTo(kv2.Value); 
        }
 
        public Hla GetGroundOrAbstractInstance(string name, ref HashSet<string> warningSet)
        {
            string soFar = name.ToUpperInvariant();

            var keyAndInParenListX = CreateCanonicalTermList(soFar, name, ref warningSet);
            var keyAndOrigList = 
                (from kv in keyAndInParenListX 
                 let k = kv.Key
                 let inParen = kv.Value 
                 let ori = (inParen == "") ? k : string.Format("{0}({1})", k, inParen)
                 select new KeyValuePair<string, string>(k, ori)).ToList();


            RemoveRedundantTerms(ref keyAndOrigList, ref warningSet);
            keyAndOrigList.Sort(SortTermsAlpha); 
 

 
            HlaMsr1 hlaMsrMsr1 = new HlaMsr1(keyAndOrigList);

            SetGroundOnHla(keyAndOrigList, hlaMsrMsr1);

            return hlaMsrMsr1;
        } 
 
        private void SetGroundOnHla(List<KeyValuePair<string, string>> termList, HlaMsr1 hlaMsrMsr1)
        { 
            if (termList.Count > 1)
            {
                hlaMsrMsr1._isGround = false;
            }
            else
            { 
                SetGroundOnSingleTermHla(hlaMsrMsr1); 
            }
 
        }

        private void SetGroundOnSingleTermHla(HlaMsr1 hlaMsrMsr1)
        {
            int hlaLength = ClassNameToHlaLength[hlaMsrMsr1.ClassName];
            switch (hlaLength) 
            { 
                case 4:
                    { 
                        if (hlaMsrMsr1.Name.Length == 1)
                        {
                            hlaMsrMsr1._isGround = false;
                        }
                        else if (HlaMsr1Factory.TrimToTwoList.Contains(hlaMsrMsr1.Name))
                        { 
                            hlaMsrMsr1._isGround = true; 
                        }
                        else 
                        {

                            hlaMsrMsr1._isGround = hlaMsrMsr1.Name.Length == 5;
                        }

                    } 
                    break; 
                case 2:
                    { 
                        if (hlaMsrMsr1.Name.Length == 1)
                        {
                            hlaMsrMsr1._isGround = false;
                        }
                        else
                        { 
 
                            hlaMsrMsr1._isGround = hlaMsrMsr1.Name.Length == 3;
                        } 
                    }
                    break;
                case 0:
                    {

                        hlaMsrMsr1._isGround = hlaMsrMsr1.Name.Length == 1; 
                    } 
                    break;
                default: 
                    SpecialFunctions.CheckCondition(false, "Don't have code for hlaLength " + hlaLength.ToString());
                    break;
            }
        }

        private List<KeyValuePair<string, string>> CreateCanonicalTermList(string soFar, string name, ref HashSet<string> warningSet) 
        { 
            List<KeyValuePair<string, string>> termList = new List<KeyValuePair<string,string>>();
            SingletonSet<string> classNameSet = SingletonSet<string>.GetInstance("All terms must have the same class. " + name); 
            int previousNumberLength = 0;

            foreach (string term in soFar.Split('/'))
            {

                string classNameSoFarOrNull; 
                string numbersSoFarX = SplitIntoClassAndNumber(name, classNameSet, term, out classNameSoFarOrNull); 

                var numbersSoFarAndInParen = CanonalizeNumbers(numbersSoFarX, classNameSet.First(), warningSet); 

                SpecialFunctions.CheckCondition((null != classNameSoFarOrNull) || previousNumberLength <= numbersSoFarAndInParen.Key.Length, "When a class name is left off after a slash, the number have at least as many digits as the previous number. " + name);

                termList.Add(new KeyValuePair<string,string>(classNameSet.First() + numbersSoFarAndInParen.Key, numbersSoFarAndInParen.Value));
                previousNumberLength = numbersSoFarAndInParen.Key.Length;
            } 
            return termList; 
        }
 
        private string SplitIntoClassAndNumber(string name, SingletonSet<string> classNameSet, string term, out string classNameSoFarOrNull)
        {

            string numbersSoFarX;
            classNameSoFarOrNull = CanonalizeClassOrNull(term, name, out numbersSoFarX);
            if (null != classNameSoFarOrNull) 
            { 
                classNameSet.Add(classNameSoFarOrNull);
            } 
            SpecialFunctions.CheckCondition(!classNameSet.IsEmpty, "Class name required. " + name);
            return numbersSoFarX;
        }

        //!!!Perforance ideas: Could the backoff models be applied better than just one at a time until one works?
        //!!!Perforance ideas: Does the web use load all the models on every use? Is this a problem? 
 
        private static void RemoveRedundantTerms(ref List<KeyValuePair<string, string>> termAndInParenList, ref HashSet<string> warningSet)
        { 
            termAndInParenList.Sort((termAndInParen1, termAndInParen2) => termAndInParen1.Key.Length.CompareTo(termAndInParen2.Key.Length));


            for (int i = termAndInParenList.Count - 1; i > 0; --i)
            {
                string keyI = termAndInParenList[i].Key; 
                for (int j = 0; j < i; ++j) 
                {
                    string keyJ = termAndInParenList[j].Key; 
                    if (HlaMsr1.IsMoreGeneralOrEqualThan(keyJ, keyI))
                    {
                        if (keyJ == keyI)
                        {
                            termAndInParenList[j] = new KeyValuePair<string, string>(keyJ, termAndInParenList[j].Value + "/" + termAndInParenList[1].Value);
                            termAndInParenList.RemoveAt(i); 
                            warningSet.Add("Two terms combined because they are equal at the generality used by the model."); 
                            break; // really break, not continue;
                        } 
                        throw new GeneralizingTermException(keyJ, keyI, string.Format("Term {0} generalizes term {1}", keyJ, keyI));
                    }
                }
            }
        }
 
        static Regex AllNumbers = new Regex("^[0-9]*$"); 

        private KeyValuePair<string, string> CanonalizeNumbers(string numbers, string className, HashSet<string> warningSet) 
        {
            string numberSoFar = numbers;
            string inParen = "";

            numberSoFar = RemoveAnyOneSetOfParens(warningSet, numberSoFar);
            RemoveAnyTrailingLetter(warningSet, ref numberSoFar, ref inParen); 
            SpecialFunctions.CheckCondition(numberSoFar.Length % 2 == 0, "The number part of an hla name must be of even length. " + className + numbers); 
            SpecialFunctions.CheckCondition(AllNumbers.IsMatch(numberSoFar), "The number part of an hla name must be digits. " + className + numbers);
            RemoveAnyTrailing00s(warningSet, ref numberSoFar, ref inParen); 
            ApplyTrimToTwoIfNeeded(className, warningSet, ref numberSoFar, ref inParen);
            TrimToFourDigitsIfNeeded(warningSet, ref numberSoFar, ref inParen);
            numberSoFar = ConvertTwoDigitB95OrA92(className, warningSet, numberSoFar);
            ShortenForBackoffModelIfNeeded(className, warningSet, ref numberSoFar, ref inParen);

            return new KeyValuePair<string, string>(numberSoFar, inParen); 
        } 

        private void ShortenForBackoffModelIfNeeded(string className, HashSet<string> warningSet, ref string numberSoFar, ref string inParen) 
        {
            int backOffLength = ClassNameToHlaLength[className];
            if (backOffLength < numberSoFar.Length)
            {
                inParen = numberSoFar.Substring(backOffLength) + inParen;
                numberSoFar = numberSoFar.Substring(0, backOffLength); 
                warningSet.Add(string.Format("Shortened {0}'s to {1} digits for backoff model", className, backOffLength)); ; 
            }
        } 

        private static string ConvertTwoDigitB95OrA92(string className, HashSet<string> warningSet, string numberSoFar)
        {
            if (MoreThan100.ContainsKey(className + numberSoFar))
            {
                numberSoFar = MoreThan100[className + numberSoFar]; 
                warningSet.Add("Converted two-digit B95->B15 or A92->A02."); 
            }
            return numberSoFar; 
        }

        private static void TrimToFourDigitsIfNeeded(HashSet<string> warningSet, ref string numberSoFar, ref string inParen)
        {
            if (numberSoFar.Length > 4)
            { 
                inParen = numberSoFar.Substring(4) + inParen; 
                numberSoFar = numberSoFar.Substring(0, 4);
                warningSet.Add("Allele name trimmed to four digits."); 
            }
        }

        private static void ApplyTrimToTwoIfNeeded(string className, HashSet<string> warningSet, ref string numberSoFar, ref string inParen)
        {
            if (numberSoFar.Length > 2) 
            { 
                if (TrimToTwoList.Contains(className + numberSoFar.Substring(0, 2)))
                { 
                    inParen = numberSoFar.Substring(2) + inParen;
                    numberSoFar = numberSoFar.Substring(0, 2);
                    warningSet.Add("Trimmed C17, C18, or A74 to two digits to fit model.");
                }
            }
        } 
 
        private static void RemoveAnyTrailing00s(HashSet<string> warningSet, ref string numberSoFar, ref string inParen)
        { 
            while (numberSoFar.EndsWith("00"))
            {
                inParen = numberSoFar.Substring(numberSoFar.Length - 2) + inParen;
                numberSoFar = numberSoFar.Substring(0, numberSoFar.Length - 2);
                warningSet.Add("Trailing '00's removed from allele name.");
            } 
        } 

        private static void RemoveAnyTrailingLetter(HashSet<string> warningSet, ref string numberSoFar, ref string inParen) 
        {
            if (numberSoFar.Length > 2 && numberSoFar.Length % 2 == 1 && char.IsLetter(numberSoFar[numberSoFar.Length - 1]))
            {
                inParen = numberSoFar.Substring(numberSoFar.Length - 1) + inParen;
                numberSoFar = numberSoFar.Substring(0, numberSoFar.Length - 1);
                warningSet.Add("Trailing letter removed from allele name."); 
 
            }
        } 

        private static string RemoveAnyOneSetOfParens(HashSet<string> warningSet, string numberSoFar)
        {
            int leftParenPos = numberSoFar.IndexOf('(');
            if (leftParenPos >= 0 && numberSoFar.EndsWith(")"))
            { 
                numberSoFar = numberSoFar.Remove(leftParenPos, 1).Substring(0, numberSoFar.Length - 2); 
                warningSet.Add("Parentheses removed from alelle name.");
            } 
            return numberSoFar;
        }

        private string CanonalizeClassOrNull(string term, string name, out string numbers)
        {
            string classNameOrNull; 
            string[] classFields = classSplitRegex.Split(term, 2); 
            if (classFields.Length == 1)
            { 
                classNameOrNull = null;
                numbers = classFields[0];
            }
            else
            {
                SpecialFunctions.CheckCondition(classFields.Length == 2, "Each slash-delimited term must start with a class letter. " + name); 
                SpecialFunctions.CheckCondition(classFields[0] == "", "Each slash-delimited term must start with a class letter. " + name); 
                string classNameIn = term.Substring(0, term.Length - classFields[1].Length);
                classNameOrNull = CanonicalClassNameOrNull(classNameIn); 
                SpecialFunctions.CheckCondition(classNameOrNull != null, "Class name not recognized. " + classNameIn);
                numbers = classFields[1];
            }

            return classNameOrNull;
        } 
 

        private string CanonicalClassNameOrNull(string inputName) 
        {
            Debug.Assert(inputName == inputName.ToUpperInvariant());// real assert
            string soFar = inputName;
            if (soFar.Length > 1 && soFar.EndsWith("*"))
            {
                soFar = soFar.Substring(0, soFar.Length - 1); 
            } 
            if (soFar == "CW")
            { 
                soFar = "C";
            }

            if (soFar == "A" || soFar == "B" || soFar == "C")
            {
                return soFar; 
            } 
            else
            { 
                return null;
            }
        }

        Dictionary<string, int> ClassNameToHlaLength;
 
        public static HlaMsr1Factory GetFactory(IEnumerable<int> hlaLengthList) 
        {
            HlaMsr1Factory hlaMsr1Factory = new HlaMsr1Factory(); 
            hlaMsr1Factory.ClassNameToHlaLength = new Dictionary<string, int>();

            foreach(KeyValuePair<string,int> classNameAndHlaLength in SpecialFunctions.EnumerateTwo(new string[]{"A", "B", "C"}, hlaLengthList, false))
            {
                hlaMsr1Factory.ClassNameToHlaLength.Add(classNameAndHlaLength.Key, classNameAndHlaLength.Value);
            } 
            return hlaMsr1Factory; 
        }
    } 

    public class HlaMsr1 : Hla, IComparable<HlaMsr1>
    {
        internal HlaMsr1(List<KeyValuePair<string,string>> nameAndOriList)
        {
            Name = nameAndOriList.Select(kv => kv.Key).StringJoin("/"); 
            NameAndOriList = nameAndOriList; 
        }
        List<KeyValuePair<string, string>> NameAndOriList; 

        internal static bool IsMoreGeneralOrEqualThan(string noSlashTerm1, string noSlashTerm2)
        {
            if (noSlashTerm2.StartsWith(noSlashTerm1))
            {
                return true; 
            } 
            if (noSlashTerm1 == "B15" && noSlashTerm2.StartsWith("B95"))
            { 
                return true;
            }
            if (noSlashTerm1 == "A02" && noSlashTerm2.StartsWith("A92"))
            {
                return true;
            } 
 
            return false;
        } 

        public override bool IsMoreGeneralThan(Hla hla)
        {
            SpecialFunctions.CheckCondition(!IsGround && hla.IsGround, "IsMoreGeneralThan is only defined between an abstract and ground hla");
            foreach (var nameAndOri in NameAndOriList)
            { 
                if (IsMoreGeneralOrEqualThan(nameAndOri.Key, hla.Name)) 
                {
                    return true; 
                }
            }
            return false;
        }

        internal bool _isGround; 
        public override bool IsGround 
        {
            get 
            {
                return _isGround;
            }
        }

        public string ClassName 
        { 
            get
            { 
                return Name.Substring(0, 1);
            }
        }

        internal IEnumerable<HlaMsr1> GeneralizationList(HlaMsr1Factory hlaMsr1Factory)
        { 
            HashSet<string> warningSetToIgnore = new HashSet<string>(); 

            SpecialFunctions.CheckCondition(IsGround, "Can only get generalization list for ground hlas"); 
            if (Name.Length == 5)
            {
                yield return this;
            }
            if (Name.Length >= 3)
            { 
                string twoDigit = Name.Substring(0, 3); 
                if (HlaMsr1Factory.MoreThan100.ContainsKey(twoDigit))
                { 
                    twoDigit = ClassName + HlaMsr1Factory.MoreThan100[twoDigit];
                }
                yield return (HlaMsr1)hlaMsr1Factory.GetGroundOrAbstractInstance(twoDigit, ref warningSetToIgnore);
                Debug.Assert(warningSetToIgnore.Count == 0); // real assert
            }
 
            yield return (HlaMsr1)hlaMsr1Factory.GetGroundOrAbstractInstance(ClassName, ref warningSetToIgnore); 
            Debug.Assert(warningSetToIgnore.Count == 0); // real assert
 
        }

        internal IEnumerable<HlaMsr1> TermList(HlaMsr1Factory hlaMsr1Factory)
        {
            //!!!this could be made faster by skipping the factory calls when it is the same as the current HLA and for when there are no "/"'s
            HashSet<string> warningSetToIgnore = new HashSet<string>(); 
            HlaMsr1 newHla = (HlaMsr1)hlaMsr1Factory.GetGroundOrAbstractInstance(ToString(/*withParens*/true), ref warningSetToIgnore); 
            foreach (var nameAndOri in newHla.NameAndOriList)
            { 
                yield return (HlaMsr1)hlaMsr1Factory.GetGroundOrAbstractInstance(nameAndOri.Value, ref warningSetToIgnore);
            }
        }

        public int CompareTo(HlaMsr1 other)
        { 
 
            //I'm an object, so if they other guy is null, we are not equal
            if (null == other) 
            {
                return 1;
            }

            return Name.CompareTo(other.Name);
        } 
 
        public string ToString(bool withParens)
        { 
            if (withParens)
            {
                return NameAndOriList.Select(kv=>kv.Value).StringJoin("/");
            }
            else
            { 
                return Name; 
            }
        } 

        public override string ToString()
        {
            throw new Exception();
        }
 
 
    }
 


}

// Microsoft Research, eScience Research Group, Microsoft Reciprocal License (Ms-RL).
// Copyright (c) Microsoft Corporation. All rights reserved.
