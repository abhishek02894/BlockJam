using System;
using UnityEngine;

namespace Tag.Block
{
    public class PersistantVariable<T>
    {
        public readonly string _key = string.Empty;

        private static readonly Type IntType = typeof(int);
        private static readonly Type FloatType = typeof(float);
        private static readonly Type StringType = typeof(string);

        private readonly bool isInt;
        private readonly bool isFloat;
        private readonly bool isString;

        private T defaultValue;

        public PersistantVariable(string key)
        {
            _key = key;
            Type t = typeof(T);
            isInt = t == IntType;
            isFloat = t == FloatType;
            isString = t == StringType;
        }

        public PersistantVariable(string key, T defaultValue) : this(key)
        {
            this.defaultValue = defaultValue;
        }

        public T Value
        {
            get
            {
                if (isInt)
                    return (T)(object)PlayerPrefs.GetInt(_key, (int)(object)defaultValue);
                if (isFloat)
                    return (T)(object)PlayerPrefs.GetFloat(_key, (float)(object)defaultValue);
                if (isString)
                    return (T)(object)PlayerPrefs.GetString(_key, (string)(object)this.defaultValue);
                return !HasKey(_key) && defaultValue != null ? defaultValue : SerializeUtility.DeserializeObject<T>(PlayerPrefs.GetString(_key));
            }
            set
            {
                if (isInt)
                {
                    PlayerPrefs.SetInt(_key, (int)(object)value);
                }
                else if (isFloat)
                {
                    PlayerPrefs.SetFloat(_key, (float)(object)value);
                }
                else if (isString)
                {
                    PlayerPrefs.SetString(_key, (string)(object)value);
                }
                else
                {
                    PlayerPrefs.SetString(_key, SerializeUtility.SerializeObject(value));
                }
            }
        }

        public string RawValue
        {
            get
            {
                if (isInt)
                    return PlayerPrefs.GetInt(_key, (int)(object)defaultValue).ToString();
                if (isFloat)
                    return PlayerPrefs.GetFloat(_key, (float)(object)defaultValue).ToString();
                if (isString)
                    return PlayerPrefs.GetString(_key, (string)(object)defaultValue);

                return !HasKey(_key) && defaultValue != null ? SerializeUtility.SerializeObject(defaultValue) : PlayerPrefs.GetString(_key);
            }
        }
        public static bool HasKey(string key) => PlayerPrefs.HasKey(key);
    }

}