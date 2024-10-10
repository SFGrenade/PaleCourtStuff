using Modding;
using SFCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using PaleCourtStuff.Consts;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine.Video;
using LanguageStrings = PaleCourtStuff.Consts.LanguageStrings;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;

namespace PaleCourtStuff;

[UsedImplicitly]
public class PaleCourtStuff : Mod
{
    public static PaleCourtStuff Instance;

    public LanguageStrings LangStrings { get; }
    public TextureStrings SpriteDict { get; }

    public static Sprite GetSprite(string name) => Instance.SpriteDict.Get(name);

    private AssetBundle _abTitleScreen = null;
    private AssetBundle _abTitleScreenRick = null;

    // Thx to 56
    public override string GetVersion()
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        string ver = asm.GetName().Version.ToString();

        SHA1 sha1 = SHA1.Create();
        FileStream stream = File.OpenRead(asm.Location);

        byte[] hashBytes = sha1.ComputeHash(stream);

        string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        stream.Close();
        sha1.Clear();

        return $"{ver}-{hash.Substring(0, 6)}";
    }

    public override List<ValueTuple<string, string>> GetPreloadNames()
    {
        List<(string, string)> dict = new List<ValueTuple<string, string>>();
        int max = 499;
        //max = 7;
        for (int i = 0; i < max; i++)
        {
            switch (i)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 8:
                case 32:
                case 33:
                case 34:
                case 47:
                case 48:
                case 49:
                case 102:
                case 103:
                case 115:
                case 126:
                case 133:
                case 180:
                case 181:
                case 209:
                case 221:
                case 229:
                case 232:
                case 238:
                case 261:
                case 325:
                case 355:
                case 361:
                case 392:
                case 410:
                case 411:
                case 412:
                case 413:
                case 414:
                case 415:
                case 416:
                case 417:
                case 418:
                case 419:
                case 420:
                case 463:
                case 470:
                case 478:
                case 481:
                case 497:
                case 498:
                    continue;
            }

            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            dict.Add((Path.GetFileNameWithoutExtension(scenePath), "_SceneManager"));
        }

        return dict;
    }

    private int _fortniteId = -1;
    private int _madeWithUnityId = -1;
    private int _silksongId = -1;
    private int _paleCourtOldId = -1;
    private int _paleCourtNewId = -1;
    private int _paleCourtNewNewId = -1;
    private int _hkLogoBlackId = -1;
    private float _t1;

    public PaleCourtStuff() : base("Testing Stuff")
    {
        LangStrings = new LanguageStrings();
        SpriteDict = new TextureStrings();

        //On.UIManager.Awake += OnUIManagerAwake;
        //On.SetVersionNumber.Start += OnSetVersionNumberStart;

        Log(
            $"MAPPING CSV: \"Scene\",\"Game object\",\"FSM\",\"FSM State\",\"Language Sheet\",\"Language Key\",\"Audioclip 1\",\"Weight\",\"Audioclip 2\",\"Weight\",\"Audioclip 3\",\"Weight\",\"Audioclip 4\",\"Weight\",\"Audioclip 5\",\"Weight\",\"Audioclip 6\",\"Weight\",\"Audioclip 7\",\"Weight\",\"Audioclip 8\",\"Weight\",\"Audioclip 9\",\"Weight\"");
        On.PlayMakerFSM.Awake += (orig, self) =>
        {
            orig(self);
            CheckFsm(self);
        };

        MenuStyleHelper.AddMenuStyleHook += AddRadiantMenuStyle;
        //MenuStyleHelper.AddMenuStyleHook += AddPCMenuStyle;
        MenuStyleHelper.AddMenuStyleHook += AddMenuStyle1;
        MenuStyleHelper.AddMenuStyleHook += AddMenuStyle2;
        //MenuStyleHelper.AddMenuStyleHook += AddMenuStyleRick;

        _fortniteId = TitleLogoHelper.AddLogo(SpriteDict.Get(TextureStrings.FLogoKey));
        _madeWithUnityId = TitleLogoHelper.AddLogo(SpriteDict.Get(TextureStrings.ULogoKey));
        _silksongId = TitleLogoHelper.AddLogo(SpriteDict.Get(TextureStrings.SLogoKey));
        _paleCourtOldId = TitleLogoHelper.AddLogo(SpriteDict.Get(TextureStrings.LogoKey));
        _paleCourtNewId = TitleLogoHelper.AddLogo(SpriteDict.Get(TextureStrings.LogoNewKey));
        _paleCourtNewNewId = TitleLogoHelper.AddLogo(SpriteDict.Get(TextureStrings.LogoNewNewKey));
        _hkLogoBlackId = TitleLogoHelper.AddLogo(SpriteDict.Get(TextureStrings.HkLogoBlackKey));

        //EnviromentParticleHelper.AddCustomDashEffectsHook += AddCustomDashEffectsHook;
        //EnviromentParticleHelper.AddCustomHardLandEffectsHook += AddCustomHardLandEffectsHook;
        //EnviromentParticleHelper.AddCustomJumpEffectsHook += AddCustomJumpEffectsHook;
        //EnviromentParticleHelper.AddCustomSoftLandEffectsHook += AddCustomSoftLandEffectsHook;
        //EnviromentParticleHelper.AddCustomRunEffectsHook += AddCustomRunEffectsHook;
    }

    private void CheckFsm(PlayMakerFSM fsm)
    {
        foreach (FsmState state in fsm.FsmStates)
        {
            bool foundCallMethodProperStartConversation = false;
            bool foundAudioPlayerOneShot = false;
            string conversationKey = "";
            string conversationSheet = "";
            string[] audioClipNames = new string[] {""};
            string[] audioClipPercentages = new string[] {"1.0"};
            foreach (FsmStateAction action in state.Actions)
            {
                if (action is CallMethodProper cmpAction)
                {
                    if (cmpAction.methodName.Value == "StartConversation")
                    {
                        foundCallMethodProperStartConversation = true;
                        conversationKey = (cmpAction.parameters[0].stringValue != "") ? cmpAction.parameters[0].stringValue : conversationKey;
                        conversationSheet = cmpAction.parameters[1].stringValue;
                    }
                }
                if (conversationKey == "" && action is BuildString bsAction)
                {
                    conversationKey = $"{bsAction.stringParts[0].Value}";
                    for (int i = 1; i < bsAction.stringParts.Length; i++)
                    {
                        conversationKey = $"{conversationKey}{bsAction.separator}{bsAction.stringParts[i].Value}";
                    }
                }
                else if (action is AudioPlayerOneShot aposAction)
                {
                    foundAudioPlayerOneShot = true;
                    audioClipNames = aposAction.audioClips.Select(x => x.name).ToArray();
                    audioClipPercentages = aposAction.weights.Select(x => x.Value.ToString("G", CultureInfo.InvariantCulture)).ToArray();
                }
                else if (action is AudioPlayerOneShotSingle apossAction)
                {
                    foundAudioPlayerOneShot = true;
                    audioClipNames = new string[] {(apossAction.audioClip.Value as AudioClip).name};
                    audioClipPercentages = new string[] {1.0f.ToString("G", CultureInfo.InvariantCulture)};
                }
            }

            if (foundCallMethodProperStartConversation && foundAudioPlayerOneShot)
            {
                string csvRow = $"\"{fsm.gameObject.scene.name}\",\"{GetGameObjectPath(fsm.gameObject)}\",\"{fsm.Fsm.Name}\",\"{state.Name}\",\"{conversationSheet}\",\"{conversationKey}\"";
                for (int i = 0; i < audioClipNames.Length; i++)
                {
                    csvRow = $"{csvRow},\"{audioClipNames[i]}\",\"{audioClipPercentages[i]}\"";
                }
                Log($"MAPPING CSV: {csvRow}");
            }
        }
    }

    private string GetGameObjectPath(GameObject go)
    {
        string ret = go.name;
        Transform parentT = go.transform.parent;
        while (parentT)
        {
            ret = $"{parentT.gameObject.name}/{ret}";
            parentT = parentT.parent;
        }
        return ret;
    }

    private void OnSetVersionNumberStart(On.SetVersionNumber.orig_Start orig, SetVersionNumber self)
    {
        orig(self);
        //self.GetAttr<SetVersionNumber, Text>("textUi").text = "1.5.1.3";
    }

    public override void Initialize()
    {
        Log("Initializing");
        Instance = this;

        ModHooks.LanguageGetHook += OnLanguageGetHook;

        //var tmpStyle = MenuStyles.Instance.styles.First(x => x.styleObject.name.Contains("Rick Style"));
        //MenuStyles.Instance.SetStyle(MenuStyles.Instance.styles.ToList().IndexOf(tmpStyle), false);

        Log("Initialized");
    }

    private void OnUIManagerAwake(On.UIManager.orig_Awake orig, UIManager self)
    {
        orig(self);
        ////self.transform.GetChild(1).GetChild(2).GetChild(1).gameObject.SetActive(false);
        ////self.transform.GetChild(1).GetChild(2).GetChild(2).GetChild(0).gameObject.SetActive(false);
        //self.transform.GetChild(1).GetChild(2).GetChild(2).GetChild(0).gameObject.GetComponent<SpriteRenderer>()
        //    .sprite = SpriteDict.Get(TextureStrings.DlcListKey);
    }

    #region Temporary Testing stuff

    private void LogObject(GameObject o, string pre = "")
    {
        string nextPre = pre + "    ";
        LogDebug($"{pre}Object \"{o.name}\"");
        Component[] comps = o.GetComponents<Component>();
        foreach (Component comp in comps)
        {
            string evenMoreNextPre = nextPre + "    ";
            LogDebug($"{nextPre}- Component: \"{comp.GetType().FullName}\"");
            if (comp is ParticleSystemRenderer psr)
            {
                LogDebug($"{evenMoreNextPre}- material: {psr.material}");
                LogDebug($"{evenMoreNextPre}- alignment: {psr.alignment}");
                LogDebug($"{evenMoreNextPre}- sortMode: {psr.sortMode}");
                LogDebug($"{evenMoreNextPre}- lengthScale: {psr.lengthScale}");
                LogDebug($"{evenMoreNextPre}- velocityScale: {psr.velocityScale}");
                LogDebug($"{evenMoreNextPre}- cameraVelocityScale: {psr.cameraVelocityScale}");
                LogDebug($"{evenMoreNextPre}- normalDirection: {psr.normalDirection}");
                LogDebug($"{evenMoreNextPre}- shadowBias: {psr.shadowBias}");
                LogDebug($"{evenMoreNextPre}- sortingFudge: {psr.sortingFudge}");
                LogDebug($"{evenMoreNextPre}- minParticleSize: {psr.minParticleSize}");
                LogDebug($"{evenMoreNextPre}- maxParticleSize: {psr.maxParticleSize}");
                LogDebug($"{evenMoreNextPre}- pivot: {psr.pivot}");
                LogDebug($"{evenMoreNextPre}- flip: {psr.flip}");
                LogDebug($"{evenMoreNextPre}- maskInteraction: {psr.maskInteraction}");
                LogDebug($"{evenMoreNextPre}- trailMaterial: {psr.trailMaterial}");
                LogDebug($"{evenMoreNextPre}- enableGPUInstancing: {psr.enableGPUInstancing}");
                LogDebug($"{evenMoreNextPre}- allowRoll: {psr.allowRoll}");
                LogDebug($"{evenMoreNextPre}- freeformStretching: {psr.freeformStretching}");
                LogDebug($"{evenMoreNextPre}- rotateWithStretchDirection: {psr.rotateWithStretchDirection}");
                LogDebug($"{evenMoreNextPre}- mesh: {psr.mesh}");
                LogDebug($"{evenMoreNextPre}- meshCount: {psr.meshCount}");
                LogDebug($"{evenMoreNextPre}- activeVertexStreamsCount: {psr.activeVertexStreamsCount}");
                LogDebug($"{evenMoreNextPre}- cameraVelocityScale: {psr.cameraVelocityScale}");
            }
        }

        int childCount = o.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            LogObject(o.transform.GetChild(i).gameObject, nextPre);
        }
    }

    private GameObject ChangePsrTexture(GameObject o)
    {
        GameObject ret = GameObject.Instantiate(o, o.transform.parent);
        foreach (ParticleSystemRenderer psr in ret.GetComponentsInChildren<ParticleSystemRenderer>())
        {
            Material newMaterial = UObject.Instantiate(psr.material);
            newMaterial.mainTexture = GetSprite(TextureStrings.PetalKey).texture;
            psr.material = newMaterial;
        }

        return ret;
    }

    GameObject valueAddCustomDashEffectsHookOnce = null;

    private (int enviromentType, GameObject dashEffects) AddCustomDashEffectsHook(DashEffect self)
    {
        if (valueAddCustomDashEffectsHookOnce == null)
        {
            valueAddCustomDashEffectsHookOnce = ChangePsrTexture(self.dashGrass);
        }

        return (7, valueAddCustomDashEffectsHookOnce);
    }

    GameObject valueAddCustomHardLandEffectsHookOnce = null;

    private (int enviromentType, GameObject hardLandEffects) AddCustomHardLandEffectsHook(HardLandEffect self)
    {
        if (valueAddCustomHardLandEffectsHookOnce == null)
        {
            valueAddCustomHardLandEffectsHookOnce = ChangePsrTexture(self.grassObj);
        }

        return (7, valueAddCustomHardLandEffectsHookOnce);
    }

    GameObject valueAddCustomJumpEffectsHookOnce = null;

    private (int enviromentType, GameObject jumpEffects) AddCustomJumpEffectsHook(JumpEffects self)
    {
        if (valueAddCustomJumpEffectsHookOnce == null)
        {
            valueAddCustomJumpEffectsHookOnce = ChangePsrTexture(self.grassEffects);
        }

        return (7, valueAddCustomJumpEffectsHookOnce);
    }

    GameObject valueAddCustomSoftLandEffectsHookOnce = null;

    private (int enviromentType, GameObject softLandEffects) AddCustomSoftLandEffectsHook(SoftLandEffect self)
    {
        if (valueAddCustomSoftLandEffectsHookOnce == null)
        {
            valueAddCustomSoftLandEffectsHookOnce = ChangePsrTexture(self.grassEffects);
        }

        return (7, valueAddCustomSoftLandEffectsHookOnce);
    }

    GameObject valueAddCustomRunEffectsHookOnce = null;

    private (int enviromentType, GameObject runEffects) AddCustomRunEffectsHook(GameObject self)
    {
        if (valueAddCustomRunEffectsHookOnce == null)
        {
            valueAddCustomRunEffectsHookOnce = ChangePsrTexture(self.transform.GetChild(1).gameObject);
        }

        return (7, valueAddCustomRunEffectsHookOnce);
    }

    private (string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achievementKeys, MenuStyles.MenuStyle.CameraCurves
        cameraCurves, AudioMixerSnapshot musicSnapshot) AddRadiantMenuStyle(MenuStyles self)
    {
        GameObject menuStylesGo = self.gameObject;
        GameObject radiantStyleGo = menuStylesGo.transform.GetChild(4).gameObject;
        //foreach (var sr in radiantStyleGo.GetComponentsInChildren<SpriteRenderer>())
        //{
        //    var tmpColor = sr.color;
        //    tmpColor.r *= 0.75f;
        //    tmpColor.g *= 0.75f;
        //    tmpColor.b *= 0.75f;
        //    sr.color = tmpColor;
        //}
        //foreach (var ps in radiantStyleGo.GetComponentsInChildren<ParticleSystem>())
        //{
        //    var main = ps.main;
        //    var tmpGrad = main.startColor;
        //    var tmpColor = tmpGrad.colorMin;
        //    tmpColor.r *= 0.75f;
        //    tmpColor.g *= 0.75f;
        //    tmpColor.b *= 0.75f;
        //    tmpGrad.colorMin = tmpColor;
        //    tmpColor = tmpGrad.colorMax;
        //    tmpColor.r *= 0.75f;
        //    tmpColor.g *= 0.75f;
        //    tmpColor.b *= 0.75f;
        //    tmpGrad.colorMax = tmpColor;
        //    main.startColor = tmpGrad;
        //}
        //radiantStyleGo.transform.localPosition = new Vector3(-6.72f, 3.72f);
        //radiantStyleGo.transform.GetChild(0).localPosition = new Vector3(0, -2.73f, -29.2f);
        //radiantStyleGo.transform.GetChild(0).localEulerAngles = new Vector3(-90, 90, -90);
        //radiantStyleGo.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, -83.9f, -0.19f);
        //radiantStyleGo.transform.GetChild(0).GetChild(0).localEulerAngles = new Vector3(-90, 0, 0);
        //radiantStyleGo.transform.GetChild(0).GetChild(1).localPosition = new Vector3(0, -91.6f, -0.19f);
        //radiantStyleGo.transform.GetChild(0).GetChild(1).localEulerAngles = new Vector3(-90, 0, 29.95f);
        //radiantStyleGo.transform.GetChild(0).GetChild(2).localPosition = new Vector3(0, -163.5f, -0.19f);
        //radiantStyleGo.transform.GetChild(0).GetChild(2).localEulerAngles = new Vector3(-90, 0, 67.2f);
        //radiantStyleGo.transform.GetChild(1).localPosition = new Vector3(0, 0, -1.145f);
        //radiantStyleGo.transform.GetChild(1).localEulerAngles = new Vector3(0, 0, 0);
        //radiantStyleGo.transform.GetChild(1).GetChild(0).localPosition = new Vector3(0, -9, 47.5f);
        //radiantStyleGo.transform.GetChild(1).GetChild(0).localEulerAngles = new Vector3(0, 0, -192.483f);
        //radiantStyleGo.transform.GetChild(2).localPosition = new Vector3(0, 7.4f, 21.21f);
        //radiantStyleGo.transform.GetChild(3).localPosition = new Vector3(0, -32.4f, 103.2f);
        //radiantStyleGo.transform.GetChild(4).localPosition = new Vector3(0, -2.7f, 145.33f);
        //radiantStyleGo.transform.GetChild(5).localPosition = new Vector3(0, -4.22f, 142.91f);

        GameObject audioGo = UObject.Instantiate(self.styles[4].styleObject.transform.GetChild(8).gameObject, radiantStyleGo.transform);
        audioGo.transform.position = Vector3.zero;
        AudioSource aSource = audioGo.GetComponent<AudioSource>();
        //dream_dialogue_loop
        aSource.clip = null;
        foreach (AudioClip ac in Resources.FindObjectsOfTypeAll<AudioClip>())
        {
            if (ac.name == "dream_dialogue_loop")
            {
                aSource.clip = ac;
                break;
            }
        }

        aSource.volume = 0.5f;

        MenuStyles.MenuStyle.CameraCurves cameraCurves = new MenuStyles.MenuStyle.CameraCurves
        {
            saturation = 1.0f,
            redChannel = new AnimationCurve(),
            greenChannel = new AnimationCurve(),
            blueChannel = new AnimationCurve()
        };
        cameraCurves.redChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.redChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.greenChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.greenChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.blueChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.blueChannel.AddKey(new Keyframe(1f, 1f));

        //AudioMixerSnapshot audioSnapshot = self.styles[1].musicSnapshot.audioMixer.FindSnapshot("Normal - Gramaphone");
        AudioMixerSnapshot audioSnapshot = self.styles[1].musicSnapshot.audioMixer.FindSnapshot("Normal");
        return ("UI_MENU_STYLE_RADIANT", radiantStyleGo, /*FortniteId*/ /*HKLogoBlackId*/ -1, "", null, cameraCurves, audioSnapshot);
    }

    private (string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achievementKeys, MenuStyles.MenuStyle.CameraCurves
        cameraCurves, AudioMixerSnapshot musicSnapshot) AddPcMenuStyle(MenuStyles self)
    {
        Log("Start");

        #region Setting up materials

        Material[] blurPlaneMaterials = new Material[1];
        //blurPlaneMaterials[0] = new Material(popShaderPrefab);
        blurPlaneMaterials[0] = new Material(Shader.Find("UI/Blur/UIBlur"));
        blurPlaneMaterials[0].SetColor(Shader.PropertyToID("_TintColor"), new Color(1.0f, 1.0f, 1.0f, 0.0f));
        blurPlaneMaterials[0].SetFloat(Shader.PropertyToID("_Size"), 53.7f);
        //blurPlaneMaterials[0].SetFloat(Shader.PropertyToID("_Size"), 107.4f);
        blurPlaneMaterials[0].SetFloat(Shader.PropertyToID("_Vibrancy"), 0.2f);
        //blurPlaneMaterials[0].SetFloat(Shader.PropertyToID("_Vibrancy"), 1.0f);
        blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_StencilComp"), 8);
        blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_Stencil"), 0);
        blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_StencilOp"), 0);
        blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_StencilWriteMask"), 255);
        blurPlaneMaterials[0].SetInt(Shader.PropertyToID("_StencilReadMask"), 255);
        Material defaultSpriteMaterial = new Material(Shader.Find("Sprites/Default"));
        defaultSpriteMaterial.SetColor(Shader.PropertyToID("_Color"), new Color(1.0f, 1.0f, 1.0f, 1.0f));
        defaultSpriteMaterial.SetInt(Shader.PropertyToID("PixelSnap"), 0);
        defaultSpriteMaterial.SetFloat(Shader.PropertyToID("_EnableExternalAlpha"), 0.0f);
        defaultSpriteMaterial.SetInt(Shader.PropertyToID("_StencilComp"), 8);
        defaultSpriteMaterial.SetInt(Shader.PropertyToID("_Stencil"), 0);
        defaultSpriteMaterial.SetInt(Shader.PropertyToID("_StencilOp"), 0);
        defaultSpriteMaterial.SetInt(Shader.PropertyToID("_StencilWriteMask"), 255);
        defaultSpriteMaterial.SetInt(Shader.PropertyToID("_StencilReadMask"), 255);
        Material litSpriteMaterial = new Material(Shader.Find("Sprites/Lit"));
        litSpriteMaterial.SetColor(Shader.PropertyToID("_Color"), new Color(1.0f, 1.0f, 1.0f, 1.0f));
        litSpriteMaterial.SetInt(Shader.PropertyToID("PixelSnap"), 0);
        litSpriteMaterial.SetFloat(Shader.PropertyToID("_EnableExternalAlpha"), 0.0f);
        litSpriteMaterial.SetInt(Shader.PropertyToID("_StencilComp"), 8);
        litSpriteMaterial.SetInt(Shader.PropertyToID("_Stencil"), 0);
        litSpriteMaterial.SetInt(Shader.PropertyToID("_StencilOp"), 0);
        litSpriteMaterial.SetInt(Shader.PropertyToID("_StencilWriteMask"), 255);
        litSpriteMaterial.SetInt(Shader.PropertyToID("_StencilReadMask"), 255);

        #endregion

        #region Loading assetbundle

        if (_abTitleScreen == null)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using Stream s = asm.GetManifestResourceStream("PaleCourtStuff.Resources.pcts");
            if (s != null)
            {
                _abTitleScreen = AssetBundle.LoadFromStream(s);
            }
        }

        #endregion

        GameObject pcStyleGo = UObject.Instantiate(_abTitleScreen.LoadAsset<GameObject>("Pale_Court_Style_1"));
        _abTitleScreen.Unload(false);
        _abTitleScreen = null;
        if (pcStyleGo == null)
        {
            pcStyleGo = new GameObject();
        }

        foreach (Transform t in pcStyleGo.GetComponentsInChildren<Transform>())
        {
            t.gameObject.SetActive(true);
        }

        foreach (SpriteRenderer t in pcStyleGo.GetComponentsInChildren<SpriteRenderer>())
        {
            t.materials = new[] { defaultSpriteMaterial };
        }

        pcStyleGo.transform.SetParent(self.gameObject.transform);
        pcStyleGo.transform.localPosition = new Vector3(0, -1.2f, 0);

        MenuStyles.MenuStyle.CameraCurves cameraCurves = new MenuStyles.MenuStyle.CameraCurves();
        cameraCurves.saturation = 1f;
        cameraCurves.redChannel = new AnimationCurve();
        cameraCurves.redChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.redChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.greenChannel = new AnimationCurve();
        cameraCurves.greenChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.greenChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.blueChannel = new AnimationCurve();
        cameraCurves.blueChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.blueChannel.AddKey(new Keyframe(1f, 1f));
        UObject.DontDestroyOnLoad(pcStyleGo);
        //PrintDebug(pcStyleGo);

        GameObject fader1 = new GameObject("Fader");
        fader1.transform.SetParent(pcStyleGo.transform);
        fader1.transform.localPosition = new Vector3(-6.125f, -1.75f, 1f);
        fader1.transform.localScale = new Vector3(3, 5, 1);
        SpriteRenderer sr = fader1.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteDict.Get(TextureStrings.FaderKey);
        sr.material = defaultSpriteMaterial;

        return ("UI_MENU_STYLE_PALE_COURT", pcStyleGo, /*PaleCourtOldId*/ /*PaleCourtNewId*/ /*PaleCourtNewNewId*/ _hkLogoBlackId /*-1*/, "", null,
            cameraCurves, null);
    }

    private (string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achievementKeys, MenuStyles.MenuStyle.CameraCurves
        cameraCurves, AudioMixerSnapshot musicSnapshot) AddMenuStyle1(MenuStyles self)
    {
        GameObject pcStyleGo = new GameObject("Unity Style");

        pcStyleGo.transform.SetParent(self.gameObject.transform);
        pcStyleGo.transform.localPosition = new Vector3(0, -1.2f, 0);

        MenuStyles.MenuStyle.CameraCurves cameraCurves = new MenuStyles.MenuStyle.CameraCurves();
        cameraCurves.saturation = 1f;
        cameraCurves.redChannel = new AnimationCurve();
        cameraCurves.redChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.redChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.greenChannel = new AnimationCurve();
        cameraCurves.greenChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.greenChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.blueChannel = new AnimationCurve();
        cameraCurves.blueChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.blueChannel.AddKey(new Keyframe(1f, 1f));
        UObject.DontDestroyOnLoad(pcStyleGo);
        //PrintDebug(pcStyleGo);
        return ("Unity", pcStyleGo, _madeWithUnityId, "", null, cameraCurves, null);
    }

    private (string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achievementKeys, MenuStyles.MenuStyle.CameraCurves
        cameraCurves, AudioMixerSnapshot musicSnapshot) AddMenuStyle2(MenuStyles self)
    {
        GameObject pcStyleGo = UObject.Instantiate(self.transform.GetChild(2).gameObject, self.gameObject.transform, true);
        pcStyleGo.name = "Silksong_Style";

        MenuStyles.MenuStyle.CameraCurves cameraCurves = new MenuStyles.MenuStyle.CameraCurves();
        cameraCurves.saturation = 1f;
        cameraCurves.redChannel = new AnimationCurve();
        cameraCurves.redChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.redChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.greenChannel = new AnimationCurve();
        cameraCurves.greenChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.greenChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.blueChannel = new AnimationCurve();
        cameraCurves.blueChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.blueChannel.AddKey(new Keyframe(1f, 1f));
        UObject.DontDestroyOnLoad(pcStyleGo);
        //PrintDebug(pcStyleGo);
        return ("Silksong", pcStyleGo, _silksongId, "", null, cameraCurves, null);
    }

    private (string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achievementKeys, MenuStyles.MenuStyle.CameraCurves
        cameraCurves, AudioMixerSnapshot musicSnapshot) AddMenuStyleRick(MenuStyles self)
    {
        GameObject pcStyleGo = new GameObject("Rick Style");
        pcStyleGo.SetActive(false);

        pcStyleGo.transform.SetParent(self.gameObject.transform);
        pcStyleGo.transform.localPosition = new Vector3(0, -1.2f, 0);

        #region Loading assetbundle

        if (_abTitleScreenRick == null)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using Stream s = asm.GetManifestResourceStream("PaleCourtStuff.Resources.rick");
            if (s != null)
            {
                _abTitleScreenRick = AssetBundle.LoadFromStream(s);
            }
        }

        #endregion

        VideoPlayer vp = pcStyleGo.AddComponent<VideoPlayer>();
        //vp.playOnAwake = false;
        vp.audioOutputMode = VideoAudioOutputMode.Direct;
        vp.renderMode = VideoRenderMode.CameraFarPlane;
        vp.isLooping = true;
        vp.targetCamera = GameCameras.instance.mainCamera;
        vp.source = VideoSource.VideoClip;
        vp.clip = _abTitleScreenRick.LoadAsset<VideoClip>("ra - nggyu");
        UObject.DontDestroyOnLoad(vp.clip);

        MenuStyles.MenuStyle.CameraCurves cameraCurves = new MenuStyles.MenuStyle.CameraCurves();
        cameraCurves.saturation = 1f;
        cameraCurves.redChannel = new AnimationCurve();
        cameraCurves.redChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.redChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.greenChannel = new AnimationCurve();
        cameraCurves.greenChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.greenChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.blueChannel = new AnimationCurve();
        cameraCurves.blueChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.blueChannel.AddKey(new Keyframe(1f, 1f));
        UObject.DontDestroyOnLoad(pcStyleGo);
        pcStyleGo.SetActive(true);
        //PrintDebug(pcStyleGo);
        return ("Rick", pcStyleGo, -1, "", null, cameraCurves,
            Resources.FindObjectsOfTypeAll<AudioMixer>().First(x => x.name == "Music").FindSnapshot("Silent"));
    }

    #endregion

    private string OnLanguageGetHook(string key, string sheet, string orig)
    {
        //Log($"Sheet: {sheet}; Key: {key}");
        if (LangStrings.ContainsKey(key, sheet))
        {
            return LangStrings.Get(key, sheet);
        }

        return orig;
    }

    private static void SetInactive(GameObject go)
    {
        if (go == null) return;

        UnityEngine.Object.DontDestroyOnLoad(go);
        go.SetActive(false);
    }

    private static void SetInactive(UnityEngine.Object go)
    {
        if (go != null)
        {
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
    }
}