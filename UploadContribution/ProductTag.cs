using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace UploadContribution
{
    public class ProductTag
    {
        protected static string Key64Bit = "64bit";
        protected static string KeyTrial = "Trial";
        protected static string KeyReadOnly = "Read-Only";
        protected static string _64bit = "_64";// optional version info i.e.  64bit
        protected static string _trial = "_Trial";// optional verison info i.e.  Trial
        protected static string _readOnly = "ReadOnly";
        protected string _key;  // main part of the product name i.e  BenchmarkFactory
        protected string _resultValue;     // look up the string and report the values

        protected string _key64;
        protected string _keyTrial;
        protected string _keyReadOnly;

        public ProductTag()
        {
            _key = "";
            _resultValue = "";
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="tagMain"> string to be use as main part of key </param>
        /// <param name="resultValue"> resulting string from the look up</param>
        public ProductTag(string tagKey, string resultValue)
        {
            _key = tagKey;
            _resultValue = resultValue;
            _key64 = Key64Bit;
            _keyTrial = KeyTrial;
            _keyReadOnly = KeyReadOnly;
        }

        public ProductTag(string tagKey, string resultValue, string tagKey64): this(tagKey, resultValue)
        {

            _key64 = tagKey64;
     
        }

        
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
        /// <summary>
        /// Pass in a name like BenchmarkFactory_7_1_0_496_32bit.msi
        //  Expect to retrieve the root plus all the options
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public virtual string GetResultValue(string name)
        {
            string result = "";
            if (name.StartsWith(_key))
            {
                result = _resultValue;
                if (name.Contains(_keyTrial))
                    result += _trial;
                if (name.Contains(_key64))
                    result += _64bit;
            }
            return result;
        }

        /// <summary>
        /// Extract the main part of the name i.e BenchmarkFactory_7_1_0_496_32bit.msi
        /// would return BenchmarkFactory
        /// Find the first number and retrieve the substring prior to it.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetMainTag(string name)
        {

            string resultString = name;
            Match match = Regex.Match(name, @".\d+", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                // find the substring
                string firstNumber = match.Value;
                resultString = name.Substring(0, name.IndexOf(firstNumber));
            }
            return resultString;
            
        }
    }

    public class ProductTagToad : ProductTag
    {
        public ProductTagToad(string tagKey, string resultValue): base(tagKey, resultValue)
        {
        }

        public ProductTagToad(string tagKey, string resultValue, string tagKey64)
            : base(tagKey, resultValue, tagKey64)
        {
        }
        /// <summary>
        /// Pass in a name like BenchmarkFactory_7_1_0_496_32bit.msi
        //  Expect to retrieve the root plus all the options
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public override string GetResultValue(string name)
        {
            string result = "";
            if (name.StartsWith(_key))
            {
                result = _resultValue;      // Toad
                if (name.Contains(_key64))
                {
                    if (name.Contains(_keyTrial))
                    {
                        result = result + _trial + _64bit;
                    }
                    else if (name.Contains(_keyReadOnly))
                    {
                        result = result + _readOnly + _64bit;
                    }
                    else
                    {
                        result = result + "_Standalone" + _64bit;
                    }
                }
                else
                {
                    if (name.Contains(_keyTrial))
                    {
                        result = result + _trial;
                    }
                    else if (name.Contains(_keyReadOnly))
                    {
                        result = result + _readOnly;
                    }
                    else
                    {
                        result = result + "_Standalone";
                    }
                }
            }
            return result;
        }
    }
}
