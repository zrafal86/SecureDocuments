# escape=`

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
SHELL ["pwsh", "-Command", "$ErrorActionPreference = 'Stop';"]

WORKDIR C:\src
COPY .\SecureDocuments.sln .
COPY .\SecureDocuments\SecureDocuments.csproj .\SecureDocuments\
COPY .\SecureDocuments.ConsoleUtils\SecureDocuments.ConsoleUtils.csproj .\SecureDocuments.ConsoleUtils\
COPY .\SecureDocuments.UnitTests\SecureDocuments.UnitTests.csproj .\SecureDocuments.UnitTests\
COPY .\SecureDocuments.WPF\SecureDocuments.WPF.csproj .\SecureDocuments.WPF\
RUN dotnet restore SecureDocuments.sln

COPY . C:\src

ADD https://files.jrsoftware.org/is/6/innosetup-6.2.0.exe C:\tools\innosetup.exe

RUN C:\tools\innosetup.exe /VERYSILENT /LOG=C:\tools\innosetup_logs.log /NORESTART /DIR=C:\tools\innosetup\ /SUPPRESSMSGBOXES /CURRENTUSER /NOICONS; `
    dotnet publish .\SecureDocuments.WPF\ -c Release -r win10-x64 -p:PublishSingleFile=true --self-contained true; `
    dotnet publish .\SecureDocuments.WPF\ -c Release -r win7-x64 --self-contained true; `
    dotnet publish .\SecureDocuments.WPF\ -c Release -r win7-x86 --self-contained true