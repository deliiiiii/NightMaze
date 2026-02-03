using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using General;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    public ObjectPool(T tPrefab, Transform transform, int initSize = 36, float yieldCount = 1)
    {
        yieldControl = new YieldControl(yieldCount);
        this.tPrefab = tPrefab;
        this.size = initSize;
        objParent = transform;
        _ = MyCreateNewAsync(size);
    }

    readonly YieldControl yieldControl;
    readonly T tPrefab;
    readonly Transform objParent;
    int size;
    readonly Stack<T> availableObject = new();
    async Task MyCreateNewAsync(int newCount)
    {
        try
        {
            size += newCount;
            for (int i = 0; i < newCount; i++)
            {
                if (!Application.isPlaying)
                    return;
                var g = GameObject.Instantiate(tPrefab, tPrefab.transform.position, tPrefab.transform.rotation, objParent);
                // g.transform.SetParent(objParent);
                g.gameObject.SetActive(false);
                availableObject.Push(g);
                await yieldControl.YieldFrames();
            }
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
            throw;
        }
    }

    public async Task<T> MyInstantiate()
    {
        try
        {
            if (availableObject.Count == 0)
            {
                await MyCreateNewAsync((int)(size * 0.5f));
            }
            var g = availableObject.Pop();
            g.transform.SetParent(objParent, worldPositionStays: false);
            g.gameObject.SetActive(true);
            return g;
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
            throw;
        }
    }
    public async Task<T> MyInstantiate(Vector3 fPos)
    {
        try
        {
            if (availableObject.Count == 0)
            {
                await MyCreateNewAsync((int)(size * 0.5f));
            }
            var g = availableObject.Pop();
            g.transform.position = fPos;
            g.transform.SetParent(objParent, worldPositionStays: false);
            g.gameObject.SetActive(true);
            return g;
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
            throw;
        }
    }
    public async Task<T> MyInstantiate(Vector3 fPos, Quaternion fRot)
    {
        try
        {
            if (availableObject.Count == 0)
            {
                await MyCreateNewAsync((int)(size * 0.5f));
            }
            var g = availableObject.Pop();
            g.transform.position = fPos;
            g.transform.rotation = fRot;
            g.transform.SetParent(objParent, worldPositionStays: false);
            g.gameObject.SetActive(true);
            return g;
        }
        catch (Exception e)
        {
            MyDebug.LogError(e);
            throw;
        }
    }
        
    public void MyDestroy(T obj)
    {
        if (obj == null)
        {
            MyDebug.LogError("MyDestroy null " + tPrefab.name + " !");
            return;
        }
        obj.gameObject.SetActive(false);
        availableObject.Push(obj);
    }
}