using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class FieldHUD : UIScene {
    public MinigameHUD CurrentMinigameHUD {
        get {
            return this.currentMinigameHUD;
        }
    }

    public bool IsDisplayChanbaraHUD () {
        return this.currentMinigameHUD == MinigameHUD.Chanbara;
    }

    public bool IsDisplayAuctionHUD () {
        return this.currentMinigameHUD == MinigameHUD.Auction;
    }

    public bool IsDisplayTutorialHUD () {
        return this.currentMinigameHUD == MinigameHUD.MogTutorial;
    }

    public bool IsDisplayJumpRopeHUD () {
        return this.currentMinigameHUD == MinigameHUD.JumpingRope;
    }

    public bool IsDisplayTelescopeHUD () {
        return this.currentMinigameHUD == MinigameHUD.Telescope;
    }

    public bool IsDisplayRacingHippaulHUD () {
        return this.currentMinigameHUD == MinigameHUD.RacingHippaul;
    }

    public bool IsDisplaySwingACageHUD () {
        return this.currentMinigameHUD == MinigameHUD.SwingACage;
    }

    public bool IsDisplayGetTheKeyHUD () {
        return this.currentMinigameHUD == MinigameHUD.GetTheKey;
    }

    public bool IsDisplayChocoHot () {
        return this.currentMinigameHUD == MinigameHUD.ChocoHot;
    }

    public bool IsDisplayChocoHotInstruction () {
        return this.currentMinigameHUD == MinigameHUD.ChocoHotInstruction;
    }

    public bool IsDisplayPandoniumElevator () {
        return this.currentMinigameHUD == MinigameHUD.PandoniumElevator;
    }

    public void DisplaySpecialHUD (MinigameHUD minigameHUD) {
        if (FF9StateSystem.MobilePlatform) {
            this.currentMinigameHUD = minigameHUD;
            switch (minigameHUD) {
                case MinigameHUD.Chanbara:
                    if (this.chanbaraHUDPrefab == null) {
                        this.chanbaraHUDPrefab = (Resources.Load ("EmbeddedAsset/UI/Prefabs/Chanbara HUD Container") as GameObject);
                    }
                    this.currentMinigameHUDGameObject = NGUITools.AddChild (this.MinigameHUDContainer, this.chanbaraHUDPrefab);
                    break;
                case MinigameHUD.Auction:
                case MinigameHUD.PandoniumElevator:
                    if (this.auctionHUDPrefab == null) {
                        this.auctionHUDPrefab = (Resources.Load ("EmbeddedAsset/UI/Prefabs/Auction HUD Container") as GameObject);
                    }
                    this.currentMinigameHUDGameObject = NGUITools.AddChild (this.MinigameHUDContainer, this.auctionHUDPrefab);
                    if (minigameHUD == MinigameHUD.PandoniumElevator) {
                        this.currentMinigameHUDGameObject.transform.GetChild (0).gameObject.SetActive (false);
                        this.currentMinigameHUDGameObject.transform.GetChild (2).gameObject.SetActive (false);
                    }
                    base.StartCoroutine (this.SetAuctionHUDDepth (this.currentMinigameHUDGameObject));
                    break;
                case MinigameHUD.MogTutorial:
                    if (this.mogTutorialHUDPrefab == null) {
                        this.mogTutorialHUDPrefab = (Resources.Load ("EmbeddedAsset/UI/Prefabs/Mognet Tutorial HUD Container") as GameObject);
                    }
                    this.currentMinigameHUDGameObject = NGUITools.AddChild (this.MinigameHUDContainer, this.mogTutorialHUDPrefab);
                    break;
                case MinigameHUD.JumpingRope:
                case MinigameHUD.Telescope:
                case MinigameHUD.GetTheKey:
                case MinigameHUD.ChocoHot:
                    {
                        if (this.jumpingRopeHUDPrefab == null) {
                            this.jumpingRopeHUDPrefab = (Resources.Load ("EmbeddedAsset/UI/Prefabs/Jumping Rope HUD Container") as GameObject);
                        }
                        this.currentMinigameHUDGameObject = NGUITools.AddChild (this.MinigameHUDContainer, this.jumpingRopeHUDPrefab);
                        MinigameHUD minigameHUD2 = this.currentMinigameHUD;
                        if (minigameHUD2 == MinigameHUD.ChocoHot) {
                            Transform child = this.currentMinigameHUDGameObject.transform.GetChild (0);
                            child.GetComponent<OnScreenButton> ().KeyCommand = Control.Special;
                            UISprite component = child.GetComponent<UISprite> ();
                            UIButton component2 = child.GetComponent<UIButton> ();
                            component.spriteName = "button_chocobo_dig_idle";
                            component2.normalSprite = component.spriteName;
                            component2.pressedSprite = "button_chocobo_dig_act";
                        }
                        break;
                    }
                case MinigameHUD.RacingHippaul:
                    if (this.racingHippaulHUDPrefab == null) {
                        this.racingHippaulHUDPrefab = (Resources.Load ("EmbeddedAsset/UI/Prefabs/Racing Hippaul HUD Container") as GameObject);
                    }
                    this.currentMinigameHUDGameObject = NGUITools.AddChild (this.MinigameHUDContainer, this.racingHippaulHUDPrefab);
                    if (FF9StateSystem.Settings.CurrentLanguage == "Japanese") {
                        this.currentMinigameHUDGameObject.transform.GetChild (1).GetComponent<EventButton> ().KeyCommand = Control.Confirm;
                    }
                    break;
                case MinigameHUD.SwingACage:
                    if (this.swingACageHUDPrefab == null) {
                        this.swingACageHUDPrefab = (Resources.Load ("EmbeddedAsset/UI/Prefabs/Swing a Cage HUD Container") as GameObject);
                    }
                    this.currentMinigameHUDGameObject = NGUITools.AddChild (this.MinigameHUDContainer, this.swingACageHUDPrefab);
                    break;
                case MinigameHUD.ChocoHotInstruction:
                    if (this.chocoHotInstructionHUDGameObject == null) {
                        this.chocoHotInstructionHUDGameObject = (Resources.Load ("EmbeddedAsset/UI/Prefabs/Choco Hot Instruction HUD Container") as GameObject);
                    }
                    this.currentMinigameHUDGameObject = NGUITools.AddChild (this.MinigameHUDContainer, this.chocoHotInstructionHUDGameObject);
                    this.currentMinigameHUDGameObject.GetComponent<UIPanel> ().depth = (int) (Dialog.DialogAdditionalRaiseDepth + Dialog.DialogMaximumDepth) - Convert.ToInt32 (Dialog.WindowID.ID0) * 2 + 2;
                    break;
            }
            if (this.currentMinigameHUDGameObject != null) {
                UIWidget component3 = this.currentMinigameHUDGameObject.GetComponent<UIWidget> ();
                int depth = Singleton<DialogManager>.Instance.Widget.depth + 1;
                if (component3 != null) {
                    component3.depth = depth++;
                }
                foreach (object obj in this.currentMinigameHUDGameObject.transform) {
                    Transform transform = (Transform) obj;
                    component3 = transform.GetComponent<UIWidget> ();
                    if (component3 != null) {
                        component3.depth = depth;
                    }
                }
            }
        }
    }

    public void DestroySpecialHUD () {
        this.currentMinigameHUD = MinigameHUD.None;
        if (FF9StateSystem.MobilePlatform) {
            UnityEngine.Object.Destroy (this.currentMinigameHUDGameObject);
        }
    }

    private IEnumerator SetAuctionHUDDepth (GameObject currentMinigameHUDGameObject) {
        yield return new WaitForEndOfFrame ();
        if (currentMinigameHUDGameObject == null) {
            yield break;
        }
        int childCount = currentMinigameHUDGameObject.transform.childCount;
        int buttonDepth = currentMinigameHUDGameObject.transform.GetChild (childCount - 1).GetComponent<UISprite> ().depth + 1;
        for (int i = 0; i < childCount - 1; i++) {
            currentMinigameHUDGameObject.transform.GetChild (i).GetComponent<UISprite> ().depth = buttonDepth;
        }
        yield break;
    }

    public int PauseWidth {
        get {
            if (this.pauseWidth == 0) {
                this.pauseWidth = this.PauseButtonGameObject.GetComponent<UISprite> ().width;
            }
            return this.pauseWidth;
        }
    }

    public override void Show (UIScene.SceneVoidDelegate afterFinished = null) {
        UIScene.SceneVoidDelegate sceneVoidDelegate = delegate () {
            PersistenSingleton<UIManager>.Instance.SetEventEnable (true);
            PersistenSingleton<UIManager>.Instance.SetMenuControlEnable (PersistenSingleton<EventEngine>.Instance.GetUserControl () && EventInput.IsMenuON && EventInput.IsMovementControl);
            PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (PersistenSingleton<EventEngine>.Instance.GetUserControl (), null);
            this.PauseButtonGameObject.SetActive (PersistenSingleton<UIManager>.Instance.IsPauseControlEnable && FF9StateSystem.MobilePlatform);
            ButtonGroupState.HelpEnabled = false;
        };
        if (afterFinished != null) {
            sceneVoidDelegate = (UIScene.SceneVoidDelegate) Delegate.Combine (sceneVoidDelegate, afterFinished);
        }
        base.Show (sceneVoidDelegate);
        PersistenSingleton<UIManager>.Instance.Booster.SetBoosterState (PersistenSingleton<UIManager>.Instance.UnityScene);
        VirtualAnalog.Init (base.gameObject);
        VirtualAnalog.FallbackTouchWidgetList.Add (PersistenSingleton<UIManager>.Instance.gameObject);
        VirtualAnalog.FallbackTouchWidgetList.Add (PersistenSingleton<UIManager>.Instance.Dialogs.gameObject);
        VirtualAnalog.FallbackTouchWidgetList.Add (PersistenSingleton<UIManager>.Instance.Booster.OutsideBoosterHitPoint);
        PersistenSingleton<UIManager>.Instance.SetGameCameraEnable (true);
    }

    public override void Hide (UIScene.SceneVoidDelegate afterFinished = null) {
        UIScene.SceneVoidDelegate sceneVoidDelegate = delegate () {
            if (!base.NextSceneIsModal) {
                PersistenSingleton<UIManager>.Instance.SetGameCameraEnable (false);
            }
        };
        if (afterFinished != null) {
            sceneVoidDelegate = (UIScene.SceneVoidDelegate) Delegate.Combine (sceneVoidDelegate, afterFinished);
        }
        base.Hide (sceneVoidDelegate);
        this.PauseButtonGameObject.SetActive (false);
        this.HelpButtonGameObject.SetActive (false);
        PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, null);
        PersistenSingleton<UIManager>.Instance.SetEventEnable (false);
    }

    public override bool OnKeyMenu (GameObject go) {
        if (base.OnKeyMenu (go)) {
            if (PersistenSingleton<UIManager>.Instance.Dialogs.Visible) {
                PersistenSingleton<UIManager>.Instance.HideAllHUD ();
            }
            this.Hide (delegate () {
                PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.MainMenu);
            });
            PersistenSingleton<UIManager>.Instance.MainMenuScene.NeedTweenAndHideSubMenu = true;
        }
        return true;
    }

    public override bool OnKeyPause (GameObject go) {
        if (base.OnKeyPause (go)) {
            if (this.isShowSkipMovieDialog) {
                return true;
            }
            base.NextSceneIsModal = true;
            this.Hide (delegate () {
                PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Pause);
            });
        }
        return true;
    }

    public override bool OnKeyConfirm (GameObject go) {
        if (base.OnKeyConfirm (go) && this.MovieHitArea.activeSelf && !MBG.IsNull && !MBG.Instance.IsFinished ()) {
            this.MovieHitArea.SetActive (false);
            ETb.sChoose = 1;
            Dialog dialog = Singleton<DialogManager>.Instance.AttachDialog (Localization.Get ("SkipMovieDialog"), 0, 0, Dialog.TailPosition.Center, Dialog.WindowStyle.WindowStylePlain, Vector2.zero, Dialog.CaptionType.Notice);
            this.isShowSkipMovieDialog = true;
            dialog.AfterDialogShown = delegate (int choice) {
                this.previousVibLeft = vib.CurrentVibrateLeft;
                this.previousVibRight = vib.CurrentVibrateRight;
                vib.VIB_actuatorReset (0);
                vib.VIB_actuatorReset (1);
            };
            dialog.AfterDialogHidden = delegate (int choice) {
                if (choice == 0) {
                    MBG.IsSkip = true;
                    fldfmv.FF9FieldFMVShutdown ();
                } else if (!MBG.IsNull && !MBG.Instance.IsFinished ()) {
                    this.MovieHitArea.SetActive (true);
                    this.previousVibLeft = vib.CurrentVibrateLeft;
                    this.previousVibRight = vib.CurrentVibrateRight;
                    vib.VIB_actuatorSet (0, this.previousVibLeft, this.previousVibRight);
                    vib.VIB_actuatorSet (1, this.previousVibLeft, this.previousVibRight);
                }
                this.isShowSkipMovieDialog = false;
            };
        }
        return true;
    }

    public override bool OnKeyCancel (GameObject go) {
        if (base.OnKeyCancel (go) && !PersistenSingleton<UIManager>.Instance.Dialogs.Visible && EventHUD.CurrentHUD == MinigameHUD.None && UIManager.Input.ContainsAndroidQuitKey ()) {
            UIManager.Input.OnQuitCommandDetected ();
        }
        return true;
    }

    public void OnPressButton (GameObject go, bool isPress) {
        if (isPress) {
            PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, null);
        }
    }

    public void EnableATE (bool isEnable, ATEType ateType) {
        this.ATEGameObject.GetComponent<ActiveTimeEvent> ().EnableATE (isEnable, ateType);
    }

    public void InitializeATEText () {
        this.ATEGameObject.GetComponent<ActiveTimeEvent> ().InitializeATE ();
    }

    public void OnATEClick () {
        this.EnableATE (true, ATEType.Blue);
    }

    public void OnItemShopClick (GameObject go, bool isClicked) {
        PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, delegate {
            this.Hide (delegate () {
                PersistenSingleton<UIManager>.Instance.ShopScene.Id = 25;
                PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Shop);
            });
        });
    }

    public void OnWeaponShopClick (GameObject go, bool isClicked) {
        PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, delegate {
            this.Hide (delegate () {
                PersistenSingleton<UIManager>.Instance.ShopScene.Id = 0;
                PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Shop);
            });
        });
    }

    public void OnSynthesisShopClick (GameObject go, bool isClicked) {
        PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, delegate {
            this.Hide (delegate () {
                PersistenSingleton<UIManager>.Instance.ShopScene.Id = 34;
                PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Shop);
            });
        });
    }

    public void OnNameSettingClick (GameObject go, bool isClicked) {
        PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, delegate {
            this.Hide (delegate () {
                PersistenSingleton<UIManager>.Instance.NameSettingScene.SubNo = 1;
                PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.NameSetting);
            });
        });
    }

    public void OnTutorialClick (GameObject go, bool isClicked) {
        PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, delegate {
            base.NextSceneIsModal = true;
            this.Hide (delegate () {
                PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Tutorial);
                PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, null);
            });
        });
    }

    public void OnTitleClick (GameObject go, bool isClicked) {
        PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (false, delegate {
            this.Hide (delegate () {
                PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Title);
            });
        });
    }

    public EventEngine eventEngine {
        get {
            if (this.eventEngineCache == null) {
                this.eventEngineCache = GameObject.Find ("EventEngine").GetComponent<EventEngine> ();
            }
            return this.eventEngineCache;
        }
    }

    public void OnDialogClick (GameObject go, bool isClicked) {
        base.NextSceneIsModal = true;
        this.Hide (delegate () {
            string testingResource = NGUIText.GetTestingResource ();
            Singleton<DialogManager>.Instance.AttachDialog (testingResource, 0, 0, Dialog.TailPosition.AutoPosition, Dialog.WindowStyle.WindowStylePlain, Vector2.zero, Dialog.CaptionType.None);
        });
    }

    public void OnSaveClick (GameObject go, bool isClicked) {
        this.Hide (delegate () {
            PersistenSingleton<UIManager>.Instance.SaveLoadScene.Type = SaveLoadUI.SerializeType.Save;
            PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Serialize);
        });
    }

    public void OnPartyClick (GameObject go, bool isClicked) {
        this.Hide (delegate () {
            FF9PARTY_INFO ff9PARTY_INFO = new FF9PARTY_INFO ();
            ff9PARTY_INFO.menu = new byte[] {
                0,
                1,
                2,
                3
            };
            ff9PARTY_INFO.select = new byte[] {
                4,
                5,
                6,
                7,
                byte.MaxValue,
                byte.MaxValue,
                byte.MaxValue,
                byte.MaxValue
            };
            FF9PARTY_INFO ff9PARTY_INFO2 = ff9PARTY_INFO;
            bool[] array = new bool[9];
            array[0] = true;
            ff9PARTY_INFO2.fix = array;
            ff9PARTY_INFO.party_ct = 4;
            PersistenSingleton<UIManager>.Instance.PartySettingScene.Info = ff9PARTY_INFO;
            PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.PartySetting);
        });
    }

    public void SetButtonVisible (bool isVisible) {
        if (FF9StateSystem.MobilePlatform) {
            this.MenuButtonGameObject.SetActive (isVisible);
            this.HelpButtonGameObject.SetActive (isVisible);
        }
    }

    public void SetPauseVisible (bool isVisible) {
        if (FF9StateSystem.MobilePlatform) {
            this.PauseButtonGameObject.SetActive (isVisible);
        }
    }

    private void Update () {
        BattleHUD.Read ();
        bool flag = this.previousDebugState != this.ShowDebugButton;
        if (flag) {
            this.previousDebugState = this.ShowDebugButton;
            bool showDebugButton = this.ShowDebugButton;
            if (showDebugButton) {
                base.gameObject.GetChild (1).SetActive (true);
            } else {
                base.gameObject.GetChild (1).SetActive (false);
            }
        }
    }

    private void Awake () {
        base.FadingComponent = this.ScreenFadeGameObject.GetComponent<HonoFading> ();
        UIEventListener uieventListener = UIEventListener.Get (this.MenuButtonGameObject);
        uieventListener.onPress = (UIEventListener.BoolDelegate) Delegate.Combine (uieventListener.onPress, new UIEventListener.BoolDelegate (this.OnPressButton));
        UIEventListener uieventListener2 = UIEventListener.Get (this.PauseButtonGameObject);
        uieventListener2.onPress = (UIEventListener.BoolDelegate) Delegate.Combine (uieventListener2.onPress, new UIEventListener.BoolDelegate (this.OnPressButton));
    }

    private void Start () {
        BattleHUD.Read ();
    }

    public GameObject MinigameHUDContainer;

    private GameObject chanbaraHUDPrefab;

    private GameObject auctionHUDPrefab;

    private GameObject mogTutorialHUDPrefab;

    private GameObject jumpingRopeHUDPrefab;

    private GameObject racingHippaulHUDPrefab;

    private GameObject swingACageHUDPrefab;

    private MinigameHUD currentMinigameHUD;

    private GameObject currentMinigameHUDGameObject;

    private GameObject chocoHotInstructionHUDGameObject;

    public bool ShowDebugButton;

    public GameObject MenuButtonGameObject;

    public GameObject PauseButtonGameObject;

    public GameObject HelpButtonGameObject;

    public GameObject ScreenFadeGameObject;

    public GameObject ATEGameObject;

    public GameObject MovieHitArea;

    private GameObject BoosterSliderGameObject;

    private bool previousDebugState;

    private int pauseWidth;

    public bool isShowSkipMovieDialog;

    private float previousVibLeft;

    private float previousVibRight;

    private EventEngine eventEngineCache;
}