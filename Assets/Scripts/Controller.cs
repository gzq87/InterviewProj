using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    // Start is called before the first frame update

    public Button playSoundButton;
    public Button lightButton;
    public Button closeButton;
    public Button refreshButton;
    public TMPro.TextMeshProUGUI stepNumberText;

    public AudioSource TipsAudioSource;
    public int CurStageIdx = 0;

    public List<StageData> stageDataList = null;
    private StageStatus stageStatus = null;

    public GameObject GameNodePrefab;
    public GameObject GameBoard;
    

    IEnumerator Start()
    {
        DataLoader.Instance.ClearDownloadedFiles();
        stageDataList = DataLoader.Instance.GenerateStageData();
        yield return DownloadAndLoadData(stageDataList);
        //yield return null;
        Debug.Log("Download Finished");

        InitStage(CurStageIdx);

        playSoundButton.onClick.AddListener(playSoundButton_Click);
        lightButton.onClick.AddListener(lightButton_Click);
        closeButton.onClick.AddListener(closeButton_Click);
        refreshButton.onClick.AddListener(refreshButton_Click);


                
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }

    private void refreshButton_Click()
    {

    }

    private void closeButton_Click()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private void lightButton_Click()
    {

    }

    private void playSoundButton_Click()
    {

    }

    private static event Action<string> OnDownloadProgressEvent = null;
    private static event Action OnDownloadCompleteEvent = null;
    private static IEnumerator DownLoad(string url, string desFilePath)
    {
        if (File.Exists(desFilePath))
        {
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.Send();
        if (request.isDone)
        {
            int packLength = 1024 * 20;
            byte[] data = request.downloadHandler.data;
            int nReadSize = 0;
            byte[] nbytes = new byte[packLength];
            using (FileStream fs = new FileStream(desFilePath, FileMode.Create))
            using (Stream netStream = new MemoryStream(data))
            {
                nReadSize = netStream.Read(nbytes, 0, packLength);
                while (nReadSize > 0)
                {
                    fs.Write(nbytes, 0, nReadSize);
                    nReadSize = netStream.Read(nbytes, 0, packLength);
                    double dDownloadedLength = fs.Length * 1.0;
                    double dTotalLength = data.Length * 1.0;
                    string ss = string.Format("{2} {0:F} bytes / {1:F} bytes downloaded", dDownloadedLength, dTotalLength, desFilePath);
                    if (OnDownloadProgressEvent != null)
                    {
                        OnDownloadProgressEvent.Invoke(ss);
                    }
                    Debug.Log(ss);
                    yield return null;
                }

            }
        }

        if (OnDownloadCompleteEvent != null)
        {
            Debug.Log($"{desFilePath} download  finished");
            OnDownloadCompleteEvent.Invoke();
        }
    }

    IEnumerator LoadAudioClipFromPath(string filePath, Action<AudioClip> callback)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError("Error loading audio file: " + www.error);
        }
        else
        {
            // 创建 AudioClip 对象
            callback(DownloadHandlerAudioClip.GetContent(www));
        }
    }

    IEnumerator LoadSpriteFromPath(string filePath, Action<Sprite> callback)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(filePath);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError("Error loading image file: " + www.error);
        }
        else
        {
            // 创建 Texture2D 对象
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            // 创建 Sprite 对象
            callback(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
        }
    }

    IEnumerator DownloadAndLoadData(List<StageData> stageDataList)
    {
        for (int i = 0; i < stageDataList.Count; i++) {
            //Download Sound
            if (!String.IsNullOrEmpty(stageDataList[i].SoundFileName))
            {
                string desFilePath = StageData.SoundPath + "/" + stageDataList[i].SoundFileName;
                yield return DownLoad(stageDataList[i].SoundURL, desFilePath);
                yield return LoadAudioClipFromPath(desFilePath, (audioClip) => { stageDataList[i].Sound = audioClip; });
            }

            for (int j = 0; j < stageDataList[i].ImageURLs.Count; j++)
            {
                string desFilePath = StageData.ImagePath + "/" + stageDataList[i].ImageFileNames[j];
                yield return DownLoad(stageDataList[i].ImageURLs[j], desFilePath);
                yield return LoadSpriteFromPath(desFilePath, (sprite) => { if (sprite != null) stageDataList[i].Images.Add(sprite); });
            }
        }
    }

    bool InitStage(int stageID)
    {
        if (stageID < 0 || stageID >= stageDataList.Count) { 
            return false;
        }

        stageStatus = new StageStatus(stageDataList[stageID]);
        TipsAudioSource.clip = stageDataList[stageID].Sound;

        if (GameBoard.transform.childCount > 0)
        {
            List<GameObject> gos = new List<GameObject>();
            for (int i = 0; i < GameBoard.transform.childCount; i++)
            {
                gos.Add(GameBoard.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < gos.Count; i++) {
                DestroyImmediate(gos[i]);
                gos[i] = null;
            }
            
            gos.Clear();
        }

        GameBoard.GetComponent<GridLayoutGroup>().constraintCount = stageDataList[stageID].Cols;

        List<GameNode> gns = new List<GameNode>();
        for (int i = 0; i < stageDataList[stageID].Images.Count; i++) {
            GameObject go = Instantiate(GameNodePrefab, GameBoard.transform);
            var gn = go.GetComponent<GameNode>();
            gn.Idx = i;
            gns.Add(gn);
            gn.SetSpriteCharacter(stageDataList[stageID].Images[i]);
            gn.EnableOutline(false);
            if (i == stageDataList[stageID].Answer[0])
            {
                gn.FadedOut = false;
            }
            else
            {
                gn.FadedOut= true;
            }
        }

        return true;
    }
}
