<?php
header("Content-type: text/plain; charset=utf-8");
function InitDB($server="localhost",$user="root",$pass="",$dbname="mediaplayer"){
    $tables=[
        "CREATE TABLE IF NOT EXISTS mediadata (
        id varchar(400) NOT NULL,
        type varchar(10) DEFAULT NULL,
        duration float(11) DEFAULT 0,
        title varchar(400) DEFAULT NULL,
        thumb varchar(400) DEFAULT NULL,
        PRIMARY KEY (id));"
    ];
    $connection = mysqli_connect($server,$user,$pass);
    if (!$connection){
        die("Connection failed: ".mysqli_connect_error());
    }
    $sql="CREATE DATABASE IF NOT EXISTS $dbname";
    if (mysqli_query($connection,$sql)){
        $connection=mysqli_connect($server,$user,$pass,$dbname);
        foreach($tables as $val){
            $query = $connection->query($val);
            if(!$query){
                die("Creation db failed ($connection->error)");
            }
        }
    }else{
        die("Creation db failed ($connection->error)");
    }
    return $connection;
}
$database=InitDB("127.0.0.1:3306","mediaplayer","aGoodPassword","mediaplayer");
function GetCached($id,$tp){
    $trybd=$GLOBALS["database"]->query("SELECT * FROM mediadata WHERE type='$tp' AND id='$id';")->fetch_assoc();
    if($trybd){
        return $trybd;
    }else{
        return false;
    }
}
$type=$_GET["tp"];
$id=$_GET["id"];
$cached=GetCached($id,$type);

if ($type=="yt"){
if ($cached){
    echo json_encode($cached);
}else{
    $api="!!!!!!!!!!!!!!!!!!";//insert YouTube API here
    $url="https://www.googleapis.com/youtube/v3/videos?id=".$id."&key=".$api."&part=snippet,contentDetails,status&videoEmbeddable=true&videoSyndicated=true";
    $ch=curl_init($url);
    curl_setopt($ch,CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V4);
    curl_setopt($ch,CURLOPT_RETURNTRANSFER,1);
    $data=curl_exec($ch);
    curl_close($ch);
    $ara=json_decode($data)->items[0];
    function ISO8601ToSeconds($ISO8601){
        $interval = new \DateInterval($ISO8601);
        return ($interval->d*24*60*60)+($interval->h*60*60)+($interval->i*60)+$interval->s;
    }
    $duration=ISO8601ToSeconds($ara->contentDetails->duration);
    $title=$database->real_escape_string($ara->snippet->title);
    $thumb=$ara->snippet->thumbnails->medium->url;
    $database->query("INSERT INTO mediadata (id,type,duration,title,thumb) VALUES ('$id','$type','$duration','$title','$thumb')");
    echo json_encode(array("duration"=>$duration,"title"=>$title,"thumb"=>$thumb));
}
}elseif($type=="tw"){
if ($cached){
    echo json_encode($cached);
}else{
    $ClientID="!!!!!!!!!!!!!!!!!!";//insert Twitch ClientID here
    $ClientSecret="!!!!!!!!!!!!!!!!!!";//insert Twitch ClientSecret here
    $url="https://id.twitch.tv/oauth2/token?client_id=".$ClientID."&client_secret=".$ClientSecret."&grant_type=client_credentials";
    $ch=curl_init($url);
    curl_setopt($ch,CURLOPT_IPRESOLVE,CURL_IPRESOLVE_V4);
    curl_setopt($ch,CURLOPT_POST,true);
    curl_setopt($ch,CURLOPT_RETURNTRANSFER,1);
    $data=curl_exec($ch);
    curl_close($ch);
    $ara=json_decode($data);

    $ch=curl_init("https://api.twitch.tv/helix/videos?id=".$id);
    curl_setopt($ch,CURLOPT_IPRESOLVE,CURL_IPRESOLVE_V4);
    curl_setopt($ch,CURLOPT_RETURNTRANSFER,1);
    curl_setopt($ch, CURLOPT_HTTPHEADER, ["Authorization: Bearer ".$ara->access_token,"Client-ID: ".$ClientID]);
    $data=curl_exec($ch);
    curl_close($ch);
    $ara=json_decode($data)->data[0];

    $dur = preg_split("/([a-zA-Z])/",$ara->duration);
    if (count($dur)==2){
        $dur=$dur[0];
    }elseif(count($dur) == 3){
    	$dur=(60*$dur[0])+$dur[1];
    }elseif (count($dur) == 4){
    	$dur=(3600*$dur[0])+(60*$dur[1])+$dur[2];
    }
    $title=$database->real_escape_string($ara->title);
    $thumb=str_replace("{width}x{height}","854x464",$ara->thumbnail_url);;
    $database->query("INSERT INTO mediadata (id,type,duration,title,thumb) VALUES ('$id','$type','$dur','$title','$thumb')");
    echo json_encode(array("duration"=>$dur,"title"=>$title,"thumb"=>$thumb));
}
}elseif($type=="twl"){
    if ($cached){
        echo json_encode($cached);
    }else{
        $ClientID="!!!!!!!!!!!!!!!!!!";//insert Twitch ClientID here
        $ClientSecret="!!!!!!!!!!!!!!!!!!";//insert Twitch ClientSecret here
        $url="https://id.twitch.tv/oauth2/token?client_id=".$ClientID."&client_secret=".$ClientSecret."&grant_type=client_credentials";
        $ch=curl_init($url);
        curl_setopt($ch,CURLOPT_IPRESOLVE,CURL_IPRESOLVE_V4);
        curl_setopt($ch,CURLOPT_POST,true);
        curl_setopt($ch,CURLOPT_RETURNTRANSFER,1);
        $data=curl_exec($ch);
        curl_close($ch);
        $ara=json_decode($data);
    
        $ch=curl_init("https://api.twitch.tv/helix/streams?user_login=".$id);
        curl_setopt($ch,CURLOPT_IPRESOLVE,CURL_IPRESOLVE_V4);
        curl_setopt($ch,CURLOPT_RETURNTRANSFER,1);
        curl_setopt($ch, CURLOPT_HTTPHEADER, ["Authorization: Bearer ".$ara->access_token,"Client-ID: ".$ClientID]);
        $data=curl_exec($ch);
        curl_close($ch);
        $ara=json_decode($data)->data[0];

        $title=$database->real_escape_string($ara->title);
        $thumb=str_replace("{width}x{height}","854x464",$ara->thumbnail_url);
        $dur=0;
        $database->query("INSERT INTO mediadata (id,type,duration,title,thumb) VALUES ('$id','$type','$dur','$title','$thumb')");
        echo json_encode(array("duration"=>$dur,"title"=>$title,"thumb"=>$thumb));
    }
}elseif($type=="sc"){
if ($cached){
    echo json_encode($cached);
}else{
    $client_id="!!!!!!!!!!!!!!!!!!";//insert Soundcloud ClientID here
    $api_url="https://api-widget.soundcloud.com/resolve?url=".$id."&format=json&client_id=".$client_id;
    $ch=curl_init($api_url);
    curl_setopt($ch,CURLOPT_IPRESOLVE, CURL_IPRESOLVE_V4);
    curl_setopt($ch,CURLOPT_RETURNTRANSFER,1);
    $data=curl_exec($ch);
    curl_close($ch);
    $ara=json_decode($data);
    $thumb=$ara->artwork_url??"https://i.imgur.com/sOQKRlm.png";
    $duration=round($ara->duration/1000);
    $title=$ara->title;
    $database->query("INSERT INTO mediadata (id,type,duration,title,thumb) VALUES ('$id','$type','$duration','$title','$thumb')");
    echo json_encode(array("thumb"=>$thumb,"duration"=>$duration,"title"=>$title));
}
}elseif($type=="dr"){
    if ($cached){
        echo json_encode($cached);
    }else{
        $probe_duration=exec("ffprobe -v quiet -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 ".$id);
        $database->query("INSERT INTO mediadata (id,type,duration,title) VALUES ('$id','$type','$probe_duration','$id')");
        echo json_encode(array("thumb"=>"https://i.imgur.com/IEsEHCH.png","duration"=>round($probe_duration),"title"=>$id));
    }
}
?>