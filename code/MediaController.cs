using System.Web;
using System;
using Sandbox;
namespace MediaPlayer;

public partial class MediaController
{
	public static float RunCD;
	const string WebHandlersURL="https://127.0.0.1/web-handlers/";
	[ClientRpc]
	public static void SeekClient(int ident)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		da.WorldWeb.WebPanel.Surface.Url=$"{WebHandlersURL}player.php?tp={da.Service}&st={da.Curtime}&dt={da.ContentID}&vol={da.Volume}";
		da.WorldWeb.Trigger=Time.Now+1;
	}
	[ClientRpc]
	public static void TogglePlayClient(int ident)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		if (da.Paused){
            da.WorldWeb.Controls.ToggleImg="/ui/play.png";
            da.WorldWeb.Controls.AddClass("Paused");
			da.WorldWeb.WebPanel.Surface.Url=$"{WebHandlersURL}player.php?tp=n";
        }else{
			da.WorldWeb.WebPanel.Surface.Url=$"{WebHandlersURL}player.php?tp={da.Service}&dt={da.ContentID}&st={da.Curtime}&vol={da.Volume}";
			da.WorldWeb.Trigger=Time.Now+1;
            da.WorldWeb.Controls.ToggleImg="/ui/pause.png";
            da.WorldWeb.Controls.RemoveClass("Paused");
        }
		
	}
	[ClientRpc]
	public static void SetUrlClient(int ident,float duration,string service,string content,string title)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		da.WorldWeb.ScreenUI.Delete();
		if (da.WorldWeb.Controls.IsValid()){da.WorldWeb.Controls.Delete();}
		da.WorldWeb.Controls=da.WorldWeb.WebPanel.AddChild<Controls>();
		var url=$"{WebHandlersURL}player.php?tp={service}&dt={content}&vol={da.Volume}";
		da.WorldWeb.WebPanel.Surface.Url=url;
		da.WorldWeb.Trigger=Time.Now+1;
		da.WorldWeb.Controls.RootEnt=da;
	}
	public static void SetUrlClientFirst(bool paused,int ident,float duration,string service,string content,string title)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		var url=string.Empty;

		da.WorldWeb.ScreenUI.Delete();
		da.WorldWeb.Controls=da.WorldWeb.WebPanel.AddChild<Controls>();
	
		if (paused){
			url=$"{WebHandlersURL}player.php?tp=n";
			da.WorldWeb.Controls.AddClass("Paused");
			da.WorldWeb.Controls.ToggleImg="/ui/pause.png";
		}else{
			url=$"{WebHandlersURL}player.php?tp={service}&dt={content}&vol={da.Volume}&st={da.Curtime}";
		}
		da.WorldWeb.WebPanel.Surface.Url=url;
		da.WorldWeb.Trigger=Time.Now+1;
		da.WorldWeb.Controls.RootEnt=da;
	}
	[ClientRpc]
	public static void SkipClient(int ident)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		da.WorldWeb.WebPanel.Surface.Url=$"{WebHandlersURL}player.php?tp=dl";
	}
	[ConCmd.Server]
	public static async void SetUrl(string url,int ident)
	{
		//var requester = ConsoleSystem.Caller?.Pawn as Player;
		//Chat.AddChatEntry("[MediaPlayer]","Bad YouTube URL!",requester.Client.SteamId,true);
		if (RunCD>Time.Now){
			Log.Error("[MediaPlayer] You're interacting too fast!");
		}else if(Uri.IsWellFormedUriString(url,UriKind.Absolute)){
			var da=Entity.FindByIndex(ident) as MPScreen;
			var uri = new UriBuilder(url).Uri;//try to build url
			if (uri.Host=="www.youtube.com"||uri.Host=="youtu.be"||uri.Host=="youtube.com"){//youtube links match
				var query = HttpUtility.ParseQueryString(uri.Query);
				var videoId = string.Empty;
				int pos=Array.IndexOf(query.AllKeys,"v");
				if (pos > -1)
				{
				    videoId = query["v"];
				}else{
					var seg=uri.Segments; 
				    videoId = seg[seg.Length-1];
				}
				try{
					var data=await Http.RequestJsonAsync<ParsedData>($"{WebHandlersURL}parser.php?tp=yt&id={videoId}","GET");
					da.Service="yt";da.Duration=data.duration;da.ContentID=videoId;da.Title=data.title;
					da.IsLive=da.Duration<=0;
					da.Curtime=da.Duration>0?1:0;
					SetUrlClient(ident,data.duration,"yt",videoId,data.title);
				}catch(Exception e){
					Log.Error("[MediaPlayer] Bad YouTube URL!");
				}
			}else if(uri.Host=="www.twitch.tv"||uri.Host=="twitch.tv"){
				var videoid=string.Empty;
				if (uri.Segments.Length<2||uri.Segments.Length>3){
					Log.Error("[MediaPlayer] Wrong twitch.tv url!");
				}else if (uri.Segments.Length==3&&uri.Segments[1]=="videos/"){
					videoid="vid,"+uri.Segments[2];
				}else{
					videoid=uri.Segments[1];
				}
				if (videoid!=""){
					if (videoid.StartsWith("vid,")){
					try{
						{
							var data=await Http.RequestJsonAsync<ParsedData>($"{WebHandlersURL}parser.php?tp=tw&id={uri.Segments[2]}","GET");
							da.Service="tw";da.Duration=data.duration;da.ContentID=videoid;da.Title=data.title;
						   	SetUrlClient(ident,data.duration,"tw",videoid,data.title);
						};
					}catch(Exception e){
						Log.Error("[MediaPlayer] Bad Twitch URL!");
					}
					}else{
						da.Service="tw";da.Duration=0;da.ContentID=videoid;da.Title="";
						da.IsLive=da.Duration<=0;
						da.Curtime=da.Duration>0?1:0;
						SetUrlClient(ident,0,"tw",videoid,"");
					}
				}
			}else if(uri.Host=="www.soundcloud.com"||uri.Host=="soundcloud.com"){
				if (uri.Segments.Length==3){
					url=$"https://soundcloud.com/{string.Join("",uri.Segments[1],uri.Segments[2])}";
					try{
						{
							var data=await Http.RequestJsonAsync<ParsedData>($"{WebHandlersURL}parser.php?tp=sc&id={url}","GET");
							da.Service="sc";da.Duration=data.duration;da.ContentID=url;da.Title=data.title;
							da.IsLive=da.Duration<=0;
							da.Curtime=da.Duration>0?1:0;
						   	SetUrlClient(ident,data.duration,"sc",url,data.title);
						};
					}catch(Exception e){
						Log.Error("[MediaPlayer] Bad SoundCloud URL!");
					}
				}else{
					Log.Error("[MediaPlayer] Wrong soundcloud url!");
				}
			}else{
				try{
					{
						var data=await Http.RequestJsonAsync<ParsedData>($"{WebHandlersURL}parser.php?tp=dr&id={url}","GET");
						da.Service="dr";da.Duration=data.duration;da.ContentID=url;da.Title=data.title;
						da.IsLive=da.Duration<=0;
						da.Curtime=da.Duration>0?1:0;
					   	SetUrlClient(ident,data.duration,"dr",url,data.title);
					};
				}catch(Exception e){
					Log.Error("[MediaPlayer] Bad DirectLink URL!");
				}
			}
			RunCD=Time.Now+2;
		}else{
			Log.Error("[MediaPlayer] Bad URL!");
		}
	}
	[ConCmd.Server]
	public static void Skip(int ident)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		da.IsLive=false;
		da.Curtime=da.Duration;
		SkipClient(ident);
	}
	[ConCmd.Server]
	public static void TogglePlay(int ident)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		if (!da.Paused){
            da.Paused=true;
        }else{
			da.Paused=false;
        }
		TogglePlayClient(ident);
	}
	[ConCmd.Server]
	public static void Seek(int ident,float time)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		da.Curtime=time;
		SeekClient(ident);
	}
	[ConCmd.Server]
	public static void SetVolume(int ident,float volume)
	{
		var da=Entity.FindByIndex(ident) as MPScreen;
		da.Volume=volume;
	}
}