using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020001FF RID: 511
public class BGSCENE_DEF : MonoBehaviour {
	// Token: 0x06000C02 RID: 3074 RVA: 0x00101B40 File Offset: 0x000FFD40
	public BGSCENE_DEF (bool useUpscaleFm) {
		this.useUpscaleFM = useUpscaleFm;
		this.name = string.Empty;
		this.ebgBin = null;
		this.overlayList = new List<BGOVERLAY_DEF> ();
		this.animList = new List<BGANIM_DEF> ();
		this.lightList = new List<BGLIGHT_DEF> ();
		this.cameraList = new List<BGCAM_DEF> ();
		this.materialList = new Dictionary<string, Material> ();
	}

	// Token: 0x06000C03 RID: 3075 RVA: 0x00101BD0 File Offset: 0x000FFDD0
	private void InitPSXTextureAtlas () {
		this.vram = new PSXVram (true);
		this.atlas = checked (new Texture2D ((int) this.ATLAS_W, (int) this.ATLAS_H));
		this.atlas.filterMode = FilterMode.Point;
		this.atlas.wrapMode = TextureWrapMode.Clamp;
		this.SPRITE_W = 16u;
		this.SPRITE_H = 16u;
	}

	// Token: 0x06000C04 RID: 3076 RVA: 0x0001E4C2 File Offset: 0x0001C6C2
	public void ReadData (BinaryReader reader) {
		this.ExtractHeaderData (reader);
		this.ExtractOverlayData (reader);
		this.ExtractSpriteData (reader);
		this.ExtractAnimationData (reader);
		this.ExtractAnimationFrameData (reader);
		this.ExtractLightData (reader);
		this.ExtractCameraData (reader);
	}

	// Token: 0x06000C05 RID: 3077 RVA: 0x00101C2C File Offset: 0x000FFE2C
	private void ExtractHeaderData (BinaryReader reader) {
		this.sceneLength = reader.ReadUInt16 ();
		this.depthBitShift = reader.ReadUInt16 ();
		this.animCount = reader.ReadUInt16 ();
		this.overlayCount = reader.ReadUInt16 ();
		this.lightCount = reader.ReadUInt16 ();
		this.cameraCount = reader.ReadUInt16 ();
		this.animOffset = reader.ReadUInt32 ();
		this.overlayOffset = reader.ReadUInt32 ();
		this.lightOffset = reader.ReadUInt32 ();
		this.cameraOffset = reader.ReadUInt32 ();
		this.orgZ = reader.ReadInt16 ();
		this.curZ = reader.ReadInt16 ();
		this.orgX = reader.ReadInt16 ();
		this.orgY = reader.ReadInt16 ();
		this.curX = reader.ReadInt16 ();
		this.curY = reader.ReadInt16 ();
		this.minX = reader.ReadInt16 ();
		this.maxX = reader.ReadInt16 ();
		this.minY = reader.ReadInt16 ();
		this.maxY = reader.ReadInt16 ();
		this.scrX = reader.ReadInt16 ();
		this.scrY = reader.ReadInt16 ();
	}

	// Token: 0x06000C06 RID: 3078 RVA: 0x00101D44 File Offset: 0x000FFF44
	private void ExtractCameraData (BinaryReader reader) {
		checked {
			reader.BaseStream.Seek ((long) (unchecked ((ulong) this.cameraOffset)), SeekOrigin.Begin);
			for (int i = 0; i < (int) this.cameraCount; i++) {
				BGCAM_DEF bgcam_DEF = new BGCAM_DEF ();
				bgcam_DEF.ReadData (reader);
				this.cameraList.Add (bgcam_DEF);
			}
		}
	}

	// Token: 0x06000C07 RID: 3079 RVA: 0x00101D90 File Offset: 0x000FFF90
	private void ExtractLightData (BinaryReader reader) {
		checked {
			reader.BaseStream.Seek ((long) (unchecked ((ulong) this.lightOffset)), SeekOrigin.Begin);
			for (int i = 0; i < (int) this.lightCount; i++) {
				BGLIGHT_DEF bglight_DEF = new BGLIGHT_DEF ();
				bglight_DEF.ReadData (reader);
				this.lightList.Add (bglight_DEF);
			}
		}
	}

	// Token: 0x06000C08 RID: 3080 RVA: 0x00101DDC File Offset: 0x000FFFDC
	private void ExtractAnimationFrameData (BinaryReader reader) {
		checked {
			for (int i = 0; i < (int) this.animCount; i++) {
				BGANIM_DEF bganim_DEF = this.animList[i];
				reader.BaseStream.Seek ((long) (unchecked ((ulong) bganim_DEF.offset)), SeekOrigin.Begin);
				for (int j = 0; j < bganim_DEF.frameCount; j++) {
					BGANIMFRAME_DEF bganimframe_DEF = new BGANIMFRAME_DEF ();
					bganimframe_DEF.ReadData (reader);
					bganim_DEF.frameList.Add (bganimframe_DEF);
				}
			}
		}
	}

	// Token: 0x06000C09 RID: 3081 RVA: 0x00101E48 File Offset: 0x00100048
	private void ExtractAnimationData (BinaryReader reader) {
		checked {
			reader.BaseStream.Seek ((long) (unchecked ((ulong) this.animOffset)), SeekOrigin.Begin);
			for (int i = 0; i < (int) this.animCount; i++) {
				BGANIM_DEF bganim_DEF = new BGANIM_DEF ();
				bganim_DEF.ReadData (reader);
				this.animList.Add (bganim_DEF);
			}
		}
	}

	// Token: 0x06000C0A RID: 3082 RVA: 0x00101E94 File Offset: 0x00100094
	private void ExtractSpriteData (BinaryReader reader) {
		checked {
			if (BattleHUD.hD == 1) {
				this.spriteCount = 0;
				for (int i = 0; i < (int) this.overlayCount; i++) {
					BGOVERLAY_DEF bgoverlay_DEF = this.overlayList[i];
					this.spriteCount += (int) bgoverlay_DEF.spriteCount;
				}
				if (this.useUpscaleFM) {
					this.ATLAS_H = (uint) this.atlas.height;
					this.ATLAS_W = (uint) this.atlas.width;
				}
				int num = this.atlas.width / 68;
				int num2 = 0;
				for (int j = 0; j < (int) this.overlayCount; j++) {
					BGOVERLAY_DEF bgoverlay_DEF2 = this.overlayList[j];
					reader.BaseStream.Seek ((long) (unchecked ((ulong) bgoverlay_DEF2.prmOffset)), SeekOrigin.Begin);
					for (int k = 0; k < (int) bgoverlay_DEF2.spriteCount; k++) {
						BGSPRITE_LOC_DEF bgsprite_LOC_DEF = new BGSPRITE_LOC_DEF ();
						bgsprite_LOC_DEF.ReadData_BGSPRITE_DEF (reader);
						bgoverlay_DEF2.spriteList.Add (bgsprite_LOC_DEF);
					}
					reader.BaseStream.Seek ((long) (unchecked ((ulong) bgoverlay_DEF2.locOffset)), SeekOrigin.Begin);
					for (int l = 0; l < (int) bgoverlay_DEF2.spriteCount; l++) {
						BGSPRITE_LOC_DEF bgsprite_LOC_DEF2 = bgoverlay_DEF2.spriteList[l];
						bgsprite_LOC_DEF2.ReadData_BGSPRITELOC_DEF (reader);
						if (this.useUpscaleFM) {
							bgsprite_LOC_DEF2.atlasX = (ushort) (2 + num2 % num * 68);
							bgsprite_LOC_DEF2.atlasY = (ushort) (2 + num2 / num * 68);
							bgsprite_LOC_DEF2.w = 64;
							bgsprite_LOC_DEF2.h = 64;
							num2++;
						}
					}
				}
				return;
			}
			this.spriteCount = 0;
			for (int m = 0; m < (int) this.overlayCount; m++) {
				BGOVERLAY_DEF bgoverlay_DEF3 = this.overlayList[m];
				this.spriteCount += (int) bgoverlay_DEF3.spriteCount;
			}
			if (this.useUpscaleFM) {
				this.ATLAS_H = (uint) this.atlas.height;
				this.ATLAS_W = (uint) this.atlas.width;
			}
			int num3 = this.atlas.width / 36;
			int num4 = 0;
			for (int n = 0; n < (int) this.overlayCount; n++) {
				BGOVERLAY_DEF bgoverlay_DEF4 = this.overlayList[n];
				reader.BaseStream.Seek ((long) (unchecked ((ulong) bgoverlay_DEF4.prmOffset)), SeekOrigin.Begin);
				for (int num5 = 0; num5 < (int) bgoverlay_DEF4.spriteCount; num5++) {
					BGSPRITE_LOC_DEF bgsprite_LOC_DEF3 = new BGSPRITE_LOC_DEF ();
					bgsprite_LOC_DEF3.ReadData_BGSPRITE_DEF (reader);
					bgoverlay_DEF4.spriteList.Add (bgsprite_LOC_DEF3);
				}
				reader.BaseStream.Seek ((long) (unchecked ((ulong) bgoverlay_DEF4.locOffset)), SeekOrigin.Begin);
				for (int num6 = 0; num6 < (int) bgoverlay_DEF4.spriteCount; num6++) {
					BGSPRITE_LOC_DEF bgsprite_LOC_DEF4 = bgoverlay_DEF4.spriteList[num6];
					bgsprite_LOC_DEF4.ReadData_BGSPRITELOC_DEF (reader);
					if (this.useUpscaleFM) {
						bgsprite_LOC_DEF4.atlasX = (ushort) (2 + num4 % num3 * 36);
						bgsprite_LOC_DEF4.atlasY = (ushort) (2 + num4 / num3 * 36);
						bgsprite_LOC_DEF4.w = 32;
						bgsprite_LOC_DEF4.h = 32;
						num4++;
					}
				}
			}
		}
	}

	// Token: 0x06000C0B RID: 3083 RVA: 0x00102190 File Offset: 0x00100390
	private void ExtractOverlayData (BinaryReader reader) {
		checked {
			reader.BaseStream.Seek ((long) (unchecked ((ulong) this.overlayOffset)), SeekOrigin.Begin);
			for (int i = 0; i < (int) this.overlayCount; i++) {
				BGOVERLAY_DEF bgoverlay_DEF = new BGOVERLAY_DEF ();
				bgoverlay_DEF.ReadData (reader);
				bgoverlay_DEF.minX = short.MinValue;
				bgoverlay_DEF.maxX = short.MaxValue;
				bgoverlay_DEF.minY = short.MinValue;
				bgoverlay_DEF.maxY = short.MaxValue;
				this.overlayList.Add (bgoverlay_DEF);
			}
		}
	}

	// Token: 0x06000C0C RID: 3084 RVA: 0x00102208 File Offset: 0x00100408
	private void _LoadDummyEBG (BGSCENE_DEF sceneUS, string path, string name, FieldMapLocalizeAreaTitleInfo info, string localizeSymbol) {
		checked {
			if (BattleHUD.hD == 1) {
				this.name = name;
				TextAsset textAsset = AssetManager.Load<TextAsset> (string.Concat (new string[] {
					path,
					name,
					"_",
					localizeSymbol,
					".bgs"
				}), false);
				if (textAsset == null) {
					return;
				}
				this.ebgBin = textAsset.bytes;
				using (BinaryReader binaryReader = new BinaryReader (new MemoryStream (this.ebgBin))) {
					this.ExtractHeaderData (binaryReader);
					this.ExtractOverlayData (binaryReader);
					int width = sceneUS.atlas.width;
					int height = sceneUS.atlas.height;
					int startOvrIdx = info.startOvrIdx;
					int endOvrIdx = info.endOvrIdx;
					bool hasUK = info.hasUK;
					int spriteStartIndex = info.GetSpriteStartIndex (localizeSymbol);
					int num = width / 68;
					int num2 = spriteStartIndex;
					for (int i = startOvrIdx; i <= endOvrIdx; i++) {
						BGOVERLAY_DEF bgoverlay_DEF = this.overlayList[i];
						binaryReader.BaseStream.Seek ((long) (unchecked ((ulong) bgoverlay_DEF.prmOffset)), SeekOrigin.Begin);
						for (int j = 0; j < (int) bgoverlay_DEF.spriteCount; j++) {
							BGSPRITE_LOC_DEF bgsprite_LOC_DEF = new BGSPRITE_LOC_DEF ();
							bgsprite_LOC_DEF.ReadData_BGSPRITE_DEF (binaryReader);
							bgoverlay_DEF.spriteList.Add (bgsprite_LOC_DEF);
						}
						binaryReader.BaseStream.Seek ((long) (unchecked ((ulong) bgoverlay_DEF.locOffset)), SeekOrigin.Begin);
						for (int k = 0; k < (int) bgoverlay_DEF.spriteCount; k++) {
							BGSPRITE_LOC_DEF bgsprite_LOC_DEF2 = bgoverlay_DEF.spriteList[k];
							bgsprite_LOC_DEF2.ReadData_BGSPRITELOC_DEF (binaryReader);
							if (this.useUpscaleFM) {
								bgsprite_LOC_DEF2.atlasX = (ushort) (2 + num2 % num * 68);
								bgsprite_LOC_DEF2.atlasY = (ushort) (2 + num2 / num * 68);
								bgsprite_LOC_DEF2.w = 64;
								bgsprite_LOC_DEF2.h = 64;
								num2++;
							}
						}
					}
					for (int l = startOvrIdx; l <= endOvrIdx; l++) {
						sceneUS.overlayList[l] = this.overlayList[l];
					}
					return;
				}
			}
			this.name = name;
			TextAsset textAsset2 = AssetManager.Load<TextAsset> (string.Concat (new string[] {
				path,
				name,
				"_",
				localizeSymbol,
				".bgs"
			}), false);
			if (textAsset2 == null) {
				return;
			}
			this.ebgBin = textAsset2.bytes;
			using (BinaryReader binaryReader2 = new BinaryReader (new MemoryStream (this.ebgBin))) {
				this.ExtractHeaderData (binaryReader2);
				this.ExtractOverlayData (binaryReader2);
				int atlasWidth = info.atlasWidth;
				int atlasHeight = info.atlasHeight;
				int startOvrIdx2 = info.startOvrIdx;
				int endOvrIdx2 = info.endOvrIdx;
				bool hasUK2 = info.hasUK;
				int spriteStartIndex2 = info.GetSpriteStartIndex (localizeSymbol);
				int num3 = atlasWidth / 36;
				int num4 = spriteStartIndex2;
				for (int m = startOvrIdx2; m <= endOvrIdx2; m++) {
					BGOVERLAY_DEF bgoverlay_DEF2 = this.overlayList[m];
					binaryReader2.BaseStream.Seek ((long) (unchecked ((ulong) bgoverlay_DEF2.prmOffset)), SeekOrigin.Begin);
					for (int n = 0; n < (int) bgoverlay_DEF2.spriteCount; n++) {
						BGSPRITE_LOC_DEF bgsprite_LOC_DEF3 = new BGSPRITE_LOC_DEF ();
						bgsprite_LOC_DEF3.ReadData_BGSPRITE_DEF (binaryReader2);
						bgoverlay_DEF2.spriteList.Add (bgsprite_LOC_DEF3);
					}
					binaryReader2.BaseStream.Seek ((long) (unchecked ((ulong) bgoverlay_DEF2.locOffset)), SeekOrigin.Begin);
					for (int num5 = 0; num5 < (int) bgoverlay_DEF2.spriteCount; num5++) {
						BGSPRITE_LOC_DEF bgsprite_LOC_DEF4 = bgoverlay_DEF2.spriteList[num5];
						bgsprite_LOC_DEF4.ReadData_BGSPRITELOC_DEF (binaryReader2);
						if (this.useUpscaleFM) {
							bgsprite_LOC_DEF4.atlasX = (ushort) (2 + num4 % num3 * 36);
							bgsprite_LOC_DEF4.atlasY = (ushort) (2 + num4 / num3 * 36);
							if (BattleHUD.hD == 1) {
								bgsprite_LOC_DEF4.w = 64;
								bgsprite_LOC_DEF4.h = 64;
							} else {
								bgsprite_LOC_DEF4.w = 32;
								bgsprite_LOC_DEF4.h = 32;
							}
							num4++;
						}
					}
				}
				for (int num6 = startOvrIdx2; num6 <= endOvrIdx2; num6++) {
					sceneUS.overlayList[num6] = this.overlayList[num6];
				}
			}
		}
	}

	// Token: 0x06000C0D RID: 3085 RVA: 0x00102638 File Offset: 0x00100838
	public void LoadEBG (FieldMap fieldMap, string path, string name) {
		if (BattleHUD.hD == 1) {
			this.name = name;
			if (!this.useUpscaleFM) {
				this.InitPSXTextureAtlas ();
			} else {
				Texture2D x = AssetManager.Load<Texture2D> (Path.Combine (path, "atlas"), false);
				if (x != null) {
					this.atlas = x;
					if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor) {
						this.atlasAlpha = AssetManager.Load<Texture2D> (Path.Combine (path, "atlas_a"), false);
					} else {
						this.atlasAlpha = null;
					}
					this.SPRITE_W = 64u;
					this.SPRITE_H = 64u;
				} else {
					this.useUpscaleFM = false;
					this.InitPSXTextureAtlas ();
				}
			}
			if (!this.useUpscaleFM) {
				this.vram.LoadTIMs (path);
			}
			TextAsset textAsset;
			if (!FieldMapEditor.useOriginalVersion) {
				textAsset = AssetManager.Load<TextAsset> (path + FieldMapEditor.GetFieldMapModName (name) + ".bgs", false);
				if (textAsset == null) {
					global::Debug.Log ("Cannot find MOD version.");
					textAsset = AssetManager.Load<TextAsset> (path + name + ".bgs", false);
				}
			} else {
				textAsset = AssetManager.Load<TextAsset> (path + name + ".bgs", false);
			}
			if (textAsset == null) {
				return;
			}
			this.ebgBin = textAsset.bytes;
			using (BinaryReader binaryReader = new BinaryReader (new MemoryStream (this.ebgBin))) {
				this.ReadData (binaryReader);
			}
			string symbol = Localization.GetSymbol ();
			if (symbol != "US") {
				FieldMapLocalizeAreaTitleInfo info = FieldMapInfo.localizeAreaTitle.GetInfo (name);
				if (info != null && (!(symbol == "UK") || info.hasUK)) {
					new BGSCENE_DEF (this.useUpscaleFM)._LoadDummyEBG (this, path, name, info, symbol);
				}
			}
			FieldMapInfo.fieldmapExtraOffset.SetOffset (name, this.overlayList);
			if (!this.useUpscaleFM) {
				this.GenerateAtlasFromBinary ();
			}
			this.CreateMaterials ();
			List<short> list = new List<short> {
				1505,
				2605,
				2653,
				2259,
				153,
				1806,
				1214,
				1823,
				1752,
				2922,
				2923,
				2924,
				2925,
				2926,
				1751,
				1752,
				1753,
				2252
			};
			this.combineMeshes = list.Contains (FF9StateSystem.Common.FF9.fldMapNo);
			if (this.combineMeshes) {
				this.CreateSceneCombined (fieldMap, this.useUpscaleFM);
				return;
			}
			this.CreateScene (fieldMap, this.useUpscaleFM);
			return;
		} else {
			this.name = name;
			if (!this.useUpscaleFM) {
				this.InitPSXTextureAtlas ();
			} else {
				Texture2D x2 = AssetManager.Load<Texture2D> (Path.Combine (path, "atlas"), false);
				if (x2 != null) {
					this.atlas = x2;
					if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor) {
						this.atlasAlpha = AssetManager.Load<Texture2D> (Path.Combine (path, "atlas_a"), false);
					} else {
						this.atlasAlpha = null;
					}
					if (BattleHUD.hD == 1) {
						this.SPRITE_W = 64u;
						this.SPRITE_H = 64u;
					} else {
						this.SPRITE_W = 32u;
						this.SPRITE_H = 32u;
					}
				} else {
					this.useUpscaleFM = false;
					this.InitPSXTextureAtlas ();
				}
			}
			if (!this.useUpscaleFM) {
				this.vram.LoadTIMs (path);
			}
			TextAsset textAsset2;
			if (!FieldMapEditor.useOriginalVersion) {
				textAsset2 = AssetManager.Load<TextAsset> (path + FieldMapEditor.GetFieldMapModName (name) + ".bgs", false);
				if (textAsset2 == null) {
					global::Debug.Log ("Cannot find MOD version.");
					textAsset2 = AssetManager.Load<TextAsset> (path + name + ".bgs", false);
				}
			} else {
				textAsset2 = AssetManager.Load<TextAsset> (path + name + ".bgs", false);
			}
			if (textAsset2 == null) {
				return;
			}
			this.ebgBin = textAsset2.bytes;
			using (BinaryReader binaryReader2 = new BinaryReader (new MemoryStream (this.ebgBin))) {
				this.ReadData (binaryReader2);
			}
			string symbol2 = Localization.GetSymbol ();
			if (symbol2 != "US") {
				FieldMapLocalizeAreaTitleInfo info2 = FieldMapInfo.localizeAreaTitle.GetInfo (name);
				if (info2 != null && (!(symbol2 == "UK") || info2.hasUK)) {
					new BGSCENE_DEF (this.useUpscaleFM)._LoadDummyEBG (this, path, name, info2, symbol2);
				}
			}
			FieldMapInfo.fieldmapExtraOffset.SetOffset (name, this.overlayList);
			if (!this.useUpscaleFM) {
				this.GenerateAtlasFromBinary ();
			}
			this.CreateMaterials ();
			List<short> list2 = new List<short> {
				1505,
				2605,
				2653,
				2259,
				153,
				1806,
				1214,
				1823,
				1752,
				2922,
				2923,
				2924,
				2925,
				2926,
				1751,
				1752,
				1753,
				2252
			};
			this.combineMeshes = list2.Contains (FF9StateSystem.Common.FF9.fldMapNo);
			if (this.combineMeshes) {
				this.CreateSceneCombined (fieldMap, this.useUpscaleFM);
				return;
			}
			this.CreateScene (fieldMap, this.useUpscaleFM);
			return;
		}
	}

	// Token: 0x06000C0E RID: 3086 RVA: 0x00102C04 File Offset: 0x00100E04
	private static Rect CalculateExpectedTextureAtlasSize (int spriteCount) {
		checked {
			foreach (Rect result in new Rect[] {
					new Rect (0f, 0f, 256f, 256f),
						new Rect (0f, 0f, 512f, 256f),
						new Rect (0f, 0f, 1024f, 256f),
						new Rect (0f, 0f, 512f, 256f),
						new Rect (0f, 0f, 512f, 512f),
						new Rect (0f, 0f, 1024f, 256f),
						new Rect (0f, 0f, 1024f, 512f),
						new Rect (0f, 0f, 2048f, 256f),
						new Rect (0f, 0f, 1024f, 1024f),
						new Rect (0f, 0f, 2048f, 512f),
						new Rect (0f, 0f, 2048f, 1024f),
						new Rect (0f, 0f, 2048f, 2048f)
				}) {
				int num = (int) result.width / 36;
				int num2 = (int) result.height / 36;
				if (num * num2 >= spriteCount) {
					return result;
				}
			}
			throw new ArgumentException ("Unexpected size of atlas texture");
		}
	}

	// Token: 0x06000C0F RID: 3087 RVA: 0x00102DDC File Offset: 0x00100FDC
	private void GenerateAtlasFromBinary () {
		checked {
			Color32[] array = new Color32[this.ATLAS_W * this.ATLAS_H];
			uint num = 0u;
			uint num2 = 1u;
			for (int i = 0; i < (int) this.overlayCount; i++) {
				BGOVERLAY_DEF bgoverlay_DEF = this.overlayList[i];
				for (int j = 0; j < (int) bgoverlay_DEF.spriteCount; j++) {
					BGSPRITE_LOC_DEF bgsprite_LOC_DEF = bgoverlay_DEF.spriteList[j];
					bgsprite_LOC_DEF.atlasX = (ushort) num;
					bgsprite_LOC_DEF.atlasY = (ushort) num2;
					if (bgsprite_LOC_DEF.res == 0) {
						int index = ArrayUtil.GetIndex ((int) (bgsprite_LOC_DEF.clutX * 16), (int) bgsprite_LOC_DEF.clutY, (int) this.vram.width, (int) this.vram.height);
						for (uint num3 = 0u; num3 < (uint) bgsprite_LOC_DEF.h; num3 += 1u) {
							int index2 = ArrayUtil.GetIndex ((int) (bgsprite_LOC_DEF.texX * 64 + bgsprite_LOC_DEF.u / 4), (int) ((uint) bgsprite_LOC_DEF.texY * 256u + (uint) bgsprite_LOC_DEF.v + num3), (int) this.vram.width, (int) this.vram.height);
							int index3 = ArrayUtil.GetIndex ((int) num, (int) (num2 + num3), (int) this.ATLAS_W, (int) this.ATLAS_H);
							uint num4 = 0u;
							while (unchecked ((ulong) num4) < (ulong) (unchecked ((long) (bgsprite_LOC_DEF.w / 2)))) {
								byte b = this.vram.rawData[index2 * 2 + (int) num4];
								byte b2 = (byte) (b & 15);
								byte b3 = (byte) (b >> 4 & 15);
								int num5 = (index + (int) b2) * 2;
								ushort num6 = (ushort) ((int) this.vram.rawData[num5] | (int) this.vram.rawData[num5 + 1] << 8);
								int num7 = index3 + (int) (num4 * 2u);
								PSX.ConvertColor16toColor32 (num6, out array[num7]);
								if (bgsprite_LOC_DEF.trans != 0 && num6 != 0) {
									if (bgsprite_LOC_DEF.alpha == 0) {
										array[num7].a = 127;
									} else if (bgsprite_LOC_DEF.alpha == 3) {
										array[num7].a = 63;
									}
								}
								num5 = (index + (int) b3) * 2;
								num6 = (ushort) ((int) this.vram.rawData[num5] | (int) this.vram.rawData[num5 + 1] << 8);
								num7 = index3 + (int) (num4 * 2u) + 1;
								PSX.ConvertColor16toColor32 (num6, out array[num7]);
								if (bgsprite_LOC_DEF.trans != 0 && num6 != 0) {
									if (bgsprite_LOC_DEF.alpha == 0) {
										array[num7].a = 127;
									} else if (bgsprite_LOC_DEF.alpha == 3) {
										array[num7].a = 63;
									}
								}
								num4 += 1u;
							}
						}
					} else if (bgsprite_LOC_DEF.res == 1) {
						int index4 = ArrayUtil.GetIndex ((int) (bgsprite_LOC_DEF.clutX * 16), (int) bgsprite_LOC_DEF.clutY, (int) this.vram.width, (int) this.vram.height);
						for (uint num8 = 0u; num8 < (uint) bgsprite_LOC_DEF.h; num8 += 1u) {
							int index5 = ArrayUtil.GetIndex ((int) (bgsprite_LOC_DEF.texX * 64 + bgsprite_LOC_DEF.u / 2), (int) ((uint) bgsprite_LOC_DEF.texY * 256u + (uint) bgsprite_LOC_DEF.v + num8), (int) this.vram.width, (int) this.vram.height);
							int index6 = ArrayUtil.GetIndex ((int) num, (int) (num2 + num8), (int) this.ATLAS_W, (int) this.ATLAS_H);
							for (uint num9 = 0u; num9 < (uint) bgsprite_LOC_DEF.w; num9 += 1u) {
								byte b4 = this.vram.rawData[index5 * 2 + (int) num9];
								int num10 = (index4 + (int) b4) * 2;
								ushort num11 = (ushort) ((int) this.vram.rawData[num10] | (int) this.vram.rawData[num10 + 1] << 8);
								int num12 = index6 + (int) num9;
								PSX.ConvertColor16toColor32 (num11, out array[num12]);
								if (bgsprite_LOC_DEF.trans != 0 && num11 != 0) {
									if (bgsprite_LOC_DEF.alpha == 0) {
										array[num12].a = 127;
									} else if (bgsprite_LOC_DEF.alpha == 3) {
										array[num12].a = 63;
									}
								}
							}
						}
					}
					for (uint num13 = 0u; num13 < (uint) bgsprite_LOC_DEF.h; num13 += 1u) {
						int index7 = ArrayUtil.GetIndex ((int) (num + this.SPRITE_W), (int) (num2 + num13), (int) this.ATLAS_W, (int) this.ATLAS_H);
						array[index7] = array[index7 - 1];
					}
					for (uint num14 = 0u; num14 < (uint) bgsprite_LOC_DEF.w; num14 += 1u) {
						int index8 = ArrayUtil.GetIndex ((int) (num + num14), (int) num2, (int) this.ATLAS_W, (int) this.ATLAS_H);
						int index9 = ArrayUtil.GetIndex ((int) (num + num14), (int) (num2 - 1u), (int) this.ATLAS_W, (int) this.ATLAS_H);
						array[index9] = array[index8];
					}
					int index10 = ArrayUtil.GetIndex ((int) (num + this.SPRITE_W - 1u), (int) num2, (int) this.ATLAS_W, (int) this.ATLAS_H);
					int index11 = ArrayUtil.GetIndex ((int) (num + this.SPRITE_W), (int) (num2 - 1u), (int) this.ATLAS_W, (int) this.ATLAS_H);
					array[index11] = array[index10];
					num += this.SPRITE_W + 1u;
					if (num >= this.ATLAS_W || this.ATLAS_W - num < this.SPRITE_W + 1u) {
						num = 0u;
						num2 += this.SPRITE_H + 1u;
					}
				}
			}
			this.atlas.SetPixels32 (array);
			this.atlas.Apply ();
		}
	}

	// Token: 0x06000C10 RID: 3088 RVA: 0x0010333C File Offset: 0x0010153C
	private void CreateMaterials () {
		Material material = new Material (Shader.Find ("PSX/FieldMap_Abr_None"));
		material.mainTexture = this.atlas;
		if (this.atlasAlpha != null) {
			material.SetTexture ("_AlphaTex", this.atlasAlpha);
		}
		this.materialList.Add ("abr_none", material);
		material = new Material (Shader.Find ("PSX/FieldMap_Abr_0"));
		material.mainTexture = this.atlas;
		if (this.atlasAlpha != null) {
			material.SetTexture ("_AlphaTex", this.atlasAlpha);
		}
		this.materialList.Add ("abr_0", material);
		material = new Material (Shader.Find ("PSX/FieldMap_Abr_1"));
		material.mainTexture = this.atlas;
		if (this.atlasAlpha != null) {
			material.SetTexture ("_AlphaTex", this.atlasAlpha);
		}
		this.materialList.Add ("abr_1", material);
		material = new Material (Shader.Find ("PSX/FieldMap_Abr_2"));
		material.mainTexture = this.atlas;
		if (this.atlasAlpha != null) {
			material.SetTexture ("_AlphaTex", this.atlasAlpha);
		}
		this.materialList.Add ("abr_2", material);
		material = new Material (Shader.Find ("PSX/FieldMap_Abr_3"));
		material.mainTexture = this.atlas;
		if (this.atlasAlpha != null) {
			material.SetTexture ("_AlphaTex", this.atlasAlpha);
		}
		this.materialList.Add ("abr_3", material);
	}

	// Token: 0x06000C11 RID: 3089 RVA: 0x001034C8 File Offset: 0x001016C8
	private void CreateScene (FieldMap fieldMap, bool UseUpscalFM) {
		bool flag = false;
		GameObject gameObject = new GameObject ("Background");
		gameObject.transform.parent = fieldMap.transform;
		if (flag) {
			gameObject.transform.localPosition = new Vector3 ((float) this.curX - 160f, -((float) this.curY - 112f), 0f);
		} else {
			gameObject.transform.localPosition = new Vector3 ((float) this.curX - 160f, -((float) this.curY - 112f), (float) this.curZ);
		}
		gameObject.transform.localScale = new Vector3 (1f, -1f, 1f);
		checked {
			for (int i = 0; i < this.cameraList.Count; i++) {
				BGCAM_DEF bgcam_DEF = this.cameraList[i];
				Transform transform = new GameObject (string.Concat (unchecked (new object[] {
					"Camera_",
					i.ToString ("D2"),
					" : ",
					(float) bgcam_DEF.vrpMaxX + 160f,
					" x ",
					(float) bgcam_DEF.vrpMaxY + 112f
				}))).transform;
				transform.parent = gameObject.transform;
				bgcam_DEF.transform = transform;
				bgcam_DEF.transform.localPosition = Vector3.zero;
				bgcam_DEF.transform.localScale = new Vector3 (1f, 1f, 1f);
			}
			List<Vector3> list = new List<Vector3> ();
			List<Vector2> list2 = new List<Vector2> ();
			List<int> list3 = new List<int> ();
			for (int j = 0; j < this.overlayList.Count; j++) {
				BGOVERLAY_DEF bgoverlay_DEF = this.overlayList[j];
				string str = "Overlay_" + j.ToString ("D2");
				Transform transform2 = new GameObject (str).transform;
				transform2.parent = this.cameraList[(int) bgoverlay_DEF.camNdx].transform;
				unchecked {
					if (flag) {
						transform2.localPosition = new Vector3 ((float) bgoverlay_DEF.curX * 1f, (float) bgoverlay_DEF.curY * 1f, 0f);
					} else {
						transform2.localPosition = new Vector3 ((float) bgoverlay_DEF.curX * 1f, (float) bgoverlay_DEF.curY * 1f, (float) bgoverlay_DEF.curZ);
					}
					transform2.localScale = new Vector3 (1f, 1f, 1f);
					bgoverlay_DEF.transform = transform2;
				}
				for (int k = 0; k < bgoverlay_DEF.spriteList.Count; k++) {
					BGSPRITE_LOC_DEF bgsprite_LOC_DEF = bgoverlay_DEF.spriteList[k];
					int num = 0;
					if (!flag) {
						num = bgsprite_LOC_DEF.depth;
					}
					GameObject gameObject2 = new GameObject (str + "_Sprite_" + k.ToString ("D3"));
					Transform transform3 = gameObject2.transform;
					transform3.parent = transform2;
					MeshRenderer meshRenderer;
					unchecked {
						if (flag) {
							transform3.localPosition = new Vector3 ((float) (checked (bgoverlay_DEF.scrX + (short) bgsprite_LOC_DEF.offX)) * 1f, (float) (checked (bgoverlay_DEF.scrY + (short) bgsprite_LOC_DEF.offY + 16)) * 1f, 0f);
						} else {
							transform3.localPosition = new Vector3 ((float) bgsprite_LOC_DEF.offX * 1f, (float) (checked (bgsprite_LOC_DEF.offY + 16)) * 1f, (float) num);
						}
						transform3.localScale = new Vector3 (1f, 1f, 1f);
						bgsprite_LOC_DEF.transform = transform3;
						bgsprite_LOC_DEF.cacheLocalPos = transform3.localPosition;
						list.Clear ();
						list2.Clear ();
						list3.Clear ();
						list.Add (new Vector3 (0f, -16f, 0f));
						list.Add (new Vector3 (16f, -16f, 0f));
						list.Add (new Vector3 (16f, 0f, 0f));
						list.Add (new Vector3 (0f, 0f, 0f));
						float num2 = this.ATLAS_W;
						float num3 = this.ATLAS_H;
						float x;
						float y;
						float x2;
						float y2;
						if (UseUpscalFM) {
							float num4 = 0.5f;
							x = ((float) bgsprite_LOC_DEF.atlasX - num4) / num2;
							y = (checked (this.ATLAS_H - (uint) bgsprite_LOC_DEF.atlasY) + num4) / num3;
							x2 = (checked ((uint) bgsprite_LOC_DEF.atlasX + this.SPRITE_W) - num4) / num2;
							y2 = (checked (this.ATLAS_H - ((uint) bgsprite_LOC_DEF.atlasY + this.SPRITE_H)) + num4) / num3;
						} else {
							float num5 = 0.5f;
							x = ((float) bgsprite_LOC_DEF.atlasX + num5) / num2;
							y = ((float) bgsprite_LOC_DEF.atlasY + num5) / num3;
							x2 = (checked ((uint) bgsprite_LOC_DEF.atlasX + this.SPRITE_W) - num5) / num2;
							y2 = (checked ((uint) bgsprite_LOC_DEF.atlasY + this.SPRITE_H) - num5) / num3;
						}
						list2.Add (new Vector2 (x, y));
						list2.Add (new Vector2 (x2, y));
						list2.Add (new Vector2 (x2, y2));
						list2.Add (new Vector2 (x, y2));
						list3.Add (2);
						list3.Add (1);
						list3.Add (0);
						list3.Add (3);
						list3.Add (2);
						list3.Add (0);
						Mesh mesh = new Mesh ();
						mesh.vertices = list.ToArray ();
						mesh.uv = list2.ToArray ();
						mesh.triangles = list3.ToArray ();
						meshRenderer = gameObject2.AddComponent<MeshRenderer> ();
						gameObject2.AddComponent<MeshFilter> ().mesh = mesh;
						GameObject gameObject3 = gameObject2;
						string text = gameObject3.name;
						gameObject3.name = string.Concat (new object[] {
							text,
							"_Atlas[",
							bgsprite_LOC_DEF.atlasX,
							", ",
							bgsprite_LOC_DEF.atlasY,
							"]"
						});
					}
					int num6 = (int) (this.curZ + (short) bgoverlay_DEF.curZ) + bgsprite_LOC_DEF.depth;
					GameObject gameObject4 = gameObject2;
					gameObject4.name = gameObject4.name + "_Depth(" + num6.ToString ("D5") + ")";
					string text2 = string.Empty;
					if (bgsprite_LOC_DEF.trans != 0) {
						if (bgsprite_LOC_DEF.alpha == 0) {
							text2 = "abr_0";
						} else if (bgsprite_LOC_DEF.alpha == 1) {
							text2 = "abr_1";
						} else if (bgsprite_LOC_DEF.alpha == 2) {
							text2 = "abr_2";
						} else {
							text2 = "abr_3";
						}
					} else {
						text2 = "abr_none";
					}
					if (fieldMap.mapName == "FBG_N39_UUVL_MAP671_UV_DEP_0" && j == 14) {
						text2 = "abr_none";
					}
					GameObject gameObject5 = gameObject2;
					gameObject5.name = gameObject5.name + "_[" + text2 + "]";
					meshRenderer.material = this.materialList[text2];
				}
				if ((bgoverlay_DEF.flags & 2) != 0) {
					bgoverlay_DEF.transform.gameObject.SetActive (true);
				} else {
					bgoverlay_DEF.transform.gameObject.SetActive (false);
				}
			}
			for (int l = 0; l < this.animList.Count; l++) {
				BGANIM_DEF bganim_DEF = this.animList[l];
				for (int m = 0; m < bganim_DEF.frameList.Count; m++) {
					GameObject gameObject6 = this.overlayList[(int) bganim_DEF.frameList[m].target].transform.gameObject;
					gameObject6.name = gameObject6.name + "_[anim_" + l.ToString ("D2") + "]";
					string text3 = gameObject6.name;
					gameObject6.name = string.Concat (new string[] {
						text3,
						"_[frame_",
						m.ToString ("D2"),
						"_of_",
						bganim_DEF.frameList.Count.ToString ("D2"),
						"]"
					});
				}
			}
		}
	}

	// Token: 0x06000C12 RID: 3090 RVA: 0x00103CF0 File Offset: 0x00101EF0
	public void CreateSeparateOverlay (FieldMap fieldMap, bool UseUpscalFM, uint ovrNdx) {
		checked {
			BGOVERLAY_DEF bgoverlay_DEF = this.overlayList[(int) ovrNdx];
			if (bgoverlay_DEF.isCreated && !bgoverlay_DEF.canCombine) {
				return;
			}
			bgoverlay_DEF.canCombine = false;
			bool flag = false;
			List<Vector3> list = new List<Vector3> ();
			List<Vector2> list2 = new List<Vector2> ();
			List<int> list3 = new List<int> ();
			MeshFilter component = bgoverlay_DEF.transform.GetComponent<MeshFilter> ();
			if (component != null) {
				UnityEngine.Object.Destroy (component);
			}
			MeshRenderer component2 = bgoverlay_DEF.transform.GetComponent<MeshRenderer> ();
			if (component2 != null) {
				UnityEngine.Object.Destroy (component2);
			}
			for (int i = 0; i < bgoverlay_DEF.spriteList.Count; i++) {
				BGSPRITE_LOC_DEF bgsprite_LOC_DEF = bgoverlay_DEF.spriteList[i];
				int num = 0;
				if (!flag) {
					num = bgsprite_LOC_DEF.depth;
				}
				GameObject gameObject = new GameObject (bgoverlay_DEF.transform.name + "_Sprite_" + i.ToString ("D3"));
				Transform transform = gameObject.transform;
				transform.parent = bgoverlay_DEF.transform;
				MeshRenderer meshRenderer;
				unchecked {
					if (flag) {
						transform.localPosition = new Vector3 ((float) (checked (bgoverlay_DEF.scrX + (short) bgsprite_LOC_DEF.offX)) * 1f, (float) (checked (bgoverlay_DEF.scrY + (short) bgsprite_LOC_DEF.offY + 16)) * 1f, 0f);
					} else {
						transform.localPosition = new Vector3 ((float) bgsprite_LOC_DEF.offX * 1f, (float) (checked (bgsprite_LOC_DEF.offY + 16)) * 1f, (float) num);
					}
					transform.localScale = new Vector3 (1f, 1f, 1f);
					bgsprite_LOC_DEF.transform = transform;
					list.Clear ();
					list2.Clear ();
					list3.Clear ();
					list.Add (new Vector3 (0f, -16f, 0f));
					list.Add (new Vector3 (16f, -16f, 0f));
					list.Add (new Vector3 (16f, 0f, 0f));
					list.Add (new Vector3 (0f, 0f, 0f));
					float num2 = this.ATLAS_W;
					float num3 = this.ATLAS_H;
					float x;
					float y;
					float x2;
					float y2;
					if (UseUpscalFM) {
						float num4 = 0.5f;
						x = ((float) bgsprite_LOC_DEF.atlasX - num4) / num2;
						y = (checked (this.ATLAS_H - (uint) bgsprite_LOC_DEF.atlasY) + num4) / num3;
						x2 = (checked ((uint) bgsprite_LOC_DEF.atlasX + this.SPRITE_W) - num4) / num2;
						y2 = (checked (this.ATLAS_H - ((uint) bgsprite_LOC_DEF.atlasY + this.SPRITE_H)) + num4) / num3;
					} else {
						float num5 = 0.5f;
						x = ((float) bgsprite_LOC_DEF.atlasX + num5) / num2;
						y = ((float) bgsprite_LOC_DEF.atlasY + num5) / num3;
						x2 = (checked ((uint) bgsprite_LOC_DEF.atlasX + this.SPRITE_W) - num5) / num2;
						y2 = (checked ((uint) bgsprite_LOC_DEF.atlasY + this.SPRITE_H) - num5) / num3;
					}
					list2.Add (new Vector2 (x, y));
					list2.Add (new Vector2 (x2, y));
					list2.Add (new Vector2 (x2, y2));
					list2.Add (new Vector2 (x, y2));
					list3.Add (2);
					list3.Add (1);
					list3.Add (0);
					list3.Add (3);
					list3.Add (2);
					list3.Add (0);
					Mesh mesh = new Mesh ();
					mesh.vertices = list.ToArray ();
					mesh.uv = list2.ToArray ();
					mesh.triangles = list3.ToArray ();
					meshRenderer = gameObject.AddComponent<MeshRenderer> ();
					gameObject.AddComponent<MeshFilter> ().mesh = mesh;
				}
				int num6 = (int) (this.curZ + (short) bgoverlay_DEF.curZ) + bgsprite_LOC_DEF.depth;
				GameObject gameObject2 = gameObject;
				gameObject2.name = gameObject2.name + "_Depth(" + num6.ToString ("D5") + ")";
				string text = string.Empty;
				if (bgsprite_LOC_DEF.trans != 0) {
					if (bgsprite_LOC_DEF.alpha == 0) {
						text = "abr_0";
					} else if (bgsprite_LOC_DEF.alpha == 1) {
						text = "abr_1";
					} else if (bgsprite_LOC_DEF.alpha == 2) {
						text = "abr_2";
					} else {
						text = "abr_3";
					}
				} else {
					text = "abr_none";
				}
				if (fieldMap.mapName == "FBG_N39_UUVL_MAP671_UV_DEP_0" && ovrNdx == 14u) {
					text = "abr_none";
				}
				GameObject gameObject3 = gameObject;
				gameObject3.name = gameObject3.name + "_[" + text + "]";
				meshRenderer.material = this.materialList[text];
			}
		}
	}

	// Token: 0x06000C13 RID: 3091 RVA: 0x00104174 File Offset: 0x00102374
	public void CreateSeparateSprites (FieldMap fieldMap, bool UseUpscalFM, uint ovrNdx, List<int> spriteIdx) {
		checked {
			BGOVERLAY_DEF bgoverlay_DEF = this.overlayList[(int) ovrNdx];
			bool flag = false;
			List<Vector3> list = new List<Vector3> ();
			List<Vector2> list2 = new List<Vector2> ();
			List<int> list3 = new List<int> ();
			int num = (int) bgoverlay_DEF.transform.localPosition.z;
			for (int i = 0; i < spriteIdx.Count; i++) {
				BGSPRITE_LOC_DEF bgsprite_LOC_DEF = bgoverlay_DEF.spriteList[spriteIdx[i]];
				int num2 = 0;
				if (!flag) {
					num2 = bgsprite_LOC_DEF.depth + num;
				}
				GameObject gameObject = new GameObject (bgoverlay_DEF.transform.name + "_Sprite_" + i.ToString ("D3"));
				Transform transform = gameObject.transform;
				transform.parent = bgoverlay_DEF.transform;
				MeshRenderer meshRenderer;
				unchecked {
					if (flag) {
						transform.localPosition = new Vector3 ((float) (checked (bgoverlay_DEF.scrX + (short) bgsprite_LOC_DEF.offX)) * 1f, (float) (checked (bgoverlay_DEF.scrY + (short) bgsprite_LOC_DEF.offY + 16)) * 1f, 0f);
					} else {
						transform.localPosition = new Vector3 ((float) bgsprite_LOC_DEF.offX * 1f, (float) (checked (bgsprite_LOC_DEF.offY + 16)) * 1f, (float) num2);
					}
					transform.localScale = new Vector3 (1f, 1f, 1f);
					bgsprite_LOC_DEF.transform = transform;
					list.Clear ();
					list2.Clear ();
					list3.Clear ();
					list.Add (new Vector3 (0f, -16f, 0f));
					list.Add (new Vector3 (16f, -16f, 0f));
					list.Add (new Vector3 (16f, 0f, 0f));
					list.Add (new Vector3 (0f, 0f, 0f));
					float num3 = this.ATLAS_W;
					float num4 = this.ATLAS_H;
					float x;
					float y;
					float x2;
					float y2;
					if (UseUpscalFM) {
						float num5 = 0.5f;
						x = ((float) bgsprite_LOC_DEF.atlasX - num5) / num3;
						y = (checked (this.ATLAS_H - (uint) bgsprite_LOC_DEF.atlasY) + num5) / num4;
						x2 = (checked ((uint) bgsprite_LOC_DEF.atlasX + this.SPRITE_W) - num5) / num3;
						y2 = (checked (this.ATLAS_H - ((uint) bgsprite_LOC_DEF.atlasY + this.SPRITE_H)) + num5) / num4;
					} else {
						float num6 = 0.5f;
						x = ((float) bgsprite_LOC_DEF.atlasX + num6) / num3;
						y = ((float) bgsprite_LOC_DEF.atlasY + num6) / num4;
						x2 = (checked ((uint) bgsprite_LOC_DEF.atlasX + this.SPRITE_W) - num6) / num3;
						y2 = (checked ((uint) bgsprite_LOC_DEF.atlasY + this.SPRITE_H) - num6) / num4;
					}
					list2.Add (new Vector2 (x, y));
					list2.Add (new Vector2 (x2, y));
					list2.Add (new Vector2 (x2, y2));
					list2.Add (new Vector2 (x, y2));
					list3.Add (2);
					list3.Add (1);
					list3.Add (0);
					list3.Add (3);
					list3.Add (2);
					list3.Add (0);
					Mesh mesh = new Mesh ();
					mesh.vertices = list.ToArray ();
					mesh.uv = list2.ToArray ();
					mesh.triangles = list3.ToArray ();
					meshRenderer = gameObject.AddComponent<MeshRenderer> ();
					gameObject.AddComponent<MeshFilter> ().mesh = mesh;
				}
				int num7 = (int) (this.curZ + (short) bgoverlay_DEF.curZ) + bgsprite_LOC_DEF.depth;
				GameObject gameObject2 = gameObject;
				gameObject2.name = gameObject2.name + "_Depth(" + num7.ToString ("D5") + ")";
				string text = string.Empty;
				if (bgsprite_LOC_DEF.trans != 0) {
					if (bgsprite_LOC_DEF.alpha == 0) {
						text = "abr_0";
					} else if (bgsprite_LOC_DEF.alpha == 1) {
						text = "abr_1";
					} else if (bgsprite_LOC_DEF.alpha == 2) {
						text = "abr_2";
					} else {
						text = "abr_3";
					}
				} else {
					text = "abr_none";
				}
				if (fieldMap.mapName == "FBG_N39_UUVL_MAP671_UV_DEP_0" && ovrNdx == 14u) {
					text = "abr_none";
				}
				GameObject gameObject3 = gameObject;
				gameObject3.name = gameObject3.name + "_[" + text + "]";
				meshRenderer.material = this.materialList[text];
			}
		}
	}

	// Token: 0x06000C14 RID: 3092 RVA: 0x001045BC File Offset: 0x001027BC
	private void CreateSceneCombined (FieldMap fieldMap, bool UseUpscalFM) {
		bool flag = false;
		GameObject gameObject = new GameObject ("Background");
		gameObject.transform.parent = fieldMap.transform;
		if (flag) {
			gameObject.transform.localPosition = new Vector3 ((float) this.curX - 160f, -((float) this.curY - 112f), 0f);
		} else {
			gameObject.transform.localPosition = new Vector3 ((float) this.curX - 160f, -((float) this.curY - 112f), (float) this.curZ);
		}
		gameObject.transform.localScale = new Vector3 (1f, -1f, 1f);
		checked {
			for (int i = 0; i < this.cameraList.Count; i++) {
				BGCAM_DEF bgcam_DEF = this.cameraList[i];
				Transform transform = new GameObject (string.Concat (unchecked (new object[] {
					"Camera_",
					i.ToString ("D2"),
					" : ",
					(float) bgcam_DEF.vrpMaxX + 160f,
					" x ",
					(float) bgcam_DEF.vrpMaxY + 112f
				}))).transform;
				transform.parent = gameObject.transform;
				bgcam_DEF.transform = transform;
				bgcam_DEF.transform.localPosition = Vector3.zero;
				bgcam_DEF.transform.localScale = new Vector3 (1f, 1f, 1f);
			}
			FieldMap.EbgCombineMeshData currentCombineMeshData = fieldMap.GetCurrentCombineMeshData ();
			List<int> list = null;
			if (currentCombineMeshData != null) {
				list = currentCombineMeshData.skipOverlayList;
			}
			List<Vector3> list2 = new List<Vector3> ();
			List<Vector2> list3 = new List<Vector2> ();
			List<int> list4 = new List<int> ();
			for (int j = 0; j < this.overlayList.Count; j++) {
				BGOVERLAY_DEF bgoverlay_DEF = this.overlayList[j];
				GameObject gameObject2 = new GameObject ("Overlay_" + j.ToString ("D2"));
				Transform transform2 = gameObject2.transform;
				transform2.parent = this.cameraList[(int) bgoverlay_DEF.camNdx].transform;
				unchecked {
					if (flag) {
						transform2.localPosition = new Vector3 ((float) bgoverlay_DEF.curX * 1f, (float) bgoverlay_DEF.curY * 1f, 0f);
					} else {
						transform2.localPosition = new Vector3 ((float) bgoverlay_DEF.curX * 1f, (float) bgoverlay_DEF.curY * 1f, (float) bgoverlay_DEF.curZ);
					}
					transform2.localScale = new Vector3 (1f, 1f, 1f);
					bgoverlay_DEF.transform = transform2;
					list2.Clear ();
					list3.Clear ();
					list4.Clear ();
					bgoverlay_DEF.canCombine = true;
					bgoverlay_DEF.isCreated = false;
				}
				if ((bgoverlay_DEF.flags & 4) != 0) {
					bgoverlay_DEF.canCombine = false;
				} else if ((bgoverlay_DEF.flags & 128) != 0) {
					bgoverlay_DEF.canCombine = false;
				} else if (bgoverlay_DEF.spriteList.Count > 1) {
					bool flag2 = false;
					if (list != null && list.Contains (j)) {
						flag2 = true;
					}
					if (!flag2) {
						int num = (!fieldMap.IsCurrentFieldMapHasCombineMeshProblem ()) ? 512 : 164;
						BGSPRITE_LOC_DEF bgsprite_LOC_DEF = bgoverlay_DEF.spriteList[0];
						int num2 = 4096;
						int num3 = -4096;
						for (int k = 0; k < bgoverlay_DEF.spriteList.Count; k++) {
							num2 = Mathf.Min (num2, bgoverlay_DEF.spriteList[k].depth);
							num3 = Mathf.Max (num3, bgoverlay_DEF.spriteList[k].depth);
							if (num3 - num2 > num) {
								bgoverlay_DEF.canCombine = false;
								break;
							}
						}
					} else {
						bgoverlay_DEF.canCombine = false;
					}
					if (FF9StateSystem.Common.FF9.fldMapNo == 552) {
						if (j == 17) {
							bgoverlay_DEF.canCombine = true;
						} else {
							bgoverlay_DEF.canCombine = true;
						}
					}
				}
				if (!bgoverlay_DEF.canCombine) {
					this.CreateSeparateOverlay (fieldMap, UseUpscalFM, (uint) j);
					if ((bgoverlay_DEF.flags & 2) != 0) {
						bgoverlay_DEF.transform.gameObject.SetActive (true);
					} else {
						bgoverlay_DEF.transform.gameObject.SetActive (false);
					}
					bgoverlay_DEF.isCreated = true;
				} else {
					List<int> list5 = null;
					if (FF9StateSystem.Common.FF9.fldMapNo == 552 && j == 17) {
						list5 = new List<int> {
							202,
							203,
							214,
							215
						};
					}
					for (int l = 0; l < bgoverlay_DEF.spriteList.Count; l++) {
						if (list5 == null || !list5.Contains (l)) {
							BGSPRITE_LOC_DEF bgsprite_LOC_DEF2 = bgoverlay_DEF.spriteList[l];
							int num4 = 0;
							if (!flag) {
								num4 = bgsprite_LOC_DEF2.depth;
							}
							Vector3 zero = Vector3.zero;
							int count;
							unchecked {
								if (flag) {
									zero = new Vector3 ((float) (checked (bgoverlay_DEF.scrX + (short) bgsprite_LOC_DEF2.offX)) * 1f, (float) (checked (bgoverlay_DEF.scrY + (short) bgsprite_LOC_DEF2.offY + 16)) * 1f, 0f);
								} else {
									zero = new Vector3 ((float) bgsprite_LOC_DEF2.offX * 1f, (float) (checked (bgsprite_LOC_DEF2.offY + 16)) * 1f, (float) num4);
								}
								count = list2.Count;
								list2.Add (new Vector3 (0f, -16f, 0f) + zero);
								list2.Add (new Vector3 (16f, -16f, 0f) + zero);
								list2.Add (new Vector3 (16f, 0f, 0f) + zero);
								list2.Add (new Vector3 (0f, 0f, 0f) + zero);
								float num5 = this.ATLAS_W;
								float num6 = this.ATLAS_H;
								float x;
								float y;
								float x2;
								float y2;
								if (UseUpscalFM) {
									float num7 = 0.5f;
									x = ((float) bgsprite_LOC_DEF2.atlasX - num7) / num5;
									y = (checked (this.ATLAS_H - (uint) bgsprite_LOC_DEF2.atlasY) + num7) / num6;
									x2 = (checked ((uint) bgsprite_LOC_DEF2.atlasX + this.SPRITE_W) - num7) / num5;
									y2 = (checked (this.ATLAS_H - ((uint) bgsprite_LOC_DEF2.atlasY + this.SPRITE_H)) + num7) / num6;
								} else {
									float num8 = 0.5f;
									x = ((float) bgsprite_LOC_DEF2.atlasX + num8) / num5;
									y = ((float) bgsprite_LOC_DEF2.atlasY + num8) / num6;
									x2 = (checked ((uint) bgsprite_LOC_DEF2.atlasX + this.SPRITE_W) - num8) / num5;
									y2 = (checked ((uint) bgsprite_LOC_DEF2.atlasY + this.SPRITE_H) - num8) / num6;
								}
								list3.Add (new Vector2 (x, y));
								list3.Add (new Vector2 (x2, y));
								list3.Add (new Vector2 (x2, y2));
								list3.Add (new Vector2 (x, y2));
							}
							list4.Add (count + 2);
							list4.Add (count + 1);
							list4.Add (count);
							list4.Add (count + 3);
							list4.Add (count + 2);
							list4.Add (count);
						}
					}
					if (bgoverlay_DEF.spriteList.Count > 0) {
						Mesh mesh = new Mesh ();
						mesh.vertices = list2.ToArray ();
						mesh.uv = list3.ToArray ();
						mesh.triangles = list4.ToArray ();
						MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer> ();
						gameObject2.AddComponent<MeshFilter> ().mesh = mesh;
						string text = string.Empty;
						BGSPRITE_LOC_DEF bgsprite_LOC_DEF3 = bgoverlay_DEF.spriteList[0];
						if (bgsprite_LOC_DEF3.trans != 0) {
							if (bgsprite_LOC_DEF3.alpha == 0) {
								text = "abr_0";
							} else if (bgsprite_LOC_DEF3.alpha == 1) {
								text = "abr_1";
							} else if (bgsprite_LOC_DEF3.alpha == 2) {
								text = "abr_2";
							} else {
								text = "abr_3";
							}
						} else {
							text = "abr_none";
						}
						if (fieldMap.mapName == "FBG_N39_UUVL_MAP671_UV_DEP_0" && j == 14) {
							text = "abr_none";
						}
						GameObject gameObject3 = gameObject2;
						gameObject3.name = gameObject3.name + "_[" + text + "]";
						meshRenderer.material = this.materialList[text];
					}
					if ((bgoverlay_DEF.flags & 2) != 0) {
						bgoverlay_DEF.transform.gameObject.SetActive (true);
					} else {
						bgoverlay_DEF.transform.gameObject.SetActive (false);
					}
					if (list5 != null) {
						this.CreateSeparateSprites (fieldMap, this.useUpscaleFM, (uint) j, list5);
					}
					bgoverlay_DEF.isCreated = true;
				}
			}
			for (int m = 0; m < this.animList.Count; m++) {
				BGANIM_DEF bganim_DEF = this.animList[m];
				for (int n = 0; n < bganim_DEF.frameList.Count; n++) {
					GameObject gameObject4 = this.overlayList[(int) bganim_DEF.frameList[n].target].transform.gameObject;
					gameObject4.name = gameObject4.name + "_[anim_" + m.ToString ("D2") + "]";
					string text2 = gameObject4.name;
					gameObject4.name = string.Concat (new string[] {
						text2,
						"_[frame_",
						n.ToString ("D2"),
						"_of_",
						bganim_DEF.frameList.Count.ToString ("D2"),
						"]"
					});
				}
			}
		}
	}

	// Token: 0x04001E95 RID: 7829
	public ushort sceneLength;

	// Token: 0x04001E96 RID: 7830
	public ushort depthBitShift;

	// Token: 0x04001E97 RID: 7831
	public ushort animCount;

	// Token: 0x04001E98 RID: 7832
	public ushort overlayCount;

	// Token: 0x04001E99 RID: 7833
	public ushort lightCount;

	// Token: 0x04001E9A RID: 7834
	public ushort cameraCount;

	// Token: 0x04001E9B RID: 7835
	public uint animOffset;

	// Token: 0x04001E9C RID: 7836
	public uint overlayOffset;

	// Token: 0x04001E9D RID: 7837
	public uint lightOffset;

	// Token: 0x04001E9E RID: 7838
	public uint cameraOffset;

	// Token: 0x04001E9F RID: 7839
	public short orgZ;

	// Token: 0x04001EA0 RID: 7840
	public short curZ;

	// Token: 0x04001EA1 RID: 7841
	public short orgX;

	// Token: 0x04001EA2 RID: 7842
	public short orgY;

	// Token: 0x04001EA3 RID: 7843
	public short curX;

	// Token: 0x04001EA4 RID: 7844
	public short curY;

	// Token: 0x04001EA5 RID: 7845
	public short minX;

	// Token: 0x04001EA6 RID: 7846
	public short maxX;

	// Token: 0x04001EA7 RID: 7847
	public short minY;

	// Token: 0x04001EA8 RID: 7848
	public short maxY;

	// Token: 0x04001EA9 RID: 7849
	public short scrX;

	// Token: 0x04001EAA RID: 7850
	public short scrY;

	// Token: 0x04001EAB RID: 7851
	public new string name;

	// Token: 0x04001EAC RID: 7852
	public byte[] ebgBin;

	// Token: 0x04001EAD RID: 7853
	public List<BGOVERLAY_DEF> overlayList;

	// Token: 0x04001EAE RID: 7854
	public List<BGANIM_DEF> animList;

	// Token: 0x04001EAF RID: 7855
	public List<BGLIGHT_DEF> lightList;

	// Token: 0x04001EB0 RID: 7856
	public List<BGCAM_DEF> cameraList;

	// Token: 0x04001EB1 RID: 7857
	public Dictionary<string, Material> materialList;

	// Token: 0x04001EB2 RID: 7858
	public PSXVram vram;

	// Token: 0x04001EB3 RID: 7859
	public Texture2D atlas;

	// Token: 0x04001EB4 RID: 7860
	public Texture2D atlasAlpha;

	// Token: 0x04001EB5 RID: 7861
	public uint SPRITE_W = 16u;

	// Token: 0x04001EB6 RID: 7862
	public uint SPRITE_H = 16u;

	// Token: 0x04001EB7 RID: 7863
	public uint ATLAS_W = 1024u;

	// Token: 0x04001EB8 RID: 7864
	public uint ATLAS_H = 1024u;

	// Token: 0x04001EB9 RID: 7865
	public bool combineMeshes = true;

	// Token: 0x04001EBA RID: 7866
	private int spriteCount;

	// Token: 0x04001EBB RID: 7867
	private bool useUpscaleFM;
}