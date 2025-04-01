using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{

    //1. corutine 함수의 상태를 저장/복원 가능!
        // 엄청 오래 걸리는 작업을 잠시 끊거나,원하는 타이밍에 함수를 잠시 Stop/복원하는 경우
        // return -> 우리가원하는타입으로 가능(class도 가능)


    protected override void init()
    {
        base.init();
        
        SceneType = Define.Scene.Game;

		Managers.UI.ShowSceneUI<UI_Inven>();
        Dictionary<int, Stat> dict = Managers.Data.StatDict;
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }
}

