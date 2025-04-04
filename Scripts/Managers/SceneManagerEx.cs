using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//이름충돌 ScenManager가이미 있음
public class SceneManagerEx 
{
    public BaseScene CurrentScene {get{return  GameObject.FindObjectOfType<BaseScene>();}}
    public void LoadScene(Define.Scene type)
    {
        Managers.Clear();
        SceneManager.LoadScene(GetSceneName(type));      
    }
    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }
    public void Clear()
    {
        CurrentScene.Clear();
    }
}
