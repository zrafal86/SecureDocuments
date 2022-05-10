@ECHO off
cls

ECHO Deleting all BIN and OBJ folders...
ECHO.

FOR /d /r . %%d in (bin,obj) DO (
	IF EXIST "%%d" (
		ECHO %%d | FIND /I "\node_modules\" > Nul && ( 
			ECHO.Skipping: %%d
		) || (
			ECHO.Deleting: %%d
			rd /s/q "%%d"
		)
	)
)

ECHO.
ECHO.BIN and OBJ folders have been successfully deleted.

dotnet publish .\SecureDocuments.WPF\ -c Release -r win10-x64 -p:PublishSingleFile=true --self-contained true
dotnet publish .\SecureDocuments.WPF\ -c Release -r win7-x64 --self-contained true
dotnet publish .\SecureDocuments.WPF\ -c Release -r win7-x86 --self-contained true

rem add to system env:path C:\Program Files (x86)\Inno Setup 6
rem iscc deployed_app_definition_win10_x64.iss
rem iscc deployed_app_definition_win7_x64.iss
rem iscc deployed_app_definition_win7_x86.iss