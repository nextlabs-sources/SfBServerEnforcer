// QueryPC.h : Declaration of the CQueryPC

#pragma once
#include "resource.h"       // main symbols
#include <string>
#include <vector>
#include <map>

#include "SDKWrapper_i.h"

using std::wstring;
using std::pair;
using std::vector;
using std::map;

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

/**
*  @brief CRequest.
*
*  @details this interface used to set parameters for query policy.
*/
class CCEAttres;
class ATL_NO_VTABLE CRequest :
	public CComObjectRootEx<CComMultiThreadModel>,
    public CComCoClass<CRequest, &CLSID_Request>,
    public IDispatchImpl<IRequest, &IID_IRequest, &LIBID_SDKWrapperLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
    friend class CQueryPC;
public:
    CRequest():m_srcAttres(NULL), m_dstAttres(NULL),m_userAttres(NULL),m_AppAttres(NULL),m_lPerformOB(1), m_lNoiseLevel(2)
    {
    }

DECLARE_REGISTRY_RESOURCEID(IDR_REQUEST)


BEGIN_COM_MAP(CRequest)
    COM_INTERFACE_ENTRY(IRequest)
    COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()

    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct()
    {
        return S_OK;
    }

    void FinalRelease()
    {
    }
private:
    wstring m_strAction; /**< action name,like: OPEN, EDIT, COPY, CONVERT, ... */
    
    /** source info */
    wstring m_strSrc;               /**< source object path */
    wstring m_strSrcType;           /**< source object type: L"fso"(document resource), L"portal"(portal resource), L"device"(device resource) */
    CComPtr<ICEAttres> m_srcAttres; /**< source object attribute */
    
    /** destination info */
    wstring m_strDst;               /**< destination object path */
    wstring m_strDstType;           /**< destination object type: L"fso"(document resource), L"portal"(portal resource), L"device"(device resource) */
    CComPtr<ICEAttres> m_dstAttres; /**< destination object attribute */
    
    /** user info */
    wstring m_struid;                   /**< SID */
    wstring m_strname;                  /**< the user name, to indicate who query the PC for the current case, default is the current logged in user */
    CComPtr<ICEAttres> m_userAttres;    /**< user attribute */
    
    /** application info */
    wstring m_strApp;               /**< application name */
    wstring m_strAppPath;           /**< application full path */
    wstring m_strUrl;               /**< URL path */
    CComPtr<ICEAttres> m_AppAttres; /**< application attribute */

    /** name attributes */
    map<wstring,CComPtr<ICEAttres>> m_nameAttres;   /**< name attribute */

    /** recipients */
    vector<wstring> m_vecRecips;    /**< a map contain all the recipients */

    LONG m_lPerformOB;  /**< a flag used to control if need to perform the obligation: 0 ignore perform obligations, otherwise perform them. default value is 1, perform obligations */

    LONG m_lNoiseLevel; /**< a flag used to control if need pop up bubble or record the notification. default value is CE_NOISE_LEVEL_APPLICATION no bubble pop up, if need to pop up bubble, can set it as: CE_NOISE_LEVEL_USER_ACTION */


public:
    /**
    *  @brief set action name for query policy.
    *
    *  @param strAction [IN] the action name should not be NULL or empty.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(set_action)(BSTR strAction);
    
    /**
    *  @brief set source/destination/name info.
    *
    *  @details this method used to set source/destination/(name attribute) info, the parameter lType used to flag the info type.
    *
    *  @param a strString  [IN] object name, it can be source/destination/(name attribute) info.
    *  @param a strStrType [IN] object type.
    *  @param a pAttres    [IN] object attribute.
    *  @param a lType      [IN] a flag used to control the object type. lType: 0: source; 1: destination; eak; 2: name attributes. if the lType is 2, parameter strString and strStrType no used.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(set_param)(BSTR strString, BSTR strStrType,ICEAttres* pAttres, LONG lType);
    
    /**
    *  @brief set user info.
    *
    *  @param strsid  [IN] SID.
    *  @param strname [IN] user name.
    *  @param pAttres [IN] user attribute.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(set_user)(BSTR strsid, BSTR strname, ICEAttres* pAttres);

    /**
    *  @brief set application info.
    *
    *  @param strname [IN] application name.
    *  @param strpath [IN] application full path.
    *  @param strurl  [IN] URL path, this can be NULL.
    *  @param pAttres [IN] application attribute.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(set_app)(BSTR strname, BSTR strpath, BSTR strurl,ICEAttres* pAttres);

    /**
    *  @brief set recipient info.
    *
    *  @details you can invoke this method by a loop to set multiple recipients information.
    *
    *  @param strRecipient [IN] recipient info.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(set_recipient)(BSTR strRecipient);
    
    /**
    *  @brief set perform obligation flag.
    *
    *  @param lPerformOB [IN] 0 is ignore perform obligations otherwise perform them.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(set_performObligation)(LONG lPerformOB);

    /**
    *  @brief set noise level to control the bubble and record info .
    *
    *  @param lNoiseLevel [IN] if it is CE_NOISE_LEVEL_APPLICATION no bubble pop up, if you need to pop up bubble, can set it as: CE_NOISE_LEVEL_USER_ACTION.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(set_noiseLevel)(LONG lNoiseLevel);
};

OBJECT_ENTRY_AUTO(__uuidof(Request), CRequest)

/**
*  @brief CQueryPC.
*
*  @details this interface used to query policy. User should set the parameters info by using CRequest interface before query the policy.
*/
class ATL_NO_VTABLE CQueryPC :
    public CComObjectRootEx<CComMultiThreadModel>,
    public CComCoClass<CQueryPC, &CLSID_QueryPC>,
    public IDispatchImpl<IQueryPC, &IID_IQueryPC, &LIBID_SDKWrapperLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
    CQueryPC():m_dwCookie(0),m_hHandle(NULL)
    {
    }

DECLARE_REGISTRY_RESOURCEID(IDR_QUERYPC)


BEGIN_COM_MAP(CQueryPC)
    COM_INTERFACE_ENTRY(IQueryPC)
    COM_INTERFACE_ENTRY(IDispatch)
END_COM_MAP()

    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct()
    {
        return S_OK;
    }

    void FinalRelease()
    {
        // release all resource at here
        if(m_hHandle != NULL)
        {
            m_sdk.fns.CECONN_Close(m_hHandle,1000);
            m_hHandle = NULL;
        }
        {
            QueryParam* pPam = NULL;
            CComCritSecLock<CComAutoCriticalSection> lock(m_cs, true);
            map<DWORD,QueryParam*>::iterator it = m_mapQuest.begin();
            for(;it != m_mapQuest.end();++it)
            {
                pPam = it->second;
                if(pPam != NULL)	delete pPam;
            }
            m_mapQuest.clear();
        }
        m_sdk.unload();
    }

private:
    /** query policy result info */
    struct PCResult
    {
        CEResponse_t enfRes;                   /**< enforcement result*/
        vector<IObligation*> vecObs;    /**< obligations info */
    };
    
    /**
    *  @brief .QueryParam
    *
    *  @details used to contain the parameters info which set by CRequest. This class is the inner class.
    */
    class QueryParam
    {
    public:
        int nCookiD;                    /**< cookie number for query policy */
        vector<IRequest*> m_vecReq;     /**< parameters bag */
        vector<PCResult*> m_vecResult;  /**< results bag */
    public:
        ~QueryParam()
        {
            vector<IRequest*>::iterator it = m_vecReq.begin();
            for(;it != m_vecReq.end();++it)
            {
                (*it)->Release();
            }
            m_vecReq.clear();
            vector<PCResult*>::iterator iter = m_vecResult.begin();
            for(;iter != m_vecResult.end();++iter)
            {
                PCResult* pPc = (*iter);
                for(vector<IObligation*>::iterator nit = pPc->vecObs.begin();
                    nit != pPc->vecObs.end();++nit)
                {
                    (*nit)->Release();
                }
                pPc->vecObs.clear();
                delete pPc;
            }
            m_vecResult.clear();
        }
    };

    CComAutoCriticalSection m_cs;       /**< critical section for thread safety */
    map<DWORD,QueryParam*> m_mapQuest;  /**< parameters list */

    nextlabs::cesdk_loader m_sdk;       /**< a manager for load and unload cesdk DLL */
    DWORD m_dwCookie;                   /**< a cookie number to flag the current query policy connect */
    CEHandle m_hHandle;                 /**< connect handle */

private:
    _CEResult_t NLQueryPolicy(_In_ const CRequest* pReq, _In_ const CEint32& ceTimeoutInMillisec, _Out_ CEEnforcement_t* enforcement);
    _CEResult_t NLQueryPolicyEx(_In_ const CRequest* pReq, _In_ const BSTR strPQL, LONG lIngoreBuiltinPolicy, _In_ const CEint32& ceTimeoutInMillisec, _Out_ CEEnforcement_t* enforcement);
    _CEResult_t NLQueryPolicyEx(_In_ std::vector<IRequest*>& vecpRep, _In_ const BSTR bstrPQL, LONG lIngoreBuiltinPolicy, _In_ const CEint32& ceTimeoutInMillisec, _Out_ CEEnforcement_t* ceEnforcement);

    CEAttributes NLGetAttributes(_In_ const CComPtr<ICEAttres> pICEAttributes);
    void NLFreeAttributes(_Inout_ CEAttributes* pCeAttributes);
    
    CEString NLAllocCEString(_In_ const std::wstring& wstrString);
    void NLFreeCEString(_Inout_ CEString* pceString);

    CEResource* NLAllocCEResource(_In_ const std::wstring& wstrResource, _In_ const std::wstring& wstrResourceType);
    void NLFreeCEResource(_Inout_ CEResource* pceResource);

    CEUser NLAllocCEUserInfo(_In_ const std::wstring& wstrUserName, _In_ const std::wstring& wstrUserID);
    void NLFreeUserInfo(_Inout_ CEUser* pceUser);

    CEApplication NLAllocCEApplicationInfo(_In_ const std::wstring& wstrAppName, _In_ const std::wstring& wstrAppPath, _In_ const std::wstring& wstrAppURL);
    void NLFreeApplicationInfo(_Inout_ CEApplication* pceApplication);

    void NLFreeCEStrings(_Inout_ CEString** pceStrings, _In_ const int knNum);
    
    void NLFreeCENameAttrs(_Inout_ CENamedAttributes** pceNameAttrs, _In_ const int knNum);

private:
    bool GetOrgObligations(vector<IObligation*>& pParam,CEAttributes& ob);
    bool AssignObligations(vector<IObligation*>& pParam,CEAttributes& ob);
    bool InitSDK();

public:
    /** 
    *  @addtogroup set query policy info, used before query policy.
    *  
    *  @{
    */

    /**
    *  @brief get current cookie number.
    *
    *  @details the cookie number is flag for query policy, user can use this number to query policy, get query result.
    *
    *  @param plCookie [OUT] return the cookie number.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(get_cookie)(LONG* plCookie);

    /**
    *  @brief set the request info.
    *
    *  @param pRequest [IN] the request info.
    *  @param lCookie  [IN] the cookie number which get by the method: get_cookie().
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(set_request)(IRequest* pRequest,LONG lCookie);
    
    /**
    *  @}
    */

    /** 
    *  @addtogroup query policy
    *  
    *  @{
    */
    
    /**
    *  @brief query policy by using the old cesdk interface: CEEVALUATE_CheckResources.
    *
    *  @details old interface one query only can query one policy.
    *
    *  @param lCookie        [IN] the cookie number which get by the method: get_cookie().
    *  @param lTimeout       [IN] the maximum query time,unit ms.
    *  @param lNeedAssignObs [IN] 0 is ignore assign obligation, otherwise assign it.
    *  @param plResult       [OUT] the result about the query policy invoke, you can  @see _CEResult_t, success is CE_RESULT_SUCCESS otherwise failed.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(check_resource)(LONG lCookie, LONG lTimeout,LONG lNeedAssignObs, LONG* plResult);

    /**
    *  @brief query policy by using the new cesdk interface: CEEVALUATE_CheckResourcesEX.
    *
    *  @details new interface one query can query multi policies.
    *
    *  @param lCookie [IN] the cookie number which get by the method: get_cookie().
    *  @param strPQL  [IN] PQL string, used to query policy.
    *  @param lIngoreBuiltinPolicy [IN] a flag 0 is ignore the build-in policy otherwise query it.
    *  @param lTimeout       [IN] the maximum query time,unit ms.
    *  @param lNeedAssignObs [IN] 0 is ignore assign obligation, otherwise assign it.
    *  @param plResult       [OUT] the result about the query policy invoke, you can  @see _CEResult_t, success is CE_RESULT_SUCCESS otherwise failed.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(check_resourceex)(LONG lCookie, BSTR strPQL, LONG lIngoreBuiltinPolicy, LONG lTimeout, LONG lNeedAssignObs, LONG* plResult);

    /**
    *  @}
    */

    /** 
    *  @addtogroup query policy result
    *  
    *  @{
    */
    
    /**
    *  @brief get query policy result.
    *
    *  @param lCookie  [IN] cookie number which we first get form get_cookie().
    *  @param nIndexOfResults [IN] the index about the query result, the first one is 0.
    *  @param plResult [OUT] the result about the query policy invoke, you can  @see _CEResult_t, success is CE_RESULT_SUCCESS otherwise failed.
    *  @param plObsNum [OUT] the obligations number, please @see check_resourceex and @see check_resourceex. 
    *                        If parameter lNeedAssignObs is 0, this number aways 1 and user need to analysis the obligations.
    *                        If parameter lNeedAssignObs is not 0, this number is point the obligations count and user need to use get_all_obligations to get the obligations array.
    *                        Of course if there is only one obligation, user also can use get_obligation to get the obligation. the more details you can @see get_all_obligations and @see get_obligation.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(get_result)(LONG lCookie, LONG nIndexOfResults, LONG* plResult, LONG* plObsNum);

    /**
    *  @brief release the policy result.
    *
    *  @param lCookie [IN] cookie number which we first get form get_cookie().
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(release_request)(LONG lCookie);

    /**
    *  @brief get the obligation form the obligation list.
    *
    *  @param lCookie   [IN] cookie number which we first get form get_cookie().
    *  @param strOBName [IN] obligation name used to query in the obligation list. if this parameter is NULL or empty we return the first one. If the obligation no assigned the first one contain all the original obligation info.
    *  @param nIndexOfResults [IN] the index about the query result, the first one is 0.
	*  @param nIndexofOb [IN] one plocy may contains several same ob, so we need to get all by pass index. Invoker need to loop to get
							the ob till get_obligation return S_FALSE.nINdexOfOb start at 0.
    *  @param pObligation [OUT] return the obligation pointer, user just need used no need release. It will be released when the CQuery interface is released.
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(get_obligation)(LONG lCookie, BSTR strOBName, LONG nIndexOfResults, LONG nIndexOfOb,IObligation** pObligation);

    /**
    *  @brief get all the obligations.
    *
    *  @param lCookie   [IN] cookie number which we first get form get_cookie().
    *  @param nIndexOfResults [IN] the index about the query result, the first one is 0.
    *  @param pObligations [OUT] return an array contain all the obligation pointers.
    *  @param lObsNum      [IN]  the obligations number which returned by invoke get_result().
    *  @retval S_OK success otherwise failed.
    */
    STDMETHOD(get_all_obligations)(LONG lCookie, LONG nIndexOfResults, IObligation** pObligations, LONG lObsNum);    

	/**
	*  @brief query policy by using the rest api interface.
	*
	*  @details new interface one query can query multi policies.
	*
	*  @param strRequest [IN] request string, json format.
	*  @param strResponse [OUT] return json format response.
	*  @param plResult [OUT] the result about the query policy invoke, you can  @see _CEResult_t, success is CE_RESULT_SUCCESS otherwise failed.
	*  @retval S_OK success otherwise failed.
	*/
	STDMETHOD(check_resourceex_json)(BSTR strRequest, BSTR *strResponse, LONG* plResult);
};

OBJECT_ENTRY_AUTO(__uuidof(QueryPC), CQueryPC)


