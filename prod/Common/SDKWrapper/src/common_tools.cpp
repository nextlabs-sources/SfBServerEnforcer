/**
*  @file add simple describe here.
*
*  @author NextLabs::kim
*/


/** precompile header and resource header files */
#include "StdAfx.h"
/** current class declare header files */
#include "common_tools.h"
/** C system header files */

/** C++ system header files */
#include <string>
/** Platform header files */
#include <Sddl.h>
#include <sal.h>
/** Third part library header files */
/** boost */
#pragma warning( push )
#pragma warning( disable: 4996 4512 4244 6011 )
#include <boost/algorithm/string.hpp>
#pragma warning( pop )
/** Other project header files */
#include "../platform/nlconfig.hpp"
/** Current project header files */
#include "nlofficerep_only_debug.h"


using namespace std;

void GetFQDN(_In_z_ LPCWSTR hostname, _Out_z_cap_(nSize) LPWSTR fqdn, _In_ int nSize)
{
    char szHostName[1001] = {0};
    WideCharToMultiByte(CP_ACP, WC_COMPOSITECHECK, hostname, (int)wcslen(hostname), szHostName, 1000, NULL, NULL);

    hostent* hostinfo;
    hostinfo = gethostbyname(szHostName);
    if(hostinfo && hostinfo->h_name)
    {
        MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, hostinfo->h_name, (int)strlen(hostinfo->h_name), fqdn, nSize);
    }
    else
    {
        wcsncpy_s(fqdn, nSize, hostname, _TRUNCATE);
    }
}

void GetUserInfo(_Out_z_cap_(nSize) LPWSTR wzSid, _In_ int nSize, _Inout_z_cap_(UserNameLen) LPWSTR UserName, _In_ int UserNameLen)
{
    HANDLE hTokenHandle = NULL;

    if(!OpenThreadToken(GetCurrentThread(), TOKEN_QUERY, TRUE, &hTokenHandle))
    {
        if(GetLastError() == ERROR_NO_TOKEN)
        {
            if(!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hTokenHandle ))
            {
                goto _exit;
            }
        }
        else
        {
            goto _exit;
        }
    }

    // Get SID
    UCHAR   InfoBuffer[512] = { 0 };
    DWORD   cbInfoBuffer = 512;
    LPTSTR  StringSid = NULL;
    WCHAR   uname[64] = {0}; DWORD unamelen = 63;
    WCHAR   dname[64] = {0}; DWORD dnamelen = 63;
    WCHAR   fqdnname[MAX_PATH+1] = { 0 };
    SID_NAME_USE snu = SidTypeUnknown;

    if(!GetTokenInformation(hTokenHandle, TokenUser, InfoBuffer, cbInfoBuffer, &cbInfoBuffer))
    {
        goto _exit;
    }
    if(ConvertSidToStringSid(((PTOKEN_USER)InfoBuffer)->User.Sid, &StringSid))
    {
        wcsncpy_s(wzSid, nSize, StringSid, _TRUNCATE);
        if(StringSid)
        {
            LocalFree(StringSid);
        }
    }
    if(LookupAccountSid(NULL, ((PTOKEN_USER)InfoBuffer)->User.Sid, uname, &unamelen, dname, &dnamelen, &snu))
    {
        char  szHostname[MAX_PATH+1]; memset(szHostname, 0, sizeof(szHostname));
        WCHAR wzHostname[MAX_PATH+1]; memset(wzHostname, 0, sizeof(wzHostname));
        gethostname(szHostname, MAX_PATH);
        if(0 != szHostname[0])
        {
            MultiByteToWideChar(CP_ACP, MB_PRECOMPOSED, szHostname, -1, wzHostname, MAX_PATH);

            GetFQDN(wzHostname, fqdnname, MAX_PATH);

            wcsncat_s(UserName, UserNameLen, fqdnname, _TRUNCATE);
            wcsncat_s(UserName, UserNameLen, L"\\", _TRUNCATE); 
            wcsncat_s(UserName, UserNameLen, uname, _TRUNCATE);
        }
    }

_exit:
    if(NULL!=hTokenHandle)
    { 
        CloseHandle(hTokenHandle);
        hTokenHandle=NULL;
    }
}

wstring GetCommonComponentsDir()
{
    wchar_t wszDir[MAX_PATH + 1] = { 0 };
    if (NLConfig::ReadKey(L"SOFTWARE\\NextLabs\\CommonLibraries\\InstallDir", wszDir, MAX_PATH))
    {
#ifdef _M_IX86
        wcscat_s(wszDir, MAX_PATH, L"bin32\\");
#else
        wcscat_s(wszDir, MAX_PATH, L"bin64\\");
#endif
    }

    return wszDir;
}

bool NLIsHttpPath(_In_ const wstring& wstrFilePath)
{
    return boost::algorithm::istarts_with(wstrFilePath, L"http");
}

void ConvertURLCharacterW(_Inout_ wstring& strUrl)
{
	/*
	*@Add for change '%5c%5b'->'\['->'[',  '%5c%5d'->'\]'->']' for bug 9339
	*/
	boost::replace_all(strUrl,L"%5c%5b",L"[");//'%5c%5b' -> '['
	boost::replace_all(strUrl,L"%5c%5d",L"]");	//'%5c%5d' -> ']'

	boost::replace_all(strUrl,L"%24",L"$");
	boost::replace_all(strUrl,L"%5e",L"^");
	boost::replace_all(strUrl,L"%26",L"&");
	boost::replace_all(strUrl,L"%5b",L"[");
	boost::replace_all(strUrl,L"%5d",L"]");
	boost::replace_all(strUrl,L"%20",L" ");	//'%20' -> ' '
	boost::replace_all(strUrl,L"%2e",L".");	// '%2e  -> '.'
	boost::replace_all(strUrl,L"%2f",L"/");	// '%2f' -> '/'
	boost::replace_all(strUrl,L"%5f",L"_");	// '%5f' -> '_'
	boost::replace_all(strUrl,L"+",L" ");		// '+'   -> ' '
	boost::replace_all(strUrl,L"%2d",L"-");	//'%2d' -> '-'
	boost::replace_all(strUrl,L"%3a",L":");	//'%2d' -> '-'
	boost::replace_all(strUrl,L"%28",L"(");	//'%28'-> '('
	boost::replace_all(strUrl,L"%29",L")");	//'%29'-> ')'
	boost::replace_all(strUrl,L"%2520",L" ");//'%2520'->' '
}

vector<wstring> SplitString(_In_ const wstring& kwstrSource, _In_ const wstring& kwstrSeparator, _In_ const bool kbRemoveEmptyItem)
{
    vector<wstring> vecOutSnippets;
    if (kwstrSeparator.empty())
    {
        vecOutSnippets.push_back(kwstrSource);
    }
    else
    {
        
        const size_t kstSourceLen = kwstrSource.length();
        const size_t kstSepLen = kwstrSeparator.length();

        for (int nPos = 0; nPos < kstSourceLen; )
        {
            int nSepPos = kwstrSource.find(kwstrSeparator, nPos);
            if (0 > nSepPos)
            {
                // The last one, save and break
                vecOutSnippets.push_back(kwstrSource.substr(nPos, kstSourceLen - nPos));
                break;
            }
            else
            {
                if (nPos == nSepPos)
                {
                    // Start with separator, jump to the next.
                    if (!kbRemoveEmptyItem)
                    {
                        vecOutSnippets.push_back(L"");
                    }
                }
                else
                {
                    // Find one, save and continue to find the next one, nSepPos always >= nPos
                    vecOutSnippets.push_back(kwstrSource.substr(nPos, nSepPos - nPos));
                }
                nPos = nSepPos + kstSepLen;
            }
        }
    }
    return vecOutSnippets;
}

bool IsSameString(_In_ const wstring& kwstrFirst, _In_ const wstring& kwstrSecond, _In_ const bool kbIgnoreCase)
{
    int nCmpResult = 0;
    if (kbIgnoreCase)
    {
        nCmpResult = wcsicmp(kwstrFirst.c_str(), kwstrSecond.c_str());
    }
    else
    {
        nCmpResult = wcscmp(kwstrFirst.c_str(), kwstrSecond.c_str());
    }
    return (0 == nCmpResult);
}

wstring ConnectVectorToString(_In_ const vector<wstring>& kvecString, _In_ const wstring& kstrSeparator)
{
    wstring wstrOut = L"";
    for (vector<wstring>::const_iterator kitrItem = kvecString.begin(); kitrItem != kvecString.end(); ++kitrItem)
    {
        wstrOut += kitrItem->c_str() + kstrSeparator;
    }
    return wstrOut;
}

bool IsFirstVecContainsInSencondVec(_In_ const vector<wstring>& vecFirstTagValue, _In_ const vector<wstring>& vecSecondTagValue, _In_ const bool kbIgnoreCase)
{
    bool bContains = true; /**< if the first vector is empty, return true, contains in the second one and do not care the second one is empty or not */

    bool bFindInSendVec = false;
    for (vector<wstring>::const_iterator kitrFirst = vecFirstTagValue.begin(); kitrFirst != vecFirstTagValue.end(); ++kitrFirst)
    {
        bFindInSendVec = IsContainsInVec((*kitrFirst), vecSecondTagValue, kbIgnoreCase);
        if (!bFindInSendVec)
        {
            bContains = false;
            break;
        }
    }
    return bContains;
}

bool IsContainsInVec(_In_ const wstring& kstrIn, _In_ const vector<wstring>& vecIn, _In_ const bool kbIgnoreCase)
{
    bool bFind = false;
    for (vector<wstring>::const_iterator kitrIn = vecIn.begin(); kitrIn != vecIn.end(); ++kitrIn)
    {
        if (IsSameString(kstrIn, (*kitrIn), kbIgnoreCase))
        {
            bFind = true;
            break;
        }
        else
        {
            continue;
        }
    }
    return bFind;
}