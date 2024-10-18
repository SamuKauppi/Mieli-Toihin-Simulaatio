using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class Link : MonoBehaviour 
{
	//Linkkien avaamis skripti
	public void OpenTextures()
	{
	#if !UNITY_EDITOR
		openWindow("https://www.textures.com/");
	#endif
	}

	public void OpenTilluke()
	{
	#if !UNITY_EDITOR
		openWindow("http://tiputilluke.blogspot.com/");
	#endif
	}

	public void OpenLeantween()
	{
#if !UNITY_EDITOR
		openWindow("https://assetstore.unity.com/packages/tools/animation/leantween-3595");
#endif
	}

	public void OpenQuickOutline()
	{
#if !UNITY_EDITOR
		openWindow("https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488");
#endif
	}

	public void OpenSkyboxFree()
	{
#if !UNITY_EDITOR
		openWindow("https://assetstore.unity.com/packages/2d/textures-materials/sky/skybox-series-free-103633");
#endif
	}

	public void OpenHyperlink()
	{
#if !UNITY_EDITOR
		openWindow("https://www.youtube.com/channel/UC9Z1XWw1kmnvOOFsj6Bzy2g");
#endif
	}
	public void OpenMigfus()
	{
#if !UNITY_EDITOR
		openWindow("https://freesound.org/people/Migfus20/");
#endif
	}
	public void OpenShadydave()
	{
#if !UNITY_EDITOR
		openWindow("https://freesound.org/people/ShadyDave/");
#endif
	}
	public void OpenLoboLoco()
	{
#if !UNITY_EDITOR
		openWindow("https://freemusicarchive.org/music/Lobo_Loco");
#endif
	}
	public void OpenBertz()
	{
#if !UNITY_EDITOR
		openWindow("https://freesound.org/people/Bertsz/");
#endif
	}
	public void OpenVollbeat()
	{
#if !UNITY_EDITOR
		openWindow("https://freesound.org/people/vollkornbrot/");
#endif
	}

	[DllImport("__Internal")]
	private static extern void openWindow(string url);

}