using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilePathManager : MonoBehaviourSingletonPersistent<FilePathManager>
{
    private string _MaxUnlockedLevel="MaxUnlockedLevel";
    public string MaxUnlockedLevel => _MaxUnlockedLevel;

    private string _Json=".json";
    public string Json => _Json;

    
    private string _LevelsLevel="Levels/level";
    public string LevelsLevel => _LevelsLevel;


    private string _ColorPath="Color/ColorData";
    public string ColorPath => _ColorPath;

    
}
