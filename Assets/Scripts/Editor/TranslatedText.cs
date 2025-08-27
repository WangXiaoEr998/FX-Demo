using Codice.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
public class TranslatedText
{
    private static TranslatedText instance;
    public static TranslatedText Instance
    {
        get
        {
            if (instance == null)
                instance = new TranslatedText();
            return instance;
        }
    }
    public string Translate(string str,string id,string key)
    {
        string q = str;
        // 源语言
        string from = "zh";
        // 目标语言
        string to = "en";
        // APP ID
        string appId = id;
        System.Random rd = new System.Random();
        string salt = rd.Next(100000).ToString();
        // 密钥
        string secretKey = key;
        string sign = EncryptString(appId + q + salt + secretKey);
        string url = "https://api.fanyi.baidu.com/api/trans/vip/translate?";
        url += "q=" + HttpUtility.UrlEncode(q, Encoding.UTF8); // 修复：指定UTF8编码，防止中文乱码
        url += "&from=" + from;
        url += "&to=" + to;
        url += "&appid=" + appId;
        url += "&salt=" + salt;
        url += "&sign=" + sign;
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream myResponseStream = response.GetResponseStream())
            using (StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8)) // 修复：强制UTF8编码
            {
                string retString = myStreamReader.ReadToEnd();
                Debug.Log(retString);
                try
                {
                    BaiduTransResponse response1 = JsonUtility.FromJson<BaiduTransResponse>(retString);
                    if (response1 != null && response1.trans_result != null && response1.trans_result.Length > 0)
                    {
                        string translatedText = response1.trans_result[0].dst;
                        Debug.Log("翻译结果: " + translatedText);
                        translatedText = CorrectText(translatedText); // 修复：调用文本修正方法
                        return translatedText;
                    }
                    else
                    {
                        Debug.LogError("翻译结果解析失败");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("JSON解析错误: " + e.Message + "\n返回内容: " + retString); // 修复：输出原始内容便于调试
                }
            }
        }
        catch (WebException ex)
        {
            Debug.LogError("网络请求失败: " + ex.Message + "请联网再试");
            if (ex.Response != null)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Debug.LogError("错误响应: " + reader.ReadToEnd());
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("未知错误: " + ex.Message);
        }
        return null;
    }
    private string CorrectText(string translatedText)
    {
        // Split用于将指定字符符分割成字符串数组  System.StringSplitOptions.RemoveEmptyEntries 去除空字符串
        string[] words = translatedText.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = words[i].Substring(0).ToLower();
            }
        }
        string pascalCase = string.Join("", words);
        // ToLower 将字符小写  Substring 截取字符串从第二个字符开始的部分
        string newTranslatedText = char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
        return newTranslatedText;
    }
    public static string EncryptString(string str)
    {
        MD5 md5 = MD5.Create();
        byte[] byteOld = Encoding.UTF8.GetBytes(str);
        byte[] byteNew = md5.ComputeHash(byteOld);
        StringBuilder sb = new StringBuilder();
        foreach (byte b in byteNew)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
    [System.Serializable]
    public class BaiduTransResponse
    {
        public string from;
        public string to;
        public TransResult[] trans_result; // 注意这里是数组类型
    }

    [System.Serializable]
    public class TransResult
    {
        public string src;
        public string dst;
    }
}