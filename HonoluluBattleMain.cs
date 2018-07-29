using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Common;
using Assets.Sources.Scripts.Common;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using UnityEngine;

// Token: 0x020000C3 RID: 195
public class HonoluluBattleMain : PersistenSingleton<MonoBehaviour>
{
	// Token: 0x0600039B RID: 923 RVA: 0x0006BB40 File Offset: 0x00069D40
	protected override void Awake()
	{
		base.Awake();
		Application.targetFrameRate = 60;
		this.playerMaterials = new List<Material>();
		this.monsterMaterials = new List<Material>();
		FF9StateSystem.Battle.isFade = false;
		this.animationName = new string[8];
		this.needClampTime = false;
		if (Application.platform == RuntimePlatform.Android)
		{
			string deviceModel = SystemInfo.deviceModel;
			if (string.Compare("Asus Nexus Player", deviceModel, true) == 0)
			{
				this.needClampTime = true;
			}
		}
		FF9StateSystem instance = PersistenSingleton<FF9StateSystem>.Instance;
		FF9StateSystem.Battle.FF9Battle.map.nextMode = instance.prevMode;
		if (instance.prevMode == 1)
		{
			FF9StateSystem.Battle.FF9Battle.map.nextMapNo = FF9StateSystem.Common.FF9.fldMapNo;
		}
		else if (instance.prevMode == 3)
		{
			FF9StateSystem.Battle.FF9Battle.map.nextMapNo = FF9StateSystem.Common.FF9.wldMapNo;
		}
	}

	// Token: 0x0600039C RID: 924 RVA: 0x0006BC3C File Offset: 0x00069E3C
	private void Start()
	{
		HonoluluBattleMain.Read();
		this.cameraController = GameObject.Find("Battle Camera").GetComponent<BattleMapCameraController>();
		this.InitBattleScene();
		HonoluluBattleMain.UpdateFrameTime(FF9StateSystem.Settings.FastForwardFactor);
		GameObject gameObject = GameObject.Find("BattleMap Root");
		HonoluluBattleMain.battleSPS = new GameObject("BattleMap SPS")
		{
			transform = 
			{
				parent = gameObject.transform
			}
		}.AddComponent<BattleSPSSystem>();
		HonoluluBattleMain.battleSPS.Init();
		byte camera = FF9StateSystem.Battle.FF9Battle.btl_scene.PatAddr[(int)FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum].Camera;
		FF9StateSystem.Battle.FF9Battle.seq_work_set.CameraNo = ((camera >= 3) ? ((byte)UnityEngine.Random.Range(0, 3)) : camera);
		SFX.StartBattle();
		bool flag = FF9StateSystem.Settings.cfg.skip_btl_camera == 0UL && FF9StateSystem.Battle.isRandomEncounter;
		if (flag)
		{
			SFX.SkipCameraAnimation(-1);
		}
		bool flag2 = FF9StateSystem.Battle.isNoBoosterMap();
		if (flag2)
		{
			FF9StateSystem.Settings.IsBoosterButtonActive[0] = false;
			FF9StateSystem.Settings.SetBoosterHudToCurrentState();
			PersistenSingleton<UIManager>.Instance.Booster.SetBoosterButton(BoosterType.BattleAssistance, false);
		}
	}

	// Token: 0x0600039D RID: 925 RVA: 0x0006BD78 File Offset: 0x00069F78
	public void InitBattleScene()
	{
		FF9StateGlobal ff = FF9StateSystem.Common.FF9;
		ff.charArray.Clear();
		this.btlScene = (FF9StateSystem.Battle.FF9Battle.btl_scene = new BTL_SCENE());
		global::Debug.Log("battleID = " + FF9StateSystem.Battle.battleMapIndex);
		HonoluluBattleMain.battleSceneName = FF9BattleDB.SceneData.FirstOrDefault((KeyValuePair<string, int> x) => x.Value == FF9StateSystem.Battle.battleMapIndex).Key;
		HonoluluBattleMain.battleSceneName = HonoluluBattleMain.battleSceneName.Substring(4);
		global::Debug.Log("battleSceneName = " + HonoluluBattleMain.battleSceneName);
		this.btlScene.ReadBattleScene(HonoluluBattleMain.battleSceneName);
		base.StartCoroutine(PersistenSingleton<FF9TextTool>.Instance.UpdateBattleText(FF9BattleDB.SceneData["BSC_" + HonoluluBattleMain.battleSceneName]));
		WMProfiler.Begin("Start Load Text");
		string text = FF9BattleDB.MapModel["BSC_" + HonoluluBattleMain.battleSceneName];
		string path = "BattleMap/BattleModel/battleMap_all/" + text + "/" + text;
		FF9StateSystem.Battle.FF9Battle.map.btlBGPtr = ModelFactory.CreateModel(path, Vector3.zero, Vector3.zero, true);
		if (!FF9StateSystem.Battle.isDebug)
		{
			FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum = (byte)this.ChoicePattern();
		}
		else
		{
			FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum = FF9StateSystem.Battle.patternIndex;
		}
		this.btlSeq = new btlseq();
		this.btlSeq.ReadBattleSequence(HonoluluBattleMain.battleSceneName);
		FF9StateSystem.Battle.FF9Battle.attr = 0;
		battle.InitBattle();
		FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
		ff9Battle.attr |= 1;
		if (!FF9StateSystem.Battle.isDebug)
		{
			string ebFileName = "EVT_BATTLE_" + HonoluluBattleMain.battleSceneName;
			FF9StateBattleMap map = FF9StateSystem.Battle.FF9Battle.map;
			map.evtPtr = EventEngineUtils.loadEventData(ebFileName, EventEngineUtils.ebSubFolderBattle);
			PersistenSingleton<EventEngine>.Instance.StartEvents(map.evtPtr);
			PersistenSingleton<EventEngine>.Instance.eTb.InitMessage();
		}
		this.CreateBattleData(ff);
		if (HonoluluBattleMain.battleSceneName == "EF_E006" || HonoluluBattleMain.battleSceneName == "EF_E007")
		{
			BTL_DATA btl_DATA = FF9StateSystem.Battle.FF9Battle.btl_data[4];
			BTL_DATA btl_DATA2 = FF9StateSystem.Battle.FF9Battle.btl_data[5];
			this.GeoBattleAttach(btl_DATA2.gameObject, btl_DATA.gameObject, 55);
			btl_DATA2.attachOffset = 100;
		}
		FF9StateBattleSystem ff9Battle2 = FF9StateSystem.Battle.FF9Battle;
		GEOTEXHEADER geotexheader = new GEOTEXHEADER();
		geotexheader.ReadBGTextureAnim(text);
		ff9Battle2.map.btlBGTexAnimPtr = geotexheader;
		BBGINFO bbginfo = new BBGINFO();
		bbginfo.ReadBattleInfo(text);
		FF9StateSystem.Battle.FF9Battle.map.btlBGInfoPtr = bbginfo;
		btlshadow.ff9battleShadowInit(13);
		battle.InitBattleMap();
		this.seqList = new List<int>();
		SB2_PATTERN sb2_PATTERN = this.btlScene.PatAddr[(int)FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
		int[] array = new int[(int)sb2_PATTERN.MonCount];
		for (int i = 0; i < (int)sb2_PATTERN.MonCount; i++)
		{
			array[i] = (int)sb2_PATTERN.Put[i].TypeNo;
		}
		array = array.Distinct<int>().ToArray<int>();
		for (int j = 0; j < array.Length; j++)
		{
			for (int k = 0; k < this.btlSeq.sequenceProperty.Length; k++)
			{
				SequenceProperty sequenceProperty = this.btlSeq.sequenceProperty[k];
				if (sequenceProperty.Montype == array[j])
				{
					for (int l = 0; l < sequenceProperty.PlayableSequence.Count; l++)
					{
						this.seqList.Add(sequenceProperty.PlayableSequence[l]);
					}
				}
			}
		}
		this.btlIDList = FF9StateSystem.Battle.FF9Battle.seq_work_set.AnmOfsList.Distinct<byte>().ToArray<byte>();
		this.battleState = HonoluluBattleMain.BattleState.PlayerTurn;
		HonoluluBattleMain.playerEnterCommand = false;
		this.playerCastingSkill = false;
		this.enemyEnterCommand = false;
	}

	// Token: 0x0600039E RID: 926 RVA: 0x0006C1DC File Offset: 0x0006A3DC
	private void CreateBattleData(FF9StateGlobal FF9)
	{
		BTL_DATA[] array = btlseq.btl_list = FF9StateSystem.Battle.FF9Battle.btl_data;
		int num = 0;
		for (int i = num; i < 4; i++)
		{
			array[i] = new BTL_DATA();
			if (FF9.party.member[i] != null)
			{
				byte serial_no = FF9.party.member[i].info.serial_no;
				BattlePlayerCharacter.CreatePlayer(array[num], (BattlePlayerCharacter.PlayerSerialNumber)serial_no);
				int num2 = 0;
				foreach (object obj in array[num].gameObject.transform)
				{
					Transform transform = (Transform)obj;
					if (transform.name.Contains("mesh"))
					{
						num2++;
					}
				}
				array[num].meshIsRendering = new bool[num2];
				for (int j = 0; j < num2; j++)
				{
					array[num].meshIsRendering[j] = true;
				}
				array[num].meshCount = num2;
				array[num].animation = array[num].gameObject.GetComponent<Animation>();
				num++;
			}
			array[i].typeNo = 5;
			array[i].idleAnimationName = this.animationName[i];
		}
		for (int k = 4; k < (int)(4 + this.btlScene.PatAddr[(int)FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum].MonCount); k++)
		{
			SB2_PATTERN sb2_PATTERN = this.btlScene.PatAddr[(int)FF9StateSystem.Battle.FF9Battle.btl_scene.PatNum];
			byte typeNo = sb2_PATTERN.Put[k - 4].TypeNo;
			SB2_MON_PARM sb2_MON_PARM = this.btlScene.MonAddr[(int)typeNo];
			string text = FF9BattleDB.GEO[(int)sb2_MON_PARM.Geo];
			Vector3 vector = new Vector3((float)sb2_PATTERN.Put[k - 4].Xpos, (float)(sb2_PATTERN.Put[k - 4].Ypos * -1), (float)sb2_PATTERN.Put[k - 4].Zpos);
			array[k] = new BTL_DATA();
			array[k].gameObject = ModelFactory.CreateModel(text, true);
			if (ModelFactory.IsUseAsEnemyCharacter(text))
			{
				if (text.Contains("GEO_MON_B3_168"))
				{
					Transform transform2 = array[k].gameObject.transform.FindChild("mesh5");
					transform2.gameObject.SetActive(false);
				}
				array[k].weapon_geo = ModelFactory.CreateDefaultWeaponForCharacterWhenUseAsEnemy(text);
				MeshRenderer[] componentsInChildren = array[k].weapon_geo.GetComponentsInChildren<MeshRenderer>();
				array[k].weaponMeshCount = componentsInChildren.Length;
				array[k].weaponRenderer = new Renderer[array[k].weaponMeshCount];
				for (int l = 0; l < array[k].weaponMeshCount; l++)
				{
					array[k].weaponRenderer[l] = componentsInChildren[l].GetComponent<Renderer>();
				}
				geo.geoAttach(array[k].weapon_geo, array[k].gameObject, ModelFactory.GetDefaultWeaponBoneIdForCharacterWhenUseAsEnemy(text));
			}
			int num3 = 0;
			foreach (object obj2 in array[k].gameObject.transform)
			{
				Transform transform3 = (Transform)obj2;
				if (transform3.name.Contains("mesh"))
				{
					num3++;
				}
			}
			array[k].meshIsRendering = new bool[num3];
			for (int m = 0; m < num3; m++)
			{
				array[k].meshIsRendering[m] = true;
			}
			array[k].meshCount = num3;
			array[k].animation = array[k].gameObject.GetComponent<Animation>();
			array[k].animation = array[k].gameObject.GetComponent<Animation>();
			array[k].typeNo = typeNo;
			array[k].idleAnimationName = this.animationName[k];
		}
	}

	// Token: 0x0600039F RID: 927 RVA: 0x0006C604 File Offset: 0x0006A804
	public void SetFrontRow()
	{
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
		{
			if (next.bi.player != 0)
			{
				Vector3 pos = next.pos;
				pos.z = (float)((!FF9StateSystem.Battle.isFrontRow) ? -1960 : -1560);
				next.pos = pos;
			}
		}
	}

	// Token: 0x060003A0 RID: 928 RVA: 0x0006C67C File Offset: 0x0006A87C
	private int ChoicePattern()
	{
		int i = UnityEngine.Random.Range(0, 100);
		int num = (int)this.btlScene.PatAddr[0].Rate;
		int num2 = 0;
		while (i >= num)
		{
			num += (int)this.btlScene.PatAddr[num2 + 1].Rate;
			num2++;
		}
		if ((int)FF9StateSystem.Common.FF9.btlSubMapNo != -1)
		{
			num2 = (int)FF9StateSystem.Common.FF9.btlSubMapNo;
		}
		if (num2 < 0 || num2 >= (int)this.btlScene.header.PatCount)
		{
			num2 = 0;
		}
		FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
		ff9Battle.btl_scene.PatNum = (byte)num2;
		return num2;
	}

	// Token: 0x1700005F RID: 95
	// (get) Token: 0x060003A1 RID: 929 RVA: 0x00018DA3 File Offset: 0x00016FA3
	public static float FrameTime
	{
		get
		{
			return HonoluluBattleMain.frameTime;
		}
	}

	// Token: 0x17000060 RID: 96
	// (get) Token: 0x060003A2 RID: 930 RVA: 0x00018DAA File Offset: 0x00016FAA
	public static float FPS
	{
		get
		{
			return (float)HonoluluBattleMain.fps;
		}
	}

	// Token: 0x060003A3 RID: 931
	public static void UpdateFrameTime(int _speed)
	{
		HonoluluBattleMain.Speed = _speed;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
		{
			bool flag = next.gameObject == null;
			bool flag2 = flag;
			bool flag3 = flag2;
			bool flag4 = flag3;
			bool flag5 = flag4;
			bool flag6 = flag5;
			if (flag6)
			{
				return;
			}
			Animation component = next.gameObject.GetComponent<Animation>();
			foreach (object obj in component)
			{
				AnimationState animationState = (AnimationState)obj;
			}
		}
		HonoluluBattleMain.fps = (15 + BattleHUD.battleSpeed) * _speed;
		HonoluluBattleMain.frameTime = 1f / (float)HonoluluBattleMain.fps;
	}

	// Token: 0x060003A4 RID: 932 RVA: 0x0006C814 File Offset: 0x0006AA14
	public static void playCommand(int characterNo, int slotNo, int target, bool isTrance = false)
	{
		if (slotNo < 0 || slotNo > 6)
		{
			global::Debug.LogError("slot number value can be only 0 to 5");
			return;
		}
		if (characterNo < 0 || characterNo > 4)
		{
			global::Debug.LogError("character number value can be only 1 to 4");
			return;
		}
		BTL_DATA regist = FF9StateSystem.Battle.FF9Battle.btl_data[characterNo];
		PLAYER player = FF9StateSystem.Common.FF9.party.member[characterNo];
		byte menu_type = player.info.menu_type;
		uint num = 0u;
		uint num2 = 0u;
		switch (slotNo)
		{
		case 0:
			num = 1u;
			num2 = (uint)rdata._FF9FAbil_ComData[(int)((UIntPtr)num)].ability;
			break;
		case 1:
			if (isTrance)
			{
				num = (uint)rdata._FF9BMenu_MenuTrance[(int)menu_type, 0];
			}
			else
			{
				num = (uint)rdata._FF9BMenu_MenuNormal[(int)menu_type, 0];
			}
			num2 = (uint)rdata._FF9FAbil_ComData[(int)((UIntPtr)num)].ability;
			break;
		case 2:
			if (isTrance)
			{
				num = (uint)rdata._FF9BMenu_MenuTrance[(int)menu_type, 1];
			}
			else
			{
				num = (uint)rdata._FF9BMenu_MenuNormal[(int)menu_type, 1];
			}
			num2 = (uint)rdata._FF9FAbil_ComData[(int)((UIntPtr)num)].ability;
			break;
		case 3:
			num = 14u;
			num2 = 236u;
			break;
		case 4:
			num = 4u;
			break;
		case 5:
			num = 7u;
			num2 = (uint)rdata._FF9FAbil_ComData[(int)((UIntPtr)num)].ability;
			break;
		}
		if (rdata._FF9FAbil_ComData[(int)((UIntPtr)num)].type == 1)
		{
			num2 = (uint)rdata._FF9BMenu_ComAbil[(int)((UIntPtr)num2)];
		}
		else if (rdata._FF9FAbil_ComData[(int)((UIntPtr)num)].type == 3)
		{
			num2 = 1u;
		}
		if (num == 0u)
		{
			return;
		}
		btl_cmd.SetCommand(new CMD_DATA
		{
			regist = regist
		}, num, num2, (ushort)target, 0u);
	}

	// Token: 0x060003A5 RID: 933 RVA: 0x0006C9C8 File Offset: 0x0006ABC8
	private void YMenu_ManagerAt()
	{
		BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next;
		if (!UIManager.Battle.FF9BMenu_IsEnableAtb())
		{
			return;
		}
		while (next != null)
		{
			if (next.sel_mode == 0 && next.sel_menu == 0 && next.cur.hp != 0 && next.bi.atb != 0)
			{
				POINTS cur = next.cur;
				POINTS max = next.max;
				POINTS points = cur;
				points.at += (short)((int)cur.at_coef * 4);
				if (cur.at >= max.at)
				{
					cur.at = max.at;
					if (!btl_stat.CheckStatus(next, 33685506u))
					{
						if (next.bi.player != 0)
						{
							if (btl_stat.CheckStatus(next, 3072u))
							{
								int num = 0;
								while (1 << num != (int)next.btl_id)
								{
									num++;
								}
								if (!UIManager.Battle.InputFinishList.Contains(num))
								{
									next.sel_mode = 1;
									btl_cmd.SetAutoCommand(next);
								}
							}
							else
							{
								int num2 = 0;
								while (1 << num2 != (int)next.btl_id)
								{
									num2++;
								}
								if (!UIManager.Battle.ReadyQueue.Contains(num2))
								{
									UIManager.Battle.AddPlayerToReady(num2);
								}
							}
						}
						else if (!FF9StateSystem.Battle.isDebug)
						{
							if (PersistenSingleton<EventEngine>.Instance.RequestAction(47, (int)next.btl_id, 0, 0) != 0)
							{
								next.sel_mode = 1;
							}
						}
						else
						{
							int num3 = Array.IndexOf<byte>(FF9StateSystem.Battle.FF9Battle.seq_work_set.AnmOfsList, this.btlIDList[(int)btl_scrp.GetBtlDataPtr(next.btl_id).typeNo]);
							if (num3 < 0)
							{
								global::Debug.LogError("Index out of range");
							}
							int num4 = UnityEngine.Random.Range(0, 4);
							if (FF9StateSystem.Battle.FF9Battle.btl_phase != 4)
							{
								return;
							}
						}
					}
				}
			}
			next = next.next;
		}
	}

	// Token: 0x060003A6 RID: 934 RVA: 0x0006CBD8 File Offset: 0x0006ADD8
	private void Update()
	{
		this.UpdateAttachModel();
		this.cumulativeTime += Time.deltaTime;
		if (this.needClampTime)
		{
			this.cumulativeTime = Mathf.Min(this.cumulativeTime, HonoluluBattleMain.frameTime * (float)SettingsState.FastForwardGameSpeed * 1.2f);
		}
		while (this.cumulativeTime >= HonoluluBattleMain.frameTime)
		{
			this.cumulativeTime -= HonoluluBattleMain.frameTime;
			HonoluluBattleMain.battleSPS.Service();
			if ((FF9StateSystem.Battle.FF9Battle.attr & 4096) == 0)
			{
				if ((FF9StateSystem.Battle.FF9Battle.attr & 256) == 0)
				{
					this.battleResult = (ulong)battle.BattleMain();
					if (!FF9StateSystem.Battle.isDebug)
					{
						if (UIManager.Battle.FF9BMenu_IsEnable())
						{
							this.YMenu_ManagerAt();
						}
						if (this.battleResult == 1UL)
						{
							PersistenSingleton<FF9StateSystem>.Instance.mode = 8;
							FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
							ff9Battle.attr |= 4096;
						}
					}
					SceneDirector.ServiceFade();
				}
			}
			else
			{
				FF9StateGlobal ff = FF9StateSystem.Common.FF9;
				FF9StateGlobal ff9StateGlobal = ff;
				switch (ff9StateGlobal.btl_result)
				{
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 7:
					if (ff9StateGlobal.btl_result == 2)
					{
						ff9StateGlobal.btl_result = 1;
					}
					if (FF9StateSystem.Battle.FF9Battle.map.nextMode == 1 || FF9StateSystem.Battle.FF9Battle.map.nextMode == 5)
					{
						FF9StateSystem.Common.FF9.fldMapNo = FF9StateSystem.Battle.FF9Battle.map.nextMapNo;
					}
					else if (FF9StateSystem.Battle.FF9Battle.map.nextMode == 3)
					{
						FF9StateSystem.Common.FF9.wldMapNo = FF9StateSystem.Battle.FF9Battle.map.nextMapNo;
					}
					UIManager.Battle.GoToBattleResult();
					if (!FF9StateSystem.Battle.isDebug)
					{
						PersistenSingleton<EventEngine>.Instance.ServiceEvents();
						SceneDirector.ServiceFade();
					}
					break;
				case 6:
					UIManager.Battle.GoToGameOver();
					break;
				}
			}
		}
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
		{
			if (next.bi.slave == 0 && next.bi.disappear == 0)
			{
				if (next.bi.shadow != 0)
				{
					FF9StateBattleSystem ff9Battle2 = FF9StateSystem.Battle.FF9Battle;
					int boneNo = ff9btl.ff9btl_set_bone(next.shadow_bone[0], next.shadow_bone[1]);
					if (next.bi.player != 0)
					{
						if ((ff9Battle2.cmd_status & 1) == 0)
						{
							if ((next.escape_key ^ ff9Battle2.btl_escape_key) != 0)
							{
								btl_mot.SetDefaultIdle(next);
							}
							next.escape_key = ff9Battle2.btl_escape_key;
						}
						btlseq.FF9DrawShadowCharBattle(ff9Battle2.map.shadowArray, (int)next.bi.slot_no, 0, boneNo);
					}
					else if (next.die_seq < 4)
					{
						btlseq.FF9DrawShadowCharBattle(ff9Battle2.map.shadowArray, (int)(9 + next.bi.slot_no), 0, boneNo);
					}
				}
			}
		}
	}

	// Token: 0x060003A7 RID: 935 RVA: 0x0006CF38 File Offset: 0x0006B138
	private void UpdateAttachModel()
	{
		if (HonoluluBattleMain.battleSceneName == "EF_E006" || HonoluluBattleMain.battleSceneName == "EF_E007")
		{
			GameObject gameObject = FF9StateSystem.Battle.FF9Battle.btl_data[4].gameObject;
			GameObject gameObject2 = FF9StateSystem.Battle.FF9Battle.btl_data[5].gameObject;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			gameObject2.transform.localScale = Vector3.one;
			this.rootBone.transform.localPosition = Vector3.zero;
			this.rootBone.transform.localRotation = Quaternion.identity;
			this.rootBone.transform.localScale = Vector3.one;
		}
	}

	// Token: 0x060003A8 RID: 936 RVA: 0x0006D00C File Offset: 0x0006B20C
	public void GeoBattleAttach(GameObject src, GameObject target, int boneIndex)
	{
		this.rootBone = src.transform.GetChildByName("bone000");
		Transform childByName = target.transform.GetChildByName("bone" + boneIndex.ToString("D3"));
		src.transform.parent = childByName.transform;
		src.transform.localPosition = Vector3.zero;
		src.transform.localRotation = Quaternion.identity;
		src.transform.localScale = Vector3.one;
		this.rootBone.transform.localPosition = Vector3.zero;
		this.rootBone.transform.localRotation = Quaternion.identity;
		this.rootBone.transform.localScale = Vector3.one;
	}

	// Token: 0x060003A9 RID: 937 RVA: 0x00018DB2 File Offset: 0x00016FB2
	private void LateUpdate()
	{
		this.UpdateAttachModel();
		UIManager.Battle.modelButtonManager.UpdateModelButtonPosition();
		Singleton<HUDMessage>.Instance.UpdateChildPosition();
	}

	// Token: 0x060003AA RID: 938 RVA: 0x00018DD3 File Offset: 0x00016FD3
	public int GetPlayerSerialNum(int battlePlayerPosID)
	{
		return HonoluluBattleMain.CurPlayerSerialNum[battlePlayerPosID];
	}

	// Token: 0x060003AB RID: 939 RVA: 0x00018DDC File Offset: 0x00016FDC
	public int GetWeaponID(int battlePlayerPosID)
	{
		return HonoluluBattleMain.CurPlayerWeaponIndex[battlePlayerPosID];
	}

	// Token: 0x060003AC RID: 940 RVA: 0x0006D0D4 File Offset: 0x0006B2D4
	private void OnGUI()
	{
		if (!EventEngineUtils.showDebugUI)
		{
			return;
		}
		Rect fullscreenRect = DebugGuiSkin.GetFullscreenRect();
		DebugGuiSkin.ApplySkin();
		if (!FF9StateSystem.Battle.isDebug)
		{
			GUILayout.BeginArea(fullscreenRect);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			return;
		}
	}

	// Token: 0x060003AD RID: 941 RVA: 0x0006D124 File Offset: 0x0006B324
	private void OnDestroy()
	{
		SFX.EndBattle();
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
		{
			if (next.texanimptr != null)
			{
				foreach (KeyValuePair<int, RenderTexture> keyValuePair in next.texanimptr.RenderTexMapping)
				{
					keyValuePair.Value.Release();
				}
				next.texanimptr.RenderTexMapping.Clear();
			}
			if (next.bi.player != 0 && next.tranceTexanimptr != null)
			{
				foreach (KeyValuePair<int, RenderTexture> keyValuePair2 in next.tranceTexanimptr.RenderTexMapping)
				{
					keyValuePair2.Value.Release();
				}
				next.tranceTexanimptr.RenderTexMapping.Clear();
			}
		}
		if (battlebg.nf_BbgTabAddress != null)
		{
			foreach (KeyValuePair<int, RenderTexture> keyValuePair3 in battlebg.nf_BbgTabAddress.RenderTexMapping)
			{
				keyValuePair3.Value.Release();
			}
			FF9StateSystem.Battle.FF9Battle.map.btlBGTexAnimPtr.RenderTexMapping.Clear();
		}
	}

	// Token: 0x060003AE RID: 942 RVA: 0x00018DE5 File Offset: 0x00016FE5
	public void SetActive(bool active)
	{
		if (active)
		{
			this.ResumeBattle();
		}
		else
		{
			this.PauseBattle();
		}
	}

	// Token: 0x060003AF RID: 943 RVA: 0x0006D2CC File Offset: 0x0006B4CC
	private void PauseBattle()
	{
		FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
		ff9Battle.attr |= 256;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
		{
			if (next.animation != null)
			{
				next.animation.enabled = false;
			}
		}
	}

	// Token: 0x060003B0 RID: 944 RVA: 0x0006D33C File Offset: 0x0006B53C
	private void ResumeBattle()
	{
		FF9StateBattleSystem ff9Battle = FF9StateSystem.Battle.FF9Battle;
		ff9Battle.attr &= 65279;
		for (BTL_DATA next = FF9StateSystem.Battle.FF9Battle.btl_list.next; next != null; next = next.next)
		{
			if (next.animation != null)
			{
				next.animation.enabled = true;
			}
		}
	}

	// Token: 0x060003B2 RID: 946 RVA: 0x0006D3AC File Offset: 0x0006B5AC
	public static void Write()
	{
		bool flag = BattleHUD.countdown >= 1f;
		bool flag2 = flag;
		bool flag3 = flag2;
		if (flag3)
		{
			BattleHUD.countdown -= Time.deltaTime;
		}
		else
		{
			BattleHUD.countdown = 5f;
			HonoluluBattleMain.Read();
			HonoluluBattleMain.Write();
		}
	}

	// Token: 0x060003B3 RID: 947
	public static void Read()
	{
		string pattern = "\\[(.*?)\\]";
		MatchCollection matchCollection = Regex.Matches(File.ReadAllText(Application.dataPath + "\\Config.txt"), pattern);
		string[] array = new string[5];
		int num = 0;
		checked
		{
			foreach (object obj in matchCollection)
			{
				Match match = (Match)obj;
				array[num] = match.Value;
				array[num] = array[num].Replace("[", "");
				array[num] = array[num].Replace("]", "");
				num++;
			}
			BattleHUD.scale = float.Parse(array[0]);
			BattleHUD.hD = int.Parse(array[1]);
			BattleHUD.encounterRate = int.Parse(array[2]);
			BattleHUD.BattleSwirl = int.Parse(array[3]);
			BattleHUD.battleSpeed = int.Parse(array[4]);
			HonoluluBattleMain.Write();
			Console.WriteLine(string.Concat(new string[]
			{
				"Scale:",
				BattleHUD.scale.ToString(),
				" BattleSwirl:",
				BattleHUD.hD.ToString(),
				" Encounter Rate:",
				BattleHUD.encounterRate.ToString(),
				" "
			}));
		}
	}

	// Token: 0x04000773 RID: 1907
	public BTL_SCENE btlScene;

	// Token: 0x04000774 RID: 1908
	public static string battleSceneName;

	// Token: 0x04000775 RID: 1909
	public BattleMapCameraController cameraController;

	// Token: 0x04000776 RID: 1910
	private bool isSetup;

	// Token: 0x04000777 RID: 1911
	public btlseq btlSeq;

	// Token: 0x04000778 RID: 1912
	public string[] animationName;

	// Token: 0x04000779 RID: 1913
	private static readonly int[] CurPlayerSerialNum = new int[4];

	// Token: 0x0400077A RID: 1914
	public static int[] CurPlayerWeaponIndex = new int[4];

	// Token: 0x0400077B RID: 1915
	public static int EnemyStartIndex = 4;

	// Token: 0x0400077C RID: 1916
	private readonly int[][] playerXPos = new int[][]
	{
		new int[1],
		new int[]
		{
			316,
			-316
		},
		new int[]
		{
			632,
			0,
			-632
		},
		new int[]
		{
			948,
			316,
			-316,
			-948
		}
	};

	// Token: 0x0400077D RID: 1917
	private List<Material> playerMaterials;

	// Token: 0x0400077E RID: 1918
	private List<Material> monsterMaterials;

	// Token: 0x0400077F RID: 1919
	private HonoluluBattleMain.BattleState battleState;

	// Token: 0x04000780 RID: 1920
	public static bool playerEnterCommand;

	// Token: 0x04000781 RID: 1921
	public bool playerCastingSkill;

	// Token: 0x04000782 RID: 1922
	public bool enemyEnterCommand;

	// Token: 0x04000783 RID: 1923
	public List<int> seqList;

	// Token: 0x04000784 RID: 1924
	private byte[] btlIDList;

	// Token: 0x04000785 RID: 1925
	private ulong battleResult;

	// Token: 0x04000786 RID: 1926
	private float debugUILastTouchTime;

	// Token: 0x04000787 RID: 1927
	private float showHideDebugUICoolDown = 0.5f;

	// Token: 0x04000788 RID: 1928
	private Transform rootBone;

	// Token: 0x04000789 RID: 1929
	public static BattleSPSSystem battleSPS;

	// Token: 0x0400078A RID: 1930
	private string scaleEdit = string.Empty;

	// Token: 0x0400078B RID: 1931
	private string distanceEdit = string.Empty;

	// Token: 0x0400078C RID: 1932
	private bool needClampTime;

	// Token: 0x0400078D RID: 1933
	private uint counter;

	// Token: 0x0400078E RID: 1934
	private float cumulativeTime;

	// Token: 0x0400078F RID: 1935
	public static int Speed;

	// Token: 0x04000790 RID: 1936
	private static int fps;

	// Token: 0x04000791 RID: 1937
	private static float frameTime;

	// Token: 0x04000792 RID: 1938
	private bool isKeyFrame;

	// Token: 0x04000794 RID: 1940
	public static float scale;

	// Token: 0x04000795 RID: 1941
	public static int hD;

	// Token: 0x04000796 RID: 1942
	public static int encounterRate;

	// Token: 0x04000797 RID: 1943
	public static int BattleSwirl;

	// Token: 0x04000798 RID: 1944
	public static int battleSpeed;

	// Token: 0x020000C4 RID: 196
	public enum BattleState
	{
		// Token: 0x0400079A RID: 1946
		PlayerTurn,
		// Token: 0x0400079B RID: 1947
		EnemyTurn
	}
}
