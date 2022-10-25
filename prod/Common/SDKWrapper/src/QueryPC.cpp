// QueryPC.cpp : Implementation of CQueryPC

#include "../include/stdafx.h"
#include <comutil.h>
#include "../platform/cesdk_loader.hpp"
#include "../platform/cesdk_attributes.hpp"
#include "../platform/policy_controller.hpp"
#include "../platform/cesdk_obligations.hpp"
#include "QueryPC.h"
#include "CEAttres.h"
#include "../platform/nlconfig.hpp"
#include "nlofficerep_only_debug.h"
#include "common_tools.h"
#include "TranslateJsonHelper.h"

#define MAX_NUM_REQUESTS (CE_MAX_NUM_REQUESTS - 50) // define the max requests number for multiple query.

STDMETHODIMP CRequest::set_action(BSTR strAction)
{
    if (NULL == strAction)
    {
        NLPRINT_DEBUGVIEWLOG(L"!!!!ERROR: parameter error, strAction is empty \n");
        return E_FAIL;
    }

    m_strAction = strAction;
    return S_OK;
}

STDMETHODIMP CRequest::set_param(BSTR strString,BSTR strStrType, ICEAttres* pAttres, LONG lType)
{
    switch(lType)
    {
        case 0: //< source
        {
            m_strSrc = (NULL == strString) ? L"" : strString;
            m_strSrcType = (NULL == strStrType) ? L"" : strStrType;
            m_srcAttres = pAttres;
            break;
        }
        case 1: //< destination
        {
            m_strDst = (NULL == strString) ? L"" : strString;
            m_strDstType = (NULL == strStrType) ? L"" : strStrType;
            m_dstAttres = pAttres;
            break;
        }
        case 2: //< name attributes
        {
            if (NULL != strString)
            {
                m_nameAttres[strString]=pAttres;
            }
            break;
        }
    }
    return S_OK;
}

STDMETHODIMP CRequest::set_user(BSTR strsid, BSTR strname, ICEAttres* pAttres)
{
    m_struid = (NULL == strsid) ? L"" : strsid;
    m_strname = (NULL == strname) ? L"" : strname;
    m_userAttres = pAttres;
    return S_OK;
}

STDMETHODIMP CRequest::set_app(BSTR strname, BSTR strpath, BSTR strurl,ICEAttres* pAttres)
{
    // TODO: Add your implementation code here
	NLPRINT_DEBUGVIEWLOG(L"request set app start \n");
    m_strApp = (NULL == strname) ? L"" : strname;
    m_strAppPath = (NULL == strpath) ? L"" : strpath;
    m_strUrl = (NULL == strurl) ? L"" : strurl;
    m_AppAttres = pAttres;
	NLPRINT_DEBUGVIEWLOG(L"request set app end \n");
    return S_OK;
}

STDMETHODIMP CRequest::set_recipient(BSTR strRecipient)
{
    // TODO: Add your implementation code here
    if ((NULL != strRecipient) && (L'\0' != strRecipient[0]))
    {
        m_vecRecips.push_back(strRecipient);
    }
    else
    {
        NLPRINT_DEBUGVIEWLOG(L"the recipient parameter is empty or NULL \n");
    }
    return S_OK;
}

STDMETHODIMP CRequest::set_performObligation(LONG lPerformOB)
{
    // TODO: Add your implementation code here
    m_lPerformOB = lPerformOB;
    return S_OK;
}

STDMETHODIMP CRequest::set_noiseLevel(LONG lNoiseLevel)
{
    // TODO: Add your implementation code here
    m_lNoiseLevel = lNoiseLevel;
    return S_OK;
}

// Query PC

STDMETHODIMP CQueryPC::get_cookie(LONG* lCookie)
{
    // TODO: Add your implementation code here
    CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
    if(m_dwCookie >= 10000)
    {
        // try to reuse the cookie id
        map<DWORD,QueryParam*>::iterator it;
        for(DWORD dwIndex=0;dwIndex<10000;dwIndex++)
        {
            it = m_mapQuest.find(dwIndex);
            if(it == m_mapQuest.end())
            {
                *lCookie = dwIndex;
                m_mapQuest[dwIndex]=NULL;
                break;
            }
        }
    }
    else
    {
        *lCookie = m_dwCookie++;
        m_mapQuest[*lCookie] = NULL;
    }
    return S_OK;
}

STDMETHODIMP CQueryPC::set_request(IRequest* pRequest,LONG lCookie)
{
    // TODO: Add your implementation code here
    CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
    map<DWORD,QueryParam*>::iterator it = m_mapQuest.find(lCookie);
    if(it != m_mapQuest.end())
    {
        QueryParam* pParm = it->second;
        if (pParm == NULL)
        {
            pParm = new QueryParam();
            pParm->nCookiD = lCookie;
        }
        pRequest->AddRef();
        pParm->m_vecReq.push_back(pRequest);
        m_mapQuest[lCookie] = pParm;
        return S_OK;
    }
    return E_FAIL;
}

// init sdk
bool CQueryPC::InitSDK()
{NLONLY_DEBUG
    CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
    if(m_hHandle != NULL)
    {
        return true;
    }

    bool bRet = false;
    if(!m_sdk.is_loaded())
    {
        wchar_t wszDir[MAX_PATH] = {0};
        if(NLConfig::ReadKey(L"SOFTWARE\\NextLabs\\CommonLibraries\\InstallDir", wszDir, MAX_PATH))
        {
#ifdef _WIN64   
            wcsncat_s(wszDir, MAX_PATH, L"\\bin64\\", _TRUNCATE);
#else
            wcsncat_s(wszDir, MAX_PATH, L"\\bin32\\", _TRUNCATE);
#endif

            NLPRINT_DEBUGVIEWLOG(L"install path:[%s] \n", wszDir);
            if (m_sdk.load(wszDir))
            {
                /** prepare user and application info for connect the policy control */
                //Get current login user info
                wchar_t wszSID[128] = {0};
                wchar_t wszName[128] = {0};
                GetUserInfo(wszSID, 128, wszName, 128);

                NLPRINT_DEBUGVIEWLOG(L"Local user name:[%s], user ID:[%d] \n", wszName, wszSID);
                CEUser ceUser = NLAllocCEUserInfo(wszName, wszSID);

                //Get current process info
                wchar_t wszAppPath[1024] = {0};
                wchar_t wszAppName[1024] = {0};
                GetModuleFileNameW(NULL, wszAppPath, 1024);
                if (wcslen(wszAppPath) > 0)
                {
                    wchar_t* pwszName = wcsrchr(wszAppPath, '\\');
                    if(NULL != pwszName)
                    {
                        wcsncpy_s(wszAppName, 1024, pwszName + 1, _TRUNCATE);
                    }
                }
                NLPRINT_DEBUGVIEWLOG(L"current application name:[%s], app path:[%s]\n", wszAppName, wszAppPath);
                CEApplication ceApp = NLAllocCEApplicationInfo(wszAppName, wszAppPath, L"");

				CEResult_t ret;
				int nWaitTimes = 0;
				ret = m_sdk.fns.CECONN_Initialize(ceApp, ceUser, NULL, &m_hHandle, 1000);
				while (ret == CE_RESULT_CONN_FAILED && nWaitTimes < 3)
				{
					nWaitTimes++;
					Sleep(50);
					ret = m_sdk.fns.CECONN_Initialize(ceApp, ceUser, NULL, &m_hHandle, 1000);					
				}
                if (ret == CE_RESULT_SUCCESS && m_hHandle != NULL)
                {
                    NLPRINT_DEBUGVIEWLOG(L"Init SDK success \n");
                    bRet = true;
                }
                else
                {
                    NLPRINT_DEBUGVIEWLOG(L"Init SDK failed: [%d] \n", ret);
                }

                /** free resource */;
                NLFreeUserInfo(&ceUser);
                NLFreeApplicationInfo(&ceApp);
            }
            else
            {
                NLPRINT_DEBUGVIEWLOG(L"Load cesdk or cesdk32 failed \n");
            }
        }
        else
        {
            NLPRINT_DEBUGVIEWLOG(L"read Install Key failed \n");
        }
    }
    else
    {
        NLPRINT_DEBUGVIEWLOG(L"load SDK failed \n");
    }
    return bRet;
}

STDMETHODIMP CQueryPC::check_resource(LONG lCookie, LONG lTimeout, LONG lNeedAssignObs, LONG* plResult)
{NLONLY_DEBUG
    /** initialize */
    HRESULT hr = E_FAIL;
    if (NULL != plResult)
    {
        *plResult = CEDontCare;
    }

    if (!InitSDK())
    {
        NLPRINT_DEBUGVIEWLOG(L"init SDK failed \n");
        return hr;
    }

    QueryParam* pParam = NULL;
    {
        CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
        map<DWORD,QueryParam*>::iterator it = m_mapQuest.find(lCookie);
        if(it != m_mapQuest.end())
        {
            pParam = it->second;
        }
    }

    if((pParam == NULL) || (pParam->m_vecReq.empty()))
    {
        NLPRINT_DEBUGVIEWLOG(L"Empty parameters \n");
        return hr;
    }

    NLPRINT_DEBUGVIEWLOG(L"policy numbers:[%d] \n", pParam->m_vecReq.size());
    for (std::vector<IRequest*>::iterator it = pParam->m_vecReq.begin(); it != pParam->m_vecReq.end(); ++it)
    {
        CRequest* pReq = dynamic_cast<CRequest*>(*it);
        if (pReq != NULL)
        {
            // query policy
            //m_sdk.fns.CEEVALUATE_CheckResources
            CEEnforcement_t theResult;
            if (CE_RESULT_SUCCESS == NLQueryPolicy(pReq, lTimeout, &theResult))
            {
                NLPRINT_DEBUGVIEWLOG(L"query policy success \n");
                hr = S_OK;

                if (NULL != plResult)
                {
                    *plResult = theResult.result;
                }

                PCResult *pResult = new PCResult();
                pResult->enfRes = theResult.result;

                if (NULL != theResult.obligation)
                {
                    if (lNeedAssignObs)
                    {
                        AssignObligations(pResult->vecObs, *theResult.obligation);
                    }
                    else
                    {
                        GetOrgObligations(pResult->vecObs, *theResult.obligation);
                    }
                }
                else
                {
                    NLPRINT_DEBUGVIEWLOG(L"the obligation is NULL \n");
                }
                pParam->m_vecResult.push_back(pResult);

                /** Free resource */
                m_sdk.fns.CEEVALUATE_FreeEnforcement(theResult);
            }
            else
            {
                NLPRINT_DEBUGVIEWLOG(L"query policy failed \n");
            }
        }
    }

    return hr;
}

STDMETHODIMP CQueryPC::check_resourceex(LONG lCookie, BSTR strPQL, LONG lIngoreBuiltinPolicy, LONG lTimeout, LONG lNeedAssignObs, LONG* plResult)
{NLONLY_DEBUG
    // TODO: Add your implementation code here
    HRESULT hr = E_FAIL;
    if(NULL != plResult)
    {
        *plResult = CEDontCare;
    }

    if (!InitSDK())
    {
        NLPRINT_DEBUGVIEWLOG(L"init SDK failed \n");
        return CE_RESULT_GENERAL_FAILED;
    }

    QueryParam* pParam = NULL;
    {
        CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
        map<DWORD,QueryParam*>::iterator iter = m_mapQuest.find(lCookie);
        if (iter != m_mapQuest.end())
        {
            pParam = iter->second;
        }
    }

    /** check request parameters */
    if ((pParam == NULL) || (pParam->m_vecReq.empty()))
    {
        NLPRINT_DEBUGVIEWLOG(L"Empty parameters \n");
        return hr;
    }

    // query policy
    //m_sdk.fns.CEEVALUATE_CheckResourcesEx
    size_t szReqNum = pParam->m_vecReq.size();
	// The number of requests in NLQueryPolicyEx is limited, I most query 200 requests one time in NLQueryPolicyEx.
	for(size_t szInd = 0; szInd <= (szReqNum - 1) / MAX_NUM_REQUESTS; ++szInd)
	{
		size_t szNum = (szReqNum - szInd * MAX_NUM_REQUESTS) > MAX_NUM_REQUESTS ? MAX_NUM_REQUESTS : (szReqNum - szInd * MAX_NUM_REQUESTS);
		vector<IRequest*> vecReq;
		for(size_t index = 0; index < szNum; ++index)
		{
			IRequest* req = (pParam->m_vecReq)[index + szInd * MAX_NUM_REQUESTS];
			vecReq.push_back(req);
		}

	    CEEnforcement_t* ptheResult = new CEEnforcement_t[szNum];
		NLPRINT_DEBUGVIEWLOG(L"Alloc  buffer, szNum:[%d] \n", szNum);

		/** Init */
		for (size_t szIndex = 0; szIndex < szNum; ++szIndex)
		{
			ptheResult[szIndex].result = CEDontCare;
			ptheResult[szIndex].obligation = NULL;
		}

		_CEResult_t ceFuncRet = NLQueryPolicyEx(vecReq, strPQL, lIngoreBuiltinPolicy, lTimeout, ptheResult);
		if (NULL != plResult)
		{
			*plResult = ceFuncRet;
		}

		if (CE_RESULT_SUCCESS == ceFuncRet)
		{
			hr = S_OK;
	        
			NLPRINT_DEBUGVIEWLOG(L"test success to check resource ex, szNum:[%d], ptheResult:[0x%x] \n", szNum, ptheResult);

			/** push result info */
			for (size_t szRes = 0; szRes < szNum; ++szRes)
			{
				PCResult *pResult = new PCResult();
				NLPRINT_DEBUGVIEWLOG(L"index:[%d], evlation result: %d \n", szRes, ptheResult[szRes].result);

                pResult->enfRes = ptheResult[szRes].result;

				/** store the obligations */
				if (NULL != ptheResult[szRes].obligation)
				{
					NLPRINT_DEBUGVIEWLOG(L"analysis the obbligations \n");

					if (lNeedAssignObs)
					{
						AssignObligations(pResult->vecObs, *(ptheResult[szRes].obligation));
					}
					else
					{
						GetOrgObligations(pResult->vecObs, *(ptheResult[szRes].obligation));
					}
				}
				else
				{
					NLPRINT_DEBUGVIEWLOG(L"The obligation is null \n");
				}
				pParam->m_vecResult.push_back(pResult);

				/** Free resource */
				m_sdk.fns.CEEVALUATE_FreeEnforcement(ptheResult[szRes]);
			}
		}
		else
		{
			NLPRINT_DEBUGVIEWLOG(L"query policy failed \n");
			break;
		}
		
		/** Free resource */
		delete[] ptheResult;
		ptheResult = NULL;
	}
    return hr;
}

STDMETHODIMP CQueryPC::check_resourceex_json(BSTR strRequest, BSTR *strResponse, LONG* plResult)
{
	if (strRequest == NULL || strResponse == NULL)
	{
		if (plResult != NULL)
		{
			*plResult = CE_RESULT_INVALID_PARAMS;
		}
		return E_FAIL;
	}
	//{
		//wstring wstr1 = strRequest;
		//NLPrintLogW(true, L"[JSONHELPER]BSTR Request[%d]:%s\n", wstr1.length(), wstr1.c_str());
	//}
	
	JsonHelper::CJsonHelperRequest jsonHelperRequest;
	wstring wstr = strRequest;
	bool succ = JsonHelper::CTranslateJsonHelper::TranslateToJsonHelperRequest(wstr, jsonHelperRequest);
	if (!succ)
	{
		if (plResult != NULL)
		{
			*plResult = CE_RESULT_INVALID_PARAMS;
		}
		NLPRINT_DEBUGVIEWLOG(L"[JSONHELPER]TranslateToJsonHelperRequest failed\n");
		return E_FAIL;
	}
	int total = jsonHelperRequest.MultiRequests.RequestReference.size();	
	if (total <= 0)
	{
		if (plResult != NULL)
		{
			*plResult = CE_RESULT_INVALID_PARAMS;
		}
		return E_FAIL;
	}

	if (!InitSDK())
	{
		if (plResult != NULL)
		{
			*plResult = CE_RESULT_CONN_FAILED;
		}
		NLPRINT_DEBUGVIEWLOG(L"init SDK failed \n");
		return E_FAIL;
	}

	JsonHelper::CJsonHelperResponse jsonHelperResponse;
	CEString cePQL = m_sdk.fns.CEM_AllocateString(L"");

	for (int i = 0; i <= (total - 1) / MAX_NUM_REQUESTS; ++i)
	{
		int num = (total - i * MAX_NUM_REQUESTS) > MAX_NUM_REQUESTS ? MAX_NUM_REQUESTS : (total - i * MAX_NUM_REQUESTS);

		size_t index = i * MAX_NUM_REQUESTS;
		JsonHelper::CRequestReference reqRef;
		CERequest *ceRequest = new CERequest[num]();
		CEEnforcement_t* ceEnforcement = new CEEnforcement_t[num]();
		for (int k = 0; k < num; ++k)
		{
			reqRef = jsonHelperRequest.MultiRequests.RequestReference[index++];			
			JsonHelper::CTranslateJsonHelper::TranslateToCERequest(jsonHelperRequest, reqRef, ceRequest[k], m_sdk.fns);

			ceEnforcement[k].result = CEDontCare;
			ceEnforcement[k].obligation = NULL;
		}
		
		_CEResult_t ceResult = m_sdk.fns.CEEVALUATE_CheckResourcesEx(m_hHandle, ceRequest, (CEint32)num, cePQL, CEFalse, 0, ceEnforcement, 10 * 6000);		  			
		if (ceResult != CE_RESULT_SUCCESS)
		{					
			NLPRINT_DEBUGVIEWLOG(L"CEEVALUATE_CheckResourcesEx failed[%d]", ceResult);
			JsonHelper::CTranslateJsonHelper::FreeResource(num, ceRequest, ceEnforcement, m_sdk.fns);
			if (plResult != NULL)
			{
				*plResult = CE_RESULT_GENERAL_FAILED;
			}
		}
		else
		{
			JsonHelper::CTranslateJsonHelper::TranslateToJsonHelperResponse(jsonHelperResponse, ceEnforcement, num, m_sdk.fns);
			JsonHelper::CTranslateJsonHelper::FreeResource(num, ceRequest, ceEnforcement, m_sdk.fns);
		}
		delete[] ceRequest;
		ceRequest = NULL;
		delete[] ceEnforcement;
		ceEnforcement = NULL;

		if (ceResult != CE_RESULT_SUCCESS)
		{
			break;
		}
	}
	m_sdk.fns.CEM_FreeString(cePQL);

	string response = JsonHelper::CTranslateJsonHelper::TranslateToJson(jsonHelperResponse);
	if (response.empty())
	{
		if (plResult != NULL)
		{
			*plResult = CE_RESULT_GENERAL_FAILED;
		}
		NLPRINT_DEBUGVIEWLOG(L"GetResponse failed\n");
		return E_FAIL;
	}	

	int length = (int)response.length();
	LPWSTR pBuf = new WCHAR[length + 1]();
	MultiByteToWideChar(CP_ACP, 0, (LPCSTR)response.c_str(), length, pBuf, length);
	*strResponse = ::SysAllocString(pBuf);
	delete[] pBuf;

	if (plResult != NULL)
	{
		*plResult = CE_RESULT_SUCCESS;
	}
	//{
	//	wstring pstr = *strResponse;
	//	NLPrintLogW(true, L"[JSONHELPER]alloc string addr:%s\n", pstr.c_str());
	//}
	
	return S_OK;
}

///////////////////////////////////About obligations//////////////////////////////////////////////
bool GetValue(__in WCHAR* (*pfn_CEM_GetString)(CEString),
    CEAttributes& obligations,
    __in const wchar_t* in_key,
    std::wstring& in_value)
{
    assert( pfn_CEM_GetString != NULL);
    assert( in_key != NULL );

    if((NULL == pfn_CEM_GetString)|| (NULL == in_key))
    {
        NLPRINT_DEBUGVIEWLOG(L"Parameter error \n");
        return false;
    }

    in_value.clear();
    for(int i = 0; i < obligations.count ; ++i)
    {
        const WCHAR* kpwszkey = pfn_CEM_GetString(obligations.attrs[i].key);
        if((NULL == kpwszkey) || (0 != wcscmp(kpwszkey,in_key)))
        {
            continue;
        }

        /* Extract value.  NULL value indicates empty which is handled by clearing
        * the input string before iteration.
        */
        const WCHAR* kpwszValue = pfn_CEM_GetString(obligations.attrs[i].value);
        if(NULL != kpwszValue)
        {
            in_value = kpwszValue;
        }

       // NLPRINT_DEBUGVIEWLOG(L"find: [%s::%s] \n", kpwszkey, kpwszValue);
        return true;
    }
    return false;
}/* GetValue */

bool CQueryPC::AssignObligations(vector<IObligation*>& pParam,CEAttributes& ob)
{NLONLY_DEBUG
    typedef WCHAR* (*GetString)(CEString);
    GetString pFunc = (GetString)m_sdk.fns.CEM_GetString;

    // get ob count
    wchar_t temp_key[128] = { 0 };
    swprintf(temp_key,_countof(temp_key),L"CE_ATTR_OBLIGATION_COUNT");
    wstring wsvalue=L"";
    if(!GetValue(pFunc,ob,temp_key,wsvalue))
    {
        NLPRINT_DEBUGVIEWLOG(L"Get value failed \n");
        return true;
    }

    int num_obs = _wtoi(wsvalue.c_str());
    NLPRINT_DEBUGVIEWLOG(L"obs number is:[%d],string :[%s] \n", num_obs, wsvalue.c_str());
    
    if(num_obs < 1)
    {
        NLPRINT_DEBUGVIEWLOG(L"no obs, return \n");
        return true;
    }

    CComPtr<IClassFactory> pObsClassFactory = NULL;
    HRESULT hr = CoGetClassObject(CLSID_Obligation, CLSCTX_INPROC_SERVER, 0, IID_IClassFactory, (void**)&pObsClassFactory);
    if (FAILED(hr) || (NULL == pObsClassFactory))
    {
        NLPRINT_DEBUGVIEWLOG(L"failed to get obligation factory \n");
        return false;
    }

    bool bResult = true;
    wstring strname = L"";
    wstring strpolicy = L"";
    for(int i = 1; i <= num_obs; ++i) /* Obligations [1,n] not [0,n-1] */
    {
        int num_values = 0;     /* Values to read from the obligation */

        swprintf(temp_key,_countof(temp_key),L"CE_ATTR_OBLIGATION_NAME:%d",i);           /* Name */
        bResult &= GetValue(pFunc,ob,temp_key,strname); 
        swprintf(temp_key,_countof(temp_key),L"CE_ATTR_OBLIGATION_POLICY:%d",i);         /* Policy */
        bResult &= GetValue(pFunc,ob,temp_key,strpolicy);

        NLPRINT_DEBUGVIEWLOG(L"policy attribute name:[%s],policy name:[%s] \n", strname.c_str(), strpolicy.c_str());

        swprintf(temp_key,_countof(temp_key),L"CE_ATTR_OBLIGATION_NUMVALUES:%d",i);      /* # of Values */
        bResult &= GetValue(pFunc,ob,temp_key,wsvalue);
        num_values = _wtoi(wsvalue.c_str());

        NLPRINT_DEBUGVIEWLOG(L"obs number is:[%d],string :[%s] \n", num_values, wsvalue.c_str());
        if(!bResult)
        {
            NLPRINT_DEBUGVIEWLOG(L"test 1\n");
            continue;
        }

        CObligation* pOb = NULL;
        hr = pObsClassFactory->CreateInstance(NULL, IID_IObligation, (void**)&pOb);
        if(FAILED(hr) || (NULL == pOb))
        {
            NLPRINT_DEBUGVIEWLOG(L"hr:[0x%x], pOb:[0x%x]\n", hr, pOb);
            continue;
        }

        /********************************************************************************
        * Extract Values or "options" of the obligation in pairs {key,value}
        *******************************************************************************/
        bResult = true;
        wstring strpname,strpvalue;
        hr = pOb->m_ceAttres.CoCreateInstance(CLSID_CEAttres,NULL,CLSCTX_ALL);
        if(FAILED(hr))
        {
            NLPRINT_DEBUGVIEWLOG(L"test 3\n");
            pOb->Release();
            continue;
        }

        for( int j = 1 ; j <= num_values ; j += 2 ) /* Obligations [1,n] not [0,n-1] */
        {
            swprintf(temp_key,_countof(temp_key),L"CE_ATTR_OBLIGATION_VALUE:%d:%d",i,j);    /* Key */
            GetValue(pFunc,ob,temp_key,strpname);
            swprintf(temp_key,_countof(temp_key),L"CE_ATTR_OBLIGATION_VALUE:%d:%d",i,j+1);  /* Value */
            GetValue(pFunc,ob,temp_key,strpvalue);
            pOb->m_ceAttres->add_attre(_bstr_t(strpname.c_str()),_bstr_t(strpvalue.c_str()));
        }/* for j */

        pOb->m_strName = strname;
        pOb->m_strPolicy = strpolicy;
        pParam.push_back((IObligation*)pOb);
    }/* for i */
    return bResult;
}

bool CQueryPC::GetOrgObligations(vector<IObligation*>& pParam,CEAttributes& ob)
{NLONLY_DEBUG

    NLPRINT_DEBUGVIEWLOG(L"count is:[%d] \n", ob.count);
    if (1 > ob.count)
    {
        NLPRINT_DEBUGVIEWLOG(L"obligation count is zero, no obligations. \n");
        return true;
    }

    bool bRet = false;
    CComPtr<IClassFactory> pObsClassFactory = NULL;
    HRESULT hr = CoGetClassObject(CLSID_Obligation, CLSCTX_INPROC_SERVER, 0, IID_IClassFactory, (void**)&pObsClassFactory);
    if (FAILED(hr) || (NULL == pObsClassFactory))
    {
        NLPRINT_DEBUGVIEWLOG(L"failed to get obligation factory \n");
        return bRet;
    }

    CObligation* pOb = NULL;
    hr = pObsClassFactory->CreateInstance(NULL, IID_IObligation, (void**)&pOb);
    if(FAILED(hr) || (NULL == pOb))
    {
        NLPRINT_DEBUGVIEWLOG(L"hr:[0x%x], pOb:[0x%x]\n", hr, pOb);
        return bRet;
    }

    hr = pOb->m_ceAttres.CoCreateInstance(CLSID_CEAttres,NULL,CLSCTX_ALL);
    if(FAILED(hr) || (NULL == (pOb->m_ceAttres)))
    {
        NLPRINT_DEBUGVIEWLOG(L"failed to get the CEAttres interface, hr:[0x%x], ceAttres:[0x%x]\n", hr, pOb->m_ceAttres);
        pOb->Release();
        return bRet;
    }

    for (CEint32 ceIndex = 0; ceIndex < ob.count; ++ceIndex)
    {
        const TCHAR* kpKey   = m_sdk.fns.CEM_GetString(ob.attrs[ceIndex].key);
        const TCHAR* kpValue = m_sdk.fns.CEM_GetString(ob.attrs[ceIndex].value);

        NLPRINT_DEBUGVIEWLOG(L"key is:[%s], value is: [%s] \n", kpKey, kpValue);

        pOb->m_ceAttres->add_attre(_bstr_t((NULL == kpKey) ? L"" : kpKey),_bstr_t((NULL == kpValue) ? L"" : kpValue));
    }

    pOb->m_strName   = L"";
    pOb->m_strPolicy = L"";
    pParam.push_back((IObligation*)pOb);

    return bRet;
}

STDMETHODIMP CQueryPC::get_obligation(LONG lCookie, BSTR strOBName, LONG nIndexOfResults, LONG nIndexOfOb, IObligation** pObligation)
{NLONLY_DEBUG
    // TODO: Add your implementation code here
    /** check parameter */
    if (NULL == pObligation)
    {
        return E_FAIL;
    }
    else
    {
        *pObligation = NULL;
    }

    QueryParam* pParam = NULL;
    {
        CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
        map<DWORD,QueryParam*>::iterator iter = m_mapQuest.find(lCookie);
        if(iter != m_mapQuest.end())
        {
            pParam = iter->second;
        }
    }

    if(NULL != pParam)
    {
#pragma warning(push)
#pragma warning(disable:4018) //< '>=' signed/unsigned mismatch
        if (nIndexOfResults >= pParam->m_vecResult.size())
        {
            NLPRINT_DEBUGVIEWLOG(L"result index error \n");
            return E_FAIL;
        }
#pragma warning(pop)

        PCResult* pResult = pParam->m_vecResult[nIndexOfResults];
        if ((NULL != pResult) && (!pResult->vecObs.empty()))
        {
            if ((NULL == strOBName) || (L'\0' == strOBName[0]))
            {
                /** NULL point or empty ob name, return the first one */
                NLPRINT_DEBUGVIEWLOG(L"pass an empty obname and retrun the first obligation object \n");
                *pObligation = pResult->vecObs[0];
				(*pObligation)->AddRef();
            }
            else
            {
				size_t nRef = 0;
                for (size_t i = 0; i < pResult->vecObs.size(); ++i)
                {
                    IObligation* pOb = pResult->vecObs[i];
                    BSTR strname = NULL;
                    HRESULT hr = pOb->get_name(&strname);
                    if (SUCCEEDED(hr) && (NULL != strname))
                    {
                        std::wstring wstrTempName = strname;
                        std::wstring wstrTempObName = strOBName;
                        if (_wcsicmp(wstrTempName.c_str(), wstrTempObName.c_str()) == 0)
                        {
							if (nRef++ == nIndexOfOb)
							{
								::SysFreeString(strname);
								pOb->AddRef();
								*pObligation = pOb;
								return S_OK;
							}
                        }
                        ::SysFreeString(strname);
                    }
                }
            }
        }
    }
    return S_FALSE;
}

STDMETHODIMP CQueryPC::get_all_obligations(LONG lCookie, LONG nIndexOfResults, IObligation** pObligations, LONG lObsNum)
{NLONLY_DEBUG
    HRESULT hr = E_FAIL;
    
    /** check parameters */
    if ((NULL == pObligations) || (0 >= lObsNum))
    {
        NLPRINT_DEBUGVIEWLOG(L"parameters error \n");
        return hr;
    }

    QueryParam* pParam = NULL;
    {
        CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
        map<DWORD,QueryParam*>::iterator iter = m_mapQuest.find(lCookie);
        if(iter != m_mapQuest.end())
        {
            pParam = iter->second;
        }
    }

    if(NULL != pParam)
    {
#pragma warning(push)
#pragma warning(disable:4018) //< '>=' signed/unsigned mismatch
        if (nIndexOfResults >= pParam->m_vecResult.size())
        {
            NLPRINT_DEBUGVIEWLOG(L"result index error \n");
            return E_FAIL;
        }
#pragma warning(pop)

        PCResult* pResult = pParam->m_vecResult[nIndexOfResults];
        if (NULL != pResult)
        {
            if (lObsNum >= (LONG)pResult->vecObs.size())
            {
#if 1
                {
                    NLPRINT_DEBUGVIEWLOG(L"cookie number:[%d], result index number:[%d], obs number: [%d]\n", lCookie, nIndexOfResults, pResult->vecObs.size());
                    for (size_t szDebIndex = 0; szDebIndex < pResult->vecObs.size(); ++szDebIndex)
                    {
                        IObligation* pOb = pResult->vecObs[szDebIndex];

                        {
                            /** obligation info */
                            NLPRINT_DEBUGVIEWLOG(L"************* Obligaiton info begin ********** \n");
                            ATL::CComBSTR comObsName(L"");
                            pOb->get_name(&comObsName);
                            std::wstring wstrObsName = comObsName;

                            ATL::CComBSTR comPolicy(L"");
                            pOb->get_policyname(&comPolicy);
                            std::wstring wstrPolicyName = comPolicy;

                            NLPRINT_DEBUGVIEWLOG(L"Obs index:[%s], Obs name:[%s], Policy name:[%s] \n", szDebIndex, wstrObsName.c_str(), wstrPolicyName.c_str());

                            ICEAttres* pAttres = NULL;
                            HRESULT hr = pOb->get_attres(&pAttres);
                            if (SUCCEEDED(hr) && (NULL != pAttres))
                            {
                                LONG lCount = 0;
                                pAttres->get_count(&lCount);
                                for (LONG lDebIndex = 0; lDebIndex < lCount; ++lDebIndex)
                                {
                                    CComBSTR comBstrKey(L"");
                                    CComBSTR comBstrValue(L"");
                                    hr = pAttres->get_attre(lDebIndex, &comBstrKey, &comBstrValue);

                                    std::wstring wstrKey = (NULL == comBstrKey) ? L"" : comBstrKey;
                                    std::wstring wstrValue = (NULL == comBstrValue) ? L"" : comBstrValue;
                                    NLPRINT_DEBUGVIEWLOG(L"Attribute: index=[%d], key=[%s], value=[%s], hr=[%x] \n", lDebIndex, wstrKey.c_str(), wstrValue.c_str(), hr);
                                }
                            }
                            NLPRINT_DEBUGVIEWLOG(L"************* Obligaiton info end ********** \n");
                        }
                    }
                }
#endif
                for (size_t i = 0; i < pResult->vecObs.size(); ++i)
                {
                    pObligations[i] = pResult->vecObs[i];
                    pObligations[i]->AddRef();
                }
                hr = S_OK;
            }
            else
            {
                NLPRINT_DEBUGVIEWLOG(L"Error, the buffer is too small \n");
            }
        }
    }

    return hr;
}

/////////////////////////////////////////////////////////////////////////////////

STDMETHODIMP CQueryPC::get_result(LONG lCookie, LONG nIndexOfResults, LONG* plResult, LONG* plObsNum)
{
    /** check parameter and initialize them */
    if (NULL != plObsNum)
    {
        *plObsNum = 0;
    }

    if (NULL == plResult)
    {
        return E_FAIL;
    }
    else
    {
        *plResult = 1;   // default , allow
    }

    QueryParam* pParam = NULL;
    {
        CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
        map<DWORD, QueryParam*>::iterator iter = m_mapQuest.find(lCookie);
        if (iter != m_mapQuest.end())	pParam = iter->second;
    }

    if(pParam != NULL)
    {
        if(nIndexOfResults < (LONG)pParam->m_vecResult.size())
        {
            *plResult = pParam->m_vecResult[nIndexOfResults]->enfRes;
            if (NULL != plObsNum)
            {
                *plObsNum = pParam->m_vecResult[nIndexOfResults]->vecObs.size();
            }
        }
    }
    return S_OK;
}

STDMETHODIMP CQueryPC::release_request(LONG lCookie)
{
    // TODO: Add your implementation code here
    QueryParam* pPam = NULL;
    {
        CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
        map<DWORD,QueryParam*>::iterator it = m_mapQuest.find(lCookie);
        if(it != m_mapQuest.end())
        {
            pPam = it->second;
            m_mapQuest.erase(it);
        }
    }
    if(pPam != NULL)	delete pPam;
    return S_OK;
}

_CEResult_t CQueryPC::NLQueryPolicy(_In_ const CRequest* pReq, _In_ const CEint32& ceTimeoutInMillisec, _Out_ CEEnforcement_t* enforcement)
{NLONLY_DEBUG
    /** check parameter */
    if (NULL == pReq)
    {
        NLPRINT_DEBUGVIEWLOG(L"Query policy parameter error \n");
        return CE_RESULT_INVALID_PARAMS;
    }

    /** prepare parameters */
    CEString     ceOperation    = m_sdk.fns.CEM_AllocateString(pReq->m_strAction.c_str());
    CEResource*  pceSrcResource = m_sdk.fns.CEM_CreateResourceW(pReq->m_strSrc.c_str(), pReq->m_strSrcType.c_str());
    CEAttributes ceSrcAttributes = NLGetAttributes(pReq->m_srcAttres);

    CEResource*  pceDesResource = m_sdk.fns.CEM_CreateResourceW(pReq->m_strDst.c_str(), pReq->m_strDstType.c_str());
    CEAttributes ceDesAttributes = NLGetAttributes(pReq->m_dstAttres);

    CEUser ceUser = { 0 };
    ceUser.userName = m_sdk.fns.CEM_AllocateString(pReq->m_strname.c_str());
	if (!pReq->m_struid.empty())
	{
		ceUser.userID = m_sdk.fns.CEM_AllocateString(pReq->m_struid.c_str());
	}
    CEAttributes ceUserAttributes = NLGetAttributes(pReq->m_userAttres);

    CEApplication ceApplication = { 0 };
    ceApplication.appName = m_sdk.fns.CEM_AllocateString(pReq->m_strApp.c_str());
    ceApplication.appPath = m_sdk.fns.CEM_AllocateString(pReq->m_strAppPath.c_str());
    ceApplication.appURL  = m_sdk.fns.CEM_AllocateString(pReq->m_strUrl.c_str());
    CEAttributes ceAppAttributes = NLGetAttributes(pReq->m_AppAttres);

    CEint32 ceRecipNum = pReq->m_vecRecips.size();
    CEString* pceRecipients = new CEString[ceRecipNum];
    for (CEint32 i = 0; i < ceRecipNum; ++i)
    {
        pceRecipients[i] = m_sdk.fns.CEM_AllocateString(pReq->m_vecRecips[i].c_str());
    }

    CEint32 ceIpNum = 0;

    /** CEFalse: ignore the obligations */
    CEBoolean cePerformObligation = (0 == pReq->m_lPerformOB) ? CEFalse : CETrue;

    CENoiseLevel_t ceNoiseLevel = static_cast<CENoiseLevel_t>(pReq->m_lNoiseLevel);

    _CEResult_t ceResult = m_sdk.fns.CEEVALUATE_CheckResources(m_hHandle,
                                        ceOperation,
                                        pceSrcResource,
                                        &ceSrcAttributes,
                                        pceDesResource,
                                        &ceDesAttributes,
                                        &ceUser,
                                        &ceUserAttributes,
                                        &ceApplication,
                                        &ceAppAttributes,
                                        pceRecipients,
                                        ceRecipNum,
                                        ceIpNum,
                                        cePerformObligation,
                                        ceNoiseLevel,
                                        enforcement,
                                        ceTimeoutInMillisec
                                        );

    NLPRINT_DEBUGVIEWLOG(L"Query policy result:[%d] \n", ceResult);

    /** free resource */
    NLFreeCEString(&ceOperation);

    NLFreeCEResource(pceSrcResource);
    NLFreeAttributes(&ceSrcAttributes);

    NLFreeCEResource(pceDesResource);
    NLFreeAttributes(&ceDesAttributes);
    
    NLFreeUserInfo(&ceUser);
    NLFreeAttributes(&ceUserAttributes);
    
    NLFreeApplicationInfo(&ceApplication);
    NLFreeAttributes(&ceAppAttributes);

    NLFreeCEStrings(&pceRecipients, ceRecipNum);
    delete[] pceRecipients;
    pceRecipients = NULL;

    return ceResult;
}

_CEResult_t CQueryPC::NLQueryPolicyEx(_In_ const CRequest* pReq, _In_ const BSTR bstrPQL, LONG lIngoreBuiltinPolicy, _In_ const CEint32& ceTimeoutInMillisec, _Out_ CEEnforcement_t* ceEnforcement)
{
    /** check parameter */
    if (NULL == pReq)
    {
        NLPRINT_DEBUGVIEWLOG(L"Query policy ex parameter error \n");
        return CE_RESULT_INVALID_PARAMS;
    }

    /** prepare parameters */
    CEString     ceOperation = m_sdk.fns.CEM_AllocateString(pReq->m_strAction.c_str());

    CEResource*  pceSrcResource = m_sdk.fns.CEM_CreateResourceW(pReq->m_strSrc.c_str(), pReq->m_strSrcType.c_str());
    CEAttributes ceSrcAttributes = NLGetAttributes(pReq->m_srcAttres);

    CEResource*  pceDesResource = m_sdk.fns.CEM_CreateResourceW(pReq->m_strDst.c_str(), pReq->m_strDstType.c_str());
    CEAttributes ceDesAttributes = NLGetAttributes(pReq->m_dstAttres);

    CEUser ceUser = { 0 };
    ceUser.userName = m_sdk.fns.CEM_AllocateString(pReq->m_strname.c_str());
	if (!pReq->m_struid.empty())
	{
		ceUser.userID = m_sdk.fns.CEM_AllocateString(pReq->m_struid.c_str());
	}
    CEAttributes ceUserAttributes = NLGetAttributes(pReq->m_userAttres);

    CEApplication ceApplication = { 0 };
    ceApplication.appName = m_sdk.fns.CEM_AllocateString(pReq->m_strApp.c_str());
    ceApplication.appPath = m_sdk.fns.CEM_AllocateString(pReq->m_strAppPath.c_str());
    ceApplication.appURL = m_sdk.fns.CEM_AllocateString(pReq->m_strUrl.c_str());
    CEAttributes ceAppAttributes = NLGetAttributes(pReq->m_AppAttres);

    CEint32 ceRecipNum = pReq->m_vecRecips.size();
    CEString* pceRecipients = new CEString[ceRecipNum];
    for (CEint32 i = 0; i < ceRecipNum; ++i)
    {
        pceRecipients[i] = m_sdk.fns.CEM_AllocateString(pReq->m_vecRecips[i].c_str());
    }

    /** prepare name attributes */
    CENamedAttributes* pceNameAttributes = NULL;
    CEint32 ceNameAttributesNum = pReq->m_nameAttres.size();
    if (0 < ceNameAttributesNum)
    {
        pceNameAttributes = new CENamedAttributes[ceNameAttributesNum];
        for (std::map<wstring, CComPtr<ICEAttres>>::const_iterator kit = pReq->m_nameAttres.begin(); kit != pReq->m_nameAttres.end(); ++kit)
        {
            pceNameAttributes->name  = m_sdk.fns.CEM_AllocateString(kit->first.c_str());
            pceNameAttributes->attrs = NLGetAttributes(kit->second);
        }
    }

    CENoiseLevel_t ceNoiseLevel = static_cast<CENoiseLevel_t>(pReq->m_lNoiseLevel);

    /** CEFalse: ignore the obligations */
    CEBoolean cePerformObligation = (0 == pReq->m_lPerformOB) ? CEFalse : CETrue;

    CERequest ceRequest = { ceOperation,
                            pceSrcResource,
                            &ceSrcAttributes,
                            pceDesResource,
                            &ceDesAttributes,
                            &ceUser,
                            &ceUserAttributes,
                            &ceApplication,
                            &ceAppAttributes,
                            pceRecipients,
                            ceRecipNum,
                            pceNameAttributes,
                            ceNameAttributesNum,
                            cePerformObligation,
                            ceNoiseLevel
                          };
    CEint32 ceIpNum = 0;

    CEBoolean ceIgnoreBuiltinPolicies = (0 == lIngoreBuiltinPolicy) ? CEFalse : CETrue;

    std::wstring wstrPQL = (NULL == bstrPQL) ? L"" : bstrPQL;;
    CEString cePQL = m_sdk.fns.CEM_AllocateString(wstrPQL.c_str());

    _CEResult_t ceResult = m_sdk.fns.CEEVALUATE_CheckResourcesEx(m_hHandle,
                                                 &ceRequest,
                                                 1,
                                                 cePQL,
                                                 ceIgnoreBuiltinPolicies,
                                                 ceIpNum,
                                                 ceEnforcement,
                                                 ceTimeoutInMillisec
                                                 );

    NLPRINT_DEBUGVIEWLOG(L"Query policy result:[%d] \n", ceResult);

    /** free resource */
    NLFreeCEString(&ceOperation);

    NLFreeCEResource(pceSrcResource);
    NLFreeAttributes(&ceSrcAttributes);

    NLFreeCEResource(pceDesResource);
    NLFreeAttributes(&ceDesAttributes);

    NLFreeUserInfo(&ceUser);
    NLFreeAttributes(&ceUserAttributes);

    NLFreeApplicationInfo(&ceApplication);
    NLFreeAttributes(&ceAppAttributes);

    NLFreeCEStrings(&pceRecipients, ceRecipNum);
    delete[] pceRecipients;
    pceRecipients = NULL;

    NLFreeCENameAttrs(&pceNameAttributes, ceNameAttributesNum);

    return ceResult;
}

_CEResult_t CQueryPC::NLQueryPolicyEx(_In_ std::vector<IRequest*>& vecpRep, _In_ const BSTR bstrPQL, LONG lIngoreBuiltinPolicy, _In_ const CEint32& ceTimeoutInMillisec, _Out_ CEEnforcement_t* ceEnforcement)
{NLONLY_DEBUG
    size_t szNumRequests = vecpRep.size();
    CERequest*  pceRequest = new CERequest[szNumRequests];

    /** Init */
    for (size_t szIndex = 0; szIndex < szNumRequests; ++szIndex)
    {
        ::memset(&pceRequest[szIndex], 0, sizeof(pceRequest[szIndex]));
    }

    for (size_t szNum = 0; szNum < szNumRequests; ++szNum)
    {
        CRequest* pReq = dynamic_cast<CRequest*>(vecpRep[szNum]);
        
        /** check parameter */
        if ((NULL == pReq))
        {
            ::OutputDebugStringW(L"Query policy ex parameter error \n");
            return CE_RESULT_INVALID_PARAMS;
        }

        /** prepare parameters */
        pceRequest[szNum].operation = m_sdk.fns.CEM_AllocateString(pReq->m_strAction.c_str());
        NLPRINT_DEBUGVIEWLOG(L"Index:[%d], action:[%s] \n", szNum, pReq->m_strAction.c_str());

        pceRequest[szNum].source = m_sdk.fns.CEM_CreateResourceW(pReq->m_strSrc.c_str(), pReq->m_strSrcType.c_str());
        NLPRINT_DEBUGVIEWLOG(L"Index:[%d], src file name:[%s], type:[%s] \n", szNum, pReq->m_strSrc.c_str(), pReq->m_strSrcType.c_str());

        pceRequest[szNum].sourceAttributes = new CEAttributes;
        *(pceRequest[szNum].sourceAttributes) = NLGetAttributes(pReq->m_srcAttres);

        pceRequest[szNum].target = m_sdk.fns.CEM_CreateResourceW(pReq->m_strDst.c_str(), pReq->m_strDstType.c_str());
        NLPRINT_DEBUGVIEWLOG(L"Index:[%d], des file name:[%s], type:[%s] \n", szNum, pReq->m_strDst.c_str(), pReq->m_strDstType.c_str());

        pceRequest[szNum].targetAttributes = new CEAttributes;
        *(pceRequest[szNum].targetAttributes) = NLGetAttributes(pReq->m_dstAttres);

        pceRequest[szNum].user = new CEUser;
		memset(pceRequest[szNum].user, 0, sizeof(CEUser));
        pceRequest[szNum].user->userName = m_sdk.fns.CEM_AllocateString(pReq->m_strname.c_str());
		if (!pReq->m_struid.empty())
		{
			pceRequest[szNum].user->userID = m_sdk.fns.CEM_AllocateString(pReq->m_struid.c_str());
		}
        pceRequest[szNum].userAttributes = new CEAttributes;
        *(pceRequest[szNum].userAttributes) = NLGetAttributes(pReq->m_userAttres);

        pceRequest[szNum].app = new CEApplication;
        pceRequest[szNum].app->appName = m_sdk.fns.CEM_AllocateString(pReq->m_strApp.c_str());
        pceRequest[szNum].app->appPath = m_sdk.fns.CEM_AllocateString(pReq->m_strAppPath.c_str());
        pceRequest[szNum].app->appURL = m_sdk.fns.CEM_AllocateString(pReq->m_strUrl.c_str());
        pceRequest[szNum].appAttributes = new CEAttributes;
        *(pceRequest[szNum].appAttributes) = NLGetAttributes(pReq->m_AppAttres);

        pceRequest[szNum].numRecipients = pReq->m_vecRecips.size();
        pceRequest[szNum].recipients = NULL;
        if (0 < pceRequest[szNum].numRecipients)
        {
            pceRequest[szNum].recipients = new CEString[pceRequest[szNum].numRecipients];
            for (CEint32 i = 0; i < pceRequest[szNum].numRecipients; ++i)
            {
                pceRequest[szNum].recipients[i] = m_sdk.fns.CEM_AllocateString(pReq->m_vecRecips[i].c_str());
            }
        }

        /** prepare name attributes */
        pceRequest[szNum].numAdditionalAttributes = pReq->m_nameAttres.size();
        if (0 < pceRequest[szNum].numAdditionalAttributes)
        {
            pceRequest[szNum].additionalAttributes = new CENamedAttributes[pceRequest[szNum].numAdditionalAttributes];
            int nNameIndex = 0;
            for (std::map<wstring, CComPtr<ICEAttres>>::const_iterator kit = pReq->m_nameAttres.begin(); (nNameIndex < pceRequest[szNum].numAdditionalAttributes) && (kit != pReq->m_nameAttres.end()); ++kit, ++nNameIndex)
            {
                pceRequest[szNum].additionalAttributes[nNameIndex].name = m_sdk.fns.CEM_AllocateString(kit->first.c_str());
                pceRequest[szNum].additionalAttributes[nNameIndex].attrs = NLGetAttributes(kit->second);
            }
        }

        pceRequest[szNum].noiseLevel = static_cast<CENoiseLevel_t>(pReq->m_lNoiseLevel);

        /** CEFalse: ignore the obligations */
        pceRequest[szNum].performObligation = (0 == pReq->m_lPerformOB) ? CEFalse : CETrue;
    }
    
    CEint32 ceIpNum = 0;
    CEBoolean ceIgnoreBuiltinPolicies = (0 == lIngoreBuiltinPolicy) ? CEFalse : CETrue;
    std::wstring wstrPQL = (NULL == bstrPQL) ? L"" : bstrPQL;
    CEString cePQL = m_sdk.fns.CEM_AllocateString(wstrPQL.c_str());

    _CEResult_t ceResult = m_sdk.fns.CEEVALUATE_CheckResourcesEx(m_hHandle,
                                                                 pceRequest,
                                                                 szNumRequests,
                                                                 cePQL,
                                                                 ceIgnoreBuiltinPolicies,
                                                                 ceIpNum,
                                                                 ceEnforcement,
                                                                 ceTimeoutInMillisec
                                                                 );

    NLPRINT_DEBUGVIEWLOG(L"Query policy result:[%d], request number:[%d] \n", ceResult, szNumRequests);

    /** free resource */
    for (size_t i = 0; i < szNumRequests; ++i)
    {
        NLPRINT_DEBUGVIEWLOG(L"------- 1: [%d] \n", i);
        NLFreeCEString(&pceRequest[i].operation);

        NLPRINT_DEBUGVIEWLOG(L"------- 2: [%d] \n", i);
        NLFreeCEResource(pceRequest[i].source);

        NLPRINT_DEBUGVIEWLOG(L"------- 3: [%d] \n", i);
        NLFreeAttributes(pceRequest[i].sourceAttributes);
        delete pceRequest[i].sourceAttributes;
        pceRequest[i].sourceAttributes = NULL;

        NLPRINT_DEBUGVIEWLOG(L"------- 4: [%d] \n", i);
        NLFreeCEResource(pceRequest[i].target);

        NLPRINT_DEBUGVIEWLOG(L"------- 5: [%d] \n", i);
        NLFreeAttributes(pceRequest[i].targetAttributes);
        delete pceRequest[i].targetAttributes;
        pceRequest[i].targetAttributes = NULL;

        NLPRINT_DEBUGVIEWLOG(L"------- 6: [%d] \n", i);
        NLFreeUserInfo(pceRequest[i].user);
        delete pceRequest[i].user;
        pceRequest[i].user = NULL;

        NLPRINT_DEBUGVIEWLOG(L"------- 7: [%d] \n", i);
        NLFreeAttributes(pceRequest[i].userAttributes);
        delete pceRequest[i].userAttributes;
        pceRequest[i].userAttributes = NULL;

        NLPRINT_DEBUGVIEWLOG(L"------- 8: [%d] \n", i);
        NLFreeApplicationInfo(pceRequest[i].app);
        delete pceRequest[i].app;
        pceRequest[i].app = NULL;

        NLPRINT_DEBUGVIEWLOG(L"------- 9: [%d] \n", i);
        NLFreeAttributes(pceRequest[i].appAttributes);
        delete pceRequest[i].appAttributes;
        pceRequest[i].appAttributes = NULL;


        NLPRINT_DEBUGVIEWLOG(L"------- 10: [%d] \n", i);
        NLFreeCEStrings(&pceRequest[i].recipients, pceRequest[i].numRecipients);

        NLPRINT_DEBUGVIEWLOG(L"------- 10:01: [%d], [0x%x], num:[%d] \n", i, pceRequest[i].recipients, pceRequest[i].numRecipients);
        delete[] pceRequest[i].recipients;
        pceRequest[i].recipients = NULL;

        NLPRINT_DEBUGVIEWLOG(L"------- 11: [%d] \n", i);
        NLFreeCENameAttrs(&pceRequest[i].additionalAttributes, pceRequest[i].numAdditionalAttributes);
        delete pceRequest[i].additionalAttributes;
        pceRequest[i].additionalAttributes = NULL;
    }

    delete[] pceRequest;
    pceRequest = NULL;

    return ceResult;
}

///////////////////////////////Tools///////////////////////////////////////////
void CQueryPC::NLFreeAttributes(_Inout_ CEAttributes* pCeAttributes)
{
    /** check parameters */
    if (NULL != pCeAttributes)
    {
        for (LONG i = 0; i < pCeAttributes->count; ++i)
        {
            NLFreeCEString(&(pCeAttributes->attrs[i].key));
            NLFreeCEString(&(pCeAttributes->attrs[i].value));
        }
    }
}

CEAttributes CQueryPC::NLGetAttributes(_In_ const CComPtr<ICEAttres> pICEAttributes)
{NLONLY_DEBUG
    /** initialize */
    CEAttributes ceAttributes = { 0 };

    /*
    *\ for local path or unc path ,we just disable PC  reading tag.
    * for http or ftp or other network path, we will disable pc reading tag and reading content.
    * disable read tag, just set "ce::file_custom_attributes_included = yes"
    * disable read tag/content, set "ce::filesystemcheck=no"
    */
	/*for  "ce::filesystemcheck", if it is  set by SDKWrapper caller, we keep its value, if not, we default it for "no"*/
    static const wchar_t* cstr_disablereadtagname = L"ce::file_custom_attributes_included";
    static const wchar_t* cstr_disablereadtagcontentname = L"ce::filesystemcheck";

    /** check parameters */
    if (NULL != pICEAttributes)
    {
        LONG lAttrCount = 0;
        pICEAttributes->get_count(&lAttrCount);
        ceAttributes.count = lAttrCount+1;
        NLPRINT_DEBUGVIEWLOG(L"attributes count:[%d] \n", lAttrCount);

        ceAttributes.attrs = new CEAttribute[lAttrCount+1];

		bool bAlreadSetDisableReadtagContentName = false;
		std::wstring strValueOfDisableReadTagContent = L"no";

        for (LONG i = 0; i < lAttrCount; ++i)
        {
            CComBSTR comBstrKey(L"");
            CComBSTR comBstrValue(L"");
            pICEAttributes->get_attre(i, &comBstrKey, &comBstrValue);

            std::wstring wstrKey = (NULL == comBstrKey) ? L"" : comBstrKey;
            std::wstring wstrValue = (NULL == comBstrValue) ? L"" : comBstrValue;

            NLPRINT_DEBUGVIEWLOG(L"Attributes key:[%s], value:[%s] \n", wstrKey.c_str(), wstrValue.c_str());
            ceAttributes.attrs[i].key = m_sdk.fns.CEM_AllocateString(wstrKey.c_str());
            ceAttributes.attrs[i].value = m_sdk.fns.CEM_AllocateString(wstrValue.c_str());

			if (_wcsicmp(wstrKey.c_str(), cstr_disablereadtagcontentname) == 0)
			{
				bAlreadSetDisableReadtagContentName = true;
				strValueOfDisableReadTagContent = wstrValue;
			}

        }
        
		ceAttributes.attrs[lAttrCount].key = m_sdk.fns.CEM_AllocateString(cstr_disablereadtagcontentname);
		if (bAlreadSetDisableReadtagContentName)
		{
			ceAttributes.attrs[lAttrCount].value = m_sdk.fns.CEM_AllocateString(strValueOfDisableReadTagContent.c_str());
		}
		else
		{
			ceAttributes.attrs[lAttrCount].value = m_sdk.fns.CEM_AllocateString(L"no");
		}

    }
    return ceAttributes;
}

CEString CQueryPC::NLAllocCEString(_In_ const std::wstring& wstrString)
{
    return m_sdk.fns.CEM_AllocateString(wstrString.c_str());
}

void CQueryPC::NLFreeCEString(_Inout_ CEString* pceString)
{
    if (NULL != pceString)
    {
        m_sdk.fns.CEM_FreeString(*pceString);
    }
}

CEResource* CQueryPC::NLAllocCEResource(_In_ const std::wstring& wstrResource, _In_ const std::wstring& wstrResourceType)
{
    return m_sdk.fns.CEM_CreateResourceW(wstrResource.c_str(), wstrResourceType.c_str());
}

void CQueryPC::NLFreeCEResource(_Inout_ CEResource* pceResource)
{
    if (NULL != pceResource)
    {
        m_sdk.fns.CEM_FreeResource(pceResource);
    }
}

CEUser CQueryPC::NLAllocCEUserInfo(_In_ const std::wstring& wstrUserName, _In_ const std::wstring& wstrUserID)
{
    CEUser ceUser = { 0 };
    ceUser.userName = NLAllocCEString(wstrUserName);
    ceUser.userID   = NLAllocCEString(wstrUserID);
    return ceUser;
}

void CQueryPC::NLFreeUserInfo(_Inout_ CEUser* pceUser)
{
    if (NULL != pceUser)
    {
        NLFreeCEString(&(pceUser->userName));
        NLFreeCEString(&(pceUser->userID));
    }
}

CEApplication CQueryPC::NLAllocCEApplicationInfo(_In_ const std::wstring& wstrAppName, _In_ const std::wstring& wstrAppPath, _In_ const std::wstring& wstrAppURL)
{
    CEApplication ceApplication = { 0 };
    ceApplication.appName = NLAllocCEString(wstrAppName);
    ceApplication.appPath = NLAllocCEString(wstrAppPath);
    ceApplication.appURL  = NLAllocCEString(wstrAppURL);
    return ceApplication;
}

void CQueryPC::NLFreeApplicationInfo(_Inout_ CEApplication* pceApplication)
{
    if (NULL != pceApplication)
    {
        NLFreeCEString(&(pceApplication->appName));
        NLFreeCEString(&(pceApplication->appPath));
        NLFreeCEString(&(pceApplication->appURL));
    }
}

void CQueryPC::NLFreeCEStrings(_Inout_ CEString** pceStrings, _In_ const int knNum)
{
    if (NULL != pceStrings)
    {
        CEString* pceString = *pceStrings;
        if (NULL != pceString)
        {
            for (int i = 0; i < knNum; ++i)
            {
                NLFreeCEString(&(pceString[i]));
            }
        }
    }
}

void CQueryPC::NLFreeCENameAttrs(_Inout_ CENamedAttributes** pceNameAttrs, _In_ const int knNum)
{
    if (NULL != pceNameAttrs)
    {
        CENamedAttributes* pceNameAttr = *pceNameAttrs;
        if (NULL != pceNameAttr)
        {
            for (int i = 0; i < knNum; ++i)
            {
                NLFreeCEString(&(pceNameAttr[i].name));
                NLFreeAttributes(&(pceNameAttr[i].attrs));
            }
        }
    }
}
