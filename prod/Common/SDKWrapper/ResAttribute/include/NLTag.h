#pragma once

#include "stdafx.h"

#include <string>
#include <vector>
#include <map>

#include "../platform/resattrmgr.h"
#include "../platform/resattrlib.h"

#include "common_tools.h"
#include "resource.h"

#include "SDKWrapper_i.h"

using namespace std;


/**
*  @file INLTag interface implement, thread unsafe
*
*  @author NextLabs::Kim
*/
class ATL_NO_VTABLE CNLTag :
    public CComObjectRootEx<CComMultiThreadModel>,
    public CComCoClass<CNLTag, &CLSID_NLTag>,
    public IDispatchImpl<INLTag, &IID_INLTag, &LIBID_SDKWrapperLib, /*wMajor =*/ 1, /*wMinor =*/ 0 >
{
private: /**< static private members */
    static const int s_knTagOpMin = 0;
    static const int s_knTagOpMax = 4;
public: /**< static public members */
    static const int s_knTagOpReplace = 1;
    static const int s_knTagOpAppend = 2;
    static const int s_knTagOpMerge = 3;

public: /**< static functions */
    static bool IsValidTagOperation(_In_ const int knTagOp);
public:
    /**
    *  @brief CNLTag constructor, init basic information.
    *
    *  @param kstrTagValueSeparator [IN] the separator for tag value if we need support multiple tag values otherwise please make sure the separator is empty.
    *  @retval no.
    */
    CNLTag(_In_ const bool kbInitFlag = false, _In_ const wstring& kstrTagValueSeparator = L"", _In_ const bool bIgnoreCaseForTagName = true, _In_ const bool bIgnoreCaseForTagValue = true);
    ~CNLTag(void);

    DECLARE_REGISTRY_RESOURCEID(IDR_NLTAG)

    BEGIN_COM_MAP(CNLTag)
        COM_INTERFACE_ENTRY(INLTag)
        COM_INTERFACE_ENTRY(IDispatch)
    END_COM_MAP()

    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct();
    void FinalRelease();

public:
    STDMETHOD(Init)(_In_ BSTR bstrTagValueSeparator, _In_ VARIANT_BOOL bIgnoreCaseForTagName, _In_ VARIANT_BOOL bIgnoreCaseForTagValue);
    STDMETHOD(Clear)(_In_ VARIANT_BOOL bClearInitFlags);
    STDMETHOD(IsInited)(_Out_ VARIANT_BOOL* pbInitFlag);

    /**
    *  @brief Get current tag item count.
    *
    *  @param plTagCount [OUT] out the tag item count.
    *  @retval return S_OK if get count success, return E_INVALIDARG if plTagCount is null, otherwise return E_FAIL.
    */
    STDMETHOD(GetTagCount)(_Out_ LONG* plTagCount);
    /**
    *  @brief Get tag value separator which initialized in constructor.
    *
    *  @param pbstrTagValueSeparator [OUT] out the tag value separator.
    *  @retval return S_OK if get tag value separator success, return E_INVALIDARG if pbstrTagValueSeparator is null, otherwise return E_FAIL.
    */
    STDMETHOD(GetTagValueSeparator)(_Out_ BSTR* pbstrTagValueSeparator);

    /** 
    *  @addtogroup traverse tags
    *  
    *  @{
    *   @param pbstrName [OUT] out the tag name.
    *   @param pbstrValue [OUT] out the tag value, if current environment support multiple tag values, all tag value will be connect with the tag value separator.
    *   @param pbIsEnd [OUT] out VARIANT_TRUE means is end, otherwise the iterator is not the end.
    */
    STDMETHOD(GetFirstTag)(_Out_ BSTR* pbstrName, _Out_ BSTR* pbstrValue);
    STDMETHOD(GetNextTag)(_Out_ BSTR* pbstrName, _Out_ BSTR* pbstrValue);
    STDMETHOD(IsEnd)(_Out_ VARIANT_BOOL* pbIsEnd);
    /**
    *  @}
    */

    /**
    *  @brief Get tag by name.
    *
    *  @param pbstrValue [OUT] out tag values, if current environment support multiple tag values, all tag value will be connect with the tag value separator.
    *  @retval return S_OK if get tag success, return E_INVALIDARG if bstrName null or empty, otherwise return E_FAIL.
    */
    STDMETHOD(GetTagByName)(_In_ BSTR bstrName, _Out_ BSTR* pbstrValue);

    /**
    *  @brief Set tag.
    *
    *  @param bstrName [IN] tag name.
    *  @param bstrValue [IN] tag value.
    *  @param nOperationType [IN] a number specify how to set the tag: Replace, Append or Merge, default is "1" replace
    *           nOperationType = 1 :  Replace: remove the old and set the new tag
    *           nOperationType = 2 : Append: just add the new tag value at the end of the old tag value
    *           nOperationType = 3 : Merge: merge tag value
    *               1. keep all the tag values
    *               2. If the new tag value do not exit add it
    *               3. if the tag value already exist ignore.
    *  @retval return S_OK if set tag success, return E_INVALIDARG if bstrName or bstrValue null or empty, or nOperationType is unknown type, otherwise return E_FAIL. After set a new tag, the tags index will be changed.
    */
    STDMETHOD(SetTag)(_In_ BSTR bstrName, _In_ BSTR bstrValue, _In_ int nOperationType);

    /**
    *  @brief Delete tag by tag name.
    *
    *  @param bstrName [IN] tag name which will be delete.
    *  @retval return S_OK if delete success, return E_INVALIDARG if bstrName is null, empty or not, otherwise return E_FAIL.
    */
    STDMETHOD(DeleteTagByName)(_In_ BSTR bstrName);
    /**
    *  @brief Delete all tags.
    */
    STDMETHOD(DeleteAllTags)();
private:
    void InnerInit(_In_ const wstring& kstrTagValueSeparator, _In_ const bool kbIgnoreCaseForTagName, _In_ const bool kbIgnoreCaseForTagValue);

    void GetCurTagInfo(_Out_ BSTR* pbstrName, _Out_ BSTR* pbstrValue) const;
    void GetCurTagInfo(_Out_ wstring& wstrName, _Out_ wstring& wstrValue) const;

    const vector<wstring>& InnerGetTagValueByName(_In_ const wstring& kwstrTagName) const;
    bool InnerSetTag(_In_ const wstring& kwstrTagName, _In_ const wstring& kwstrTagValue, _In_ const int nOperationType);
    bool InnerDeleteTagValueByName(_In_ const wstring& kwstrTagName);

    bool IsSameTagName(_In_ const wstring& kwstrFirstTagName, _In_ const wstring& kwstrSecondTagName) const;
    bool IsSameTagValue(_In_ const wstring& kwstrFirstTagValue, _In_ const wstring& kwstrSecondTagValue) const;
    bool IsSameTagValue(_In_ const vector<wstring>& vecFirstTagValue, _In_ const vector<wstring>& vecSecondTagValue) const;
    const vector<wstring>& GetVectorTagValuesFromStringTagValue(_In_ const wstring& kwstrTagValue) const;

    void SetInitFlag(_In_ const bool kbInitFlag) { m_bInited = kbInitFlag; }
    bool GetInitFlag() const { return m_bInited; }
private: /**< independence tools */


private:
    bool m_kbIgnoreCaseForTagName;
    bool m_kbIgnoreCaseForTagValue;
    wstring m_kstrTagValueSeparator;
    bool m_bInited;

    map<wstring, vector<wstring>> m_mapTagInfo;
    map<wstring, vector<wstring>>::const_iterator m_kpCurIterator;
};

OBJECT_ENTRY_AUTO(__uuidof(NLTag), CNLTag)