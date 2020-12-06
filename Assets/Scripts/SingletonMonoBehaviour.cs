using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Type t = typeof(T);

                _instance = (T) FindObjectOfType(t);
                if (_instance == null)
                {
                    Debug.LogError(t + "をアタッチしているGameObjectはありません");
                }
            }

            return _instance;
        }
    }

    protected void Awake()
    {
        CheckInstance();
    }

    bool CheckInstance()
    {
        if (_instance == null)
        {
            _instance = this as T;
            return true;
        }

        if (_instance == this)
        {
            return true;
        }
        
        Destroy(this);
        return false;
    }
}
