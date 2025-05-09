using System;
using System.Collections.Generic;
using UnityEngine;

public interface IAutoSaveValue
{
    string key { get; }

    Type GetValueType();

    void Delete();
}

public abstract class AutoSaveValue<T> : IAutoSaveValue
{
    public Type GetValueType()
    {
        return typeof(T);
    }

    T _defaultValue;

    public string key { get; private set; }
    public AutoSaveValue(string key, T defaultValue = default)
    {
        this.key = key;
    }

    bool hasRead;

    T _value;
    public T value
    {
        get
        {
            if (!hasRead)
            {
                hasRead = true;
                _value = LoadValue(_defaultValue);
            }
            return _value;
        }
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                SaveValue(value);
            }
        }
    }

    abstract protected T LoadValue(T defaultValue);

    abstract protected void SaveValue(T value);

    public void Delete()
    {
        PlayerPrefs.DeleteKey(key);
    }
}


public class AutoSaveInt : AutoSaveValue<int>
{
    public AutoSaveInt(string key) : base(key)
    {
    }

    protected override int LoadValue(int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    protected override void SaveValue(int value)
    {
        PlayerPrefs.SetInt(key, value);
    }
}

public class AutoSaveBool : AutoSaveValue<bool>
{
    public AutoSaveBool(string key) : base(key)
    {
    }
    protected override bool LoadValue(bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue?1:0) == 1;
    }
    protected override void SaveValue(bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}

public class AutoSaveFloat : AutoSaveValue<float>
{
    public AutoSaveFloat(string key) : base(key)
    {
    }
    protected override float LoadValue(float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }
    protected override void SaveValue(float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }
}

public class AutoSaveString : AutoSaveValue<string>
{
    public AutoSaveString(string key) : base(key)
    {
    }
    protected override string LoadValue(string defaultValue)
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }
    protected override void SaveValue(string value)
    {
        PlayerPrefs.SetString(key, value);
    }
}