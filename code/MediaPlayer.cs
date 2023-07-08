using Editor;
using Sandbox;
using System;
using Sandbox.UI;
using System.Threading.Tasks;
namespace MediaPlayer;
[Library("ent_mediaplayer",Title="Play mediacontent and have fun!"),HammerEntity,Spawnable,EditorModel("models/screen.vmdl")]
public partial class MPScreen:ModelEntity
{
	public WorldWeb WorldWeb{get;set;}
	public float NextThink{get;set;}=0;
	[Net] public float Duration{get;set;}=0;
	[Net] public float Curtime{get;set;}=0;
	[Net] public string Service{get;set;}
	[Net] public string ContentID{get;set;}
	[Net] public bool Paused {get;set;}=false;
	[Net] public bool IsLive {get;set;}=false;
	[Net] public float Volume{get;set;}=50;
	[Net] public string Title {get;set;}="";
	public override void Spawn()
	{
		base.Spawn();
		SetModel("models/screen.vmdl");
		SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
		Name="MediaScreen";
		Transmit=TransmitType.Always;
	}
	public override void ClientSpawn()
	{
		WorldWeb=new WorldWeb(this);
		if (ContentID!=null){
			MediaController.SetUrlClientFirst(Paused,NetworkIdent,Duration,Service,ContentID,Title);
		}
	}
	[Event.Tick]
	public void da(){
		if (Game.IsServer){
			if (!Paused&&Curtime>0&&NextThink<Time.Now&&!IsLive){
            	Curtime=+Math.Clamp(Curtime+1,0,Duration);
            	NextThink=Time.Now+1;
        	}
		}
    }
}
