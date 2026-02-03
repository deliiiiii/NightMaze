using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int Pro => field;
    async void Start()
    {
        try
        {
            await UniTask.CompletedTask;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
