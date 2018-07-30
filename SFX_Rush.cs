using System;
using Assets.Scripts.Common;
using UnityEngine;

// Token: 0x02000356 RID: 854
public class SFX_Rush
{
	// Token: 0x060014D2 RID: 5330 RVA: 0x00150294 File Offset: 0x0014E494
	public SFX_Rush()
	{
		this.rush_type = FF9StateSystem.Battle.isRandomEncounter;
		this.rot = 0f;
		this.rotInc = 0.03926991f;
		this.subCol = 0f;
		this.scale = 1f;
		if (!this.rush_type)
		{
			this.addCol = 0.1f;
			this.addColDec = 0.002f;
			this.subColDec = 0.0008f;
			this.scaleAdd = -0.008f;
		}
		else
		{
			this.addCol = 0.1f;
			this.addColDec = 0.002f;
			this.subColDec = 0.0008f;
			this.scaleAdd = 0.006f;
		}
		if ((double)UnityEngine.Random.value > 0.5)
		{
			this.rotInc *= -1f;
		}
		Rect screenSize = SFX_Rush.GetScreenSize();
		this.texture = new RenderTexture[2];
		GL.PushMatrix();
		GL.LoadIdentity();
		Matrix4x4 mat = Matrix4x4.Scale(new Vector3(1f, 2f, 1f));
		GL.MultMatrix(mat);
		GL.Viewport(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height));
		for (int i = 0; i < 2; i++)
		{
			this.texture[i] = new RenderTexture((int)screenSize.width, (int)screenSize.height, 0, RenderTextureFormat.ARGB32);
			this.texture[i].enableRandomWrite = false;
			this.texture[i].wrapMode = TextureWrapMode.Clamp;
			this.texture[i].filterMode = FilterMode.Point;
			this.texture[i].Create();
			this.texture[i].name = "Rush_RT_" + i;
			Graphics.Blit(SFX_Rush.result, this.texture[i]);
		}
		GL.PopMatrix();
		SceneDirector.FF9Wipe_FadeOutEx(5);
	}

	// Token: 0x060014D4 RID: 5332 RVA: 0x00150478 File Offset: 0x0014E678
	public void ReleaseRenderTarget()
	{
		for (int i = 0; i < 2; i++)
		{
			if (this.texture[i] != null)
			{
				this.texture[i].Release();
				this.texture[i] = null;
			}
		}
	}

	// Token: 0x060014D5 RID: 5333 RVA: 0x001504C0 File Offset: 0x0014E6C0
	public bool update() // TehMight
	{
		int num = this.rush_seq;
		checked
		{
			this.rush_seq = num + 1;
			if (num > BattleHUD.battleSwirl + 5)
			{
				return true;
			}
			if (this.rush_seq > BattleHUD.battleSwirl)
			{
				SceneDirector.ServiceFade();
			}
			this.isUpdate = true;
			return false;
		}
	}

	// Token: 0x060014D6 RID: 5334 RVA: 0x00150504 File Offset: 0x0014E704
	public void PostProcess(RenderTexture src, RenderTexture dest)
	{
		int num = 0;
		int num2 = 1;
		if (this.isUpdate)
		{
			this.isUpdate = false;
			Material material = new Material(Shader.Find("SFX_RUSH_SUB"));
			material.SetVector("_Param", new Vector4(this.rot, this.scale, this.subCol, 0f));
			Graphics.Blit(this.texture[num], this.texture[num2], material);
			Material material2 = new Material(Shader.Find("SFX_RUSH_ADD"));
			material2.SetVector("_Center", new Vector4(SFX_Rush.px, SFX_Rush.py, 0f, 0f));
			material2.SetVector("_Param", new Vector4(this.rot, this.scale, this.addCol, 0f));
			Graphics.Blit(this.texture[num], this.texture[num2], material2);
			Graphics.Blit(this.texture[num2], this.texture[num]);
			if ((this.rush_seq & 1) != 0)
			{
				SFX_Rush.px += ((SFX_Rush.px >= 0.5f) ? -0.009375f : 0.009375f);
			}
			if ((this.rush_seq & 2) != 0)
			{
				SFX_Rush.py += ((SFX_Rush.py >= 0.5f) ? -0.0133928573f : 0.0133928573f);
			}
			if (!this.rush_type)
			{
				float num3 = (float)Math.Sin((double)this.rush_seq * 14.0 * 3.1415926535897931 / 180.0);
				SFX_Rush.px += num3 * (0.5f - SFX_Rush.px);
				SFX_Rush.py += num3 * (0.5f - SFX_Rush.py);
			}
			if (!this.rush_type && this.rush_seq >= 16)
			{
				this.addCol -= this.addColDec;
				if (this.addCol < 0f)
				{
					this.addCol = 0f;
				}
			}
			if (this.rush_seq >= 1)
			{
				this.subCol += this.subColDec;
				if (this.subCol > 1f)
				{
					this.subCol = 1f;
				}
			}
			this.rot += this.rotInc;
			this.scale += this.scaleAdd;
		}
		Graphics.Blit(this.texture[num], dest);
	}

	// Token: 0x060014D7 RID: 5335 RVA: 0x00150784 File Offset: 0x0014E984
	public static Rect GetScreenSize()
	{
		Vector2 vector = new Vector2(320f, 224f);
		if (PersistenSingleton<SceneDirector>.Instance.CurrentScene == "BattleMap" || PersistenSingleton<SceneDirector>.Instance.CurrentScene == "BattleMapDebug")
		{
			vector = new Vector2(320f, 220f);
		}
		float num = Mathf.Min((float)Screen.width / vector.x, (float)Screen.height / vector.y);
		Rect rect = default(Rect);
		rect.width = vector.x * num;
		rect.height = vector.y * num;
		rect.x = ((float)Screen.width - rect.width) * 0.5f;
		rect.y = ((float)Screen.height - rect.height) * 0.5f;
		return rect;
	}

	// Token: 0x060014D8 RID: 5336 RVA: 0x00150868 File Offset: 0x0014EA68
	public static void CreateScreen()
	{
		Rect screenSize = SFX_Rush.GetScreenSize();
		int num = (int)screenSize.x;
		int num2 = (int)screenSize.y;
		int num3 = (int)screenSize.width;
		int num4 = (int)screenSize.height;
		SFX_Rush.result = new Texture2D((int)screenSize.width, (int)screenSize.height, TextureFormat.ARGB32, false);
		Color color = new Color(0f, 0f, 0f, 0f);
		for (int i = 0; i < num3; i++)
		{
			SFX_Rush.result.SetPixel(i, 0, color);
			SFX_Rush.result.SetPixel(i, num4 - 1, color);
		}
		for (int j = 1; j < num4 - 1; j++)
		{
			SFX_Rush.result.SetPixel(0, j, color);
			SFX_Rush.result.SetPixel(num3 - 1, j, color);
		}
		SFX_Rush.result.ReadPixels(new Rect((float)(num + 1), (float)(num2 + 1), (float)(num3 - 2), (float)(num4 - 2)), 1, 1, false);
		SFX_Rush.result.Apply();
	}

	// Token: 0x060014D9 RID: 5337 RVA: 0x00150974 File Offset: 0x0014EB74
	public static void SetCenterPosition(int type)
	{
		if (type != 0)
		{
			if (type != 1)
			{
				SFX_Rush.px = 0.5f;
				SFX_Rush.py = 0.5f;
			}
			else if (ff9.w_moveActorPtr != null)
			{
				Vector3 pos = ff9.w_moveActorPtr.pos;
				Camera w_frameCameraPtr = ff9.w_frameCameraPtr;
				Vector3 vector = w_frameCameraPtr.WorldToScreenPoint(pos);
				vector.x /= (float)w_frameCameraPtr.pixelWidth;
				vector.y /= (float)w_frameCameraPtr.pixelHeight;
				SFX_Rush.px = vector.x;
				SFX_Rush.py = vector.y;
				global::Debug.Log(string.Concat(new object[]
				{
					"px : ",
					SFX_Rush.px,
					" , py : ",
					SFX_Rush.py
				}));
			}
			else
			{
				SFX_Rush.px = 0.5f;
				SFX_Rush.py = 0.5f;
			}
		}
		else
		{
			Obj objUID = PersistenSingleton<EventEngine>.Instance.GetObjUID(250);
			if (objUID != null && objUID.cid == 4)
			{
				PosObj posObj = (PosObj)objUID;
				FieldMap fieldmap = PersistenSingleton<EventEngine>.Instance.fieldmap;
				Camera mainCamera = fieldmap.GetMainCamera();
				BGCAM_DEF currentBgCamera = fieldmap.GetCurrentBgCamera();
				Vector3 vertex = new Vector3(posObj.pos[0], posObj.pos[1], posObj.pos[2]);
				Vector3 position = PSX.CalculateGTE_RTPT(vertex, Matrix4x4.identity, currentBgCamera.GetMatrixRT(), currentBgCamera.GetViewDistance(), fieldmap.GetProjectionOffset());
				position = mainCamera.WorldToScreenPoint(position);
				position.x /= (float)mainCamera.pixelWidth;
				position.y /= (float)mainCamera.pixelHeight;
				SFX_Rush.px = position.x;
				SFX_Rush.py = position.y;
				global::Debug.Log(string.Concat(new object[]
				{
					"px : ",
					SFX_Rush.px,
					" , py : ",
					SFX_Rush.py
				}));
			}
			else
			{
				SFX_Rush.px = 0.5f;
				SFX_Rush.py = 0.5f;
			}
		}
	}

	// Token: 0x04002845 RID: 10309
	private const int RushParamLastFrame = 120;

	// Token: 0x04002846 RID: 10310
	private const int StartFadeOutFrame = 115;

	// Token: 0x04002847 RID: 10311
	private bool rush_type = true;

	// Token: 0x04002848 RID: 10312
	private int rush_seq;

	// Token: 0x04002849 RID: 10313
	private static float px;

	// Token: 0x0400284A RID: 10314
	private static float py;

	// Token: 0x0400284B RID: 10315
	private RenderTexture[] texture;

	// Token: 0x0400284C RID: 10316
	private float addCol;

	// Token: 0x0400284D RID: 10317
	private float addColDec;

	// Token: 0x0400284E RID: 10318
	private float subCol;

	// Token: 0x0400284F RID: 10319
	private float subColDec;

	// Token: 0x04002850 RID: 10320
	private float rot;

	// Token: 0x04002851 RID: 10321
	private float rotInc;

	// Token: 0x04002852 RID: 10322
	private float scale;

	// Token: 0x04002853 RID: 10323
	private float scaleAdd;

	// Token: 0x04002854 RID: 10324
	private bool flip;

	// Token: 0x04002855 RID: 10325
	private bool isUpdate;

	// Token: 0x04002856 RID: 10326
	private static Texture2D result;
}
