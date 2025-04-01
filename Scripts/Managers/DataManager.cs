using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<key, value>
{
    Dictionary<key, value> MakeDict();
}
public class DataManager 
{
    public Dictionary<int, Stat> StatDict {get; private set;} =new Dictionary<int, Stat>();

    //xml 보다 json을 더 많이 사용하고 빠르다.
    //xml은 계층분석하는데 더빠르다.(데이터 층)
    public void init()
    {
        StatDict = LoadJson<StatData, int, Stat>("StatData").MakeDict();
    }
    TLoader LoadJson<TLoader, TKey, TValue>(string path) where TLoader : ILoader<TKey, TValue>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<TLoader>(textAsset.text);
    }
}
