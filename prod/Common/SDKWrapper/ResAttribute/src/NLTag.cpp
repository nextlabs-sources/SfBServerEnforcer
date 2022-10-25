#include "StdAfx.h"
#include "../include/NLTag.h"

// Windows
#include <shlwapi.h>

// C++
#include <string>
#include <vector>
#include <map>

// Boost
#pragma warning( push )
#pragma warning( disable: 4996 4512 4244 6011 )
#include <boost/algorithm/string.hpp>
#pragma warning( pop )

#include "nlofficerep_only_debug.h"
#include "common_tools.h"


////////////////////////////CELog2//////////////////////////////////////////////
// for CELog2 we should define: CELOG_CUR_FILE
#define CELOG_CUR_FILE static_cast<celog_filepathint_t>(CELOG_FILEPATH_OFFICEPEP_MIN + EMNLFILEINTEGER_NLTAG)
//////////////////////////////////////////////////////////////////////////

using namespace std;

bool CNLTag::IsValidTagOperation(_In_ const int knTagOp)
{
    return (knTagOp > s_knTagOpMin) && (knTagOp < s_knTagOpMax);
}

CNLTag::CNLTag(_In_ const bool kbInitFlag, _In_ const wstring& kstrTagValueSeparator, _In_ const bool bIgnoreCaseForTagName, _In_ const bool bIgnoreCaseForTagValue)
    : m_kstrTagValueSeparator(kstrTagValueSeparator), m_kbIgnoreCaseForTagName(bIgnoreCaseForTagName), m_kbIgnoreCaseForTagValue(bIgnoreCaseForTagValue)
{
    SetInitFlag(false);
    InnerInit(kstrTagValueSeparator, bIgnoreCaseForTagName, bIgnoreCaseForTagValue);
    SetInitFlag(kbInitFlag);
}

CNLTag::~CNLTag(void)
{

}

HRESULT CNLTag::FinalConstruct()
{
    return S_OK;
}

void CNLTag::FinalRelease()
{
}

STDMETHODIMP CNLTag::Init(_In_ BSTR bstrTagValueSeparator, _In_ VARIANT_BOOL bIgnoreCaseForTagName, _In_ VARIANT_BOOL bIgnoreCaseForTagValue)
{
    InnerInit(((NULL == bstrTagValueSeparator) ? L"" : bstrTagValueSeparator), bIgnoreCaseForTagName, bIgnoreCaseForTagValue);
    return S_OK;
}
STDMETHODIMP CNLTag::Clear(_In_ VARIANT_BOOL bClearInitFlags)
{
    if (bClearInitFlags)
    {
        // Init all information
        SetInitFlag(false);
        InnerInit(L"", true, true);
    }
    else
    {
        // Just clear the tag information
        m_mapTagInfo.clear();
        m_kpCurIterator = m_mapTagInfo.cbegin();
    }
    return S_OK;
}
STDMETHODIMP CNLTag::IsInited(_Out_ VARIANT_BOOL* pbInitFlag)
{
    if (NULL == pbInitFlag)
    {
        return E_INVALIDARG;
    }
    
    *pbInitFlag = m_bInited;
    return S_OK;
}

STDMETHODIMP CNLTag::GetTagCount(_Out_ LONG* plTagCount)
{
    if (NULL == plTagCount)
    {
        return E_INVALIDARG;
    }
    else if (!GetInitFlag())
    {
        return E_UNEXPECTED;
    }
    else
    {
        *plTagCount = m_mapTagInfo.size();
    }
    return S_OK;
}
STDMETHODIMP CNLTag::GetTagValueSeparator(_Out_ BSTR* pbstrTagValueSeparator)
{
    if (NULL == pbstrTagValueSeparator)
    {
        return E_INVALIDARG;
    }
    else if (!GetInitFlag())
    {
        return E_UNEXPECTED;
    }
    else if (m_kstrTagValueSeparator.empty())
    {
        *pbstrTagValueSeparator = NULL;
    }
    else
    {
        *pbstrTagValueSeparator = ::SysAllocString(m_kstrTagValueSeparator.c_str());
    }
    return S_OK;
}

STDMETHODIMP CNLTag::GetFirstTag(_Out_ BSTR* pbstrName, _Out_ BSTR* pbstrValue)
{
    if ((NULL == pbstrName) || (NULL == pbstrValue))
    {
        return E_INVALIDARG;
    }
    else if (!GetInitFlag())
    {
        return E_UNEXPECTED;
    }

    m_kpCurIterator = m_mapTagInfo.begin();
    GetCurTagInfo(pbstrName, pbstrValue);
    return S_OK;
}
STDMETHODIMP CNLTag::GetNextTag(_Out_ BSTR* pbstrName, _Out_ BSTR* pbstrValue)
{
    if ((NULL == pbstrName) || (NULL == pbstrValue))
    {
        return E_INVALIDARG;
    }
    else if (!GetInitFlag())
    {
        return E_UNEXPECTED;
    }

    ++m_kpCurIterator;
    GetCurTagInfo(pbstrName, pbstrValue);
    return S_OK;
}
STDMETHODIMP CNLTag::IsEnd(_Out_ VARIANT_BOOL* pbIsEnd)
{
    if (NULL == pbIsEnd)
    {
        return E_INVALIDARG;
    }
    else if (!GetInitFlag())
    {
        return E_UNEXPECTED;
    }
   
    *pbIsEnd = (m_kpCurIterator == m_mapTagInfo.end()) ? VARIANT_TRUE : VARIANT_FALSE;
    return S_OK;
}

STDMETHODIMP CNLTag::GetTagByName(_In_ BSTR bstrName, _Out_ BSTR* pbstrValue)
{
    if ((NULL == bstrName) || (NULL == pbstrValue))
    {
        return E_INVALIDARG;
    }
    else if (!GetInitFlag())
    {
        return E_UNEXPECTED;
    }

    const vector<wstring>& kvecTagValue = InnerGetTagValueByName(bstrName);
    wstring wstrValue = ConnectVectorToString(kvecTagValue, m_kstrTagValueSeparator);
    *pbstrValue = ::SysAllocString(wstrValue.c_str());

    return S_OK;
}

STDMETHODIMP CNLTag::SetTag(_In_ BSTR bstrName, _In_ BSTR bstrValue, _In_ int nOperationType)
{NLONLY_DEBUG
    if ((NULL == bstrName) || (NULL == bstrValue))
    {
        NLPRINT_DEBUGVIEWLOG(L"Parameter error\n");
        return E_INVALIDARG;
    }
    else if (!GetInitFlag())
    {
        NLPRINT_DEBUGVIEWLOG(L"Do not initialized\n");
        return E_UNEXPECTED;
    }

    if (m_kstrTagValueSeparator.empty())
    {
        nOperationType = s_knTagOpReplace;
    }

    bool bInnerSetTag = InnerSetTag(bstrName, bstrValue, nOperationType);
    return bInnerSetTag ? S_OK : E_FAIL;
}

STDMETHODIMP CNLTag::DeleteTagByName(_In_ BSTR bstrName)
{
    if (NULL == bstrName)
    {
        return E_INVALIDARG;
    }
    else if (!GetInitFlag())
    {
        return E_UNEXPECTED;
    }

    InnerDeleteTagValueByName(bstrName);    /**< Delete failed, means the tag name do not exist, for this case the interface maybe no need return E_FAIL */
    return S_OK;
}
STDMETHODIMP CNLTag::DeleteAllTags()
{
    if (!GetInitFlag())
    {
        return E_UNEXPECTED;
    }

    m_mapTagInfo.clear();
    return S_OK;
}

void CNLTag::GetCurTagInfo(_Out_ BSTR* pbstrName, _Out_ BSTR* pbstrValue) const
{
    if (m_kpCurIterator == m_mapTagInfo.end())
    {
        *pbstrName = NULL;
        *pbstrValue = NULL;
    }
    else
    {
        *pbstrName = ::SysAllocString((m_kpCurIterator->first).c_str());
        wstring wstrValue = ConnectVectorToString(m_kpCurIterator->second, m_kstrTagValueSeparator);
        *pbstrValue = ::SysAllocString(wstrValue.c_str());
    }
}


void CNLTag::GetCurTagInfo(_Out_ wstring& wstrName, _Out_ wstring& wstrValue) const
{
    if (m_kpCurIterator == m_mapTagInfo.end())
    {
        wstrName = L"";
        wstrValue = L"";
    }
    else
    {
        wstrName = m_kpCurIterator->first;
        wstrValue = ConnectVectorToString(m_kpCurIterator->second, m_kstrTagValueSeparator);
   }
}

void CNLTag::InnerInit(_In_ const wstring& kstrTagValueSeparator, _In_ const bool kbIgnoreCaseForTagName, _In_ const bool kbIgnoreCaseForTagValue)
{
    if (!GetInitFlag())
    {
        m_kbIgnoreCaseForTagName = kbIgnoreCaseForTagName;
        m_kbIgnoreCaseForTagValue = kbIgnoreCaseForTagValue;
        m_kstrTagValueSeparator = kstrTagValueSeparator;

        m_mapTagInfo.clear();
        m_kpCurIterator = m_mapTagInfo.cbegin();

        SetInitFlag(true);
    }
}

const vector<wstring>& CNLTag::InnerGetTagValueByName(_In_ const wstring& kwstrTagName) const
{
    vector<wstring> vecTagValues;
    if (m_kbIgnoreCaseForTagName)
    {
        GetValueFromMapByKey(m_mapTagInfo, boost::to_lower_copy(kwstrTagName), vecTagValues);
    }
    else
    {
        GetValueFromMapByKey(m_mapTagInfo, kwstrTagName, vecTagValues);
    }
    return vecTagValues;
}

bool CNLTag::InnerSetTag(_In_ const wstring& kwstrTagName, _In_ const wstring& kwstrTagValue, _In_ const int nOperationType)
{NLONLY_DEBUG
    bool bRet = false;

    // Make a standard tag name, if tag name ignore case make all tag name as lower case.
    wstring wstrTagName = kwstrTagName;
    if (m_kbIgnoreCaseForTagName)
    {
        wstrTagName = boost::to_lower_copy(kwstrTagName);
    }

    // Make a standard tag value
    bool bIsMultipleValue = false;
    vector<wstring> vecNewTagValues;
    if (m_kstrTagValueSeparator.empty())
    {
        vecNewTagValues.push_back(kwstrTagValue);
    }
    else
    {
        bIsMultipleValue = true;
        vecNewTagValues = SplitString(kwstrTagValue, m_kstrTagValueSeparator, true);
    }

    switch (nOperationType)
    {
    case s_knTagOpReplace:
    {
        m_mapTagInfo[wstrTagName] = vecNewTagValues;
        bRet = true;
        break;
    }
    case s_knTagOpAppend:
    {
        if (bIsMultipleValue)
        {
            for (vector<wstring>::iterator itr = vecNewTagValues.begin(); itr != vecNewTagValues.end(); ++itr)
            {
                m_mapTagInfo[wstrTagName].push_back(*itr);
            }
        }
        else
        {
            if (m_mapTagInfo[wstrTagName].empty())
            {
                m_mapTagInfo[wstrTagName].push_back(vecNewTagValues[0]);
            }
            else
            {
                m_mapTagInfo[wstrTagName][0] += vecNewTagValues[0];
            }
        }
        bRet = true;
        break;
    }
    case s_knTagOpMerge:
    {
        if (bIsMultipleValue)
        {
            vector<wstring>& vecOrgTagInfo = m_mapTagInfo[wstrTagName];
            bool bFindOrg = false;
            for (vector<wstring>::iterator itr = vecNewTagValues.begin(); itr != vecNewTagValues.end(); ++itr)
            {
                bFindOrg = IsContainsInVec((*itr), vecOrgTagInfo, m_kbIgnoreCaseForTagValue);
                if (bFindOrg)
                {
                    // Find, already exist, ignore
                    continue;
                }
                else
                {
                    // Do not find, add the new tag value
                    vecOrgTagInfo.push_back(*itr);
                }
            }
        }
        else
        {
            // Signal value, merge is replace
            m_mapTagInfo[wstrTagName] = vecNewTagValues;
        }
        bRet = true;
        break;
    }
    default:
        break;
    }
    return bRet;
}

bool CNLTag::InnerDeleteTagValueByName(_In_ const wstring& kwstrTagName)
{
    size_t stEraseNum = 0;
    if (m_kbIgnoreCaseForTagName)
    {
        stEraseNum = m_mapTagInfo.erase(boost::to_lower_copy(kwstrTagName));
    }
    else
    {
        stEraseNum = m_mapTagInfo.erase(kwstrTagName);
    }
    return (0 < stEraseNum);
}

bool CNLTag::IsSameTagName(_In_ const wstring& kwstrFirstTagName, _In_ const wstring& kwstrSecondTagName) const
{
    return IsSameString(kwstrFirstTagName, kwstrSecondTagName, m_kbIgnoreCaseForTagName);
}

bool CNLTag::IsSameTagValue(_In_ const wstring& kwstrFirstTagValue, _In_ const wstring& kwstrSecondTagValue) const
{
    bool bIsMultipleValue = false;
    vector<wstring> vecFirstTagValue = GetVectorTagValuesFromStringTagValue(kwstrFirstTagValue);
    vector<wstring> vecSecondTagValue = GetVectorTagValuesFromStringTagValue(kwstrSecondTagValue);
    return IsSameTagValue(vecFirstTagValue, vecSecondTagValue);
}

bool CNLTag::IsSameTagValue(_In_ const vector<wstring>& vecFirstTagValue, _In_ const vector<wstring>& vecSecondTagValue) const
{
    return ((IsFirstVecContainsInSencondVec(vecFirstTagValue, vecSecondTagValue, m_kbIgnoreCaseForTagValue)) && (IsFirstVecContainsInSencondVec(vecSecondTagValue, vecFirstTagValue, m_kbIgnoreCaseForTagValue)));
}

const vector<wstring>& CNLTag::GetVectorTagValuesFromStringTagValue(_In_ const wstring& kwstrTagValue) const
{
    vector<wstring> vecTagValues;
    if (m_kstrTagValueSeparator.empty())
    {
        vecTagValues.push_back(kwstrTagValue);
    }
    else
    {
        vecTagValues = SplitString(kwstrTagValue, m_kstrTagValueSeparator, true);
    }
    return vecTagValues;
}