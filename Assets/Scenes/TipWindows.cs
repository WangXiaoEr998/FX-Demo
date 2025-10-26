using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static YooAssetWrap;


public class TipWindows : MonoBehaviour
{


    private static TipWindows _instance;

    public static TipWindows Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TipWindows>();
            }
            return _instance;
        }
    }

    public GameObject parent;
    public Text tipsContent;
    public Text tipsProgress;
    public Slider slider;
    public Button btnConfig;
    public Button btnCancel;


    public delegate void TipsWindowCallback(bool state);
    private TipsWindowCallback clickCallback;

    private void Start()
    {
        slider.interactable = false;
        btnConfig.onClick.AddListener(Event_Config);
        btnCancel.onClick.AddListener(Event_Cancel);
    }

    public void Display1View()
    {
        tipsContent.transform.gameObject.SetActive(true);
        tipsProgress.transform.gameObject.SetActive(false);
        slider.transform.gameObject.SetActive(false);
        btnConfig.transform.gameObject.SetActive(true);
        btnCancel.transform.gameObject.SetActive(true);
    }

    public void Display2View()
    {
        tipsContent.transform.gameObject.SetActive(true);
        tipsProgress.transform.gameObject.SetActive(true);
        slider.transform.gameObject.SetActive(true);
        btnConfig.transform.gameObject.SetActive(false);
        btnCancel.transform.gameObject.SetActive(false);
    }

    public void Display3View()
    {
        tipsContent.transform.gameObject.SetActive(true);
        tipsProgress.transform.gameObject.SetActive(false);
        slider.transform.gameObject.SetActive(false);
        btnConfig.transform.gameObject.SetActive(false);
        btnCancel.transform.gameObject.SetActive(false);
    }

    private void Event_Config()
    {
        if (clickCallback != null)
        {
            clickCallback(true);
        }
    }


    private void Event_Cancel()
    {
        if (clickCallback != null)
        {
            clickCallback(false);
        }
        parent.SetActive(false);
    }


    public void SetTipsContent(string msg)
    {
        tipsContent.text = msg;
    }

    public void SetTipsProgress(string msg)
    {
        tipsProgress.text = msg;
    }

    public void SetSliderValue(int value)
    {
        slider.value = value;
    }

    public void SetClickCallback(TipsWindowCallback callback)
    {
        clickCallback = callback;
    }

    public void SetTipWindowActive(bool state)
    {
        parent.SetActive(state);
    }



}
