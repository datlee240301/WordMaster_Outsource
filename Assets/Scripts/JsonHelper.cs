using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }

    public static List<T> FromJson<T>(string json)
    {
        string newJson = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.items;
    }
}