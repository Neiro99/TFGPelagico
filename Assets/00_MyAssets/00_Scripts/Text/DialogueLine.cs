using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string Name;
    [TextArea(2, 4)] public string text;

    public bool isDecision;
    public List<string> options;
    public List<int> nextLineIndex;
    public int nextLine = -1;
    public List<int> affinityChange;

}
