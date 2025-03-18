using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class UIManager 
//sort order를 관리하기 위해서 
//ui_button sort를 가지고 있기때문에 그것의 상위 컴포넌트인 UIPopup으로 스택을 만들자
{
    int _order = 10;
    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    UI_Scene _sceneUI = null;
    
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
            {
                root = new GameObject { name = "@UI_Root" };
            }
            return root;
        }
    }
    
    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting =true;

        if(sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }

    }
    public T MakeSubItem<T>(Transform parent = null,string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        GameObject go = Managers.Resource.Instantiate($"UI/SubItem/{name}");
        
        if(parent != null)
            go.transform.SetParent(parent);

        return Util.GetOrAddComponent<T>(go);
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {   
        if(string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");

        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;
        go.transform.SetParent(Root.transform);

        return sceneUI;
    }

    //name은 프리팹의 이름 T타입은 UIButton.cs랑 연관있다
    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {   
        if(string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");

        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);
        go.transform.SetParent(Root.transform);

        return popup;
    }
    public void ClosePopupUI(UI_Popup popup)
    {
      if (_popupStack.Count == 0)
            return;

        if(_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed");
            return ;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;
        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup =null;
        _order --;
    }
    public void CloseAllPopUI()
    {
        while(_popupStack.Count > 0)
        CloseAllPopUI();
    }

    public void Clear()
    {
        CloseAllPopUI();
        _sceneUI =null;
    }
}
