#pragma once

#include <string>
#include <map>
#include <vector>

using namespace std;

void GetFQDN(_In_z_ LPCWSTR hostname, _Out_z_cap_(nSize) LPWSTR fqdn, _In_ int nSize);

void GetUserInfo(_Out_z_cap_(nSize) LPWSTR wzSid, _In_ int nSize, _Inout_z_cap_(UserNameLen) LPWSTR UserName, _In_ int UserNameLen);

wstring GetCommonComponentsDir();

bool NLIsHttpPath( _In_ const wstring& wstrFilePath );

void ConvertURLCharacterW(_Inout_ wstring& strUrl);

vector<wstring> SplitString(_In_ const wstring& kwstrSource, _In_ const wstring& kwstrSeparator, _In_ const bool kbRemoveEmptyItem);

bool IsSameString(_In_ const wstring& kwstrFirst, _In_ const wstring& kwstrSecond, _In_ const bool kbIgnoreCase);

wstring ConnectVectorToString(_In_ const vector<wstring>& kvecString, _In_ const wstring& kstrSeparator);

bool IsFirstVecContainsInSencondVec(_In_ const vector<wstring>& vecFirstTagValue, _In_ const vector<wstring>& vecSecondTagValue, _In_ const bool kbIgnoreCase);

bool IsContainsInVec(_In_ const wstring& kstrIn, _In_ const vector<wstring>& vecIn, _In_ const bool kbIgnoreCase);

template<class TKey, class TValue>
bool GetValueFromMapByKey(_In_ const std::map<TKey, TValue>& kmapObject, _In_ const TKey tkeyInput, _Inout_ TValue& tvalueOutput)
{
    bool bRet = false;
#if 0
    try
    {
        // C++11 support .at method. It will throw out_of_range exception if the key is not exist
        tvalueOutput = kmapObject.at(tkeyInput);
        bRet = true;
    }
    catch (const std::out_of_range& /*oor*/)    // Note: oor.what() return value is char* not wchar_t*
    {
        NLPRINT_DEBUGVIEWLOG(L"Cannot find source file path in cache with out_of_range exception.\n");
    }
#else
    std::map<TKey, TValue>::const_iterator kItr = kmapObject.find(tkeyInput);
    std::map<TKey, TValue>::const_iterator kItrEnd = kmapObject.end();
    if (kItr != kItrEnd)
    {
        tvalueOutput = kItr->second;
        bRet = true;
    }
#endif
    return bRet;
}