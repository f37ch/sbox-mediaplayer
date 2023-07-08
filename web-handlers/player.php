<html>
<body id="bod" style="overflow:hidden;width:100%;height:100%;margin:0;padding:0;background-image: radial-gradient(#333 0%,#161616 100%);background-size: 100% 100%;align-items:center">
<div id="player" style="pointer-events: none;"></div>
<script> const urlParams = new URLSearchParams(window.location.search);</script>
<?php
    $type=$_GET["tp"];
?>
<?php if ($type=="yt"){ ?><!--youtube-->
<script>
  var tag=document.createElement("script");
  tag.src="https://www.youtube.com/iframe_api";
  var firstScriptTag=document.getElementsByTagName("script")[0];
  firstScriptTag.parentNode.insertBefore(tag,firstScriptTag);
  var player;
  function onYouTubeIframeAPIReady() {
    player=new YT.Player("player",{
      height:"100%",
      width:"100%",
      videoId:urlParams.get("dt"),
      start:urlParams.get("st"),
      events: {
        "onReady":onPlayerReady,
      },
      playerVars: {
        autoplay:1,
        mute:0,
	    	controls:0,
	    	cc_load_policy:0,
	    	rel:0,
	    	iv_load_policy:3,
	    	disablekb:1
	    }});
  }
  function onPlayerReady(event){
    event.target.playVideo();
    event.target.setVolume(urlParams.get("vol"));
    event.target.seekTo(urlParams.get("st"));
    let volume=document.getElementById("volume-control");
    volume.value=urlParams.get("vol");
    volume.addEventListener("change", function(e) {
      event.target.setVolume(e.currentTarget.value);
      console.log(e.currentTarget.value);
    })
  }
</script>
<!--<input type="range" id="volume-control" style="position:fixed;width:20%;bottom:0;left:0;height:3vw;background-color: red;">-->
<?php }elseif($type=="tw"){ ?><!--twitch-->
<script src="https://player.twitch.tv/js/embed/v1.js"></script>
<script>
  var player;
  var id=urlParams.get("dt");
  if (id.startsWith("vid,")) {
		player = new Twitch.Player("player",{
			height:"100%",
			width:"100%",
      time:urlParams.get("st"),
			video:"v"+id.split(",")[1]
		});
	}else{
		player = new Twitch.Player("player",{
			height:"100%",
			width:"100%",
			channel:id
		});
	}
  setTimeout(function () {
    player.setVolume(urlParams.get("vol")/100);
  },2000);
</script>
<?php }elseif($type=="sc"){ ?><!--soundcloud-->
<script src="https://connect.soundcloud.com/sdk.js"></script>
<script src="https://w.soundcloud.com/player/api.js"></script>
<iframe id="scplayer" width="100%" height="180" style="border: 0 none;  pointer-events: all;" allow="autoplay"
src="https://w.soundcloud.com/player/"></iframe>
<script type="text/javascript">
	document.getElementById("scplayer").src="https://w.soundcloud.com/player/?url="+encodeURIComponent(urlParams.get("dt"))+"&color=ff3366&auto_play=true";
  var widget=SC.Widget(document.getElementById("scplayer"));
  function onPlayerReady(){
    setTimeout(function () {
      widget.seekTo(urlParams.get("st")*1000);
      widget.setVolume(urlParams.get("vol"));
    },500);
  }
  document.getElementById("bod").style.display="flex"
  widget.bind("SC.Widget.Events.PLAY",onPlayerReady())
</script>
<?php }elseif($type=="dr"){ ?><!--directLink-->
<script type="text/javascript">
  var vp = document.createElement("video");
	vp.src = urlParams.get("dt");
	vp.loop = false;
	vp.autoplay = true;
	vp.preload = "auto";
	vp.setAttribute("width","100%");
	vp.setAttribute("height","100%");
	vp.addEventListener("loadeddata",function() {
		if (vp.buffered.length===0) return;
			var bufferedSeconds=vp.buffered.end(0)-vp.buffered.start(0);
	},false)
	document.getElementById("player").appendChild(vp)
  vp.currentTime=urlParams.get("st");
  vp.volume=urlParams.get("vol")/100;
</script>
<?php }?>
</body>
</html>