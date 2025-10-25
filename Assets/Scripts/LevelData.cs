using System;
using System.Collections.Generic;

[Serializable]
public class LevelWord
{
    public string word;
    public string definition; // thêm field definition
}


[Serializable]
public class LevelItem
{
    public int level;
    public List<string> letters;
    public List<LevelWord> words;
}