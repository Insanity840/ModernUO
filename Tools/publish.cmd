:<<"::SHELLSCRIPT"
@ECHO OFF
GOTO :CMDSCRIPT

::SHELLSCRIPT
if [[ -z $2 ]]
then
  c="-c Release"
else
  c="-c $2"
fi

Tools/build-native-libraries.cmd $2
dotnet restore

if [[ $1 ]]
then
  r="-r $1-x64"
else
  if [[ $(uname) = "Darwin" ]]
  then
    r="-r osx-x64"
  else
    r="-r linux-x64"
  fi
fi

dotnet publish ${c} ${r} --self-contained=false -o Distribution Projects/Server/Server.csproj
dotnet publish ${c} ${r} --self-contained=false -o Distribution/Assemblies Projects/UOContent/UOContent.csproj
exit $?

:CMDSCRIPT
IF "%~2" == "" (
  SET c=-c Release
) ELSE (
  SET c=-c %~2
)

CALL Tools\build-native-libraries.cmd %~2
dotnet restore

IF "%~1" == "" (
  SET r=-r win-x64
) ELSE (
  SET r=-r %~1-x64
)

dotnet publish %c% %r% --self-contained=false -o Distribution Projects\Server\Server.csproj
dotnet publish %c% %r% --self-contained=false -o Distribution\Assemblies Projects\UOContent\UOContent.csproj
