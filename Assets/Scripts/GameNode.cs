using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameNode : MonoBehaviour
{
    public Sprite SpriteCharacter;
    public bool FadedOut
    {
        get { return _fadedOut; }
        set
        {
            //_mpbFadedOut.SetFloat("_alpha", value ? 0.5f : 1);
            //ImageCharacter.GetComponent<Renderer>().SetPropertyBlock(_mpbFadedOut);
            ImageBackground.color = value ? new Color(1, 1, 1, 0.5f) : Color.white;
            _fadedOut = value;
        }
    }
    private bool _fadedOut;
    //private MaterialPropertyBlock _mpbFadedOut = new MaterialPropertyBlock();
    
    public bool HasOutline
    {
        get { return _hasOutline; }
        set
        {
            EnableOutline(value);
            _hasOutline = value;
        }
    }
    private bool _hasOutline;

    public Image ImageBackground;
    public Image ImageCharacter;

    private Material characterMaterial = null;

    //Shake Animation
    public float shakeAmount = 15f; // 摇晃幅度
    public float shakeDuration = 0.8f; // 摇晃持续时间
    public bool shakeLoop = false; // 是否循环

    //Scale Animation
    public float scaleAmount = 0.1f;
    public float scaleDuration = 0.8f;
    public bool scaleLoop = false;

    //Fade Animation
    public float fadeDuration = 0.8f;

    public int Idx = 0;

    // Start is called before the first frame update
    void Start()
    {
        characterMaterial = ImageCharacter.material;
        EnableOutline(false);

        //StartShake();
        //StartScale();
        //StartFade();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetSpriteCharacter(Sprite sprite)
    {
        this.SpriteCharacter = sprite;
        ImageCharacter.sprite = sprite;
    }

    public void EnableOutline(bool enable) 
    {
        characterMaterial = ImageCharacter.material;
        characterMaterial.SetInt("_lineWidth", enable ? 20 : 0);
        _hasOutline = enable;
    }

    public void StartShake()
    {
        // 使用 DoTween 创建左右摇晃的动画
        DOTween.Sequence()
            .Append(ImageCharacter.transform.DOShakeRotation(shakeDuration, new Vector3(0, 0, shakeAmount)))
            .SetLoops(shakeLoop ? -1 : 1, LoopType.Restart); // 循环次数，-1 表示无限循环
    }

    public void StartScale()
    {
        DOTween.Sequence()
            .Append(ImageCharacter.transform.DOShakeScale(scaleDuration, scaleAmount))
            .SetLoops(scaleLoop ? -1 : 1, LoopType.Restart);
    }

    public void StartFade(bool fadeOut = true)
    {
        Color endColor = Color.white;
        if (fadeOut) {
            endColor = new Color(1, 1, 1, 0.5f);
        }
        DOTween.Sequence().
            Append(ImageBackground.DOColor(endColor, fadeDuration)).
            AppendCallback(() => { _fadedOut = fadeOut; });
        //ImageBackground.DOColor(endColor, fadeDuration);
        //DOTween.Sequence().
            //Append(() => { }).
            //Append(chracterMaterial.DOFloat(endColor.a, "_alpha", fadeDuration)).
            //AppendCallback(() => { _fadedOut = fadeOut; });
    }

    public void OnClick()
    {
        Debug.Log($"OnClick:{Idx}");
        //EnableOutline(true);
        StartScale();
    }

    void OnDisable()
    {
        // 当对象被禁用时停止所有动画
        DOTween.KillAll();
    }
}
