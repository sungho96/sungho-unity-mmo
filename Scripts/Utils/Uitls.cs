using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Util
    
{   
    //찾던거 추가해서 던져 주는 함수
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {       T component = go.GetComponent<T>();
            if (component == null)
            component = go.AddComponent<T>();
            return component;
    }
    //게임오브젝트를 나오는 찾아는 주는 함수 
    public static GameObject FindChild(GameObject go, string name = null,bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go,name,recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }
    //기능성함수 전부 넣어주기(button,text등등)
    public static T FindChild<T>(GameObject go, string name = null,bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive ==false)
        {   for (int i = 0; i <go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if(string.IsNullOrEmpty(name) || transform.name ==name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {   //t type의 컴포넌트를 순회한다.
            foreach(T component in go.GetComponentsInChildren<T>())
            {
                if(string.IsNullOrEmpty(name) || component.name ==name)
                    return component;
            }
        }

        return null;
    }
}
