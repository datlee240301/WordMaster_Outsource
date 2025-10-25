using System;
using System.Collections.Generic;

[Serializable]
public class LevelWord
{
    public string word;
}

[Serializable]
public class LevelItem
{
    public int level;
    public List<string> letters;
    public List<LevelWord> words;
}