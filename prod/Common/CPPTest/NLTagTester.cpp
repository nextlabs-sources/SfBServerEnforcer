/** precompile header and resource header files */
#include "stdafx.h"
/** current class declare header files */
#include "NLTagTester.h"

/** C system header files */

/** C++ system header files */
#include <string>
#include <vector>
/** Platform header files */

/** Third part library header files */
/** boost */

/** Other project header files */
#include "./import/SDKWrapper_i.h"
#include "./import/SDKWrapper_i.c"

// #include "./import/sdkwrapper.tlh"
// using namespace SDKWrapperLib;

/** Current project header files */


using namespace std;

/*
    Import command example:
        #import "Office2013_64bitDLL/MSADDNDR.OLB" raw_interfaces_only
*/
CNLTagTester::CNLTagTester()
{
    // Com initialize
    ::CoInitialize(NULL);
}

CNLTagTester::~CNLTagTester()
{
    ::CoUninitialize();
}

// vector<pair<wstring, wstring>> CNLTagTester::GetVecTagsFromINLTag(_In_ INLTag* pINLTag) const
vector<pair<wstring, wstring>> GetVecTagsFromINLTag(_In_ INLTag* pINLTag)
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
    return vecTagPair;
}

void* GetComInstance(_In_ const CLSID clsidCom, _In_ const IID iidInterface, _Inout_ CComPtr<IClassFactory>* pInObsClassFactory)
{
    HRESULT hr = E_FAIL;
    CComPtr<IClassFactory> pObsClassFactory = NULL;
    if ((NULL == pInObsClassFactory) || (NULL == *pInObsClassFactory))
    {
        hr = CoGetClassObject(clsidCom, CLSCTX_INPROC_SERVER, 0, IID_IClassFactory, (void**)&pObsClassFactory);
        wprintf(L"CoCreateInstance for IObligation factory, [0x%x]\n", hr);
        if (SUCCEEDED(hr) && (NULL != pObsClassFactory))
        {
            if (NULL != pInObsClassFactory)
            {
                *pInObsClassFactory = pObsClassFactory;
            }
        }
    }
    else
    {
        pObsClassFactory = *pInObsClassFactory;
    }

    void* pOb = NULL;
    if (SUCCEEDED(hr) && (NULL != pObsClassFactory))
    {
        hr = pObsClassFactory->CreateInstance(NULL, iidInterface, &pOb);
        wprintf(L"Create instance by factory, hr:[0x%x], pOb:[0x%x]\n", hr, pOb);
        if (FAILED(hr))
        {
            pOb = NULL;
        }
    }
    return pOb;
}

void CNLTagTester::Test(_In_ const wstring& kwstrTestFilePath)
{
    NLPRINT_DEBUGVIEWLOG(L"File path:[%s]\n", kwstrTestFilePath.c_str());

    {
        // Test
        CComPtr<IClassFactory> pObsClassFactory = NULL;
        HRESULT hr = CoGetClassObject(CLSID_Obligation, CLSCTX_INPROC_SERVER, 0, IID_IClassFactory, (void**)&pObsClassFactory);
        wprintf(L"CoCreateInstance for IObligation factory, [0x%x]\n", hr);

        IObligation* pOb = NULL;
        hr = pObsClassFactory->CreateInstance(NULL, IID_IObligation, (void**)&pOb);
        wprintf(L"Create instance by factory, hr:[0x%x], pOb:[0x%x]\n", hr, pOb);
    }

    // Get NLTagManager and NLTag
    INLTagManager* pINLTagManager = (INLTagManager*)GetComInstance(CLSID_NLTagManager, IID_INLTagManager, NULL);
    if (NULL == pINLTagManager)
    {
        wprintf(L"Get INLTagManager instance failed\n");
        return;
    }

    INLTag* pINLTag = (INLTag*)GetComInstance(CLSID_NLTag, IID_INLTag, NULL);
    if (NULL == pINLTag)
    {
        wprintf(L"Get INLTag instance failed\n");
        return ;
    }

    // Init
    pINLTag->Init(NULL, VARIANT_TRUE, VARIANT_TRUE);
    CComBSTR comBstrFilePath(kwstrTestFilePath.c_str());

    // Read tags
    {
        pINLTag->Clear(VARIANT_FALSE);

        HRESULT hr = pINLTagManager->ReadTag(comBstrFilePath, 1, &pINLTag);
        NLPRINT_DEBUGVIEWLOG(L"ReadTag 1 HRESULT:[0x%x]\n", hr);
        vector<pair<wstring, wstring>> vecTags = GetVecTagsFromINLTag(pINLTag);
        NLPRINT_TAGPAIRLOG(vecTags, L"Read tags:", L"End");
    }
#if 1
    // Write new tags
    {
        pINLTag->Clear(VARIANT_FALSE);
        {
            CComBSTR comBstrTagName(L"TestName1");
            CComBSTR comBstrTagValue(L"TestValue1");
            HRESULT hr = pINLTag->SetTag(comBstrTagName, comBstrTagValue, 1);
            NLPRINT_DEBUGVIEWLOG(L"SetTag 1 HRESULT:[0x%x]\n", hr);
        }
        {
            CComBSTR comBstrTagName(L"TestName2");
            CComBSTR comBstrTagValue(L"TestValue2");
            HRESULT hr = pINLTag->SetTag(comBstrTagName, comBstrTagValue, 1);
            NLPRINT_DEBUGVIEWLOG(L"SetTag 2 HRESULT:[0x%x]\n", hr);
        }
        
        vector<pair<wstring, wstring>> vecTags = GetVecTagsFromINLTag(pINLTag);
        NLPRINT_TAGPAIRLOG(vecTags, L"Test INLTag set:", L"End");

        HRESULT hr = pINLTagManager->WriteTag(comBstrFilePath, pINLTag);
        NLPRINT_DEBUGVIEWLOG(L"WriteTag 1 HRESULT:[0x%x]\n", hr);
        NLPRINT_DEBUGVIEWLOG(L"ReadTag HRESULT:[0x%x]\n", hr);
    }
    // Read tags
    {
        pINLTag->Clear(VARIANT_FALSE);
        
        HRESULT hr = pINLTagManager->ReadTag(comBstrFilePath, 1, &pINLTag);
        NLPRINT_DEBUGVIEWLOG(L"ReadTag 2 HRESULT:[0x%x]\n", hr);
        vector<pair<wstring, wstring>> vecTags = GetVecTagsFromINLTag(pINLTag);
        NLPRINT_TAGPAIRLOG(vecTags, L"Read tags:", L"End");
    }
#endif
#if 1
    // Delete tags
    {
        pINLTag->Clear(VARIANT_FALSE);
        {
            CComBSTR comBstrTagName(L"TestName1");
            CComBSTR comBstrTagValue(L"TestValue1");
            HRESULT hr = pINLTag->SetTag(comBstrTagName, comBstrTagValue, 1);
            NLPRINT_DEBUGVIEWLOG(L"SetTag which need remove 1 HRESULT:[0x%x]\n", hr);
        }
        vector<pair<wstring, wstring>> vecTags = GetVecTagsFromINLTag(pINLTag);
        NLPRINT_TAGPAIRLOG(vecTags, L"Delete tags:", L"End");

        HRESULT hr = pINLTagManager->RemoveTag(comBstrFilePath, pINLTag);
        NLPRINT_DEBUGVIEWLOG(L"RemoveTags 1 HRESULT:[0x%x]\n", hr);
    }
    // Read tags
    {
        pINLTag->Clear(VARIANT_FALSE);

        HRESULT hr = pINLTagManager->ReadTag(comBstrFilePath, 1, &pINLTag);
        NLPRINT_DEBUGVIEWLOG(L"ReadTag 3 HRESULT:[0x%x]\n", hr);
        vector<pair<wstring, wstring>> vecTags = GetVecTagsFromINLTag(pINLTag);
        NLPRINT_TAGPAIRLOG(vecTags, L"Read tags:", L"End");
    }
#endif
#if 1 // Delete all tags
    // Clear all tags
    {
        pINLTag->Clear(VARIANT_FALSE);
        
        HRESULT hr = pINLTagManager->RemoveAllTags(comBstrFilePath);
        NLPRINT_DEBUGVIEWLOG(L"RemoveAllTags 1 HRESULT:[0x%x]\n", hr);
    }

    // Read tags
    {
        pINLTag->Clear(VARIANT_FALSE);
        
        HRESULT hr = pINLTagManager->ReadTag(comBstrFilePath, 1, &pINLTag);
        NLPRINT_DEBUGVIEWLOG(L"ReadTag 3 HRESULT:[0x%x]\n", hr);
        vector<pair<wstring, wstring>> vecTags = GetVecTagsFromINLTag(pINLTag);
        NLPRINT_TAGPAIRLOG(vecTags, L"Read tags:", L"End");
    }
#endif
}
