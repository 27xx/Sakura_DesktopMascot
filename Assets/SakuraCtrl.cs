using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TouchType
{
    Normal,
    Head,
    Special
}

[RequireComponent(typeof(Animator))]
public class SakuraCtrl : MonoBehaviour
{
    List<string> _aniHead, _aniNormal, _aniSpecial, _aniUndef;
    Dictionary<string, AudioClip> _audioClip = new Dictionary<string, AudioClip>();
    Animator _animator;
    AudioSource _audioPlayer;


    static SakuraCtrl _instance;
    public static SakuraCtrl Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SakuraCtrl>();
            }
            return _instance;
        }
    }
    SakuraCtrl() { }

    void Start()
    {
        // 读取动作
        _aniHead = LoadAniName("AnimationClip/Head");
        _aniNormal = LoadAniName("AnimationClip/Normal");
        _aniSpecial = LoadAniName("AnimationClip/Special");
        _aniUndef = LoadAniName("AnimationClip/Undefined");
        Debug.Assert(_aniHead.Count + _aniNormal.Count + _aniSpecial.Count + _aniUndef.Count > 0, "未读取到动作，请将动作分类放到Resources/AnimationClip各文件夹下");
        // 读取音频
        var audio = Resources.LoadAll<AudioClip>("Audio/Sakura_Bridge");
        Debug.Assert(audio.Length > 0, "未读取到音频，请将音频分类放入Resources/Audio/Sakura_Bridge各文件夹下");
        foreach (var audioClip in audio)
        {
            _audioClip.Add(audioClip.name, audioClip);
        }
        // 初始化
        _animator = GetComponent<Animator>();
        Debug.Assert(_animator != null && _animator.hasBoundPlayables, "丢失Animator或AnimatorController");
        _audioPlayer = FindObjectOfType<AudioSource>();
        Debug.Assert(_audioPlayer != null, "丢失AudioPlayer");
        Resources.UnloadUnusedAssets();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) &&
        _animator.GetNextAnimatorClipInfo(0).Length == 0 &&
        _animator.GetCurrentAnimatorStateInfo(0).IsName("Avatar_Yae_Sakura_Ani_StandBy"))
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100f, LayerMask.GetMask("Gal")))
            {
                switch (hitInfo.collider.tag)
                {
                    case "Gal_Normal":
                        Touch(TouchType.Normal);
#if UNITY_EDITOR
                        Debug.Log("普通触摸");
#endif
                        break;
                    case "Gal_Head":
                        Touch(TouchType.Head);
#if UNITY_EDITOR
                        Debug.Log("摸头触摸");
#endif
                        break;
                    case "Gal_Special":
                        Touch(TouchType.Special);
#if UNITY_EDITOR
                        Debug.Log("特殊触摸");
#endif
                        break;
                }
            }
        }
    }

    void Touch(TouchType type)
    {
        string aniName = null;
        switch (type)
        {
            case TouchType.Normal:
                aniName = RandomIndex(_aniNormal);
                break;
            case TouchType.Head:
                aniName = RandomIndex(_aniHead);
                break;
            case TouchType.Special:
                aniName = RandomIndex(_aniSpecial);
                break;
        }
        if (aniName == null)
        {
            aniName = RandomIndex(_aniUndef);
        }
        _animator.CrossFade(aniName, 0.5f, 0);
    }

    // 触发音频
    void TriggerAudioPattern(string audioName)
    {
        if (_audioClip.ContainsKey(audioName))
        {
            _audioPlayer.clip = _audioClip[audioName];
            _audioPlayer.Play();
        }
        else
        {
            Debug.LogWarning("当前Animation Clip中的Audio Clip名称不存在");
        }
    }

    // 触发触摸特效
    void GalTouchEffect(string effectName)
    {
        Debug.Log("触发特效：" + effectName);
    }

    // 触发UI特效
    void PlayUIEffect(string effectName)
    {
        Debug.Log("触发UI特效：" + effectName);
    }

    // 触发黑屏
    void FadeBlack(float time)
    {
        Debug.Log("触发黑屏：" + time);
    }

    // 触发重启
    void RestartGame()
    {
        Debug.Log("触发重启");
    }

    List<string> LoadAniName(string path)
    {
        List<string> names = new List<string>();
        foreach (var item in Resources.LoadAll<AnimationClip>(path))
        {
            names.Add(item.name);
        }
        return names;
    }

    string RandomIndex(List<string> array)
    {
        if (array.Count > 0)
            return array[Random.Range(0, array.Count)];
        else
            return null;
    }
}
