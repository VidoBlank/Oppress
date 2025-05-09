using System;
using System.Collections.Generic;

public class AutoSaveData : IDisposable
{
    public string key { get; private set; }
    public AutoSaveData(string key)
    {
        this.key = key;
    }

    Dictionary<string, string> instanceKeys = new Dictionary<string, string>();

    string GetInstanceKey(string key)
    {
        if (!instanceKeys.TryGetValue(key, out var fullKey))
        {
            fullKey = $"{this.key}_{key}";
            instanceKeys.Add(key, fullKey);
        }
        return fullKey;
    }


    Dictionary<string, IAutoSaveValue> _values = new Dictionary<string, IAutoSaveValue>();

    public bool Contains(string key)
    {
        return _values.ContainsKey(key);
    }

    public void Delete(string key)
    {
        if (_values.ContainsKey(key))
        {
            _values.Remove(key);
        }
    }


    public int GetInt(string key)
    {
        if (_values.TryGetValue(key, out var v))
        {
            if (v is AutoSaveInt vT)
            {
                return vT.value;
            }
            else
            {
                throw new Exception($"Property {key} already exists with type: {v.GetValueType()}");
            }
        }
        else
        {
            var vInt = new AutoSaveInt(GetInstanceKey(key));
            _values.Add(key, vInt);
            return vInt.value;
        }
    }

    public bool GetBool(string key)
    {
        if (_values.TryGetValue(key, out var v))
        {
            if (v is AutoSaveBool vT)
            {
                return vT.value;
            }
            else
            {
                throw new Exception($"Property {key} already exists with type: {v.GetValueType()}");
            }
        }
        else
        {
            var vT = new AutoSaveBool(GetInstanceKey(key));
            _values.Add(key, vT);
            return vT.value;
        }
    }

    public float GetFloat(string key)
    {
        if (_values.TryGetValue(key, out var v))
        {
            if (v is AutoSaveFloat vT)
            {
                return vT.value;
            }
            else
            {
                throw new Exception($"Property {key} already exists with type: {v.GetValueType()}");
            }
        }
        else
        {
            var vT = new AutoSaveFloat(GetInstanceKey(key));
            _values.Add(key, vT);
            return vT.value;
        }
    }


    public string GetString(string key)
    {
        if (_values.TryGetValue(key, out var v))
        {
            if (v is AutoSaveString vT)
            {
                return vT.value;
            }
            else
            {
                throw new Exception($"Property {key} already exists with type: {v.GetValueType()}");
            }
        }
        else
        {
            var vT = new AutoSaveString(GetInstanceKey(key));
            _values.Add(key, vT);
            return vT.value;
        }
    }






    public void SetInt(string key, int value)
    {
        if (_values.TryGetValue(key, out var v))
        {
            if (v is AutoSaveInt vT)
            {
                vT.value = value;
            }
            else
            {
                throw new Exception($"Property {key} already exists with type: {v.GetValueType()}");
            }
        }
        else
        {
            var vT = new AutoSaveInt(GetInstanceKey(key));
            _values.Add(key, vT);
            vT.value = value;
        }
    }

    public void SetBool(string key, bool value)
    {
        if (_values.TryGetValue(key, out var v))
        {
            if (v is AutoSaveBool vT)
            {
                vT.value = value;
            }
            else
            {
                throw new Exception($"Property {key} already exists with type: {v.GetValueType()}");
            }
        }
        else
        {
            var vT = new AutoSaveBool(GetInstanceKey(key));
            _values.Add(key, vT);
            vT.value = value;
        }
    }

    public void SetFloat(string key, float value)
    {
        if (_values.TryGetValue(key, out var v))
        {
            if (v is AutoSaveFloat vT)
            {
                vT.value = value;
            }
            else
            {
                throw new Exception($"Property {key} already exists with type: {v.GetValueType()}");
            }
        }
        else
        {
            var vT = new AutoSaveFloat(GetInstanceKey(key));
            _values.Add(key, vT);
            vT.value = value;
        }
    }


    public void SetString(string key,string value)
    {
        if (_values.TryGetValue(key, out var v))
        {
            if (v is AutoSaveString vT)
            {
                vT.value = value;
            }
            else
            {
                throw new Exception($"Property {key} already exists with type: {v.GetValueType()}");
            }
        }
        else
        {
            var vT = new AutoSaveString(GetInstanceKey(key));
            _values.Add(key, vT);
            vT.value = value;
        }
    }

    public void DeleteAll()
    {
        if (_values.Count > 0)
        {
            foreach (var kvp in _values)
            {
                kvp.Value.Delete();
            }
            _values.Clear();
        }
    }

    public void Dispose()
    {
        instanceKeys.Clear();
        instanceKeys = null;
        _values.Clear();
        _values = null;
    }

}