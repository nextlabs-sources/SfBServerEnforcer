// oeinstca.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <msi.h>
#include <msiquery.h>
#include <stdio.h>
#include <Winreg.h>
#include <Shlwapi.h>
#include <shellapi.h>
#include <string>
#include <fstream>
#include <iostream>
#include <tlhelp32.h>

using namespace std;

#define MAX_BUFFER 1024

#define NLLYNCENDPOINTPROXY_COMM L"sfbe.xml"
#define NLLYNCENDPOINTPROXY_LOGCFG L"sfbe_log.xml"
#define SFBSERVERENFORCER_AM L"SfbServerEnforcer.am"

#define PRODUCT_NAME L"NextLabs Entitlement Management for Skype"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

//Note:  Messagebox can not use in defered execution since not be able to get UILevel property
UINT _stdcall MessageAndLogging(MSIHANDLE hInstall, BOOL bLogOnly, const WCHAR* wstrMsg )
{
	if(bLogOnly == FALSE && hInstall!= NULL)
	{
		INT nUILevel =0;
		WCHAR wstrTemp[2] = {0};
		DWORD dwBufsize = 0;
		
		dwBufsize = sizeof(wstrTemp)/sizeof(WCHAR);	
		if(ERROR_SUCCESS == MsiGetProperty(hInstall, TEXT("UILevel"), wstrTemp, &dwBufsize))
		{
			nUILevel = _wtoi(wstrTemp);
		}

		if(nUILevel > 2)
		{
			MessageBox(GetForegroundWindow(),(LPCWSTR) wstrMsg, (LPCWSTR)PRODUCT_NAME, MB_OK|MB_ICONWARNING);	
		}
	}

	//add log here
	PMSIHANDLE hRecord = MsiCreateRecord(1);
	if(hRecord !=NULL)
	{
		MsiRecordSetString(hRecord, 0, wstrMsg);
		// send message to running installer
		MsiProcessMessage(hInstall, INSTALLMESSAGE_INFO, hRecord);
		MsiCloseHandle(hRecord);
	}

	
	return ERROR_SUCCESS;
}//return service current status, or return 0 for service not existed


BOOL SHCopy(LPCWSTR from, LPCWSTR to, BOOL bDeleteFrom)
{
	SHFILEOPSTRUCT fileOp = {0};
	WCHAR newFrom[MAX_PATH];
	WCHAR newTo[MAX_PATH];

	if(bDeleteFrom)
		fileOp.wFunc = FO_MOVE;
	else
		fileOp.wFunc = FO_COPY;

	fileOp.fFlags = FOF_SILENT | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_NOCONFIRMMKDIR;

	wcscpy_s(newFrom, from);
	newFrom[wcslen(from) + 1] = NULL;
	fileOp.pFrom = newFrom;

	wcscpy_s(newTo, to);
	newTo[wcslen(to) + 1] = NULL;
	fileOp.pTo = newTo;

	int result = SHFileOperation(&fileOp);

	return result == 0;
}

BOOL SHDelete(LPCWSTR strFile)
{
	SHFILEOPSTRUCT fileOp = { 0 };
	WCHAR newFrom[MAX_PATH] = { 0 };

	fileOp.wFunc = FO_DELETE;
	fileOp.fFlags = FOF_MULTIDESTFILES | FOF_SILENT | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_NOCONFIRMMKDIR;

	wcscpy_s(newFrom, strFile);
	newFrom[wcslen(strFile) + 1] = NULL;
	fileOp.pFrom = newFrom;
	fileOp.pTo = NULL;
	fileOp.fAnyOperationsAborted = true;

	int result = SHFileOperation(&fileOp);

	return result == 0;
}

void ReplaceString(std::wstring &s, const std::wstring &to_find, const std::wstring &replace_with)
{
	std::wstring result;
	std::wstring::size_type pos = 0;
	while(1)
	{
		std::wstring::size_type next = s.find(to_find, pos);
		result.append(s, pos, next-pos);
		if(next != std::wstring::npos)
		{
			result.append(replace_with);
			pos = next + to_find.size();
		}
		else
			break;
	}
	s.swap(result);
	return;	
}


int ReplaceStringInFile(wfstream &inFile, wfstream &outFile, const std::wstring &to_find, const std::wstring &repl_with)
{	
	wchar_t strReplace[MAX_BUFFER];	
	while(!inFile.eof())
	{
		inFile.getline(strReplace,MAX_BUFFER,'\n');
		wstring s;
		s = strReplace;
		if(!s.empty())
		{
			ReplaceString( s, to_find, repl_with) ;	
		}
		outFile <<s <<endl;
	}
	return 1;
}




UINT __stdcall Find_NLLYNCENDPOINTPROXY_COMM_File(MSIHANDLE hInstall)
{
	WCHAR wstrSourceDir[MAX_PATH + 1] = { 0 };
	WCHAR wstrTemp[MAX_PATH + 1] = { 0 };
	DWORD dwPathBuffer = 0;
	UINT uiRet = 0;
	WCHAR wstrMsg[128] = { 0 };
	DWORD dwErrorCode = 0;
	BOOL bFindFile = FALSE;

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Start checking file NLLYNCENDPOINTPROXY_COMM"));

	//get temp path
	DWORD dwRetVal = 0;
	dwRetVal = GetTempPath(MAX_PATH + 1, wstrTemp);
	if ((dwRetVal > MAX_PATH + 1) || (dwRetVal == 0))
	{
		MessageAndLogging(hInstall, FALSE, TEXT("Failed to get TEMP path in this computer."));
		return ERROR_INSTALL_FAILURE;
	}

	// verify temp path exists
	if (wstrTemp[wcslen(wstrTemp) - 1] != L'\\')
	{
		wcscat_s(wstrTemp, L"\\");
	}
	wcscat_s(wstrTemp, L"NxPCFile\\");

	HANDLE hTempFile = INVALID_HANDLE_VALUE;
	hTempFile = CreateFile(wstrTemp,
		GENERIC_READ,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING | CREATE_NEW,
		FILE_FLAG_BACKUP_SEMANTICS,
		NULL);

	if (hTempFile == INVALID_HANDLE_VALUE)
	{
		if (!CreateDirectory(wstrTemp, NULL))
		{
			dwErrorCode = GetLastError();
			if (dwErrorCode != ERROR_ALREADY_EXISTS)
			{
				swprintf_s(wstrMsg, 128, L"Failed to create temp path. Error Code: %d", dwErrorCode);
				MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);
				return ERROR_INSTALL_FAILURE;
			}
		}
	}
	CloseHandle(hTempFile);

	// Move file from source to temp
	if (wstrTemp[wcslen(wstrTemp) - 1] != L'\\')
	{
		wcscat_s(wstrTemp, L"\\");
	}
	wcscat_s(wstrTemp, NLLYNCENDPOINTPROXY_COMM);
	SetFileAttributes(wstrTemp, FILE_ATTRIBUTE_NORMAL);
	//Clean up temp file first
	DeleteFile(wstrTemp);

	if (bFindFile == FALSE)
	{
		// Check if file exists in current directory
		ZeroMemory(wstrSourceDir, sizeof(wstrSourceDir));
		uiRet = 0;
		dwPathBuffer = sizeof(wstrSourceDir) / sizeof(WCHAR);

		uiRet = MsiGetProperty(hInstall, TEXT("CURRENTDIRECTORY"), wstrSourceDir, &dwPathBuffer);
		if (uiRet != ERROR_SUCCESS)
		{
			dwErrorCode = GetLastError();
			swprintf_s(wstrMsg, 128, L"Failed to get current directory from installer. Error Code: %d", dwErrorCode);
			MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);

			return ERROR_INSTALL_FAILURE;
		}

		//Check if file exist
		if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
		{
			wcscat_s(wstrSourceDir, L"\\");
		}
		wcscat_s(wstrSourceDir, NLLYNCENDPOINTPROXY_COMM);

		if (GetFileAttributes(wstrSourceDir) == INVALID_FILE_ATTRIBUTES && GetLastError() == ERROR_FILE_NOT_FOUND)
		{
			MessageAndLogging(hInstall, TRUE, TEXT("The installer could not find the NLLYNCENDPOINTPROXY_COMM.xml file in the MSI folder."));
		}
		else
		{
			MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Use NLLYNCENDPOINTPROXY_COMM.xml from MSI folder"));
			bFindFile = TRUE;
		}
	}

	if (bFindFile == FALSE)
	{
		// Check File in installer folder c:\program files\nextlabs
		ZeroMemory(wstrSourceDir, sizeof(wstrSourceDir));
		dwPathBuffer = sizeof(wstrSourceDir) / sizeof(WCHAR);
		uiRet = 0;
		uiRet = MsiGetProperty(hInstall, TEXT("INSTALLDIR"), wstrSourceDir, &dwPathBuffer);
		if (uiRet == ERROR_SUCCESS)
		{
			if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
			{
				wcscat_s(wstrSourceDir, L"\\");
			}
			wcscat_s(wstrSourceDir, L"Sfb Server Enforcer\\config\\");
			wcscat_s(wstrSourceDir, NLLYNCENDPOINTPROXY_COMM);
			MessageAndLogging(hInstall, TRUE, wstrSourceDir);

			// exist in Policy Controller folder
			if ((GetFileAttributes(wstrSourceDir) == INVALID_FILE_ATTRIBUTES))
			{
				MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: can't find NLLYNCENDPOINTPROXY_COMM.xml from Program Files\\nextlabs"));
			}
			else
			{
				MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Use NLLYNCENDPOINTPROXY_COMM.xml from existing Program Files\\nextlabs"));
				bFindFile = TRUE;
			}
		}
	}

	if (bFindFile == FALSE)
	{
		MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: can't find NLLYNCENDPOINTPROXY_COMM.xml both MSI folder and existing nextlabs folder. Let's use default one."));
		return ERROR_INSTALL_FAILURE;
	}

	// start copying to temp
	if (CopyFile(wstrSourceDir, wstrTemp, FALSE) == FALSE) //Failed
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Failed to copy file to temp path. Error Code: %d", dwErrorCode);
		MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);
		return ERROR_INSTALL_FAILURE;
	}

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Checking file NLLYNCENDPOINTPROXY_COMM.xml done.  Status: Good."));

	return ERROR_SUCCESS;
}

UINT __stdcall Copy_NLLYNCENDPOINTPROXY_COMM_File(MSIHANDLE hInstall) //run in defered execution
{
	WCHAR wstrSourceDir[MAX_PATH + 1] = { 0 };
	WCHAR wstrInstallDir[MAX_PATH + 1] = { 0 };
	DWORD dwPathBuffer = 0;
	WCHAR wstrMsg[128] = { 0 };
	DWORD dwErrorCode = 0;

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Start copy NLLYNCENDPOINTPROXY_COMM.xml."));
	//get current Installdir from MSI
	dwPathBuffer = sizeof(wstrInstallDir) / sizeof(WCHAR);
	if (ERROR_SUCCESS != MsiGetProperty(hInstall, TEXT("CustomActionData"), wstrInstallDir, &dwPathBuffer))
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Failed to get install directory from installer. Error Code: %d", dwErrorCode);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrMsg);//log only

		return ERROR_INSTALL_FAILURE;
	}

	if (wstrInstallDir[wcslen(wstrInstallDir) - 1] != L'\\')
	{
		wcscat_s(wstrInstallDir, L"\\");
	}

	wcscat_s(wstrInstallDir, L"Sfb Server Enforcer\\config\\");
	wcscat_s(wstrInstallDir, NLLYNCENDPOINTPROXY_COMM);

	//get file from temp
	DWORD dwRetVal = 0;
	dwRetVal = GetTempPath(MAX_PATH + 1, wstrSourceDir);
	if ((dwRetVal > MAX_PATH + 1) || (dwRetVal == 0))
	{
		MessageAndLogging(hInstall, TRUE, TEXT("Failed to get temp path in this computer."));
		return ERROR_INSTALL_FAILURE;
	}

	// verify temp path exists
	if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
	{
		wcscat_s(wstrSourceDir, MAX_PATH + 1, L"\\");
	}
	wcscat_s(wstrSourceDir, L"NxPCFile\\");
	wcscat_s(wstrSourceDir, NLLYNCENDPOINTPROXY_COMM);

	//prevent read only file already existed
	SetFileAttributes(wstrInstallDir, FILE_ATTRIBUTE_NORMAL);

	//Move file from Temp to Install Directory
	if (CopyFile(wstrSourceDir, wstrInstallDir, FALSE) == FALSE)
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Copy NLLYNCENDPOINTPROXY_COMM.xml file failed. Error Code: %d", dwErrorCode);

		//print log only
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrMsg);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrSourceDir);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrInstallDir);
		return ERROR_INSTALL_FAILURE;
	}

	//Clean up file
	DeleteFile(wstrSourceDir);
	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Copy file NLLYNCENDPOINTPROXY_COMM.xml success."));

	return ERROR_SUCCESS;
}

UINT __stdcall Find_NLLYNCENDPOINTPROXY_LOGCFG_File(MSIHANDLE hInstall)
{
	WCHAR wstrSourceDir[MAX_PATH + 1] = { 0 };
	WCHAR wstrTemp[MAX_PATH + 1] = { 0 };
	DWORD dwPathBuffer = 0;
	UINT uiRet = 0;
	WCHAR wstrMsg[128] = { 0 };
	DWORD dwErrorCode = 0;
	BOOL bFindFile = FALSE;

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Start checking file NLLYNCENDPOINTPROXY_LOGCFG"));

	//get temp path
	DWORD dwRetVal = 0;
	dwRetVal = GetTempPath(MAX_PATH + 1, wstrTemp);
	if ((dwRetVal > MAX_PATH + 1) || (dwRetVal == 0))
	{
		MessageAndLogging(hInstall, FALSE, TEXT("Failed to get TEMP path in this computer."));
		return ERROR_INSTALL_FAILURE;
	}

	// verify temp path exists
	if (wstrTemp[wcslen(wstrTemp) - 1] != L'\\')
	{
		wcscat_s(wstrTemp, L"\\");
	}
	wcscat_s(wstrTemp, L"NxPCFile\\");

	HANDLE hTempFile = INVALID_HANDLE_VALUE;
	hTempFile = CreateFile(wstrTemp,
		GENERIC_READ,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING | CREATE_NEW,
		FILE_FLAG_BACKUP_SEMANTICS,
		NULL);

	if (hTempFile == INVALID_HANDLE_VALUE)
	{
		if (!CreateDirectory(wstrTemp, NULL))
		{
			dwErrorCode = GetLastError();
			if (dwErrorCode != ERROR_ALREADY_EXISTS)
			{
				swprintf_s(wstrMsg, 128, L"Failed to create temp path. Error Code: %d", dwErrorCode);
				MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);
				return ERROR_INSTALL_FAILURE;
			}
		}
	}
	CloseHandle(hTempFile);

	// Move file from source to temp
	if (wstrTemp[wcslen(wstrTemp) - 1] != L'\\')
	{
		wcscat_s(wstrTemp, L"\\");
	}
	wcscat_s(wstrTemp, NLLYNCENDPOINTPROXY_LOGCFG);
	SetFileAttributes(wstrTemp, FILE_ATTRIBUTE_NORMAL);
	//Clean up temp file first
	DeleteFile(wstrTemp);

	if (bFindFile == FALSE)
	{
		// Check if file exists in current directory
		ZeroMemory(wstrSourceDir, sizeof(wstrSourceDir));
		uiRet = 0;
		dwPathBuffer = sizeof(wstrSourceDir) / sizeof(WCHAR);

		uiRet = MsiGetProperty(hInstall, TEXT("CURRENTDIRECTORY"), wstrSourceDir, &dwPathBuffer);
		if (uiRet != ERROR_SUCCESS)
		{
			dwErrorCode = GetLastError();
			swprintf_s(wstrMsg, 128, L"Failed to get current directory from installer. Error Code: %d", dwErrorCode);
			MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);

			return ERROR_INSTALL_FAILURE;
		}

		//Check if file exist
		if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
		{
			wcscat_s(wstrSourceDir, L"\\");
		}
		wcscat_s(wstrSourceDir, NLLYNCENDPOINTPROXY_LOGCFG);

		if (GetFileAttributes(wstrSourceDir) == INVALID_FILE_ATTRIBUTES && GetLastError() == ERROR_FILE_NOT_FOUND)
		{
			MessageAndLogging(hInstall, TRUE, TEXT("The installer could not find the NLLYNCENDPOINTPROXY_LOGCFG.xml file in the MSI folder."));
		}
		else
		{
			MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Use NLLYNCENDPOINTPROXY_LOGCFG.xml from MSI folder"));
			bFindFile = TRUE;
		}
	}

	if (bFindFile == FALSE)
	{
		// Check File in installer folder c:\program files\nextlabs
		ZeroMemory(wstrSourceDir, sizeof(wstrSourceDir));
		dwPathBuffer = sizeof(wstrSourceDir) / sizeof(WCHAR);
		uiRet = 0;
		uiRet = MsiGetProperty(hInstall, TEXT("INSTALLDIR"), wstrSourceDir, &dwPathBuffer);
		if (uiRet == ERROR_SUCCESS)
		{
			if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
			{
				wcscat_s(wstrSourceDir, L"\\");
			}
			wcscat_s(wstrSourceDir, L"Sfb Server Enforcer\\config\\");
			wcscat_s(wstrSourceDir, NLLYNCENDPOINTPROXY_LOGCFG);
			MessageAndLogging(hInstall, TRUE, wstrSourceDir);

			// exist in Policy Controller folder
			if ((GetFileAttributes(wstrSourceDir) == INVALID_FILE_ATTRIBUTES))
			{
				MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: can't find NLLYNCENDPOINTPROXY_LOGCFG.xml from Program Files\\nextlabs"));
			}
			else
			{
				MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Use NLLYNCENDPOINTPROXY_LOGCFG.xml from existing Program Files\\nextlabs"));
				bFindFile = TRUE;
			}
		}
	}

	if (bFindFile == FALSE)
	{
		MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: can't find NLLYNCENDPOINTPROXY_LOGCFG.xml both MSI folder and existing nextlabs folder. Let's use default one."));
		return ERROR_INSTALL_FAILURE;
	}

	// start copying to temp
	if (CopyFile(wstrSourceDir, wstrTemp, FALSE) == FALSE) //Failed
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Failed to copy file to temp path. Error Code: %d", dwErrorCode);
		MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);
		return ERROR_INSTALL_FAILURE;
	}

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Checking file NLLYNCENDPOINTPROXY_LOGCFG.xml done.  Status: Good."));

	return ERROR_SUCCESS;
}

UINT __stdcall Copy_NLLYNCENDPOINTPROXY_LOGCFG_File(MSIHANDLE hInstall) //run in defered execution
{
	WCHAR wstrSourceDir[MAX_PATH + 1] = { 0 };
	WCHAR wstrInstallDir[MAX_PATH + 1] = { 0 };
	DWORD dwPathBuffer = 0;
	WCHAR wstrMsg[128] = { 0 };
	DWORD dwErrorCode = 0;

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Start copy NLLYNCENDPOINTPROXY_LOGCFG.xml."));
	//get current Installdir from MSI
	dwPathBuffer = sizeof(wstrInstallDir) / sizeof(WCHAR);
	if (ERROR_SUCCESS != MsiGetProperty(hInstall, TEXT("CustomActionData"), wstrInstallDir, &dwPathBuffer))
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Failed to get install directory from installer. Error Code: %d", dwErrorCode);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrMsg);//log only

		return ERROR_INSTALL_FAILURE;
	}

	if (wstrInstallDir[wcslen(wstrInstallDir) - 1] != L'\\')
	{
		wcscat_s(wstrInstallDir, L"\\");
	}

	wstring wstrInstPC = wstrInstallDir;
	wstrInstPC += L"Policy Controller\\";

	wcscat_s(wstrInstallDir, L"Sfb Server Enforcer\\config\\");
	wcscat_s(wstrInstallDir, NLLYNCENDPOINTPROXY_LOGCFG);

	//get file from temp
	DWORD dwRetVal = 0;
	dwRetVal = GetTempPath(MAX_PATH + 1, wstrSourceDir);
	if ((dwRetVal > MAX_PATH + 1) || (dwRetVal == 0))
	{
		MessageAndLogging(hInstall, TRUE, TEXT("Failed to get temp path in this computer."));
		return ERROR_INSTALL_FAILURE;
	}

	// verify temp path exists
	if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
	{
		wcscat_s(wstrSourceDir, MAX_PATH + 1, L"\\");
	}
	wcscat_s(wstrSourceDir, L"NxPCFile\\");
	wcscat_s(wstrSourceDir, NLLYNCENDPOINTPROXY_LOGCFG);

	//prevent read only file already existed
	SetFileAttributes(wstrInstallDir, FILE_ATTRIBUTE_NORMAL);

	//Move file from Temp to Install Directory
	if (CopyFile(wstrSourceDir, wstrInstallDir, FALSE) == FALSE)
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Copy NLLYNCENDPOINTPROXY_LOGCFG.xml file failed. Error Code: %d", dwErrorCode);

		//print log only
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrMsg);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrSourceDir);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrInstallDir);
		return ERROR_INSTALL_FAILURE;
	}

	//Clean up file
	DeleteFile(wstrSourceDir);
	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Copy file NLLYNCENDPOINTPROXY_LOGCFG.xml success."));

	return ERROR_SUCCESS;
}

UINT __stdcall Find_SFBSERVERENFORCER_AM_File(MSIHANDLE hInstall)
{
	WCHAR wstrSourceDir[MAX_PATH + 1] = { 0 };
	WCHAR wstrTemp[MAX_PATH + 1] = { 0 };
	DWORD dwPathBuffer = 0;
	UINT uiRet = 0;
	WCHAR wstrMsg[128] = { 0 };
	DWORD dwErrorCode = 0;
	BOOL bFindFile = FALSE;

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Start checking file SFBSERVERENFORCER_AM"));

	//get temp path
	DWORD dwRetVal = 0;
	dwRetVal = GetTempPath(MAX_PATH + 1, wstrTemp);
	if ((dwRetVal > MAX_PATH + 1) || (dwRetVal == 0))
	{
		MessageAndLogging(hInstall, FALSE, TEXT("Failed to get TEMP path in this computer."));
		return ERROR_INSTALL_FAILURE;
	}

	// verify temp path exists
	if (wstrTemp[wcslen(wstrTemp) - 1] != L'\\')
	{
		wcscat_s(wstrTemp, L"\\");
	}
	wcscat_s(wstrTemp, L"NxPCFile\\");

	HANDLE hTempFile = INVALID_HANDLE_VALUE;
	hTempFile = CreateFile(wstrTemp,
		GENERIC_READ,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING | CREATE_NEW,
		FILE_FLAG_BACKUP_SEMANTICS,
		NULL);

	if (hTempFile == INVALID_HANDLE_VALUE)
	{
		if (!CreateDirectory(wstrTemp, NULL))
		{
			dwErrorCode = GetLastError();
			if (dwErrorCode != ERROR_ALREADY_EXISTS)
			{
				swprintf_s(wstrMsg, 128, L"Failed to create temp path. Error Code: %d", dwErrorCode);
				MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);
				return ERROR_INSTALL_FAILURE;
			}
		}
	}
	CloseHandle(hTempFile);

	// Move file from source to temp
	if (wstrTemp[wcslen(wstrTemp) - 1] != L'\\')
	{
		wcscat_s(wstrTemp, L"\\");
	}
	wcscat_s(wstrTemp, SFBSERVERENFORCER_AM);
	SetFileAttributes(wstrTemp, FILE_ATTRIBUTE_NORMAL);
	//Clean up temp file first
	DeleteFile(wstrTemp);

	if (bFindFile == FALSE)
	{
		// Check if file exists in current directory
		ZeroMemory(wstrSourceDir, sizeof(wstrSourceDir));
		uiRet = 0;
		dwPathBuffer = sizeof(wstrSourceDir) / sizeof(WCHAR);

		uiRet = MsiGetProperty(hInstall, TEXT("CURRENTDIRECTORY"), wstrSourceDir, &dwPathBuffer);
		if (uiRet != ERROR_SUCCESS)
		{
			dwErrorCode = GetLastError();
			swprintf_s(wstrMsg, 128, L"Failed to get current directory from installer. Error Code: %d", dwErrorCode);
			MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);

			return ERROR_INSTALL_FAILURE;
		}

		//Check if file exist
		if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
		{
			wcscat_s(wstrSourceDir, L"\\");
		}
		wcscat_s(wstrSourceDir, SFBSERVERENFORCER_AM);

		if (GetFileAttributes(wstrSourceDir) == INVALID_FILE_ATTRIBUTES && GetLastError() == ERROR_FILE_NOT_FOUND)
		{
			MessageAndLogging(hInstall, TRUE, TEXT("The installer could not find the SFBSERVERENFORCER_AM.xml file in the MSI folder."));
		}
		else
		{
			MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Use SFBSERVERENFORCER_AM.xml from MSI folder"));
			bFindFile = TRUE;
		}
	}

	if (bFindFile == FALSE)
	{
		// Check File in installer folder c:\program files\nextlabs
		ZeroMemory(wstrSourceDir, sizeof(wstrSourceDir));
		dwPathBuffer = sizeof(wstrSourceDir) / sizeof(WCHAR);
		uiRet = 0;
		uiRet = MsiGetProperty(hInstall, TEXT("INSTALLDIR"), wstrSourceDir, &dwPathBuffer);
		if (uiRet == ERROR_SUCCESS)
		{
			if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
			{
				wcscat_s(wstrSourceDir, L"\\");
			}
			wcscat_s(wstrSourceDir, L"Sfb Server Enforcer\\config\\");
			wcscat_s(wstrSourceDir, SFBSERVERENFORCER_AM);
			MessageAndLogging(hInstall, TRUE, wstrSourceDir);

			// exist in Policy Controller folder
			if ((GetFileAttributes(wstrSourceDir) == INVALID_FILE_ATTRIBUTES))
			{
				MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: can't find SFBSERVERENFORCER_AM.xml from Program Files\\nextlabs"));
			}
			else
			{
				MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Use SIPCOMPONENT_LOGCFG.xml from existing Program Files\\nextlabs"));
				bFindFile = TRUE;
			}
		}
	}

	if (bFindFile == FALSE)
	{
		MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: can't find SFBSERVERENFORCER_AM.xml both MSI folder and existing nextlabs folder. Let's use default one."));
		return ERROR_INSTALL_FAILURE;
	}

	// start copying to temp
	if (CopyFile(wstrSourceDir, wstrTemp, FALSE) == FALSE) //Failed
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Failed to copy file to temp path. Error Code: %d", dwErrorCode);
		MessageAndLogging(hInstall, FALSE, (LPCWSTR)wstrMsg);
		return ERROR_INSTALL_FAILURE;
	}

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Checking file SFBSERVERENFORCER_AM.xml done.  Status: Good."));

	return ERROR_SUCCESS;
}

UINT __stdcall Copy_SFBSERVERENFORCER_AM_File(MSIHANDLE hInstall) //run in defered execution
{
	WCHAR wstrSourceDir[MAX_PATH + 1] = { 0 };
	WCHAR wstrInstallDir[MAX_PATH + 1] = { 0 };
	DWORD dwPathBuffer = 0;
	WCHAR wstrMsg[128] = { 0 };
	DWORD dwErrorCode = 0;

	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Start copy SFBSERVERENFORCER_AM.xml."));
	//get current Installdir from MSI
	dwPathBuffer = sizeof(wstrInstallDir) / sizeof(WCHAR);
	if (ERROR_SUCCESS != MsiGetProperty(hInstall, TEXT("CustomActionData"), wstrInstallDir, &dwPathBuffer))
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Failed to get install directory from installer. Error Code: %d", dwErrorCode);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrMsg);//log only

		return ERROR_INSTALL_FAILURE;
	}

	if (wstrInstallDir[wcslen(wstrInstallDir) - 1] != L'\\')
	{
		wcscat_s(wstrInstallDir, L"\\");
	}

	wstring wstrInstPC = wstrInstallDir;
	wstrInstPC += L"Policy Controller\\";

	wcscat_s(wstrInstallDir, L"Sfb Server Enforcer\\config\\");
	wcscat_s(wstrInstallDir, SFBSERVERENFORCER_AM);

	//get file from temp
	DWORD dwRetVal = 0;
	dwRetVal = GetTempPath(MAX_PATH + 1, wstrSourceDir);
	if ((dwRetVal > MAX_PATH + 1) || (dwRetVal == 0))
	{
		MessageAndLogging(hInstall, TRUE, TEXT("Failed to get temp path in this computer."));
		return ERROR_INSTALL_FAILURE;
	}

	// verify temp path exists
	if (wstrSourceDir[wcslen(wstrSourceDir) - 1] != L'\\')
	{
		wcscat_s(wstrSourceDir, MAX_PATH + 1, L"\\");
	}
	wcscat_s(wstrSourceDir, L"NxPCFile\\");
	wcscat_s(wstrSourceDir, SFBSERVERENFORCER_AM);

	//prevent read only file already existed
	SetFileAttributes(wstrInstallDir, FILE_ATTRIBUTE_NORMAL);

	//Move file from Temp to Install Directory
	if (CopyFile(wstrSourceDir, wstrInstallDir, FALSE) == FALSE)
	{
		dwErrorCode = GetLastError();
		swprintf_s(wstrMsg, 128, L"Copy SIPCOMPONENT_LOGCFG.xml file failed. Error Code: %d", dwErrorCode);

		//print log only
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrMsg);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrSourceDir);
		MessageAndLogging(hInstall, TRUE, (LPCWSTR)wstrInstallDir);
		return ERROR_INSTALL_FAILURE;
	}

	//Clean up file
	DeleteFile(wstrSourceDir);
	MessageAndLogging(hInstall, TRUE, TEXT("******** NXPCLOG: Copy file SFBSERVERENFORCER_AM.xml success."));

	return ERROR_SUCCESS;
}

//CAFindCommFile commprofile.xml, call in immediate excution
UINT __stdcall FindConfigFile(MSIHANDLE hInstall )
{
	Find_NLLYNCENDPOINTPROXY_COMM_File(hInstall);
	Find_NLLYNCENDPOINTPROXY_LOGCFG_File(hInstall);
	Find_SFBSERVERENFORCER_AM_File(hInstall);

    return ERROR_SUCCESS;
}

//CACopyCommfile, call in defered execution in system context
UINT __stdcall CopyConfigFile(MSIHANDLE hInstall ) //run in defered execution
{
	Copy_NLLYNCENDPOINTPROXY_COMM_File(hInstall);
	Copy_NLLYNCENDPOINTPROXY_LOGCFG_File(hInstall);
	Copy_SFBSERVERENFORCER_AM_File(hInstall);
	
	return ERROR_SUCCESS;
}



