#include "StdAfx.h"
#include "../include/NLTagManager.h"

// Windows
#include <shlwapi.h>

// C++
#include <string>
#include <vector>

// Boost
#pragma warning( push )
#pragma warning( disable: 4996 4512 4244 6011 )
#include <boost/algorithm/string.hpp>
#pragma warning( pop )

#include "nlofficerep_only_debug.h"
#include "../include/NLTag.h"

#include "SDKWrapper_i.h"

#ifdef _WIN64
#define NLOFFICERESATTRLIB_MODULE_NAME	L"resattrlib.dll"
#else
#define NLOFFICERESATTRLIB_MODULE_NAME	L"resattrlib32.dll"
#endif

#ifdef _WIN64
#define NLOFFICERESATTRMGR_MODULE_NAME	L"resattrmgr.dll"
#else
#define NLOFFICERESATTRMGR_MODULE_NAME	L"resattrmgr32.dll"
#endif

#define print_string(s)  s?s:" "
#define print_long_string(s) s?s:L" "
#define print_non_string(s) s?*s:0 


////////////////////////////CELog2//////////////////////////////////////////////
// for CELog2 we should define: CELOG_CUR_FILE
#define CELOG_CUR_FILE static_cast<celog_filepathint_t>(CELOG_FILEPATH_OFFICEPEP_MIN + EMNLFILEINTEGER_NLTAG)
//////////////////////////////////////////////////////////////////////////

using namespace std;

// SE File inherent tags
static const wchar_t* g_pwchSeTagNameEncrypted = L"NXL_encrypted";
static const wchar_t* g_pwchSeTagNameKeyRingName = L"NXL_keyRingName";
static const wchar_t* g_pwchSeTagNameRequires = L"NXL_requiresLocalEncryption";
static const wchar_t* g_pwchSeTagNameWrapped = L"NXL_wrapped";

CNLTagManager::CNLTagManager(void) : m_hLib(NULL), m_hMgr(NULL), m_bIsGetFunSuc(false)
{
    if (GetTagDllHandle())
    {
        SetGetFunSucFlag(GetTagFunAddr());
    }
}

CNLTagManager::~CNLTagManager(void)
{
    if (NULL != m_hLib)
    {
        FreeLibrary(m_hLib);
        m_hLib = NULL;
    }
    if (NULL != m_hMgr)
    {
        FreeLibrary(m_hMgr);
        m_hMgr = NULL;
    }
}

HRESULT CNLTagManager::FinalConstruct()
{
    return S_OK;
}

void CNLTagManager::FinalRelease()
{
}

//////////////////////////////Interface////////////////////////////////////////////
STDMETHODIMP CNLTagManager::ReadTag(_In_ BSTR bstrFilePath, _In_ int nOperationType, _Inout_ INLTag** pINLTag)
{NLONLY_DEBUG
    if ((NULL == bstrFilePath) || (!CNLTag::IsValidTagOperation(nOperationType)) || (NULL == pINLTag) || ((NULL == *pINLTag)))
    {
        NLPRINT_DEBUGVIEWLOG(L"Parameter error\n");
        return E_INVALIDARG;
    }

    vector<pair<wstring, wstring>> vecTagPair;
    bool bReadTag = ReadTag(bstrFilePath, vecTagPair);
    NLPRINT_TAGPAIRLOG(vecTagPair, L"Read tag", (bReadTag?L"true":L"false"));
    if (bReadTag)
    {
        CComBSTR comBstrTagName(L"");
        CComBSTR comBstrTagValue(L"");
        for (vector<pair<wstring, wstring>>::const_iterator kitr = vecTagPair.cbegin(); kitr != vecTagPair.cend(); ++kitr)
        {
            comBstrTagName = kitr->first.c_str();
            comBstrTagValue = kitr->second.c_str();
            (*pINLTag)->SetTag(comBstrTagName, comBstrTagValue, nOperationType);
        }
    }
    return bReadTag ? S_OK : E_FAIL;
}

STDMETHODIMP CNLTagManager::WriteTag(_In_ BSTR bstrFilePath, _In_ INLTag* pINLTag)
{NLONLY_DEBUG
    if ((NULL == bstrFilePath) || (NULL == pINLTag))
    {
        return E_INVALIDARG;
    }

    vector<pair<wstring, wstring>> vecTagPair = GetVecTagsFromINLTag(pINLTag);

    bool bRet = AddTag(bstrFilePath, vecTagPair);
    return bRet ? S_OK : E_FAIL;
}

STDMETHODIMP CNLTagManager::RemoveTag(_In_ BSTR bstrFilePath, _In_ INLTag* pINLTag)
{NLONLY_DEBUG
    if ((NULL == bstrFilePath) || (NULL == pINLTag))
    {
        return E_INVALIDARG;
    }

    vector<pair<wstring, wstring>> vecTagPair = GetVecTagsFromINLTag(pINLTag);

    bool bRet = RemoveTag(bstrFilePath, vecTagPair);
    return bRet ? S_OK : E_FAIL;
}
STDMETHODIMP CNLTagManager::RemoveAllTags(_In_ BSTR bstrFilePath)
{NLONLY_DEBUG
    if ((NULL == bstrFilePath))
    {
        return E_INVALIDARG;
    }

    bool bRet = RemoveAllTag(bstrFilePath);
    return bRet ? S_OK : E_FAIL;
}
//////////////////////////////End interface////////////////////////////////////////////

/////////////////////////////Basic tools/////////////////////////////////////////////
bool CNLTagManager::ReadTag(_In_ const wstring& kwstrInFilePath, _Out_ vector<pair<wstring, wstring>>& vecTagPair) const
{
    // 1. check tag library, if all functions load success.
    if (!IsGetFunSuc())
    {
        NLPRINT_DEBUGVIEWLOG(L"Get function address failed\n");
        return false;
    }

    // 2. get file path: HTTP path we should convert it to UNC path and need check the path if it is exist.
    wstring wstrFilePath = NLGetEffectFilePath(kwstrInFilePath);
    if (wstrFilePath.empty())
    {
        NLPRINT_DEBUGVIEWLOG(L"Convert path:[%s] as en effective file path failed\n", kwstrInFilePath.c_str());
        return false;
    }

    // 3. alloc resource
    ResourceAttributeManager* pMgr = NULL;
    ResourceAttributes* pAttrs = NULL;
    if (!NLAlloceResource(pMgr, pAttrs))
    {
        NLPRINT_DEBUGVIEWLOG(L"Alloc resource failed\n");
        return false;
    }

    bool bRet = false;

    // If return value is 1 means read tags success, otherwise failed. But there are some bugs in tag library, return 1 but not always means success in some special case.
    int nReadRet = m_lfReadResourceAttributesW(pMgr, wstrFilePath.c_str(), pAttrs);
    if (1 == nReadRet)
    {
        bRet = true;
        NLGetAttributeToVetor(pAttrs, vecTagPair);
    }

    NLFreeResource(pMgr, pAttrs);
    return bRet;
}

bool CNLTagManager::AddTag(_In_ const wstring& kwstrInFilePath, _In_ const vector<pair<wstring, wstring>>& kvecTagPair) const
{
    // 1. check tag library
    if (!IsGetFunSuc())
    {
        NLPRINT_DEBUGVIEWLOG(L"Get function address failed\n");
        return false;
    }

    // 2. check tag pair
    if (kvecTagPair.empty())
    {
        NLPRINT_DEBUGVIEWLOG(L"Tags is empty\n");
        return true;
    }

    // add logs
    NLPRINT_TAGPAIRLOG(kvecTagPair, L"AddTag, TagInfo:", L"End");

    // 2. get file path: HTTP path we should convert it to UNC path and need check the path if it is exist.
    wstring wstrFilePath = NLGetEffectFilePath(kwstrInFilePath);
    if (wstrFilePath.empty())
    {
        NLPRINT_DEBUGVIEWLOG(L"Convert path:[%s] as en effective file path failed\n", kwstrInFilePath.c_str());
        return false;
    }

    // 3. alloc resource attributes
    ResourceAttributeManager* pMgr = NULL;
    ResourceAttributes* pAttrs = NULL;

    if (!NLAlloceResource(pMgr, pAttrs))
    {
        NLPRINT_DEBUGVIEWLOG(L"Alloc resource failed\n");
        return false;
    }

    // 4. add tags into attribute, SE file need ignore its inherent tags, for bug:23431, no SE file now
    NLAddAttributeFromVector(pAttrs, kvecTagPair);

    // 5. write tags
    int nWriteRet = m_lfWriteResourceAttributesW(pMgr, wstrFilePath.c_str(), pAttrs);
    NLPRINT_DEBUGVIEWLOG(L"the WriteResourceAttributesW return value is:[%d] \n", nWriteRet);

    NLFreeResource(pMgr, pAttrs);

#pragma chMSG( "Maybe here we don't need to check this return value" )
    return (1 == nWriteRet ? true : false);
}

bool CNLTagManager::RemoveTag(_In_ const wstring& kwstrInFilePath, _In_ const vector<pair<wstring, wstring>>& kvecTagPair) const
{NLONLY_DEBUG
    // 1. check tag library
    if (!IsGetFunSuc())
    {
        NLPRINT_DEBUGVIEWLOG(L"Get function address failed\n");
        return false;
    }

    // 2. check parameter
    if (kvecTagPair.empty())
    {
        NLPRINT_DEBUGVIEWLOG(L"Tags is empty\n");
        return true;
    }

    // 3. get file path: HTTP path we should convert it to UNC path and need check the path if it is exist.
    wstring wstrFilePath = NLGetEffectFilePath(kwstrInFilePath);
    if (wstrFilePath.empty())
    {
        NLPRINT_DEBUGVIEWLOG(L"Convert path:[%s] as en effective file path failed\n", kwstrInFilePath.c_str());
        return false;
    }

    // 3. alloc
    ResourceAttributeManager* pMgr = NULL;
    ResourceAttributes* pAttrs = NULL;

    if (!NLAlloceResource(pMgr, pAttrs))
    {
        NLPRINT_DEBUGVIEWLOG(L"Alloc resource failed\n");
        return false;
    }

    NLAddAttributeFromVector(pAttrs, kvecTagPair);  // this function if we can make sure the pAttr is not NULL, if will return true
    int nRet = m_lfRemoveResourceAttributesW(pMgr, wstrFilePath.c_str(), pAttrs);

    NLFreeResource(pMgr, pAttrs);
    return (1 == nRet ? true : false);
}

bool CNLTagManager::RemoveAllTag(_In_ const wstring& kwstrInFilePath) const
{
    // 1. check tag library
    if (!IsGetFunSuc())
    {
        NLPRINT_DEBUGVIEWLOG(L"Get function address failed\n");
        return false;
    }

    // 2. get file path: HTTP path we should convert it to UNC path and need check the path if it is exist.
    wstring wstrFilePath = NLGetEffectFilePath(kwstrInFilePath);
    if (wstrFilePath.empty())
    {
        NLPRINT_DEBUGVIEWLOG(L"Convert path:[%s] as en effective file path failed\n", kwstrInFilePath.c_str());
        return false;
    }

    // 3. alloc
    ResourceAttributeManager* pMgr = NULL;
    ResourceAttributes* pAttrs = NULL;

    if (!NLAlloceResource(pMgr, pAttrs))
    {
        NLPRINT_DEBUGVIEWLOG(L"Alloc resource failed\n");
        return false;
    }

    // 4. remove all tags
    int nRet = m_lfReadResourceAttributesW(pMgr, wstrFilePath.c_str(), pAttrs);
    if (1 == nRet)
    {
        nRet = m_lfRemoveResourceAttributesW(pMgr, wstrFilePath.c_str(), pAttrs);
    }

    NLFreeResource(pMgr, pAttrs);
    return (1 == nRet ? true : false);
}

vector<pair<wstring, wstring>> CNLTagManager::GetVecTagsFromINLTag(_In_ INLTag* pINLTag) const
{
    vector<pair<wstring, wstring>> vecTagPair;

     if (NULL == pINLTag)
    {
        return vecTagPair;
    }

    CComBSTR comBstrTagName = NULL;
    CComBSTR comBstrTagValue = NULL;
    pINLTag->GetFirstTag(&comBstrTagName, &comBstrTagValue);

    VARIANT_BOOL bIsEnd = VARIANT_TRUE;
    HRESULT hr = pINLTag->IsEnd(&bIsEnd);
    while (SUCCEEDED(hr) && (!bIsEnd))
    {
        vecTagPair.push_back(pair<wstring, wstring>(comBstrTagName.m_str, comBstrTagValue.m_str));

        pINLTag->GetNextTag(&comBstrTagName, &comBstrTagValue);
        hr = pINLTag->IsEnd(&bIsEnd);
    }
    NLPRINT_TAGPAIRLOG(vecTagPair, L"Begin output tags:", L"End");   // Debug output tags
    return vecTagPair;
}
/////////////////////////////End basic tools/////////////////////////////////////////////

////////////////////////////Tag library tools//////////////////////////////////////////////
void CNLTagManager::NLFreeResource(_Inout_ ResourceAttributeManager*& pMgr, _Inout_ ResourceAttributes*& pAttr) const
{
    if (NULL != pMgr)
    {
        m_lfCloseAttributeManager(pMgr);
        pMgr = NULL;
    }

    if (NULL != pAttr)
    {
        m_lfFreeAttributes(pAttr);
    }
}

void CNLTagManager::NLFreeResource(_Inout_ ResourceAttributeManager*& pMgr, _Inout_ vector<ResourceAttributes*>& vecpAttr) const
{
    if (NULL != pMgr)
    {
        m_lfCloseAttributeManager(pMgr);
        pMgr = NULL;
    }

    for (vector<ResourceAttributes*>::iterator itr = vecpAttr.begin(); itr != vecpAttr.end(); itr++)
    {
        if (NULL != *itr)
        {
            m_lfFreeAttributes(*itr);
        }
    }
    vecpAttr.clear();
}

bool CNLTagManager::NLAlloceResource(_Out_ ResourceAttributeManager*& pMgr, _Out_ ResourceAttributes*& pAttr) const
{
    // 1. initialize
    pMgr = NULL;
    pAttr = NULL;

    // 2. alloc mgr
    ResourceAttributeManager* pMgrTemp = NULL;
    m_lfCreateAttributeManager(&pMgrTemp);
    pMgr = pMgrTemp;

    // 2. check mgr
    if (NULL == pMgr)
    {
        return false;
    }

    ResourceAttributes* pResourcceAttr = NULL;
    m_lfAllocAttributes(&pResourcceAttr);
    pAttr = pResourcceAttr;

    if (NULL == pAttr)
    {
        NLFreeResource(pMgr, pAttr);
        return false;
    }
    return true;
}

bool CNLTagManager::NLAlloceResource(_Out_ ResourceAttributeManager*& pMgr, _Out_ vector<ResourceAttributes*>& vecpAttr, _In_ unsigned int nCount) const
{
    // 1. initialize
    pMgr = NULL;
    vecpAttr.clear();

    // 2. alloc mgr
    ResourceAttributeManager* pMgrTemp = NULL;
    m_lfCreateAttributeManager(&pMgrTemp);
    pMgr = pMgrTemp;

    // 2. check mgr
    if (NULL == pMgr)
    {
        return false;
    }

    for (unsigned int i = 0; i < nCount; i++)
    {
        ResourceAttributes* pResourcceAttr = NULL;
        m_lfAllocAttributes(&pResourcceAttr);

        if (NULL != pResourcceAttr)
        {
            vecpAttr.push_back(pResourcceAttr);
        }
        else
        {
            NLFreeResource(pMgr, vecpAttr);
            return false;
        }
    }
    return true;
}

void CNLTagManager::NLGetAttributeToVetor(_In_   ResourceAttributes*& pAttr, _Out_ vector<pair<wstring, wstring>>& vecTagPair) const
{
    int nSze = m_lfGetAttributeCount(pAttr);
    for (int i = 0; i < nSze; ++i)
    {
        wstring wstrTagName = (WCHAR *)m_lfGetAttributeName(pAttr, i);
        wstring wstrTagValue = (WCHAR *)m_lfGetAttributeValue(pAttr, i);

        boost::algorithm::trim(wstrTagName);
        boost::algorithm::trim(wstrTagValue);

        vecTagPair.push_back(pair<wstring, wstring>(wstrTagName, wstrTagValue));
    }
    NLPRINT_TAGPAIRLOG(vecTagPair, L"NLGetAttributeToVetor, TagInfo:", L"End");
}

void CNLTagManager::NLAddAttributeFromVector(_Inout_ ResourceAttributes*& pAttr, _In_ const vector<pair<wstring, wstring>>& kvecTagPair) const
{
    vector<pair<wstring, wstring>>::const_iterator itr;
    for (itr = kvecTagPair.begin(); itr != kvecTagPair.end(); itr++)
    {
        wstring wstrTagName = boost::algorithm::trim_copy(itr->first);
        wstring wstrTagValue = boost::algorithm::trim_copy(itr->second);

        if (!wstrTagName.empty() && !wstrTagValue.empty())
        {
            m_lfAddAttributeW(pAttr, wstrTagName.c_str(), wstrTagValue.c_str());
        }
    }
    NLPRINT_TAGPAIRLOG(kvecTagPair, L"NLAddAttributeFromVector, TagInfo:", L"End");
}

bool CNLTagManager::GetTagDllHandle()
{
    if (NULL != m_hLib && NULL != m_hMgr)// the libraries have been loaded
    {
        return true;
    }

    std::wstring strCommonPath = GetCommonComponentsDir();
    wstring wstrLib = strCommonPath + NLOFFICERESATTRLIB_MODULE_NAME;
    wstring wstrMgr = strCommonPath + NLOFFICERESATTRMGR_MODULE_NAME;

    m_hLib = LoadLibrary(wstrLib.c_str());
    if (NULL == m_hLib)
    {
        NLPRINT_DEBUGLOG(L"Load Library [%s] Failed! The error code is [%d]", wstrLib.c_str(), GetLastError());
        return false;
    }

    wchar_t szDllPathBak[2048] = { 0 };
    GetDllDirectoryW(2048, szDllPathBak);
    SetDllDirectoryW(strCommonPath.c_str());
    m_hMgr = LoadLibrary(wstrMgr.c_str());
    SetDllDirectoryW(szDllPathBak);

    if (NULL == m_hMgr)
    {
        NLPRINT_DEBUGLOG(L"Load Library [%s] Failed! The error code is [%d]", wstrMgr.c_str(), GetLastError());
        return false;
    }
    SetDllDirectoryW(NULL);
    return true;
}

bool CNLTagManager::GetTagFunAddr()
{
    m_lfCreateAttributeManager = (CreateAttributeManagerType)GetProcAddress(m_hMgr, "CreateAttributeManager");
    m_lfAllocAttributes = (AllocAttributesType)GetProcAddress(m_hLib, "AllocAttributes");
    m_lfReadResourceAttributesW = (ReadResourceAttributesWType)GetProcAddress(m_hMgr, "ReadResourceAttributesW");
    m_lfGetAttributeCount = (GetAttributeCountType)GetProcAddress(m_hLib, "GetAttributeCount");
    m_lfFreeAttributes = (FreeAttributesType)GetProcAddress(m_hLib, "FreeAttributes");
    m_lfCloseAttributeManager = (CloseAttributeManagerType)GetProcAddress(m_hMgr, "CloseAttributeManager");
    m_lfAddAttributeW = (AddAttributeWType)GetProcAddress(m_hLib, "AddAttributeW");
    m_lfGetAttributeName = (GetAttributeNameType)GetProcAddress(m_hLib, "GetAttributeName");
    m_lfGetAttributeValue = (GetAttributeValueType)GetProcAddress(m_hLib, "GetAttributeValue");
    m_lfWriteResourceAttributesW = (WriteResourceAttributesWType)GetProcAddress(m_hMgr, "WriteResourceAttributesW");
    m_lfRemoveResourceAttributesW = (RemoveResourceAttributesWType)GetProcAddress(m_hMgr, "RemoveResourceAttributesW");

    m_lConvert4GetAttr = (Convert4GetAttr)GetProcAddress(m_hMgr, "Convert_GetAttributes");
    m_lConvert4SetAttr = (Convert4SetAttr)GetProcAddress(m_hMgr, "Convert_SetAttributes");

    if (!(m_lfCreateAttributeManager && m_lfAllocAttributes &&
        m_lfReadResourceAttributesW && m_lfGetAttributeCount &&
        m_lfFreeAttributes && m_lfCloseAttributeManager && m_lfAddAttributeW &&
        m_lfGetAttributeName && m_lfGetAttributeValue &&
        m_lfWriteResourceAttributesW&& m_lfRemoveResourceAttributesW &&
        m_lConvert4GetAttr && m_lConvert4SetAttr))
    {
        NLPRINT_DEBUGLOG(L"failed to get resattrlib/resattrmgr functions\r\n");
        return false;
    }
    return true;
}
////////////////////////////End tag library tools//////////////////////////////////////////////

////////////////////////////Independence tools//////////////////////////////////////////////
wstring CNLTagManager::NLGetEffectFilePath(_In_ const wstring& kwstrFilePath) const
{
    // If we need support HTTP path, here we need convert the HTTP path to UNC path
    if (!PathFileExists(kwstrFilePath.c_str()))
    {
        return L"";
    }
    return kwstrFilePath;
}
////////////////////////////End independence tools//////////////////////////////////////////////