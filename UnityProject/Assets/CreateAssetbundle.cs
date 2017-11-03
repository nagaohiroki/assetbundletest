using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CreateAssetbundle : MonoBehaviour
{
	[SerializeField]
	enum PathType
	{
		LocalHost,
		LocalPath
	}
	[SerializeField]
	PathType mPathType;

	[SerializeField]
	string mAssetbundleName;

	UnityWebRequest download;
	TextMesh text;
	// プラットホームごとのディレクトリ
	string PlatformDirectory
	{
		get
		{
#if UNITY_STANDALONE_OSX
			return "OSX";
#elif UNITY_STANDALONE_WIN
			return "WIN";
#elif UNITY_WEBGL
			return "WEBGL";
#elif UNITY_IOS
			return "IOS";
#elif UNITY_ANDROID
			return "ANDROID";
#endif
		}
	}
	string GetLocalHostUrl( string inAssetbundleName )
	{
		return string.Format( "http://localhost:8000/{0}/{1}", PlatformDirectory, inAssetbundleName );
	}
	string GetLocalPath( string inAssetbundleName )
	{
		string assetbunlePath = Path.Combine( "file:///" + Application.streamingAssetsPath, PlatformDirectory );
		return Path.Combine( assetbunlePath, inAssetbundleName );
	}
	string GetUrl()
	{
		switch( mPathType )
		{
		case PathType.LocalHost:
		{
			return GetLocalHostUrl( mAssetbundleName );
		}
		case PathType.LocalPath:
		{
			return GetLocalPath( mAssetbundleName );
		}
		}
		return null;
	}
	void Start()
	{
		download = UnityWebRequest.GetAssetBundle( GetUrl() );
		download.SendWebRequest();
		text = GetComponent<TextMesh>();
	}
	void Update()
	{
		text.text = download.url + "\n";
		text.text += "Progress:" + download.downloadProgress + "\n";
		if( download.isNetworkError )
		{
			text.text += "失敗\n";
			text.text += download.error;
			return;
		}
		if( !download.isDone )
		{
			return;
		}
		text.text += "成功\n";
		var abHeader = download.downloadHandler as DownloadHandlerAssetBundle;
		if( abHeader == null )
		{
			text.text += "AB失敗\n";
			return;
		}
		text.text += "AB成功\n";
		var ab = abHeader.assetBundle;
		foreach( var n in ab.GetAllAssetNames() )
		{
			text.text += n + "\n";
		}
		var a = ab.LoadAsset<TextAsset>( "test" );
		text.text += a.text + "\n";
	}
#if UNITY_EDITOR
	[MenuItem( "Assets/Build AssetBundles %#a" )]
	static void BuildAssetBundles()
	{
		var buildTargetToDir = new Dictionary<BuildTarget, string>
		{
			{BuildTarget.StandaloneWindows64, "WIN"},
			{BuildTarget.StandaloneOSXIntel64, "OSX"},
			{BuildTarget.WebGL, "WEBGL"},
			{BuildTarget.iOS, "IOS"},
			{BuildTarget.Android, "ANDROID"},
		};
		foreach( var p in buildTargetToDir )
		{
			string dir = Path.Combine( Application.streamingAssetsPath, p.Value );
			// ディレクトリの作成
			Directory.CreateDirectory( dir );
			// アセットバンドルの作成
			var manifest = BuildPipeline.BuildAssetBundles( dir, BuildAssetBundleOptions.None, p.Key );
			if( manifest == null )
			{
				Debug.LogError( dir + "を作成失敗" );
				return;
			}
			Debug.Log( dir + "を作成しました" );
		}
	}
#endif
}
