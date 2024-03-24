using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
//using System;
public class APITest : MonoBehaviour
{
    public string test;
    private string URL = "https://api.musixmatch.com/ws/1.1/track.lyrics.get?track_id=84203066&apikey=c768e65b1455ee10354d6bb938c1b820";
    //private string URL = "https://api.genius.com/search?q=Pitbull&access_token=";
    
    
    public TMP_Text testText;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetData());
    }
    
    IEnumerator GetData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL))
        {
            yield return request.SendWebRequest();

            if (request.result==UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                string json = request.downloadHandler.text;
                SimpleJSON.JSONNode stats = SimpleJSON.JSON.Parse(json);
                
                //https://api.musixmatch.com/ws/1.1/track.search?q_artist=justin bieber&page_size=3&page=1&s_track_rating=desc
                //Debug.Log(stats["message"]["body"]["track_list"][0]["track"]["track_id"]);
                
                //https://api.musixmatch.com/ws/1.1/track.lyrics.get?track_id=84203066&apikey=c768e65b1455ee10354d6bb938c1b820;
                Debug.Log(stats["message"]["body"]["lyrics"]["lyrics_body"]);
                testText.text = stats["message"]["body"]["lyrics"]["lyrics_body"];
                
                string[] verses = (""+stats["message"]["body"]["lyrics"]["lyrics_body"]).Split(new string[] { "\n" }, System.StringSplitOptions.None);
                
                for (int i = 0; i < verses.Length; i++)
                {
                    Debug.Log("Verse " + (i + 1) + ": " + verses[i]);
                }
            }
        }
    }
}