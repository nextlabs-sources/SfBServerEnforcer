// CEAttres.h : Declaration of the CCEAttres

#pragma once
#include "resource.h"       // main symbols

#include "SDKWrapper_i.h"

#include <string>
#include <vector>
using std::wstring;
using std::pair;
using std::vector;
#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CCEAttres

class ATL_NO_VTABLE CCEAttres :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCEAttres, &CLSID_CEAttres>,
	public IDispatchImpl<ICEAttres, &IID_ICEAttres, &LIBID_SDKWrapperLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
	friend class CQueryPC;
public:
	CCEAttres()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_CEATTRES)


BEGIN_COM_MAP(CCEAttres)
	COM_INTERFACE_ENTRY(ICEAttres)
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
	vector<pair<wstring,wstring>> m_vecAttres;
public:

	STDMETHOD(add_attre)(BSTR strName, BSTR strValue);
	STDMETHOD(get_attre)(LONG nIndex, BSTR* strName, BSTR* strValue);
	STDMETHOD(get_count)(LONG* lCount);
};

OBJECT_ENTRY_AUTO(__uuidof(CEAttres), CCEAttres)


// CObligation

class ATL_NO_VTABLE CObligation :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObligation, &CLSID_Obligation>,
	public IDispatchImpl<IObligation, &IID_IObligation, &LIBID_SDKWrapperLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
	friend class CQueryPC;
public:
	CObligation()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_OBLIGATION)


BEGIN_COM_MAP(CObligation)
	COM_INTERFACE_ENTRY(IObligation)
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
	wstring m_strName;      /**< obligation attribute name, if no analysis this is empty */
	wstring m_strPolicy;    /**< policy name, if no analysis this is empty */
	CComQIPtr<ICEAttres,&IID_ICEAttres> m_ceAttres; /**< obligation key and values, if no analysis they the original obligation info */
public:

	STDMETHOD(get_name)(BSTR* bstrOBName);
	STDMETHOD(get_policyname)(BSTR* strPolicyName);
	STDMETHOD(get_attres)(ICEAttres** pAttres);
};

OBJECT_ENTRY_AUTO(__uuidof(Obligation), CObligation)
