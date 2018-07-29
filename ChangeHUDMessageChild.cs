using System;
using UnityEngine;

// Token: 0x020003E9 RID: 1001
[RequireComponent (typeof (UILabel))]
[RequireComponent (typeof (TweenPosition))]
[RequireComponent (typeof (TweenAlpha))]
public class HUDMessageChild : MonoBehaviour {
	// Token: 0x170002DC RID: 732
	// (get) Token: 0x060019C3 RID: 6595 RVA: 0x0002481B File Offset: 0x00022A1B
	public HUDMessage.MessageStyle DisplayStyle {
		get {
			return this.displayStyle;
		}
	}

	// Token: 0x170002DD RID: 733
	// (get) Token: 0x060019C4 RID: 6596 RVA: 0x00024823 File Offset: 0x00022A23
	public UIFollowTarget Follower {
		get {
			return this.follower;
		}
	}

	// Token: 0x170002DE RID: 734
	// (get) Token: 0x060019C5 RID: 6597 RVA: 0x0002482B File Offset: 0x00022A2B
	// (set) Token: 0x060019C6 RID: 6598 RVA: 0x00024833 File Offset: 0x00022A33
	public byte MessageId {
		get {
			return this.messageId;
		}
		set {
			this.messageId = value;
		}
	}

	// Token: 0x170002DF RID: 735
	// (get) Token: 0x060019C7 RID: 6599 RVA: 0x0002483C File Offset: 0x00022A3C
	public float Countdown {
		get {
			return this.countdown;
		}
	}

	// Token: 0x170002E0 RID: 736
	// (set) Token: 0x060019C8 RID: 6600 RVA: 0x00024844 File Offset: 0x00022A44
	public string Label {
		set {
			this.label.text = value;
		}
	}

	// Token: 0x170002E1 RID: 737
	// (get) Token: 0x060019C9 RID: 6601 RVA: 0x00024852 File Offset: 0x00022A52
	public GameObject ParentGameObject {
		get {
			return this.parentGameObject;
		}
	}

	// Token: 0x060019CA RID: 6602 RVA: 0x0017E960 File Offset: 0x0017CB60
	public void Initial () {
		bool flag = !this.isInitialized;
		if (flag) {
			this.myTransform = base.transform;
			this.myTransform.localScale = Vector2.one * 1.45f; // TehMight
			this.parentGameObject = this.myTransform.parent.gameObject;
			this.label = base.GetComponent<UILabel> ();
			this.uiWidget = base.GetComponent<UIWidget> ();
			this.tweenPosition = base.GetComponent<TweenPosition> ();
			this.tweenAlpha = base.GetComponent<TweenAlpha> ();
			this.follower = this.myTransform.parent.GetComponent<UIFollowTarget> ();
			this.follower.enabled = false;
			this.tweenPositionDuration = this.tweenPosition.duration;
			this.tweenAlphaDuration = this.tweenAlpha.duration;
			this.tweenAlpha.animationCurve = Singleton<HUDMessage>.Instance.alphaTweenCurve;
			this.isInitialized = true;
		}
	}

	// Token: 0x060019CB RID: 6603 RVA: 0x0002485A File Offset: 0x00022A5A
	private void EnableTween (bool isEnabled) {
		this.tweenPosition.enabled = isEnabled;
		this.tweenAlpha.enabled = isEnabled;
	}

	// Token: 0x060019CC RID: 6604 RVA: 0x0017EA54 File Offset: 0x0017CC54
	public void Show (Transform target, string message, HUDMessage.MessageStyle style, Vector3 offset) {
		this.displayStyle = style;
		this.follower.target = target;
		this.follower.targetTransformOffset = offset;
		this.label.text = message;
		this.tweenPosition.duration = this.tweenPositionDuration / Singleton<HUDMessage>.Instance.Speed;
		this.tweenAlpha.duration = this.tweenAlphaDuration / Singleton<HUDMessage>.Instance.Speed;
		switch (style) {
			case HUDMessage.MessageStyle.DAMAGE:
			case HUDMessage.MessageStyle.GUARD:
			case HUDMessage.MessageStyle.MISS:
			case HUDMessage.MessageStyle.DEATH:
				this.DamageSetting ();
				break;
			case HUDMessage.MessageStyle.RESTORE_HP:
			case HUDMessage.MessageStyle.RESTORE_MP:
				this.RestoreSetting ();
				break;
			case HUDMessage.MessageStyle.DEATH_SENTENCE:
				this.DeathSentencesSetting ();
				break;
			case HUDMessage.MessageStyle.PETRIFY:
				this.PetrifySetting ();
				break;
			case HUDMessage.MessageStyle.CRITICAL:
				this.CriticalSetting ();
				break;
		}
		this.PlayAnimation ();
		this.PrintLog (true);
	}

	// Token: 0x060019CD RID: 6605 RVA: 0x0017EB3C File Offset: 0x0017CD3C
	private void DamageSetting () {
		this.uiWidget.color = Singleton<HUDMessage>.Instance.damageColor;
		this.tweenPosition.animationCurve = Singleton<HUDMessage>.Instance.damageTweenCurve;
		this.tweenPosition.to = HUDMessage.NormalTargetPosition;
		this.EnableTween (true);
	}

	// Token: 0x060019CE RID: 6606 RVA: 0x0017EB8C File Offset: 0x0017CD8C
	private void CriticalSetting () {
		this.uiWidget.color = Singleton<HUDMessage>.Instance.criticalColor;
		this.tweenPosition.animationCurve = Singleton<HUDMessage>.Instance.criticalTweenCurve;
		this.tweenPosition.to = HUDMessage.NormalTargetPosition;
		this.EnableTween (true);
	}

	// Token: 0x060019CF RID: 6607 RVA: 0x0017EBDC File Offset: 0x0017CDDC
	private void RestoreSetting () {
		this.label.text = NGUIText.EncodeColor (this.label.text, Singleton<HUDMessage>.Instance.restoreColor);
		this.tweenPosition.animationCurve = Singleton<HUDMessage>.Instance.restoreTweenCurve;
		this.tweenPosition.to = HUDMessage.RecoverTargetPosition;
		this.EnableTween (true);
	}

	// Token: 0x060019D0 RID: 6608 RVA: 0x00024874 File Offset: 0x00022A74
	private void DeathSentencesSetting () {
		this.uiWidget.color = Singleton<HUDMessage>.Instance.damageColor;
		this.EnableTween (false);
	}

	// Token: 0x060019D1 RID: 6609 RVA: 0x00024874 File Offset: 0x00022A74
	private void PetrifySetting () {
		this.uiWidget.color = Singleton<HUDMessage>.Instance.damageColor;
		this.EnableTween (false);
	}

	// Token: 0x060019D2 RID: 6610 RVA: 0x00024892 File Offset: 0x00022A92
	private void PlayAnimation () {
		this.parentGameObject.SetActive (true);
	}

	// Token: 0x060019D3 RID: 6611 RVA: 0x00016C5C File Offset: 0x00014E5C
	private void PrintLog (bool isShow) { }

	// Token: 0x060019D4 RID: 6612 RVA: 0x000248A0 File Offset: 0x00022AA0
	public void SetupCamera (Camera worldCamera, Camera uiCamera) {
		this.follower.worldCam = worldCamera;
		this.follower.uiCam = uiCamera;
	}

	// Token: 0x060019D5 RID: 6613 RVA: 0x0017EC3C File Offset: 0x0017CE3C
	public void Clear () {
		if (this.isInitialized) {
			this.PrintLog (false);
			this.parentGameObject.SetActive (false);
			this.EnableTween (false);
			this.tweenPosition.duration = this.tweenPositionDuration;
			this.tweenAlpha.duration = this.tweenAlphaDuration;
			this.tweenPosition.ResetToBeginning ();
			this.tweenAlpha.ResetToBeginning ();
			this.displayStyle = HUDMessage.MessageStyle.NONE;
			Singleton<HUDMessage>.Instance.FinishMessage (this.messageId);
		}
	}

	// Token: 0x060019D6 RID: 6614 RVA: 0x000248BA File Offset: 0x00022ABA
	public void Pause (bool isPause) {
		this.tweenPosition.IsPause = isPause;
		this.tweenAlpha.IsPause = isPause;
	}

	// Token: 0x04002E48 RID: 11848
	[SerializeField]
	private GameObject parentGameObject;

	// Token: 0x04002E49 RID: 11849
	private Transform myTransform;

	// Token: 0x04002E4A RID: 11850
	private UILabel label;

	// Token: 0x04002E4B RID: 11851
	private UIWidget uiWidget;

	// Token: 0x04002E4C RID: 11852
	private TweenPosition tweenPosition;

	// Token: 0x04002E4D RID: 11853
	private TweenAlpha tweenAlpha;

	// Token: 0x04002E4E RID: 11854
	private UIFollowTarget follower;

	// Token: 0x04002E4F RID: 11855
	[SerializeField]
	private HUDMessage.MessageStyle displayStyle;

	// Token: 0x04002E50 RID: 11856
	private float tweenPositionDuration;

	// Token: 0x04002E51 RID: 11857
	private float tweenAlphaDuration;

	// Token: 0x04002E52 RID: 11858
	[SerializeField]
	private byte messageId;

	// Token: 0x04002E53 RID: 11859
	private float countdown;

	// Token: 0x04002E54 RID: 11860
	private bool isInitialized;
}