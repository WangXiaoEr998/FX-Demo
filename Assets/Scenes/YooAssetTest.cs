using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class YooAssetTest : MonoBehaviour
{
    public string packageName = "DefaultPackage";


    // Start is called before the first frame update
    void Start()
    {
        YooAssetWrap.Instance.CheckPackage(packageName, Callback_CheckPackage);

    }


    private void Callback_CheckPackage(YooAssetWrap.YooAssetWrapCallbackMsg _callbackMsg)
    {
        if (_callbackMsg.retCode == 0)
        {
            // ab 包下载更新成功

            YooAssetWrap.Instance.GetSpriteAsset(packageName, "Assets/jk", "jk", Callback_Image);

            //YooAssetWrap.Instance.GetAudioAsset(packageName, "Assets/Bark", Callback_Audio);

            //YooAssetWrap.Instance.GetPrefabAsset(packageName, "Assets/Test", Callback_Prefab);
        }
        if (_callbackMsg.retCode < 0)
        {
            // ab 包下载更新失败

            print(_callbackMsg.retMsg);
        }
    }


    private void Callback_Image(YooAssetWrap.YooAssetWrapCallbackMsg _callbackMsg)
    {
        if (_callbackMsg.retCode == 0)
        {
            Debug.Log("success");

            Sprite go = _callbackMsg.retAsset as Sprite;
            GameObject.Find("Canvas/Image").GetComponent<Image>().sprite = go;
        }
        else
        {
            Debug.LogWarning("failed: " + _callbackMsg.retMsg);
        }
    }

    private void Callback_Audio(YooAssetWrap.YooAssetWrapCallbackMsg _callbackMsg)
    {
        if (_callbackMsg.retCode == 0)
        {
            Debug.Log("success");

            AudioClip go = _callbackMsg.retAsset as AudioClip;
            GameObject.Find("Audio Source").GetComponent<AudioSource>().clip = go;
            GameObject.Find("Audio Source").GetComponent<AudioSource>().Play();
        }
        else
        {
            Debug.LogWarning("failed: " + _callbackMsg.retMsg);
        }
    }

    private void Callback_Prefab(YooAssetWrap.YooAssetWrapCallbackMsg _callbackMsg)
    {
        if (_callbackMsg.retCode == 0)
        {
            Debug.Log("success");

            GameObject go = _callbackMsg.retAsset as GameObject;
            go.transform.SetParent(GameObject.Find("Canvas").transform);
            go.transform.localPosition = new Vector3(0, 0, 0);

        }
        else
        {
            Debug.LogWarning("failed: " + _callbackMsg.retMsg);
        }
    }



}