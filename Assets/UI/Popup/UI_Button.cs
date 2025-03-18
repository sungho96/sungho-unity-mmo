using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Button : UI_Popup
{   
    enum Buttons
    {
        PointButton
    }
    enum GameObjects
    {
        TestObjects,
    }

    enum Texts
    {
        PointText,
        ScoreText,  
    }
    enum Images
    {
        ItemIcon,
        
    }
    private void Start()
    {   
        Init();
    }
    public override void Init()
    {   
        base.Init();
        //자동바인딩
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));

        

        GetButton((int)Buttons.PointButton).gameObject.BindEvent(OnButtonClicked);
        

        //이벤트 추가하는법(pos위치 값을 바꿈)
        GameObject go = GetImage((int)Images.ItemIcon).gameObject;
        BindEvent(go, (PointerEventData data) => {go.gameObject.transform.position = data.position;}, Define.UIEvent.Drag);
        
    }

    int _score = 0;

    public void OnButtonClicked(PointerEventData data)
    {
        _score++;

        GetText((int)Texts.ScoreText).text=$"점수 : {_score}";

    }
}
