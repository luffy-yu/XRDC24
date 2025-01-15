using System;
using Meta.XR.MRUtilityKit;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using XRDC24.AI;
using System.Collections.Generic;
using XRDC24.Bubble;
using UnityEngine.VFX;
using System.Collections;
using XRDC24.Helper;
using XRDC24.Interaction;
using static ButtonBubbleTrigger;

namespace XRDC24.Demo
{
    public class ModuleManagerHack : MonoBehaviour
    {
        // [SerializeField] OVRCameraRig m_OVRCameraRig;
        // [SerializeField] OVRManager m_OVRManager;
        [SerializeField] BubblesManager m_BubbleManager;
        [SerializeField] LLMAgentManager m_LLMAgentManager;
        [SerializeField] STT m_SpeechToText;
        [SerializeField] TTS m_TextToSpeech;
        [SerializeField] MRUK m_MRUK;
        [SerializeField] PortalManager m_PortalManager;

        // UI
        [Header("User Interfaces")] public GameObject m_3DUIPanel;
        public GameObject m_ButtonNext;
        public GameObject m_AIAvatar;
        public VisualEffect m_AIAvatarVFX;
        public GameObject m_AvatarBackground;
        public GameObject m_TitleLogo;
        public GameObject m_3DButton;
        public GameObject m_BubbleButtonStart;
        public GameObject m_BubbleButtonExit;

        private int frameIndex = 0;
        private int audioFrameIndex = 0;
        private TextMeshProUGUI textAgent;
        private TextMeshProUGUI textUser;

        // bubble
        private float positivePercentage;
        private float negativePercentage;
        private int videoClipLength;
        private int totalBubbleN;
        private string moodRes;

        // agent avatar
        private float[] spectrumData = new float[256];
        [Header("AI Avatar Setting")] public Vector3 originalScale = Vector3.one;
        public float originalRate = 1f;
        public float originalAnimatedSpeed = 1f;
        public float scaleMultiplier = 100f;
        public float minFactor = 0.8f;
        public float maxFactor = 1.2f;
        public float amplitudeSmoothTime = 0.1f;
        private float currentAmplitude;
        private float amplitudeVelocity;
        private bool isPoseInit = false;

        // agent pre-defined sound
        public AudioClip m_BubbleAgentSound1;
        public AudioClip m_BubbleAgentSound2;
        private bool isBubbleFirstPoked = true;
        private bool isBubbleSeconPoked = true;

        // scene
        List<MRUKAnchor> walls = new List<MRUKAnchor>();
        MRUKAnchor ground;

        public enum ModuleState
        {
            Enter,
            OnBoarding,
            MoodDetection,
            FreeSpeech,
            Next
        }

        public ModuleState m_ModuleState = ModuleState.OnBoarding;

        private void OnEnable()
        {
            m_TextToSpeech.OnResultAvailable += PlayAIAgentAudioClip;
            m_SpeechToText.OnResultAvailable += SendTextToLLM;
            m_LLMAgentManager.OnMoodResultAvailable += ReceivedMoodResult;
            m_LLMAgentManager.OnMeditationResultAvailable += ReceivedMeditationResult;
            m_BubbleManager.OnBubbleAnimated += OnBubbleTriggered;
            m_BubbleManager.OnBubbleInteractionFinished += ToNext;
            m_3DButton.transform.Find("RecordingButton").GetChild(0).GetComponent<TriggerForwarder>()
                .OnRecordingTriggerEnter += StartRecording;
            m_3DButton.transform.Find("RecordingButton").GetChild(0).GetComponent<TriggerForwarder>()
                .OnRecordingTriggerExit += EndRecording;
            m_BubbleButtonStart.transform.Find("Sphere").GetChild(0).GetComponent<ButtonBubbleTrigger>()
                .OnBubbleButtonClicked += ModuleTrigger;
            m_BubbleButtonExit.transform.Find("Sphere").GetChild(0).GetComponent<ButtonBubbleTrigger>()
                .OnBubbleButtonClicked += ModuleTrigger;
        }

        private void OnDisable()
        {
            m_TextToSpeech.OnResultAvailable -= PlayAIAgentAudioClip;
            m_SpeechToText.OnResultAvailable -= SendTextToLLM;
            m_LLMAgentManager.OnMoodResultAvailable -= ReceivedMoodResult;
            m_LLMAgentManager.OnMeditationResultAvailable -= ReceivedMeditationResult;
            m_BubbleManager.OnBubbleAnimated -= OnBubbleTriggered;
            m_BubbleManager.OnBubbleInteractionFinished -= ToNext;
        }

        private void PlayAIAgentAudioClip(AudioClip clip, string text)
        {
            m_TextToSpeech.audioSource.Play();

            StartCoroutine(ShowAgentText(clip.length, text));
        }

        private IEnumerator ShowAgentText(float second, string text)
        {
            GameObject dialog = virtualCanvasRoot.transform.Find("DialogAgentUser").gameObject;

            // show text
            dialog.SetActive(true);
            dialog.transform.Find("Dialog_Agent").gameObject.SetActive(true);
            textAgent.text = text;

            yield return new WaitForSeconds(second);

            // enable after audio is played
            nextActionAvailable = true;

            // hide text
            dialog.SetActive(false);
            dialog.transform.Find("Dialog_Agent").gameObject.SetActive(false);

            if (m_ModuleState == ModuleState.OnBoarding)
            {
                if (audioFrameIndex == 0)
                {
                    yield return new WaitForSeconds(0.5f);

                    m_3DButton.SetActive(true);
                    virtualCanvasRoot.transform.Find("FigmaCanvas02").gameObject.SetActive(true);

                    frameIndex = 3;
                    audioFrameIndex++;
                }
                else if (audioFrameIndex == 1)
                {
                    frameIndex = 4;
                    audioFrameIndex++;
                }
                else if (audioFrameIndex == 2)
                {
                    // to moode detection mode
                    ToNext();
                }
            }
            else if (m_ModuleState == ModuleState.MoodDetection)
            {
                if (audioFrameIndex == 0)
                {
                    // m_TextToSpeech.SendRequest(
                    // "With each bubble you break, the world around you opens a little more. Not all bubbles need to be touched. Sometimes letting them fall brings new life.");
                    PlayTTSCache(0);
                    audioFrameIndex++;
                }
            }
        }

        private IEnumerator ShowUserText(string text)
        {
            GameObject dialog = virtualCanvasRoot.transform.Find("DialogAgentUser").gameObject;

            // show text
            dialog.SetActive(true);
            dialog.transform.Find("Dialog_User").gameObject.SetActive(true);
            textUser.text = text;

            yield return new WaitForSeconds(3);

            dialog.SetActive(false);
            dialog.transform.Find("Dialog_User").gameObject.SetActive(false);

            // trigger bubble instruction
            // m_TextToSpeech.SendRequest(
            // "Let’s pop some bubbles together. Stretch your hand to break them, or let them float down – it’s all part of the journey!");
            PlayTTSCache(1);
            audioFrameIndex = 0;
        }


        private void SendTextToLLM(string res)
        {
            switch (m_ModuleState)
            {
                case ModuleState.OnBoarding:
                    break;

                case ModuleState.MoodDetection:
                    StartCoroutine(ShowUserText(res));
                    m_LLMAgentManager.SendMsgToGPTForMoodRequest(res);
                    break;

                case ModuleState.FreeSpeech:
                    m_LLMAgentManager.SendMsgToMeditationGPT(res);
                    break;

                default:
                    break;
            }
        }

        private void PlayOnBoardingFrames()
        {
            if (m_ModuleState != ModuleState.OnBoarding)
                return;

            switch (frameIndex)
            {
                case 0:
                    break;

                case 1:
                    StartCoroutine(DisappearingTitle());
                    frameIndex = 0;
                    break;

                case 2:
                    StartCoroutine(ShowOnboardPanel());
                    frameIndex = 0;
                    break;

                case 3:
                    // m_TextToSpeech.SendRequest("Take your time. As you speak, I’ll reflect your words as bubbles.");
                    PlayTTSCache(2);
                    frameIndex = 0;
                    break;

                case 4:
                    // m_TextToSpeech.SendRequest(
                    // "Your positive emotions will form soft, glowing bubbles. If there are heavier \r\nfeelings, they might appear darker or sharper – and that’s okay.");
                    PlayTTSCache(3);
                    frameIndex = 0;
                    audioFrameIndex = 3; // by pass
                    break;

            }
        }

        private IEnumerator ShowOnboardPanel()
        {
            virtualCanvasRoot.transform.Find("FigmaCanvasI01")?.gameObject.SetActive(true);

            yield return new WaitForSeconds(Constants.ONBOARD_PANEL_SHOW_SECONDS);

            virtualCanvasRoot.transform.Find("FigmaCanvasI01")?.gameObject.SetActive(false);

            // to next onboarding frame
            // m_TextToSpeech.SendRequest(
            // "Hello, my name is Flo, your personal mental health assistant, I’m here to listen. How are you feeling today?");
            PlayTTSCache(4);
        }

        private IEnumerator DisappearingTitle()
        {
            yield return new WaitForSeconds(Constants.TITLE_SHOW_SECONDS);

            float time = 0f;
            float startAlpha = 1f;

            while (time <= Constants.TITLE_DISAPPEAR_DURATION)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, time / Constants.TITLE_DISAPPEAR_DURATION);

                // Update the material’s color
                foreach (Transform child in m_TitleLogo.transform)
                {
                    child.GetComponent<MeshRenderer>().material.SetFloat("_OverallAlpha", alpha);
                }


                yield return null;
            }

            m_TitleLogo.SetActive(false);
            frameIndex = 2;
        }

        private void ReceivedMoodResult(string res)
        {
            Debug.Log($"Receive mood result: {res}");
            moodRes = res;

            SpawnBublesByMoodRes();
        }

        private void SpawnBublesByMoodRes()
        {
            // disable the tree background
            m_AvatarBackground.SetActive(false);
            m_3DButton.SetActive(false);

            ProcessResultForBubbleSpawn(moodRes);

            int positiveBubbleN = (int)(totalBubbleN * positivePercentage / 100);
            int negativeBubbleN = totalBubbleN - positiveBubbleN;
            m_BubbleManager.SpawnBubbles(positiveBubbleN, negativeBubbleN);

            Debug.Log($"positve bubbles: {positiveBubbleN}\nnegative bubbles: {negativeBubbleN}");
        }

        private void ReceivedMeditationResult(string res)
        {
            Debug.Log($"receive meditation result: {res}");

            // m_TextToSpeech.SendRequest(res);
            PlayTTSCache(5);
        }

        private void SpawnPortal(Vector3 origin, Vector3 target, AnimationType type)
        {
            var newOrigin = new Vector3(origin.x, origin.y, origin.z);

            newOrigin.y -= 1.0f;

            Ray ray = new Ray(newOrigin, target - newOrigin);

            print("Spawning portal");

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << LayerMask.NameToLayer("Default")))
            {
                print("Spawned portal");

                Vector3 hitPoint = hit.point;
                GameObject hitObject = hit.collider.gameObject;

                m_PortalManager.SpawnPortal(hitPoint, hitObject.transform.rotation);
            }
        }

        private void PlayPreDefinedAgentSound(AnimationType type)
        {
            // add bubble agent sound if needed
            float totalTriggerNum = m_BubbleManager.fallCount + m_BubbleManager.pokeCount;
            if (type == AnimationType.Poke && !m_TextToSpeech.audioSource.isPlaying)
            {
                if (isBubbleFirstPoked)
                {
                    // m_TextToSpeech.SendRequest(
                    // "Wonderful! You’ve touched a bubble – feel the energy flow through you.");

                    // disable to make it play
                    nextActionAvailable = false;
                    PlayTTSCache(6);
                    isBubbleFirstPoked = false;
                }
                else if (isBubbleSeconPoked)
                {
                    // m_TextToSpeech.SendRequest(
                    // "Great focus! Watch as the bubble shatters, opening portals around you.");
                    nextActionAvailable = false;
                    PlayTTSCache(7);
                    isBubbleSeconPoked = false;
                }
            }

            if (totalTriggerNum == 14)
            {
                // disable next
                nextActionAvailable = false;
                // m_TextToSpeech.SendRequest("Every movement matters.");
                PlayTTSCache(8);
            }

            if (totalTriggerNum == 17)
            {
                // disable next
                nextActionAvailable = false;
                // m_TextToSpeech.SendRequest("Keep going at your own pace.");
                PlayTTSCache(9);
            }
        }

        private void OnBubbleTriggered(Vector3 origin, Vector3 target, AnimationType type)
        {
            SpawnPortal(origin, target, type);

            PlayPreDefinedAgentSound(type);
        }

        private void StartRecording(TriggerType type)
        {
            if (type != TriggerType.Recording)
                return;

            virtualCanvasRoot.transform.Find("FigmaCanvas02").gameObject.SetActive(false);
            m_SpeechToText.StartRecording();
        }

        private void EndRecording(TriggerType type)
        {
            if (type != TriggerType.Recording)
                return;

            Debug.Log("inside the end recording");
            m_SpeechToText.EndRecording();
        }

        // public void SceneInitialized()
        // {
        //     Debug.Log("scene loaded and created Successfully");
        //
        //     foreach (MRUKRoom room in MRUK.Instance.Rooms)
        //     {
        //         foreach (MRUKAnchor childAnchor in room.Anchors)
        //         {
        //             if (childAnchor.HasAnyLabel(MRUKAnchor.SceneLabels.WALL_FACE))
        //             {
        //                 walls.Add(childAnchor);
        //             }
        //             else if (childAnchor.HasAnyLabel(MRUKAnchor.SceneLabels.FLOOR))
        //             {
        //                 ground = childAnchor;
        //             }
        //
        //         }
        //     }
        //
        //     Debug.Log($"wall num: {walls.Count}");
        //     Debug.Log($"ground: {ground != null}");
        // }


        // private void SetupSceneMeshes()
        // {
        //     print("SetupSceneMeshes");
        //     // walls
        //     foreach (var anchor in walls)
        //     {
        //         if (anchor.transform.GetChild(0))
        //             anchor.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("RoomMesh");
        //         else
        //             Debug.LogError("No scene meshes were created! Please check!");
        //     }
        //
        //     // ground
        //     if (ground.transform.GetChild(0))
        //         ground.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("RoomMesh");
        //     else
        //         Debug.LogError("No scene meshes were created! Please check!");
        //
        // }

        private void ModuleTrigger(BubbleButtonType type)
        {
            if (type == BubbleButtonType.Start)
            {
                m_ModuleState = ModuleState.OnBoarding;
                frameIndex = 1;
                audioFrameIndex = 0;

                m_BubbleButtonStart.SetActive(false);
            }
            else if (type == BubbleButtonType.Next)
            {
                m_ModuleState = ModuleState.Next;
                TriggerPassthrough(false);

                m_BubbleButtonExit.SetActive(false);
            }
        }

        private void Awake()
        {

        }

        void Start()
        {
            // init states
            m_ModuleState = ModuleState.Enter;
            frameIndex = 0;
            audioFrameIndex = 0;
            m_3DUIPanel.SetActive(true);
            m_3DButton.SetActive(false);
            m_BubbleButtonStart.SetActive(true);
            m_TitleLogo.SetActive(true);
            m_AvatarBackground.SetActive(true);
            m_AIAvatar.SetActive(true);

            textAgent = m_3DUIPanel.transform.Find("DialogAgentUser").Find("Dialog_Agent").GetChild(0).GetChild(0)
                .GetComponent<TextMeshProUGUI>();
            textUser = m_3DUIPanel.transform.Find("DialogAgentUser").Find("Dialog_User").GetChild(0).GetChild(0)
                .GetComponent<TextMeshProUGUI>();

            #region Hack

            // Save the initial position and rotation for reset functionality
            var t = unityCamera.transform;
            initialPosition = t.position;
            initialRotation = t.rotation;

            m_BubbleManager.unityCamera = unityCamera;
            m_PortalManager.unityCamera = unityCamera;

            LoadTTSCache();
            nextActionAvailable = true;

            humanSource = GetComponent<AudioSource>();

            virtualCanvasRoot = new GameObject();
            virtualCanvasRoot.transform.SetAsFirstSibling();
            virtualCanvasRoot.name = "VirtualCanvasRoot";

            // generate virtual canvas
            if (virtualCanvasRoot.transform.childCount == 0)
            {
                var count = m_3DUIPanel.transform.childCount;
                for (var i = 0; i < count; i++)
                {

                    // error for unknown reason
                    try
                    {
                        var child = m_3DUIPanel.transform.GetChild(i).gameObject;
                        var name = child.name.ToLower();
                        if (name.Contains("canvas") || name.Contains("dialog") || name.Contains("cube"))
                        {
                            // change parent
                            child.transform.SetParent(virtualCanvasRoot.transform);
                        }
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            }

            #endregion
        }

        void Update()
        {
            ControlCamera();
            
            AdjustUIPose();

            UpdateAIAvatarScale();
            UpdateAIAvatarParticleRate();

            PlayOnBoardingFrames();
        }

        private void ControlCamera()
        {
            if (!enableCameraControl) return;

            // Camera rotation
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // disable
                enableRotation = false;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) ||
                Input.GetKeyDown(KeyCode.Mouse2))
            {
                // enable
                enableRotation = true;
            }

            if (enableRotation)
            {
                // Mouse for camera rotation
                float mouseSensitivity = 2f;
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

                unityCamera.transform.Rotate(Vector3.up, mouseX, Space.World);
                unityCamera.transform.Rotate(Vector3.left, mouseY, Space.Self);
            }

            // Space to reset position and rotation
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResetPositionAndRotation();
            }
        }

        private void ResetPositionAndRotation()
        {
            unityCamera.transform.position = initialPosition;
            unityCamera.transform.rotation = initialRotation;
        }

        private void AdjustUIPose()
        {
            Vector3 headPos = unityCamera.transform.position;
            if (Vector3.Distance(headPos, virtualCanvasRoot.transform.position) < 1f)
                return;

            Vector3 forward = unityCamera.transform.forward;
            // only update canvas
            virtualCanvasRoot.transform.position = headPos + forward * 0.55f;
            virtualCanvasRoot.transform.rotation = unityCamera.transform.rotation;

            // m_AIAvatar.transform.position = headPos + forward * 1.5f;
            // m_AIAvatar.transform.rotation = unityCamera.transform.rotation;
        }

        private void UpdateAIAvatarScale()
        {
            if (!m_TextToSpeech.audioSource.isPlaying)
            {
                m_AIAvatar.transform.localScale = originalScale;
                return;
            }

            // get the current spectrum data
            m_TextToSpeech.audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

            float sum = 0f;
            for (int i = 0; i < spectrumData.Length; i++)
            {
                sum += spectrumData[i];
            }

            // compute an amplitube
            float targetAmplitude = Mathf.Clamp01(sum / spectrumData.Length);
            targetAmplitude *= scaleMultiplier;
            //Debug.Log(targetAmplitude);

            currentAmplitude = Mathf.SmoothDamp(
                currentAmplitude,
                targetAmplitude * 0.1f,
                ref amplitudeVelocity,
                amplitudeSmoothTime
            );

            m_AIAvatar.transform.localScale = originalScale * (1f + currentAmplitude);
        }

        private void UpdateAIAvatarParticleRate()
        {
            if (!m_TextToSpeech.audioSource.isPlaying)
            {
                m_AIAvatarVFX.playRate = originalRate;
                m_AIAvatar.GetComponent<Animator>().speed = originalAnimatedSpeed;
                return;
            }

            // get the current spectrum data
            m_TextToSpeech.audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

            float sum = 0f;
            for (int i = 0; i < spectrumData.Length; i++)
            {
                sum += spectrumData[i];
            }

            // compute an amplitube
            float targetAmplitude = Mathf.Clamp01(sum / spectrumData.Length);
            targetAmplitude *= scaleMultiplier;
            //Debug.Log(targetAmplitude);

            currentAmplitude = Mathf.SmoothDamp(
                currentAmplitude,
                targetAmplitude,
                ref amplitudeVelocity,
                amplitudeSmoothTime
            );

            m_AIAvatarVFX.playRate = originalRate * (1f + currentAmplitude * 10);
            m_AIAvatar.GetComponent<Animator>().speed = originalAnimatedSpeed * (1f + currentAmplitude);
        }

        public void ProcessResultForBubbleSpawn(string input)
        {
            string pattern = @"positive:\s*(\d+)%\s*and\s*negative:\s*(\d+)%";

            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                positivePercentage = int.Parse(match.Groups[1].Value);
                negativePercentage = int.Parse(match.Groups[2].Value);

                Debug.Log($"Parsed Positive: {positivePercentage}%");
                Debug.Log($"Parsed Negative: {negativePercentage}%");

                videoClipLength = m_SpeechToText.GetVideoClipLength();
                if (videoClipLength > 0 && videoClipLength < 15)
                    totalBubbleN = 5;
                else if (videoClipLength >= 16 && videoClipLength < 30)
                    totalBubbleN = 16;
                else
                    totalBubbleN = 20;
            }
            else
            {
                Debug.LogError("Failed to parse mood result.");
            }
        }

        public void ToNext()
        {
            print("bubbles Finished");
            pokingBubbles = false;
            // disable virtual canvas
            virtualCanvasRoot.SetActive(false);

            // early exit
            if (!pokingBubbles)
            {
                nextActionAvailable = true;
                return;
            }

            m_ModuleState++;

            #region Hack

            pokingBubbles = false;

            #endregion

            // to handle the pipline
            switch (m_ModuleState)
            {
                case ModuleState.OnBoarding:
                    break;

                case ModuleState.MoodDetection:
                    // SetupSceneMeshes();
                    break;

                case ModuleState.FreeSpeech:
                    m_LLMAgentManager.ClearGPTContext();
                    m_BubbleManager.ClearAllBubbles();
                    m_PortalManager.ClearPortals();
                    m_BubbleButtonExit.SetActive(true);
                    break;

                default:
                    break;
            }

        }

        public void TriggerPassthrough(bool isEnabled)
        {
            if (isEnabled)
            {
                // m_OVRManager.isInsightPassthroughEnabled = true;
                unityCamera.clearFlags = CameraClearFlags.SolidColor;
            }
            else
            {
                // m_OVRManager.isInsightPassthroughEnabled = false;
                unityCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }

        #region Hack for Windows Demostration

        private string TTSCacheFolder = "TTSCache";

        private Dictionary<int, AudioClip> TTSCache = new Dictionary<int, AudioClip>();

        private string[] demoTTSStrings = new string[]
        {
            "With each bubble you break, the world around you opens a little more. Not all bubbles need to be touched. Sometimes letting them fall brings new life.",
            "Let’s pop some bubbles together. Stretch your hand to break them, or let them float down – it’s all part of the journey!",
            "Take your time. As you speak, I’ll reflect your words as bubbles.",
            "Your positive emotions will form soft, glowing bubbles. If there are heavier \r\nfeelings, they might appear darker or sharper – and that’s okay.",
            "Hello, my name is Flo, your personal mental health assistant, I’m here to listen. How are you feeling today?",
            "Hi, I’m doing great today! I just got my new VR Headset today. Which I’ve been dreaming about for months.", // this is input from user
            "Wonderful! You’ve touched a bubble – feel the energy flow through you.",
            "Great focus! Watch as the bubble shatters, opening portals around you.",
            "Every movement matters.",
            "Keep going at your own pace.",
        };

        private AudioClip positiveInput;
        private AudioClip positiveOutput;

        void LoadTTSCache()
        {
            print("Loading TTS cache...");
            TTSCache.Clear();
            InitSounds();
        }

        void PlayTTSCache(int index)
        {
            // load clip and get text
            var clip = TTSCache[index];
            var text = demoTTSStrings[index];
            // set clip
            m_TextToSpeech.audioSource.clip = clip;
            PlayAIAgentAudioClip(clip, text);
        }

        void InitSounds()
        {
            var count = demoTTSStrings.Length;
            for (var i = 0; i < count; i++)
            {
                var clip = LoadSound(i);
                TTSCache.Add(i, clip);
            }

            // load positive
            positiveInput = LoadSound("positive_input");
            positiveOutput = LoadSound("positive_output");
        }

        AudioClip LoadSoundByPath(string path)
        {
            AudioClip clip = Resources.Load<AudioClip>(path);

            if (clip != null)
            {
                Debug.Log($"Loaded {path}");
            }
            else
            {
                Debug.LogWarning($"Failed to load {path}");
            }

            return clip;
        }

        AudioClip LoadSound(int index)
        {
            // start from 1
            string path = $"Sounds/Windows/TTSCache_input_{index + 1}";
            return LoadSoundByPath(path);
        }

        AudioClip LoadSound(string name)
        {
            // start from 1
            string path = $"Sounds/Windows/{name}";
            return LoadSoundByPath(path);
        }

        void ClickStart()
        {
            m_BubbleButtonStart.transform.Find("Sphere").GetChild(0).GetComponent<ButtonBubbleTrigger>()
                .ManualTrigger();
            nextActionAvailable = false;
        }

        void PlayPositiveInput()
        {
            print("Playing positive input...");
            var text =
                "Hi, I’m doing great today! I just got my new VR Headset today, which I’ve been dreaming about for months.";
            // m_TextToSpeech.audioSource.clip = positiveInput;

            // play audio first and then show text
            var second = positiveInput.length;
            // m_TextToSpeech.audioSource.Play();

            humanSource.clip = positiveInput;
            humanSource.Play();

            StartCoroutine(DelayShowText(second, text));
        }

        IEnumerator DelayShowText(float second, string text)
        {
            yield return new WaitForSeconds(second + 0.5f);
            yield return ShowAgentText(second, text);
        }

        void PlayPositiveOutput()
        {
            var text =
                "That's awesome! VR headsets are so cool. What games or experiences are you planing to try first?";
            m_TextToSpeech.audioSource.clip = positiveOutput;
            PlayAIAgentAudioClip(positiveOutput, text);
            // change stage
            StartCoroutine(SwitchState(positiveOutput.length));
        }

        IEnumerator SwitchState(float gap)
        {
            yield return new WaitForSeconds(gap);
            m_ModuleState = ModuleState.MoodDetection;
        }

        void SpawnBubbles()
        {
            StartCoroutine(SpwanBubblesImpl());
        }

        IEnumerator SpwanBubblesImpl()
        {
            // wait for voice to be played
            yield return new WaitForSeconds(1.0f);
            
            totalBubbleN = 18;
            int positiveBubbleN = 10;
            int negativeBubbleN = totalBubbleN - positiveBubbleN;
            m_BubbleManager.SpawnBubbles(unityCamera, positiveBubbleN, negativeBubbleN);
        }

        void StartPoking()
        {
            pokingBubbles = true;
        }

        void RandomPokeBubbles()
        {
            m_BubbleManager.TriggerBubbleAnimation();
        }

        void ShowExitButton()
        {
            print("Show exit button...");
            m_BubbleButtonExit.SetActive(true);
        }

        void SwitchToNext()
        {
            print("Switch to next...");
            // remove all portals
            // disable stage 1
            nextActionAvailable = false;
            // disable environment
            m_AvatarBackground.SetActive(false);
            // disable exit button
            m_BubbleButtonExit.SetActive(false);
            // disable camera control
            enableCameraControl = false;
            // reset camera
            ResetPositionAndRotation();
            // switch part2
            SwitchToPart2?.Invoke();
        }

        private int actionIndex = -2;

        private bool nextActionAvailable = false;

        private bool pokingBubbles = false;

        private AudioSource humanSource;

        private GameObject virtualCanvasRoot;

        public System.Action SwitchToPart2;

        [Space(30)] [Header("Hack")] public Camera unityCamera;
        public FullScreenImage fullScreenImage;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool enableRotation = false;

        private bool enableCameraControl = false;

        // public List<GameObject> wallObjects;
        // public List<GameObject> groundObjects;

        public void SimulateAction()
        {
            switch (actionIndex)
            {
                // case -2:
                case -2:
                    fullScreenImage.ShowStart();
                    break;
                // click start
                case -1:
                    fullScreenImage.DisableSplash();
                    m_BubbleButtonStart.SetActive(true);
                    break;
                case 0:
                    ClickStart();
                    break;
                // click recording button, play positive input
                case 1:
                    PlayPositiveInput();
                    break;
                // play positive output
                case 2:
                    // enable camera control
                    enableCameraControl = true;
                    PlayPositiveOutput();
                    SpawnBubbles();
                    break;
                // start poking bubbles
                case 3:
                    StartPoking();
                    break;
                // show exit button
                case 4:
                    ShowExitButton();
                    break;
                // swith to next sceen
                case 5:
                    SwitchToNext();
                    break;
                default:
                    break;
            }
        }

        public void BackToStart()
        {
            // disable camera control
            enableCameraControl = false;
            // reset camera
            ResetPositionAndRotation();
            actionIndex = -1;
            nextActionAvailable = true;
            pokingBubbles = false;
        }

        public void ClearPortals()
        {
            m_PortalManager.ClearPortals();
        }

        private void LateUpdate()
        {
            if (nextActionAvailable && !pokingBubbles && Input.GetKeyUp(KeyCode.N))
            {
                SimulateAction();
                actionIndex++;
            }

            if (nextActionAvailable && pokingBubbles && Input.GetKeyUp(KeyCode.N))
            {
                RandomPokeBubbles();
            }
        }

        #endregion

    }
}