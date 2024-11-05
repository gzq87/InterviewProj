using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageBox : MonoBehaviour
{

    public TMPro.TextMeshProUGUI MessageText;
    private float timeWent = 0f;
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            timeWent += Time.deltaTime;
            if (timeWent >= timer)
            {
                Hide();
            }
        }
        
    }

    public void ShowMessage(string message, float timeSeconds = 2f)
    {
        timer = timeSeconds;
        timeWent = 0;
        MessageText.text = message;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
