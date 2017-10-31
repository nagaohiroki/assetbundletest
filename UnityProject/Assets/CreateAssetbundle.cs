using System.IO;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CreateAssetbundle : MonoBehaviour
{
	UnityWebRequest download;
	TextMesh text;
	HttpListener listener;
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
	void Start()
	{
		// localhost test 今は動かない
		listener = new HttpListener();
		const string prefix = "http://localhost:8089/";
		listener.Prefixes.Add( prefix );
		listener.Start();
		string assetbunlePath = "";
		assetbunlePath += "file:///";
		assetbunlePath += Path.Combine( Application.streamingAssetsPath, PlatformDirectory );
		assetbunlePath = Path.Combine( assetbunlePath, "test" );
		download = UnityWebRequest.GetAssetBundle( assetbunlePath );
		download.SendWebRequest();
		text = GetComponent<TextMesh>();
	}
	void Update()
	{
		if( download.isNetworkError )
		{
			text.text = download.error;
			return;
		}
		if( !download.isDone )
		{
			return;
		}
		text.text = "成功\n";
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
		var mBuildTargetToDir = new Dictionary<BuildTarget, string>
		{
			{BuildTarget.StandaloneWindows64, "WIN"},
			{BuildTarget.StandaloneOSXIntel64, "OSX"},
			{BuildTarget.WebGL, "WEBGL"},
			{BuildTarget.iOS, "IOS"},
		};
		foreach( var p in mBuildTargetToDir )
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
