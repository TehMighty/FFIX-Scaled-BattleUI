using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using UnityEngine;

// Token: 0x02000368 RID: 872
public class BattleHUD : UIScene {
	// Token: 0x0600153B RID: 5435 RVA: 0x001554C8 File Offset: 0x001536C8
	public bool AndroidTVOnKeyRightTrigger (GameObject go) {
		bool result = false;
		bool flag = base.CheckAndroidTVModule (Control.RightTrigger);
		if (flag) {
			result = true;
		}
		return result;
	}

	// Token: 0x0600153C RID: 5436 RVA: 0x001554EC File Offset: 0x001536EC
	private void UpdateAndroidTV () {
		HonoInputManager instance = PersistenSingleton<HonoInputManager>.Instance;
		bool flag = FF9StateSystem.AndroidTVPlatform && instance.IsControllerConnect && FF9StateSystem.EnableAndroidTVJoystickMode;
		if (flag) {
			float axisRaw = Input.GetAxisRaw (instance.SpecificPlatformRightTriggerKey);
			bool button = Input.GetButton (instance.DefaultJoystickInputKeys[2]);
			bool flag2 = false;
			bool flag3 = axisRaw > 0.19f && button;
			if (flag3) {
				flag2 = true;
			}
			bool flag4 = flag2 && this.lastFrameRightTriggerAxis > 0.19f && this.lastFramePressOnMenu;
			if (flag4) {
				flag2 = false;
			}
			bool flag5 = flag2 && !this.hidingHud;
			if (flag5) {
				this.ProcessAutoBattleInput ();
			}
			this.lastFrameRightTriggerAxis = axisRaw;
			this.lastFramePressOnMenu = button;
		}
	}

	// Token: 0x1700022A RID: 554
	// (get) Token: 0x0600153D RID: 5437 RVA: 0x001555A4 File Offset: 0x001537A4
	public bool BtlWorkLibra {
		get {
			return this.currentLibraMessageNumber > 0;
		}
	}

	// Token: 0x1700022B RID: 555
	// (get) Token: 0x0600153E RID: 5438 RVA: 0x001555C0 File Offset: 0x001537C0
	public bool BtlWorkPeep {
		get {
			return this.currentPeepingMessageCount > 0;
		}
	}

	// Token: 0x0600153F RID: 5439 RVA: 0x001555DC File Offset: 0x001537DC
	private void UpdateMessage () {
		bool flag = this.BattleDialogGameObject.activeSelf && PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.BattleHUD;
		if (flag) {
			this.battleMessageCounter += Time.deltaTime * (float) FF9StateSystem.Settings.FastForwardFactor;
			bool flag2 = this.battleMessageCounter >= (float) BattleHUD.BattleMessageTimeTick[(int) ((IntPtr) (checked ((long) FF9StateSystem.Settings.cfg.btl_msg)))] / 15f;
			if (flag2) {
				this.BattleDialogGameObject.SetActive (false);
				this.currentMessagePriority = 0;
				bool flag3 = this.currentLibraMessageNumber > 0;
				if (flag3) {
					this.DisplayMessageLibra ();
				}
				bool flag4 = this.currentPeepingMessageCount > 0;
				if (flag4) {
					this.DisplayMessagePeeping ();
				}
			}
		}
	}

	// Token: 0x06001540 RID: 5440 RVA: 0x001556A8 File Offset: 0x001538A8
	private void DisplayBattleMessage (string str, bool isRect) {
		this.BattleDialogGameObject.SetActive (false);
		checked {
			if (isRect) {
				this.battleDialogWidget.width = (int) (unchecked (128f * UIManager.ResourceXMultipier));
				this.battleDialogWidget.height = 120;
				this.battleDialogWidget.transform.localPosition = new Vector3 (0f, 445f, 0f);
			} else {
				this.battleDialogWidget.width = (int) (unchecked (240f * UIManager.ResourceXMultipier));
				bool flag = str.Contains ("\n");
				if (flag) {
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
	}

	// Token: 0x06001541 RID: 5441 RVA: 0x001557E4 File Offset: 0x001539E4
	private void DisplayMessageLibra () {
		bool flag = this.libraBtlData == null;
		checked {
			if (!flag) {
				string text = string.Empty;
				bool flag2 = this.currentLibraMessageNumber == 1;
				if (flag2) {
					bool flag3 = this.libraBtlData.bi.player > 0;
					if (flag3) {
						text = btl_util.getPlayerPtr (this.libraBtlData).name;
					} else {
						text = btl_util.getEnemyPtr (this.libraBtlData).et.name;
					}
					text += FF9TextTool.BattleLibraText (10);
					text += this.libraBtlData.level.ToString ();
					this.currentLibraMessageNumber = 2;
				} else {
					bool flag4 = this.currentLibraMessageNumber == 2;
					if (flag4) {
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
					} else {
						bool flag5 = this.currentLibraMessageNumber == 3;
						if (flag5) {
							bool flag6 = this.libraBtlData.bi.player == 0;
							if (flag6) {
								int category = (int) FF9StateSystem.Battle.FF9Battle.enemy[(int) this.libraBtlData.bi.slot_no].et.category;
								int num;
								for (;;) {
									byte b;
									this.currentLibraMessageCount = (b = this.currentLibraMessageCount) + 1;
									bool flag7 = (num = (int) b) >= 8;
									if (flag7) {
										break;
									}
									if ((category & 1 << num) != 0) {
										goto Block_8;
									}
								}
								goto IL_212;
								Block_8:
									text = FF9TextTool.BattleLibraText (num);
								this.SetBattleMessage (text, 2);
								return;
							}
							IL_212:
								this.currentLibraMessageCount = 0;
							this.currentLibraMessageNumber = 4;
						}
					}
				}
				bool flag8 = this.currentLibraMessageNumber == 4;
				if (flag8) {
					int num2 = (int) (this.libraBtlData.def_attr.weak & ~(int) this.libraBtlData.def_attr.invalid);
					for (;;) {
						byte b2;
						this.currentLibraMessageCount = (b2 = this.currentLibraMessageCount) + 1;
						int num3;
						bool flag9 = (num3 = (int) b2) >= 8;
						if (flag9) {
							break;
						}
						if ((num2 & 1 << num3) != 0) {
							goto Block_11;
						}
					}
					this.currentLibraMessageCount = 0;
					this.currentLibraMessageNumber = 5;
					goto IL_2F7;
					Block_11:
						bool flag10 = Localization.GetSymbol () == "JP";
					if (flag10) {
						int num3;
						text = this.BtlGetAttrName (1 << num3);
						text += FF9TextTool.BattleLibraText (14);
					} else {
						int num3;
						text += FF9TextTool.BattleLibraText (14 + num3);
					}
					this.SetBattleMessage (text, 2);
					return;
				}
				IL_2F7:
					bool flag11 = this.currentLibraMessageNumber == 5;
				if (flag11) {
					this.libraBtlData = null;
					this.currentLibraMessageCount = 0;
					this.currentLibraMessageNumber = 0;
				} else {
					this.SetBattleMessage (text, 2);
				}
			}
		}
	}

	// Token: 0x06001542 RID: 5442 RVA: 0x00155B1C File Offset: 0x00153D1C
	private void DisplayMessagePeeping () {
		bool flag = this.peepingEnmData == null;
		checked {
			if (!flag) {
				string text = string.Empty;
				for (;;) {
					byte b;
					this.currentPeepingMessageCount = (b = this.currentPeepingMessageCount) + 1;
					int num;
					bool flag2 = (num = (int) b) >= this.peepingEnmData.steal_item.Length + 1;
					if (flag2) {
						break;
					}
					int num2 = (int) this.peepingEnmData.steal_item[this.peepingEnmData.steal_item.Length - num];
					if (num2 != 255) {
						goto Block_3;
					}
				}
				this.peepingEnmData = null;
				this.currentPeepingMessageCount = 0;
				return;
				Block_3:
					bool flag3 = Localization.GetSymbol () == "JP";
				if (flag3) {
					int num2;
					text = FF9TextTool.ItemName (num2);
					text += FF9TextTool.BattleLibraText (8);
				} else {
					text = FF9TextTool.BattleLibraText (8);
					int num2;
					text += FF9TextTool.ItemName (num2);
				}
				this.SetBattleMessage (text, 2);
			}
		}
	}

	// Token: 0x06001543 RID: 5443 RVA: 0x00155C04 File Offset: 0x00153E04
	public void SetBattleFollowMessage (int pMesNo, params object[] args) {
		checked {
			string text = FF9TextTool.BattleFollowText (pMesNo + 7);
			bool flag = string.IsNullOrEmpty (text);
			if (!flag) {
				byte priority = (byte) char.GetNumericValue (text[0]);
				text = text.Substring (1);
				bool flag2 = args.Length != 0;
				if (flag2) {
					string text2 = args[0].ToString ();
					int num;
					bool flag3 = int.TryParse (text2, out num);
					if (flag3) {
						text = text.Replace ("&", text2);
					} else {
						text = text.Replace ("%", text2);
					}
				}
				this.SetBattleMessage (text, priority);
			}
		}
	}

	// Token: 0x06001544 RID: 5444 RVA: 0x00155C90 File Offset: 0x00153E90
	public void SetBattleCommandTitle (CMD_DATA pCmd) {
		string text = string.Empty;
		string str = (!(Localization.GetSymbol () == "JP")) ? " " : string.Empty;
		byte cmd_no = pCmd.cmd_no;
		bool flag = cmd_no != 14 && cmd_no != 15;
		if (flag) {
			bool flag2 = cmd_no != 50;
			if (flag2) {
				bool flag3 = pCmd.sub_no < 192;
				if (flag3) {
					int num = (int) BattleHUD.CmdTitleTable[(int) pCmd.sub_no];
					int num2 = num;
					bool flag4 = num2 != 254;
					if (flag4) {
						bool flag5 = num2 != 255;
						if (flag5) {
							bool flag6 = num2 != 0;
							if (flag6) {
								bool flag7 = num < 192;
								if (flag7) {
									text = FF9TextTool.ActionAbilityName (num);
								} else {
									text = FF9TextTool.BattleCommandTitleText (checked ((num & 63) + 1));
								}
							}
						} else {
							text = FF9TextTool.ActionAbilityName ((int) pCmd.sub_no);
						}
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
		bool flag8 = !string.IsNullOrEmpty (text);
		if (flag8) {
			this.SetBattleTitle (text, 1);
		}
	}

	// Token: 0x06001545 RID: 5445 RVA: 0x00155DE4 File Offset: 0x00153FE4
	public string BtlGetAttrName (int pAttr) {
		int num = 0;
		checked {
			while ((pAttr >>= 1) != 0) {
				num++;
			}
			return FF9TextTool.BattleFollowText (num);
		}
	}

	// Token: 0x06001546 RID: 5446 RVA: 0x00022169 File Offset: 0x00020369
	public void SetBattleLibra (BTL_DATA pBtl) {
		this.currentLibraMessageNumber = 1;
		this.libraBtlData = pBtl;
		this.DisplayMessageLibra ();
	}

	// Token: 0x06001547 RID: 5447 RVA: 0x00155E14 File Offset: 0x00154014
	public void SetBattlePeeping (BTL_DATA pBtl) {
		bool flag = pBtl.bi.player > 0;
		checked {
			if (!flag) {
				this.peepingEnmData = FF9StateSystem.Battle.FF9Battle.enemy[(int) pBtl.bi.slot_no];
				bool flag2 = false;
				for (int i = 0; i < 4; i++) {
					bool flag3 = this.peepingEnmData.steal_item[i] != byte.MaxValue;
					if (flag3) {
						flag2 = true;
						break;
					}
				}
				bool flag4 = !flag2;
				if (flag4) {
					this.SetBattleMessage (FF9TextTool.BattleLibraText (9), 2);
					this.currentPeepingMessageCount = 5;
				} else {
					this.currentPeepingMessageCount = 1;
					this.DisplayMessagePeeping ();
				}
			}
		}
	}

	// Token: 0x06001548 RID: 5448 RVA: 0x00155EC4 File Offset: 0x001540C4
	public void SetBattleTitle (string str, byte priority) {
		bool flag = this.currentMessagePriority <= priority;
		if (flag) {
			this.currentMessagePriority = priority;
			this.battleMessageCounter = 0f;
			this.DisplayBattleMessage (str, true);
		}
	}

	// Token: 0x06001549 RID: 5449 RVA: 0x00155F00 File Offset: 0x00154100
	public void SetBattleMessage (string str, byte priority) {
		bool flag = this.currentMessagePriority <= priority;
		if (flag) {
			this.currentMessagePriority = priority;
			this.battleMessageCounter = 0f;
			this.DisplayBattleMessage (str, false);
		}
	}

	// Token: 0x0600154A RID: 5450 RVA: 0x00155F3C File Offset: 0x0015413C
	private void DisplayCommand () {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		byte menu_type = FF9StateSystem.Common.FF9.party.member[(int) btl_DATA.bi.line_no].info.menu_type;
		bool flag = Status.checkCurStat (btl_DATA, 16384u);
		checked {
			byte b;
			byte b2;
			if (flag) {
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
			bool flag2 = b > 0;
			bool flag3 = b2 > 0;
			bool flag4 = b2 == 31;
			if (flag4) {
				bool flag5 = !this.magicSwordCond.IsViviExist;
				if (flag5) {
					text2 = string.Empty;
					flag3 = false;
				} else {
					bool flag6 = this.magicSwordCond.IsViviDead || this.magicSwordCond.IsSteinerMini;
					if (flag6) {
						flag3 = false;
					}
				}
			}
			this.commandDetailHUD.Skill1Component.Label.text = text;
			ButtonGroupState.SetButtonEnable (this.commandDetailHUD.Skill1, flag2);
			ButtonGroupState.SetButtonAnimation (this.commandDetailHUD.Skill1, flag2);
			bool flag7 = flag2;
			if (flag7) {
				this.commandDetailHUD.Skill1Component.Label.color = FF9TextTool.White;
				this.commandDetailHUD.Skill1Component.ButtonGroup.Help.Enable = true;
				this.commandDetailHUD.Skill1Component.ButtonGroup.Help.TextKey = string.Empty;
				this.commandDetailHUD.Skill1Component.ButtonGroup.Help.Text = FF9TextTool.CommandHelpDescription ((int) b);
			} else {
				this.commandDetailHUD.Skill1Component.Label.color = FF9TextTool.Gray;
				this.commandDetailHUD.Skill1Component.UIBoxCollider.enabled = flag2;
				this.commandDetailHUD.Skill1Component.ButtonGroup.Help.Enable = false;
			}
			this.commandDetailHUD.Skill2Component.Label.text = text2;
			ButtonGroupState.SetButtonEnable (this.commandDetailHUD.Skill2, flag3);
			ButtonGroupState.SetButtonAnimation (this.commandDetailHUD.Skill2, flag3);
			bool flag8 = flag3;
			if (flag8) {
				this.commandDetailHUD.Skill2Component.Label.color = FF9TextTool.White;
				this.commandDetailHUD.Skill2Component.ButtonGroup.Help.Enable = true;
				this.commandDetailHUD.Skill2Component.ButtonGroup.Help.TextKey = string.Empty;
				this.commandDetailHUD.Skill2Component.ButtonGroup.Help.Text = FF9TextTool.CommandHelpDescription ((int) b2);
			} else {
				this.commandDetailHUD.Skill2Component.Label.color = FF9TextTool.Gray;
				this.commandDetailHUD.Skill2Component.UIBoxCollider.enabled = flag3;
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
			bool flag9 = ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton;
			if (flag9) {
				this.SetCommandVisibility (true, false);
			}
		}
	}

	// Token: 0x0600154B RID: 5451 RVA: 0x00156444 File Offset: 0x00154644
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
		checked {
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
						bool flag = next.bi.player != 0 && this.matchBattleIdPlayerList.Contains (num);
						if (flag) {
							int num2 = this.matchBattleIdPlayerList.IndexOf (num);
							BattleHUD.NumberSubModeHUD numberSubModeHUD = this.hpStatusHudList[num2];
							numberSubModeHUD.Self.SetActive (true);
							numberSubModeHUD.Current.text = next.cur.hp.ToString ();
							numberSubModeHUD.Max.text = next.max.hp.ToString ();
							BattleHUD.ParameterStatus parameterStatus = this.CheckHPState (next);
							bool flag2 = parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
							if (flag2) {
								numberSubModeHUD.TextColor = FF9TextTool.Red;
							} else {
								bool flag3 = parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
								if (flag3) {
									numberSubModeHUD.TextColor = FF9TextTool.Yellow;
								} else {
									numberSubModeHUD.TextColor = FF9TextTool.White;
								}
							}
							list.Remove (num2);
						}
					}
					using (List<int>.Enumerator enumerator = list.GetEnumerator ()) {
						while (enumerator.MoveNext ()) {
						int index = enumerator.Current;
						this.hpStatusHudList[index].Self.SetActive (false);
						}
						return;
					}
					break;
				case 2:
					break;
				case 3:
					goto IL_414;
				case 4:
					goto IL_640;
				default:
					return;
			}
			this.mpStatusPanel.SetActive (true);
			this.hpCaption.SetActive (false);
			this.mpCaption.SetActive (false);
			this.atbCaption.SetActive (false);
			for (BTL_DATA next2 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next2 != null; next2 = next2.next) {
				int num3 = 0;
				while (1 << num3 != (int) next2.btl_id) {
					num3++;
				}
				bool flag4 = next2.bi.player != 0 && this.matchBattleIdPlayerList.Contains (num3);
				if (flag4) {
					int num4 = this.matchBattleIdPlayerList.IndexOf (num3);
					BattleHUD.NumberSubModeHUD numberSubModeHUD2 = this.mpStatusHudList[num4];
					numberSubModeHUD2.Self.SetActive (true);
					numberSubModeHUD2.Current.text = next2.cur.mp.ToString ();
					numberSubModeHUD2.Max.text = next2.max.mp.ToString ();
					bool flag5 = this.CheckMPState (next2) == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
					if (flag5) {
						numberSubModeHUD2.TextColor = FF9TextTool.Yellow;
					} else {
						numberSubModeHUD2.TextColor = FF9TextTool.White;
					}
					list.Remove (num4);
				}
			}
			using (List<int>.Enumerator enumerator2 = list.GetEnumerator ()) {
				while (enumerator2.MoveNext ()) {
				int index2 = enumerator2.Current;
				this.mpStatusHudList[index2].Self.SetActive (false);
				}
				return;
			}
			IL_414:
				this.badStatusPanel.SetActive (true);
			this.hpCaption.SetActive (false);
			this.mpCaption.SetActive (false);
			this.atbCaption.SetActive (false);
			for (BTL_DATA next3 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next3 != null; next3 = next3.next) {
				int num5 = 0;
				while (1 << num5 != (int) next3.btl_id) {
					num5++;
				}
				bool flag6 = next3.bi.player != 0 && this.matchBattleIdPlayerList.Contains (num5);
				if (flag6) {
					int num6 = this.matchBattleIdPlayerList.IndexOf (num5);
					BattleHUD.StatusSubModeHUD statusSubModeHUD = this.badStatusHudList[num6];
					uint num7 = next3.stat.cur | next3.stat.permanent;
					statusSubModeHUD.Self.SetActive (true);
					UISprite[] statusesSpriteList = statusSubModeHUD.StatusesSpriteList;
					int num8;
					unchecked {
						for (int i = 0; i < statusesSpriteList.Length; i++) {
							statusesSpriteList[i].alpha = 0f;
						}
						num8 = 0;
					}
					foreach (KeyValuePair<uint, byte> keyValuePair in BattleHUD.BadIconDict) {
						bool flag7 = (num7 & keyValuePair.Key) > 0u;
						if (flag7) {
							statusSubModeHUD.StatusesSpriteList[num8].alpha = 1f;
							statusSubModeHUD.StatusesSpriteList[num8].spriteName = FF9UIDataTool.IconSpriteName[(int) keyValuePair.Value];
							num8++;
							bool flag8 = num8 > statusSubModeHUD.StatusesSpriteList.Length;
							if (flag8) {
								break;
							}
						}
					}
					list.Remove (num6);
				}
			}
			using (List<int>.Enumerator enumerator4 = list.GetEnumerator ()) {
				while (enumerator4.MoveNext ()) {
				int index3 = enumerator4.Current;
				this.badStatusHudList[index3].Self.SetActive (false);
				}
				return;
			}
			IL_640:
				this.goodStatusPanel.SetActive (true);
			this.hpCaption.SetActive (false);
			this.mpCaption.SetActive (false);
			this.atbCaption.SetActive (false);
			for (BTL_DATA next4 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next4 != null; next4 = next4.next) {
				int num9 = 0;
				while (1 << num9 != (int) next4.btl_id) {
					num9++;
				}
				bool flag9 = next4.bi.player != 0 && this.matchBattleIdPlayerList.Contains (num9);
				if (flag9) {
					int num10 = this.matchBattleIdPlayerList.IndexOf (num9);
					BattleHUD.StatusSubModeHUD statusSubModeHUD2 = this.goodStatusHudList[num10];
					uint num11 = next4.stat.cur | next4.stat.permanent;
					statusSubModeHUD2.Self.SetActive (true);
					UISprite[] statusesSpriteList2 = statusSubModeHUD2.StatusesSpriteList;
					int num12;
					unchecked {
						for (int j = 0; j < statusesSpriteList2.Length; j++) {
							statusesSpriteList2[j].alpha = 0f;
						}
						num12 = 0;
					}
					foreach (KeyValuePair<uint, byte> keyValuePair2 in BattleHUD.GoodIconDict) {
						bool flag10 = (num11 & keyValuePair2.Key) > 0u;
						if (flag10) {
							statusSubModeHUD2.StatusesSpriteList[num12].alpha = 1f;
							statusSubModeHUD2.StatusesSpriteList[num12].spriteName = FF9UIDataTool.IconSpriteName[(int) keyValuePair2.Value];
							num12++;
							bool flag11 = num12 > statusSubModeHUD2.StatusesSpriteList.Length;
							if (flag11) {
								break;
							}
						}
					}
					list.Remove (num10);
				}
			}
			foreach (int index4 in list) {
				this.goodStatusHudList[index4].Self.SetActive (false);
			}
		}
	}

	// Token: 0x0600154C RID: 5452 RVA: 0x00156D04 File Offset: 0x00154F04
	private void DisplayAbilityRealTime () {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		bool flag = this.currentSilenceStatus != btl_stat.CheckStatus (btl_DATA, 8u);
		if (flag) {
			this.currentSilenceStatus = !this.currentSilenceStatus;
			this.DisplayAbility ();
		}
		bool flag2 = this.currentMpValue != (int) btl_DATA.cur.mp;
		if (flag2) {
			this.currentMpValue = (int) btl_DATA.cur.mp;
			this.DisplayAbility ();
		}
	}

	// Token: 0x0600154D RID: 5453 RVA: 0x00156D8C File Offset: 0x00154F8C
	private void DisplayItemRealTime () {
		bool flag = this.needItemUpdate;
		if (flag) {
			this.needItemUpdate = false;
			rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[checked ((int) ((uint) ((UIntPtr) this.currentCommandId)))];
			this.DisplayItem (ff9COMMAND.type == 3);
		}
	}

	// Token: 0x0600154E RID: 5454 RVA: 0x00156DD8 File Offset: 0x00154FD8
	private void DisplayStatusRealtime () {
		bool activeSelf = this.hpStatusPanel.activeSelf;
		checked {
			if (activeSelf) {
				for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
					bool flag = next.bi.player > 0;
					if (flag) {
						int num = 0;
						while (1 << num != (int) next.btl_id) {
							num++;
						}
						bool flag2 = this.matchBattleIdPlayerList.Contains (num);
						if (flag2) {
							int index = this.matchBattleIdPlayerList.IndexOf (num);
							BattleHUD.NumberSubModeHUD numberSubModeHUD = this.hpStatusHudList[index];
							numberSubModeHUD.Self.SetActive (true);
							numberSubModeHUD.Current.text = next.cur.hp.ToString ();
							numberSubModeHUD.Max.text = next.max.hp.ToString ();
							BattleHUD.ParameterStatus parameterStatus = this.CheckHPState (next);
							bool flag3 = parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
							if (flag3) {
								numberSubModeHUD.TextColor = FF9TextTool.Red;
							} else {
								bool flag4 = parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
								if (flag4) {
									numberSubModeHUD.TextColor = FF9TextTool.Yellow;
								} else {
									numberSubModeHUD.TextColor = FF9TextTool.White;
								}
							}
						}
					}
				}
			} else {
				bool activeSelf2 = this.mpStatusPanel.activeSelf;
				if (activeSelf2) {
					for (BTL_DATA next2 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next2 != null; next2 = next2.next) {
						bool flag5 = next2.bi.player > 0;
						if (flag5) {
							int num2 = 0;
							while (1 << num2 != (int) next2.btl_id) {
								num2++;
							}
							bool flag6 = this.matchBattleIdPlayerList.Contains (num2);
							if (flag6) {
								int index2 = this.matchBattleIdPlayerList.IndexOf (num2);
								BattleHUD.NumberSubModeHUD numberSubModeHUD2 = this.mpStatusHudList[index2];
								numberSubModeHUD2.Self.SetActive (true);
								numberSubModeHUD2.Current.text = next2.cur.mp.ToString ();
								numberSubModeHUD2.Max.text = next2.max.mp.ToString ();
								bool flag7 = this.CheckMPState (next2) == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
								if (flag7) {
									numberSubModeHUD2.TextColor = FF9TextTool.Yellow;
								} else {
									numberSubModeHUD2.TextColor = FF9TextTool.White;
								}
							}
						}
					}
				} else {
					bool activeSelf3 = this.badStatusPanel.activeSelf;
					if (activeSelf3) {
						for (BTL_DATA next3 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next3 != null; next3 = next3.next) {
							bool flag8 = next3.bi.player > 0;
							if (flag8) {
								int num3 = 0;
								while (1 << num3 != (int) next3.btl_id) {
									num3++;
								}
								bool flag9 = this.matchBattleIdPlayerList.Contains (num3);
								if (flag9) {
									int index3 = this.matchBattleIdPlayerList.IndexOf (num3);
									BattleHUD.StatusSubModeHUD statusSubModeHUD = this.badStatusHudList[index3];
									uint num4 = next3.stat.cur | next3.stat.permanent;
									statusSubModeHUD.Self.SetActive (true);
									UISprite[] statusesSpriteList = statusSubModeHUD.StatusesSpriteList;
									int num5;
									unchecked {
										for (int i = 0; i < statusesSpriteList.Length; i++) {
											statusesSpriteList[i].alpha = 0f;
										}
										num5 = 0;
									}
									foreach (KeyValuePair<uint, byte> keyValuePair in BattleHUD.BadIconDict) {
										bool flag10 = (num4 & keyValuePair.Key) > 0u;
										if (flag10) {
											statusSubModeHUD.StatusesSpriteList[num5].alpha = 1f;
											statusSubModeHUD.StatusesSpriteList[num5].spriteName = FF9UIDataTool.IconSpriteName[(int) keyValuePair.Value];
											num5++;
											bool flag11 = num5 > statusSubModeHUD.StatusesSpriteList.Length;
											if (flag11) {
												break;
											}
										}
									}
								}
							}
						}
					} else {
						bool activeSelf4 = this.goodStatusPanel.activeSelf;
						if (activeSelf4) {
							for (BTL_DATA next4 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next4 != null; next4 = next4.next) {
								bool flag12 = next4.bi.player > 0;
								if (flag12) {
									int num6 = 0;
									while (1 << num6 != (int) next4.btl_id) {
										num6++;
									}
									bool flag13 = this.matchBattleIdPlayerList.Contains (num6);
									if (flag13) {
										int index4 = this.matchBattleIdPlayerList.IndexOf (num6);
										BattleHUD.StatusSubModeHUD statusSubModeHUD2 = this.goodStatusHudList[index4];
										uint num7 = next4.stat.cur | next4.stat.permanent;
										statusSubModeHUD2.Self.SetActive (true);
										UISprite[] statusesSpriteList2 = statusSubModeHUD2.StatusesSpriteList;
										int num8;
										unchecked {
											for (int j = 0; j < statusesSpriteList2.Length; j++) {
												statusesSpriteList2[j].alpha = 0f;
											}
											num8 = 0;
										}
										foreach (KeyValuePair<uint, byte> keyValuePair2 in BattleHUD.GoodIconDict) {
											bool flag14 = (num7 & keyValuePair2.Key) > 0u;
											if (flag14) {
												statusSubModeHUD2.StatusesSpriteList[num8].alpha = 1f;
												statusSubModeHUD2.StatusesSpriteList[num8].spriteName = FF9UIDataTool.IconSpriteName[(int) keyValuePair2.Value];
												num8++;
												bool flag15 = num8 > statusSubModeHUD2.StatusesSpriteList.Length;
												if (flag15) {
													break;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x0600154F RID: 5455 RVA: 0x00157410 File Offset: 0x00155610
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
		if (itemScrollList.ItemsPool.Count == 0) {
			this.itemScrollList.PopulateListItemWithData = new Action<Transform, ListDataTypeBase, int, bool> (this.DisplayItemDetail);
			this.itemScrollList.OnRecycleListItemClick += this.OnListItemClick;
			this.itemScrollList.Invoke ("RepositionList", 0.1f);
			this.itemScrollList.InitTableView (list, 0);
		} else {
			this.itemScrollList.SetOriginalData (list);
			this.itemScrollList.Invoke ("RepositionList", 0.1f);
		}
	}

	// Token: 0x06001550 RID: 5456 RVA: 0x001575C0 File Offset: 0x001557C0
	public void DisplayItemDetail (Transform item, ListDataTypeBase data, int index, bool isInit) {
		BattleHUD.BattleItemListData battleItemListData = (BattleHUD.BattleItemListData) data;
		ItemListDetailWithIconHUD itemListDetailWithIconHUD = new ItemListDetailWithIconHUD (item.gameObject, true);
		if (isInit) {
			this.DisplayWindowBackground (item.gameObject, null);
		}
		bool flag = battleItemListData.Id == 255;
		if (flag) {
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

	// Token: 0x06001551 RID: 5457 RVA: 0x001576EC File Offset: 0x001558EC
	private void DisplayAbility () {
		rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) this.currentCommandId))];
		this.SetAbilityAp (this.abilityDetailDict[this.currentPlayerId]);
		List<ListDataTypeBase> list = new List<ListDataTypeBase> ();
		for (int i = (int) ff9COMMAND.ability; i < (int) ff9COMMAND.ability + (int) ff9COMMAND.count; i++) {
			list.Add (new BattleHUD.BattleAbilityListData {
				Index = i
			});
		}
		if (abilityScrollList.ItemsPool.Count == 0) {
			this.abilityScrollList.PopulateListItemWithData = new Action<Transform, ListDataTypeBase, int, bool> (this.DisplayAbilityDetail);
			this.abilityScrollList.OnRecycleListItemClick += this.OnListItemClick;
			this.abilityScrollList.Invoke ("RepositionList", 0.1f);
			this.abilityScrollList.InitTableView (list, 0);
		} else {
			this.abilityScrollList.SetOriginalData (list);
			this.abilityScrollList.Invoke ("RepositionList", 0.1f);
		}
	}

	// Token: 0x06001552 RID: 5458 RVA: 0x001577D4 File Offset: 0x001559D4
	private void DisplayAbilityDetail (Transform item, ListDataTypeBase data, int index, bool isInit) {
		BattleHUD.BattleAbilityListData battleAbilityListData = (BattleHUD.BattleAbilityListData) data;
		ItemListDetailHUD itemListDetailHUD = new ItemListDetailHUD (item.gameObject);
		if (isInit) {
			this.DisplayWindowBackground (item.gameObject, null);
		}
		int num = rdata._FF9BMenu_ComAbil[battleAbilityListData.Index];
		BattleHUD.AbilityStatus abilityState = this.GetAbilityState (num);
		AA_DATA aaData = FF9StateSystem.Battle.FF9Battle.aa_data[num];
		bool flag = abilityState == BattleHUD.AbilityStatus.ABILSTAT_NONE;
		if (flag) {
			itemListDetailHUD.Content.SetActive (false);
			ButtonGroupState.SetButtonAnimation (itemListDetailHUD.Self, false);
			itemListDetailHUD.Button.Help.TextKey = string.Empty;
			itemListDetailHUD.Button.Help.Text = string.Empty;
		} else {
			itemListDetailHUD.Content.SetActive (true);
			itemListDetailHUD.NameLabel.text = FF9TextTool.ActionAbilityName (num);
			int mp = this.GetMp (aaData);
			bool flag2 = mp != 0;
			if (flag2) {
				itemListDetailHUD.NumberLabel.text = mp.ToString ();
			} else {
				itemListDetailHUD.NumberLabel.text = string.Empty;
			}
			bool flag3 = abilityState == BattleHUD.AbilityStatus.ABILSTAT_DISABLE;
			if (flag3) {
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

	// Token: 0x06001553 RID: 5459 RVA: 0x00022181 File Offset: 0x00020381
	public void DisplayInfomation () {
		this.DisplayParty ();
	}

	// Token: 0x06001554 RID: 5460 RVA: 0x00157980 File Offset: 0x00155B80
	private void DisplayParty () {
		int i = 0;
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				int num = 0;
				while (1 << num != (int) next.btl_id) {
					num++;
				}
				bool flag = next.bi.player > 0;
				if (flag) {
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
			this.PartyDetailPanel.transform.localPosition = new Vector3 (this.PartyDetailPanel.transform.localPosition.x, unchecked (BattleHUD.DefaultPartyPanelPosY - BattleHUD.PartyItemHeight * (float) (checked (this.playerDetailPanelList.Count - i))), this.PartyDetailPanel.transform.localPosition.z);
			while (i < this.playerDetailPanelList.Count) {
				this.playerDetailPanelList[i].Self.SetActive (false);
				this.playerDetailPanelList[i].PlayerId = -1;
				i++;
			}
		}
	}

	// Token: 0x06001555 RID: 5461 RVA: 0x00157B04 File Offset: 0x00155D04
	public void DisplayPartyRealtime () {
		int num = 0;
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				bool flag = next.bi.player > 0;
				if (flag) {
					BattleHUD.PlayerDetailHUD playerHud = this.playerDetailPanelList[num];
					BattleHUD.InfoVal hp = this.hpInfoVal[num];
					BattleHUD.InfoVal mp = this.mpInfoVal[num];
					num++;
					this.DisplayCharacterParameter (playerHud, next, hp, mp);
				}
			}
		}
	}

	// Token: 0x06001556 RID: 5462 RVA: 0x00157B90 File Offset: 0x00155D90
	private void DisplayTarget () {
		bool flag = false;
		int num = this.enemyCount;
		int num2 = this.playerCount;
		int num3 = 0;
		int num4 = 0;
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				bool flag2 = next.bi.player > 0;
				if (flag2) {
					bool flag3 = next.bi.target > 0;
					if (flag3) {
						num4++;
					}
				} else {
					bool flag4 = next.bi.target > 0;
					if (flag4) {
						num3++;
					}
				}
			}
			bool flag5 = num3 != num || num4 != num2;
			if (flag5) {
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
				bool flag6 = next2.btl_id != 0 && next2.bi.target > 0;
				if (flag6) {
					bool flag7 = next2.bi.player > 0;
					if (flag7) {
						BattleHUD.ParameterStatus parameterStatus = this.CheckHPState (next2);
						bool flag8 = num5 >= this.currentCharacterHp.Count;
						if (flag8) {
							this.currentCharacterHp.Add (parameterStatus);
							this.matchBattleIdPlayerList.Add (num7);
							flag = true;
						} else {
							bool flag9 = parameterStatus != this.currentCharacterHp[num5];
							if (flag9) {
								this.currentCharacterHp[num5] = parameterStatus;
								flag = true;
							}
						}
						num5++;
					} else {
						bool flag10 = Status.checkCurStat (next2, 256u);
						bool flag11 = num6 >= this.currentEnemyDieState.Count;
						if (flag11) {
							this.currentEnemyDieState.Add (flag10);
							this.matchBattleIdEnemyList.Add (num7);
							flag = true;
						} else {
							bool flag12 = flag10 != this.currentEnemyDieState[num6];
							if (flag12) {
								this.currentEnemyDieState[num6] = flag10;
								flag = true;
							}
						}
						num6++;
					}
				}
			}
			bool flag13 = !flag;
			if (!flag13) {
				foreach (BattleHUD.TargetHUD targetHUD in this.targetHudList) {
					targetHUD.KeyNavigate.startsSelected = false;
					targetHUD.Self.SetActive (false);
				}
				GameObject gameObject = null;
				int num8 = 0;
				int num9 = 4;
				bool flag14 = this.cursorType == BattleHUD.CursorGroup.Individual;
				if (flag14) {
					gameObject = ButtonGroupState.ActiveButton;
				}
				for (BTL_DATA next3 = FF9StateSystem.Battle.FF9Battle.btl_list.next; next3 != null; next3 = next3.next) {
					bool flag15 = next3.btl_id != 0 && next3.bi.target > 0;
					if (flag15) {
						bool flag16 = next3.bi.player > 0;
						if (flag16) {
							BattleHUD.TargetHUD targetHUD2 = this.targetHudList[num8];
							GameObject self = targetHUD2.Self;
							UILabel nameLabel = targetHUD2.NameLabel;
							self.SetActive (true);
							nameLabel.text = btl_util.getPlayerPtr (next3).name;
							bool flag17 = this.currentCharacterHp[num8] == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
							if (flag17) {
								bool flag18 = this.cursorType == BattleHUD.CursorGroup.Individual;
								if (flag18) {
									bool flag19 = this.targetDead == 0;
									if (flag19) {
										ButtonGroupState.SetButtonEnable (self, false);
										bool flag20 = self == gameObject;
										if (flag20) {
											int firstPlayer = this.GetFirstPlayer ();
											bool flag21 = firstPlayer != -1;
											if (flag21) {
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
							} else {
								bool flag22 = this.currentCharacterHp[num8] == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
								if (flag22) {
									bool flag23 = this.cursorType == BattleHUD.CursorGroup.Individual;
									if (flag23) {
										ButtonGroupState.SetButtonEnable (self, true);
									}
									nameLabel.color = FF9TextTool.Yellow;
								} else {
									bool flag24 = this.cursorType == BattleHUD.CursorGroup.Individual;
									if (flag24) {
										ButtonGroupState.SetButtonEnable (self, true);
									}
									nameLabel.color = FF9TextTool.White;
								}
							}
							num8++;
						} else {
							BattleHUD.TargetHUD targetHUD3 = this.targetHudList[num9];
							GameObject self2 = targetHUD3.Self;
							UILabel nameLabel2 = targetHUD3.NameLabel;
							float num10 = 0f;
							self2.SetActive (true);
							nameLabel2.text = nameLabel2.PhrasePreOpcodeSymbol (btl_util.getEnemyPtr (next3).et.name, ref num10);
							bool flag25 = this.currentEnemyDieState[num9 - 4];
							if (flag25) {
								bool flag26 = this.cursorType == BattleHUD.CursorGroup.Individual;
								if (flag26) {
									ButtonGroupState.SetButtonEnable (self2, false);
									bool flag27 = this.targetDead == 0;
									if (flag27) {
										bool flag28 = self2 == gameObject;
										if (flag28) {
											int num11 = this.GetFirstEnemy () + HonoluluBattleMain.EnemyStartIndex;
											bool flag29 = num11 != -1;
											if (flag29) {
												bool flag30 = this.currentCommandIndex == BattleHUD.CommandMenu.Attack && FF9StateSystem.PCPlatform && this.enemyCount > 1;
												if (flag30) {
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
								bool flag31 = this.cursorType == BattleHUD.CursorGroup.Individual;
								if (flag31) {
									ButtonGroupState.SetButtonEnable (self2, true);
								}
								nameLabel2.color = FF9TextTool.White;
							}
							num9++;
						}
					}
				}
				bool flag32 = (num != num3 || num2 != num4) && ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton;
				if (flag32) {
					this.SetTargetDefault ();
					this.modelButtonManager.Reset ();
					this.EnableTargetArea ();
					this.SetTargetHelp ();
					ButtonGroupState.DisableAllGroup (true);
					ButtonGroupState.ActiveGroup = BattleHUD.TargetGroupButton;
				}
				bool flag33 = gameObject != null && this.cursorType == BattleHUD.CursorGroup.Individual && gameObject.activeSelf;
				if (flag33) {
					ButtonGroupState.ActiveButton = gameObject;
				} else {
					this.DisplayTargetPointer ();
				}
			}
		}
	}

	// Token: 0x06001557 RID: 5463 RVA: 0x001582DC File Offset: 0x001564DC
	private void DisplayCharacterParameter (BattleHUD.PlayerDetailHUD playerHud, BTL_DATA bd, BattleHUD.InfoVal hp, BattleHUD.InfoVal mp) {
		playerHud.NameLabel.text = btl_util.getPlayerPtr (bd).name;
		playerHud.HPLabel.text = hp.disp_val.ToString ();
		playerHud.MPLabel.text = mp.disp_val.ToString ();
		BattleHUD.ParameterStatus parameterStatus = this.CheckHPState (bd);
		bool flag = parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
		if (flag) {
			playerHud.ATBSlider.value = 0f;
			playerHud.HPLabel.color = FF9TextTool.Red;
			playerHud.NameLabel.color = FF9TextTool.Red;
			playerHud.ATBBlink = false;
			playerHud.TranceBlink = false;
		} else {
			bool flag2 = parameterStatus == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
			if (flag2) {
				playerHud.ATBSlider.value = (float) bd.cur.at / (float) bd.max.at;
				playerHud.HPLabel.color = FF9TextTool.Yellow;
				playerHud.NameLabel.color = FF9TextTool.Yellow;
			} else {
				playerHud.ATBSlider.value = (float) bd.cur.at / (float) bd.max.at;
				playerHud.HPLabel.color = FF9TextTool.White;
				playerHud.NameLabel.color = FF9TextTool.White;
			}
		}
		bool flag3 = this.CheckMPState (bd) == BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
		if (flag3) {
			playerHud.MPLabel.color = FF9TextTool.Yellow;
		} else {
			playerHud.MPLabel.color = FF9TextTool.White;
		}
		string spriteName = BattleHUD.ATENormal;
		bool flag4 = btl_stat.CheckStatus (bd, 1052672u);
		if (flag4) {
			spriteName = BattleHUD.ATEGray;
		} else {
			bool flag5 = btl_stat.CheckStatus (bd, 524288u);
			if (flag5) {
				spriteName = BattleHUD.ATEOrange;
			}
		}
		playerHud.ATBForegroundSprite.spriteName = spriteName;
		bool flag6 = bd.bi.t_gauge > 0;
		if (flag6) {
			playerHud.TranceSlider.value = (float) bd.trance / 256f;
			bool flag7 = parameterStatus != BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
			if (flag7) {
				bool flag8 = bd.trance == byte.MaxValue && !playerHud.TranceBlink;
				if (flag8) {
					playerHud.TranceBlink = true;
					bool flag9 = !this.currentTrancePlayer.Contains ((int) bd.bi.line_no);
					if (flag9) {
						this.currentTrancePlayer.Add ((int) bd.bi.line_no);
						this.currentTranceTrigger = true;
					}
				} else {
					bool flag10 = bd.trance != byte.MaxValue;
					if (flag10) {
						playerHud.TranceBlink = false;
						bool flag11 = this.currentTrancePlayer.Contains ((int) bd.bi.line_no);
						if (flag11) {
							this.currentTrancePlayer.Remove ((int) bd.bi.line_no);
							this.currentTranceTrigger = true;
						}
					}
				}
			}
		}
	}

	// Token: 0x06001558 RID: 5464 RVA: 0x001585B0 File Offset: 0x001567B0
	public void AddPlayerToReady (int playerId) {
		bool flag = !this.unconsciousStateList.Contains (playerId);
		if (flag) {
			this.readyQueue.Add (playerId);
			this.playerDetailPanelList.First ((BattleHUD.PlayerDetailHUD hud) => hud.PlayerId == playerId).ATBBlink = true;
		}
	}

	// Token: 0x06001559 RID: 5465 RVA: 0x00158618 File Offset: 0x00156818
	public void RemovePlayerFromAction (int btl_id, bool isNeedToClearCommand) {
		int num = 0;
		checked {
			while (1 << num != btl_id) {
				num++;
			}
			bool flag = this.inputFinishedList.Contains (num) && isNeedToClearCommand;
			if (flag) {
				this.inputFinishedList.Remove (num);
			}
			bool flag2 = this.readyQueue.Contains (num) && isNeedToClearCommand;
			if (flag2) {
				this.readyQueue.Remove (num);
			}
		}
	}

	// Token: 0x0600155A RID: 5466 RVA: 0x00158684 File Offset: 0x00156884
	private void ManageAbility () {
		this.abilityDetailDict.Clear ();
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				int num = 0;
				while (1 << num != (int) next.btl_id) {
					num++;
				}
				bool flag = next.bi.player > 0;
				if (flag) {
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
	}

	// Token: 0x0600155B RID: 5467 RVA: 0x00158774 File Offset: 0x00156974
	private BattleHUD.ParameterStatus CheckHPState (BTL_DATA bd) {
		bool flag = bd.cur.hp == 0;
		BattleHUD.ParameterStatus result;
		if (flag) {
			result = BattleHUD.ParameterStatus.PARAMSTAT_EMPTY;
		} else {
			bool flag2 = (float) bd.cur.hp > (float) bd.max.hp / 6f;
			if (flag2) {
				result = BattleHUD.ParameterStatus.PARAMSTAT_NORMAL;
			} else {
				bool flag3 = bd.bi.player > 0;
				if (flag3) {
					result = BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
				} else {
					result = BattleHUD.ParameterStatus.PARAMSTAT_NORMAL;
				}
			}
		}
		return result;
	}

	// Token: 0x0600155C RID: 5468 RVA: 0x001587E0 File Offset: 0x001569E0
	private BattleHUD.ParameterStatus CheckMPState (BTL_DATA bd) {
		bool flag = (float) bd.cur.mp <= (float) bd.max.mp / 6f;
		BattleHUD.ParameterStatus result;
		if (flag) {
			result = BattleHUD.ParameterStatus.PARAMSTAT_CRITICAL;
		} else {
			result = BattleHUD.ParameterStatus.PARAMSTAT_NORMAL;
		}
		return result;
	}

	// Token: 0x0600155D RID: 5469 RVA: 0x00158820 File Offset: 0x00156A20
	private void CheckDoubleCast (int battleIndex, BattleHUD.CursorGroup cursorType) {
		bool flag = (this.IsDoubleCast && this.doubleCastCount == 2) || !this.IsDoubleCast;
		checked {
			if (flag) {
				this.doubleCastCount = 0;
				this.SetTarget (battleIndex);
			} else {
				bool flag2 = this.IsDoubleCast && this.doubleCastCount < 2;
				if (flag2) {
					this.doubleCastCount += 1;
					this.firstCommand = this.ProcessCommand (battleIndex, cursorType);
					this.subMenuType = BattleHUD.SubMenuType.CommandAbility;
					this.DisplayAbility ();
					this.SetTargetVisibility (false);
					this.SetAbilityPanelVisibility (true, true);
					this.BackButton.SetActive (FF9StateSystem.MobilePlatform);
				}
			}
		}
	}

	// Token: 0x0600155E RID: 5470 RVA: 0x001588CC File Offset: 0x00156ACC
	private void CheckPlayerState () {
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				int num = 0;
				while (1 << num != (int) next.btl_id) {
					num++;
				}
				bool flag = next.bi.player > 0;
				if (flag) {
					bool flag2 = !this.IsEnableInput (next);
					if (flag2) {
						bool flag3 = !this.unconsciousStateList.Contains (num);
						if (flag3) {
							this.unconsciousStateList.Add (num);
						}
					} else {
						bool flag4 = this.unconsciousStateList.Contains (num);
						if (flag4) {
							this.unconsciousStateList.Remove (num);
						}
					}
				}
			}
		}
	}

	// Token: 0x0600155F RID: 5471 RVA: 0x00158998 File Offset: 0x00156B98
	public void ActivateTurnForPlayer (int playerId) {
		BattleHUD.PlayerDetailHUD playerDetailHUD = this.playerDetailPanelList.Find ((BattleHUD.PlayerDetailHUD hud) => hud.PlayerId == playerId);
		playerDetailHUD.Component.UIBoxCollider.enabled = false;
		playerDetailHUD.Component.ButtonColor.SetState (UIButtonColor.State.Pressed, false);
		this.DisplayCommand ();
		this.SetCommandVisibility (true, false);
	}

	// Token: 0x06001560 RID: 5472 RVA: 0x0002218B File Offset: 0x0002038B
	private void SwitchPlayer (int playerId) {
		this.SetIdle ();
		FF9Sfx.FF9SFX_Play (1044);
		this.currentPlayerId = playerId;
		this.ActivateTurnForPlayer (playerId);
	}

	// Token: 0x06001561 RID: 5473 RVA: 0x00158A00 File Offset: 0x00156C00
	private void UpdatePlayer () {
		this.blinkAlphaCounter += RealTime.deltaTime * 3f;
		this.blinkAlphaCounter = ((this.blinkAlphaCounter <= 2f) ? this.blinkAlphaCounter : 0f);
		bool flag = this.blinkAlphaCounter <= 1f;
		float alpha;
		if (flag) {
			alpha = this.blinkAlphaCounter;
		} else {
			alpha = 2f - this.blinkAlphaCounter;
		}
		bool flag2 = this.commandEnable;
		checked {
			if (flag2) {
				foreach (BattleHUD.PlayerDetailHUD playerDetailHUD in this.playerDetailPanelList) {
					bool flag3 = playerDetailHUD.PlayerId != -1;
					if (flag3) {
						BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[playerDetailHUD.PlayerId];
						bool flag4 = (Status.checkCurStat (btl_DATA, 1024u) || Status.checkCurStat (btl_DATA, 2048u)) && playerDetailHUD.ATBBlink;
						if (flag4) {
							playerDetailHUD.ATBBlink = false;
						}
						bool flag5 = this.IsEnableInput (btl_DATA) && !this.isAutoAttack;
						if (flag5) {
							bool atbblink = playerDetailHUD.ATBBlink;
							if (atbblink) {
								playerDetailHUD.ATBForegroundWidget.alpha = alpha;
							}
							bool flag6 = playerDetailHUD.TranceBlink && btl_DATA.bi.t_gauge > 0;
							if (flag6) {
								playerDetailHUD.TranceForegroundWidget.alpha = alpha;
							}
						} else {
							bool atbblink2 = playerDetailHUD.ATBBlink;
							if (atbblink2) {
								playerDetailHUD.ATBForegroundWidget.alpha = 1f;
								playerDetailHUD.ATBHighlightSprite.alpha = 0f;
							}
							bool flag7 = playerDetailHUD.TranceBlink && btl_DATA.bi.t_gauge > 0;
							if (flag7) {
								playerDetailHUD.TranceForegroundWidget.alpha = 1f;
								playerDetailHUD.TranceHighlightSprite.alpha = 0f;
							}
						}
					}
				}
				this.YMenu_ManagerHpMp ();
				this.CheckPlayerState ();
				this.DisplayPartyRealtime ();
				bool activeSelf = this.TargetPanel.activeSelf;
				if (activeSelf) {
					this.DisplayTarget ();
					this.DisplayStatusRealtime ();
				}
				this.ManagerTarget ();
				this.ManagerInfo ();
				bool flag8 = this.currentPlayerId > -1;
				if (flag8) {
					BTL_DATA btl_DATA2 = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
					bool flag9 = ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton && this.isTranceMenu;
					if (flag9) {
						this.tranceColorCounter = (this.tranceColorCounter + 1) % this.tranceTextColor.Length;
						this.CommandCaptionLabel.color = this.tranceTextColor[this.tranceColorCounter];
					}
					bool flag10 = !this.IsEnableInput (btl_DATA2);
					if (flag10) {
						this.SetIdle ();
						return;
					}
					bool activeSelf2 = this.TargetPanel.activeSelf;
					if (activeSelf2) {
						bool flag11 = !this.ManageTargetCommand ();
						if (flag11) {
							return;
						}
					} else {
						bool flag12 = this.AbilityPanel.activeSelf || this.ItemPanel.activeSelf;
						if (flag12) {
							bool activeSelf3 = this.AbilityPanel.activeSelf;
							if (activeSelf3) {
								this.DisplayAbilityRealTime ();
							}
							bool activeSelf4 = this.ItemPanel.activeSelf;
							if (activeSelf4) {
								this.DisplayItemRealTime ();
							}
							bool flag13 = this.currentCommandId == 31u && (!this.magicSwordCond.IsViviExist || this.magicSwordCond.IsViviDead || this.magicSwordCond.IsSteinerMini);
							if (flag13) {
								FF9Sfx.FF9SFX_Play (101);
								this.ResetToReady ();
								return;
							}
							bool flag14 = !this.isTranceMenu && btl_stat.CheckStatus (btl_DATA2, 16384u);
							if (flag14) {
								FF9Sfx.FF9SFX_Play (101);
								this.ResetToReady ();
								return;
							}
						}
					}
				}
				bool flag15 = this.readyQueue.Count > 0 && this.currentPlayerId == -1;
				if (flag15) {
					for (int i = this.readyQueue.Count - 1; i >= 0; i--) {
						bool flag16 = this.unconsciousStateList.Contains (this.readyQueue[i]);
						if (flag16) {
							BTL_DATA btl_DATA3 = FF9StateSystem.Battle.FF9Battle.btl_data[this.readyQueue[i]];
							this.RemovePlayerFromAction ((int) btl_DATA3.btl_id, btl_stat.CheckStatus (btl_DATA3, 134403u));
						}
					}
					foreach (int num in this.readyQueue) {
						bool flag17 = !this.inputFinishedList.Contains (num) && !this.unconsciousStateList.Contains (num);
						if (flag17) {
							bool flag18 = this.isAutoAttack;
							if (flag18) {
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
	}

	// Token: 0x06001562 RID: 5474 RVA: 0x00158F58 File Offset: 0x00157158
	private BattleHUD.AbilityStatus CheckAbilityStatus (int subMenuIndex) {
		int num = checked ((int) rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) this.currentCommandId))].ability + subMenuIndex);
		int abilId = rdata._FF9BMenu_ComAbil[num];
		return this.GetAbilityState (abilId);
	}

	// Token: 0x06001563 RID: 5475 RVA: 0x00158F9C File Offset: 0x0015719C
	public void YMenu_ManagerHpMp () {
		BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next;
		int num = 0;
		checked {
			while (next != null) {
				bool flag = next.bi.player > 0;
				if (flag) {
					BattleHUD.InfoVal infoVal = this.hpInfoVal[num];
					BattleHUD.InfoVal infoVal2 = this.mpInfoVal[num];
					for (int i = 0; i < 2; i++) {
						BattleHUD.InfoVal infoVal3 = (i != 0) ? infoVal2 : infoVal;
						bool flag2 = infoVal3.anim_frm != 0;
						if (flag2) {
							bool flag3 = 0 <= infoVal3.inc_val;
							if (flag3) {
								bool flag4 = infoVal3.disp_val + infoVal3.inc_val >= infoVal3.req_val;
								if (flag4) {
									infoVal3.disp_val = infoVal3.req_val;
									infoVal3.anim_frm = 0;
								} else {
									infoVal3.disp_val += infoVal3.inc_val;
									infoVal3.anim_frm--;
								}
							} else {
								bool flag5 = infoVal3.disp_val + infoVal3.inc_val <= infoVal3.req_val;
								if (flag5) {
									infoVal3.disp_val = infoVal3.req_val;
									infoVal3.anim_frm = 0;
								} else {
									infoVal3.disp_val += infoVal3.inc_val;
									infoVal3.anim_frm--;
								}
							}
						} else {
							int num2 = (int) ((i != 0) ? next.cur.mp : ((short) next.cur.hp));
							int num3 = (int) ((i != 0) ? next.max.mp : ((short) next.max.hp));
							int num4;
							bool flag6 = (num4 = num2 - infoVal3.disp_val) != 0;
							if (flag6) {
								int num5 = Mathf.Abs (num4);
								infoVal3.req_val = (int) ((short) num2);
								bool flag7 = num5 < BattleHUD.YINFO_ANIM_HPMP_MIN;
								if (flag7) {
									infoVal3.anim_frm = num5;
								} else {
									infoVal3.anim_frm = num5 * BattleHUD.YINFO_ANIM_HPMP_MAX / num3;
									bool flag8 = BattleHUD.YINFO_ANIM_HPMP_MIN > infoVal3.anim_frm;
									if (flag8) {
										infoVal3.anim_frm = BattleHUD.YINFO_ANIM_HPMP_MIN;
									}
								}
								bool flag9 = 0 <= num4;
								if (flag9) {
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
	}

	// Token: 0x06001564 RID: 5476 RVA: 0x0015923C File Offset: 0x0015743C
	private void ManagerInfo () {
		BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next;
		BattleHUD.MagicSwordCondition magicSwordCondition = new BattleHUD.MagicSwordCondition ();
		BattleHUD.MagicSwordCondition magicSwordCondition2 = new BattleHUD.MagicSwordCondition ();
		magicSwordCondition.IsViviExist = this.magicSwordCond.IsViviExist;
		magicSwordCondition.IsViviDead = this.magicSwordCond.IsViviDead;
		magicSwordCondition.IsSteinerMini = this.magicSwordCond.IsSteinerMini;
		while (next != null && next.bi.player > 0) {
			bool flag = next.bi.slot_no == 1;
			if (flag) {
				magicSwordCondition2.IsViviExist = true;
				bool flag2 = next.cur.hp == 0;
				if (flag2) {
					magicSwordCondition2.IsViviDead = true;
				} else {
					bool flag3 = btl_stat.CheckStatus (next, 318905611u);
					if (flag3) {
						magicSwordCondition2.IsViviDead = true;
					}
				}
			} else {
				bool flag4 = next.bi.slot_no == 3;
				if (flag4) {
					bool flag5 = btl_stat.CheckStatus (next, 268435456u);
					if (flag5) {
						magicSwordCondition2.IsSteinerMini = true;
					} else {
						magicSwordCondition2.IsSteinerMini = false;
					}
				}
			}
			next = next.next;
		}
		bool flag6 = magicSwordCondition != magicSwordCondition2;
		if (flag6) {
			this.magicSwordCond.IsViviExist = magicSwordCondition2.IsViviExist;
			this.magicSwordCond.IsViviDead = magicSwordCondition2.IsViviDead;
			this.magicSwordCond.IsSteinerMini = magicSwordCondition2.IsSteinerMini;
			bool flag7 = this.currentPlayerId != -1;
			if (flag7) {
				this.DisplayCommand ();
			}
		} else {
			bool flag8 = !this.isTranceMenu && this.currentPlayerId != -1 && btl_stat.CheckStatus (FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId], 16384u);
			if (flag8) {
				this.DisplayCommand ();
			}
		}
	}

	// Token: 0x06001565 RID: 5477 RVA: 0x00159400 File Offset: 0x00157600
	private bool ManageTargetCommand () {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		bool flag = this.currentCommandId == 31u && (!this.magicSwordCond.IsViviExist || this.magicSwordCond.IsViviDead || this.magicSwordCond.IsSteinerMini);
		checked {
			bool result;
			if (flag) {
				FF9Sfx.FF9SFX_Play (101);
				this.ResetToReady ();
				result = false;
			} else {
				bool flag2 = !this.isTranceMenu && btl_stat.CheckStatus (btl_DATA, 16384u);
				if (flag2) {
					FF9Sfx.FF9SFX_Play (101);
					this.ResetToReady ();
					result = false;
				} else {
					bool flag3 = this.subMenuType == BattleHUD.SubMenuType.CommandAbility;
					if (flag3) {
						int num = (int) rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) this.currentCommandId))].ability;
						int num2 = this.PatchAbility (rdata._FF9BMenu_ComAbil[num + this.currentSubMenuIndex]);
						AA_DATA aa_DATA = FF9StateSystem.Battle.FF9Battle.aa_data[num2];
						int num3 = ff9abil.FF9Abil_GetEnableSA ((int) btl_DATA.bi.slot_no, BattleHUD.AbilSaMpHalf) ? (aa_DATA.MP >> 1) : ((int) aa_DATA.MP);
						bool flag4 = (int) btl_DATA.cur.mp < num3;
						if (flag4) {
							FF9Sfx.FF9SFX_Play (101);
							this.DisplayAbility ();
							this.SetTargetVisibility (false);
							this.ClearModelPointer ();
							this.SetAbilityPanelVisibility (true, true);
							return false;
						}
						bool flag5 = (aa_DATA.Category & 2) != 0 && btl_stat.CheckStatus (btl_DATA, 8u);
						if (flag5) {
							FF9Sfx.FF9SFX_Play (101);
							this.DisplayAbility ();
							this.SetTargetVisibility (false);
							this.ClearModelPointer ();
							this.SetAbilityPanelVisibility (true, true);
							return false;
						}
					}
					bool flag6 = (this.subMenuType == BattleHUD.SubMenuType.CommandItem || this.subMenuType == BattleHUD.SubMenuType.CommandThrow) && ff9item.FF9Item_GetCount (this.itemIdList[this.currentSubMenuIndex]) == 0;
					if (flag6) {
						FF9Sfx.FF9SFX_Play (101);
						this.DisplayItem (BattleHUD.SubMenuType.CommandThrow == this.subMenuType);
						this.SetTargetVisibility (false);
						this.ClearModelPointer ();
						this.SetItemPanelVisibility (true, true);
						result = false;
					} else {
						result = true;
					}
				}
			}
			return result;
		}
	}

	// Token: 0x06001566 RID: 5478 RVA: 0x0015962C File Offset: 0x0015782C
	private void ManagerTarget () {
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			bool flag = next.tar_mode >= 2;
			if (flag) {
				bool flag2 = next.tar_mode == 2;
				if (flag2) {
					next.bi.target = (next.bi.atb = 0);
					next.tar_mode = 0;
				} else {
					bool flag3 = next.tar_mode == 3;
					if (flag3) {
						next.bi.target = (next.bi.atb = 1);
						next.tar_mode = 1;
					}
				}
			}
		}
	}

	// Token: 0x06001567 RID: 5479 RVA: 0x001596E0 File Offset: 0x001578E0
	private void InitHpMp () {
		this.hpInfoVal.Clear ();
		this.mpInfoVal.Clear ();
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			BattleHUD.InfoVal infoVal = new BattleHUD.InfoVal ();
			BattleHUD.InfoVal infoVal2 = new BattleHUD.InfoVal ();
			infoVal.req_val = (infoVal.disp_val = (int) (checked ((short) next.cur.hp)));
			infoVal2.req_val = (infoVal2.disp_val = (int) next.cur.mp);
			infoVal.anim_frm = (infoVal2.anim_frm = 0);
			infoVal.inc_val = (infoVal2.inc_val = 0);
			this.hpInfoVal.Add (infoVal);
			this.mpInfoVal.Add (infoVal2);
		}
	}

	// Token: 0x06001568 RID: 5480 RVA: 0x001597B4 File Offset: 0x001579B4
	private int GetMp (AA_DATA aaData) {
		int num = (int) aaData.MP;
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		int slot_no = (int) FF9StateSystem.Common.FF9.party.member[(int) btl_DATA.bi.line_no].info.slot_no;
		bool flag = (aaData.Type & 4) != 0 && FF9StateSystem.EventState.gEventGlobal[18] > 0;
		if (flag) {
			num <<= 2;
		}
		bool flag2 = ff9abil.FF9Abil_GetEnableSA (slot_no, BattleHUD.AbilSaMpHalf);
		if (flag2) {
			num >>= 1;
		}
		return num;
	}

	// Token: 0x06001569 RID: 5481 RVA: 0x00159850 File Offset: 0x00157A50
	private BattleHUD.AbilityStatus GetAbilityState (int abilId) {
		BattleHUD.AbilityPlayerDetail abilityPlayerDetail = this.abilityDetailDict[this.currentPlayerId];
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
		AA_DATA aa_DATA = FF9StateSystem.Battle.FF9Battle.aa_data[abilId];
		bool flag = abilityPlayerDetail.HasAp && !abilityPlayerDetail.AbilityEquipList.ContainsKey (abilId);
		if (flag) {
			bool flag2 = !abilityPlayerDetail.AbilityPaList.ContainsKey (abilId);
			if (flag2) {
				return BattleHUD.AbilityStatus.ABILSTAT_NONE;
			}
			int num = abilityPlayerDetail.AbilityPaList[abilId];
			int num2 = abilityPlayerDetail.AbilityMaxPaList[abilId];
			bool flag3 = num < num2;
			if (flag3) {
				return BattleHUD.AbilityStatus.ABILSTAT_NONE;
			}
		}
		bool flag4 = (aa_DATA.Category & 2) != 0 && (btl_stat.CheckStatus (btl_DATA, 8u) || FF9StateSystem.Battle.FF9Battle.btl_scene.Info.NoMagical > 0);
		BattleHUD.AbilityStatus result;
		if (flag4) {
			result = BattleHUD.AbilityStatus.ABILSTAT_DISABLE;
		} else {
			bool flag5 = this.GetMp (aa_DATA) > (int) btl_DATA.cur.mp;
			if (flag5) {
				result = BattleHUD.AbilityStatus.ABILSTAT_DISABLE;
			} else {
				result = BattleHUD.AbilityStatus.ABILSTAT_ENABLE;
			}
		}
		return result;
	}

	// Token: 0x0600156A RID: 5482 RVA: 0x00159970 File Offset: 0x00157B70
	private void SetAbilityAp (BattleHUD.AbilityPlayerDetail abilityPlayer) {
		PLAYER player = abilityPlayer.Player;
		bool hasAp = abilityPlayer.HasAp;
		checked {
			if (hasAp) {
				PA_DATA[] array = ff9abil._FF9Abil_PaData[(int) player.info.menu_type];
				for (int i = 0; i < 192; i++) {
					int num;
					bool flag = 0 <= (num = ff9abil.FF9Abil_GetIndex ((int) player.info.slot_no, i));
					if (flag) {
						abilityPlayer.AbilityPaList[i] = (int) player.pa[num];
						abilityPlayer.AbilityMaxPaList[i] = (int) array[num].max_ap;
					}
				}
			}
		}
	}

	// Token: 0x0600156B RID: 5483 RVA: 0x00159A0C File Offset: 0x00157C0C
	private void SetAbilityEquip (BattleHUD.AbilityPlayerDetail abilityPlayer) {
		PLAYER player = abilityPlayer.Player;
		checked {
			for (int i = 0; i < 5; i++) {
				int num = (int) player.equip[i];
				bool flag = num != 255;
				if (flag) {
					FF9ITEM_DATA ff9ITEM_DATA = ff9item._FF9Item_Data[num];
					for (int j = 0; j < 3; j++) {
						int num2 = (int) ff9ITEM_DATA.ability[j];
						bool flag2 = num2 != 0 && 192 > num2;
						if (flag2) {
							abilityPlayer.AbilityEquipList[num2] = true;
						}
					}
				}
			}
		}
	}

	// Token: 0x0600156C RID: 5484 RVA: 0x00159AA8 File Offset: 0x00157CA8
	private void SetAbilityTrance (BattleHUD.AbilityPlayerDetail abilityPlayer) {
		PLAYER player = abilityPlayer.Player;
		int menu_type = (int) player.info.menu_type;
		bool flag = !ff9abil.FF9Abil_HasAp (player);
		checked {
			if (!flag) {
				bool flag2 = rdata._FF9BMenu_MenuTrance[menu_type, 2] != 1 && rdata._FF9BMenu_MenuTrance[menu_type, 2] != 2;
				if (!flag2) {
					int num = (int) (rdata._FF9BMenu_MenuTrance[menu_type, 2] - 1);
					rdata.FF9COMMAND ff9COMMAND = rdata._FF9BMenu_ComData[(int) rdata._FF9BMenu_MenuNormal[menu_type, num]];
					rdata.FF9COMMAND ff9COMMAND2 = rdata._FF9BMenu_ComData[(int) rdata._FF9BMenu_MenuTrance[menu_type, num]];
					PA_DATA[] array = ff9abil._FF9Abil_PaData[menu_type];
					for (int i = 0; i < (int) ff9COMMAND.count; i++) {
						int num2 = rdata._FF9BMenu_ComAbil[(int) ff9COMMAND.ability + i];
						int num3 = rdata._FF9BMenu_ComAbil[(int) ff9COMMAND2.ability + i];
						bool flag3 = num2 != num3;
						if (flag3) {
							abilityPlayer.AbilityPaList[num3] = abilityPlayer.AbilityPaList[num2];
							abilityPlayer.AbilityMaxPaList[num3] = abilityPlayer.AbilityMaxPaList[num2];
							bool flag4 = abilityPlayer.AbilityEquipList.ContainsKey (num2);
							if (flag4) {
								abilityPlayer.AbilityEquipList[num3] = abilityPlayer.AbilityEquipList[num2];
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x0600156D RID: 5485 RVA: 0x00159C10 File Offset: 0x00157E10
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
		bool flag = player.info.slot_no != 3;
		checked {
			if (!flag) {
				for (int i = 0; i < (int) ff9COMMAND.count; i++) {
					int key = rdata._FF9FAbil_ComAbil[(int) ff9COMMAND.ability + i];
					int num;
					bool flag2 = 0 <= (num = ff9abil.FF9Abil_GetIndex (1, array2[i]));
					if (flag2) {
						abilityPlayer.AbilityPaList[key] = (int) player2.pa[num];
						abilityPlayer.AbilityMaxPaList[key] = (int) array[num].max_ap;
					}
				}
				for (int j = 0; j < 5; j++) {
					int num2 = (int) player2.equip[j];
					bool flag3 = num2 != 255;
					if (flag3) {
						FF9ITEM_DATA ff9ITEM_DATA = ff9item._FF9Item_Data[num2];
						for (int k = 0; k < 3; k++) {
							int num3 = (int) ff9ITEM_DATA.ability[k];
							bool flag4 = num3 != 0 && 192 > num3;
							if (flag4) {
								for (int l = 0; l < (int) ff9COMMAND.count; l++) {
									bool flag5 = num3 == array2[l];
									if (flag5) {
										int key2 = rdata._FF9FAbil_ComAbil[(int) ff9COMMAND.ability + l];
										abilityPlayer.AbilityEquipList[key2] = true;
									}
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x1700022C RID: 556
	// (get) Token: 0x0600156E RID: 5486 RVA: 0x00159DCC File Offset: 0x00157FCC
	private int currentPlayerIndex {
		get {
			return this.matchBattleIdPlayerList.IndexOf (this.currentPlayerId);
		}
	}

	// Token: 0x1700022D RID: 557
	// (get) Token: 0x0600156F RID: 5487 RVA: 0x00159DF0 File Offset: 0x00157FF0
	public GameObject PlayerTargetPanel {
		get {
			return this.TargetPanel.GetChild (0);
		}
	}

	// Token: 0x1700022E RID: 558
	// (get) Token: 0x06001570 RID: 5488 RVA: 0x00159E10 File Offset: 0x00158010
	public GameObject EnemyTargetPanel {
		get {
			return this.TargetPanel.GetChild (1);
		}
	}

	// Token: 0x1700022F RID: 559
	// (get) Token: 0x06001571 RID: 5489 RVA: 0x00159E30 File Offset: 0x00158030
	public List<int> ReadyQueue {
		get {
			return this.readyQueue;
		}
	}

	// Token: 0x17000230 RID: 560
	// (get) Token: 0x06001572 RID: 5490 RVA: 0x00159E48 File Offset: 0x00158048
	public List<int> InputFinishList {
		get {
			return this.inputFinishedList;
		}
	}

	// Token: 0x17000231 RID: 561
	// (get) Token: 0x06001573 RID: 5491 RVA: 0x00159E60 File Offset: 0x00158060
	public int CurrentPlayerIndex {
		get {
			return this.currentPlayerId;
		}
	}

	// Token: 0x17000232 RID: 562
	// (get) Token: 0x06001574 RID: 5492 RVA: 0x00159E78 File Offset: 0x00158078
	public bool IsDoubleCast {
		get {
			return this.currentCommandId == 23u || this.currentCommandId == 21u;
		}
	}

	public UIRect UiRect { get => uiRect; set => uiRect = value; }
	public UIRoot UiRoot { get => uiRoot; set => uiRoot = value; }
	public static float Scale { get => scale; set => scale = value; }
	public static int BattleSwirl { get => battleSwirl; set => battleSwirl = value; }
	public static float Countdown { get => countdown; set => countdown = value; }
	public static int EncounterRate { get => encounterRate; set => encounterRate = value; }
	public static int HD { get => hD; set => hD = value; }
	public static int BattleSpeed { get => battleSpeed; set => battleSpeed = value; }

	// Token: 0x06001575 RID: 5493 RVA: 0x00159EA4 File Offset: 0x001580A4
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
				this.FF9BMenu_EnableMenu (true);
				ButtonGroupState.ActiveGroup = this.currentButtonGroup;
				this.DisplayTargetPointer ();
			}
		}
		this.isFromPause = false;
		this.oneTime = true;
	}

	// Token: 0x06001576 RID: 5494 RVA: 0x00159F90 File Offset: 0x00158190
	public override void Hide (UIScene.SceneVoidDelegate afterFinished = null) {
		base.Hide (afterFinished);
		this.PauseButtonGameObject.SetActive (false);
		this.HelpButtonGameObject.SetActive (false);
		bool flag = !this.isFromPause;
		if (flag) {
			this.RemoveCursorMemorize ();
		}
	}

	// Token: 0x06001577 RID: 5495 RVA: 0x00159FD8 File Offset: 0x001581D8
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

	// Token: 0x06001578 RID: 5496 RVA: 0x0015A044 File Offset: 0x00158244
	public override bool OnKeyConfirm (GameObject go) {
		bool flag = base.OnKeyConfirm (go) && !this.hidingHud;
		checked {
			if (flag) {
				bool flag2 = ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton;
				if (flag2) {
					FF9Sfx.FF9SFX_Play (103);
					int siblingIndex = go.transform.GetSiblingIndex ();
					this.currentCommandIndex = (BattleHUD.CommandMenu) siblingIndex;
					this.currentCommandId = (uint) this.GetCommandFromCommandIndex (this.currentCommandIndex, this.currentPlayerId);
					this.commandCursorMemorize[this.currentPlayerId] = this.currentCommandIndex;
					this.subMenuType = BattleHUD.SubMenuType.CommandNormal;
					bool flag3 = this.IsDoubleCast && this.doubleCastCount < 2;
					if (flag3) {
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
								BattleHUD.CommandMenu commandMenu = this.currentCommandIndex;
								rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) this.currentCommandId))];
								bool flag4 = ff9COMMAND.type == 0;
								if (flag4) {
									this.subMenuType = BattleHUD.SubMenuType.CommandNormal;
									this.SetCommandVisibility (false, false);
									this.SetTargetVisibility (true);
								} else {
									bool flag5 = ff9COMMAND.type == 1;
									if (flag5) {
										this.subMenuType = BattleHUD.SubMenuType.CommandAbility;
										this.DisplayAbility ();
										this.SetCommandVisibility (false, false);
										this.SetAbilityPanelVisibility (true, false);
									} else {
										bool flag6 = ff9COMMAND.type == 3;
										if (flag6) {
											this.subMenuType = BattleHUD.SubMenuType.CommandThrow;
											this.DisplayItem (true);
											this.SetCommandVisibility (false, false);
											this.SetItemPanelVisibility (true, false);
										}
									}
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
				} else {
					bool flag7 = ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton;
					if (flag7) {
						FF9Sfx.FF9SFX_Play (103);
						bool flag8 = this.cursorType == BattleHUD.CursorGroup.Individual;
						if (flag8) {
							int num = this.targetHudList.IndexOf (this.targetHudList.Single ((BattleHUD.TargetHUD hud) => hud.Self == go));
							bool flag9 = num < HonoluluBattleMain.EnemyStartIndex;
							if (flag9) {
								bool flag10 = num < this.matchBattleIdPlayerList.Count;
								if (flag10) {
									int battleIndex = this.matchBattleIdPlayerList[num];
									this.CheckDoubleCast (battleIndex, this.cursorType);
								}
							} else {
								bool flag11 = num - HonoluluBattleMain.EnemyStartIndex < this.matchBattleIdEnemyList.Count;
								if (flag11) {
									int battleIndex2 = this.matchBattleIdEnemyList[num - HonoluluBattleMain.EnemyStartIndex];
									this.CheckDoubleCast (battleIndex2, this.cursorType);
								}
							}
						} else {
							bool flag12 = this.cursorType == BattleHUD.CursorGroup.AllPlayer || this.cursorType == BattleHUD.CursorGroup.AllEnemy || this.cursorType == BattleHUD.CursorGroup.All;
							if (flag12) {
								this.CheckDoubleCast (-1, this.cursorType);
							}
						}
					} else {
						bool flag13 = ButtonGroupState.ActiveGroup == BattleHUD.AbilityGroupButton;
						if (flag13) {
							bool flag14 = this.CheckAbilityStatus (go.GetComponent<RecycleListItem> ().ItemDataIndex) == BattleHUD.AbilityStatus.ABILSTAT_ENABLE;
							if (flag14) {
								FF9Sfx.FF9SFX_Play (103);
								this.currentSubMenuIndex = go.GetComponent<RecycleListItem> ().ItemDataIndex;
								bool flag15 = this.currentCommandIndex == BattleHUD.CommandMenu.Ability1;
								if (flag15) {
									this.ability1CursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
								} else {
									this.ability2CursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
								}
								this.SetAbilityPanelVisibility (false, false);
								this.SetTargetVisibility (true);
							} else {
								FF9Sfx.FF9SFX_Play (102);
							}
						} else {
							bool flag16 = ButtonGroupState.ActiveGroup == BattleHUD.ItemGroupButton;
							if (flag16) {
								bool flag17 = this.itemIdList[this.currentSubMenuIndex] != 255;
								if (flag17) {
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
					}
				}
			}
			return true;
		}
	}

	// Token: 0x06001579 RID: 5497 RVA: 0x0015A4F8 File Offset: 0x001586F8
	public override bool OnKeyCancel (GameObject go) {
		bool key = UIManager.Input.GetKey (Control.Special);
		checked {
			bool result;
			if (key) {
				result = true;
			} else {
				bool flag = base.OnKeyCancel (go) && !this.hidingHud && !(ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton);
				if (flag) {
					bool flag2 = ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton;
					if (flag2) {
						FF9Sfx.FF9SFX_Play (101);
						this.SetTargetVisibility (false);
						this.ClearModelPointer ();
						switch (this.currentCommandIndex) {
							case BattleHUD.CommandMenu.Attack:
								this.SetCommandVisibility (true, true);
								break;
							case BattleHUD.CommandMenu.Ability1:
							case BattleHUD.CommandMenu.Ability2:
								{
									bool flag3 = this.subMenuType == BattleHUD.SubMenuType.CommandAbility;
									if (flag3) {
										this.SetAbilityPanelVisibility (true, true);
									} else {
										bool flag4 = this.subMenuType == BattleHUD.SubMenuType.CommandThrow;
										if (flag4) {
											this.SetItemPanelVisibility (true, true);
										} else {
											this.SetCommandVisibility (true, true);
										}
									}
									break;
								}
							case BattleHUD.CommandMenu.Item:
								this.SetItemPanelVisibility (true, true);
								break;
						}
					} else {
						bool flag5 = ButtonGroupState.ActiveGroup == BattleHUD.AbilityGroupButton;
						if (flag5) {
							FF9Sfx.FF9SFX_Play (101);
							bool flag6 = this.IsDoubleCast && this.doubleCastCount > 0;
							if (flag6) {
								this.doubleCastCount -= 1;
							}
							bool flag7 = this.doubleCastCount == 0;
							if (flag7) {
								this.SetAbilityPanelVisibility (false, false);
								this.SetCommandVisibility (true, true);
							} else {
								this.SetAbilityPanelVisibility (true, false);
							}
						} else {
							bool flag8 = ButtonGroupState.ActiveGroup == BattleHUD.ItemGroupButton;
							if (flag8) {
								FF9Sfx.FF9SFX_Play (101);
								this.SetItemPanelVisibility (false, false);
								this.SetCommandVisibility (true, true);
							} else {
								bool flag9 = ButtonGroupState.ActiveGroup == string.Empty && UIManager.Input.ContainsAndroidQuitKey ();
								if (flag9) {
									this.OnKeyQuit ();
								}
							}
						}
					}
				}
				result = true;
			}
			return result;
		}
	}

	// Token: 0x0600157A RID: 5498 RVA: 0x0015A6E4 File Offset: 0x001588E4
	public override bool OnKeyMenu (GameObject go) {
		bool flag = base.OnKeyMenu (go) && !this.hidingHud && ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton;
		if (flag) {
			bool flag2 = this.readyQueue.Count > 1;
			if (flag2) {
				int item = this.readyQueue[0];
				this.readyQueue.RemoveAt (0);
				this.readyQueue.Add (item);
				using (List<int>.Enumerator enumerator = this.readyQueue.GetEnumerator ()) {
					while (enumerator.MoveNext ()) {
					int num = enumerator.Current;
					bool flag3 = !this.inputFinishedList.Contains (num) && !this.unconsciousStateList.Contains (num) && num != this.currentPlayerId;
					if (flag3) {
					bool flag4 = this.readyQueue.IndexOf (num) > 0;
					if (flag4) {
					this.readyQueue.Remove (num);
					this.readyQueue.Insert (0, num);
							}
							this.SwitchPlayer (num);
							break;
						}
					}
					return true;
				}
			}
			bool flag5 = this.readyQueue.Count == 1;
			if (flag5) {
				this.SwitchPlayer (this.readyQueue[0]);
			}
		}
		return true;
	}

	// Token: 0x0600157B RID: 5499 RVA: 0x0015A854 File Offset: 0x00158A54
	public override bool OnKeyPause (GameObject go) {
		bool flag = base.OnKeyPause (go) && FF9StateSystem.Battle.FF9Battle.btl_seq != 2 && FF9StateSystem.Battle.FF9Battle.btl_seq != 1;
		if (flag) {
			base.NextSceneIsModal = true;
			this.isFromPause = true;
			this.beforePauseCommandEnable = this.commandEnable;
			this.currentButtonGroup = ((!this.hidingHud) ? ButtonGroupState.ActiveGroup : this.currentButtonGroup);
			this.FF9BMenu_EnableMenu (false);
			Singleton<HUDMessage>.Instance.Pause (true);
			this.Hide (delegate {
				PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.Pause);
			});
		}
		return base.OnKeyPause (go);
	}

	// Token: 0x0600157C RID: 5500 RVA: 0x0015A918 File Offset: 0x00158B18
	public override void OnKeyQuit () {
		bool flag = !base.Loading && FF9StateSystem.Battle.FF9Battle.btl_seq != 2 && FF9StateSystem.Battle.FF9Battle.btl_seq != 1;
		if (flag) {
			this.beforePauseCommandEnable = this.commandEnable;
			this.currentButtonGroup = ButtonGroupState.ActiveGroup;
			this.FF9BMenu_EnableMenu (false);
			base.ShowQuitUI (this.onResumeFromQuit);
		}
	}

	// Token: 0x0600157D RID: 5501 RVA: 0x0015A98C File Offset: 0x00158B8C
	public override bool OnKeyLeftBumper (GameObject go) {
		bool flag = base.OnKeyLeftBumper (go) && !this.hidingHud && ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton && (this.targetCursor == 3 || this.targetCursor == 5 || this.targetCursor == 4);
		if (flag) {
			FF9Sfx.FF9SFX_Play (103);
			this.isAllTarget = !this.isAllTarget;
			this.allTargetToggle.value = this.isAllTarget;
			this.allTargetButtonComponent.SetState (UIButtonColor.State.Normal, false);
			this.ToggleAllTarget ();
		}
		return true;
	}

	// Token: 0x0600157E RID: 5502 RVA: 0x0015AA28 File Offset: 0x00158C28
	public override bool OnKeyRightBumper (GameObject go) {
		bool flag = base.OnKeyRightBumper (go) && !this.hidingHud && ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton && (this.targetCursor == 3 || this.targetCursor == 5 || this.targetCursor == 4);
		if (flag) {
			FF9Sfx.FF9SFX_Play (103);
			this.isAllTarget = !this.isAllTarget;
			this.allTargetToggle.value = this.isAllTarget;
			this.allTargetButtonComponent.SetState (UIButtonColor.State.Normal, false);
			this.ToggleAllTarget ();
		}
		return true;
	}

	// Token: 0x0600157F RID: 5503 RVA: 0x0015AAC4 File Offset: 0x00158CC4
	public override bool OnKeyRightTrigger (GameObject go) {
		bool flag = base.OnKeyRightTrigger (go) && !this.hidingHud && !this.AndroidTVOnKeyRightTrigger (go);
		if (flag) {
			this.ProcessAutoBattleInput ();
		}
		return true;
	}

	// Token: 0x06001580 RID: 5504 RVA: 0x0015AB04 File Offset: 0x00158D04
	public override bool OnItemSelect (GameObject go) {
		bool flag = base.OnItemSelect (go);
		checked {
			if (flag) {
				bool flag2 = ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton;
				if (flag2) {
					int siblingIndex = go.transform.GetSiblingIndex ();
					bool flag3 = siblingIndex != (int) this.currentCommandIndex;
					if (flag3) {
						this.currentCommandIndex = (BattleHUD.CommandMenu) siblingIndex;
					}
				} else {
					bool flag4 = ButtonGroupState.ActiveGroup == BattleHUD.AbilityGroupButton || ButtonGroupState.ActiveGroup == BattleHUD.ItemGroupButton;
					if (flag4) {
						this.currentSubMenuIndex = go.GetComponent<RecycleListItem> ().ItemDataIndex;
					}
				}
				bool flag5 = ButtonGroupState.ActiveGroup == BattleHUD.TargetGroupButton;
				if (flag5) {
					bool flag6 = go.transform.parent == this.modelButtonManager.transform;
					if (flag6) {
						bool flag7 = this.cursorType == BattleHUD.CursorGroup.Individual;
						if (flag7) {
							int index = go.GetComponent<ModelButton> ().index;
							bool flag8 = index < HonoluluBattleMain.EnemyStartIndex;
							int num;
							if (flag8) {
								num = this.matchBattleIdPlayerList.IndexOf (index);
							} else {
								num = this.matchBattleIdEnemyList.IndexOf (index) + 4;
							}
							bool flag9 = num != -1;
							if (flag9) {
								BattleHUD.TargetHUD targetHUD = this.targetHudList[num];
								bool enabled = targetHUD.ButtonGroup.enabled;
								if (enabled) {
									ButtonGroupState.ActiveButton = targetHUD.Self;
								}
							}
						}
					} else {
						bool flag10 = go.transform.parent.parent == this.TargetPanel.transform && this.cursorType == BattleHUD.CursorGroup.Individual;
						if (flag10) {
							int num2 = go.transform.GetSiblingIndex ();
							bool flag11 = go.GetParent ().transform.GetSiblingIndex () == 1;
							if (flag11) {
								num2 += HonoluluBattleMain.EnemyStartIndex;
							}
							bool flag12 = this.currentTargetIndex != num2;
							if (flag12) {
								this.currentTargetIndex = num2;
								this.DisplayTargetPointer ();
							}
						}
					}
				}
			}
			return true;
		}
	}

	// Token: 0x06001581 RID: 5505 RVA: 0x0015AD0C File Offset: 0x00158F0C
	private void OnAllTargetHover (GameObject go, bool isHover) {
		bool flag = isHover && (this.cursorType == BattleHUD.CursorGroup.AllEnemy || this.cursorType == BattleHUD.CursorGroup.AllPlayer);
		if (flag) {
			bool flag2 = go == this.allPlayerButton;
			if (flag2) {
				bool flag3 = this.cursorType != BattleHUD.CursorGroup.AllPlayer;
				if (flag3) {
					FF9Sfx.FF9SFX_Play (103);
					this.cursorType = BattleHUD.CursorGroup.AllPlayer;
					this.DisplayTargetPointer ();
				}
			} else {
				bool flag4 = go == this.allEnemyButton && this.cursorType != BattleHUD.CursorGroup.AllEnemy;
				if (flag4) {
					FF9Sfx.FF9SFX_Play (103);
					this.cursorType = BattleHUD.CursorGroup.AllEnemy;
					this.DisplayTargetPointer ();
				}
			}
		}
	}

	// Token: 0x06001582 RID: 5506 RVA: 0x0015ADB4 File Offset: 0x00158FB4
	private void OnTargetNavigate (GameObject go, KeyCode key) {
		bool flag = this.cursorType == BattleHUD.CursorGroup.AllEnemy;
		if (flag) {
			bool flag2 = this.targetCursor == 3 && key == KeyCode.RightArrow;
			if (flag2) {
				FF9Sfx.FF9SFX_Play (103);
				this.cursorType = BattleHUD.CursorGroup.AllPlayer;
				this.DisplayTargetPointer ();
			}
		} else {
			bool flag3 = this.cursorType == BattleHUD.CursorGroup.AllPlayer && this.targetCursor == 3 && key == KeyCode.LeftArrow;
			if (flag3) {
				FF9Sfx.FF9SFX_Play (103);
				this.cursorType = BattleHUD.CursorGroup.AllEnemy;
				this.DisplayTargetPointer ();
			}
		}
	}

	// Token: 0x06001583 RID: 5507 RVA: 0x0015AE40 File Offset: 0x00159040
	private void OnAllTargetClick (GameObject go) {
		bool flag = this.cursorType == BattleHUD.CursorGroup.All;
		if (flag) {
			FF9Sfx.FF9SFX_Play (103);
			this.CheckDoubleCast (-1, this.cursorType);
		} else {
			bool flag2 = UICamera.currentTouchID == 0 || UICamera.currentTouchID == 1;
			if (flag2) {
				FF9Sfx.FF9SFX_Play (103);
				bool flag3 = go == this.allPlayerButton;
				if (flag3) {
					bool flag4 = this.cursorType == BattleHUD.CursorGroup.AllPlayer;
					if (flag4) {
						this.CheckDoubleCast (-1, this.cursorType);
					} else {
						this.OnTargetNavigate (go, KeyCode.RightArrow);
					}
				} else {
					bool flag5 = go == this.allEnemyButton;
					if (flag5) {
						bool flag6 = this.cursorType == BattleHUD.CursorGroup.AllEnemy;
						if (flag6) {
							this.CheckDoubleCast (-1, this.cursorType);
						} else {
							this.OnTargetNavigate (go, KeyCode.LeftArrow);
						}
					}
				}
			} else {
				bool flag7 = UICamera.currentTouchID == -1;
				if (flag7) {
					FF9Sfx.FF9SFX_Play (103);
					bool flag8 = go == this.allPlayerButton;
					if (flag8) {
						this.cursorType = BattleHUD.CursorGroup.AllPlayer;
					} else {
						bool flag9 = go == this.allEnemyButton;
						if (flag9) {
							this.cursorType = BattleHUD.CursorGroup.AllEnemy;
						}
					}
					this.CheckDoubleCast (-1, this.cursorType);
				}
			}
		}
	}

	// Token: 0x06001584 RID: 5508 RVA: 0x0015AF80 File Offset: 0x00159180
	private void onPartyDetailClick (GameObject go) {
		bool flag = go.GetParent () == this.PartyDetailPanel.GetChild (0);
		if (flag) {
			int siblingIndex = go.transform.GetSiblingIndex ();
			int playerId = this.playerDetailPanelList[siblingIndex].PlayerId;
			bool flag2 = this.readyQueue.Contains (playerId) && !this.inputFinishedList.Contains (playerId) && !this.unconsciousStateList.Contains (playerId) && playerId != this.currentPlayerId;
			if (flag2) {
				this.SwitchPlayer (playerId);
			}
		} else {
			base.onClick (go);
		}
	}

	// Token: 0x06001585 RID: 5509 RVA: 0x000221AF File Offset: 0x000203AF
	private void OnRunPress (GameObject go, bool isDown) {
		this.runCounter = 0f;
		this.isTryingToRun = isDown;
	}

	// Token: 0x06001586 RID: 5510 RVA: 0x0015B020 File Offset: 0x00159220
	private bool OnAllTargetToggleValidate (bool choice) {
		bool flag = this.isAllTarget != this.allTargetToggle.value;
		bool result;
		if (flag) {
			result = true;
		} else {
			this.allTargetButtonComponent.SetState (UIButtonColor.State.Normal, false);
			result = false;
		}
		return result;
	}

	// Token: 0x06001587 RID: 5511 RVA: 0x0015B060 File Offset: 0x00159260
	private bool OnAutoToggleValidate (bool choice) {
		bool flag = this.isAutoAttack != this.autoBattleToggle.value;
		bool result;
		if (flag) {
			result = true;
		} else {
			this.autoBattleButtonComponent.SetState (UIButtonColor.State.Normal, false);
			result = false;
		}
		return result;
	}

	// Token: 0x06001588 RID: 5512 RVA: 0x0015B0A0 File Offset: 0x001592A0
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
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				int num = 0;
				while (1 << num != (int) next.btl_id) {
					num++;
				}
				bool flag = next.bi.target > 0;
				if (flag) {
					bool flag2 = next.bi.player > 0;
					if (flag2) {
						this.matchBattleIdPlayerList.Add (num);
					} else {
						this.matchBattleIdEnemyList.Add (num);
					}
				}
			}
			int num2 = 0;
			foreach (int num3 in this.matchBattleIdPlayerList) {
				bool flag3 = num2 != num3;
				if (flag3) {
					global::Debug.LogWarning ("This Battle, player index and id not the same. Please be careful.");
					break;
				}
				num2++;
			}
		}
	}

	// Token: 0x06001589 RID: 5513 RVA: 0x0015B344 File Offset: 0x00159544
	public void GoToBattleResult () {
		bool flag = this.oneTime;
		if (flag) {
			this.oneTime = false;
			Application.targetFrameRate = 60;
			this.UiRoot.scalingStyle = UIRoot.Scaling.Constrained;
			this.UiRoot.minimumHeight = Mathf.RoundToInt ((float) Screen.currentResolution.height);
			this.Hide (delegate {
				PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.BattleResult);
			});
		}
	}

	// Token: 0x0600158A RID: 5514 RVA: 0x0015B3C0 File Offset: 0x001595C0
	public void GoToGameOver () {
		bool flag = this.oneTime;
		if (flag) {
			this.oneTime = false;
			Application.targetFrameRate = 60;
			this.UiRoot.scalingStyle = UIRoot.Scaling.Constrained;
			this.UiRoot.minimumHeight = Mathf.RoundToInt ((float) Screen.currentResolution.height);
			this.Hide (delegate {
				PersistenSingleton<UIManager>.Instance.ChangeUIState (UIManager.UIState.GameOver);
			});
		}
	}

	// Token: 0x0600158B RID: 5515 RVA: 0x0015B43C File Offset: 0x0015963C
	private void SendAutoAttackCommand (int playerIndex) {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[playerIndex];
		CMD_DATA cmd_DATA = btl_DATA.cmd[0];
		bool flag = cmd_DATA == null || !btl_cmd.CheckUsingCommand (cmd_DATA);
		if (flag) {
			this.currentPlayerId = playerIndex;
			this.currentCommandIndex = BattleHUD.CommandMenu.Attack;
			BTL_DATA firstEnemyPtr = this.GetFirstEnemyPtr ();
			btl_cmd.SetCommand (btl_DATA.cmd[0], 1u, 176u, firstEnemyPtr.btl_id, 0u);
			this.inputFinishedList.Add (this.currentPlayerId);
			this.currentPlayerId = -1;
		}
	}

	// Token: 0x0600158C RID: 5516 RVA: 0x0015B4C4 File Offset: 0x001596C4
	private BattleHUD.CommandDetail ProcessCommand (int target, BattleHUD.CursorGroup cursor) {
		BattleHUD.CommandDetail commandDetail = new BattleHUD.CommandDetail ();
		commandDetail.CommandId = this.currentCommandId;
		commandDetail.SubId = 0u;
		checked {
			int type = (int) rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) commandDetail.CommandId))].type;
			bool flag = type == 0;
			if (flag) {
				commandDetail.SubId = (uint) rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) commandDetail.CommandId))].ability;
			}
			bool flag2 = type == 1;
			if (flag2) {
				int num = (int) rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) commandDetail.CommandId))].ability;
				commandDetail.SubId = (uint) this.PatchAbility (rdata._FF9BMenu_ComAbil[num + this.currentSubMenuIndex]);
			} else {
				bool flag3 = type == 2 || type == 3;
				if (flag3) {
					int num2 = this.itemIdList[this.currentSubMenuIndex];
					commandDetail.SubId = (uint) num2;
				}
			}
			commandDetail.TargetId = 0;
			bool flag4 = cursor == BattleHUD.CursorGroup.Individual;
			if (flag4) {
				commandDetail.TargetId = (ushort) (1 << target);
			} else {
				bool flag5 = cursor == BattleHUD.CursorGroup.AllPlayer;
				if (flag5) {
					commandDetail.TargetId = 15;
				} else {
					bool flag6 = cursor == BattleHUD.CursorGroup.AllEnemy;
					if (flag6) {
						commandDetail.TargetId = 240;
					} else {
						bool flag7 = cursor == BattleHUD.CursorGroup.All;
						if (flag7) {
							commandDetail.TargetId = 255;
						}
					}
				}
			}
			commandDetail.TargetType = (uint) this.GetSelectMode (cursor);
			return commandDetail;
		}
	}

	// Token: 0x0600158D RID: 5517 RVA: 0x0015B62C File Offset: 0x0015982C
	private void SendCommand (BattleHUD.CommandDetail command) {
		CMD_DATA cmd_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId].cmd[0];
		cmd_DATA.regist.sel_mode = 1;
		btl_cmd.SetCommand (cmd_DATA, command.CommandId, command.SubId, command.TargetId, command.TargetType);
		this.SetPartySwapButtonActive (false);
		this.inputFinishedList.Add (this.currentPlayerId);
		BattleHUD.PlayerDetailHUD playerDetailHUD = this.playerDetailPanelList.Find ((BattleHUD.PlayerDetailHUD hud) => hud.PlayerId == this.currentPlayerId);
		bool flag = playerDetailHUD != null;
		if (flag) {
			playerDetailHUD.ATBBlink = false;
			playerDetailHUD.TranceBlink = false;
		}
	}

	// Token: 0x0600158E RID: 5518 RVA: 0x0015B6D0 File Offset: 0x001598D0
	private void SendDoubleCastCommand (BattleHUD.CommandDetail firstCommand, BattleHUD.CommandDetail secondCommand) {
		CMD_DATA cmd_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId].cmd[3];
		cmd_DATA.regist.sel_mode = 1;
		btl_cmd.SetCommand (cmd_DATA, firstCommand.CommandId, firstCommand.SubId, firstCommand.TargetId, firstCommand.TargetType);
		btl_cmd.SetCommand (FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId].cmd[0], secondCommand.CommandId, secondCommand.SubId, secondCommand.TargetId, secondCommand.TargetType);
		this.SetPartySwapButtonActive (false);
		this.inputFinishedList.Add (this.currentPlayerId);
		BattleHUD.PlayerDetailHUD playerDetailHUD = this.playerDetailPanelList.Find ((BattleHUD.PlayerDetailHUD hud) => hud.PlayerId == this.currentPlayerId);
		bool flag = playerDetailHUD != null;
		if (flag) {
			playerDetailHUD.ATBBlink = false;
			playerDetailHUD.TranceBlink = false;
		}
	}

	// Token: 0x0600158F RID: 5519 RVA: 0x0015B7B0 File Offset: 0x001599B0
	private command_tags GetCommandFromCommandIndex (BattleHUD.CommandMenu commandIndex, int playerIndex) {
		BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[playerIndex];
		int menu_type = (int) FF9StateSystem.Common.FF9.party.member[(int) btl_DATA.bi.line_no].info.menu_type;
		command_tags result;
		switch (commandIndex) {
			case BattleHUD.CommandMenu.Attack:
				result = command_tags.CMD_ATTACK;
				break;
			case BattleHUD.CommandMenu.Defend:
				result = command_tags.CMD_DEFEND;
				break;
			case BattleHUD.CommandMenu.Ability1:
				{
					bool flag = Status.checkCurStat (btl_DATA, 16384u);
					if (flag) {
						result = (command_tags) rdata._FF9BMenu_MenuTrance[menu_type, 0];
					} else {
						result = rdata._FF9BMenu_MenuNormal[menu_type, 0];
					}
					break;
				}
			case BattleHUD.CommandMenu.Ability2:
				{
					bool flag2 = Status.checkCurStat (btl_DATA, 16384u);
					if (flag2) {
						result = (command_tags) rdata._FF9BMenu_MenuTrance[menu_type, 1];
					} else {
						result = rdata._FF9BMenu_MenuNormal[menu_type, 1];
					}
					break;
				}
			case BattleHUD.CommandMenu.Item:
				result = command_tags.CMD_ITEM;
				break;
			case BattleHUD.CommandMenu.Change:
				result = command_tags.CMD_CHANGE;
				break;
			default:
				result = command_tags.CMD_NONE;
				break;
		}
		return result;
	}

	// Token: 0x06001590 RID: 5520 RVA: 0x0015B894 File Offset: 0x00159A94
	private void SetCommandVisibility (bool isVisible, bool forceCursorMemo) {
		GameObject gameObject = this.commandDetailHUD.Attack;
		this.SetPartySwapButtonActive (isVisible);
		this.BackButton.SetActive (!isVisible && FF9StateSystem.MobilePlatform);
		bool flag = !isVisible;
		if (flag) {
			this.commandCursorMemorize[this.currentPlayerId] = this.currentCommandIndex;
			this.commandDetailHUD.Self.SetActive (false);
		} else {
			bool flag2 = !this.commandDetailHUD.Self.activeSelf;
			if (flag2) {
				this.commandDetailHUD.Self.SetActive (true);
				ButtonGroupState.RemoveCursorMemorize (BattleHUD.CommandGroupButton);
				bool flag3 = (this.commandCursorMemorize.ContainsKey (this.currentPlayerId) && FF9StateSystem.Settings.cfg.cursor > 0UL) || forceCursorMemo;
				if (flag3) {
					gameObject = this.commandDetailHUD.GetGameObjectFromCommand (this.commandCursorMemorize[this.currentPlayerId]);
				}
				bool enabled = gameObject.GetComponent<ButtonGroupState> ().enabled;
				if (enabled) {
					ButtonGroupState.SetCursorMemorize (gameObject, BattleHUD.CommandGroupButton);
				} else {
					ButtonGroupState.SetCursorMemorize (this.commandDetailHUD.Attack, BattleHUD.CommandGroupButton);
				}
			} else {
				bool flag4 = (this.commandCursorMemorize.ContainsKey (this.currentPlayerId) && FF9StateSystem.Settings.cfg.cursor > 0UL) || forceCursorMemo;
				if (flag4) {
					gameObject = this.commandDetailHUD.GetGameObjectFromCommand (this.commandCursorMemorize[this.currentPlayerId]);
				}
				bool enabled2 = gameObject.GetComponent<ButtonGroupState> ().enabled;
				if (enabled2) {
					ButtonGroupState.ActiveButton = gameObject;
				} else {
					ButtonGroupState.ActiveButton = this.commandDetailHUD.Attack;
				}
			}
			bool flag5 = !this.hidingHud;
			if (flag5) {
				ButtonGroupState.ActiveGroup = BattleHUD.CommandGroupButton;
			} else {
				this.currentButtonGroup = BattleHUD.CommandGroupButton;
			}
		}
	}

	// Token: 0x06001591 RID: 5521 RVA: 0x0015BA74 File Offset: 0x00159C74
	private void SetItemPanelVisibility (bool isVisible, bool forceCursorMemo) {
		if (isVisible) {
			this.ItemPanel.SetActive (true);
			ButtonGroupState.RemoveCursorMemorize (BattleHUD.ItemGroupButton);
			bool flag = (this.itemCursorMemorize.ContainsKey (this.currentPlayerId) && FF9StateSystem.Settings.cfg.cursor > 0UL) || forceCursorMemo;
			if (flag) {
				this.itemScrollList.Invoke ("RepositionList", 0.1f);
				this.itemScrollList.JumpToIndex (this.itemCursorMemorize[this.currentPlayerId], true);
			} else {
				this.itemScrollList.Invoke ("RepositionList", 0.1f);
				this.itemScrollList.JumpToIndex (0, false);
			}
			ButtonGroupState.RemoveCursorMemorize (BattleHUD.ItemGroupButton);
			ButtonGroupState.ActiveGroup = BattleHUD.ItemGroupButton;
		} else {
			bool flag2 = this.currentCommandIndex == BattleHUD.CommandMenu.Item && this.currentSubMenuIndex != -1;
			if (flag2) {
				this.itemCursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
			}
			this.ItemPanel.SetActive (false);
		}
	}

	// Token: 0x06001592 RID: 5522 RVA: 0x0015BB8C File Offset: 0x00159D8C
	private void SetAbilityPanelVisibility (bool isVisible, bool forceCursorMemo) {
		if (isVisible) {
			bool flag = !this.AbilityPanel.activeSelf;
			if (flag) {
				this.AbilityPanel.SetActive (true);
				Dictionary<int, int> dictionary = (this.currentCommandIndex != BattleHUD.CommandMenu.Ability1) ? this.ability2CursorMemorize : this.ability1CursorMemorize;
				ButtonGroupState.RemoveCursorMemorize (BattleHUD.AbilityGroupButton);
				bool flag2 = (dictionary.ContainsKey (this.currentPlayerId) && FF9StateSystem.Settings.cfg.cursor > 0UL) || forceCursorMemo;
				if (flag2) {
					this.abilityScrollList.Invoke ("RepositionList", 0.1f);
					this.abilityScrollList.JumpToIndex (dictionary[this.currentPlayerId], true);
				} else {
					this.abilityScrollList.Invoke ("RepositionList", 0.1f);
					this.abilityScrollList.JumpToIndex (0, true);
				}
			}
			bool flag3 = this.IsDoubleCast && this.doubleCastCount == 1;
			if (flag3) {
				ButtonGroupState.SetPointerNumberToGroup (1, BattleHUD.AbilityGroupButton);
			} else {
				bool flag4 = this.IsDoubleCast && this.doubleCastCount == 2;
				if (flag4) {
					ButtonGroupState.SetPointerNumberToGroup (2, BattleHUD.AbilityGroupButton);
				} else {
					ButtonGroupState.SetPointerNumberToGroup (0, BattleHUD.AbilityGroupButton);
				}
			}
			ButtonGroupState.ActiveGroup = BattleHUD.AbilityGroupButton;
			ButtonGroupState.UpdateActiveButton ();
		} else {
			bool flag5 = this.currentCommandIndex == BattleHUD.CommandMenu.Ability1 && this.currentSubMenuIndex != -1;
			if (flag5) {
				this.ability1CursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
			} else {
				bool flag6 = this.currentCommandIndex == BattleHUD.CommandMenu.Ability2 && this.currentSubMenuIndex != -1;
				if (flag6) {
					this.ability2CursorMemorize[this.currentPlayerId] = this.currentSubMenuIndex;
				}
			}
			this.AbilityPanel.SetActive (false);
		}
	}

	// Token: 0x06001593 RID: 5523 RVA: 0x0015BD64 File Offset: 0x00159F64
	private void SetTargetVisibility (bool isVisible) {
		checked {
			if (isVisible) {
				byte targetAvalability = 0;
				byte subMode = 0;
				this.defaultTargetCursor = 0;
				this.defaultTargetDead = 0;
				this.targetDead = 0;
				bool flag = this.currentCommandIndex == BattleHUD.CommandMenu.Ability1 || this.currentCommandIndex == BattleHUD.CommandMenu.Ability2;
				if (flag) {
					rdata.FF9COMMAND ff9COMMAND = rdata._FF9FAbil_ComData[(int) ((uint) ((UIntPtr) this.currentCommandId))];
					bool flag2 = ff9COMMAND.type == 1;
					int num;
					if (flag2) {
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
				} else {
					bool flag3 = this.currentCommandIndex != BattleHUD.CommandMenu.Attack && this.currentCommandIndex == BattleHUD.CommandMenu.Item;
					if (flag3) {
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
	}

	// Token: 0x06001594 RID: 5524 RVA: 0x0015BFFC File Offset: 0x0015A1FC
	private void SetTargetAvalability (byte cursor) {
		this.targetCursor = cursor;
		bool flag = cursor == 0;
		if (flag) {
			this.cursorType = BattleHUD.CursorGroup.Individual;
			foreach (object obj in this.PlayerTargetPanel.transform) {
				ButtonGroupState.SetButtonEnable (((Transform) obj).gameObject, true);
			}
			foreach (object obj2 in this.EnemyTargetPanel.transform) {
				ButtonGroupState.SetButtonEnable (((Transform) obj2).gameObject, true);
			}
			this.AllTargetButton.SetActive (false);
			this.allPlayerButton.SetActive (false);
			this.allEnemyButton.SetActive (false);
		} else {
			bool flag2 = cursor == 2;
			if (flag2) {
				this.cursorType = BattleHUD.CursorGroup.Individual;
				foreach (object obj3 in this.PlayerTargetPanel.transform) {
					ButtonGroupState.SetButtonEnable (((Transform) obj3).gameObject, false);
				}
				foreach (object obj4 in this.EnemyTargetPanel.transform) {
					ButtonGroupState.SetButtonEnable (((Transform) obj4).gameObject, true);
				}
				this.AllTargetButton.SetActive (false);
				this.allPlayerButton.SetActive (false);
				this.allEnemyButton.SetActive (false);
			} else {
				bool flag3 = cursor == 1;
				if (flag3) {
					this.cursorType = BattleHUD.CursorGroup.Individual;
					foreach (object obj5 in this.PlayerTargetPanel.transform) {
						ButtonGroupState.SetButtonEnable (((Transform) obj5).gameObject, true);
					}
					foreach (object obj6 in this.EnemyTargetPanel.transform) {
						ButtonGroupState.SetButtonEnable (((Transform) obj6).gameObject, false);
					}
					this.AllTargetButton.SetActive (false);
					this.allPlayerButton.SetActive (false);
					this.allEnemyButton.SetActive (false);
				} else {
					bool flag4 = cursor == 3;
					if (flag4) {
						foreach (object obj7 in this.PlayerTargetPanel.transform) {
							ButtonGroupState.SetButtonEnable (((Transform) obj7).gameObject, true);
						}
						foreach (object obj8 in this.EnemyTargetPanel.transform) {
							ButtonGroupState.SetButtonEnable (((Transform) obj8).gameObject, true);
						}
						this.AllTargetButton.SetActive (FF9StateSystem.MobilePlatform);
						this.allPlayerButton.SetActive (false);
						this.allEnemyButton.SetActive (false);
					} else {
						bool flag5 = cursor == 5;
						if (flag5) {
							foreach (object obj9 in this.PlayerTargetPanel.transform) {
								ButtonGroupState.SetButtonEnable (((Transform) obj9).gameObject, false);
							}
							foreach (object obj10 in this.EnemyTargetPanel.transform) {
								ButtonGroupState.SetButtonEnable (((Transform) obj10).gameObject, true);
							}
							this.AllTargetButton.SetActive (FF9StateSystem.MobilePlatform);
							this.allPlayerButton.SetActive (false);
							this.allEnemyButton.SetActive (false);
						} else {
							bool flag6 = cursor == 4;
							if (flag6) {
								foreach (object obj11 in this.PlayerTargetPanel.transform) {
									ButtonGroupState.SetButtonEnable (((Transform) obj11).gameObject, true);
								}
								foreach (object obj12 in this.EnemyTargetPanel.transform) {
									ButtonGroupState.SetButtonEnable (((Transform) obj12).gameObject, false);
								}
								this.AllTargetButton.SetActive (FF9StateSystem.MobilePlatform);
								this.allPlayerButton.SetActive (false);
								this.allEnemyButton.SetActive (false);
							} else {
								bool flag7 = cursor == 8 || cursor == 11;
								if (flag7) {
									this.cursorType = BattleHUD.CursorGroup.AllEnemy;
									foreach (object obj13 in this.PlayerTargetPanel.transform) {
										ButtonGroupState.SetButtonEnable (((Transform) obj13).gameObject, false);
									}
									foreach (object obj14 in this.EnemyTargetPanel.transform) {
										ButtonGroupState.SetButtonEnable (((Transform) obj14).gameObject, true);
									}
									this.AllTargetButton.SetActive (false);
									this.allPlayerButton.SetActive (false);
									this.allEnemyButton.SetActive (true);
									this.isAllTarget = true;
								} else {
									bool flag8 = cursor == 7 || cursor == 10;
									if (flag8) {
										this.cursorType = BattleHUD.CursorGroup.AllPlayer;
										foreach (object obj15 in this.PlayerTargetPanel.transform) {
											ButtonGroupState.SetButtonEnable (((Transform) obj15).gameObject, true);
										}
										foreach (object obj16 in this.EnemyTargetPanel.transform) {
											ButtonGroupState.SetButtonEnable (((Transform) obj16).gameObject, false);
										}
										this.AllTargetButton.SetActive (false);
										this.allPlayerButton.SetActive (true);
										this.allEnemyButton.SetActive (false);
										this.isAllTarget = true;
									} else {
										bool flag9 = cursor == 6 || cursor == 12 || cursor == 9;
										if (flag9) {
											this.cursorType = BattleHUD.CursorGroup.All;
											foreach (object obj17 in this.PlayerTargetPanel.transform) {
												ButtonGroupState.SetButtonEnable (((Transform) obj17).gameObject, true);
											}
											foreach (object obj18 in this.EnemyTargetPanel.transform) {
												ButtonGroupState.SetButtonEnable (((Transform) obj18).gameObject, true);
											}
											this.AllTargetButton.SetActive (false);
											this.allPlayerButton.SetActive (true);
											this.allEnemyButton.SetActive (true);
											this.isAllTarget = true;
										} else {
											bool flag10 = cursor == 13;
											if (flag10) {
												this.cursorType = BattleHUD.CursorGroup.Individual;
												foreach (object obj19 in this.PlayerTargetPanel.transform) {
													ButtonGroupState.SetButtonEnable (((Transform) obj19).gameObject, false);
												}
												foreach (object obj20 in this.EnemyTargetPanel.transform) {
													ButtonGroupState.SetButtonEnable (((Transform) obj20).gameObject, false);
												}
												int currentPlayerIndex = this.currentPlayerIndex;
												ButtonGroupState.SetButtonEnable (this.PlayerTargetPanel.GetChild (currentPlayerIndex), true);
												this.AllTargetButton.SetActive (false);
												this.allPlayerButton.SetActive (false);
												this.allEnemyButton.SetActive (false);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06001595 RID: 5525 RVA: 0x0015CA20 File Offset: 0x0015AC20
	private void SetTargetDefault () {
		int num = 0;
		int num2 = 4;
		bool flag = this.targetDead == 0;
		checked {
			if (flag) {
				for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
					int num3 = 0;
					while (1 << num3 != (int) next.btl_id) {
						num3++;
					}
					bool flag2 = next.btl_id != 0 && next.bi.target > 0;
					if (flag2) {
						bool flag3 = next.bi.player > 0;
						if (flag3) {
							bool flag4 = btl_stat.CheckStatus (next, 256u);
							if (flag4) {
								ButtonGroupState.SetButtonEnable (this.targetHudList[num].Self, false);
							}
							num++;
						} else {
							bool flag5 = btl_stat.CheckStatus (next, 256u);
							if (flag5) {
								ButtonGroupState.SetButtonEnable (this.targetHudList[num2].Self, false);
							}
							num2++;
						}
					}
				}
			}
			bool flag6 = this.targetCursor != 0 && this.targetCursor != 1 && this.targetCursor != 2 && this.targetCursor != 3 && this.targetCursor != 4 && this.targetCursor != 5;
			if (flag6) {
				bool flag7 = this.targetCursor == 13;
				if (flag7) {
					int currentPlayerIndex = this.currentPlayerIndex;
					ButtonGroupState.SetCursorStartSelect (this.targetHudList[currentPlayerIndex].Self, BattleHUD.TargetGroupButton);
					this.currentTargetIndex = currentPlayerIndex;
					ButtonGroupState.RemoveCursorMemorize (BattleHUD.TargetGroupButton);
				}
			} else {
				bool flag8 = this.defaultTargetCursor == 1;
				if (flag8) {
					bool flag9 = this.defaultTargetDead > 0;
					if (flag9) {
						int dead = (int) this.GetDead (true);
						ButtonGroupState.SetCursorStartSelect (this.targetHudList[dead].Self, BattleHUD.TargetGroupButton);
					} else {
						int currentPlayerIndex2 = this.currentPlayerIndex;
						ButtonGroupState.SetCursorStartSelect (this.targetHudList[currentPlayerIndex2].Self, BattleHUD.TargetGroupButton);
					}
					this.currentTargetIndex = 0;
					ButtonGroupState.RemoveCursorMemorize (BattleHUD.TargetGroupButton);
				} else {
					int num4 = HonoluluBattleMain.EnemyStartIndex;
					bool flag10 = this.defaultTargetDead > 0;
					if (flag10) {
						num4 = (int) this.GetDead (false);
						ButtonGroupState.SetCursorStartSelect (this.targetHudList[num4].Self, BattleHUD.TargetGroupButton);
					} else {
						num4 = this.GetFirstEnemy () + HonoluluBattleMain.EnemyStartIndex;
						bool flag11 = num4 != -1;
						if (flag11) {
							bool flag12 = this.currentCommandIndex == BattleHUD.CommandMenu.Attack && FF9StateSystem.PCPlatform;
							if (flag12) {
								this.ValidateDefaultTarget (ref num4);
							}
							ButtonGroupState.SetCursorStartSelect (this.targetHudList[num4].Self, BattleHUD.TargetGroupButton);
						}
					}
					this.currentTargetIndex = num4;
					ButtonGroupState.RemoveCursorMemorize (BattleHUD.TargetGroupButton);
				}
			}
		}
	}

	// Token: 0x06001596 RID: 5526 RVA: 0x0015CD00 File Offset: 0x0015AF00
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
		bool flag2 = this.isAllTarget;
		if (flag2) {
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
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				bool flag3 = next.btl_id != 0 && next.bi.target > 0;
				if (flag3) {
					bool flag4 = next.bi.player > 0;
					if (flag4) {
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
	}

	// Token: 0x06001597 RID: 5527 RVA: 0x0015D048 File Offset: 0x0015B248
	private void SetHelpMessageVisibility (bool active) {
		bool helpEnabled = ButtonGroupState.HelpEnabled;
		if (helpEnabled) {
			bool active2 = active && (this.CommandPanel.activeSelf || this.ItemPanel.activeSelf || this.AbilityPanel.activeSelf || this.TargetPanel.activeSelf);
			Singleton<HelpDialog>.Instance.gameObject.SetActive (active2);
		}
	}

	// Token: 0x06001598 RID: 5528 RVA: 0x0015D0B0 File Offset: 0x0015B2B0
	private void SetHudVisibility (bool active) {
		bool flag = this.hidingHud != active;
		if (!flag) {
			this.hidingHud = !active;
			this.AllMenuPanel.SetActive (active);
			this.SetHelpMessageVisibility (active);
			bool flag2 = !active;
			if (flag2) {
				this.currentButtonGroup = ButtonGroupState.ActiveGroup;
				ButtonGroupState.DisableAllGroup (false);
				ButtonGroupState.SetPointerVisibilityToGroup (active, this.currentButtonGroup);
			} else {
				bool flag3 = this.currentButtonGroup == BattleHUD.CommandGroupButton && !this.CommandPanel.activeSelf;
				if (flag3) {
					this.currentButtonGroup = string.Empty;
				}
				this.isTryingToRun = false;
				ButtonGroupState.ActiveGroup = this.currentButtonGroup;
				this.DisplayTargetPointer ();
			}
		}
	}

	// Token: 0x06001599 RID: 5529 RVA: 0x0015D16C File Offset: 0x0015B36C
	private void ProcessAutoBattleInput () {
		this.isAutoAttack = !this.isAutoAttack;
		this.autoBattleToggle.value = this.isAutoAttack;
		this.AutoBattleHud.SetActive (this.isAutoAttack);
		this.autoBattleButtonComponent.SetState (UIButtonColor.State.Normal, false);
		bool flag = this.isAutoAttack;
		if (flag) {
			this.SetIdle ();
			this.SetPartySwapButtonActive (false);
		} else {
			this.SetPartySwapButtonActive (true);
			foreach (BattleHUD.PlayerDetailHUD playerDetailHUD in this.playerDetailPanelList) {
				bool flag2 = this.readyQueue.Contains (playerDetailHUD.PlayerId);
				if (flag2) {
					bool flag3 = this.inputFinishedList.Contains (playerDetailHUD.PlayerId);
					if (flag3) {
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

	// Token: 0x0600159A RID: 5530 RVA: 0x0015D278 File Offset: 0x0015B478
	public bool FF9BMenu_IsEnable () {
		return this.commandEnable;
	}

	// Token: 0x0600159B RID: 5531 RVA: 0x0015D290 File Offset: 0x0015B490
	public bool FF9BMenu_IsEnableAtb () {
		bool flag = !this.commandEnable;
		bool result;
		if (flag) {
			result = false;
		} else {
			bool flag2 = FF9StateSystem.Settings.cfg.atb != 1UL;
			if (flag2) {
				result = true;
			} else {
				bool flag3 = this.hidingHud;
				if (flag3) {
					result = (this.currentPlayerId == -1 || this.currentButtonGroup == BattleHUD.CommandGroupButton || this.currentButtonGroup == string.Empty);
				} else {
					result = (this.currentPlayerId == -1 || ButtonGroupState.ActiveGroup == BattleHUD.CommandGroupButton || ButtonGroupState.ActiveGroup == string.Empty);
				}
			}
		}
		return result;
	}

	// Token: 0x0600159C RID: 5532 RVA: 0x0015D340 File Offset: 0x0015B540
	public void FF9BMenu_EnableMenu (bool active) {
		bool isShowQuitUI = PersistenSingleton<UIManager>.Instance.QuitScene.isShowQuitUI;
		if (!isShowQuitUI) {
			bool flag = PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.BattleHUD;
			if (flag) {
				this.commandEnable = active;
				this.AllMenuPanel.SetActive (active);
				this.HideHudHitAreaGameObject.SetActive (active);
				bool flag2 = !active;
				if (flag2) {
					ButtonGroupState.DisableAllGroup (true);
				} else {
					bool flag3 = (!this.isFromPause && ButtonGroupState.ActiveGroup == string.Empty) || this.isNeedToInit;
					if (flag3) {
						this.isNeedToInit = false;
						this.InitialBattle ();
						this.DisplayParty ();
						this.SetIdle ();
					}
				}
			} else {
				this.beforePauseCommandEnable = active;
				this.isNeedToInit = active;
			}
		}
	}

	// Token: 0x0600159D RID: 5533 RVA: 0x000221C4 File Offset: 0x000203C4
	public void FF9BMenu_Enable (bool enable) { }

	// Token: 0x0600159E RID: 5534 RVA: 0x0015D404 File Offset: 0x0015B604
	private int PatchAbility (int id) {
		bool flag = BattleHUD.AbilCarbuncle == id;
		checked {
			if (flag) {
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
			} else {
				bool flag2 = BattleHUD.AbilFenril == id;
				if (flag2) {
					BTL_DATA btl_DATA2 = FF9StateSystem.Battle.FF9Battle.btl_data[this.currentPlayerId];
					byte b = FF9StateSystem.Common.FF9.party.member[(int) btl_DATA2.bi.line_no].equip[4];
					id += ((b != 222) ? 0 : 1);
				}
			}
			return id;
		}
	}

	// Token: 0x0600159F RID: 5535 RVA: 0x0015D4FC File Offset: 0x0015B6FC
	private ushort GetDead (bool player) {
		ushort num = 0;
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				bool flag = next.bi.target != 0 && (next.stat.cur & 256u) > 0u;
				if (flag) {
					bool flag2 = player && next.bi.player > 0;
					ushort result;
					if (flag2) {
						result = num;
					} else {
						bool flag3 = !player && next.bi.player == 0;
						if (!flag3) {
							num += 1;
							goto IL_89;
						}
						result = num;
					}
					return result;
				}
				IL_89: ;
			}
			return (ushort) this.currentPlayerIndex;
		}
	}

	// Token: 0x060015A0 RID: 5536 RVA: 0x0015D5B4 File Offset: 0x0015B7B4
	private int GetFirstPlayer () {
		int num = -1;
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				bool flag = next.bi.player > 0;
				if (flag) {
					num++;
				}
				bool flag2 = next.bi.player != 0 && next.cur.hp > 0;
				if (flag2) {
					return num;
				}
			}
			return num;
		}
	}

	// Token: 0x060015A1 RID: 5537 RVA: 0x0015D638 File Offset: 0x0015B838
	private int GetFirstEnemy () {
		int num = -1;
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				bool flag = next.bi.player == 0;
				if (flag) {
					num++;
				}
				bool flag2 = next.bi.player == 0 && next.cur.hp > 0;
				if (flag2) {
					return num;
				}
			}
			return num;
		}
	}

	// Token: 0x060015A2 RID: 5538 RVA: 0x0015D6BC File Offset: 0x0015B8BC
	private BTL_DATA GetFirstEnemyPtr () {
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
			bool flag = next.bi.player == 0 && next.cur.hp > 0;
			if (flag) {
				return next;
			}
		}
		return null;
	}

	// Token: 0x060015A3 RID: 5539 RVA: 0x000221C7 File Offset: 0x000203C7
	public void ItemRequest (int id) {
		this.needItemUpdate = true;
	}

	// Token: 0x060015A4 RID: 5540 RVA: 0x0015D720 File Offset: 0x0015B920
	public void ItemUse (int id) {
		bool flag = ff9item.FF9Item_Remove (id, 1) != 0;
		if (flag) {
			this.needItemUpdate = true;
		}
	}

	// Token: 0x060015A5 RID: 5541 RVA: 0x000221C7 File Offset: 0x000203C7
	public void ItemUnuse (int id) {
		this.needItemUpdate = true;
	}

	// Token: 0x060015A6 RID: 5542 RVA: 0x0015D720 File Offset: 0x0015B920
	public void ItemRemove (int id) {
		bool flag = ff9item.FF9Item_Remove (id, 1) != 0;
		if (flag) {
			this.needItemUpdate = true;
		}
	}

	// Token: 0x060015A7 RID: 5543 RVA: 0x0015D748 File Offset: 0x0015B948
	public void ItemAdd (int id) {
		bool flag = ff9item.FF9Item_Add (id, 1) != 0;
		if (flag) {
			this.needItemUpdate = true;
		}
	}

	// Token: 0x060015A8 RID: 5544 RVA: 0x0015D770 File Offset: 0x0015B970
	private bool IsEnableInput (BTL_DATA cur) {
		return cur != null && cur.cur.hp != 0 && !btl_stat.CheckStatus (cur, 1107434755u) && ((int) battle.btl_bonus.member_flag & 1 << (int) cur.bi.line_no) != 0;
	}

	// Token: 0x060015A9 RID: 5545 RVA: 0x0015D7C0 File Offset: 0x0015B9C0
	private int GetSelectMode (BattleHUD.CursorGroup cursor) {
		bool flag = this.targetCursor == 9 || this.targetCursor == 10 || this.targetCursor == 11;
		int result;
		if (flag) {
			result = 2;
		} else {
			bool flag2 = cursor == BattleHUD.CursorGroup.Individual;
			if (flag2) {
				result = 0;
			} else {
				result = 1;
			}
		}
		return result;
	}

	// Token: 0x060015AA RID: 5546 RVA: 0x0015D80C File Offset: 0x0015BA0C
	private void EnableTargetArea () {
		checked {
			for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next) {
				bool flag = next.bi.target > 0;
				if (flag) {
					int num = 0;
					while (1 << num != (int) next.btl_id) {
						num++;
					}
					bool flag2 = next.bi.player > 0;
					if (flag2) {
						Transform transform = next.gameObject.transform.GetChildByName ("bone" + next.tar_bone.ToString ("D3")).gameObject.transform;
						this.modelButtonManager.Show (transform, num, false, (float) next.radius, (float) next.height);
					} else {
						Transform transform2 = next.gameObject.transform.GetChildByName ("bone" + next.tar_bone.ToString ("D3")).gameObject.transform;
						this.modelButtonManager.Show (transform2, num, true, (float) next.radius, (float) next.height);
					}
				}
			}
		}
	}

	// Token: 0x060015AB RID: 5547 RVA: 0x000221D1 File Offset: 0x000203D1
	private void DisableTargetArea () {
		this.modelButtonManager.Reset ();
		this.targetIndexList.Clear ();
	}

	// Token: 0x060015AC RID: 5548 RVA: 0x0015D944 File Offset: 0x0015BB44
	private void ClearModelPointer () {
		foreach (int index in this.targetIndexList) {
			GameObject gameObject = this.modelButtonManager.GetGameObject (index);
			Singleton<PointerManager>.Instance.RemovePointerFromGameObject (gameObject);
		}
		this.targetIndexList.Clear ();
	}

	// Token: 0x060015AD RID: 5549 RVA: 0x0015D9BC File Offset: 0x0015BBBC
	private void PointToModel (BattleHUD.CursorGroup selectType, int targetIndex = 0) {
		this.ClearModelPointer ();
		bool isBlink = false;
		checked {
			switch (selectType) {
				case BattleHUD.CursorGroup.Individual:
					{
						bool flag = targetIndex < HonoluluBattleMain.EnemyStartIndex;
						if (flag) {
							bool flag2 = targetIndex < this.matchBattleIdPlayerList.Count;
							if (flag2) {
								int item = this.matchBattleIdPlayerList[targetIndex];
								this.targetIndexList.Add (item);
							}
						} else {
							bool flag3 = targetIndex - HonoluluBattleMain.EnemyStartIndex < this.matchBattleIdEnemyList.Count;
							if (flag3) {
								int item2 = this.matchBattleIdEnemyList[targetIndex - HonoluluBattleMain.EnemyStartIndex];
								this.targetIndexList.Add (item2);
							}
						}
						isBlink = false;
						break;
					}
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
	}

	// Token: 0x060015AE RID: 5550 RVA: 0x0015DB4C File Offset: 0x0015BD4C
	private void ToggleAllTarget () {
		bool flag = this.cursorType == BattleHUD.CursorGroup.AllEnemy || this.cursorType == BattleHUD.CursorGroup.AllPlayer;
		if (flag) {
			bool flag2 = ButtonGroupState.ActiveButton;
			if (flag2) {
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
			bool flag3 = this.currentTargetIndex < HonoluluBattleMain.EnemyStartIndex;
			if (flag3) {
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

	// Token: 0x060015AF RID: 5551 RVA: 0x0015DC88 File Offset: 0x0015BE88
	private void DisplayTargetPointer () {
		bool flag = ButtonGroupState.ActiveGroup != BattleHUD.TargetGroupButton;
		checked {
			if (!flag) {
				bool flag2 = this.cursorType == BattleHUD.CursorGroup.Individual;
				if (flag2) {
					this.PointToModel (this.cursorType, this.currentTargetIndex);
					ButtonGroupState.SetAllTarget (false);
				} else {
					this.PointToModel (this.cursorType, 0);
					foreach (BattleHUD.TargetHUD targetHUD in this.targetHudList) {
						Singleton<PointerManager>.Instance.SetPointerVisibility (targetHUD.Self, false);
					}
					bool flag3 = this.cursorType == BattleHUD.CursorGroup.AllPlayer;
					if (flag3) {
						List<GameObject> list = new List<GameObject> ();
						for (int i = 0; i < this.playerCount; i++) {
							bool flag4 = this.currentCharacterHp[i] != BattleHUD.ParameterStatus.PARAMSTAT_EMPTY || this.targetDead > 0;
							if (flag4) {
								list.Add (this.targetHudList[i].Self);
							}
						}
						ButtonGroupState.SetMultipleTarget (list, true);
					} else {
						bool flag5 = this.cursorType == BattleHUD.CursorGroup.AllEnemy;
						if (flag5) {
							List<GameObject> list2 = new List<GameObject> ();
							for (int j = 0; j < this.enemyCount; j++) {
								bool flag6 = !this.currentEnemyDieState[j] || this.targetDead > 0;
								if (flag6) {
									list2.Add (this.targetHudList[j + HonoluluBattleMain.EnemyStartIndex].Self);
								}
							}
							ButtonGroupState.SetMultipleTarget (list2, true);
						} else {
							ButtonGroupState.SetAllTarget (true);
						}
					}
				}
			}
		}
	}

	// Token: 0x060015B0 RID: 5552 RVA: 0x0015DE4C File Offset: 0x0015C04C
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

	// Token: 0x060015B1 RID: 5553 RVA: 0x000221EC File Offset: 0x000203EC
	public void ResetToReady () {
		this.SetItemPanelVisibility (false, false);
		this.SetAbilityPanelVisibility (false, false);
		this.SetTargetVisibility (false);
		this.ClearModelPointer ();
		this.DisplayCommand ();
		this.SetCommandVisibility (true, false);
	}

	// Token: 0x060015B2 RID: 5554 RVA: 0x0015DF14 File Offset: 0x0015C114
	public void SetPartySwapButtonActive (bool isActive) {
		foreach (BattleHUD.PlayerDetailHUD playerDetailHUD in this.playerDetailPanelList) {
			bool flag = this.currentPlayerId == playerDetailHUD.PlayerId;
			if (flag) {
				playerDetailHUD.Component.UIBoxCollider.enabled = false;
				playerDetailHUD.Component.ButtonColor.disabledColor = playerDetailHUD.Component.ButtonColor.pressed;
			} else {
				playerDetailHUD.Component.UIBoxCollider.enabled = isActive;
				playerDetailHUD.Component.ButtonColor.disabledColor = playerDetailHUD.Component.ButtonColor.defaultColor;
			}
		}
	}

	// Token: 0x060015B3 RID: 5555 RVA: 0x0015DFE8 File Offset: 0x0015C1E8
	private void Update () {
		bool flag = BattleHUD.Scale < 1f;
		bool flag2 = flag;
		bool flag3 = flag2;
		if (flag3) {
			BattleHUD.Scale = 1f;
		}
		bool flag4 = !PersistenSingleton<UIManager>.Instance.QuitScene.isShowQuitUI && PersistenSingleton<UIManager>.Instance.State == UIManager.UIState.BattleHUD;
		bool flag5 = flag4;
		bool flag6 = flag5;
		bool flag7 = flag6;
		if (flag7) {
			this.UpdatePlayer ();
			this.UpdateMessage ();
			bool flag8 = this.commandEnable;
			bool flag9 = flag8;
			bool flag10 = flag9;
			bool flag11 = flag10;
			if (flag11) {
				bool flag12 = (UIManager.Input.GetKey (Control.LeftBumper) && UIManager.Input.GetKey (Control.RightBumper)) || this.isTryingToRun;
				bool flag13 = flag12;
				bool flag14 = flag13;
				bool flag15 = flag14;
				if (flag15) {
					this.runCounter += RealTime.deltaTime;
					FF9StateSystem.Battle.FF9Battle.btl_escape_key = 1;
					bool flag16 = this.runCounter > 1f;
					bool flag17 = flag16;
					bool flag18 = flag17;
					bool flag19 = flag18;
					if (flag19) {
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
				bool flag20 = key;
				bool flag21 = flag20;
				bool flag22 = flag21;
				if (flag22) {
					this.SetHudVisibility (false);
				} else {
					this.SetHudVisibility (true);
				}
				this.UpdateAndroidTV ();
				this.SetScaleOfUI (); // TehMightTehMight
			}
		}
	}

	// Token: 0x060015B4 RID: 5556 RVA: 0x0015E170 File Offset: 0x0015C370
	public void ForceClearReadyQueue () {
		checked {
			for (int i = this.readyQueue.Count - 1; i >= 0; i--) {
				BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[this.readyQueue[i]];
				bool flag = this.inputFinishedList.Contains (this.readyQueue[i]);
				if (flag) {
					this.inputFinishedList.Remove (this.readyQueue[i]);
				}
				this.readyQueue.RemoveAt (i);
			}
		}
	}

	// Token: 0x060015B5 RID: 5557 RVA: 0x0015E200 File Offset: 0x0015C400
	public void VerifyTarget (int modelIndex) {
		bool flag = !this.hidingHud && this.commandEnable && this.cursorType == BattleHUD.CursorGroup.Individual;
		if (flag) {
			bool flag2 = modelIndex < HonoluluBattleMain.EnemyStartIndex;
			int num;
			if (flag2) {
				num = this.matchBattleIdPlayerList.IndexOf (modelIndex);
			} else {
				num = checked (this.matchBattleIdEnemyList.IndexOf (modelIndex) + 4);
			}
			bool flag3 = num != -1;
			if (flag3) {
				FF9Sfx.FF9SFX_Play (103);
				bool enabled = this.targetHudList[num].ButtonGroup.enabled;
				if (enabled) {
					this.CheckDoubleCast (modelIndex, BattleHUD.CursorGroup.Individual);
				}
			}
		}
	}

	// Token: 0x060015B6 RID: 5558 RVA: 0x0015E29C File Offset: 0x0015C49C
	private void SetTarget (int battleIndex) {
		bool isDoubleCast = this.IsDoubleCast;
		if (isDoubleCast) {
			this.SendDoubleCastCommand (this.firstCommand, this.ProcessCommand (battleIndex, this.cursorType));
		} else {
			this.SendCommand (this.ProcessCommand (battleIndex, this.cursorType));
		}
		this.SetTargetVisibility (false);
		this.SetIdle ();
	}

	// Token: 0x060015B7 RID: 5559 RVA: 0x0015E2F8 File Offset: 0x0015C4F8
	private void ValidateDefaultTarget (ref int firstIndex) {
		checked {
			for (int i = firstIndex; i < this.targetHudList.Count; i++) {
				BattleHUD.TargetHUD targetHUD = this.targetHudList[i];
				bool flag = targetHUD.Self.activeSelf && targetHUD.NameLabel.color != FF9TextTool.Gray;
				if (flag) {
					firstIndex = i;
					break;
				}
			}
		}
	}

	// Token: 0x04002912 RID: 10514
	public const byte BTLMES_LEVEL_FOLLOW_0 = 0;

	// Token: 0x04002913 RID: 10515
	public const byte BTLMES_LEVEL_FOLLOW_1 = 1;

	// Token: 0x04002914 RID: 10516
	public const byte BTLMES_LEVEL_TITLE = 1;

	// Token: 0x04002915 RID: 10517
	public const byte BTLMES_LEVEL_LIBRA = 2;

	// Token: 0x04002916 RID: 10518
	public const byte BTLMES_LEVEL_EVENT = 3;

	// Token: 0x04002917 RID: 10519
	public const byte LIBRA_MES_NO = 10;

	// Token: 0x04002918 RID: 10520
	public const byte PEEPING_MES_NO = 8;

	// Token: 0x04002919 RID: 10521
	public const byte BTLMES_ATTRIBUTE_START = 0;

	// Token: 0x0400291A RID: 10522
	private float lastFrameRightTriggerAxis;

	// Token: 0x0400291B RID: 10523
	private bool lastFramePressOnMenu;

	// Token: 0x0400291C RID: 10524
	private static byte[] BattleMessageTimeTick = new byte[] {
		54,
		46,
		48,
		30,
		24,
		18,
		12
	};

	// Token: 0x0400291D RID: 10525
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
		194,
		193,
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

	// Token: 0x0400291E RID: 10526
	private byte currentLibraMessageNumber;

	// Token: 0x0400291F RID: 10527
	private byte currentLibraMessageCount;

	// Token: 0x04002920 RID: 10528
	private BTL_DATA libraBtlData;

	// Token: 0x04002921 RID: 10529
	private byte currentPeepingMessageCount;

	// Token: 0x04002922 RID: 10530
	private ENEMY peepingEnmData;

	// Token: 0x04002923 RID: 10531
	private byte currentMessagePriority;

	// Token: 0x04002924 RID: 10532
	private float battleMessageCounter;

	// Token: 0x04002925 RID: 10533
	private static int YINFO_ANIM_HPMP_MIN = 4;

	// Token: 0x04002926 RID: 10534
	private static int YINFO_ANIM_HPMP_MAX = 16;

	// Token: 0x04002927 RID: 10535
	private static int AbilFenril = 66;

	// Token: 0x04002928 RID: 10536
	private static int AbilCarbuncle = 68;

	// Token: 0x04002929 RID: 10537
	private static int AbilSaMpHalf = 226;

	// Token: 0x0400292A RID: 10538
	private static string ATENormal = "battle_bar_atb";

	// Token: 0x0400292B RID: 10539
	private static string ATEGray = "battle_bar_slow";

	// Token: 0x0400292C RID: 10540
	private static string ATEOrange = "battle_bar_haste";

	// Token: 0x0400292D RID: 10541
	private static float DefaultPartyPanelPosY = -350f;

	// Token: 0x0400292E RID: 10542
	private static float PartyItemHeight = 82f;

	// Token: 0x0400292F RID: 10543
	private List<BattleHUD.PlayerDetailHUD> playerDetailPanelList = new List<BattleHUD.PlayerDetailHUD> ();

	// Token: 0x04002930 RID: 10544
	private BattleHUD.CommandHUD commandDetailHUD;

	// Token: 0x04002931 RID: 10545
	private Dictionary<int, BattleHUD.AbilityPlayerDetail> abilityDetailDict = new Dictionary<int, BattleHUD.AbilityPlayerDetail> ();

	// Token: 0x04002932 RID: 10546
	private BattleHUD.MagicSwordCondition magicSwordCond = new BattleHUD.MagicSwordCondition ();

	// Token: 0x04002933 RID: 10547
	private int enemyCount = -1;

	// Token: 0x04002934 RID: 10548
	private int playerCount = -1;

	// Token: 0x04002935 RID: 10549
	private List<BattleHUD.ParameterStatus> currentCharacterHp = new List<BattleHUD.ParameterStatus> ();

	// Token: 0x04002936 RID: 10550
	private List<bool> currentEnemyDieState = new List<bool> ();

	// Token: 0x04002937 RID: 10551
	private List<BattleHUD.InfoVal> hpInfoVal = new List<BattleHUD.InfoVal> ();

	// Token: 0x04002938 RID: 10552
	private List<BattleHUD.InfoVal> mpInfoVal = new List<BattleHUD.InfoVal> ();

	// Token: 0x04002939 RID: 10553
	private RecycleListPopulator itemScrollList;

	// Token: 0x0400293A RID: 10554
	private RecycleListPopulator abilityScrollList;

	// Token: 0x0400293B RID: 10555
	private List<int> currentTrancePlayer = new List<int> ();

	// Token: 0x0400293C RID: 10556
	private bool currentTranceTrigger;

	// Token: 0x0400293D RID: 10557
	private bool isTranceMenu;

	// Token: 0x0400293E RID: 10558
	private bool needItemUpdate;

	// Token: 0x0400293F RID: 10559
	private bool currentSilenceStatus;

	// Token: 0x04002940 RID: 10560
	private int currentMpValue = -1;

	// Token: 0x04002941 RID: 10561
	private float blinkAlphaCounter;

	// Token: 0x04002942 RID: 10562
	private int tranceColorCounter;

	// Token: 0x04002943 RID: 10563
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

	// Token: 0x04002944 RID: 10564
	public ModelButtonManager modelButtonManager;

	// Token: 0x04002945 RID: 10565
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

	// Token: 0x04002946 RID: 10566
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

	// Token: 0x04002947 RID: 10567
	public static string CommandGroupButton = "Battle.Command";

	// Token: 0x04002948 RID: 10568
	public static string TargetGroupButton = "Battle.Target";

	// Token: 0x04002949 RID: 10569
	public static string AbilityGroupButton = "Battle.Ability";

	// Token: 0x0400294A RID: 10570
	public static string ItemGroupButton = "Battle.Item";

	// Token: 0x0400294B RID: 10571
	public GameObject AutoBattleHud;

	// Token: 0x0400294C RID: 10572
	public GameObject AutoBattleButton;

	// Token: 0x0400294D RID: 10573
	public GameObject AllTargetButton;

	// Token: 0x0400294E RID: 10574
	public GameObject RunButton;

	// Token: 0x0400294F RID: 10575
	public GameObject BackButton;

	// Token: 0x04002950 RID: 10576
	public GameObject PauseButtonGameObject;

	// Token: 0x04002951 RID: 10577
	public GameObject HelpButtonGameObject;

	// Token: 0x04002952 RID: 10578
	public GameObject HideHudHitAreaGameObject;

	// Token: 0x04002953 RID: 10579
	public GameObject AllMenuPanel;

	// Token: 0x04002954 RID: 10580
	public GameObject TargetPanel;

	// Token: 0x04002955 RID: 10581
	public GameObject AbilityPanel;

	// Token: 0x04002956 RID: 10582
	public GameObject ItemPanel;

	// Token: 0x04002957 RID: 10583
	public GameObject CommandPanel;

	// Token: 0x04002958 RID: 10584
	public GameObject PartyDetailPanel;

	// Token: 0x04002959 RID: 10585
	public GameObject BattleDialogGameObject;

	// Token: 0x0400295A RID: 10586
	public GameObject StatusContainer;

	// Token: 0x0400295B RID: 10587
	public GameObject TransitionGameObject;

	// Token: 0x0400295C RID: 10588
	public GameObject ScreenFadeGameObject;

	// Token: 0x0400295D RID: 10589
	private UILabel CommandCaptionLabel;

	// Token: 0x0400295E RID: 10590
	private UIWidget battleDialogWidget;

	// Token: 0x0400295F RID: 10591
	private UILabel battleDialogLabel;

	// Token: 0x04002960 RID: 10592
	private UIToggle autoBattleToggle;

	// Token: 0x04002961 RID: 10593
	private UIToggle allTargetToggle;

	// Token: 0x04002962 RID: 10594
	private UIButton autoBattleButtonComponent;

	// Token: 0x04002963 RID: 10595
	private UIButton allTargetButtonComponent;

	// Token: 0x04002964 RID: 10596
	private GameObject allPlayerButton;

	// Token: 0x04002965 RID: 10597
	private GameObject allEnemyButton;

	// Token: 0x04002966 RID: 10598
	private HonoTweenClipping itemTransition;

	// Token: 0x04002967 RID: 10599
	private HonoTweenClipping abilityTransition;

	// Token: 0x04002968 RID: 10600
	private HonoTweenClipping targetTransition;

	// Token: 0x04002969 RID: 10601
	private GameObject hpStatusPanel;

	// Token: 0x0400296A RID: 10602
	private GameObject mpStatusPanel;

	// Token: 0x0400296B RID: 10603
	private GameObject goodStatusPanel;

	// Token: 0x0400296C RID: 10604
	private GameObject badStatusPanel;

	// Token: 0x0400296D RID: 10605
	private GameObject hpCaption;

	// Token: 0x0400296E RID: 10606
	private GameObject mpCaption;

	// Token: 0x0400296F RID: 10607
	private GameObject atbCaption;

	// Token: 0x04002970 RID: 10608
	private List<BattleHUD.NumberSubModeHUD> hpStatusHudList = new List<BattleHUD.NumberSubModeHUD> ();

	// Token: 0x04002971 RID: 10609
	private List<BattleHUD.NumberSubModeHUD> mpStatusHudList = new List<BattleHUD.NumberSubModeHUD> ();

	// Token: 0x04002972 RID: 10610
	private List<BattleHUD.StatusSubModeHUD> goodStatusHudList = new List<BattleHUD.StatusSubModeHUD> ();

	// Token: 0x04002973 RID: 10611
	private List<BattleHUD.StatusSubModeHUD> badStatusHudList = new List<BattleHUD.StatusSubModeHUD> ();

	// Token: 0x04002974 RID: 10612
	private bool commandEnable;

	// Token: 0x04002975 RID: 10613
	private bool beforePauseCommandEnable;

	// Token: 0x04002976 RID: 10614
	private bool isFromPause;

	// Token: 0x04002977 RID: 10615
	private bool isNeedToInit;

	// Token: 0x04002978 RID: 10616
	private BattleHUD.CommandMenu currentCommandIndex;

	// Token: 0x04002979 RID: 10617
	private uint currentCommandId;

	// Token: 0x0400297A RID: 10618
	private string currentButtonGroup = string.Empty;

	// Token: 0x0400297B RID: 10619
	private int currentSubMenuIndex;

	// Token: 0x0400297C RID: 10620
	private int currentPlayerId = -1;

	// Token: 0x0400297D RID: 10621
	private int currentTargetIndex = -1;

	// Token: 0x0400297E RID: 10622
	private List<int> targetIndexList = new List<int> ();

	// Token: 0x0400297F RID: 10623
	private BattleHUD.SubMenuType subMenuType;

	// Token: 0x04002980 RID: 10624
	private List<int> readyQueue = new List<int> ();

	// Token: 0x04002981 RID: 10625
	private List<int> inputFinishedList = new List<int> ();

	// Token: 0x04002982 RID: 10626
	private List<int> unconsciousStateList = new List<int> ();

	// Token: 0x04002983 RID: 10627
	private float runCounter;

	// Token: 0x04002984 RID: 10628
	private bool hidingHud;

	// Token: 0x04002985 RID: 10629
	private BattleHUD.CursorGroup cursorType;

	// Token: 0x04002986 RID: 10630
	private byte defaultTargetCursor;

	// Token: 0x04002987 RID: 10631
	private byte defaultTargetDead;

	// Token: 0x04002988 RID: 10632
	private byte targetDead;

	// Token: 0x04002989 RID: 10633
	private byte targetCursor;

	// Token: 0x0400298A RID: 10634
	private bool isTryingToRun;

	// Token: 0x0400298B RID: 10635
	private bool isAutoAttack;

	// Token: 0x0400298C RID: 10636
	private bool isAllTarget;

	// Token: 0x0400298D RID: 10637
	private byte doubleCastCount;

	// Token: 0x0400298E RID: 10638
	private BattleHUD.CommandDetail firstCommand = new BattleHUD.CommandDetail ();

	// Token: 0x0400298F RID: 10639
	private Dictionary<int, BattleHUD.CommandMenu> commandCursorMemorize = new Dictionary<int, BattleHUD.CommandMenu> ();

	// Token: 0x04002990 RID: 10640
	private Dictionary<int, int> ability1CursorMemorize = new Dictionary<int, int> ();

	// Token: 0x04002991 RID: 10641
	private Dictionary<int, int> ability2CursorMemorize = new Dictionary<int, int> ();

	// Token: 0x04002992 RID: 10642
	private Dictionary<int, int> itemCursorMemorize = new Dictionary<int, int> ();

	// Token: 0x04002993 RID: 10643
	private List<int> matchBattleIdPlayerList = new List<int> ();

	// Token: 0x04002994 RID: 10644
	private List<int> matchBattleIdEnemyList = new List<int> ();

	// Token: 0x04002995 RID: 10645
	private List<int> itemIdList = new List<int> ();

	// Token: 0x04002996 RID: 10646
	private List<BattleHUD.TargetHUD> targetHudList = new List<BattleHUD.TargetHUD> ();

	// Token: 0x04002997 RID: 10647
	private Action onResumeFromQuit;

	// Token: 0x04002998 RID: 10648
	private bool oneTime = true;

	// Token: 0x04002999 RID: 10649
	private UIRect uiRect;

	// Token: 0x0400299A RID: 10650
	private UIRoot uiRoot;

	// Token: 0x0400299B RID: 10651
	private static float scale;

	// Token: 0x0400299C RID: 10652
	private static int battleSwirl;

	// Token: 0x0400299D RID: 10653
	private static float countdown;

	// Token: 0x0400299E RID: 10654
	private static int encounterRate;

	// Token: 0x0400299F RID: 10655
	private static int hD;

	// Token: 0x040029A0 RID: 10656
	private static int battleSpeed;

	// Token: 0x02000369 RID: 873
	private struct HUDCompanent {
		// Token: 0x060015C2 RID: 5570 RVA: 0x00022239 File Offset: 0x00020439
		public HUDCompanent (GameObject go) {
			this.ButtonGroup = go.GetComponent<ButtonGroupState> ();
			this.Label = go.GetComponent<UILabel> ();
			this.UIBoxCollider = go.GetComponent<BoxCollider> ();
			this.ButtonColor = go.GetComponent<UIButtonColor> ();
			this.Button = go.GetComponent<UIButton> ();
		}

		// Token: 0x040029A1 RID: 10657
		public ButtonGroupState ButtonGroup;

		// Token: 0x040029A2 RID: 10658
		public UILabel Label;

		// Token: 0x040029A3 RID: 10659
		public BoxCollider UIBoxCollider;

		// Token: 0x040029A4 RID: 10660
		public UIButtonColor ButtonColor;

		// Token: 0x040029A5 RID: 10661
		public UIButton Button;
	}

	// Token: 0x0200036A RID: 874
	private class CommandHUD {
		// Token: 0x060015C3 RID: 5571 RVA: 0x0015F974 File Offset: 0x0015DB74
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

		// Token: 0x060015C4 RID: 5572 RVA: 0x0015FC14 File Offset: 0x0015DE14
		public GameObject GetGameObjectFromCommand (BattleHUD.CommandMenu menu) {
			GameObject result;
			switch (menu) {
				case BattleHUD.CommandMenu.Attack:
					result = this.Attack;
					break;
				case BattleHUD.CommandMenu.Defend:
					result = this.Defend;
					break;
				case BattleHUD.CommandMenu.Ability1:
					result = this.Skill1;
					break;
				case BattleHUD.CommandMenu.Ability2:
					result = this.Skill2;
					break;
				case BattleHUD.CommandMenu.Item:
					result = this.Item;
					break;
				case BattleHUD.CommandMenu.Change:
					result = this.Change;
					break;
				default:
					result = this.Attack;
					break;
			}
			return result;
		}

		// Token: 0x040029A6 RID: 10662
		public GameObject Self;

		// Token: 0x040029A7 RID: 10663
		public GameObject Attack;

		// Token: 0x040029A8 RID: 10664
		public BattleHUD.HUDCompanent AttackComponent;

		// Token: 0x040029A9 RID: 10665
		public GameObject Skill1;

		// Token: 0x040029AA RID: 10666
		public BattleHUD.HUDCompanent Skill1Component;

		// Token: 0x040029AB RID: 10667
		public GameObject Skill2;

		// Token: 0x040029AC RID: 10668
		public BattleHUD.HUDCompanent Skill2Component;

		// Token: 0x040029AD RID: 10669
		public GameObject Item;

		// Token: 0x040029AE RID: 10670
		public BattleHUD.HUDCompanent ItemComponent;

		// Token: 0x040029AF RID: 10671
		public GameObject Defend;

		// Token: 0x040029B0 RID: 10672
		public BattleHUD.HUDCompanent DefendComponent;

		// Token: 0x040029B1 RID: 10673
		public GameObject Change;

		// Token: 0x040029B2 RID: 10674
		public BattleHUD.HUDCompanent ChangeComponent;
	}

	// Token: 0x0200036B RID: 875
	private class PlayerDetailHUD {
		// Token: 0x060015C5 RID: 5573 RVA: 0x0015FC84 File Offset: 0x0015DE84
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

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x060015C6 RID: 5574 RVA: 0x0015FDB0 File Offset: 0x0015DFB0
		// (set) Token: 0x060015C7 RID: 5575 RVA: 0x00022278 File Offset: 0x00020478
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

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x060015C8 RID: 5576 RVA: 0x0015FDC8 File Offset: 0x0015DFC8
		// (set) Token: 0x060015C9 RID: 5577 RVA: 0x000222B8 File Offset: 0x000204B8
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

		// Token: 0x040029B3 RID: 10675
		private bool atbBlink;

		// Token: 0x040029B4 RID: 10676
		private bool tranceBlink;

		// Token: 0x040029B5 RID: 10677
		public int PlayerId;

		// Token: 0x040029B6 RID: 10678
		public GameObject Self;

		// Token: 0x040029B7 RID: 10679
		public BattleHUD.HUDCompanent Component;

		// Token: 0x040029B8 RID: 10680
		public UILabel NameLabel;

		// Token: 0x040029B9 RID: 10681
		public UILabel HPLabel;

		// Token: 0x040029BA RID: 10682
		public UILabel MPLabel;

		// Token: 0x040029BB RID: 10683
		public UISprite ATBHighlightSprite;

		// Token: 0x040029BC RID: 10684
		public UIWidget ATBForegroundWidget;

		// Token: 0x040029BD RID: 10685
		public UISprite ATBForegroundSprite;

		// Token: 0x040029BE RID: 10686
		public UIProgressBar ATBSlider;

		// Token: 0x040029BF RID: 10687
		public UISprite TranceHighlightSprite;

		// Token: 0x040029C0 RID: 10688
		public UIWidget TranceForegroundWidget;

		// Token: 0x040029C1 RID: 10689
		public UIProgressBar TranceSlider;

		// Token: 0x040029C2 RID: 10690
		public GameObject TranceSliderGameObject;
	}

	// Token: 0x0200036C RID: 876
	private class NumberSubModeHUD {
		// Token: 0x060015CA RID: 5578 RVA: 0x0015FDE0 File Offset: 0x0015DFE0
		public NumberSubModeHUD (GameObject go) {
			this.Self = go;
			this.Current = go.GetChild (0).GetComponent<UILabel> ();
			this.slash = go.GetChild (1).GetComponent<UILabel> ();
			this.Max = go.GetChild (2).GetComponent<UILabel> ();
		}

		// Token: 0x17000235 RID: 565
		// (set) Token: 0x060015CB RID: 5579 RVA: 0x000222F8 File Offset: 0x000204F8
		public Color TextColor {
			set {
				this.Current.color = value;
				this.slash.color = value;
				this.Max.color = value;
			}
		}

		// Token: 0x040029C3 RID: 10691
		public GameObject Self;

		// Token: 0x040029C4 RID: 10692
		public UILabel Current;

		// Token: 0x040029C5 RID: 10693
		public UILabel Max;

		// Token: 0x040029C6 RID: 10694
		private UILabel slash;
	}

	// Token: 0x0200036D RID: 877
	private class StatusSubModeHUD {
		// Token: 0x060015CC RID: 5580 RVA: 0x0015FE34 File Offset: 0x0015E034
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

		// Token: 0x040029C7 RID: 10695
		public GameObject Self;

		// Token: 0x040029C8 RID: 10696
		public UISprite[] StatusesSpriteList;
	}

	// Token: 0x0200036E RID: 878
	private enum AbilityStatus {
		// Token: 0x040029CA RID: 10698
		ABILSTAT_NONE,
		// Token: 0x040029CB RID: 10699
		ABILSTAT_DISABLE,
		// Token: 0x040029CC RID: 10700
		ABILSTAT_ENABLE
	}

	// Token: 0x0200036F RID: 879
	private enum ParameterStatus {
		// Token: 0x040029CE RID: 10702
		PARAMSTAT_NORMAL,
		// Token: 0x040029CF RID: 10703
		PARAMSTAT_CRITICAL,
		// Token: 0x040029D0 RID: 10704
		PARAMSTAT_EMPTY
	}

	// Token: 0x02000370 RID: 880
	private enum SubMenuType {
		// Token: 0x040029D2 RID: 10706
		CommandNormal,
		// Token: 0x040029D3 RID: 10707
		CommandAbility,
		// Token: 0x040029D4 RID: 10708
		CommandItem,
		// Token: 0x040029D5 RID: 10709
		CommandThrow,
		// Token: 0x040029D6 RID: 10710
		CommandSlide
	}

	// Token: 0x02000371 RID: 881
	private class AbilityPlayerDetail {
		// Token: 0x060015CE RID: 5582 RVA: 0x0002234D File Offset: 0x0002054D
		public void Clear () {
			this.AbilityEquipList.Clear ();
			this.AbilityPaList.Clear ();
			this.AbilityMaxPaList.Clear ();
		}

		// Token: 0x040029D7 RID: 10711
		public PLAYER Player;

		// Token: 0x040029D8 RID: 10712
		public bool HasAp;

		// Token: 0x040029D9 RID: 10713
		public Dictionary<int, bool> AbilityEquipList = new Dictionary<int, bool> ();

		// Token: 0x040029DA RID: 10714
		public Dictionary<int, int> AbilityPaList = new Dictionary<int, int> ();

		// Token: 0x040029DB RID: 10715
		public Dictionary<int, int> AbilityMaxPaList = new Dictionary<int, int> ();
	}

	// Token: 0x02000372 RID: 882
	private class MagicSwordCondition {
		// Token: 0x060015D0 RID: 5584 RVA: 0x0015FF04 File Offset: 0x0015E104
		public override bool Equals (object obj) {
			bool flag = obj == null || base.GetType () != obj.GetType ();
			bool result;
			if (flag) {
				result = false;
			} else {
				BattleHUD.MagicSwordCondition other = obj as BattleHUD.MagicSwordCondition;
				result = (this == other);
			}
			return result;
		}

		// Token: 0x060015D1 RID: 5585 RVA: 0x0015FF44 File Offset: 0x0015E144
		public override int GetHashCode () {
			return base.GetHashCode ();
		}

		// Token: 0x060015D2 RID: 5586 RVA: 0x0015FF5C File Offset: 0x0015E15C
		public static bool operator == (BattleHUD.MagicSwordCondition self, BattleHUD.MagicSwordCondition other) {
			return self.IsViviExist == other.IsViviExist && self.IsViviDead == other.IsViviDead && self.IsSteinerMini == other.IsSteinerMini;
		}

		// Token: 0x060015D3 RID: 5587 RVA: 0x0015FF9C File Offset: 0x0015E19C
		public static bool operator != (BattleHUD.MagicSwordCondition self, BattleHUD.MagicSwordCondition other) {
			return !(self == other);
		}

		// Token: 0x040029DC RID: 10716
		public bool IsViviExist;

		// Token: 0x040029DD RID: 10717
		public bool IsViviDead;

		// Token: 0x040029DE RID: 10718
		public bool IsSteinerMini;
	}

	// Token: 0x02000373 RID: 883
	public class InfoVal {
		// Token: 0x040029DF RID: 10719
		public int inc_val;

		// Token: 0x040029E0 RID: 10720
		public int disp_val;

		// Token: 0x040029E1 RID: 10721
		public int req_val;

		// Token: 0x040029E2 RID: 10722
		public int anim_frm;

		// Token: 0x040029E3 RID: 10723
		public byte pad;
	}

	// Token: 0x02000374 RID: 884
	public class BattleItemListData : ListDataTypeBase {
		// Token: 0x040029E4 RID: 10724
		public int Id;

		// Token: 0x040029E5 RID: 10725
		public int Count;
	}

	// Token: 0x02000375 RID: 885
	public class BattleAbilityListData : ListDataTypeBase {
		// Token: 0x040029E6 RID: 10726
		public int Index;
	}

	// Token: 0x02000376 RID: 886
	private struct TargetHUD {
		// Token: 0x060015D7 RID: 5591 RVA: 0x00022388 File Offset: 0x00020588
		public TargetHUD (GameObject go) {
			this.Self = go;
			this.ButtonGroup = go.GetComponent<ButtonGroupState> ();
			this.NameLabel = go.GetChild (0).GetComponent<UILabel> ();
			this.KeyNavigate = go.GetComponent<UIKeyNavigation> ();
		}

		// Token: 0x040029E7 RID: 10727
		public GameObject Self;

		// Token: 0x040029E8 RID: 10728
		public ButtonGroupState ButtonGroup;

		// Token: 0x040029E9 RID: 10729
		public UIKeyNavigation KeyNavigate;

		// Token: 0x040029EA RID: 10730
		public UILabel NameLabel;
	}

	// Token: 0x02000377 RID: 887
	public enum SubMenu {
		// Token: 0x040029EC RID: 10732
		Command,
		// Token: 0x040029ED RID: 10733
		Target,
		// Token: 0x040029EE RID: 10734
		Ability,
		// Token: 0x040029EF RID: 10735
		Item,
		// Token: 0x040029F0 RID: 10736
		None
	}

	// Token: 0x02000378 RID: 888
	public enum CommandMenu {
		// Token: 0x040029F2 RID: 10738
		Attack,
		// Token: 0x040029F3 RID: 10739
		Defend,
		// Token: 0x040029F4 RID: 10740
		Ability1,
		// Token: 0x040029F5 RID: 10741
		Ability2,
		// Token: 0x040029F6 RID: 10742
		Item,
		// Token: 0x040029F7 RID: 10743
		Change,
		// Token: 0x040029F8 RID: 10744
		None
	}

	// Token: 0x02000379 RID: 889
	public enum CursorGroup {
		// Token: 0x040029FA RID: 10746
		Individual,
		// Token: 0x040029FB RID: 10747
		AllPlayer,
		// Token: 0x040029FC RID: 10748
		AllEnemy,
		// Token: 0x040029FD RID: 10749
		All,
		// Token: 0x040029FE RID: 10750
		None
	}

	// Token: 0x0200037A RID: 890
	private class CommandDetail {
		// Token: 0x040029FF RID: 10751
		public uint CommandId;

		// Token: 0x04002A00 RID: 10752
		public uint SubId;

		// Token: 0x04002A01 RID: 10753
		public ushort TargetId;

		// Token: 0x04002A02 RID: 10754
		public uint TargetType;
	}
}