using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordData
{
    public string correctWord;
    public string wrongWord;

    public WordData(string correct, string wrong)
    {
        correctWord = correct;
        wrongWord = wrong;
    }
}
