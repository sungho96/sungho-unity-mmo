using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginScene : BaseScene
{
    protected override void init()
    {
        base.init();
        SceneType = Define.Scene.Login;
    }
    private void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Managers.Scene.LoadScene(Define.Scene.Game);
        }

    }
    public override void Clear()
    {
       Debug.Log("Login");
    }


}
