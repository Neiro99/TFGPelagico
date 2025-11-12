using System.Collections.Generic;
using UnityEngine;

public static class DialogueCSVLoader
{
    public static List<DialogueLine> LoadDialogue(string csvFileName)
    {
        List<DialogueLine> dialogueLines = new();
        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);
        if (csvFile == null)
        {
            Debug.LogError($"No se encontró el archivo CSV: {csvFileName}");
            return dialogueLines;
        }

        string[] lines = csvFile.text.Replace("\r", "").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            var raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string[] parts = SplitCsvLine(raw);
            if (parts.Length < 2) continue;

            System.Array.Resize(ref parts, 6);

            DialogueLine dl = new DialogueLine
            {
                Name = parts[0]?.Trim() ?? "",
                text = parts[1]?.Trim() ?? "",
                isDecision = !string.IsNullOrWhiteSpace(parts[2]) && parts[2].Trim().ToLower() == "true"
            };

            if (dl.isDecision)
            {
                // Options (3)
                dl.options = new List<string>();
                if (!string.IsNullOrWhiteSpace(parts[3]))
                    dl.options.AddRange(parts[3].Split(';'));

                // NextLines (4)
                dl.nextLineIndex = new List<int>();
                if (!string.IsNullOrWhiteSpace(parts[4]))
                {
                    foreach (var idx in parts[4].Split(';'))
                        if (int.TryParse(idx.Trim(), out int val)) dl.nextLineIndex.Add(val);
                }

                // AffinityChange (5)
                dl.affinityChange = new List<int>();
                if (!string.IsNullOrWhiteSpace(parts[5]))
                {
                    foreach (var a in parts[5].Split(';'))
                        dl.affinityChange.Add(int.TryParse(a.Trim(), out int v) ? v : 0);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(parts[4]) && int.TryParse(parts[4].Trim(), out int next))
                    dl.nextLine = next;
            }

            dialogueLines.Add(dl);
        }

        return dialogueLines;
    }

    static string[] SplitCsvLine(string line)
    {
        var res = new List<string>();
        bool inQuotes = false;
        var cur = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                res.Add(cur.ToString());
                cur.Length = 0;
            }
            else
            {
                cur.Append(c);
            }
        }
        res.Add(cur.ToString());
        return res.ToArray();
    }
}
