nuget restore
msbuild BasicBot.sln -p:DeployOnBuild=true -p:PublishProfile=tostibot-Web-Deploy.pubxml -p:Password=AXaPfqEahuumraESzLcvNrmjfl8E5ctyJSjWH90MxTSnGwHtmuZuimi2PX1d

