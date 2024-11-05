using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class StageStatus
{
    public int CurrentStep = 1;
    public int TotalStep = 0;
    public int FailedTime = 0;
    public List<int> StepList = new List<int>();
    public List<GameNode> gameNodeList = new List<GameNode>();

    public StageStatus(StageData stageData)
    {
        CurrentStep = 1;
        TotalStep = stageData.Answer.Count;
        StepList = new List<int>();
        StepList.Add(stageData.Answer[0]);
        FailedTime = 0;
    }
}

public class StageData
{
    public static string SoundPath = Application.persistentDataPath + "/" + "Sound";
    public static string ImagePath = Application.persistentDataPath + "/" + "Image";

    public int Rows = 0;
    public int Cols = 0;
    public List<int> Answer = new List<int>();
    public List<string> ImageURLs = new List<string>();
    public string SoundURL = string.Empty;
    public List<string> ImageFileNames = new List<string>();
    public List<Sprite> Images = new List<Sprite>();
    public AudioClip Sound = null;
    public string SoundFileName = string.Empty;

    public void Clear()
    {
        Rows = 0; Cols = 0;
        Answer.Clear();
        ImageURLs.Clear();
        SoundURL = string.Empty;
        Images.Clear();
        Sound = null;
        ImageFileNames.Clear();
        SoundFileName = string.Empty;
    }
}

public class DataLoader
{
    private string jsonStr = string.Empty;
    private JObject jsonObject = null;
    private List<StageData> stageDataList = new List<StageData>();

    private static DataLoader instance = null;
    public static DataLoader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DataLoader();
            }
            return instance;
        }
    }

    private DataLoader() {
        TextAsset ta = Resources.Load<TextAsset>("Data/data");
        jsonStr = ta.text.Trim();
        jsonObject = (JObject)(((JArray)JArray.Parse(jsonStr))[0]);
    }

    public List<StageData> GenerateStageData()
    {
        List<StageData> list = new List<StageData>();

        JArray jar = (JArray)jsonObject["Activity"]["Questions"];
        if (jar != null)
        {
            int stageCount = jar.Count;
            for (int i = 0; i < stageCount; i++) {
                StageData data = new StageData();

                data.Rows = (int)jar[i]["Body"]["tags"][0]["rows"];
                data.Cols = (int)jar[i]["Body"]["tags"][0]["cols"];
                JArray jarAnswer = (JArray)jar[i]["Body"]["answers"][0];
                for (int j = 0; j < jarAnswer.Count; j++) {
                    data.Answer.Add((int)jarAnswer[j]);
                }

                JArray jarOptions = (JArray)jar[i]["Body"]["options"];
                for (int j = 0; j < jarOptions.Count; j++) {
                    data.ImageURLs.Add((string)jarOptions[j]["image"]["url"]);
                    data.ImageFileNames.Add(data.ImageURLs[j].Substring(data.ImageURLs[j].LastIndexOf('/') + 1));
                }

                data.SoundURL = (string)jar[i]["stimulusOfQuestion"]["Body"]["item"]["audio"]["url"];
                data.SoundFileName = data.SoundURL.Substring(data.SoundURL.LastIndexOf('/') + 1);

                list.Add(data);
            }
        }

        return list;
    }

    public void ClearDownloadedFiles()
    {
        if (Directory.Exists(StageData.SoundPath))
        {
            Directory.Delete(StageData.SoundPath, true);
        }
        Directory.CreateDirectory(StageData.SoundPath);

        if (Directory.Exists(StageData.ImagePath))
        {
            Directory.Delete(StageData.ImagePath, true);
        }
        Directory.CreateDirectory(StageData.ImagePath);
    }
}
