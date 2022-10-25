#pragma once

#include "stdafx.h"

#include <string>
#include <vector>

#include "../platform/resattrmgr.h"
#include "../platform/resattrlib.h"

#include "common_tools.h"
#include "resource.h"

#include "SDKWrapper_i.h"

using namespace std;

typedef int(*CreateAttributeManagerType)(ResourceAttributeManager **mgr);
typedef int(*AllocAttributesType)(ResourceAttributes **attrs);
typedef int(*ReadResourceAttributesWType)(ResourceAttributeManager *mgr, const WCHAR *filename, ResourceAttributes *attrs);
typedef int(*GetAttributeCountType)(const ResourceAttributes *attrs);
typedef void(*FreeAttributesType)(ResourceAttributes *attrs);
typedef void(*CloseAttributeManagerType)(ResourceAttributeManager *mgr);
typedef void(*AddAttributeWType)(ResourceAttributes *attrs, const WCHAR *name, const WCHAR *value);
typedef const WCHAR *(*GetAttributeNameType)(const ResourceAttributes *attrs, int index);
typedef const WCHAR * (*GetAttributeValueType)(const ResourceAttributes *attrs, int index);
typedef int(*WriteResourceAttributesWType)(ResourceAttributeManager *mgr, const WCHAR *filename, ResourceAttributes *attrs);
typedef int(*RemoveResourceAttributesWType)(ResourceAttributeManager *mgr, const WCHAR *filename, ResourceAttributes *attrs);
typedef int(*Convert4GetAttr)(ResourceAttributes *attrs, ResourceAttributes* existing_attrs);
typedef int(*Convert4SetAttr)(ResourceAttributes* attrs_to_set, ResourceAttributes* merged_attrs);

class ATL_NO_VTABLE CNLTagManager :
    public CComObjectRootEx<CComMultiThreadModel>,
    public CComCoClass<CNLTagManager, &CLSID_NLTagManager>,
    public IDispatchImpl<INLTagManager, &IID_INLTagManager, &LIBID_SDKWrapperLib, /*wMajor =*/ 1, /*wMinor =*/ 0 >
{
public:
    CNLTagManager(void);
    ~CNLTagManager(void);

    DECLARE_REGISTRY_RESOURCEID(IDR_NLTagManager)

    BEGIN_COM_MAP(CNLTagManager)
        COM_INTERFACE_ENTRY(INLTagManager)
        COM_INTERFACE_ENTRY(IDispatch)
    END_COM_MAP()

    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct();
    void FinalRelease();

public:
    STDMETHOD(ReadTag)(_In_ BSTR bstrFilePath, _In_ int nOperationType, _Inout_ INLTag** pINLTag);

    STDMETHOD(WriteTag)(_In_ BSTR bstrFilePath, _In_ INLTag* pINLTag);

    STDMETHOD(RemoveTag)(_In_ BSTR bstrFilePath, _In_ INLTag* pINLTag);
    STDMETHOD(RemoveAllTags)(_In_ BSTR bstrFilePath);

private:
    bool ReadTag(_In_ const wstring& kwstrInFilePath, _Out_ vector<pair<wstring, wstring>>& vecTagPair) const;

    // add tag base on file use tag library.
    bool AddTag(_In_ const wstring& kwstrInFilePath, _In_ const vector<pair<wstring, wstring>>& kvecTagPair) const;
    // add tag on document base on office COM object, add tag to custom attributes

    bool RemoveTag(_In_ const wstring& kwstrInFilePath, _In_ const vector<pair<wstring, wstring>>& kvecTagPair) const;

    bool RemoveAllTag(_In_ const wstring& kwstrInFilePath) const;

private:
    // tools, before you use the following function you need the if we get the tag library success
    void NLAddAttributeFromVector(_Inout_ ResourceAttributes*& pAttr, _In_ const vector<pair<wstring, wstring>>& kvecTagPair) const;
    void NLGetAttributeToVetor(_In_   ResourceAttributes*& pAttr, _Out_ vector<pair<wstring, wstring>>& kvecTagPair) const;

    bool NLAlloceResource(_Out_ ResourceAttributeManager*& pMgr, _Out_ vector<ResourceAttributes*>& vecpAttr, _In_ unsigned int nCount) const;
    bool NLAlloceResource(_Out_ ResourceAttributeManager*& pMgr, _Out_ ResourceAttributes*& pAttr) const;

    void NLFreeResource(_Inout_ ResourceAttributeManager*& pMgr, _Inout_ vector<ResourceAttributes*>&  vecpAttr) const;
    void NLFreeResource(_Inout_ ResourceAttributeManager*& pMgr, _Inout_ ResourceAttributes*& pAttr) const;

    // Common function
    vector<pair<wstring, wstring>> GetVecTagsFromINLTag(_In_ INLTag* pINLTag) const;

    // for file path
    wstring NLGetEffectFilePath(_In_ const wstring& kwstrFilePath) const;

    // for load tag library
    bool GetTagDllHandle();
    bool GetTagFunAddr();
    bool IsGetFunSuc() const { return m_bIsGetFunSuc; }
    void SetGetFunSucFlag(_In_ const bool kbSuccess) { m_bIsGetFunSuc = kbSuccess; }

private:
    bool	m_bIsGetFunSuc;
    HMODULE	m_hLib;
    HMODULE	m_hMgr;

    // we can not make sure the return value means 
    CreateAttributeManagerType		m_lfCreateAttributeManager;
    AllocAttributesType				m_lfAllocAttributes;
    ReadResourceAttributesWType		m_lfReadResourceAttributesW;	// return 1, means right
    GetAttributeCountType			m_lfGetAttributeCount;

    FreeAttributesType				m_lfFreeAttributes;				// no return value
    CloseAttributeManagerType		m_lfCloseAttributeManager;		// no return value
    AddAttributeWType				m_lfAddAttributeW;				// no return value

    GetAttributeNameType			m_lfGetAttributeName;
    GetAttributeValueType			m_lfGetAttributeValue;
    WriteResourceAttributesWType	m_lfWriteResourceAttributesW;	// return 1, means right
    RemoveResourceAttributesWType	m_lfRemoveResourceAttributesW;

    Convert4GetAttr					m_lConvert4GetAttr;		// this function alway return 1, no means			
    Convert4SetAttr					m_lConvert4SetAttr;		// this function alway return 1, no means
};

OBJECT_ENTRY_AUTO(__uuidof(NLTagManager), CNLTagManager)