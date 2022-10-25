// CEAttres.cpp : Implementation of CCEAttres

#include "../include/stdafx.h"
#include "../include/CEAttres.h"


// CCEAttres


STDMETHODIMP CCEAttres::add_attre(BSTR strName, BSTR strValue)
{
	// TODO: Add your implementation code here
	m_vecAttres.push_back(pair<wstring,wstring>(strName,strValue));
	return S_OK;
}

STDMETHODIMP CCEAttres::get_attre(LONG nIndex, BSTR* strName, BSTR* strValue)
{
	// TODO: Add your implementation code here
	if(nIndex >= (LONG)m_vecAttres.size())	return E_FAIL;
	*strName = ::SysAllocString(m_vecAttres[nIndex].first.c_str());
	*strValue = ::SysAllocString(m_vecAttres[nIndex].second.c_str());
	return S_OK;
}

STDMETHODIMP CCEAttres::get_count(LONG* lCount)
{
	// TODO: Add your implementation code here
	*lCount = m_vecAttres.size();
	return S_OK;
}



STDMETHODIMP CObligation::get_name(BSTR* bstrOBName)
{
	// TODO: Add your implementation code here
	*bstrOBName = ::SysAllocString(m_strName.c_str());
	return S_OK;
}

STDMETHODIMP CObligation::get_policyname(BSTR* strPolicyName)
{
	// TODO: Add your implementation code here
	*strPolicyName = ::SysAllocString(m_strPolicy.c_str());
	return S_OK;
}

STDMETHODIMP CObligation::get_attres(ICEAttres** pAttres)
{
	// TODO: Add your implementation code here
	*pAttres = m_ceAttres;
	(*pAttres)->AddRef();
	return S_OK;
}
