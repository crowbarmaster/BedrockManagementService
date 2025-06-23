class BedrockInfo {
   [string]$version;
   [string]$winurl;
   [string]$linurl;
   [PropInfoEntry[]]$proplist;
}

class PropInfoEntry {
  [string]$Key;
  [string]$Value;
}

$ConnectionHeaders = @{
	"Accept"="text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";
	"Accept-Encoding"="gzip, deflate, br, zstd";
	"Accept-Language"="en-US,en;q=0.9";
	"User-Agent"="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36"
};
[Net.ServicePointManager]::SecurityProtocol = "tls12", "tls11";
$reply = Invoke-WebRequest -UseBasicParsing -Headers $ConnectionHeaders -Uri "https://net-secondary.web.minecraft-services.net/api/v1.0/download/links";
$versionJson = $reply.Content | ConvertFrom-Json;
$bedrockWinUrl = "";
$bedrockLinuxUrl = "";
foreach ($entry in $versionJson.result.links) {
	if($entry.downloadType -eq "serverBedrockWindows") {
		$bedrockWinUrl = $entry.downloadUrl;
	}
	if($entry.downloadType -eq "serverBedrockLinux") {
		$bedrockLinuxUrl = $entry.downloadUrl;
	}
}
$propMftFile = get-content 'MMS_Files\\bedrock_version_prop_manifest.json';
if ($bedrockWinUrl -match 'bedrock-server-(?<version>.+)\.zip') {
   $version = $Matches.version;
   if ($propMftFile) {
	   $propMftJson = $propMftFile | ConvertFrom-Json;
	   $latestVer = $propMftJson | Select-Object -Last 1;
	   $entryVer = $latestVer.version;
	   if($entryVer -ne $version) {
		  mkdir 'BedrockTemp';
		  $curDir = Get-Location;
		  $zipPath = $curDir.Path + '\\BedrockTemp\\Server.zip'
		  Invoke-WebRequest -UseBasicParsing -Headers $ConnectionHeaders -Uri $latestVer.winurl -OutFile $zipPath;
		  $zipStream = New-Object IO.FileStream $zipPath, 'Open', 'Read', 'Read';
		  $zipArchive = New-Object IO.Compression.ZipArchive $zipStream, "Read";
		  $propList = @();
		  foreach ($file in $zipArchive.Entries) {
			 if ($file.Name -eq 'server.properties') {
				$fileStream = $file.Open();
				$reader = New-Object IO.StreamReader $fileStream;
				while (($line = $reader.ReadLine()) -ne $null) {
					if ($line -ne "" -And !$line.StartsWith('#')) {
						$kvp = $line.Split('=');
						if ($kvp.Length -lt 2) {
							$temp = [string[]];
							$temp[0] = $kvp[0];
							$temp[1] = "";
							$kvp = $temp;
						}
						$newProp = [PropInfoEntry]@{
							Key = $kvp[0];
							Value = $kvp[1];
						}
						$propList += $newProp;
					}
				}
			 }
		  }        
		  $zipArchive.Dispose();
		  $zipStream.Close();
		  $newListEntry = [BedrockInfo]@{
			version=$version;
			winurl=$bedrockWinUrl;
			linurl=$bedrockLinuxUrl;
			proplist= $propList;
		  }
		  $propMftJson += $newListEntry;
		  $propMftJson | ConvertTo-Json -depth 100 | Out-File "MMS_Files\\bedrock_version_prop_manifest.json";
	 }
  }			
}