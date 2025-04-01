using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//[Serializable] 메모리에서 들고 있는 것을(Json) 파일로 변환할 수 있다
#region Stat
[Serializable]
public class Stat
{
    public int level;
    public int hp;
    public int attack;
}

[Serializable]
public class StatData : ILoader<int, Stat>
{
    public List<Stat> stats = new List<Stat>();

    public Dictionary<int, Stat> MakeDict()
    {
        Dictionary<int, Stat> dict =new Dictionary<int, Stat>();
            foreach( Stat stat in stats)
                dict.Add(stat.level, stat);
            return dict;
    }
}
#endregion