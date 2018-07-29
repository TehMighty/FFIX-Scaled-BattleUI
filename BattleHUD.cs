using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using UnityEngine;

public class BattleHUD : UIScene {
	public bool AndroidTVOnKeyRightTrigger (GameObject go) {
		bool result = false;
		if (base.CheckAndroidTVModule (Control.RightTrigger)) {
			result = true;
		}
		return result;
	}

	private void UpdateAndroidTV () {
		HonoInputManager instance = PersistenSingleton<HonoInputManager>.Instance;
		if (FF9StateSystem.AndroidTVPlatform && instance.IsControllerConnect && FF9StateSystem.EnableAndroidTVJoystickMode) {
			float axisRaw = Input.GetAxisRaw (instance.SpecificPlatformRightTriggerKey);
			bool button = Input.GetButton (instance.DefaultJoystickInputKeys[2]);
			bool flag = false;
			if (axisRaw > 0.19f && button) {
				flag = true;
			}
			if (flag && this.lastFrameRightTriggerAxis > 0.19f && this.lastFramePressOnMenu) {
				flag = false;
			}
			if (flag && !this.hidingHud) {
				this.ProcessAutoBattleInput ();
			}
			this.lastFrameRightTriggerAxis = axisRaw;
			this.lastFramePressOnMenu = button;
		}
	}

	public bool BtlWorkLibra {
		get {
			return this.currentLibraMessageNumber > 0;
		}
	}

	public bool BtlWorkPeep {
		get {
			return this.currentPeepingMessageCount > 0;
		}
	}

	private void UpdateMessage () {
		if (this.BattleDialogGameObject.activeSelf && PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.BattleHUD) {
			this.battleMessageCounter += Time.deltaTime * (float) FF9StateSystem.Settings.FastForwardFactor;
			if (this.battleMessageCounter >= (float) BattleHUD.BattleMessageTimeTick[(int) (checked ((IntPtr) FF9StateSystem.Settings.cfg.btl_msg))] / 15f) {
				this.BattleDialogGameObject.SetActive (false);
				this.currentMessagePriority = 0;
				if (this.currentLibraMessageNumber > 0) {
					this.DisplayMessageLibra ();
				}
				if (this.currentPeepingMessageCount > 0) {
					this.DisplayMessagePeeping ();
				}
			}
		}
	}

	private void DisplayBattleMessage (string str, bool isRect) {
		this.BattleDialogGameObject.SetActive (false);
		if (isRect) {
			this.battleDialogWidget.width = (int) (128f * UIManager.ResourceXMultipier);
			this.battleDialogWidget.height = 120;
			this.battleDialogWidget.transform.localPosition = new Vector3 (0f, 445f, 0f);
		} else {
			this.battleDialogWidget.width = (int) (240f * UIManager.ResourceXMultipier);
			if (str.Contains ("\n")) {
				this.battleDialogWidget.height = 200;
				this.battleDialogWidget.transform.localPosition = new Vector3 (-10f, 405f, 0f);
			} else {
				this.battleDialogWidget.height = 120;
				this.battleDialogWidget.transform.localPosition = new Vector3 (-10f, 445f, 0f);
			}
		}
		float num = 0f;
		this.battleDialogLabel.text = this.battleDialogLabel.PhrasePreOpcodeSymbol (str, ref num);
		this.BattleDialogGameObject.SetActive (true);
	}

	private void DisplayMessageLibra () {
		if (this.libraBtlData == null) {
			return;
		}
		string text = string.Empty;
		if (this.currentLibraMessageNumber == 1) {
			if (this.libraBtlData.bi.player != 0) {
				text = btl_util.getPlayerPtr (this.libraBtlData).name;
			} else {
				text = btl_util.getEnemyPtr (this.libraBtlData).et.name;
			}
			text += FF9TextTool.BattleLibraText (10);
			text += this.libraBtlData.level.ToString ();
			this.currentLibraMessageNumber = 2;
		} else if (this.currentLibraMessageNumber == 2) {
			text = FF9TextTool.BattleLibraText (11);
			text += this.libraBtlData.cur.hp;
			text += FF9TextTool.BattleLibraText (13);
			text += this.libraBtlData.max.hp;
			text += FF9TextTool.BattleLibraText (12);
			text += this.libraBtlData.cur.mp;
			text += FF9TextTool.BattleLibraText (13);
			text += this.libraBtlData.max.mp;
			this.currentLibraMessageCount = 0;
			this.currentLibraMessageNumber = 3;
		} else if (this.currentLibraMessageNumber == 3) {
			if (this.libraBtlData.bi.player == 0) {
				int num = (int) FF9StateSystem.Battle.FF9Battle.enemy[(int) this.libraBtlData.bi.slot_no].et.category;
				int num2;
				do {
					byte b;
					this.currentLibraMessageCount = (b = this.currentLibraMessageCount) + 1;
					if ((num2 = (int) b) >= 8) {
						goto IL_1D9;
					}
				}
				while ((num & 1 << num2) == 0);
				text = FF9TextTool.BattleLibraText (num2);
				this.SetBattleMessage (text, 2);
				return;
			}
			IL_1D9:
				this.currentLibraMessageCount = 0;
			this.currentLibraMessageNumber = 4;
		}
		if (this.currentLibraMessageNumber == 4) {
			int num = (int) (this.libraBtlData.def_attr.weak & ~(int) this.libraBtlData.def_attr.invalid);
			int num2;
			do {
				byte b;
				this.currentLibraMessageCount = (b = this.currentLibraMessageCount) + 1;
				if ((num2 = (int) b) >= 8) {
					goto Block_11;
				}
			}
			while ((num & 1 << num2) == 0);
			if (Localization.GetSymbol () == "JP") {
				text = this.BtlGetAttrName (1 << num2);
				text += FF9TextTool.BattleLibraText (14);
			} else {
				text += FF9TextTool.BattleLibraText (14 + num2);
			}
			this.SetBattleMessage (text, 2);
			return;
			Block_11:
				this.currentLibraMessageCount = 0;
			this.currentLibraMessageNumber = 5;
		}
		if (this.currentLibraMessageNumber == 5) {
			this.libraBtlData = null;
			this.currentLibraMessageCount = 0;
			this.currentLibraMessageNumber = 0;
			return;
		}
		this.SetBattleMessage (text, 2);
	}

	private void DisplayMessagePeeping () {
		if (this.peepingEnmData == null) {
			return;
		}
		string text = string.Empty;
		int num2;
		do {
			byte b;
			this.currentPeepingMessageCount = (b = this.currentPeepingMessageCount) + 1;
			int num;
			if ((num = (int) b) >= this.peepingEnmData.steal_item.Length + 1) {
				goto Block_4;
			}
			num2 = (int) this.peepingEnmData.steal_item[this.peepingEnmData.steal_item.Length - num];
		}
		while (num2 == 255);
		if (Localization.GetSymbol () == "JP") {
			text = FF9TextTool.ItemName (num2);
			text += FF9TextTool.BattleLibraText (8);
		} else {
			text = FF9TextTool.BattleLibraText (8);
			text += FF9TextTool.ItemName (num2);
		}
		this.SetBattleMessage (text, 2);
		return;
		Block_4:
			this.peepingEnmData = null;
		this.currentPeepingMessageCount = 0;
	}

	public void SetBattleFollowMessage (int pMesNo, params object[] args) {
		string text = FF9TextTool.BattleFollowText (pMesNo + 7);
		if (string.IsNullOrEmpty (text)) {
			return;
		}
		byte b = (byte) char.GetNumericValue (text[0]);
		text = text.Substring (1);
		if (args.Length > 0) {
			string text2 = args[0].ToString ();
			int num;
			if (int.TryParse (text2, out num)) {
				text = text.Replace ("&", text2);
			} else {
				text = text.Replace ("%", text2);
			}
		}
		this.SetBattleMessage (text, b);
	}

	public void SetBattleCommandTitle (CMD_DATA pCmd) {
		string text = string.Empty;
		string str = (!(Localization.GetSymbol () == "JP")) ? " " : string.Empty;
		byte cmd_no = pCmd.cmd_no;
		if (cmd_no != 14 && cmd_no != 15) {
			if (cmd_no != 50) {
				if (pCmd.sub_no < 192) {
					int num = (int) BattleHUD.CmdTitleTable[(int) pCmd.sub_no];
					int num2 = num;
					if (num2 != 254) {
						if (num2 != 255) {
							if (num2 != 0) {
								if (num < 192) {
									text = FF9TextTool.ActionAbilityName (num);
								} else {
									text = FF9TextTool.BattleCommandTitleText ((num & 63) + 1);
								}
							}
						} else {
							text = FF9TextTool.ActionAbilityName ((int) pCmd.sub_no);
						}
					} else if (Localization.GetSymbol () == "FR" || Localization.GetSymbol () == "IT" || Localization.GetSymbol () == "ES") {
						text = FF9TextTool.BattleCommandTitleText (0) + FF9TextTool.ActionAbilityName ((int) pCmd.sub_no);
					} else {
						text = FF9TextTool.ActionAbilityName ((int) pCmd.sub_no) + str + FF9TextTool.BattleCommandTitleText (0);
					}
				}
			} else {
				text = pCmd.aa.Name;
			}
		} else {
			text = FF9TextTool.ItemName ((int) pCmd.sub_no);
		}
		if (!string.IsNullOrEmpty (text)) {
			this.SetBattleTitle (text, 1);
		}
	}

	public string BtlGetAttrName (int pAttr) {
		int num = 0;
		while ((pAttr >>= 1) != 0) {
			num++;
		}
		return FF9TextTool.BattleFollowText (num);
	}

	public void SetBattleLibra (BTL_DATA pBtl) {
		this.currentLibraMessageNumber = 1;
		this.libraBtlData = pBtl;
		this.DisplayMessageLibra ();
	}

	public void SetBattlePeeping (BTL_DATA pBtl) {
		if (pBtl.bi.player != 0) {
			return;
		}
		this.peepingEnmData = FF9StateSystem.Battle.FF9Battle.enemy[(int) pBtl.bi.slot_no];
		bool flag = false;
		for (int i = 0; i < 4; i++) {
			if (this.peepingEnmData.steal_item[i] != 255) {
				flag = true;
				break;
			}
		}
		if (!flag) {
			this.SetBattleMessage (FF9TextTool.BattleLibraText (9), 2);
			this.currentPeepingMessageCount = 5;
		} else {
			this.currentPeepingMessageCount = 1;
			this.DisplayMessagePeeping ();
		}
	}

	public void SetBattleTitle (string str, byte priority) {
		if (this.currentMessagePriority <= priority) {
			this.currentMessagePriority = priority;
			this.battleMessageCounter = 0f;
			this.DisplayBattleMessage (str, true);
		}
	}

	public void SetBattleMessage (string str, byte priority) {
		if (this.currentMessagePriority <= priority) {
			this.currentMessagePriority = priority;
			this.battleMessageCounter = 0f;
			this.DisplayBattleMessage (str, false);
		}
	}

	private void DisplayCommand () {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		byte menu_type = FF9StateSystem.Common.FF9.party.member[(int) btl_DATA.bi.line_no].info.menu_type;
		byte b;
		byte b2;
		if (Status.checkCurStat (btl_DATA, 16384u)) {
			b = rdata._FF9BMenu_MenuTrance[(int) menu_type, 0];
			b2 = rdata._FF9BMenu_MenuTrance[(int) menu_type, 1];
			this.CommandCaptionLabel.text = Localization.Get ("TranceCaption");
			this.isTranceMenu = true;
		} else {
			b = (byte) rdata._FF9BMenu_MenuNormal[(int) menu_type, 0];
			b2 = (byte) rdata._FF9BMenu_MenuNormal[(int) menu_type, 1];
			this.CommandCaptionLabel.text = Localization.Get ("CommandCaption");
			this.CommandCaptionLabel.color = FF9TextTool.White;
			this.isTranceMenu = false;
		}
		string text = FF9TextTool.CommandName ((int) b);
		string text2 = FF9TextTool.CommandName ((int) b2);
		bool flag = b != 0;
		bool flag2 = b2 != 0;
		if (b2 == 31) {
			if (!this.magicSwordCond.IsViviExist) {
				text2 = string.Empty;
				flag2 = false;
			} else if (this.magicSwordCond.IsViviDead || this.magicSwordCond.IsSteinerMini) {
				flag2 = false;
			}
		}
		this.commandDetailHUD.Skill1Component.Label.text = text;
		ButtonGroupState.SetButtonEnable (this.commandDetailHUD.Skill1, flag);
		ButtonGroupState.SetButtonAnimation (this.commandDetailHUD.Skill1, flag);
		if (flag) {
			this.commandDetailHUD.Skill1Component.Label.color = FF9TextTool.White;
			this.commandDetailHUD.Skill1Component.ButtonGroup.Help.Enable = true;
			this.commandDetailHUD.Skill1Component.ButtonGroup.Help.TextKey = string.Empty;
			this.commandDetailHUD.Skill1Component.ButtonGroup.Help.Text = FF9TextTool.CommandHelpDescription ((int) b);
		} else {
			this.commandDetailHUD.Skill1Component.Label.color = FF9TextTool.Gray;
			this.commandDetailHUD.Skill1Component.UIBoxCollider.enabled = flag;
			this.commandDetailHUD.Skill1Component.ButtonGroup.Help.Enable = false;
		}
		this.commandDetailHUD.Skill2Component.Label.text = text2;
		ButtonGroupState.SetButtonEnable (this.commandDetailHUD.Skill2, flag2);
		ButtonGroupState.SetButtonAnimation (this.commandDetailHUD.Skill2, flag2);
		if (flag2) {
			this.commandDetailHUD.Skill2Component.Label.color = FF9TextTool.White;
			this.commandDetailHUD.Skill2Component.ButtonGroup.Help.Enable = true;
			this.commandDetailHUD.Skill2Component.ButtonGroup.Help.TextKey = string.Empty;
			this.commandDetailHUD.Skill2Component.ButtonGroup.Help.Text = FF9TextTool.CommandHelpDescription ((int) b2);
		} else {
			this.commandDetailHUD.Skill2Component.Label.color = FF9TextTool.Gray;
			this.commandDetailHUD.Skill2Component.UIBoxCollider.enabled = flag2;
			this.commandDetailHUD.Skill2Component.ButtonGroup.Help.Enable = false;
		}
		this.commandDetailHUD.AttackComponent.Label.text = FF9TextTool.CommandName (1);
		this.commandDetailHUD.DefendComponent.Label.text = FF9TextTool.CommandName (4);
		this.commandDetailHUD.ItemComponent.Label.text = FF9TextTool.CommandName (14);
		this.commandDetailHUD.ChangeComponent.Label.text = FF9TextTool.CommandName (7);
		this.commandDetailHUD.AttackComponent.ButtonGroup.Help.TextKey = string.Empty;
		this.commandDetailHUD.AttackComponent.ButtonGroup.Help.Text = FF9TextTool.CommandHelpDescription (1);
		this.commandDetailHUD.DefendComponent.ButtonGroup.Help.TextKey = string.Empty;
		this.commandDetailHUD.DefendComponent.ButtonGroup.Help.Text = FF9TextTool.CommandHelpDescription (4);
		this.commandDetailHUD.ItemComponent.ButtonGroup.Help.TextKey = string.Empty;
		this.commandDetailHUD.ItemComponent.ButtonGroup.Help.Text = FF9TextTool.CommandHelpDescription (14);
		this.commandDetailHUD.ChangeComponent.ButtonGroup.Help.TextKey = string.Empty;
		this.commandDetailHUD.ChangeComponent.ButtonGroup.Help.Text = FF9TextTool.CommandHelpDescription (7);
		if (ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton) {
			this.SetCommandVisibility (true, false);
		}
	}

	private void DisplayStatus (byte subMode) {
		this.StatusContainer.SetActive (true);
		this.hpStatusPanel.SetActive (false);
		this.mpStatusPanel.SetActive (false);
		this.goodStatusPanel.SetActive (false);
		this.badStatusPanel.SetActive (false);
		this.hpCaption.SetActive (true);
		this.mpCaption.SetActive (true);
		this.atbCaption.SetActive (true);
		List<int> list = new List<int> (new int[] {
			0,
			1,
			2,
			3
		});
		switch (subMode) {
			case 1:
				this.hpStatusPanel.SetActive (true);
				this.hpCaption.SetActive (false);
				this.mpCaption.SetActive (false);
				this.atbCaption.SetActive (false);
				for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
					int num = 0;
					while (1 << num != (int) next.btl_id) {
						num++;
					}
					if (next.bi.player != 0 && this.matchBattleIdPlayerList.Contains (num)) {
						int num2 = this.matchBattleIdPlayerList.IndexOf (num);
						BattleHUD.NumberSubModeHUD numberSubModeHUD = this.hpStatusHudList[num2];
						numberSubModeHUD.Self.SetActive (true);
						numberSubModeHUD.Current.text = next.cur.hp.ToString ();
						numberSubModeHUD.Max.text = next.max.hp.ToString ();
						BattleHUD.ParameterStatus parameterStatus = this.CheckHPState (next);
						if (parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY) {
							numberSubModeHUD.TextColor = FF9TextTool.Red;
						} else if (parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL) {
							numberSubModeHUD.TextColor = FF9TextTool.Yellow;
						} else {
							numberSubModeHUD.TextColor = FF9TextTool.White;
						}
						list.Remove (num2);
					}
				}
				foreach (int index in list) {
					BattleHUD.NumberSubModeHUD numberSubModeHUD2 = this.hpStatusHudList[index];
					numberSubModeHUD2.Self.SetActive (false);
				}
				break;
			case 2:
				this.mpStatusPanel.SetActive (true);
				this.hpCaption.SetActive (false);
				this.mpCaption.SetActive (false);
				this.atbCaption.SetActive (false);
				for (BTL_DATA next2 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next2 != null; next2 = next2.next) {
					int num3 = 0;
					while (1 << num3 != (int) next2.btl_id) {
						num3++;
					}
					if (next2.bi.player != 0 && this.matchBattleIdPlayerList.Contains (num3)) {
						int num4 = this.matchBattleIdPlayerList.IndexOf (num3);
						BattleHUD.NumberSubModeHUD numberSubModeHUD3 = this.mpStatusHudList[num4];
						numberSubModeHUD3.Self.SetActive (true);
						numberSubModeHUD3.Current.text = next2.cur.mp.ToString ();
						numberSubModeHUD3.Max.text = next2.max.mp.ToString ();
						BattleHUD.ParameterStatus parameterStatus2 = this.CheckMPState (next2);
						if (parameterStatus2 == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY) {
							numberSubModeHUD3.TextColor = FF9TextTool.Yellow;
						} else {
							numberSubModeHUD3.TextColor = FF9TextTool.White;
						}
						list.Remove (num4);
					}
				}
				foreach (int index2 in list) {
					BattleHUD.NumberSubModeHUD numberSubModeHUD4 = this.mpStatusHudList[index2];
					numberSubModeHUD4.Self.SetActive (false);
				}
				break;
			case 3:
				this.badStatusPanel.SetActive (true);
				this.hpCaption.SetActive (false);
				this.mpCaption.SetActive (false);
				this.atbCaption.SetActive (false);
				for (BTL_DATA next3 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next3 != null; next3 = next3.next) {
					int num5 = 0;
					while (1 << num5 != (int) next3.btl_id) {
						num5++;
					}
					if (next3.bi.player != 0 && this.matchBattleIdPlayerList.Contains (num5)) {
						int num6 = this.matchBattleIdPlayerList.IndexOf (num5);
						BattleHUD.StatusSubModeHUD statusSubModeHUD = this.badStatusHudList[num6];
						uint num7 = next3.stat.cur | next3.stat.permanent;
						statusSubModeHUD.Self.SetActive (true);
						foreach (UISprite uisprite in statusSubModeHUD.StatusesSpriteList) {
							uisprite.alpha = 0f;
						}
						int num8 = 0;
						foreach (KeyValuePair<uint, byte> keyValuePair in BattleHUD.BadIconDict) {
							if ((num7 & keyValuePair.Key) != 0u) {
								statusSubModeHUD.StatusesSpriteList[num8].alpha = 1f;
								statusSubModeHUD.StatusesSpriteList[num8].spriteName = FF9UIDataTool.IconSpriteName[(int) keyValuePair.Value];
								num8++;
								if (num8 > statusSubModeHUD.StatusesSpriteList.Length) {
									break;
								}
							}
						}
						list.Remove (num6);
					}
				}
				foreach (int index3 in list) {
					BattleHUD.StatusSubModeHUD statusSubModeHUD2 = this.badStatusHudList[index3];
					statusSubModeHUD2.Self.SetActive (false);
				}
				break;
			case 4:
				this.goodStatusPanel.SetActive (true);
				this.hpCaption.SetActive (false);
				this.mpCaption.SetActive (false);
				this.atbCaption.SetActive (false);
				for (BTL_DATA next4 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next4 != null; next4 = next4.next) {
					int num9 = 0;
					while (1 << num9 != (int) next4.btl_id) {
						num9++;
					}
					if (next4.bi.player != 0 && this.matchBattleIdPlayerList.Contains (num9)) {
						int num10 = this.matchBattleIdPlayerList.IndexOf (num9);
						BattleHUD.StatusSubModeHUD statusSubModeHUD3 = this.goodStatusHudList[num10];
						uint num11 = next4.stat.cur | next4.stat.permanent;
						statusSubModeHUD3.Self.SetActive (true);
						foreach (UISprite uisprite2 in statusSubModeHUD3.StatusesSpriteList) {
							uisprite2.alpha = 0f;
						}
						int num12 = 0;
						foreach (KeyValuePair<uint, byte> keyValuePair2 in BattleHUD.GoodIconDict) {
							if ((num11 & keyValuePair2.Key) != 0u) {
								statusSubModeHUD3.StatusesSpriteList[num12].alpha = 1f;
								statusSubModeHUD3.StatusesSpriteList[num12].spriteName = FF9UIDataTool.IconSpriteName[(int) keyValuePair2.Value];
								num12++;
								if (num12 > statusSubModeHUD3.StatusesSpriteList.Length) {
									break;
								}
							}
						}
						list.Remove (num10);
					}
				}
				foreach (int index4 in list) {
					BattleHUD.StatusSubModeHUD statusSubModeHUD4 = this.goodStatusHudList[index4];
					statusSubModeHUD4.Self.SetActive (false);
				}
				break;
		}
	}

	private void DisplayAbilityRealTime () {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		if (this.currentSilenceStatus != btl_stat.CheckStatus (btl_DATA, 8u)) {
			this.currentSilenceStatus = !this.currentSilenceStatus;
			this.DisplayAbility ();
		}
		if (this.currentMpValue != (int) btl_DATA.cur.mp) {
			this.currentMpValue = (int) btl_DATA.cur.mp;
			this.DisplayAbility ();
		}
	}

	private void DisplayItemRealTime () {
		if (this.needItemUpdate) {
			this.needItemUpdate = false;
			rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[(int) ((UIntPtr) this.currentCommandId)];
			this.DisplayItem (ff9COMMAND.type == 3);
		}
	}

	private void DisplayStatusRealtime () {
		if (this.hpStatusPanel.activeSelf) {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				if (next.bi.player != 0) {
					int num = 0;
					while (1 << num != (int) next.btl_id) {
						num++;
					}
					if (this.matchBattleIdPlayerList.Contains (num)) {
						int index = this.matchBattleIdPlayerList.IndexOf (num);
						BattleHUD.NumberSubModeHUD numberSubModeHUD = this.hpStatusHudList[index];
						numberSubModeHUD.Self.SetActive (true);
						numberSubModeHUD.Current.text = next.cur.hp.ToString ();
						numberSubModeHUD.Max.text = next.max.hp.ToString ();
						BattleHUD.ParameterStatus parameterStatus = this.CheckHPState (next);
						if (parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY) {
							numberSubModeHUD.TextColor = FF9TextTool.Red;
						} else if (parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL) {
							numberSubModeHUD.TextColor = FF9TextTool.Yellow;
						} else {
							numberSubModeHUD.TextColor = FF9TextTool.White;
						}
					}
				}
			}
		} else if (this.mpStatusPanel.activeSelf) {
			for (BTL_DATA next2 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next2 != null; next2 = next2.next) {
				if (next2.bi.player != 0) {
					int num2 = 0;
					while (1 << num2 != (int) next2.btl_id) {
						num2++;
					}
					if (this.matchBattleIdPlayerList.Contains (num2)) {
						int index2 = this.matchBattleIdPlayerList.IndexOf (num2);
						BattleHUD.NumberSubModeHUD numberSubModeHUD2 = this.mpStatusHudList[index2];
						numberSubModeHUD2.Self.SetActive (true);
						numberSubModeHUD2.Current.text = next2.cur.mp.ToString ();
						numberSubModeHUD2.Max.text = next2.max.mp.ToString ();
						BattleHUD.ParameterStatus parameterStatus2 = this.CheckMPState (next2);
						if (parameterStatus2 == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY) {
							numberSubModeHUD2.TextColor = FF9TextTool.Yellow;
						} else {
							numberSubModeHUD2.TextColor = FF9TextTool.White;
						}
					}
				}
			}
		} else if (this.badStatusPanel.activeSelf) {
			for (BTL_DATA next3 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next3 != null; next3 = next3.next) {
				if (next3.bi.player != 0) {
					int num3 = 0;
					while (1 << num3 != (int) next3.btl_id) {
						num3++;
					}
					if (this.matchBattleIdPlayerList.Contains (num3)) {
						int index3 = this.matchBattleIdPlayerList.IndexOf (num3);
						BattleHUD.StatusSubModeHUD statusSubModeHUD = this.badStatusHudList[index3];
						uint num4 = next3.stat.cur | next3.stat.permanent;
						statusSubModeHUD.Self.SetActive (true);
						foreach (UISprite uisprite in statusSubModeHUD.StatusesSpriteList) {
							uisprite.alpha = 0f;
						}
						int num5 = 0;
						foreach (KeyValuePair<uint, byte> keyValuePair in BattleHUD.BadIconDict) {
							if ((num4 & keyValuePair.Key) != 0u) {
								statusSubModeHUD.StatusesSpriteList[num5].alpha = 1f;
								statusSubModeHUD.StatusesSpriteList[num5].spriteName = FF9UIDataTool.IconSpriteName[(int) keyValuePair.Value];
								num5++;
								if (num5 > statusSubModeHUD.StatusesSpriteList.Length) {
									break;
								}
							}
						}
					}
				}
			}
		} else if (this.goodStatusPanel.activeSelf) {
			for (BTL_DATA next4 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next4 != null; next4 = next4.next) {
				if (next4.bi.player != 0) {
					int num6 = 0;
					while (1 << num6 != (int) next4.btl_id) {
						num6++;
					}
					if (this.matchBattleIdPlayerList.Contains (num6)) {
						int index4 = this.matchBattleIdPlayerList.IndexOf (num6);
						BattleHUD.StatusSubModeHUD statusSubModeHUD2 = this.goodStatusHudList[index4];
						uint num7 = next4.stat.cur | next4.stat.permanent;
						statusSubModeHUD2.Self.SetActive (true);
						foreach (UISprite uisprite2 in statusSubModeHUD2.StatusesSpriteList) {
							uisprite2.alpha = 0f;
						}
						int num8 = 0;
						foreach (KeyValuePair<uint, byte> keyValuePair2 in BattleHUD.GoodIconDict) {
							if ((num7 & keyValuePair2.Key) != 0u) {
								statusSubModeHUD2.StatusesSpriteList[num8].alpha = 1f;
								statusSubModeHUD2.StatusesSpriteList[num8].spriteName = FF9UIDataTool.IconSpriteName[(int) keyValuePair2.Value];
								num8++;
								if (num8 > statusSubModeHUD2.StatusesSpriteList.Length) {
									break;
								}
							}
						}
					}
				}
			}
		}
	}

	private void DisplayItem (bool isThrow) {
		this.itemIdList.Clear ();
		List<ListDataTypeBase> list = new List<ListDataTypeBase> ();
		foreach (FF9ITEM ff9ITEM in FF9StateSystem.Common.FF9.item) {
			if (!isThrow) {
				if (citem.YCITEM_IS_ITEM ((int) ff9ITEM.id) && ff9ITEM.count > 0) {
					this.itemIdList.Add ((int) ff9ITEM.id);
					list.Add (new BattleHUD.BattleItemListData {
						Count = (int) ff9ITEM.count,
							Id = (int) ff9ITEM.id
					});
				}
			} else {
				if (citem.YCITEM_IS_THROW ((int) ff9ITEM.id) && ff9ITEM.count > 0) {
					this.itemIdList.Add ((int) ff9ITEM.id);
					list.Add (new BattleHUD.BattleItemListData {
						Count = (int) ff9ITEM.count,
							Id = (int) ff9ITEM.id
					});
				}
			}
		}
		if (list.Count == 0) {
			this.itemIdList.Add (255);
			list.Add (new BattleHUD.BattleItemListData {
				Count = 0,
					Id = 255
			});
		}
		if (this.itemScrollList.ItemsPool.Count == 0) {
			this.itemScrollList.PopulateListItemWithData = new Action<Transform, ListDataTypeBase, int, bool> (this.DisplayItemDetail);
			this.itemScrollList.OnRecycleListItemClick += this.OnListItemClick;
			this.itemScrollList.Invoke ("RepositionList", 0.1f);
			this.itemScrollList.InitTableView (list, 0);
		} else {
			this.itemScrollList.SetOriginalData (list);
			this.itemScrollList.Invoke ("RepositionList", 0.1f);
		}
	}

	public void DisplayItemDetail (Transform item, ListDataTypeBase data, int index, bool isInit) {
		BattleHUD.BattleItemListData battleItemListData = (BattleHUD.BattleItemListData) data;
		ItemListDetailWithIconHUD itemListDetailWithIconHUD = new ItemListDetailWithIconHUD (item.gameObject, true);
		if (isInit) {
			this.DisplayWindowBackground (item.gameObject, null);
		}
		if (battleItemListData.Id == 255) {
			itemListDetailWithIconHUD.IconSprite.alpha = 0f;
			itemListDetailWithIconHUD.NameLabel.text = string.Empty;
			itemListDetailWithIconHUD.NumberLabel.text = string.Empty;
			itemListDetailWithIconHUD.Button.Help.Enable = false;
			itemListDetailWithIconHUD.Button.Help.TextKey = string.Empty;
			itemListDetailWithIconHUD.Button.Help.Text = string.Empty;
		} else {
			FF9UIDataTool.DisplayItem (battleItemListData.Id, itemListDetailWithIconHUD.IconSprite, itemListDetailWithIconHUD.NameLabel, true);
			itemListDetailWithIconHUD.NumberLabel.text = battleItemListData.Count.ToString ();
			itemListDetailWithIconHUD.Button.Help.Enable = true;
			itemListDetailWithIconHUD.Button.Help.TextKey = string.Empty;
			itemListDetailWithIconHUD.Button.Help.Text = FF9TextTool.ItemBattleDescription (battleItemListData.Id);
		}
	}

	private void DisplayAbility () {
		rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) this.currentCommandId))];
		this.SetAbilityAp (this.abilityDetailDict[this.currentPlayerId]);
		List<ListDataTypeBase> list = new List<ListDataTypeBase> ();
		for (int i = (int) ff9COMMAND.ability; i < (int) ff9COMMAND.ability + (int) ff9COMMAND.count; i++) {
			list.Add (new BattleHUD.BattleAbilityListData {
				Index = i
			});
		}
		bool flag = this.abilityScrollList.ItemsPool.Count == 0;
		if (flag) {
			this.abilityScrollList.PopulateListItemWithData = new Action<Transform, ListDataTypeBase, int, bool> (this.DisplayAbilityDetail);
			this.abilityScrollList.OnRecycleListItemClick += this.OnListItemClick;
			this.abilityScrollList.Invoke ("RepositionList", 0.1f);
			this.abilityScrollList.InitTableView (list, 0);
		} else {
			this.abilityScrollList.SetOriginalData (list);
			this.abilityScrollList.Invoke ("RepositionList", 0.1f);
		}
	}

	private void DisplayAbilityDetail (Transform item, ListDataTypeBase data, int index, bool isInit) {
		BattleHUD.BattleAbilityListData battleAbilityListData = (BattleHUD.BattleAbilityListData) data;
		ItemListDetailHUD itemListDetailHUD = new ItemListDetailHUD (item.gameObject);
		if (isInit) {
			this.DisplayWindowBackground (item.gameObject, null);
		}
		int num = rdata._FF9BMenu_ComAbil[battleAbilityListData.Index];
		BattleHUD.AbilityStatus abilityState = this.GetAbilityState (num);
		AA_DATA aaData = FF9StateSystem.Battle.FF9Battle.aa_data[num];
		if (abilityState == BattleHUD.AbilityStatus.ABILSTAT_NONE) {
			itemListDetailHUD.Content.SetActive (false);
			ButtonGroupState.SetButtonAnimation (itemListDetailHUD.Self, false);
			itemListDetailHUD.Button.Help.TextKey = string.Empty;
			itemListDetailHUD.Button.Help.Text = string.Empty;
		} else {
			itemListDetailHUD.Content.SetActive (true);
			itemListDetailHUD.NameLabel.text = FF9TextTool.ActionAbilityName (num);
			int mp = this.GetMp (aaData);
			if (mp != 0) {
				itemListDetailHUD.NumberLabel.text = mp.ToString ();
			} else {
				itemListDetailHUD.NumberLabel.text = string.Empty;
			}
			if (abilityState == BattleHUD.AbilityStatus.ABILSTAT_DISABLE) {
				itemListDetailHUD.NameLabel.color = FF9TextTool.Gray;
				itemListDetailHUD.NumberLabel.color = FF9TextTool.Gray;
				ButtonGroupState.SetButtonAnimation (itemListDetailHUD.Self, false);
			} else {
				itemListDetailHUD.NameLabel.color = FF9TextTool.White;
				itemListDetailHUD.NumberLabel.color = FF9TextTool.White;
				ButtonGroupState.SetButtonAnimation (itemListDetailHUD.Self, true);
			}
			itemListDetailHUD.Button.Help.TextKey = string.Empty;
			itemListDetailHUD.Button.Help.Text = FF9TextTool.ActionAbilityHelpDescription (num);
		}
	}

	public void DisplayInfomation () {
		this.DisplayParty ();
	}

	private void DisplayParty () {
		int i = 0;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			int num = 0;
			while (1 << num != (int) next.btl_id) {
				num++;
			}
			if (next.bi.player > 0) {
				BattleHUD.PlayerDetailHUD playerDetailHUD = this.playerDetailPanelList[i];
				BattleHUD.InfoVal hp = this.hpInfoVal[i];
				BattleHUD.InfoVal mp = this.mpInfoVal[i];
				playerDetailHUD.PlayerId = num;
				playerDetailHUD.Self.SetActive (true);
				this.DisplayCharacterParameter (playerDetailHUD, next, hp, mp);
				playerDetailHUD.TranceSliderGameObject.SetActive (next.bi.t_gauge > 0);
				i++;
			}
		}
		while (i < this.playerDetailPanelList.Count) {
			this.playerDetailPanelList[i].Self.SetActive (false);
			this.playerDetailPanelList[i].PlayerId = -1;
			i++;
		}
	}

	public void DisplayPartyRealtime () {
		int num = 0;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.bi.player != 0) {
				BattleHUD.PlayerDetailHUD playerHud = this.playerDetailPanelList[num];
				BattleHUD.InfoVal hp = this.hpInfoVal[num];
				BattleHUD.InfoVal mp = this.mpInfoVal[num];
				num++;
				this.DisplayCharacterParameter (playerHud, next, hp, mp);
			}
		}
	}

	private void DisplayTarget () {
		bool flag = false;
		int num = this.enemyCount;
		int num2 = this.playerCount;
		int num3 = 0;
		int num4 = 0;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.bi.player != 0) {
				if (next.bi.target != 0) {
					num4++;
				}
			} else if (next.bi.target != 0) {
				num3++;
			}
		}
		if (num3 != num || num4 != num2) {
			flag = true;
			this.matchBattleIdPlayerList.Clear ();
			this.currentCharacterHp.Clear ();
			this.matchBattleIdEnemyList.Clear ();
			this.currentEnemyDieState.Clear ();
			this.enemyCount = num3;
			this.playerCount = num4;
		}
		int num5 = 0;
		int num6 = 0;
		for (BTL_DATA next2 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next2 != null; next2 = next2.next) {
			int num7 = 0;
			while (1 << num7 != (int) next2.btl_id) {
				num7++;
			}
			if (next2.btl_id != 0 && next2.bi.target != 0) {
				if (next2.bi.player != 0) {
					BattleHUD.ParameterStatus parameterStatus = this.CheckHPState (next2);
					if (num5 >= this.currentCharacterHp.Count) {
						this.currentCharacterHp.Add (parameterStatus);
						this.matchBattleIdPlayerList.Add (num7);
						flag = true;
					} else if (parameterStatus != this.currentCharacterHp[num5]) {
						this.currentCharacterHp[num5] = parameterStatus;
						flag = true;
					}
					num5++;
				} else {
					bool flag2 = Status.checkCurStat (next2, 256u);
					if (num6 >= this.currentEnemyDieState.Count) {
						this.currentEnemyDieState.Add (flag2);
						this.matchBattleIdEnemyList.Add (num7);
						flag = true;
					} else if (flag2 != this.currentEnemyDieState[num6]) {
						this.currentEnemyDieState[num6] = flag2;
						flag = true;
					}
					num6++;
				}
			}
		}
		if (!flag) {
			return;
		}
		foreach (BattleHUD.TargetHUD targetHUD in this.targetHudList) {
			targetHUD.KeyNavigate.startsSelected = false;
			targetHUD.Self.SetActive (false);
		}
		GameObject gameObject = null;
		int num8 = 0;
		int num9 = 4;
		if (this.cursorType == BattleHUD.CursorGroup.Individual) {
			gameObject = ButtonGroupState.ActiveButton;
		}
		for (BTL_DATA next3 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next3 != null; next3 = next3.next) {
			if (next3.btl_id != 0 && next3.bi.target != 0) {
				if (next3.bi.player != 0) {
					BattleHUD.TargetHUD targetHUD2 = this.targetHudList[num8];
					GameObject self = targetHUD2.Self;
					UILabel nameLabel = targetHUD2.NameLabel;
					self.SetActive (true);
					nameLabel.text = btl_util.getPlayerPtr (next3).name;
					if (this.currentCharacterHp[num8] == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY) {
						if (this.cursorType == BattleHUD.CursorGroup.Individual) {
							if (this.targetDead == 0) {
								ButtonGroupState.SetButtonEnable (self, false);
								if (self == gameObject) {
									int firstPlayer = this.GetFirstPlayer ();
									if (firstPlayer != -1) {
										this.currentTargetIndex = firstPlayer;
										gameObject = this.targetHudList[firstPlayer].Self;
									} else {
										global::Debug.LogError ("NO player active !!");
									}
									Singleton<PointerManager>.Instance.RemovePointerFromGameObject (self);
								}
							} else {
								ButtonGroupState.SetButtonEnable (self, true);
							}
						}
						nameLabel.color = FF9TextTool.Red;
					} else if (this.currentCharacterHp[num8] == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL) {
						if (this.cursorType == BattleHUD.CursorGroup.Individual) {
							ButtonGroupState.SetButtonEnable (self, true);
						}
						nameLabel.color = FF9TextTool.Yellow;
					} else {
						if (this.cursorType == BattleHUD.CursorGroup.Individual) {
							ButtonGroupState.SetButtonEnable (self, true);
						}
						nameLabel.color = FF9TextTool.White;
					}
					num8++;
				} else {
					BattleHUD.TargetHUD targetHUD3 = this.targetHudList[num9];
					GameObject self2 = targetHUD3.Self;
					UILabel nameLabel2 = targetHUD3.NameLabel;
					float num10 = 0f;
					self2.SetActive (true);
					nameLabel2.text = nameLabel2.PhrasePreOpcodeSymbol (btl_util.getEnemyPtr (next3).et.name, ref num10);
					if (this.currentEnemyDieState[num9 - 4]) {
						if (this.cursorType == BattleHUD.CursorGroup.Individual) {
							ButtonGroupState.SetButtonEnable (self2, false);
							if (this.targetDead == 0) {
								if (self2 == gameObject) {
									int num11 = this.GetFirstEnemy () + HonoluluBattleMain.EnemyStartIndex;
									if (num11 != -1) {
										if (this.currentCommandIndex == BattleHUD.CommandMenu.Attack && FF9StateSystem.PCPlatform && this.enemyCount > 1) {
											int num12 = (this.currentTargetIndex != num11) ? num11 : (num11 + 1);
											num12 = ((num12 >= this.targetHudList.Count) ? num11 : num12);
											this.ValidateDefaultTarget (ref num12);
											num11 = num12;
										}
										this.currentTargetIndex = num11;
										gameObject = this.targetHudList[num11].Self;
									} else {
										global::Debug.LogError ("NO enemy active !!");
									}
									Singleton<PointerManager>.Instance.RemovePointerFromGameObject (self2);
								}
							} else {
								ButtonGroupState.SetButtonEnable (self2, true);
							}
						}
						nameLabel2.color = FF9TextTool.Gray;
					} else {
						if (this.cursorType == BattleHUD.CursorGroup.Individual) {
							ButtonGroupState.SetButtonEnable (self2, true);
						}
						nameLabel2.color = FF9TextTool.White;
					}
					num9++;
				}
			}
		}
		if ((num != num3 || num2 != num4) && ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton) {
			this.SetTargetDefault ();
			this.modelButtonManager.Reset ();
			this.EnableTargetArea ();
			this.SetTargetHelp ();
			ButtonGroupState.DisableAllGroup (true);
			ButtonGroupState.ActiveGroup = BattleHUD.TargetGroupButton;
		}
		if (gameObject != null && this.cursorType == BattleHUD.CursorGroup.Individual && gameObject.activeSelf) {
			ButtonGroupState.ActiveButton = gameObject;
		} else {
			this.DisplayTargetPointer ();
		}
	}

	private void DisplayCharacterParameter (BattleHUD.PlayerDetailHUD playerHud, BTL_DATA bd, BattleHUD.InfoVal hp, BattleHUD.InfoVal mp) {
		playerHud.NameLabel.text = btl_util.getPlayerPtr (bd).name;
		playerHud.HPLabel.text = hp.disp_val.ToString ();
		playerHud.MPLabel.text = mp.disp_val.ToString ();
		BattleHUD.ParameterStatus parameterStatus = this.CheckHPState (bd);
		if (parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY) {
			playerHud.ATBSlider.value = 0f;
			playerHud.HPLabel.color = FF9TextTool.Red;
			playerHud.NameLabel.color = FF9TextTool.Red;
			playerHud.ATBBlink = false;
			playerHud.TranceBlink = false;
		} else if (parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL) {
			playerHud.ATBSlider.value = (float) bd.cur.at / (float) bd.max.at;
			playerHud.HPLabel.color = FF9TextTool.Yellow;
			playerHud.NameLabel.color = FF9TextTool.Yellow;
		} else {
			playerHud.ATBSlider.value = (float) bd.cur.at / (float) bd.max.at;
			playerHud.HPLabel.color = FF9TextTool.White;
			playerHud.NameLabel.color = FF9TextTool.White;
		}
		BattleHUD.ParameterStatus parameterStatus2 = this.CheckMPState (bd);
		if (parameterStatus2 == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL) {
			playerHud.MPLabel.color = FF9TextTool.Yellow;
		} else {
			playerHud.MPLabel.color = FF9TextTool.White;
		}
		string spriteName = BattleHUD.ATENormal;
		if (btl_stat.CheckStatus (bd, 1052672u)) {
			spriteName = BattleHUD.ATEGray;
		} else if (btl_stat.CheckStatus (bd, 524288u)) {
			spriteName = BattleHUD.ATEOrange;
		}
		playerHud.ATBForegroundSprite.spriteName = spriteName;
		if (bd.bi.t_gauge != 0) {
			playerHud.TranceSlider.value = (float) bd.trance / 256f;
			if (parameterStatus != BattleHUD.ParameterStatus.PARAMSTAT_EMPTY) {
				if (bd.trance == 255 && !playerHud.TranceBlink) {
					playerHud.TranceBlink = true;
					if (!this.currentTrancePlayer.Contains ((int) bd.bi.line_no)) {
						this.currentTrancePlayer.Add ((int) bd.bi.line_no);
						this.currentTranceTrigger = true;
					}
				} else if (bd.trance != 255) {
					playerHud.TranceBlink = false;
					if (this.currentTrancePlayer.Contains ((int) bd.bi.line_no)) {
						this.currentTrancePlayer.Remove ((int) bd.bi.line_no);
						this.currentTranceTrigger = true;
					}
				}
			}
		}
	}

	public void AddPlayerToReady (int playerId) {
		if (!this.unconsciousStateList.Contains (playerId)) {
			this.readyQueue.Add (playerId);
			BattleHUD.PlayerDetailHUD playerDetailHUD = this.playerDetailPanelList.First ((BattleHUD.PlayerDetailHUD hud) => hud.PlayerId == playerId);
			playerDetailHUD.ATBBlink = true;
		}
	}

	public void RemovePlayerFromAction (int btl_id, bool isNeedToClearCommand) {
		int num = 0;
		while (1 << num != btl_id) {
			num++;
		}
		if (this.inputFinishedList.Contains (num) && isNeedToClearCommand) {
			this.inputFinishedList.Remove (num);
		}
		if (this.readyQueue.Contains (num) && isNeedToClearCommand) {
			this.readyQueue.Remove (num);
		}
	}

	private void ManageAbility () {
		this.abilityDetailDict.Clear ();
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			int num = 0;
			while (1 << num != (int) next.btl_id) {
				num++;
			}
			if (next.bi.player != 0) {
				BattleHUD.AbilityPlayerDetail abilityPlayerDetail = new BattleHUD.AbilityPlayerDetail ();
				abilityPlayerDetail.Player = FF9StateSystem.Common.FF9.player[(int) next.bi.slot_no];
				abilityPlayerDetail.HasAp = ff9abil.FF9Abil_HasAp (abilityPlayerDetail.Player);
				this.SetAbilityAp (abilityPlayerDetail);
				this.SetAbilityEquip (abilityPlayerDetail);
				this.SetAbilityTrance (abilityPlayerDetail);
				this.SetAbilityMagic (abilityPlayerDetail);
				this.abilityDetailDict[num] = abilityPlayerDetail;
			}
		}
	}

	private BattleHUD.ParameterStatus CheckHPState (BTL_DATA bd) {
		if (bd.cur.hp == 0) {
			return BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
		}
		if ((float) bd.cur.hp <= (float) bd.max.hp / 6f) {
			return (bd.bi.player == 0) ? BattleHUD.ParameterStatus.PARAMSTAT_NORMAL : BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
		}
		return BattleHUD.ParameterStatus.PARAMSTAT_NORMAL;
	}

	private BattleHUD.ParameterStatus CheckMPState (BTL_DATA bd) {
		if ((float) bd.cur.mp <= (float) bd.max.mp / 6f) {
			return BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
		}
		return BattleHUD.ParameterStatus.PARAMSTAT_NORMAL;
	}

	private void CheckDoubleCast (int battleIndex, BattleHUD.CursorGroup cursorType) {
		if ((this.IsDoubleCast && this.doubleCastCount == 2) || !this.IsDoubleCast) {
			this.doubleCastCount = 0;
			this.SetTarget (battleIndex);
		} else if (this.IsDoubleCast && this.doubleCastCount < 2) {
			this.doubleCastCount += 1;
			this.firstCommand = this.ProcessCommand (battleIndex, cursorType);
			this.subMenuType = BattleHUD.SubMenuType.CommandAbility;
			this.DisplayAbility ();
			this.SetTargetVisibility (false);
			this.SetAbilityPanelVisibility (true, true);
			this.BackButton.SetActive (FF9StateSystem.MobilePlatform);
		}
	}

	private void CheckPlayerState () {
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			int num = 0;
			while (1 << num != (int) next.btl_id) {
				num++;
			}
			if (next.bi.player != 0) {
				if (!this.IsEnableInput (next)) {
					if (!this.unconsciousStateList.Contains (num)) {
						this.unconsciousStateList.Add (num);
					}
				} else if (this.unconsciousStateList.Contains (num)) {
					this.unconsciousStateList.Remove (num);
				}
			}
		}
	}

	public void ActivateTurnForPlayer (int playerId) {
		BattleHUD.PlayerDetailHUD playerDetailHUD = this.playerDetailPanelList.Find ((BattleHUD.PlayerDetailHUD hud) => hud.PlayerId == playerId);
		playerDetailHUD.Component.UIBoxCollider.enabled = false;
		playerDetailHUD.Component.ButtonColor.SetState (UIButtonColor.State.Pressed, false);
		this.DisplayCommand ();
		this.SetCommandVisibility (true, false);
	}

	private void SwitchPlayer (int playerId) {
		this.SetIdle ();
		FF9Sfx.FF9SFX_Play (1044);
		this.currentPlayerId = playerId;
		this.ActivateTurnForPlayer (playerId);
	}

	private void UpdatePlayer () {
		this.blinkAlphaCounter += RealTime.deltaTime * 3f;
		this.blinkAlphaCounter = ((this.blinkAlphaCounter <= 2f) ? this.blinkAlphaCounter : 0f);
		float alpha;
		if (this.blinkAlphaCounter <= 1f) {
			alpha = this.blinkAlphaCounter;
		} else {
			alpha = 2f - this.blinkAlphaCounter;
		}
		if (this.commandEnable) {
			foreach (BattleHUD.PlayerDetailHUD playerDetailHUD in this.playerDetailPanelList) {
				if (playerDetailHUD.PlayerId != -1) {
					BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[playerDetailHUD.PlayerId];
					if ((Status.checkCurStat (btl_DATA, 1024u) || Status.checkCurStat (btl_DATA, 2048u)) && playerDetailHUD.ATBBlink) {
						playerDetailHUD.ATBBlink = false;
					}
					if (this.IsEnableInput (btl_DATA) && !this.isAutoAttack) {
						if (playerDetailHUD.ATBBlink) {
							playerDetailHUD.ATBForegroundWidget.alpha = alpha;
						}
						if (playerDetailHUD.TranceBlink && btl_DATA.bi.t_gauge != 0) {
							playerDetailHUD.TranceForegroundWidget.alpha = alpha;
						}
					} else {
						if (playerDetailHUD.ATBBlink) {
							playerDetailHUD.ATBForegroundWidget.alpha = 1f;
							playerDetailHUD.ATBHighlightSprite.alpha = 0f;
						}
						if (playerDetailHUD.TranceBlink && btl_DATA.bi.t_gauge != 0) {
							playerDetailHUD.TranceForegroundWidget.alpha = 1f;
							playerDetailHUD.TranceHighlightSprite.alpha = 0f;
						}
					}
				}
			}
			this.YMenu_ManagerHpMp ();
			this.CheckPlayerState ();
			this.DisplayPartyRealtime ();
			if (this.TargetPanel.activeSelf) {
				this.DisplayTarget ();
				this.DisplayStatusRealtime ();
			}
			this.ManagerTarget ();
			this.ManagerInfo ();
			if (this.currentPlayerId > -1) {
				BTL_DATA btl_DATA2 = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
				if (ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton && this.isTranceMenu) {
					this.tranceColorCounter = (this.tranceColorCounter + 1) % this.tranceTextColor.Length;
					this.CommandCaptionLabel.color = this.tranceTextColor[this.tranceColorCounter];
				}
				if (!this.IsEnableInput (btl_DATA2)) {
					this.SetIdle ();
					return;
				}
				if (this.TargetPanel.activeSelf) {
					if (!this.ManageTargetCommand ()) {
						return;
					}
				} else if (this.AbilityPanel.activeSelf || this.ItemPanel.activeSelf) {
					if (this.AbilityPanel.activeSelf) {
						this.DisplayAbilityRealTime ();
					}
					if (this.ItemPanel.activeSelf) {
						this.DisplayItemRealTime ();
					}
					if (this.currentCommandId == 31u && (!this.magicSwordCond.IsViviExist || this.magicSwordCond.IsViviDead || this.magicSwordCond.IsSteinerMini)) {
						FF9Sfx.FF9SFX_Play (101);
						this.ResetToReady ();
						return;
					}
					if (!this.isTranceMenu && btl_stat.CheckStatus (btl_DATA2, 16384u)) {
						FF9Sfx.FF9SFX_Play (101);
						this.ResetToReady ();
						return;
					}
				}
			}
			if (this.readyQueue.Count > 0 && this.currentPlayerId == -1) {
				for (int i = this.readyQueue.Count - 1; i >= 0; i--) {
					if (this.unconsciousStateList.Contains (this.readyQueue[i])) {
						BTL_DATA btl_DATA3 = FF9StateSystem.Battle.FF9Battle.btl_data[this.readyQueue[i]];
						this.RemovePlayerFromAction ((int) btl_DATA3.btl_id, btl_stat.CheckStatus (btl_DATA3, 134403u));
					}
				}
				foreach (int num in this.readyQueue) {
					if (!this.inputFinishedList.Contains (num) && !this.unconsciousStateList.Contains (num)) {
						if (this.isAutoAttack) {
							this.SendAutoAttackCommand (num);
							break;
						}
						this.SwitchPlayer (num);
						break;
					}
				}
			}
		}
	}

	private BattleHUD.AbilityStatus CheckAbilityStatus (int subMenuIndex) {
		rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[(int) ((UIntPtr) this.currentCommandId)];
		int num = (int) ff9COMMAND.ability + subMenuIndex;
		int abilId = rdata._FF9BMenu_ComAbil[num];
		return this.GetAbilityState (abilId);
	}

	public void YMenu_ManagerHpMp () {
		BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next;
		int num = 0;
		while (next != null) {
			if (next.bi.player != 0) {
				BattleHUD.InfoVal infoVal = this.hpInfoVal[num];
				BattleHUD.InfoVal infoVal2 = this.mpInfoVal[num];
				for (int i = 0; i < 2; i++) {
					BattleHUD.InfoVal infoVal3 = (i != 0) ? infoVal2 : infoVal;
					if (infoVal3.anim_frm != 0) {
						if (0 <= infoVal3.inc_val) {
							if (infoVal3.disp_val + infoVal3.inc_val >= infoVal3.req_val) {
								infoVal3.disp_val = infoVal3.req_val;
								infoVal3.anim_frm = 0;
							} else {
								infoVal3.disp_val += infoVal3.inc_val;
								infoVal3.anim_frm--;
							}
						} else if (infoVal3.disp_val + infoVal3.inc_val <= infoVal3.req_val) {
							infoVal3.disp_val = infoVal3.req_val;
							infoVal3.anim_frm = 0;
						} else {
							infoVal3.disp_val += infoVal3.inc_val;
							infoVal3.anim_frm--;
						}
					} else {
						int num2 = (int) ((i != 0) ? next.cur.mp : ((short) next.cur.hp));
						int num3 = (int) ((i != 0) ? next.max.mp : ((short) next.max.hp));
						int num4;
						if ((num4 = num2 - infoVal3.disp_val) != 0) {
							int num5 = Mathf.Abs (num4);
							infoVal3.req_val = (int) ((short) num2);
							if (num5 < BattleHUD.YINFO_ANIM_HPMP_MIN) {
								infoVal3.anim_frm = num5;
							} else {
								infoVal3.anim_frm = num5 * BattleHUD.YINFO_ANIM_HPMP_MAX / num3;
								if (BattleHUD.YINFO_ANIM_HPMP_MIN > infoVal3.anim_frm) {
									infoVal3.anim_frm = BattleHUD.YINFO_ANIM_HPMP_MIN;
								}
							}
							if (0 <= num4) {
								infoVal3.inc_val = (num4 + (infoVal3.anim_frm - 1)) / infoVal3.anim_frm;
							} else {
								infoVal3.inc_val = (num4 - (infoVal3.anim_frm - 1)) / infoVal3.anim_frm;
							}
						}
					}
				}
				num++;
			}
			next = next.next;
		}
	}

	private void ManagerInfo () {
		BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next;
		BattleHUD.MagicSwordCondition magicSwordCondition = new BattleHUD.MagicSwordCondition ();
		BattleHUD.MagicSwordCondition magicSwordCondition2 = new BattleHUD.MagicSwordCondition ();
		magicSwordCondition.IsViviExist = this.magicSwordCond.IsViviExist;
		magicSwordCondition.IsViviDead = this.magicSwordCond.IsViviDead;
		magicSwordCondition.IsSteinerMini = this.magicSwordCond.IsSteinerMini;
		while (next != null) {
			if (next.bi.player == 0) {
				break;
			}
			if (next.bi.slot_no == 1) {
				magicSwordCondition2.IsViviExist = true;
				if (next.cur.hp == 0) {
					magicSwordCondition2.IsViviDead = true;
				} else if (btl_stat.CheckStatus (next, 318905611u)) {
					magicSwordCondition2.IsViviDead = true;
				}
			} else if (next.bi.slot_no == 3) {
				if (btl_stat.CheckStatus (next, 268435456u)) {
					magicSwordCondition2.IsSteinerMini = true;
				} else {
					magicSwordCondition2.IsSteinerMini = false;
				}
			}
			next = next.next;
		}
		if (magicSwordCondition != magicSwordCondition2) {
			this.magicSwordCond.IsViviExist = magicSwordCondition2.IsViviExist;
			this.magicSwordCond.IsViviDead = magicSwordCondition2.IsViviDead;
			this.magicSwordCond.IsSteinerMini = magicSwordCondition2.IsSteinerMini;
			if (this.currentPlayerId != -1) {
				this.DisplayCommand ();
			}
		} else if (!this.isTranceMenu && this.currentPlayerId != -1) {
			BTL_DATA btl = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
			if (btl_stat.CheckStatus (btl, 16384u)) {
				this.DisplayCommand ();
			}
		}
	}

	private bool ManageTargetCommand () {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		if (this.currentCommandId == 31u && (!this.magicSwordCond.IsViviExist || this.magicSwordCond.IsViviDead || this.magicSwordCond.IsSteinerMini)) {
			FF9Sfx.FF9SFX_Play (101);
			this.ResetToReady ();
			return false;
		}
		if (!this.isTranceMenu && btl_stat.CheckStatus (btl_DATA, 16384u)) {
			FF9Sfx.FF9SFX_Play (101);
			this.ResetToReady ();
			return false;
		}
		if (this.subMenuType == BattleHUD.SubMenuType.CommandAbility) {
			int num = (int) rdata._FF9FAbil_ComData[(int) ((UIntPtr) this.currentCommandId)].ability;
			int num2 = this.PatchAbility (rdata._FF9BMenu_ComAbil[num + this.currentSubMenuIndex]);
			AA_DATA aa_DATA = FF9StateSystem.Battle.FF9Battle.aa_data[num2];
			int num3 = ff9abil.FF9Abil_GetEnableSA ((int) btl_DATA.bi.slot_no, BattleHUD.AbilSaMpHalf) ? (aa_DATA.MP >> 1) : ((int) aa_DATA.MP);
			if ((int) btl_DATA.cur.mp < num3) {
				FF9Sfx.FF9SFX_Play (101);
				this.DisplayAbility ();
				this.SetTargetVisibility (false);
				this.ClearModelPointer ();
				this.SetAbilityPanelVisibility (true, true);
				return false;
			}
			if ((aa_DATA.Category & 2) != 0 && btl_stat.CheckStatus (btl_DATA, 8u)) {
				FF9Sfx.FF9SFX_Play (101);
				this.DisplayAbility ();
				this.SetTargetVisibility (false);
				this.ClearModelPointer ();
				this.SetAbilityPanelVisibility (true, true);
				return false;
			}
		}
		if (this.subMenuType == BattleHUD.SubMenuType.CommandItem || this.subMenuType == BattleHUD.SubMenuType.CommandThrow) {
			int id = this.itemIdList[this.currentSubMenuIndex];
			if (ff9item.FF9Item_GetCount (id) == 0) {
				FF9Sfx.FF9SFX_Play (101);
				this.DisplayItem (BattleHUD.SubMenuType.CommandThrow == this.subMenuType);
				this.SetTargetVisibility (false);
				this.ClearModelPointer ();
				this.SetItemPanelVisibility (true, true);
				return false;
			}
		}
		return true;
	}

	private void ManagerTarget () {
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.tar_mode >= 2) {
				if (next.tar_mode == 2) {
					next.bi.target = (next.bi.atb = 0);
					next.tar_mode = 0;
				} else if (next.tar_mode == 3) {
					next.bi.target = (next.bi.atb = 1);
					next.tar_mode = 1;
				}
			}
		}
	}

	private void InitHpMp () {
		this.hpInfoVal.Clear ();
		this.mpInfoVal.Clear ();
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			BattleHUD.InfoVal infoVal = new BattleHUD.InfoVal ();
			BattleHUD.InfoVal infoVal2 = new BattleHUD.InfoVal ();
			infoVal.req_val = (infoVal.disp_val = (int) ((short) next.cur.hp));
			infoVal2.req_val = (infoVal2.disp_val = (int) next.cur.mp);
			infoVal.anim_frm = (infoVal2.anim_frm = 0);
			infoVal.inc_val = (infoVal2.inc_val = 0);
			this.hpInfoVal.Add (infoVal);
			this.mpInfoVal.Add (infoVal2);
		}
	}

	private int GetMp (AA_DATA aaData) {
		int num = (int) aaData.MP;
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		int slot_no = (int) FF9StateSystem.Common.FF9.party.member[(int) btl_DATA.bi.line_no].info.slot_no;
		if ((aaData.Type & 4) != 0 && FF9StateSystem.EventState.gEventGlobal[18] != 0) {
			num <<= 2;
		}
		if (ff9abil.FF9Abil_GetEnableSA (slot_no, BattleHUD.AbilSaMpHalf)) {
			num >>= 1;
		}
		return num;
	}

	private BattleHUD.AbilityStatus GetAbilityState (int abilId) {
		BattleHUD.AbilityPlayerDetail abilityPlayerDetail = this.abilityDetailDict[this.currentPlayerId];
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		AA_DATA aa_DATA = FF9StateSystem.Battle.FF9Battle.aa_data[abilId];
		if (abilityPlayerDetail.HasAp && !abilityPlayerDetail.AbilityEquipList.ContainsKey (abilId)) {
			if (!abilityPlayerDetail.AbilityPaList.ContainsKey (abilId)) {
				return BattleHUD.AbilityStatus.ABILSTAT_NONE;
			}
			int num = abilityPlayerDetail.AbilityPaList[abilId];
			int num2 = abilityPlayerDetail.AbilityMaxPaList[abilId];
			if (num < num2) {
				return BattleHUD.AbilityStatus.ABILSTAT_NONE;
			}
		}
		if ((aa_DATA.Category & 2) != 0 && (btl_stat.CheckStatus (btl_DATA, 8u) || FF9StateSystem.Battle.FF9Battle.btl_scene.Info.NoMagical != 0)) {
			return BattleHUD.AbilityStatus.ABILSTAT_DISABLE;
		}
		int mp = this.GetMp (aa_DATA);
		if (mp > (int) btl_DATA.cur.mp) {
			return BattleHUD.AbilityStatus.ABILSTAT_DISABLE;
		}
		return BattleHUD.AbilityStatus.ABILSTAT_ENABLE;
	}

	private void SetAbilityAp (BattleHUD.AbilityPlayerDetail abilityPlayer) {
		PLAYER player = abilityPlayer.Player;
		if (abilityPlayer.HasAp) {
			PA_DATA[] array = ff9abil._FF9Abil_PaData[(int) player.info.menu_type];
			for (int i = 0; i < 192; i++) {
				int num;
				if (0 <= (num = ff9abil.FF9Abil_GetIndex ((int) player.info.slot_no, i))) {
					abilityPlayer.AbilityPaList[i] = (int) player.pa[num];
					abilityPlayer.AbilityMaxPaList[i] = (int) array[num].max_ap;
				}
			}
		}
	}

	private void SetAbilityEquip (BattleHUD.AbilityPlayerDetail abilityPlayer) {
		PLAYER player = abilityPlayer.Player;
		for (int i = 0; i < 5; i++) {
			int num = (int) player.equip[i];
			if (num != 255) {
				FF9ITEM_DATA ff9ITEM_DATA = ff9item._FF9Item_Data[num];
				for (int j = 0; j < 3; j++) {
					int num2 = (int) ff9ITEM_DATA.ability[j];
					if (num2 != 0 && 192 > num2) {
						abilityPlayer.AbilityEquipList[num2] = true;
					}
				}
			}
		}
	}

	private void SetAbilityTrance (BattleHUD.AbilityPlayerDetail abilityPlayer) {
		PLAYER player = abilityPlayer.Player;
		int menu_type = (int) player.info.menu_type;
		if (!ff9abil.FF9Abil_HasAp (player)) {
			return;
		}
		if (rdata._FF9BMenu_MenuTrance[menu_type, 2] != 1 && rdata._FF9BMenu_MenuTrance[menu_type, 2] != 2) {
			return;
		}
		int num = (int) (rdata._FF9BMenu_MenuTrance[menu_type, 2] - 1);
		rdata.FF9COMMAND ff9COMMAND = rdata._FF9BMenu_ComData[(int) rdata._FF9BMenu_MenuNormal[menu_type, num]];
		rdata.FF9COMMAND ff9COMMAND2 = rdata._FF9BMenu_ComData[(int) rdata._FF9BMenu_MenuTrance[menu_type, num]];
		PA_DATA[] array = ff9abil._FF9Abil_PaData[menu_type];
		for (int i = 0; i < (int) ff9COMMAND.count; i++) {
			int num2 = rdata._FF9BMenu_ComAbil[(int) ff9COMMAND.ability + i];
			int num3 = rdata._FF9BMenu_ComAbil[(int) ff9COMMAND2.ability + i];
			if (num2 != num3) {
				abilityPlayer.AbilityPaList[num3] = abilityPlayer.AbilityPaList[num2];
				abilityPlayer.AbilityMaxPaList[num3] = abilityPlayer.AbilityMaxPaList[num2];
				if (abilityPlayer.AbilityEquipList.ContainsKey (num2)) {
					abilityPlayer.AbilityEquipList[num3] = abilityPlayer.AbilityEquipList[num2];
				}
			}
		}
	}

	private void SetAbilityMagic (BattleHUD.AbilityPlayerDetail abilityPlayer) {
		PLAYER player = abilityPlayer.Player;
		rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[31];
		PLAYER player2 = FF9StateSystem.Common.FF9.player[1];
		PA_DATA[] array = ff9abil._FF9Abil_PaData[1];
		int[] array2 = new int[] {
			25,
			26,
			27,
			29,
			30,
			31,
			33,
			34,
			35,
			38,
			45,
			47,
			48
		};
		if (player.info.slot_no != 3) {
			return;
		}
		for (int i = 0; i < (int) ff9COMMAND.count; i++) {
			int key = rdata._FF9FAbil_ComAbil[(int) ff9COMMAND.ability + i];
			int num;
			if (0 <= (num = ff9abil.FF9Abil_GetIndex (1, array2[i]))) {
				abilityPlayer.AbilityPaList[key] = (int) player2.pa[num];
				abilityPlayer.AbilityMaxPaList[key] = (int) array[num].max_ap;
			}
		}
		for (int i = 0; i < 5; i++) {
			int num2 = (int) player2.equip[i];
			if (num2 != 255) {
				FF9ITEM_DATA ff9ITEM_DATA = ff9item._FF9Item_Data[num2];
				for (int j = 0; j < 3; j++) {
					int num3 = (int) ff9ITEM_DATA.ability[j];
					if (num3 != 0 && 192 > num3) {
						for (int k = 0; k < (int) ff9COMMAND.count; k++) {
							if (num3 == array2[k]) {
								int key = rdata._FF9FAbil_ComAbil[(int) ff9COMMAND.ability + k];
								abilityPlayer.AbilityEquipList[key] = true;
							}
						}
					}
				}
			}
		}
	}

	private int currentPlayerIndex {
		get {
			return this.matchBattleIdPlayerList.IndexOf (this.currentPlayerId);
		}
	}

	public GameObject PlayerTargetPanel {
		get {
			return this.TargetPanel.GetChild (0);
		}
	}

	public GameObject EnemyTargetPanel {
		get {
			return this.TargetPanel.GetChild (1);
		}
	}

	public List<int> ReadyQueue {
		get {
			return this.readyQueue;
		}
	}

	public List<int> InputFinishList {
		get {
			return this.inputFinishedList;
		}
	}

	public int CurrentPlayerIndex {
		get {
			return this.currentPlayerId;
		}
	}

	public bool IsDoubleCast {
		get {
			return this.currentCommandId == 23u || this.currentCommandId == 21u;
		}
	}

	public override void Show (UIScene.SceneVoidDelegate afterFinished = null) {
		UIScene.SceneVoidDelegate sceneVoidDelegate = delegate () {
			PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (true, null);
			PersistenSingleton<UIManager>.Instance.SetGameCameraEnable (true);
			PersistenSingleton<UIManager>.Instance.SetMenuControlEnable (true);
			PersistenSingleton<UIManager>.Instance.SetUIPauseEnable (true);
			this.PauseButtonGameObject.SetActive (PersistenSingleton<UIManager>.Instance.IsPauseControlEnable && FF9StateSystem.MobilePlatform);
			this.HelpButtonGameObject.SetActive (PersistenSingleton<UIManager>.Instance.IsPauseControlEnable && FF9StateSystem.MobilePlatform);
			ButtonGroupState.SetScrollButtonToGroup (this.abilityScrollList.ScrollButton, BattleHUD.AbilityGroupButton);
			ButtonGroupState.SetScrollButtonToGroup (this.itemScrollList.ScrollButton, BattleHUD.ItemGroupButton);
			ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (34f, 0f), BattleHUD.AbilityGroupButton);
			ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (34f, 0f), BattleHUD.ItemGroupButton);
			ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (16f, 0f), BattleHUD.TargetGroupButton);
			ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (10f, 0f), BattleHUD.CommandGroupButton);
			ButtonGroupState.SetPointerLimitRectToGroup (this.AbilityPanel.GetComponent<UIWidget> (), this.abilityScrollList.cellHeight, BattleHUD.AbilityGroupButton);
			ButtonGroupState.SetPointerLimitRectToGroup (this.ItemPanel.GetComponent<UIWidget> (), this.itemScrollList.cellHeight, BattleHUD.ItemGroupButton);
		};
		bool flag = afterFinished != null;
		if (flag) {
			sceneVoidDelegate = (UIScene.SceneVoidDelegate) Delegate.Combine (sceneVoidDelegate, afterFinished);
		}
		bool flag2 = !this.isFromPause;
		if (flag2) {
			base.Show (sceneVoidDelegate);
			PersistenSingleton<UIManager>.Instance.Booster.SetBoosterState (PersistenSingleton<UIManager>.Instance.UnityScene);
			FF9StateSystem.Settings.SetMasterSkill ();
			this.AllMenuPanel.SetActive (false);
		} else {
			this.commandEnable = this.beforePauseCommandEnable;
			this.isTryingToRun = false;
			Singleton<HUDMessage>.Instance.Pause (false);
			base.Show (sceneVoidDelegate);
			bool flag3 = this.commandEnable && !this.hidingHud;
			if (flag3) {
				this.abilityScrollList.Invoke ("RepositionList", 0.1f);
				this.itemScrollList.Invoke ("RepositionList", 0.1f);
				this.FF9BMenu_EnableMenu (true);
				ButtonGroupState.ActiveGroup = this.currentButtonGroup;
				this.DisplayTargetPointer ();
			}
		}
		this.isFromPause = false;
		this.oneTime = true;
	}

	public override void Hide (UIScene.SceneVoidDelegate afterFinished = null) {
		base.Hide (afterFinished);
		this.PauseButtonGameObject.SetActive (false);
		this.HelpButtonGameObject.SetActive (false);
		if (!this.isFromPause) {
			this.RemoveCursorMemorize ();
		}
	}

	private void RemoveCursorMemorize () {
		this.commandCursorMemorize.Clear ();
		this.ability1CursorMemorize.Clear ();
		this.ability2CursorMemorize.Clear ();
		this.itemCursorMemorize.Clear ();
		ButtonGroupState.RemoveCursorMemorize (BattleHUD.CommandGroupButton);
		ButtonGroupState.RemoveCursorMemorize (BattleHUD.ItemGroupButton);
		ButtonGroupState.RemoveCursorMemorize (BattleHUD.AbilityGroupButton);
		ButtonGroupState.RemoveCursorMemorize (BattleHUD.TargetGroupButton);
	}

	public override bool OnKeyConfirm (GameObject go) {
		if (base.OnKeyConfirm (go) && !this.hidingHud) {
			if (ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton) {
				FF9Sfx.FF9SFX_Play (103);
				int siblingIndex = go.transform.GetSiblingIndex ();
				this.currentCommandIndex = (BattleHUD.CommandMenu) siblingIndex;
				this.currentCommandId = (uint) this.GetCommandFromCommandIndex (this.currentCommandIndex, this.currentPlayerId);
				this.commandCursorMemorize[this.currentPlayerId] = this.currentCommandIndex;
				this.subMenuType = BattleHUD.SubMenuType.CommandNormal;
				if (this.IsDoubleCast && this.doubleCastCount < 2) {
					this.doubleCastCount += 1;
				}
				switch (this.currentCommandIndex) {
					case BattleHUD.CommandMenu.Attack:
						this.SetCommandVisibility (false, false);
						this.SetTargetVisibility (true);
						break;
					case BattleHUD.CommandMenu.Defend:
						this.targetCursor = 0;
						this.SendCommand (this.ProcessCommand (this.currentPlayerId, BattleHUD.CursorGroup.Individual));
						this.SetIdle ();
						break;
					case BattleHUD.CommandMenu.Ability1:
					case BattleHUD.CommandMenu.Ability2:
						{
							int num = (this.currentCommandIndex != BattleHUD.CommandMenu.Ability2) ? 0 : 1;
							rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[(int) ((UIntPtr) this.currentCommandId)];
							if (ff9COMMAND.type == 0) {
								this.subMenuType = BattleHUD.SubMenuType.CommandNormal;
								this.SetCommandVisibility (false, false);
								this.SetTargetVisibility (true);
							} else if (ff9COMMAND.type == 1) {
								this.subMenuType = BattleHUD.SubMenuType.CommandAbility;
								this.DisplayAbility ();
								this.SetCommandVisibility (false, false);
								this.SetAbilityPanelVisibility (true, false);
							} else if (ff9COMMAND.type == 3) {
								this.subMenuType = BattleHUD.SubMenuType.CommandThrow;
								this.DisplayItem (true);
								this.SetCommandVisibility (false, false);
								this.SetItemPanelVisibility (true, false);
							}
							break;
						}
					case BattleHUD.CommandMenu.Item:
						this.DisplayItem (false);
						this.SetCommandVisibility (false, false);
						this.SetItemPanelVisibility (true, false);
						break;
					case BattleHUD.CommandMenu.Change:
						this.targetCursor = 0;
						this.SendCommand (this.ProcessCommand (this.currentPlayerId, BattleHUD.CursorGroup.Individual));
						this.SetIdle ();
						break;
				}
			} else if (ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton) {
				FF9Sfx.FF9SFX_Play (103);
				if (this.cursorType == BattleHUD.CursorGroup.Individual) {
					int num2 = this.targetHudList.IndexOf (this.targetHudList.Single ((BattleHUD.TargetHUD hud) => hud.Self == go));
					if (num2 < HonoluluBattleMain.EnemyStartIndex) {
						if (num2 < this.matchBattleIdPlayerList.Count) {
							int battleIndex = this.matchBattleIdPlayerList[num2];
							this.CheckDoubleCast (battleIndex, this.cursorType);
						}
					} else if (num2 - HonoluluBattleMain.EnemyStartIndex < this.matchBattleIdEnemyList.Count) {
						int battleIndex = this.matchBattleIdEnemyList[num2 - HonoluluBattleMain.EnemyStartIndex];
						this.CheckDoubleCast (battleIndex, this.cursorType);
					}
				} else if (this.cursorType == BattleHUD.CursorGroup.AllPlayer || this.cursorType == BattleHUD.CursorGroup.AllEnemy || this.cursorType == BattleHUD.CursorGroup.All) {
					this.CheckDoubleCast (-1, this.cursorType);
				}
			} else if (ButtonGroupState.ActiveGroup == BattleHUD.AbilityGroupButton) {
				if (this.CheckAbilityStatus (go.GetComponent<RecycleListItem> ().ItemDataIndex) == BattleHUD.AbilityStatus.ABILSTAT_ENABLE) {
					FF9Sfx.FF9SFX_Play (103);
					this.currentSubMenuIndex = go.GetComponent<RecycleListItem> ().ItemDataIndex;
					if (this.currentCommandIndex == BattleHUD.CommandMenu.Ability1) {
						this.ability1CursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
					} else {
						this.ability2CursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
					}
					this.SetAbilityPanelVisibility (false, false);
					this.SetTargetVisibility (true);
				} else {
					FF9Sfx.FF9SFX_Play (102);
				}
			} else if (ButtonGroupState.ActiveGroup == BattleHUD.ItemGroupButton) {
				int num3 = this.itemIdList[this.currentSubMenuIndex];
				if (num3 != 255) {
					FF9Sfx.FF9SFX_Play (103);
					this.currentSubMenuIndex = go.GetComponent<RecycleListItem> ().ItemDataIndex;
					this.itemCursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
					this.SetItemPanelVisibility (false, false);
					this.SetTargetVisibility (true);
				} else {
					FF9Sfx.FF9SFX_Play (102);
				}
			}
		}
		return true;
	}

	public override bool OnKeyCancel (GameObject go) {
		if (UIManager.Input.GetKey (Control.Special)) {
			return true;
		}
		if (base.OnKeyCancel (go) && !this.hidingHud) {
			if (!(ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton)) {
				if (ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton) {
					FF9Sfx.FF9SFX_Play (101);
					this.SetTargetVisibility (false);
					this.ClearModelPointer ();
					switch (this.currentCommandIndex) {
						case BattleHUD.CommandMenu.Attack:
							this.SetCommandVisibility (true, true);
							break;
						case BattleHUD.CommandMenu.Ability1:
						case BattleHUD.CommandMenu.Ability2:
							if (this.subMenuType == BattleHUD.SubMenuType.CommandAbility) {
								this.SetAbilityPanelVisibility (true, true);
							} else if (this.subMenuType == BattleHUD.SubMenuType.CommandThrow) {
								this.SetItemPanelVisibility (true, true);
							} else {
								this.SetCommandVisibility (true, true);
							}
							break;
						case BattleHUD.CommandMenu.Item:
							this.SetItemPanelVisibility (true, true);
							break;
					}
				} else if (ButtonGroupState.ActiveGroup == BattleHUD.AbilityGroupButton) {
					FF9Sfx.FF9SFX_Play (101);
					if (this.IsDoubleCast && this.doubleCastCount > 0) {
						this.doubleCastCount -= 1;
					}
					if (this.doubleCastCount == 0) {
						this.SetAbilityPanelVisibility (false, false);
						this.SetCommandVisibility (true, true);
					} else {
						this.SetAbilityPanelVisibility (true, false);
					}
				} else if (ButtonGroupState.ActiveGroup == BattleHUD.ItemGroupButton) {
					FF9Sfx.FF9SFX_Play (101);
					this.SetItemPanelVisibility (false, false);
					this.SetCommandVisibility (true, true);
				} else if (ButtonGroupState.ActiveGroup == string.Empty && UIManager.Input.ContainsAndroidQuitKey ()) {
					this.OnKeyQuit ();
				}
			}
		}
		return true;
	}

	public override bool OnKeyMenu (GameObject go) {
		if (base.OnKeyMenu (go) && !this.hidingHud && ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton) {
			if (this.readyQueue.Count > 1) {
				int item = this.readyQueue[0];
				this.readyQueue.RemoveAt (0);
				this.readyQueue.Add (item);
				foreach (int num in this.readyQueue) {
					if (!this.inputFinishedList.Contains (num) && !this.unconsciousStateList.Contains (num) && num != this.currentPlayerId) {
						if (this.readyQueue.IndexOf (num) > 0) {
							this.readyQueue.Remove (num);
							this.readyQueue.Insert (0, num);
						}
						this.SwitchPlayer (num);
						break;
					}
				}
			} else if (this.readyQueue.Count == 1) {
				this.SwitchPlayer (this.readyQueue[0]);
			}
		}
		return true;
	}

	public override bool OnKeyPause (GameObject go) {
		if (base.OnKeyPause (go) && FF9StateSystem.Battle.FF9Battle.btl_seq != 2 && FF9StateSystem.Battle.FF9Battle.btl_seq != 1) {
			base.NextSceneIsModal = true;
			this.isFromPause = true;
			this.beforePauseCommandEnable = this.commandEnable;
			this.currentButtonGroup = ((!this.hidingHud) ? ButtonGroupState.ActiveGroup : this.currentButtonGroup);
			this.FF9BMenu_EnableMenu (false);
			Singleton<HUDMessage>.Instance.Pause (true);
			this.Hide (delegate () {
				PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Pause);
			});
		}
		return base.OnKeyPause (go);
	}

	public override void OnKeyQuit () {
		if (!base.Loading && FF9StateSystem.Battle.FF9Battle.btl_seq != 2 && FF9StateSystem.Battle.FF9Battle.btl_seq != 1) {
			this.beforePauseCommandEnable = this.commandEnable;
			this.currentButtonGroup = ButtonGroupState.ActiveGroup;
			this.FF9BMenu_EnableMenu (false);
			base.ShowQuitUI (this.onResumeFromQuit);
		}
	}

	public override bool OnKeyLeftBumper (GameObject go) {
		if (base.OnKeyLeftBumper (go) && !this.hidingHud && ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton && (this.targetCursor == 3 || this.targetCursor == 5 || this.targetCursor == 4)) {
			FF9Sfx.FF9SFX_Play (103);
			this.isAllTarget = !this.isAllTarget;
			this.allTargetToggle.value = this.isAllTarget;
			this.allTargetButtonComponent.SetState (UIButtonColor.State.Normal, false);
			this.ToggleAllTarget ();
		}
		return true;
	}

	public override bool OnKeyRightBumper (GameObject go) {
		if (base.OnKeyRightBumper (go) && !this.hidingHud && ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton && (this.targetCursor == 3 || this.targetCursor == 5 || this.targetCursor == 4)) {
			FF9Sfx.FF9SFX_Play (103);
			this.isAllTarget = !this.isAllTarget;
			this.allTargetToggle.value = this.isAllTarget;
			this.allTargetButtonComponent.SetState (UIButtonColor.State.Normal, false);
			this.ToggleAllTarget ();
		}
		return true;
	}

	public override bool OnKeyRightTrigger (GameObject go) {
		if (base.OnKeyRightTrigger (go) && !this.hidingHud && !this.AndroidTVOnKeyRightTrigger (go)) {
			this.ProcessAutoBattleInput ();
		}
		return true;
	}

	public override bool OnItemSelect (GameObject go) {
		if (base.OnItemSelect (go)) {
			if (ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton) {
				int siblingIndex = go.transform.GetSiblingIndex ();
				if (siblingIndex != (int) this.currentCommandIndex) {
					this.currentCommandIndex = (BattleHUD.CommandMenu) siblingIndex;
				}
			} else if (ButtonGroupState.ActiveGroup == BattleHUD.AbilityGroupButton || ButtonGroupState.ActiveGroup == BattleHUD.ItemGroupButton) {
				this.currentSubMenuIndex = go.GetComponent<RecycleListItem> ().ItemDataIndex;
			}
			if (ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton) {
				if (go.transform.parent == this.modelButtonManager.transform) {
					if (this.cursorType == BattleHUD.CursorGroup.Individual) {
						int index = go.GetComponent<ModelButton> ().index;
						int num;
						if (index < HonoluluBattleMain.EnemyStartIndex) {
							num = this.matchBattleIdPlayerList.IndexOf (index);
						} else {
							num = this.matchBattleIdEnemyList.IndexOf (index) + 4;
						}
						if (num != -1) {
							BattleHUD.TargetHUD targetHUD = this.targetHudList[num];
							if (targetHUD.ButtonGroup.enabled) {
								ButtonGroupState.ActiveButton = targetHUD.Self;
							}
						}
					}
				} else if (go.transform.parent.parent == this.TargetPanel.transform && this.cursorType == BattleHUD.CursorGroup.Individual) {
					int num2 = go.transform.GetSiblingIndex ();
					if (go.GetParent ().transform.GetSiblingIndex () == 1) {
						num2 += HonoluluBattleMain.EnemyStartIndex;
					}
					if (this.currentTargetIndex != num2) {
						this.currentTargetIndex = num2;
						this.DisplayTargetPointer ();
					}
				}
			}
		}
		return true;
	}

	private void OnAllTargetHover (GameObject go, bool isHover) {
		if (isHover && (this.cursorType == BattleHUD.CursorGroup.AllEnemy || this.cursorType == BattleHUD.CursorGroup.AllPlayer)) {
			if (go == this.allPlayerButton) {
				if (this.cursorType != BattleHUD.CursorGroup.AllPlayer) {
					FF9Sfx.FF9SFX_Play (103);
					this.cursorType = BattleHUD.CursorGroup.AllPlayer;
					this.DisplayTargetPointer ();
				}
			} else if (go == this.allEnemyButton && this.cursorType != BattleHUD.CursorGroup.AllEnemy) {
				FF9Sfx.FF9SFX_Play (103);
				this.cursorType = BattleHUD.CursorGroup.AllEnemy;
				this.DisplayTargetPointer ();
			}
		}
	}

	private void OnTargetNavigate (GameObject go, KeyCode key) {
		if (this.cursorType == BattleHUD.CursorGroup.AllEnemy) {
			if (this.targetCursor == 3 && key == KeyCode.RightArrow) {
				FF9Sfx.FF9SFX_Play (103);
				this.cursorType = BattleHUD.CursorGroup.AllPlayer;
				this.DisplayTargetPointer ();
			}
		} else if (this.cursorType == BattleHUD.CursorGroup.AllPlayer && this.targetCursor == 3 && key == KeyCode.LeftArrow) {
			FF9Sfx.FF9SFX_Play (103);
			this.cursorType = BattleHUD.CursorGroup.AllEnemy;
			this.DisplayTargetPointer ();
		}
	}

	private void OnAllTargetClick (GameObject go) {
		if (this.cursorType == BattleHUD.CursorGroup.All) {
			FF9Sfx.FF9SFX_Play (103);
			this.CheckDoubleCast (-1, this.cursorType);
		} else if (UICamera.currentTouchID == 0 || UICamera.currentTouchID == 1) {
			FF9Sfx.FF9SFX_Play (103);
			if (go == this.allPlayerButton) {
				if (this.cursorType == BattleHUD.CursorGroup.AllPlayer) {
					this.CheckDoubleCast (-1, this.cursorType);
				} else {
					this.OnTargetNavigate (go, KeyCode.RightArrow);
				}
			} else if (go == this.allEnemyButton) {
				if (this.cursorType == BattleHUD.CursorGroup.AllEnemy) {
					this.CheckDoubleCast (-1, this.cursorType);
				} else {
					this.OnTargetNavigate (go, KeyCode.LeftArrow);
				}
			}
		} else if (UICamera.currentTouchID == -1) {
			FF9Sfx.FF9SFX_Play (103);
			if (go == this.allPlayerButton) {
				this.cursorType = BattleHUD.CursorGroup.AllPlayer;
			} else if (go == this.allEnemyButton) {
				this.cursorType = BattleHUD.CursorGroup.AllEnemy;
			}
			this.CheckDoubleCast (-1, this.cursorType);
		}
	}

	private void onPartyDetailClick (GameObject go) {
		if (go.GetParent () == this.PartyDetailPanel.GetChild (0)) {
			int siblingIndex = go.transform.GetSiblingIndex ();
			int playerId = this.playerDetailPanelList[siblingIndex].PlayerId;
			if (this.readyQueue.Contains (playerId) && !this.inputFinishedList.Contains (playerId) && !this.unconsciousStateList.Contains (playerId) && playerId != this.currentPlayerId) {
				this.SwitchPlayer (playerId);
			}
		} else {
			base.onClick (go);
		}
	}

	private void OnRunPress (GameObject go, bool isDown) {
		this.runCounter = 0f;
		this.isTryingToRun = isDown;
	}

	private bool OnAllTargetToggleValidate (bool choice) {
		if (this.isAllTarget != this.allTargetToggle.value) {
			return true;
		}
		this.allTargetButtonComponent.SetState (UIButtonColor.State.Normal, false);
		return false;
	}

	private bool OnAutoToggleValidate (bool choice) {
		if (this.isAutoAttack != this.autoBattleToggle.value) {
			return true;
		}
		this.autoBattleButtonComponent.SetState (UIButtonColor.State.Normal, false);
		return false;
	}

	private void InitialBattle () {
		this.currentCommandIndex = BattleHUD.CommandMenu.Attack;
		this.currentSubMenuIndex = 0;
		this.currentPlayerId = -1;
		this.subMenuType = BattleHUD.SubMenuType.CommandNormal;
		this.runCounter = 0f;
		this.isTryingToRun = false;
		this.unconsciousStateList.Clear ();
		this.readyQueue.Clear ();
		this.inputFinishedList.Clear ();
		this.matchBattleIdPlayerList.Clear ();
		this.matchBattleIdEnemyList.Clear ();
		this.itemIdList.Clear ();
		foreach (BattleHUD.AbilityPlayerDetail abilityPlayerDetail in this.abilityDetailDict.Values) {
			abilityPlayerDetail.Clear ();
		}
		this.currentCharacterHp.Clear ();
		this.currentTrancePlayer.Clear ();
		this.enemyCount = 0;
		this.playerCount = 0;
		foreach (BattleHUD.PlayerDetailHUD playerDetailHUD in this.playerDetailPanelList) {
			playerDetailHUD.ATBBlink = false;
			playerDetailHUD.TranceBlink = false;
		}
		this.AutoBattleHud.SetActive (this.isAutoAttack);
		Singleton<HUDMessage>.Instance.WorldCamera = PersistenSingleton<UIManager>.Instance.BattleCamera;
		this.modelButtonManager.WorldCamera = PersistenSingleton<UIManager>.Instance.BattleCamera;
		this.ManageAbility ();
		this.InitHpMp ();
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			int num = 0;
			while (1 << num != (int) next.btl_id) {
				num++;
			}
			if (next.bi.target != 0) {
				if (next.bi.player != 0) {
					this.matchBattleIdPlayerList.Add (num);
				} else {
					this.matchBattleIdEnemyList.Add (num);
				}
			}
		}
		int num2 = 0;
		foreach (int num3 in this.matchBattleIdPlayerList) {
			if (num2 != num3) {
				global::Debug.LogWarning ("This Battle, player index and id not the same. Please be careful.");
				break;
			}
			num2++;
		}
	}

	public void GoToBattleResult () {
		bool flag = this.oneTime;
		if (flag) {
			this.oneTime = false;
			Application.targetFrameRate = 60;
			this.uiRoot.scalingStyle = UIRoot.Scaling.Constrained;
			this.uiRoot.minimumHeight = Mathf.RoundToInt ((float) Screen.currentResolution.height);
			this.Hide (delegate {
				PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.BattleResult);
			});
		}
	}

	public void GoToGameOver () {
		bool flag = this.oneTime;
		if (flag) {
			this.oneTime = false;
			Application.targetFrameRate = 60;
			this.uiRoot.scalingStyle = UIRoot.Scaling.Constrained;
			this.uiRoot.minimumHeight = Mathf.RoundToInt ((float) Screen.currentResolution.height);
			this.Hide (delegate {
				PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.GameOver);
			});
		}
	}

	private void SendAutoAttackCommand (int playerIndex) {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[playerIndex];
		CMD_DATA cmd_DATA = btl_DATA.cmd[0];
		if (cmd_DATA == null || !btl_cmd.CheckUsingCommand (cmd_DATA)) {
			this.currentPlayerId = playerIndex;
			this.currentCommandIndex = BattleHUD.CommandMenu.Attack;
			BTL_DATA firstEnemyPtr = this.GetFirstEnemyPtr ();
			btl_cmd.SetCommand (btl_DATA.cmd[0], 1u, 176u, firstEnemyPtr.btl_id, 0u);
			this.inputFinishedList.Add (this.currentPlayerId);
			this.currentPlayerId = -1;
		}
	}

	private BattleHUD.CommandDetail ProcessCommand (int target, BattleHUD.CursorGroup cursor) {
		BattleHUD.CommandDetail commandDetail = new BattleHUD.CommandDetail ();
		commandDetail.CommandId = this.currentCommandId;
		commandDetail.SubId = 0u;
		int type = (int) rdata._FF9FAbil_ComData[(int) ((UIntPtr) commandDetail.CommandId)].type;
		if (type == 0) {
			commandDetail.SubId = (uint) rdata._FF9FAbil_ComData[(int) ((UIntPtr) commandDetail.CommandId)].ability;
		}
		if (type == 1) {
			int num = (int) rdata._FF9FAbil_ComData[(int) ((UIntPtr) commandDetail.CommandId)].ability;
			commandDetail.SubId = (uint) this.PatchAbility (rdata._FF9BMenu_ComAbil[num + this.currentSubMenuIndex]);
		} else if (type == 2 || type == 3) {
			int subId = this.itemIdList[this.currentSubMenuIndex];
			commandDetail.SubId = (uint) subId;
		}
		commandDetail.TargetId = 0;
		if (cursor == BattleHUD.CursorGroup.Individual) {
			commandDetail.TargetId = (ushort) (1 << target);
		} else if (cursor == BattleHUD.CursorGroup.AllPlayer) {
			commandDetail.TargetId = 15;
		} else if (cursor == BattleHUD.CursorGroup.AllEnemy) {
			commandDetail.TargetId = 240;
		} else if (cursor == BattleHUD.CursorGroup.All) {
			commandDetail.TargetId = 255;
		}
		commandDetail.TargetType = (uint) this.GetSelectMode (cursor);
		return commandDetail;
	}

	private void SendCommand (BattleHUD.CommandDetail command) {
		CMD_DATA cmd_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId].cmd[0];
		cmd_DATA.regist.sel_mode = 1;
		btl_cmd.SetCommand (cmd_DATA, command.CommandId, command.SubId, command.TargetId, command.TargetType);
		this.SetPartySwapButtonActive (false);
		this.inputFinishedList.Add (this.currentPlayerId);
		BattleHUD.PlayerDetailHUD playerDetailHUD = this.playerDetailPanelList.Find ((BattleHUD.PlayerDetailHUD hud) => hud.PlayerId == this.currentPlayerId);
		if (playerDetailHUD != null) {
			playerDetailHUD.ATBBlink = false;
			playerDetailHUD.TranceBlink = false;
		}
	}

	private void SendDoubleCastCommand (BattleHUD.CommandDetail firstCommand, BattleHUD.CommandDetail secondCommand) {
		CMD_DATA cmd_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId].cmd[3];
		cmd_DATA.regist.sel_mode = 1;
		btl_cmd.SetCommand (cmd_DATA, firstCommand.CommandId, firstCommand.SubId, firstCommand.TargetId, firstCommand.TargetType);
		cmd_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId].cmd[0];
		btl_cmd.SetCommand (cmd_DATA, secondCommand.CommandId, secondCommand.SubId, secondCommand.TargetId, secondCommand.TargetType);
		this.SetPartySwapButtonActive (false);
		this.inputFinishedList.Add (this.currentPlayerId);
		BattleHUD.PlayerDetailHUD playerDetailHUD = this.playerDetailPanelList.Find ((BattleHUD.PlayerDetailHUD hud) => hud.PlayerId == this.currentPlayerId);
		if (playerDetailHUD != null) {
			playerDetailHUD.ATBBlink = false;
			playerDetailHUD.TranceBlink = false;
		}
	}

	private command_tags GetCommandFromCommandIndex (BattleHUD.CommandMenu commandIndex, int playerIndex) {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[playerIndex];
		int menu_type = (int) FF9StateSystem.Common.FF9.party.member[(int) btl_DATA.bi.line_no].info.menu_type;
		switch (commandIndex) {
			case BattleHUD.CommandMenu.Attack:
				return command_tags.CMD_ATTACK;
			case BattleHUD.CommandMenu.Defend:
				return command_tags.CMD_DEFEND;
			case BattleHUD.CommandMenu.Ability1:
				if (Status.checkCurStat (btl_DATA, 16384u)) {
					return (command_tags) rdata._FF9BMenu_MenuTrance[menu_type, 0];
				}
				return rdata._FF9BMenu_MenuNormal[menu_type, 0];
			case BattleHUD.CommandMenu.Ability2:
				if (Status.checkCurStat (btl_DATA, 16384u)) {
					return (command_tags) rdata._FF9BMenu_MenuTrance[menu_type, 1];
				}
				return rdata._FF9BMenu_MenuNormal[menu_type, 1];
			case BattleHUD.CommandMenu.Item:
				return command_tags.CMD_ITEM;
			case BattleHUD.CommandMenu.Change:
				return command_tags.CMD_CHANGE;
			default:
				return command_tags.CMD_NONE;
		}
	}

	private void SetCommandVisibility (bool isVisible, bool forceCursorMemo) {
		GameObject gameObject = this.commandDetailHUD.Attack;
		this.SetPartySwapButtonActive (isVisible);
		this.BackButton.SetActive (!isVisible && FF9StateSystem.MobilePlatform);
		if (isVisible) {
			if (!this.commandDetailHUD.Self.activeSelf) {
				this.commandDetailHUD.Self.SetActive (true);
				ButtonGroupState.RemoveCursorMemorize (BattleHUD.CommandGroupButton);
				if ((this.commandCursorMemorize.ContainsKey (this.currentPlayerId) && FF9StateSystem.Settings.cfg.cursor != 0UL) || forceCursorMemo) {
					gameObject = this.commandDetailHUD.GetGameObjectFromCommand (this.commandCursorMemorize[this.currentPlayerId]);
				}
				if (gameObject.GetComponent<ButtonGroupState> ().enabled) {
					ButtonGroupState.SetCursorMemorize (gameObject, BattleHUD.CommandGroupButton);
				} else {
					ButtonGroupState.SetCursorMemorize (this.commandDetailHUD.Attack, BattleHUD.CommandGroupButton);
				}
			} else {
				if ((this.commandCursorMemorize.ContainsKey (this.currentPlayerId) && FF9StateSystem.Settings.cfg.cursor != 0UL) || forceCursorMemo) {
					gameObject = this.commandDetailHUD.GetGameObjectFromCommand (this.commandCursorMemorize[this.currentPlayerId]);
				}
				if (gameObject.GetComponent<ButtonGroupState> ().enabled) {
					ButtonGroupState.ActiveButton = gameObject;
				} else {
					ButtonGroupState.ActiveButton = this.commandDetailHUD.Attack;
				}
			}
			if (!this.hidingHud) {
				ButtonGroupState.ActiveGroup = BattleHUD.CommandGroupButton;
			} else {
				this.currentButtonGroup = BattleHUD.CommandGroupButton;
			}
		} else {
			this.commandCursorMemorize[this.currentPlayerId] = this.currentCommandIndex;
			this.commandDetailHUD.Self.SetActive (false);
		}
	}

	private void SetItemPanelVisibility (bool isVisible, bool forceCursorMemo) {
		if (isVisible) {
			this.ItemPanel.SetActive (true);
			ButtonGroupState.RemoveCursorMemorize (BattleHUD.ItemGroupButton);
			if ((this.itemCursorMemorize.ContainsKey (this.currentPlayerId) && FF9StateSystem.Settings.cfg.cursor != 0UL) || forceCursorMemo) {
				this.itemScrollList.JumpToIndex (this.itemCursorMemorize[this.currentPlayerId], true);
			} else {
				this.itemScrollList.JumpToIndex (0, false);
			}
			ButtonGroupState.RemoveCursorMemorize (BattleHUD.ItemGroupButton);
			ButtonGroupState.ActiveGroup = BattleHUD.ItemGroupButton;
		} else {
			if (this.currentCommandIndex == BattleHUD.CommandMenu.Item && this.currentSubMenuIndex != -1) {
				this.itemCursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
			}
			this.ItemPanel.SetActive (false);
		}
	}

	private void SetAbilityPanelVisibility (bool isVisible, bool forceCursorMemo) {
		if (isVisible) {
			if (!this.AbilityPanel.activeSelf) {
				this.AbilityPanel.SetActive (true);
				Dictionary<int, int> dictionary = (this.currentCommandIndex != BattleHUD.CommandMenu.Ability1) ? this.ability2CursorMemorize : this.ability1CursorMemorize;
				ButtonGroupState.RemoveCursorMemorize (BattleHUD.AbilityGroupButton);
				if ((dictionary.ContainsKey (this.currentPlayerId) && FF9StateSystem.Settings.cfg.cursor != 0UL) || forceCursorMemo) {
					this.abilityScrollList.JumpToIndex (dictionary[this.currentPlayerId], true);
				} else {
					this.abilityScrollList.JumpToIndex (0, true);
				}
			}
			if (this.IsDoubleCast && this.doubleCastCount == 1) {
				ButtonGroupState.SetPointerNumberToGroup (1, BattleHUD.AbilityGroupButton);
			} else if (this.IsDoubleCast && this.doubleCastCount == 2) {
				ButtonGroupState.SetPointerNumberToGroup (2, BattleHUD.AbilityGroupButton);
			} else {
				ButtonGroupState.SetPointerNumberToGroup (0, BattleHUD.AbilityGroupButton);
			}
			ButtonGroupState.ActiveGroup = BattleHUD.AbilityGroupButton;
			ButtonGroupState.UpdateActiveButton ();
		} else {
			if (this.currentCommandIndex == BattleHUD.CommandMenu.Ability1 && this.currentSubMenuIndex != -1) {
				this.ability1CursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
			} else if (this.currentCommandIndex == BattleHUD.CommandMenu.Ability2 && this.currentSubMenuIndex != -1) {
				this.ability2CursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
			}
			this.AbilityPanel.SetActive (false);
		}
	}

	private void SetTargetVisibility (bool isVisible) {
		if (isVisible) {
			byte targetAvalability = 0;
			byte subMode = 0;
			this.defaultTargetCursor = 0;
			this.defaultTargetDead = 0;
			this.targetDead = 0;
			if (this.currentCommandIndex == BattleHUD.CommandMenu.Ability1 || this.currentCommandIndex == BattleHUD.CommandMenu.Ability2) {
				rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[(int) ((UIntPtr) this.currentCommandId)];
				int num;
				if (ff9COMMAND.type == 1) {
					num = rdata._FF9BMenu_ComAbil[(int) ff9COMMAND.ability + this.currentSubMenuIndex];
				} else {
					num = (int) ff9COMMAND.ability;
				}
				AA_DATA aa_DATA = FF9StateSystem.Battle.FF9Battle.aa_data[num];
				targetAvalability = aa_DATA.Info.cursor;
				this.defaultTargetCursor = aa_DATA.Info.def_cur;
				this.defaultTargetDead = aa_DATA.Info.def_dead;
				this.targetDead = aa_DATA.Info.dead;
				subMode = aa_DATA.Info.sub_win;
			} else if (this.currentCommandIndex != BattleHUD.CommandMenu.Attack) {
				if (this.currentCommandIndex == BattleHUD.CommandMenu.Item) {
					ITEM_DATA item_DATA = ff9item._FF9Item_Info[this.itemIdList[this.currentSubMenuIndex] - 224];
					targetAvalability = item_DATA.info.cursor;
					this.defaultTargetCursor = item_DATA.info.def_cur;
					this.defaultTargetDead = item_DATA.info.dead;
					this.targetDead = item_DATA.info.dead;
					subMode = item_DATA.info.sub_win;
				}
			}
			this.isAllTarget = false;
			this.TargetPanel.SetActive (true);
			this.EnableTargetArea ();
			this.DisplayTarget ();
			this.DisplayStatus (subMode);
			this.SetTargetAvalability (targetAvalability);
			this.SetTargetDefault ();
			this.SetTargetHelp ();
			ButtonGroupState.ActiveGroup = BattleHUD.TargetGroupButton;
			this.allTargetToggle.Set (this.isAllTarget);
			this.DisplayTargetPointer ();
		} else {
			this.DisableTargetArea ();
			this.ClearModelPointer ();
			ButtonGroupState.SetAllTarget (false);
			this.cursorType = BattleHUD.CursorGroup.Individual;
			this.allTargetToggle.value = false;
			ButtonGroupState.DisableAllGroup (true);
			this.AllTargetButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
			this.allPlayerButton.SetActive (false);
			this.StatusContainer.SetActive (false);
			this.hpCaption.SetActive (true);
			this.mpCaption.SetActive (true);
			this.atbCaption.SetActive (true);
			this.TargetPanel.SetActive (false);
		}
	}

	private void SetTargetAvalability (byte cursor) {
		this.targetCursor = cursor;
		if (cursor == 0) {
			this.cursorType = BattleHUD.CursorGroup.Individual;
			foreach (object obj in this.PlayerTargetPanel.transform) {
				Transform transform = (Transform) obj;
				ButtonGroupState.SetButtonEnable (transform.gameObject, true);
			}
			foreach (object obj2 in this.EnemyTargetPanel.transform) {
				Transform transform2 = (Transform) obj2;
				ButtonGroupState.SetButtonEnable (transform2.gameObject, true);
			}
			this.AllTargetButton.SetActive (false);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		} else if (cursor == 2) {
			this.cursorType = BattleHUD.CursorGroup.Individual;
			foreach (object obj3 in this.PlayerTargetPanel.transform) {
				Transform transform3 = (Transform) obj3;
				ButtonGroupState.SetButtonEnable (transform3.gameObject, false);
			}
			foreach (object obj4 in this.EnemyTargetPanel.transform) {
				Transform transform4 = (Transform) obj4;
				ButtonGroupState.SetButtonEnable (transform4.gameObject, true);
			}
			this.AllTargetButton.SetActive (false);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		} else if (cursor == 1) {
			this.cursorType = BattleHUD.CursorGroup.Individual;
			foreach (object obj5 in this.PlayerTargetPanel.transform) {
				Transform transform5 = (Transform) obj5;
				ButtonGroupState.SetButtonEnable (transform5.gameObject, true);
			}
			foreach (object obj6 in this.EnemyTargetPanel.transform) {
				Transform transform6 = (Transform) obj6;
				ButtonGroupState.SetButtonEnable (transform6.gameObject, false);
			}
			this.AllTargetButton.SetActive (false);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		} else if (cursor == 3) {
			foreach (object obj7 in this.PlayerTargetPanel.transform) {
				Transform transform7 = (Transform) obj7;
				ButtonGroupState.SetButtonEnable (transform7.gameObject, true);
			}
			foreach (object obj8 in this.EnemyTargetPanel.transform) {
				Transform transform8 = (Transform) obj8;
				ButtonGroupState.SetButtonEnable (transform8.gameObject, true);
			}
			this.AllTargetButton.SetActive (FF9StateSystem.MobilePlatform);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		} else if (cursor == 5) {
			foreach (object obj9 in this.PlayerTargetPanel.transform) {
				Transform transform9 = (Transform) obj9;
				ButtonGroupState.SetButtonEnable (transform9.gameObject, false);
			}
			foreach (object obj10 in this.EnemyTargetPanel.transform) {
				Transform transform10 = (Transform) obj10;
				ButtonGroupState.SetButtonEnable (transform10.gameObject, true);
			}
			this.AllTargetButton.SetActive (FF9StateSystem.MobilePlatform);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		} else if (cursor == 4) {
			foreach (object obj11 in this.PlayerTargetPanel.transform) {
				Transform transform11 = (Transform) obj11;
				ButtonGroupState.SetButtonEnable (transform11.gameObject, true);
			}
			foreach (object obj12 in this.EnemyTargetPanel.transform) {
				Transform transform12 = (Transform) obj12;
				ButtonGroupState.SetButtonEnable (transform12.gameObject, false);
			}
			this.AllTargetButton.SetActive (FF9StateSystem.MobilePlatform);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		} else if (cursor == 8 || cursor == 11) {
			this.cursorType = BattleHUD.CursorGroup.AllEnemy;
			foreach (object obj13 in this.PlayerTargetPanel.transform) {
				Transform transform13 = (Transform) obj13;
				ButtonGroupState.SetButtonEnable (transform13.gameObject, false);
			}
			foreach (object obj14 in this.EnemyTargetPanel.transform) {
				Transform transform14 = (Transform) obj14;
				ButtonGroupState.SetButtonEnable (transform14.gameObject, true);
			}
			this.AllTargetButton.SetActive (false);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (true);
			this.isAllTarget = true;
		} else if (cursor == 7 || cursor == 10) {
			this.cursorType = BattleHUD.CursorGroup.AllPlayer;
			foreach (object obj15 in this.PlayerTargetPanel.transform) {
				Transform transform15 = (Transform) obj15;
				ButtonGroupState.SetButtonEnable (transform15.gameObject, true);
			}
			foreach (object obj16 in this.EnemyTargetPanel.transform) {
				Transform transform16 = (Transform) obj16;
				ButtonGroupState.SetButtonEnable (transform16.gameObject, false);
			}
			this.AllTargetButton.SetActive (false);
			this.allPlayerButton.SetActive (true);
			this.allEnemyButton.SetActive (false);
			this.isAllTarget = true;
		} else if (cursor == 6 || cursor == 12 || cursor == 9) {
			this.cursorType = BattleHUD.CursorGroup.All;
			foreach (object obj17 in this.PlayerTargetPanel.transform) {
				Transform transform17 = (Transform) obj17;
				ButtonGroupState.SetButtonEnable (transform17.gameObject, true);
			}
			foreach (object obj18 in this.EnemyTargetPanel.transform) {
				Transform transform18 = (Transform) obj18;
				ButtonGroupState.SetButtonEnable (transform18.gameObject, true);
			}
			this.AllTargetButton.SetActive (false);
			this.allPlayerButton.SetActive (true);
			this.allEnemyButton.SetActive (true);
			this.isAllTarget = true;
		} else if (cursor == 13) {
			this.cursorType = BattleHUD.CursorGroup.Individual;
			foreach (object obj19 in this.PlayerTargetPanel.transform) {
				Transform transform19 = (Transform) obj19;
				ButtonGroupState.SetButtonEnable (transform19.gameObject, false);
			}
			foreach (object obj20 in this.EnemyTargetPanel.transform) {
				Transform transform20 = (Transform) obj20;
				ButtonGroupState.SetButtonEnable (transform20.gameObject, false);
			}
			int currentPlayerIndex = this.currentPlayerIndex;
			ButtonGroupState.SetButtonEnable (this.PlayerTargetPanel.GetChild (currentPlayerIndex), true);
			this.AllTargetButton.SetActive (false);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		}
	}

	private void SetTargetDefault () {
		int num = 0;
		int num2 = 4;
		if (this.targetDead == 0) {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				int num3 = 0;
				while (1 << num3 != (int) next.btl_id) {
					num3++;
				}
				if (next.btl_id != 0 && next.bi.target != 0) {
					if (next.bi.player != 0) {
						if (btl_stat.CheckStatus (next, 256u)) {
							ButtonGroupState.SetButtonEnable (this.targetHudList[num].Self, false);
						}
						num++;
					} else {
						if (btl_stat.CheckStatus (next, 256u)) {
							ButtonGroupState.SetButtonEnable (this.targetHudList[num2].Self, false);
						}
						num2++;
					}
				}
			}
		}
		if (this.targetCursor == 0 || this.targetCursor == 1 || this.targetCursor == 2 || this.targetCursor == 3 || this.targetCursor == 4 || this.targetCursor == 5) {
			if (this.defaultTargetCursor == 1) {
				if (this.defaultTargetDead != 0) {
					int dead = (int) this.GetDead (true);
					ButtonGroupState.SetCursorStartSelect (this.targetHudList[dead].Self, BattleHUD.TargetGroupButton);
				} else {
					int currentPlayerIndex = this.currentPlayerIndex;
					ButtonGroupState.SetCursorStartSelect (this.targetHudList[currentPlayerIndex].Self, BattleHUD.TargetGroupButton);
				}
				this.currentTargetIndex = 0;
				ButtonGroupState.RemoveCursorMemorize (BattleHUD.TargetGroupButton);
			} else {
				int num4 = HonoluluBattleMain.EnemyStartIndex;
				if (this.defaultTargetDead != 0) {
					num4 = (int) this.GetDead (false);
					ButtonGroupState.SetCursorStartSelect (this.targetHudList[num4].Self, BattleHUD.TargetGroupButton);
				} else {
					num4 = this.GetFirstEnemy () + HonoluluBattleMain.EnemyStartIndex;
					if (num4 != -1) {
						if (this.currentCommandIndex == BattleHUD.CommandMenu.Attack && FF9StateSystem.PCPlatform) {
							this.ValidateDefaultTarget (ref num4);
						}
						ButtonGroupState.SetCursorStartSelect (this.targetHudList[num4].Self, BattleHUD.TargetGroupButton);
					}
				}
				this.currentTargetIndex = num4;
				ButtonGroupState.RemoveCursorMemorize (BattleHUD.TargetGroupButton);
			}
		} else if (this.targetCursor == 13) {
			int currentPlayerIndex2 = this.currentPlayerIndex;
			ButtonGroupState.SetCursorStartSelect (this.targetHudList[currentPlayerIndex2].Self, BattleHUD.TargetGroupButton);
			this.currentTargetIndex = currentPlayerIndex2;
			ButtonGroupState.RemoveCursorMemorize (BattleHUD.TargetGroupButton);
		}
	}

	private void SetTargetHelp () {
		string str = string.Empty;
		bool flag = true;
		switch (this.targetCursor) {
			case 0:
				str = Localization.Get ("BattleTargetHelpIndividual");
				break;
			case 1:
				str = Localization.Get ("BattleTargetHelpIndividualPC");
				break;
			case 2:
				str = Localization.Get ("BattleTargetHelpIndividualNPC");
				break;
			case 3:
				str = Localization.Get ("BattleTargetHelpMultiS");
				break;
			case 4:
				str = Localization.Get ("BattleTargetHelpMultiPCS");
				break;
			case 5:
				str = Localization.Get ("BattleTargetHelpMultiNPCS");
				break;
			case 6:
				str = Localization.Get ("BattleTargetHelpAll");
				break;
			case 7:
				str = Localization.Get ("BattleTargetHelpAllPC");
				break;
			case 8:
				str = Localization.Get ("BattleTargetHelpAllNPC");
				break;
			case 9:
				str = Localization.Get ("BattleTargetHelpRand");
				break;
			case 10:
				str = Localization.Get ("BattleTargetHelpRandPC");
				break;
			case 11:
				str = Localization.Get ("BattleTargetHelpRandNPC");
				break;
			case 12:
				str = Localization.Get ("BattleTargetHelpWhole");
				break;
			case 13:
				str = Localization.Get ("BattleTargetHelpSelf");
				break;
		}
		switch (this.targetCursor) {
			case 0:
				flag = true;
				break;
			case 1:
				flag = true;
				break;
			case 2:
				flag = true;
				break;
			case 3:
				flag = true;
				break;
			case 4:
				flag = true;
				break;
			case 5:
				flag = true;
				break;
			case 6:
				flag = false;
				break;
			case 7:
				flag = false;
				break;
			case 8:
				flag = false;
				break;
			case 9:
				flag = false;
				break;
			case 10:
				flag = false;
				break;
			case 11:
				flag = false;
				break;
			case 12:
				flag = false;
				break;
			case 13:
				flag = true;
				break;
		}
		if (this.isAllTarget) {
			flag = false;
			switch (this.targetCursor) {
				case 3:
					str = Localization.Get ("BattleTargetHelpMultiM");
					break;
				case 4:
					str = Localization.Get ("BattleTargetHelpMultiPCM");
					break;
				case 5:
					str = Localization.Get ("BattleTargetHelpMultiNPCM");
					break;
			}
		}
		int num = 0;
		int num2 = 4;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.btl_id != 0 && next.bi.target != 0) {
				if (next.bi.player != 0) {
					BattleHUD.TargetHUD targetHUD = this.targetHudList[num];
					string str2 = (!flag) ? string.Empty : btl_util.getPlayerPtr (next).name;
					targetHUD.ButtonGroup.Help.Enable = true;
					targetHUD.ButtonGroup.Help.Text = str + "\n" + str2;
					num++;
				} else {
					BattleHUD.TargetHUD targetHUD2 = this.targetHudList[num2];
					float num3 = 0f;
					string str3 = (!flag) ? string.Empty : Singleton<HelpDialog>.Instance.PhraseLabel.PhrasePreOpcodeSymbol (btl_util.getEnemyPtr (next).et.name, ref num3);
					targetHUD2.ButtonGroup.Help.Enable = true;
					targetHUD2.ButtonGroup.Help.Text = str + "\n" + str3;
					num2++;
				}
			}
		}
	}

	private void SetHelpMessageVisibility (bool active) {
		if (ButtonGroupState.HelpEnabled) {
			bool active2 = active && (this.CommandPanel.activeSelf || this.ItemPanel.activeSelf || this.AbilityPanel.activeSelf || this.TargetPanel.activeSelf);
			Singleton<HelpDialog>.Instance.gameObject.SetActive (active2);
		}
	}

	private void SetHudVisibility (bool active) {
		if (this.hidingHud != active) {
			return;
		}
		this.hidingHud = !active;
		this.AllMenuPanel.SetActive (active);
		this.SetHelpMessageVisibility (active);
		if (!active) {
			this.currentButtonGroup = ButtonGroupState.ActiveGroup;
			ButtonGroupState.DisableAllGroup (false);
			ButtonGroupState.SetPointerVisibilityToGroup (active, this.currentButtonGroup);
		} else {
			if (this.currentButtonGroup == BattleHUD.CommandGroupButton && !this.CommandPanel.activeSelf) {
				this.currentButtonGroup = string.Empty;
			}
			this.isTryingToRun = false;
			ButtonGroupState.ActiveGroup = this.currentButtonGroup;
			this.DisplayTargetPointer ();
		}
	}

	private void ProcessAutoBattleInput () {
		this.isAutoAttack = !this.isAutoAttack;
		this.autoBattleToggle.value = this.isAutoAttack;
		this.AutoBattleHud.SetActive (this.isAutoAttack);
		this.autoBattleButtonComponent.SetState (UIButtonColor.State.Normal, false);
		if (this.isAutoAttack) {
			this.SetIdle ();
			this.SetPartySwapButtonActive (false);
		} else {
			this.SetPartySwapButtonActive (true);
			foreach (BattleHUD.PlayerDetailHUD playerDetailHUD in this.playerDetailPanelList) {
				if (this.readyQueue.Contains (playerDetailHUD.PlayerId)) {
					if (this.inputFinishedList.Contains (playerDetailHUD.PlayerId)) {
						playerDetailHUD.ATBBlink = false;
					} else {
						playerDetailHUD.ATBBlink = true;
					}
				} else {
					playerDetailHUD.ATBBlink = false;
				}
			}
		}
	}

	public bool FF9BMenu_IsEnable () {
		return this.commandEnable;
	}

	public bool FF9BMenu_IsEnableAtb () {
		if (!this.commandEnable) {
			return false;
		}
		if (FF9StateSystem.Settings.cfg.atb != 1UL) {
			return true;
		}
		if (this.hidingHud) {
			return this.currentPlayerId == -1 || this.currentButtonGroup == BattleHUD.CommandGroupButton || this.currentButtonGroup == string.Empty;
		}
		return this.currentPlayerId == -1 || ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton || ButtonGroupState.ActiveGroup == string.Empty;
	}

	public void FF9BMenu_EnableMenu (bool active) {
		if (PersistenSingleton<UIManager>.Instance.QuitScene.isShowQuitUI) {
			return;
		}
		if (PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.BattleHUD) {
			this.commandEnable = active;
			this.AllMenuPanel.SetActive (active);
			this.HideHudHitAreaGameObject.SetActive (active);
			if (!active) {
				ButtonGroupState.DisableAllGroup (true);
			} else if ((!this.isFromPause && ButtonGroupState.ActiveGroup == string.Empty) || this.isNeedToInit) {
				this.isNeedToInit = false;
				this.InitialBattle ();
				this.DisplayParty ();
				this.SetIdle ();
			}
		} else {
			this.beforePauseCommandEnable = active;
			this.isNeedToInit = active;
		}
	}

	public void FF9BMenu_Enable (bool enable) { }

	private int PatchAbility (int id) {
		if (BattleHUD.AbilCarbuncle == id) {
			BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
			switch (FF9StateSystem.Common.FF9.party.member[(int) btl_DATA.bi.line_no].equip[4]) {
				case 227:
					id += 3;
					break;
				case 228:
					id++;
					break;
				case 229:
					id += 2;
					break;
			}
		} else if (BattleHUD.AbilFenril == id) {
			BTL_DATA btl_DATA2 = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
			byte b = FF9StateSystem.Common.FF9.party.member[(int) btl_DATA2.bi.line_no].equip[4];
			id += ((b != 222) ? 0 : 1);
		}
		return id;
	}

	private ushort GetDead (bool player) {
		ushort num = 0;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.bi.target != 0 && (next.stat.cur & 256u) != 0u) {
				if (player && next.bi.player != 0) {
					return num;
				}
				if (!player && next.bi.player == 0) {
					return num;
				}
				num += 1;
			}
		}
		return (ushort) this.currentPlayerIndex;
	}

	private int GetFirstPlayer () {
		int num = -1;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.bi.player != 0) {
				num++;
			}
			if (next.bi.player != 0 && next.cur.hp != 0) {
				return num;
			}
		}
		return num;
	}

	private int GetFirstEnemy () {
		int num = -1;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.bi.player == 0) {
				num++;
			}
			if (next.bi.player == 0 && next.cur.hp != 0) {
				return num;
			}
		}
		return num;
	}

	private BTL_DATA GetFirstEnemyPtr () {
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.bi.player == 0 && next.cur.hp != 0) {
				return next;
			}
		}
		return null;
	}

	public void ItemRequest (int id) {
		this.needItemUpdate = true;
	}

	public void ItemUse (int id) {
		if (ff9item.FF9Item_Remove (id, 1) != 0) {
			this.needItemUpdate = true;
		}
	}

	public void ItemUnuse (int id) {
		this.needItemUpdate = true;
	}

	public void ItemRemove (int id) {
		if (ff9item.FF9Item_Remove (id, 1) != 0) {
			this.needItemUpdate = true;
		}
	}

	public void ItemAdd (int id) {
		if (ff9item.FF9Item_Add (id, 1) != 0) {
			this.needItemUpdate = true;
		}
	}

	private bool IsEnableInput (BTL_DATA cur) {
		return cur != null && cur.cur.hp != 0 && !btl_stat.CheckStatus (cur, 1107434755u) && ((int) battle.btl_bonus.member_flag & 1 << (int) cur.bi.line_no) != 0;
	}

	private int GetSelectMode (BattleHUD.CursorGroup cursor) {
		if (this.targetCursor == 9 || this.targetCursor == 10 || this.targetCursor == 11) {
			return 2;
		}
		if (cursor == BattleHUD.CursorGroup.Individual) {
			return 0;
		}
		return 1;
	}

	private void EnableTargetArea () {
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			if (next.bi.target != 0) {
				int num = 0;
				while (1 << num != (int) next.btl_id) {
					num++;
				}
				if (next.bi.player != 0) {
					Transform transform = next.gameObject.transform.GetChildByName ("bone" + next.tar_bone.ToString ("D3")).gameObject.transform;
					this.modelButtonManager.Show (transform, num, false, (float) next.radius, (float) next.height);
				} else {
					Transform transform2 = next.gameObject.transform.GetChildByName ("bone" + next.tar_bone.ToString ("D3")).gameObject.transform;
					this.modelButtonManager.Show (transform2, num, true, (float) next.radius, (float) next.height);
				}
			}
		}
	}

	private void DisableTargetArea () {
		this.modelButtonManager.Reset ();
		this.targetIndexList.Clear ();
	}

	private void ClearModelPointer () {
		foreach (int index in this.targetIndexList) {
			GameObject gameObject = this.modelButtonManager.GetGameObject (index);
			Singleton<PointerManager>.Instance.RemovePointerFromGameObject (gameObject);
		}
		this.targetIndexList.Clear ();
	}

	private void PointToModel (BattleHUD.CursorGroup selectType, int targetIndex = 0) {
		this.ClearModelPointer ();
		bool isBlink = false;
		switch (selectType) {
			case BattleHUD.CursorGroup.Individual:
				if (targetIndex < HonoluluBattleMain.EnemyStartIndex) {
					if (targetIndex < this.matchBattleIdPlayerList.Count) {
						int item = this.matchBattleIdPlayerList[targetIndex];
						this.targetIndexList.Add (item);
					}
				} else if (targetIndex - HonoluluBattleMain.EnemyStartIndex < this.matchBattleIdEnemyList.Count) {
					int item = this.matchBattleIdEnemyList[targetIndex - HonoluluBattleMain.EnemyStartIndex];
					this.targetIndexList.Add (item);
				}
				isBlink = false;
				break;
			case BattleHUD.CursorGroup.AllPlayer:
				this.targetIndexList = this.modelButtonManager.GetAllPlayerIndex ();
				isBlink = true;
				break;
			case BattleHUD.CursorGroup.AllEnemy:
				this.targetIndexList = this.modelButtonManager.GetAllEnemyIndex ();
				isBlink = true;
				break;
			case BattleHUD.CursorGroup.All:
				this.targetIndexList = this.modelButtonManager.GetAllIndex ();
				isBlink = true;
				break;
		}
		foreach (int index in this.targetIndexList) {
			GameObject gameObject = this.modelButtonManager.GetGameObject (index);
			Singleton<PointerManager>.Instance.PointerDepth = 0;
			Singleton<PointerManager>.Instance.AttachPointerToGameObject (gameObject, true);
			Singleton<PointerManager>.Instance.SetPointerBlinkAt (gameObject, isBlink);
			Singleton<PointerManager>.Instance.SetPointerLimitRectBehavior (gameObject, PointerManager.LimitRectBehavior.Hide);
			Singleton<PointerManager>.Instance.PointerDepth = 5;
		}
	}

	private void ToggleAllTarget () {
		if (this.cursorType == BattleHUD.CursorGroup.AllEnemy || this.cursorType == BattleHUD.CursorGroup.AllPlayer) {
			if (ButtonGroupState.ActiveButton) {
				ButtonGroupState.SetButtonAnimation (ButtonGroupState.ActiveButton, true);
			} else {
				foreach (BattleHUD.TargetHUD targetHUD in this.targetHudList) {
					ButtonGroupState.SetButtonAnimation (targetHUD.Self, true);
				}
				ButtonGroupState.ActiveButton = ButtonGroupState.GetCursorStartSelect (BattleHUD.TargetGroupButton);
			}
			this.cursorType = BattleHUD.CursorGroup.Individual;
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		} else {
			ButtonGroupState.SetButtonAnimation (ButtonGroupState.ActiveButton, false);
			Singleton<PointerManager>.Instance.RemovePointerFromGameObject (ButtonGroupState.ActiveButton);
			if (this.currentTargetIndex < HonoluluBattleMain.EnemyStartIndex) {
				this.cursorType = BattleHUD.CursorGroup.AllPlayer;
			} else {
				this.cursorType = BattleHUD.CursorGroup.AllEnemy;
			}
			this.allPlayerButton.SetActive (true);
			this.allEnemyButton.SetActive (true);
		}
		this.SetTargetHelp ();
		this.DisplayTargetPointer ();
	}

	private void DisplayTargetPointer () {
		if (ButtonGroupState.ActiveGroup != BattleHUD.TargetGroupButton) {
			return;
		}
		if (this.cursorType == BattleHUD.CursorGroup.Individual) {
			this.PointToModel (this.cursorType, this.currentTargetIndex);
			ButtonGroupState.SetAllTarget (false);
		} else {
			this.PointToModel (this.cursorType, 0);
			foreach (BattleHUD.TargetHUD targetHUD in this.targetHudList) {
				Singleton<PointerManager>.Instance.SetPointerVisibility (targetHUD.Self, false);
			}
			if (this.cursorType == BattleHUD.CursorGroup.AllPlayer) {
				List<GameObject> list = new List<GameObject> ();
				for (int i = 0; i < this.playerCount; i++) {
					if (this.currentCharacterHp[i] != BattleHUD.ParameterStatus.PARAMSTAT_EMPTY || this.targetDead != 0) {
						list.Add (this.targetHudList[i].Self);
					}
				}
				ButtonGroupState.SetMultipleTarget (list, true);
			} else if (this.cursorType == BattleHUD.CursorGroup.AllEnemy) {
				List<GameObject> list2 = new List<GameObject> ();
				for (int j = 0; j < this.enemyCount; j++) {
					if (!this.currentEnemyDieState[j] || this.targetDead != 0) {
						list2.Add (this.targetHudList[j + HonoluluBattleMain.EnemyStartIndex].Self);
					}
				}
				ButtonGroupState.SetMultipleTarget (list2, true);
			} else {
				ButtonGroupState.SetAllTarget (true);
			}
		}
	}

	public void SetIdle () {
		this.SetCommandVisibility (false, false);
		this.SetTargetVisibility (false);
		this.SetItemPanelVisibility (false, false);
		this.SetAbilityPanelVisibility (false, false);
		this.BackButton.SetActive (false);
		this.currentSilenceStatus = false;
		this.currentMpValue = -1;
		this.currentCommandIndex = BattleHUD.CommandMenu.Attack;
		this.currentSubMenuIndex = -1;
		this.currentPlayerId = -1;
		this.currentTranceTrigger = false;
		ButtonGroupState.DisableAllGroup (true);
		foreach (BattleHUD.PlayerDetailHUD playerDetailHUD in this.playerDetailPanelList) {
			playerDetailHUD.Component.ButtonColor.SetState (UIButtonColor.State.Normal, false);
		}
	}

	public void ResetToReady () {
		this.SetItemPanelVisibility (false, false);
		this.SetAbilityPanelVisibility (false, false);
		this.SetTargetVisibility (false);
		this.ClearModelPointer ();
		this.DisplayCommand ();
		this.SetCommandVisibility (true, false);
	}

	public void SetPartySwapButtonActive (bool isActive) {
		foreach (BattleHUD.PlayerDetailHUD playerDetailHUD in this.playerDetailPanelList) {
			if (this.currentPlayerId == playerDetailHUD.PlayerId) {
				playerDetailHUD.Component.UIBoxCollider.enabled = false;
				playerDetailHUD.Component.ButtonColor.disabledColor = playerDetailHUD.Component.ButtonColor.pressed;
			} else {
				playerDetailHUD.Component.UIBoxCollider.enabled = isActive;
				playerDetailHUD.Component.ButtonColor.disabledColor = playerDetailHUD.Component.ButtonColor.defaultColor;
			}
		}
	}

	private void Update () {
		BattleHUD.Read ();
		bool isShowQuitUI = PersistenSingleton<UIManager>.Instance.QuitScene.isShowQuitUI;
		bool flag = !isShowQuitUI;
		bool flag2 = flag;
		if (flag2) {
			bool flag3 = PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.BattleHUD;
			bool flag4 = flag3;
			bool flag5 = flag4;
			if (flag5) {
				this.UpdatePlayer ();
				this.UpdateMessage ();
				bool flag6 = this.commandEnable;
				bool flag7 = flag6;
				bool flag8 = flag7;
				if (flag8) {
					bool flag9 = (UIManager.Input.GetKey (Control.LeftBumper) && UIManager.Input.GetKey (Control.RightBumper)) || this.isTryingToRun;
					bool flag10 = flag9;
					bool flag11 = flag10;
					if (flag11) {
						this.runCounter += RealTime.deltaTime;
						FF9StateSystem.Battle.FF9Battle.btl_escape_key = 1;
						bool flag12 = this.runCounter > 1f;
						bool flag13 = flag12;
						bool flag14 = flag13;
						if (flag14) {
							this.runCounter = 0f;
							btl_sys.CheckEscape (true);
						} else {
							btl_sys.CheckEscape (false);
						}
					} else {
						this.runCounter = 0f;
						FF9StateSystem.Battle.FF9Battle.btl_escape_key = 0;
					}
					bool key = UIManager.Input.GetKey (Control.Special);
					bool flag15 = key;
					bool flag16 = flag15;
					if (flag16) {
						this.abilityScrollList.Invoke ("RepositionList", 0.1f);
						this.itemScrollList.Invoke ("RepositionList", 0.1f);
						this.SetHudVisibility (false);
					} else {
						this.SetHudVisibility (true);
					}
					this.UpdateAndroidTV ();
					this.SetScaleOfUI ();
				}
			}
		}
	}

	public void ForceClearReadyQueue () {
		for (int i = this.readyQueue.Count - 1; i >= 0; i--) {
			BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.readyQueue[i]];
			if (this.inputFinishedList.Contains (this.readyQueue[i])) {
				this.inputFinishedList.Remove (this.readyQueue[i]);
			}
			this.readyQueue.RemoveAt (i);
		}
	}

	public void VerifyTarget (int modelIndex) {
		if (!this.hidingHud && this.commandEnable && this.cursorType == BattleHUD.CursorGroup.Individual) {
			int num;
			if (modelIndex < HonoluluBattleMain.EnemyStartIndex) {
				num = this.matchBattleIdPlayerList.IndexOf (modelIndex);
			} else {
				num = this.matchBattleIdEnemyList.IndexOf (modelIndex) + 4;
			}
			if (num != -1) {
				FF9Sfx.FF9SFX_Play (103);
				if (this.targetHudList[num].ButtonGroup.enabled) {
					this.CheckDoubleCast (modelIndex, BattleHUD.CursorGroup.Individual);
				}
			}
		}
	}

	private void SetTarget (int battleIndex) {
		if (this.IsDoubleCast) {
			this.SendDoubleCastCommand (this.firstCommand, this.ProcessCommand (battleIndex, this.cursorType));
		} else {
			this.SendCommand (this.ProcessCommand (battleIndex, this.cursorType));
		}
		this.SetTargetVisibility (false);
		this.SetIdle ();
	}

	private void ValidateDefaultTarget (ref int firstIndex) {
		for (int i = firstIndex; i < this.targetHudList.Count; i++) {
			BattleHUD.TargetHUD targetHUD = this.targetHudList[i];
			if (targetHUD.Self.activeSelf && targetHUD.NameLabel.color != FF9TextTool.Gray) {
				firstIndex = i;
				break;
			}
		}
	}

	private void Awake () {
		base.FadingComponent = this.ScreenFadeGameObject.GetComponent<HonoFading> ();
		foreach (object obj in this.PartyDetailPanel.GetChild (0).transform) {
			Transform transform = (Transform) obj;
			GameObject gameObject = transform.gameObject;
			UIEventListener uieventListener = UIEventListener.Get (gameObject);
			uieventListener.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener.onClick, new UIEventListener.VoidDelegate (this.onPartyDetailClick));
			BattleHUD.PlayerDetailHUD item = new BattleHUD.PlayerDetailHUD (gameObject);
			this.playerDetailPanelList.Add (item);
		}
		this.hpCaption = this.PartyDetailPanel.GetChild (1).GetChild (2);
		this.mpCaption = this.PartyDetailPanel.GetChild (1).GetChild (3);
		this.atbCaption = this.PartyDetailPanel.GetChild (1).GetChild (4);
		this.autoBattleButtonComponent = this.AutoBattleButton.GetComponent<UIButton> ();
		this.allTargetButtonComponent = this.AllTargetButton.GetComponent<UIButton> ();
		this.autoBattleToggle = this.AutoBattleButton.GetComponent<UIToggle> ();
		this.allTargetToggle = this.AllTargetButton.GetComponent<UIToggle> ();
		this.autoBattleToggle.validator = new UIToggle.Validate (this.OnAutoToggleValidate);
		this.allTargetToggle.validator = new UIToggle.Validate (this.OnAllTargetToggleValidate);
		this.allPlayerButton = this.TargetPanel.GetChild (2).GetChild (0);
		this.allEnemyButton = this.TargetPanel.GetChild (2).GetChild (1);
		bool mobilePlatform = FF9StateSystem.MobilePlatform;
		if (mobilePlatform) {
			this.RunButton.SetActive (true);
			UIEventListener uieventListener2 = UIEventListener.Get (this.RunButton);
			uieventListener2.onPress = (UIEventListener.BoolDelegate) Delegate.Combine (uieventListener2.onPress, new UIEventListener.BoolDelegate (this.OnRunPress));
		} else {
			this.RunButton.SetActive (false);
		}
		this.battleDialogWidget = this.BattleDialogGameObject.GetComponent<UIWidget> ();
		this.battleDialogLabel = this.BattleDialogGameObject.GetChild (1).GetComponent<UILabel> ();
		UIEventListener uieventListener3 = UIEventListener.Get (this.CommandPanel.GetChild (0));
		uieventListener3.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener3.onClick, new UIEventListener.VoidDelegate (this.onClick));
		UIEventListener uieventListener4 = UIEventListener.Get (this.CommandPanel.GetChild (1));
		uieventListener4.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener4.onClick, new UIEventListener.VoidDelegate (this.onClick));
		UIEventListener uieventListener5 = UIEventListener.Get (this.CommandPanel.GetChild (2));
		uieventListener5.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener5.onClick, new UIEventListener.VoidDelegate (this.onClick));
		UIEventListener uieventListener6 = UIEventListener.Get (this.CommandPanel.GetChild (3));
		uieventListener6.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener6.onClick, new UIEventListener.VoidDelegate (this.onClick));
		UIEventListener uieventListener7 = UIEventListener.Get (this.CommandPanel.GetChild (4));
		uieventListener7.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener7.onClick, new UIEventListener.VoidDelegate (this.onClick));
		UIEventListener uieventListener8 = UIEventListener.Get (this.CommandPanel.GetChild (5));
		uieventListener8.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener8.onClick, new UIEventListener.VoidDelegate (this.onClick));
		this.CommandCaptionLabel = this.CommandPanel.GetChild (6).GetChild (2).GetComponent<UILabel> ();
		this.commandDetailHUD = new BattleHUD.CommandHUD (this.CommandPanel);
		foreach (object obj2 in this.PlayerTargetPanel.transform) {
			Transform transform2 = (Transform) obj2;
			GameObject gameObject2 = transform2.gameObject;
			UIEventListener uieventListener9 = UIEventListener.Get (gameObject2);
			uieventListener9.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener9.onClick, new UIEventListener.VoidDelegate (this.onClick));
			UIEventListener uieventListener10 = UIEventListener.Get (gameObject2);
			uieventListener10.onNavigate = (UIEventListener.KeyCodeDelegate) Delegate.Combine (uieventListener10.onNavigate, new UIEventListener.KeyCodeDelegate (this.OnTargetNavigate));
			this.targetHudList.Add (new BattleHUD.TargetHUD (gameObject2));
		}
		foreach (object obj3 in this.EnemyTargetPanel.transform) {
			Transform transform3 = (Transform) obj3;
			GameObject gameObject3 = transform3.gameObject;
			UIEventListener uieventListener11 = UIEventListener.Get (gameObject3);
			uieventListener11.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener11.onClick, new UIEventListener.VoidDelegate (this.onClick));
			UIEventListener uieventListener12 = UIEventListener.Get (gameObject3);
			uieventListener12.onNavigate = (UIEventListener.KeyCodeDelegate) Delegate.Combine (uieventListener12.onNavigate, new UIEventListener.KeyCodeDelegate (this.OnTargetNavigate));
			this.targetHudList.Add (new BattleHUD.TargetHUD (gameObject3));
		}
		UIEventListener uieventListener13 = UIEventListener.Get (this.allPlayerButton);
		uieventListener13.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener13.onClick, new UIEventListener.VoidDelegate (this.OnAllTargetClick));
		UIEventListener uieventListener14 = UIEventListener.Get (this.allEnemyButton);
		uieventListener14.onClick = (UIEventListener.VoidDelegate) Delegate.Combine (uieventListener14.onClick, new UIEventListener.VoidDelegate (this.OnAllTargetClick));
		UIEventListener uieventListener15 = UIEventListener.Get (this.allPlayerButton);
		uieventListener15.onHover = (UIEventListener.BoolDelegate) Delegate.Combine (uieventListener15.onHover, new UIEventListener.BoolDelegate (this.OnAllTargetHover));
		UIEventListener uieventListener16 = UIEventListener.Get (this.allEnemyButton);
		uieventListener16.onHover = (UIEventListener.BoolDelegate) Delegate.Combine (uieventListener16.onHover, new UIEventListener.BoolDelegate (this.OnAllTargetHover));
		this.hpStatusPanel = this.StatusContainer.GetChild (0);
		foreach (object obj4 in this.hpStatusPanel.GetChild (0).transform) {
			Transform transform4 = (Transform) obj4;
			GameObject gameObject4 = transform4.gameObject;
			BattleHUD.NumberSubModeHUD item2 = new BattleHUD.NumberSubModeHUD (gameObject4);
			this.hpStatusHudList.Add (item2);
		}
		this.mpStatusPanel = this.StatusContainer.GetChild (1);
		foreach (object obj5 in this.mpStatusPanel.GetChild (0).transform) {
			Transform transform5 = (Transform) obj5;
			GameObject gameObject5 = transform5.gameObject;
			BattleHUD.NumberSubModeHUD item3 = new BattleHUD.NumberSubModeHUD (gameObject5);
			this.mpStatusHudList.Add (item3);
		}
		this.goodStatusPanel = this.StatusContainer.GetChild (2);
		foreach (object obj6 in this.goodStatusPanel.GetChild (0).transform) {
			Transform transform6 = (Transform) obj6;
			GameObject gameObject6 = transform6.gameObject;
			BattleHUD.StatusSubModeHUD item4 = new BattleHUD.StatusSubModeHUD (gameObject6);
			this.goodStatusHudList.Add (item4);
		}
		this.badStatusPanel = this.StatusContainer.GetChild (3);
		foreach (object obj7 in this.badStatusPanel.GetChild (0).transform) {
			Transform transform7 = (Transform) obj7;
			GameObject gameObject7 = transform7.gameObject;
			BattleHUD.StatusSubModeHUD item5 = new BattleHUD.StatusSubModeHUD (gameObject7);
			this.badStatusHudList.Add (item5);
		}
		this.itemScrollList = this.ItemPanel.GetChild (1).GetComponent<RecycleListPopulator> ();
		this.abilityScrollList = this.AbilityPanel.GetChild (1).GetComponent<RecycleListPopulator> ();
		this.itemTransition = this.TransitionGameObject.GetChild (0).GetComponent<HonoTweenClipping> ();
		this.abilityTransition = this.TransitionGameObject.GetChild (1).GetComponent<HonoTweenClipping> ();
		this.targetTransition = this.TransitionGameObject.GetChild (2).GetComponent<HonoTweenClipping> ();
		this.onResumeFromQuit = delegate () {
			PersistenSingleton<UIManager>.Instance.SetPlayerControlEnable (true, null);
			PersistenSingleton<UIManager>.Instance.SetMenuControlEnable (true);
			PersistenSingleton<UIManager>.Instance.SetUIPauseEnable (true);
			this.commandEnable = this.beforePauseCommandEnable;
			bool flag = this.commandEnable;
			if (flag) {
				this.isFromPause = true;
				this.FF9BMenu_EnableMenu (true);
				this.DisplayTargetPointer ();
				this.isFromPause = false;
			}
		};
		this.uiRoot = this.abilityScrollList.gameObject.transform.root.GetComponent<UIRoot> ();
		this.uiRect = this.uiRoot.GetComponent<UIRect> ();
	}

	private void MoveUIObjectsForScale () {
		BattleHUD.Write ();
		float num = UIManager.UIActualScreenSize.x / UIManager.UIActualScreenSize.y;
		if (num < 1.6f) {
			this.abilityScrollList.gameObject.GetParent ().transform.localPosition = new Vector3 (-UIManager.UIActualScreenSize.x * 0.5f + (float) this.AbilityPanel.GetComponent<UIWidget> ().width * 0.52f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.AbilityPanel.GetComponent<UIWidget> ().height * 0.68f);
			this.abilityScrollList.panel.RebuildAllDrawCalls ();
			this.itemScrollList.gameObject.GetParent ().transform.localPosition = new Vector3 (-UIManager.UIActualScreenSize.x * 0.5f + (float) this.ItemPanel.GetComponent<UIWidget> ().width * 0.52f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.ItemPanel.GetComponent<UIWidget> ().height * 0.68f);
			this.itemScrollList.panel.RebuildAllDrawCalls ();
			this.CommandPanel.gameObject.transform.localPosition = new Vector3 (-UIManager.UIActualScreenSize.x * 0.5f + (float) this.CommandPanel.GetComponent<UIWidget> ().width * 0.52f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.CommandPanel.GetComponent<UIWidget> ().height * 0.85f);
			this.TargetPanel.gameObject.transform.localPosition = new Vector3 (-UIManager.UIActualScreenSize.x * 0.5f + (float) this.TargetPanel.GetComponent<UIWidget> ().width * 0.52f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.TargetPanel.GetComponent<UIWidget> ().height * 0.78f);
			this.hpStatusPanel.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.hpStatusPanel.GetComponent<UIWidget> ().width * 0.55f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.hpStatusPanel.GetComponent<UIWidget> ().height * 0.78f);
			this.mpStatusPanel.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.mpStatusPanel.GetComponent<UIWidget> ().width * 0.55f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.mpStatusPanel.GetComponent<UIWidget> ().height * 0.78f);
			this.goodStatusPanel.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.goodStatusPanel.GetComponent<UIWidget> ().width * 0.55f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.goodStatusPanel.GetComponent<UIWidget> ().height * 0.78f);
			this.badStatusPanel.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.badStatusPanel.GetComponent<UIWidget> ().width * 0.55f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.badStatusPanel.GetComponent<UIWidget> ().height * 0.78f);
			this.PartyDetailPanel.gameObject.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.PartyDetailPanel.GetComponent<UIWidget> ().width * 0.52f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.PartyDetailPanel.GetComponent<UIWidget> ().height * 0.78f);
			this.PartyDetailPanel.GetComponent<UIWidget> ().panel.RebuildAllDrawCalls ();
			this.BattleDialogGameObject.gameObject.transform.localPosition = new Vector3 (0f, UIManager.UIActualScreenSize.y * 0.5f - 100f);
			this.BattleDialogGameObject.gameObject.transform.localScale = new Vector3 (1.1f, 1.1f);
			ButtonGroupState.SetScrollButtonToGroup (this.abilityScrollList.ScrollButton, BattleHUD.AbilityGroupButton);
			ButtonGroupState.SetScrollButtonToGroup (this.itemScrollList.ScrollButton, BattleHUD.ItemGroupButton);
			ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (34f, 0f), BattleHUD.AbilityGroupButton);
			ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (34f, 0f), BattleHUD.ItemGroupButton);
			ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (16f, 0f), BattleHUD.TargetGroupButton);
			ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (10f, 0f), BattleHUD.CommandGroupButton);
			ButtonGroupState.SetPointerLimitRectToGroup (this.AbilityPanel.GetComponent<UIWidget> (), this.abilityScrollList.cellHeight, BattleHUD.AbilityGroupButton);
			ButtonGroupState.SetPointerLimitRectToGroup (this.ItemPanel.GetComponent<UIWidget> (), this.itemScrollList.cellHeight, BattleHUD.ItemGroupButton);
		} else {
			if (num > 1.6f) {
				this.abilityScrollList.gameObject.GetParent ().transform.localPosition = new Vector3 (-UIManager.UIActualScreenSize.x * 0.5f + (float) this.AbilityPanel.GetComponent<UIWidget> ().width, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.AbilityPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.abilityScrollList.panel.RebuildAllDrawCalls ();
				this.itemScrollList.gameObject.GetParent ().transform.localPosition = new Vector3 (-UIManager.UIActualScreenSize.x * 0.5f + (float) this.AbilityPanel.GetComponent<UIWidget> ().width, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.AbilityPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.itemScrollList.panel.RebuildAllDrawCalls ();
				this.CommandPanel.gameObject.transform.localPosition = new Vector3 (-UIManager.UIActualScreenSize.x * 0.5f + (float) this.CommandPanel.GetComponent<UIWidget> ().width, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.CommandPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.TargetPanel.gameObject.transform.localPosition = new Vector3 (-UIManager.UIActualScreenSize.x * 0.5f + (float) this.TargetPanel.GetComponent<UIWidget> ().width * 0.8f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.TargetPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.hpStatusPanel.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.hpStatusPanel.GetComponent<UIWidget> ().width * 1f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.hpStatusPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.mpStatusPanel.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.mpStatusPanel.GetComponent<UIWidget> ().width * 1f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.mpStatusPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.goodStatusPanel.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.goodStatusPanel.GetComponent<UIWidget> ().width * 1f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.goodStatusPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.badStatusPanel.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.badStatusPanel.GetComponent<UIWidget> ().width * 1f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.badStatusPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.PartyDetailPanel.gameObject.transform.localPosition = new Vector3 (UIManager.UIActualScreenSize.x * 0.5f - (float) this.PartyDetailPanel.GetComponent<UIWidget> ().width * 0.85f, -UIManager.UIActualScreenSize.y * 0.5f + (float) this.PartyDetailPanel.GetComponent<UIWidget> ().height / 1.75f);
				this.PartyDetailPanel.GetComponent<UIWidget> ().panel.RebuildAllDrawCalls ();
				this.BattleDialogGameObject.gameObject.transform.localPosition = new Vector3 (0f, UIManager.UIActualScreenSize.y * 0.5f - 100f);
				this.BattleDialogGameObject.gameObject.transform.localScale = new Vector3 (1.1f, 1.1f);
				ButtonGroupState.SetScrollButtonToGroup (this.abilityScrollList.ScrollButton, BattleHUD.AbilityGroupButton);
				ButtonGroupState.SetScrollButtonToGroup (this.itemScrollList.ScrollButton, BattleHUD.ItemGroupButton);
				ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (34f, 0f), BattleHUD.AbilityGroupButton);
				ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (34f, 0f), BattleHUD.ItemGroupButton);
				ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (16f, 0f), BattleHUD.TargetGroupButton);
				ButtonGroupState.SetPointerOffsetToGroup (new Vector2 (10f, 0f), BattleHUD.CommandGroupButton);
				ButtonGroupState.SetPointerLimitRectToGroup (this.AbilityPanel.GetComponent<UIWidget> (), this.abilityScrollList.cellHeight, BattleHUD.AbilityGroupButton);
				ButtonGroupState.SetPointerLimitRectToGroup (this.ItemPanel.GetComponent<UIWidget> (), this.itemScrollList.cellHeight, BattleHUD.ItemGroupButton);
			}
		}
	}

	public void SetScaleOfUI () {
		BattleHUD.Read ();
		this.uiRoot.scalingStyle = UIRoot.Scaling.Flexible;
		this.uiRoot.minimumHeight = Mathf.RoundToInt ((float) Screen.currentResolution.height * BattleHUD.scale);
		base.FadingComponent.ForegroundSprite.GetComponent<UISprite> ().SetRect (0f, 0f, (float) Screen.currentResolution.width * BattleHUD.scale, (float) Screen.currentResolution.height * BattleHUD.scale);
		base.FadingComponent.ForegroundSprite.CalculateBounds ();
		this.uiRect.SetRect (-(float) Screen.currentResolution.width / BattleHUD.scale, -(float) Screen.currentResolution.height / BattleHUD.scale, (float) (checked (Screen.currentResolution.width * 3)), (float) (checked (Screen.currentResolution.height * 3)));
		this.uiRoot.UpdateScale (true);
		this.uiRect.ResetAndUpdateAnchors ();
		if (BattleHUD.scale > 1.1f) {
			this.MoveUIObjectsForScale ();
		}
	}

	private void Start () {
		BattleHUD.Read ();
	}

	public static void Write () {
		if (BattleHUD.countdown >= 1f) {
			BattleHUD.countdown -= Time.deltaTime;
		} else {
			BattleHUD.countdown = 2f;
			BattleHUD.Read ();
			BattleHUD.Write ();
		}
	}

	public static void Read () {
		string pattern = "\\[(.*?)\\]";
		MatchCollection matchCollection = Regex.Matches (File.ReadAllText (Application.dataPath + "\\Config.txt"), pattern);
		string[] array = new string[20];
		int num = 0;
		foreach (object obj in matchCollection) {
			Match match = (Match) obj;
			array[num] = match.Value;
			array[num] = array[num].Replace ("[", "");
			array[num] = array[num].Replace ("]", "");
			num++;
		}
		BattleHUD.scale = float.Parse (array[0]);
		BattleHUD.hD = int.Parse (array[1]);
		BattleHUD.encounterRate = int.Parse (array[2]);
		BattleHUD.battleSwirl = int.Parse (array[3]);
		BattleHUD.battleSpeed = int.Parse (array[4]);
		BattleHUD.Write ();
		Console.WriteLine (string.Concat (new string[] {
			"Scale:",
			BattleHUD.scale.ToString (),
			" BattleSwirl:",
			BattleHUD.hD.ToString (),
			" Encounter Rate:",
			BattleHUD.encounterRate.ToString (),
			" "
		}));
	}

	public const byte BTLMES_LEVEL_FOLLOW_0 = 0;

	public const byte BTLMES_LEVEL_FOLLOW_1 = 1;

	public const byte BTLMES_LEVEL_TITLE = 1;

	public const byte BTLMES_LEVEL_LIBRA = 2;

	public const byte BTLMES_LEVEL_EVENT = 3;

	public const byte LIBRA_MES_NO = 10;

	public const byte PEEPING_MES_NO = 8;

	public const byte BTLMES_ATTRIBUTE_START = 0;

	private float lastFrameRightTriggerAxis;

	private bool lastFramePressOnMenu;

	private static byte[] BattleMessageTimeTick = new byte[] {
		54,
		46,
		48,
		30,
		24,
		18,
		12
	};

	private static byte[] CmdTitleTable = new byte[] {
		0,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		153,
		254,
		154,
		254,
		155,
		254,
		192,
		254,
		254,
		157,
		254,
		158,
		254,
		159,
		254,
		160,
		254,
		193,
		194,
		195,
		196,
		197,
		198,
		73,
		byte.MaxValue,
		187,
		254,
		254,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		192,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		0,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		0,
		0,
		0,
		byte.MaxValue,
		0,
		0,
		byte.MaxValue,
		0,
		0,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		byte.MaxValue,
		254,
		0,
		254
	};

	private byte currentLibraMessageNumber;

	private byte currentLibraMessageCount;

	private BTL_DATA libraBtlData;

	private byte currentPeepingMessageCount;

	private ENEMY peepingEnmData;

	private byte currentMessagePriority;

	private float battleMessageCounter;

	private static int YINFO_ANIM_HPMP_MIN = 4;

	private static int YINFO_ANIM_HPMP_MAX = 16;

	private static int AbilFenril = 66;

	private static int AbilCarbuncle = 68;

	private static int AbilSaMpHalf = 226;

	private static string ATENormal = "battle_bar_atb";

	private static string ATEGray = "battle_bar_slow";

	private static string ATEOrange = "battle_bar_haste";

	private static float DefaultPartyPanelPosY = -350f;

	private static float PartyItemHeight = 82f;

	private List<BattleHUD.PlayerDetailHUD> playerDetailPanelList = new List<BattleHUD.PlayerDetailHUD> ();

	private BattleHUD.CommandHUD commandDetailHUD;

	private Dictionary<int, BattleHUD.AbilityPlayerDetail> abilityDetailDict = new Dictionary<int, BattleHUD.AbilityPlayerDetail> ();

	private BattleHUD.MagicSwordCondition magicSwordCond = new BattleHUD.MagicSwordCondition ();

	private int enemyCount = -1;

	private int playerCount = -1;

	private List<BattleHUD.ParameterStatus> currentCharacterHp = new List<BattleHUD.ParameterStatus> ();

	private List<bool> currentEnemyDieState = new List<bool> ();

	private List<BattleHUD.InfoVal> hpInfoVal = new List<BattleHUD.InfoVal> ();

	private List<BattleHUD.InfoVal> mpInfoVal = new List<BattleHUD.InfoVal> ();

	private RecycleListPopulator itemScrollList;

	private RecycleListPopulator abilityScrollList;

	private List<int> currentTrancePlayer = new List<int> ();

	private bool currentTranceTrigger;

	private bool isTranceMenu;

	private bool needItemUpdate;

	private bool currentSilenceStatus;

	private int currentMpValue = -1;

	private float blinkAlphaCounter;

	private int tranceColorCounter;

	private Color[] tranceTextColor = new Color[] {
		new Color (1f, 0.215686277f, 0.31764707f),
			new Color (1f, 0.349019617f, 0.3529412f),
			new Color (1f, 0.4862745f, 0.392156869f),
			new Color (1f, 0.623529434f, 0.427450985f),
			new Color (1f, 0.75686276f, 0.466666669f),
			new Color (1f, 0.894117653f, 0.5058824f),
			new Color (1f, 0.9647059f, 0.5254902f),
			new Color (1f, 0.894117653f, 0.5058824f),
			new Color (1f, 0.75686276f, 0.466666669f),
			new Color (1f, 0.623529434f, 0.427450985f),
			new Color (1f, 0.4862745f, 0.392156869f),
			new Color (1f, 0.349019617f, 0.3529412f),
			new Color (1f, 0.215686277f, 0.31764707f)
	};

	public ModelButtonManager modelButtonManager;

	private static Dictionary<uint, byte> BadIconDict = new Dictionary<uint, byte> {
		{
			1u,
			154
		},
		{
			4096u,
			145
		},
		{
			2u,
			153
		},
		{
			65536u,
			144
		},
		{
			4u,
			152
		},
		{
			64u,
			148
		},
		{
			16777216u,
			141
		},
		{
			33554432u,
			140
		},
		{
			1024u,
			147
		},
		{
			131072u,
			143
		},
		{
			2048u,
			146
		},
		{
			32u,
			149
		},
		{
			8u,
			151
		},
		{
			16u,
			150
		},
		{
			268435456u,
			142
		},
		{
			1048576u,
			139
		}
	};

	private static Dictionary<uint, byte> GoodIconDict = new Dictionary<uint, byte> {
		{
			8192u,
			131
		},
		{
			262144u,
			138
		},
		{
			536870912u,
			132
		},
		{
			524288u,
			137
		},
		{
			4194304u,
			135
		},
		{
			8388608u,
			134
		},
		{
			2097152u,
			136
		},
		{
			67108864u,
			133
		}
	};

	public static string CommandGroupButton = "Battle.Command";

	public static string TargetGroupButton = "Battle.Target";

	public static string AbilityGroupButton = "Battle.Ability";

	public static string ItemGroupButton = "Battle.Item";

	public GameObject AutoBattleHud;

	public GameObject AutoBattleButton;

	public GameObject AllTargetButton;

	public GameObject RunButton;

	public GameObject BackButton;

	public GameObject PauseButtonGameObject;

	public GameObject HelpButtonGameObject;

	public GameObject HideHudHitAreaGameObject;

	public GameObject AllMenuPanel;

	public GameObject TargetPanel;

	public GameObject AbilityPanel;

	public GameObject ItemPanel;

	public GameObject CommandPanel;

	public GameObject PartyDetailPanel;

	public GameObject BattleDialogGameObject;

	public GameObject StatusContainer;

	public GameObject TransitionGameObject;

	public GameObject ScreenFadeGameObject;

	private UILabel CommandCaptionLabel;

	private UIWidget battleDialogWidget;

	private UILabel battleDialogLabel;

	private UIToggle autoBattleToggle;

	private UIToggle allTargetToggle;

	private UIButton autoBattleButtonComponent;

	private UIButton allTargetButtonComponent;

	private GameObject allPlayerButton;

	private GameObject allEnemyButton;

	private HonoTweenClipping itemTransition;

	private HonoTweenClipping abilityTransition;

	private HonoTweenClipping targetTransition;

	private GameObject hpStatusPanel;

	private GameObject mpStatusPanel;

	private GameObject goodStatusPanel;

	private GameObject badStatusPanel;

	private GameObject hpCaption;

	private GameObject mpCaption;

	private GameObject atbCaption;

	private List<BattleHUD.NumberSubModeHUD> hpStatusHudList = new List<BattleHUD.NumberSubModeHUD> ();

	private List<BattleHUD.NumberSubModeHUD> mpStatusHudList = new List<BattleHUD.NumberSubModeHUD> ();

	private List<BattleHUD.StatusSubModeHUD> goodStatusHudList = new List<BattleHUD.StatusSubModeHUD> ();

	private List<BattleHUD.StatusSubModeHUD> badStatusHudList = new List<BattleHUD.StatusSubModeHUD> ();

	private bool commandEnable;

	private bool beforePauseCommandEnable;

	private bool isFromPause;

	private bool isNeedToInit;

	private BattleHUD.CommandMenu currentCommandIndex;

	private uint currentCommandId;

	private string currentButtonGroup = string.Empty;

	private int currentSubMenuIndex;

	private int currentPlayerId = -1;

	private int currentTargetIndex = -1;

	private List<int> targetIndexList = new List<int> ();

	private BattleHUD.SubMenuType subMenuType;

	private List<int> readyQueue = new List<int> ();

	private List<int> inputFinishedList = new List<int> ();

	private List<int> unconsciousStateList = new List<int> ();

	private float runCounter;

	private bool hidingHud;

	private BattleHUD.CursorGroup cursorType;

	private byte defaultTargetCursor;

	private byte defaultTargetDead;

	private byte targetDead;

	private byte targetCursor;

	private bool isTryingToRun;

	private bool isAutoAttack;

	private bool isAllTarget;

	private byte doubleCastCount;

	private BattleHUD.CommandDetail firstCommand = new BattleHUD.CommandDetail ();

	private Dictionary<int, BattleHUD.CommandMenu> commandCursorMemorize = new Dictionary<int, BattleHUD.CommandMenu> ();

	private Dictionary<int, int> ability1CursorMemorize = new Dictionary<int, int> ();

	private Dictionary<int, int> ability2CursorMemorize = new Dictionary<int, int> ();

	private Dictionary<int, int> itemCursorMemorize = new Dictionary<int, int> ();

	private List<int> matchBattleIdPlayerList = new List<int> ();

	private List<int> matchBattleIdEnemyList = new List<int> ();

	private List<int> itemIdList = new List<int> ();

	private List<BattleHUD.TargetHUD> targetHudList = new List<BattleHUD.TargetHUD> ();

	private Action onResumeFromQuit;

	private bool oneTime = true;

	public UIRoot uiRoot;

	public UIRect uiRect;

	public static float scale;

	public static int battleSwirl;

	public static float countdown;

	public static int encounterRate;

	public static int hD;

	public static int battleSpeed;

	private struct HUDCompanent {
		public HUDCompanent (GameObject go) {
			this.ButtonGroup = go.GetComponent<ButtonGroupState> ();
			this.Label = go.GetComponent<UILabel> ();
			this.UIBoxCollider = go.GetComponent<BoxCollider> ();
			this.ButtonColor = go.GetComponent<UIButtonColor> ();
			this.Button = go.GetComponent<UIButton> ();
		}

		public ButtonGroupState ButtonGroup;

		public UILabel Label;

		public BoxCollider UIBoxCollider;

		public UIButtonColor ButtonColor;

		public UIButton Button;
	}

	private class CommandHUD {
		public CommandHUD (GameObject go) {
			this.Self = go;
			this.Attack = go.GetChild (0);
			this.Defend = go.GetChild (1);
			this.Skill1 = go.GetChild (2);
			this.Skill2 = go.GetChild (3);
			this.Item = go.GetChild (4);
			this.Change = go.GetChild (5);
			this.AttackComponent.UIBoxCollider = this.Attack.GetComponent<BoxCollider> ();
			this.AttackComponent.Label = this.Attack.GetChild (0).GetComponent<UILabel> ();
			this.AttackComponent.Button = this.Attack.GetComponent<UIButton> ();
			this.AttackComponent.ButtonGroup = this.Attack.GetComponent<ButtonGroupState> ();
			this.Skill1Component.UIBoxCollider = this.Skill1.GetComponent<BoxCollider> ();
			this.Skill1Component.Label = this.Skill1.GetChild (0).GetComponent<UILabel> ();
			this.Skill1Component.Button = this.Skill1.GetComponent<UIButton> ();
			this.Skill1Component.ButtonGroup = this.Skill1.GetComponent<ButtonGroupState> ();
			this.Skill2Component.UIBoxCollider = this.Skill2.GetComponent<BoxCollider> ();
			this.Skill2Component.Label = this.Skill2.GetChild (0).GetComponent<UILabel> ();
			this.Skill2Component.Button = this.Skill2.GetComponent<UIButton> ();
			this.Skill2Component.ButtonGroup = this.Skill2.GetComponent<ButtonGroupState> ();
			this.ItemComponent.UIBoxCollider = this.Item.GetComponent<BoxCollider> ();
			this.ItemComponent.Label = this.Item.GetChild (0).GetComponent<UILabel> ();
			this.ItemComponent.Button = this.Item.GetComponent<UIButton> ();
			this.ItemComponent.ButtonGroup = this.Item.GetComponent<ButtonGroupState> ();
			this.DefendComponent.UIBoxCollider = this.Defend.GetComponent<BoxCollider> ();
			this.DefendComponent.Label = this.Defend.GetChild (0).GetComponent<UILabel> ();
			this.DefendComponent.Button = this.Defend.GetComponent<UIButton> ();
			this.DefendComponent.ButtonGroup = this.Defend.GetComponent<ButtonGroupState> ();
			this.ChangeComponent.UIBoxCollider = this.Change.GetComponent<BoxCollider> ();
			this.ChangeComponent.Label = this.Change.GetChild (0).GetComponent<UILabel> ();
			this.ChangeComponent.Button = this.Change.GetComponent<UIButton> ();
			this.ChangeComponent.ButtonGroup = this.Change.GetComponent<ButtonGroupState> ();
		}

		public GameObject GetGameObjectFromCommand (BattleHUD.CommandMenu menu) {
			switch (menu) {
				case BattleHUD.CommandMenu.Attack:
					return this.Attack;
				case BattleHUD.CommandMenu.Defend:
					return this.Defend;
				case BattleHUD.CommandMenu.Ability1:
					return this.Skill1;
				case BattleHUD.CommandMenu.Ability2:
					return this.Skill2;
				case BattleHUD.CommandMenu.Item:
					return this.Item;
				case BattleHUD.CommandMenu.Change:
					return this.Change;
				default:
					return this.Attack;
			}
		}

		public GameObject Self;

		public GameObject Attack;

		public BattleHUD.HUDCompanent AttackComponent;

		public GameObject Skill1;

		public BattleHUD.HUDCompanent Skill1Component;

		public GameObject Skill2;

		public BattleHUD.HUDCompanent Skill2Component;

		public GameObject Item;

		public BattleHUD.HUDCompanent ItemComponent;

		public GameObject Defend;

		public BattleHUD.HUDCompanent DefendComponent;

		public GameObject Change;

		public BattleHUD.HUDCompanent ChangeComponent;
	}

	private class PlayerDetailHUD {
		public PlayerDetailHUD (GameObject go) {
			this.atbBlink = false;
			this.tranceBlink = false;
			this.Self = go;
			this.Component = new BattleHUD.HUDCompanent (this.Self);
			this.NameLabel = go.GetChild (0).GetComponent<UILabel> ();
			this.HPLabel = go.GetChild (1).GetComponent<UILabel> ();
			this.MPLabel = go.GetChild (2).GetComponent<UILabel> ();
			this.ATBSlider = go.GetChild (3).GetComponent<UIProgressBar> ();
			this.ATBForegroundWidget = go.GetChild (3).GetChild (0).GetComponent<UIWidget> ();
			this.ATBForegroundSprite = go.GetChild (3).GetChild (0).GetChild (1).GetComponent<UISprite> ();
			this.ATBHighlightSprite = go.GetChild (3).GetChild (0).GetChild (0).GetComponent<UISprite> ();
			this.TranceSlider = go.GetChild (4).GetComponent<UIProgressBar> ();
			this.TranceSliderGameObject = go.GetChild (4);
			this.TranceForegroundWidget = go.GetChild (4).GetChild (0).GetComponent<UIWidget> ();
			this.TranceHighlightSprite = go.GetChild (4).GetChild (0).GetChild (0).GetComponent<UISprite> ();
		}

		public bool ATBBlink {
			get {
				return this.atbBlink;
			}
			set {
				this.ATBHighlightSprite.alpha = ((!value) ? 0f : 0.6f);
				this.ATBForegroundWidget.alpha = ((!value) ? 1f : 0f);
				this.atbBlink = value;
			}
		}

		public bool TranceBlink {
			get {
				return this.tranceBlink;
			}
			set {
				this.TranceHighlightSprite.alpha = ((!value) ? 0f : 0.6f);
				this.TranceForegroundWidget.alpha = ((!value) ? 1f : 0f);
				this.tranceBlink = value;
			}
		}

		private bool atbBlink;

		private bool tranceBlink;

		public int PlayerId;

		public GameObject Self;

		public BattleHUD.HUDCompanent Component;

		public UILabel NameLabel;

		public UILabel HPLabel;

		public UILabel MPLabel;

		public UISprite ATBHighlightSprite;

		public UIWidget ATBForegroundWidget;

		public UISprite ATBForegroundSprite;

		public UIProgressBar ATBSlider;

		public UISprite TranceHighlightSprite;

		public UIWidget TranceForegroundWidget;

		public UIProgressBar TranceSlider;

		public GameObject TranceSliderGameObject;
	}

	private class NumberSubModeHUD {
		public NumberSubModeHUD (GameObject go) {
			this.Self = go;
			this.Current = go.GetChild (0).GetComponent<UILabel> ();
			this.slash = go.GetChild (1).GetComponent<UILabel> ();
			this.Max = go.GetChild (2).GetComponent<UILabel> ();
		}

		public Color TextColor {
			set {
				this.Current.color = value;
				this.slash.color = value;
				this.Max.color = value;
			}
		}

		public GameObject Self;

		public UILabel Current;

		public UILabel Max;

		private UILabel slash;
	}

	private class StatusSubModeHUD {
		public StatusSubModeHUD (GameObject go) {
			this.Self = go;
			this.StatusesSpriteList = new UISprite[] {
				go.GetChild (0).GetChild (0).GetComponent<UISprite> (),
					go.GetChild (0).GetChild (1).GetComponent<UISprite> (),
					go.GetChild (0).GetChild (2).GetComponent<UISprite> (),
					go.GetChild (0).GetChild (3).GetComponent<UISprite> (),
					go.GetChild (0).GetChild (4).GetComponent<UISprite> (),
					go.GetChild (0).GetChild (5).GetComponent<UISprite> (),
					go.GetChild (0).GetChild (6).GetComponent<UISprite> (),
					go.GetChild (0).GetChild (7).GetComponent<UISprite> ()
			};
		}

		public GameObject Self;

		public UISprite[] StatusesSpriteList;
	}

	private enum AbilityStatus {
		ABILSTAT_NONE,
		ABILSTAT_DISABLE,
		ABILSTAT_ENABLE
	}

	private enum ParameterStatus {
		PARAMSTAT_NORMAL,
		PARAMSTAT_CRITICAL,
		PARAMSTAT_EMPTY
	}

	private enum SubMenuType {
		CommandNormal,
		CommandAbility,
		CommandItem,
		CommandThrow,
		CommandSlide
	}

	private class AbilityPlayerDetail {
		public void Clear () {
			this.AbilityEquipList.Clear ();
			this.AbilityPaList.Clear ();
			this.AbilityMaxPaList.Clear ();
		}

		public PLAYER Player;

		public bool HasAp;

		public Dictionary<int, bool> AbilityEquipList = new Dictionary<int, bool> ();

		public Dictionary<int, int> AbilityPaList = new Dictionary<int, int> ();

		public Dictionary<int, int> AbilityMaxPaList = new Dictionary<int, int> ();
	}

	private class MagicSwordCondition {
		public override bool Equals (object obj) {
			if (obj == null || base.GetType () != obj.GetType ()) {
				return false;
			}
			BattleHUD.MagicSwordCondition other = obj as BattleHUD.MagicSwordCondition;
			return this == other;
		}

		public override int GetHashCode () {
			return base.GetHashCode ();
		}

		public static bool operator == (BattleHUD.MagicSwordCondition self, BattleHUD.MagicSwordCondition other) {
			return self.IsViviExist == other.IsViviExist && self.IsViviDead == other.IsViviDead && self.IsSteinerMini == other.IsSteinerMini;
		}

		public static bool operator != (BattleHUD.MagicSwordCondition self, BattleHUD.MagicSwordCondition other) {
			return !(self == other);
		}

		public bool IsViviExist;

		public bool IsViviDead;

		public bool IsSteinerMini;
	}

	public class InfoVal {
		public int inc_val;

		public int disp_val;

		public int req_val;

		public int anim_frm;

		public byte pad;
	}

	public class BattleItemListData : ListDataTypeBase {
		public int Id;

		public int Count;
	}

	public class BattleAbilityListData : ListDataTypeBase {
		public int Index;
	}

	private struct TargetHUD {
		public TargetHUD (GameObject go) {
			this.Self = go;
			this.ButtonGroup = go.GetComponent<ButtonGroupState> ();
			this.NameLabel = go.GetChild (0).GetComponent<UILabel> ();
			this.KeyNavigate = go.GetComponent<UIKeyNavigation> ();
		}

		public GameObject Self;

		public ButtonGroupState ButtonGroup;

		public UIKeyNavigation KeyNavigate;

		public UILabel NameLabel;
	}

	public enum SubMenu {
		Command,
		Target,
		Ability,
		Item,
		None
	}

	public enum CommandMenu {
		Attack,
		Defend,
		Ability1,
		Ability2,
		Item,
		Change,
		None
	}

	public enum CursorGroup {
		Individual,
		AllPlayer,
		AllEnemy,
		All,
		None
	}

	private class CommandDetail {
		public uint CommandId;

		public uint SubId;

		public ushort TargetId;

		public uint TargetType;
	}
}