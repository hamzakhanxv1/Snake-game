using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader{
    
    public enum Scene {
        Gamescene,
        Loading,
    }

    private static Action loaderCallBackAction;
    public static void Load(Scene scene){

        loaderCallBackAction = () => {
            SceneManager.LoadScene(scene.ToString());
        };
        SceneManager.LoadScene(Scene.Loading.ToString());

       
    }

    public static void LoaderCallBack(){
        if(loaderCallBackAction != null){
            loaderCallBackAction();
            loaderCallBackAction = null;
        }
    }

}
