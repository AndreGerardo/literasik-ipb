using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("Game Reference")]
    private LevelErrorGenerator EG;
    [HideInInspector] public bool isPlaying = false;
    public float _timeRemaining = 120f;
    public float timeRemaining
    {
        get { return _timeRemaining; }
        set { _timeRemaining = value; DisplayTime(_timeRemaining); }
    }
    private int errorCount, _errorFound;
    public int errorFound
    {
        get { return _errorFound; }
        set { _errorFound = value; errorText.text = _errorFound + " / " + errorCount; }
    }

    [Header("TopBar Reference")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text errorText;


    private void Start()
    {
        EG = GetComponent<LevelErrorGenerator>();

        PlayGame();
    }


    private void Update()
    {
        if (isPlaying && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
    }

    public void PlayGame()
    {
        EG.InitializeErrorGeneration();

        errorCount = EG.entireErrorCount;
        errorFound = 0;

        isPlaying = true;
    }

    public void RestartGame()
    {

    }

    public void EndGame()
    {

    }

    private void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

}
