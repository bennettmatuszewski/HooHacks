using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public int questionNumber;
    public string test;
    public int score;
    
    [Space][Space]
    
    [Header("References")]
    public TMP_InputField inputField;
    private string artist;
    private bool canSubmit = true;
    public TMP_Text invalidArtistText;
    private string URL = "https://api.musixmatch.com/ws/1.1/track.lyrics.get?track_id=84203066&apikey=c768e65b1455ee10354d6bb938c1b820";
    public Transform camera;
    public TMP_Text questionTransitionText;
    public RectTransform questionTransitionTextRT;
    public TMP_Text scoreText;
    public TMP_Text scoreTextEffect;
    public TMP_Text correctWrongText;
    public Color redColor;
    public Color greenColor;
    public Image fadeOver;
    public Image backgroundImage;
    public TMP_Text yourScoreText;
    public TMP_Text[] lyrics;
    public int[] lyricsMovePos;
    public int[] lyricsOgPos;
    public TMP_Text[] answerButtonTexts;
    public Color[] backgroundColors;
    public Question[] questions;
    private SimpleJSON.JSONNode artistSongsJson;
    private int currentLyricGuess;
    private bool cant;
    private bool cantMoveLyric;
    private bool cantGuess;
    private bool cantPressEndButton;
    public int difficulty; 
    private void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        fadeOver.DOFade(0, 1f);
        ChangeBackgroundColor();

    }

    public void SubmitArtist()
    {
        if (!canSubmit)
        {
            return;
        }

        StartCoroutine(CheckArtist());

    }

    IEnumerator CheckArtist()
    {
        bool failed = false;
        using (UnityWebRequest request = UnityWebRequest.Get("https://api.musixmatch.com/ws/1.1/track.search?q_artist=" + inputField.text + "&page_size=" + difficulty + "&page=1&s_track_rating=desc&apikey="+test))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                failed = true;
            }
            else
            {
                string json = request.downloadHandler.text;
                SimpleJSON.JSONNode stats = SimpleJSON.JSON.Parse(json);
                artistSongsJson = stats;
                
                if (stats["message"]["body"]["track_list"].Count<15)
                {
                    failed = true;
                }
            }

            if (failed)
            {
                canSubmit = true;
                invalidArtistText.DOFade(1, 1).OnComplete(() => invalidArtistText.DOFade(0, 1));
            }
            else
            {
                StartGame();
            }
        }
    }

    void StartGame()
    {
        artist = inputField.text;
        StartCoroutine(GenerateLyricsAndQuestions());
        StartCoroutine(NextQuestion());
    }
    IEnumerator NextQuestion()
    {
        ChangeBackgroundColor();
        cant = false;
        questionNumber++;
        currentLyricGuess = 0;
        camera.DOMoveY(-10, 2).SetEase(Ease.InOutBack);
        yield return new WaitForSeconds(1.7f);
        cantGuess = false;
        cantPressEndButton = false;
        scoreTextEffect.GetComponent<RectTransform>().DOAnchorPosY(403, 0);
        correctWrongText.GetComponent<RectTransform>().DOScale(Vector3.zero, 0f);
        correctWrongText.GetComponent<RectTransform>().DOAnchorPosX(0, 0f);
        questionTransitionText.text = "Question " + questionNumber;
        for (int i = 0; i < 4; i++)
        {
            answerButtonTexts[i].transform.parent.GetComponent<Image>().color = Color.white;
        }
        questionTransitionTextRT.DOAnchorPosX(1500, 2).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(1.7f);
        camera.DOMoveY(-20, 1).SetEase(Ease.InOutQuad);
        if (questionNumber==0)
        {
            yield return new WaitForSeconds(2);
        }
        else
        {
            yield return new WaitForSeconds(1);
        }
        for (int j = 0; j < 4; j++)
        {
            answerButtonTexts[j].text = questions[questionNumber-1].answers[j];
        }
        MoveLyric();
        questionTransitionTextRT.DOAnchorPosX(-1500, 0);
        for (int i = 0; i < 4; i++)
        {
            lyrics[i].text = questions[questionNumber - 1].lyrics[i];
        }
    }
    
    IEnumerator GenerateLyricsAndQuestions()
    {
        //CHANGE BACK TO 5 FOR NORM
        for (int i = 0; i < 5; i++)
        {
            int lengthOfSongs = artistSongsJson["message"]["body"]["track_list"].Count;
            
            int c = Random.Range(0, lengthOfSongs);
            int correctAnswerPos = Random.Range(0, 4);
            questions[i].correctAnswer = artistSongsJson["message"]["body"]["track_list"][c]["track"]["track_name"];
            questions[i].answers[correctAnswerPos] = questions[i].correctAnswer;
            List<string> alreadyUsedFakeAnswers = new List<string>();
            for (int j = 0; j < 4; j++)
            {
                if (string.IsNullOrEmpty(questions[i].answers[j]))
                {
                    string fakeAnswer = questions[i].correctAnswer;
                    while (fakeAnswer == questions[i].correctAnswer || alreadyUsedFakeAnswers.Contains(fakeAnswer))
                    {
                        fakeAnswer=artistSongsJson["message"]["body"]["track_list"][Random.Range(0,lengthOfSongs)]["track"]["track_name"];
                    }

                    alreadyUsedFakeAnswers.Add(fakeAnswer);
                    questions[i].answers[j] = fakeAnswer;
                }
            }
            
            using (UnityWebRequest request = UnityWebRequest.Get("https://api.musixmatch.com/ws/1.1/track.lyrics.get?track_id="+artistSongsJson["message"]["body"]["track_list"][c]["track"]["track_id"] + "&apikey="+test))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError)
                { Debug.LogError(request.error); }
                else
                {
                    string json = request.downloadHandler.text;
                    SimpleJSON.JSONNode stats = SimpleJSON.JSON.Parse(json);

                    string[] lyricss = ("" + stats["message"]["body"]["lyrics"]["lyrics_body"]).Split(new string[] { "\n" }, System.StringSplitOptions.None);
                    lyricss = lyricss.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    int min= Random.Range(0, lyricss.Length-8);
                    int a = 0;
                    
                    for (int j = min; j < min+4; j++)
                    {
                        try
                        {
                            questions[i].lyrics[a] = lyricss[j];
                        }
                        catch (Exception e)
                        {
                            string s = "";
                            for (int k = 0; k < lyricss.Length; k++)
                            {
                                s += lyricss[j];
                            }
                            Debug.LogError(s);
                        }

                        a++;
                    }

                }
            }
        }
    }
    
    public void MoveLyric()
    {
        if (currentLyricGuess>4 || cantMoveLyric)
        {
            return;
        }

        cantMoveLyric = true;
        lyrics[currentLyricGuess].GetComponent<RectTransform>().DOAnchorPosX(lyricsMovePos[currentLyricGuess], 1).SetEase(Ease.InOutQuad).OnComplete(() => cantMoveLyric = false);
        currentLyricGuess++;
    }


    public void GoToMakeGuess()
    {
        if (cant)
        {
            return;
        }

        cant = true;
        camera.DOMoveY(-30, 1).SetEase(Ease.InOutQuad);
    }
    
    public void MakeGuess(Button n)
    {
        if (cantGuess)
        {
            return;
        }
        cantGuess = true;
        for (int i = 0; i < 4; i++)
        {
            lyrics[i].GetComponent<RectTransform>().DOAnchorPosX(lyricsOgPos[i], 0);
        }
        
        StartCoroutine(MakeGuess2(n));
    }

    public IEnumerator MakeGuess2(Button a)
    {
        if (a.transform.GetChild(0).GetComponent<TMP_Text>().text == questions[questionNumber - 1].correctAnswer)
        {
            a.GetComponent<Image>().DOColor(greenColor, 0.5f);
            correctWrongText.DOColor(greenColor, 0);
            correctWrongText.text = "Correct!";
            correctWrongText.GetComponent<RectTransform>().DOScale(Vector3.one, 1f).SetEase(Ease.InOutCirc);
            int toAdd = (100 - ((currentLyricGuess-1) * 25));
            scoreTextEffect.text = "+" + toAdd;
            scoreTextEffect.GetComponent<RectTransform>().DOAnchorPosY(583, 1.5f).SetEase(Ease.InOutQuad);
            scoreTextEffect.DOFade(1, 0.5f).OnComplete(() => scoreTextEffect.DOFade(0, 0.5f));
            for (int i = score; i < score+toAdd + 1; i++)
            {
                scoreText.text = i.ToString();
                yield return new WaitForSeconds(0.025f);
            }
            score += toAdd;
        }
        else
        {
            a.GetComponent<Image>().DOColor(redColor, 0.5f);
            correctWrongText.DOColor(redColor, 0);
            correctWrongText.text = "Wrong";
            correctWrongText.GetComponent<RectTransform>().DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.75f);
            for (int i = 0; i < 4; i++)
            {
                if (answerButtonTexts[i].text == questions[questionNumber-1].correctAnswer)
                {
                    answerButtonTexts[i].transform.parent.GetComponent<Image>().DOColor(greenColor, 0.5f);
                }
            }
            correctWrongText.GetComponent<RectTransform>().DOShakeAnchorPos(1f, Vector2.one * 10, 10, 90);
        }

        yield return new WaitForSeconds(1f);
        correctWrongText.GetComponent<RectTransform>().DOAnchorPosX(1600, 0.5f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(0.25f);
        if (questionNumber<5)
        {
            StartCoroutine(NextQuestion());   
        }
        else
        {
            EndGame();
        }
    }

    void EndGame()
    {
        yourScoreText.text = "Your Score: " + score;
        camera.DOMoveY(-40, 2).SetEase(Ease.InOutQuad);
    }
    
    public void PlayAgainSame()
    {
        if (cantPressEndButton)
        {
            return;   
        }
        cantPressEndButton = true;
        questionNumber = 0;
        score = 0;
        scoreText.text = "0";
        StartCoroutine(NextQuestion());
    }
    public void PlayAgainNew()
    {
        if (cantPressEndButton)
        {
            return;
        }
        cantPressEndButton = true;
        StartCoroutine(PlayAgainNew2());
    }
    IEnumerator PlayAgainNew2()
    {
        questionNumber = 0;
        score = 0;
        scoreText.text = "0";
        yield return StartCoroutine(GenerateLyricsAndQuestions());
        StartCoroutine(NextQuestion());
    }

    public void NewArtist()
    {
        if (cantPressEndButton)
        {
            return;   
        }

        cantPressEndButton = true;
        fadeOver.DOFade(1, 0.5f).OnComplete(()=> SceneManager.LoadScene("Game"));
    }

    public void ChangeDifficulty(TMP_Text text)
    {
        if (text.text == "Medium")
        {
            difficulty = 50;
            text.text = "Hard";
        }
        else if (text.text == "Hard")
        {
            difficulty = 15;
            text.text = "Easy";
        }
        else if (text.text == "Easy")
        {
            difficulty = 30;
            text.text = "Medium";
        }
    }

    public void ChangeBackgroundColor()
    {
        backgroundImage.DOColor(backgroundColors[questionNumber], 2);
    }

    [System.Serializable]
    public class Question
    {
        public string[] lyrics;
        public string[] answers;
        public string correctAnswer;
    }
    
}
