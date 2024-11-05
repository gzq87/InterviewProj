using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    // Start is called before the first frame update
    public bool EnableClick = false;
    public GameObject PanelCover;

    public MessageBox MsgBox;

    public Button playSoundButton;
    public Button lightButton;
    public Button closeButton;
    public Button refreshButton;
    public Button nextButton;
    public Image stepNumberImage;
    public TMPro.TextMeshProUGUI stepNumberText;

    public AudioSource TipsAudioSource;
    public int CurStageIdx = 0;

    public List<StageData> stageDataList = null;
    private StageStatus stageStatus = null;

    public GameObject GameNodePrefab;
    public GameObject GameBoard;
    


    IEnumerator Start()
    {
        MsgBox.ShowMessage("Downloading Resources...", 50);
        DataLoader.Instance.ClearDownloadedFiles();
        stageDataList = DataLoader.Instance.GenerateStageData();
        yield return DownloadAndLoadData(stageDataList);
        //yield return null;
        Debug.Log("Download Finished");
        MsgBox.Hide();

        InitStage(CurStageIdx);

        playSoundButton.onClick.AddListener(playSoundButton_Click);
        lightButton.onClick.AddListener(lightButton_Click);
        closeButton.onClick.AddListener(closeButton_Click);
        refreshButton.onClick.AddListener(refreshButton_Click);
        nextButton.onClick.AddListener(nextButton_Click);


                
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void refreshButton_Click()
    {
        InitStage(CurStageIdx);
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
        ShowCorrectAnswer();
    }

    private void playSoundButton_Click()
    {
        TipsAudioSource.Stop();
        TipsAudioSource.PlayOneShot(stageDataList[CurStageIdx].Sound);
    }

    private void nextButton_Click()
    {
        if (stageStatus.StepList.Count < stageDataList[CurStageIdx].Answer.Count)
        {
            return;
        }

        if (stageStatus.StepList.SequenceEqual(stageDataList[CurStageIdx].Answer))
        {
            if (CurStageIdx >= stageDataList.Count - 1)
            {
                MsgBox.ShowMessage("You Win! This Is The Last Stage!");
            }
            else
            {
                MsgBox.ShowMessage("You Win! Go To Next Stage!");
                CurStageIdx++;
                InitStage(CurStageIdx);
            }
               
        }
        else
        {
            stageStatus.FailedTime++;
            if (stageStatus.FailedTime >= 2)
            {
                ShowCorrectAnswer();
            }
            else
            {
                int startIdx = 0;
                var list = FindIncorrectNodeIdx(out startIdx);

                for (int i = 0; i < list.Count; i++) {
                    stageStatus.gameNodeList[list[i]].StartShake();
                }

                //Go Back
                GoBackToStep(startIdx - 1);
            }

        }
    }

    private void ShowCorrectAnswer()
    {
        MsgBox.ShowMessage("The Correct Answer Is:");
        for (int i = 0; i < stageStatus.gameNodeList.Count; i++)
        {
            stageStatus.gameNodeList[i].FadedOut = true;
            stageStatus.gameNodeList[i].Dissolved = false;
        }
        for (int i = 0;i < stageDataList[CurStageIdx].Answer.Count; i++)
        {
            stageStatus.gameNodeList[stageDataList[CurStageIdx].Answer[i]].FadedOut = false;
            stageStatus.gameNodeList[stageDataList[CurStageIdx].Answer[i]].Dissolved = true;
        }
        EnableClick = false;

        StartCoroutine(GoToNextLevel());
    }

    private IEnumerator GoToNextLevel()
    {
        yield return new WaitForSeconds(2);
        if (CurStageIdx >= stageDataList.Count)
        {
            MsgBox.ShowMessage("This Is The Last Stage! Will Go Back To The 1st Stage Soon!");
            yield return new WaitForSeconds(2);
            CurStageIdx = 0;
            InitStage(CurStageIdx);
        }
        else
        {
            MsgBox.ShowMessage("Go To Next Level Soon!");
            yield return new WaitForSeconds(2);
            CurStageIdx++;
            InitStage(CurStageIdx);
        }
    }

    private void GoBackToStep(int stepIdx)
    {
        for (int i = 0; i < stageStatus.gameNodeList.Count; i++)
        {
            stageStatus.gameNodeList[i].FadedOut = true;
            stageStatus.gameNodeList[i].Dissolved = false;
        }

        for (int i = 0; i <= stepIdx; i++)
        {
            stageStatus.gameNodeList[stageStatus.StepList[i]].Dissolved = true;
            stageStatus.gameNodeList[stageStatus.StepList[i]].FadedOut = false;
        }
        stageStatus.StepList.RemoveRange(stepIdx + 1, stageStatus.StepList.Count - stepIdx - 1);
        stageStatus.CurrentStep = stepIdx + 1;

        var list = GetNeighborGameNodes(stageStatus.StepList[stageStatus.StepList.Count - 1]);
        foreach (var item in list)
        {
            stageStatus.gameNodeList[item].FadedOut = false;
        }
        nextButton.gameObject.SetActive(false);
        stepNumberImage.gameObject.SetActive(true);
        stepNumberText.text = $"{stageStatus.CurrentStep}/{stageStatus.TotalStep}";
        EnableClick = true;
    }

    private List<int> FindIncorrectNodeIdx(out int startIdx)
    {
        List<int> l = new List<int>();
        startIdx = 0;

        for (int i = 0; i < stageStatus.StepList.Count; i++) {
            if (stageStatus.StepList[i] != stageDataList[CurStageIdx].Answer[i])
            {
                startIdx = i;
                l.AddRange(stageStatus.StepList.GetRange(i, stageStatus.StepList.Count - i));
                break;
            }
        }

        return l;
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
        PanelCover.gameObject.SetActive(true);
        EnableClick = false;
        nextButton.gameObject.SetActive(false);
        stepNumberImage.gameObject.SetActive(true);
        stepNumberText.text = $"1/{stageDataList[stageID].Answer.Count}";

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
            gn.Cntlr = this;
            gns.Add(gn);
            gn.SetSpriteCharacter(stageDataList[stageID].Images[i]);
            gn.EnableOutline(false);
            gn.Dissolved = false;
            if (i == stageDataList[stageID].Answer[0])
            {
                gn.FadedOut = false;
                gn.Dissolved = true;
            }
            else
            {
                gn.FadedOut= true;
                gn.Dissolved= false;
            }
        }
        var list = GetNeighborGameNodes(stageDataList[stageID].Answer[0]);
        for (int i = 0; i < list.Count; i++)
        {
            gns[list[i]].FadedOut = false;
        }
        stageStatus.gameNodeList = gns;
        EnableClick = true;
        PanelCover.gameObject.SetActive(false);

        return true;
    }

    private List<int> GetNeighborGameNodes(int Idx)
    {
        List<int> l = new List<int>();

        int row = Idx / stageDataList[CurStageIdx].Cols;
        int col = Idx % stageDataList[CurStageIdx].Cols;

        int row_up = row - 1;
        int row_down = row + 1;
        int col_left = col - 1;
        int col_right = col + 1;

        if (row_up >= 0)
        {
            l.Add(col + row_up * stageDataList[CurStageIdx].Cols);
        }
        if (row_down < stageDataList[CurStageIdx].Rows)
        {
            l.Add(col + row_down * stageDataList[CurStageIdx].Cols);
        }
        if (col_left >= 0)
        {
            l.Add(col_left + row * stageDataList[CurStageIdx].Cols);
        }
        if (col_right < stageDataList[CurStageIdx].Cols)
        {
            l.Add(col_right + row * stageDataList[CurStageIdx].Cols);
        }

        return l;
    }

    public void OnGameNodeClick(int Idx)
    {
        stageStatus.StepList.Add(Idx);
        stageStatus.CurrentStep++;
        stepNumberText.text = $"{stageStatus.StepList.Count}/{stageDataList[CurStageIdx].Answer.Count}";
        if (stageStatus.StepList.Count == stageDataList[CurStageIdx].Answer.Count)
        {
            stepNumberImage.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            EnableClick = false;
        }
        else
        {
            List<int> l = GetNeighborGameNodes(Idx);
            for (int i = 0; i < l.Count; i++)
            {
                stageStatus.gameNodeList[l[i]].FadedOut = false;
            }
        }
    }
}
