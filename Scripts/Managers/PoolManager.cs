using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager  
{   
    class Pool
    {
        public GameObject Original {get; private set;}
        public Transform Root {get;set;}

        Stack<Poolable> _poolStack = new Stack<Poolable>();
        public void Init(GameObject original, int count = 5)
        {
            Original = original;
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            for (int i =0; i <count; i++)
            {
                Push(Create());
            }
        }
        Poolable Create()
        {
            GameObject go = Object.Instantiate<GameObject>(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }
        public void Push(Poolable poolable)
        {
            if(poolable == null)
            return;

            poolable.transform.parent = Root;
            poolable.gameObject.SetActive(false);
            poolable.IsUsing = false;

            _poolStack.Push(poolable);
        }
        public Poolable Pop(Transform parent)
        {
            Poolable poolable;

            if(_poolStack.Count > 0)
                poolable = _poolStack.Pop();
            else
                poolable = Create();
            
            poolable.gameObject.SetActive(true);
            //Don't DestroyOnLoad 해제 용도 
            if(parent==null)
                poolable.transform.parent = Managers.Scene.CurrentScene.transform;
            poolable.transform.parent = parent;
            poolable.IsUsing = true;

            return poolable;
        }
    }
    
    Dictionary<string, Pool> _pool = new Dictionary<string, Pool>();
    //game object도 상관없음
    Transform _root;
    public void init()
    {
        if(_root == null)
        {
            _root = new GameObject{ name = "@Pool_Root"}.transform;
            Object.DontDestroyOnLoad(_root);
        }
    }
    //count는 임의로 5개만 한거임.
    public void CreatePool(GameObject original, int count =5)
    {
        Pool pool = new Pool();
        pool.Init(original,count);
        pool.Root.parent = _root.transform;

        _pool.Add(original.name, pool);
    }
    public void push(Poolable poolable)
    {   
        string name = poolable.gameObject.name;
        if(_pool.ContainsKey(name) == false)
        {
            GameObject.Destroy(poolable.gameObject);
            return;
        }
        _pool[name].Push(poolable);
    }
    public Poolable Pop(GameObject original, Transform parent = null)
    {   
        if(_pool.ContainsKey(original.name)==false)
            CreatePool(original);
        return _pool[original.name].Pop(parent);
    }

    public GameObject GetOriginal(string name)
    {   if(_pool.ContainsKey(name)==false)
            return null;
        return _pool[name].Original;
    }

    public void Cleaer()
    {
       foreach (Transform child in _root)
            GameObject.Destroy(child.gameObject); 
    }
}

