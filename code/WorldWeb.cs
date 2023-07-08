using Sandbox;
using Sandbox.UI;
using System;

namespace MediaPlayer;
public partial class WorldWeb : WorldPanel
{
	public WebPanel WebPanel { get; set; } 
	public ScreenUI ScreenUI { get; set; }
	public Controls Controls { get; set; }
	public float PanelScale=0.7F,ScreenScale=1,Trigger=0;
	private static float boundy,boundz;
	public MPScreen Screen;
	public WorldInput WorldInput=new();
	public WorldWeb(MPScreen scr)
	{ 
		Screen=scr;
		WebPanel=AddChild<WebPanel>();
		ScreenUI=WebPanel.AddChild<ScreenUI>();

		boundy=Screen.Model.Bounds.Size.y-16;
		boundz=Screen.Model.Bounds.Size.z-16;

		PanelBounds = new Rect(-boundy/2,-boundz-8,boundy,boundz);//align with screen borders

		WorldScale=20;

		Style.PointerEvents=PointerEvents.All;
		WebPanel.Style.Width=Length.Percent(100);
		WebPanel.Style.Height=Length.Percent(100);

		WebPanel.AcceptsFocus=false;
		ScreenUI.AcceptsFocus=false;
		MaxInteractionDistance=2000;
	}
	public override void Tick()
	{
		if (Screen.Scale!=ScreenScale){
			if (Game.IsClient){
				PanelBounds = new Rect(-Screen.Scale*(boundy/2),-Screen.Scale*(boundz+8),Screen.Scale*boundy,Screen.Scale*boundz);
				ScreenScale=Screen.Scale;
			}
		}
		if (Game.IsClient){
			if (Screen.IsValid){
				//SceneObject.Bounds=Screen.WorldSpaceBounds;
				//DebugOverlay.Box(SceneObject.Bounds.Mins,SceneObject.Bounds.Maxs);
				
				Position=Screen.LocalPosition;
				Rotation=Screen.LocalRotation;
				Position=Position+Rotation.Backward*0.6F;
				Rotation=Rotation.LookAt(Rotation.Backward,Rotation.Up);

				//var tr=Trace.Ray(Game.LocalPawn.AimRay.Position,Game.LocalPawn.AimRay.Position+Game.LocalPawn.AimRay.Forward*MaxInteractionDistance).Ignore(Game.LocalPawn).Run();
				//DebugOverlay.TraceResult(tr);
			}else if(IsValid){
				Delete();
				WorldInput.Enabled=false;
			}
			if (Scale!=PanelScale){
				Scale=PanelScale;
			}
			if (Controls.IsValid()){
				if (Controls.Slider.IsValid()){
					Controls.Slider.ActualMouse=MousePosActual;
					Controls.VolumeSlider.ActualMouse=MousePosActual;
					Controls.Slider.Value=Math.Clamp(Screen.Curtime,(Screen.Duration/100)*2,Screen.Duration);
				}
				if (!Screen.Paused){
					if (Screen.Curtime==Screen.Duration&&!Screen.IsLive){
						Screen.Curtime=0;
						Screen.Duration=0;
						Controls.Delete();
						ScreenUI=WebPanel.AddChild<ScreenUI>();
        			}else if(Trigger>Time.Now){// what a shit am i doing?
						WebPanel.Surface.TellMouseButton(MouseButtons.Left,true);
						WebPanel.Surface.TellMouseButton(MouseButtons.Left,false);
					}
				}
			}
		}
	}
	
	[Event.Client.BuildInput]
	public void BuildInput()
	{
		if (!ScreenUI.Menu.IsValid()){
			WorldInput.Ray=Game.LocalPawn.AimRay;
			WorldInput.MouseLeftPressed=Input.Down(InputButton.Use);
			if (WorldInput.Hovered != null && Input.Down(InputButton.Use) && !Input.Down(InputButton.PrimaryAttack))
			{
				if (Input.Pressed(InputButton.Use)){
					PlaySound("ui.button.press");
				}
				Input.ClearButton(InputButton.Use);
			}
			WorldInput.Enabled=true;
		}else{
			ScreenUI.Menu.ident=Screen.NetworkIdent;
			WorldInput.Enabled=false;
		}
	}
}
