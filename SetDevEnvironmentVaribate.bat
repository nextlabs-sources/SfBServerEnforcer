@echo off

echo ���Թ���Ա��������

rem ��Ҫ���õĻ�������: WORKSPACE, NLBUILDROOT, BUID_OUTPUT, BUILD_NUMBER

set BAT_DIR=%~dp0
set BAT_DIR=%BAT_DIR:~0,-1%

setx /m WORKSPACE %BAT_DIR%
setx /m NLBUILDROOT %BAT_DIR%
setx /m BUID_OUTPUT %BAT_DIR%\output
setx /m BUILD_NUMBER 100
setx /m SFBVERSION "SFB 2019"
setx /m SFBDNETVERSION "v4.7"

echo WORKSPACE=%WORKSPACE%
echo NLBUILDROOT=%NLBUILDROOT%
echo BUID_OUTPUT=%BUID_OUTPUT%
echo BUILD_NUMBER=%BUILD_NUMBER%
echo SFBVERSION=%SFBVERSION%

echo -----------------------
echo �����´򿪵�dev.exe�б���
echo ������ʾ�����ڿ����޷��˳������ֶ��ر�
echo -----------------------

rem set VC_VARS_ALL_BAT=%VS140COMNTOOLS%
rem set VC_VARS_ALL_BAT=%VC_VARS_ALL_BAT:Tools=IDE%devenv.exe

rem mkdir "%BAT_DIR%\.git\hooks"
rem copy /y "%BAT_DIR%\pre-commit" "%BAT_DIR%\.git\hooks\pre-commit"

pause