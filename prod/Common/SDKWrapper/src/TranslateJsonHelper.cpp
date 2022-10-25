
#include "stdafx.h"

#include "../platform/cesdk_loader.hpp"

#include "TranslateJsonHelper.h"
#include "nlofficerep_only_debug.h"

using namespace JsonHelper;

bool GetValue(__in WCHAR* (*pfn_CEM_GetString)(CEString),
	CEAttributes& obligations,
	__in const wchar_t* in_key,
	std::wstring& in_value);

#define JSONHELPER_PRINT(args, ...) do {NLPrintLogW(true, L"[JSONHELPER]"args##L"   [%s:%d]\r\n", ##__VA_ARGS__, __FILEW__, __LINE__);}while(0)
#ifdef _DEBUG
#define JSONHELPER_DEBUG(args, ...) do {NLPrintLogW(true, L"[JSONHELPER]"args##L"   [%s:%d]\r\n", ##__VA_ARGS__, __FILEW__, __LINE__);}while(0)
#else
#define JSONHELPER_DEBUG(args, ...)
#endif

void JsonHelperPrintAttributes(const TCHAR *prefix, int index, CEAttributes *attributes, nextlabs::sdk_functions_t* fns)
{
#ifdef _DEBUG
	JSONHELPER_DEBUG(L"CERequest[%d].%s attribute num:%d", index, prefix, attributes->count);	
	for (int i = 0; i < attributes->count; ++i)
	{
		JSONHELPER_DEBUG(L"Attribute[%d] key:%s", i, fns->CEM_GetString(attributes->attrs[i].key));
		JSONHELPER_DEBUG(L"Attribute[%d] value:%s", i, fns->CEM_GetString(attributes->attrs[i].value));
	}
#else
	prefix = prefix, index = index, attributes = attributes, fns = fns;
#endif
}

void JsonHelperPrintCERequest(int num, CERequest *request, nextlabs::sdk_functions_t* fns)
{
	for (int i = 0; i < num; ++i)
	{
		if (request[i].operation)
		{
			JSONHELPER_DEBUG(L"CERequest[%d].operation:%s", i, fns->CEM_GetString(request[i].operation));
		}
		if (request[i].source)
		{
			JSONHELPER_DEBUG(L"CERequest[%d].source type:%s", i, fns->CEM_GetString(request[i].source->resourceType));
			JSONHELPER_DEBUG(L"CERequest[%d].source name:%s", i, fns->CEM_GetString(request[i].source->resourceName));
		}
		if (request[i].sourceAttributes)
		{
			JsonHelperPrintAttributes(L"source", i, request[i].sourceAttributes, fns);
		}
		if (request[i].target)
		{
			JSONHELPER_DEBUG(L"CERequest[%d].target type:%s", i, fns->CEM_GetString(request[i].target->resourceType));
			JSONHELPER_DEBUG(L"CERequest[%d].target name:%s", i, fns->CEM_GetString(request[i].target->resourceName));
		}
		if (request[i].targetAttributes)
		{
			JsonHelperPrintAttributes(L"target", i, request[i].targetAttributes, fns);
		}
		if (request[i].user)
		{
			JSONHELPER_DEBUG(L"CERequest[%d].user id:%s", i, fns->CEM_GetString(request[i].user->userID));
			JSONHELPER_DEBUG(L"CERequest[%d].user name:%s", i, fns->CEM_GetString(request[i].user->userName));
		}
		if (request[i].userAttributes)
		{
			JsonHelperPrintAttributes(L"user", i, request[i].userAttributes, fns);
		}
		if (request[i].app)
		{
			JSONHELPER_DEBUG(L"CERequest[%d].app name:%s", i, fns->CEM_GetString(request[i].app->appName));
			JSONHELPER_DEBUG(L"CERequest[%d].app path:%s", i, fns->CEM_GetString(request[i].app->appPath));
			JSONHELPER_DEBUG(L"CERequest[%d].app url:%s", i, fns->CEM_GetString(request[i].app->appURL));			
		}
		if (request[i].appAttributes)
		{
			JsonHelperPrintAttributes(L"app", i, request[i].appAttributes, fns);
		}
		JSONHELPER_DEBUG(L"CERequest[%d].recipient num:%d", i, request[i].numRecipients);
		for (int k = 0; k < request[i].numRecipients; ++k)
		{
			JSONHELPER_DEBUG(L"CERequest[%d].recipient[%d]:%s", i, k, fns->CEM_GetString(request[i].recipients[k]));
		}
		JSONHELPER_DEBUG(L"CERequest[%d].additional attribute num:%d", i, request[i].numAdditionalAttributes);
		for (int k = 0; k < request[i].numAdditionalAttributes; ++k)
		{
			JSONHELPER_DEBUG(L"CERequest[%d].additional attributes[%d] key:%s", i, k, fns->CEM_GetString(request[i].additionalAttributes[k].name));
		}
		JSONHELPER_DEBUG(L"CERequest[%d].PerformObligation:%d", i, request[i].performObligation);
		JSONHELPER_DEBUG(L"CERequest[%d].NoiseLevel:%d", i, request[i].noiseLevel);
	}
}

wstring StringToWstring(const string &str)
{
	int length = (int)str.length();
	LPWSTR pBuf = new TCHAR[length + 1]();
	MultiByteToWideChar(CP_ACP, 0, (LPCSTR)str.c_str(), length, pBuf, length);

	wstring wstr = pBuf;
	delete[] pBuf;
	return wstr;
}

string WstringToString(const wstring& wstr)
{
	int length = (int)wstr.length();
	LPSTR pBuf = new CHAR[length + 1]();	
	WideCharToMultiByte(CP_ACP, 0, (LPWSTR)wstr.c_str(), length, pBuf, length, NULL, NULL);

	string str = pBuf;
	delete[] pBuf;
	return str;
}

CTranslateJsonHelper::CTranslateJsonHelper()
{	
}

CTranslateJsonHelper::~CTranslateJsonHelper()
{
}

void CTranslateJsonHelper::SetAttribute(Json::Value value, CAttribute& attribute)
{
	attribute.AttributeId = value["AttributeId"].asString();
	attribute.Value = value["Value"].asString();
	attribute.IncludeInResult = value["IncludeInResult"].asString();
	attribute.DataType = value["DataType"].asString();
}

void CTranslateJsonHelper::SetRequestReference(Json::Value value, CRequestReference& reference)
{
	for (unsigned int i = 0; i < value["ReferenceId"].size(); ++i)
	{
		string id = value["ReferenceId"][i].asString();
		reference.ReferenceId.push_back(id);
	}
}

void CTranslateJsonHelper::SetMultiRequest(Json::Value value, CMultiRequests& multiRequest)
{
	for (unsigned int i = 0; i < value["RequestReference"].size(); ++i)
	{
		CRequestReference reference;
		SetRequestReference(value["RequestReference"][i], reference);
		multiRequest.RequestReference.push_back(reference);
	}
}

bool CTranslateJsonHelper::TranslateToJsonHelperRequest(const wstring& strRequest, CJsonHelperRequest& jsonHelperRequest)
{	
	string str = WstringToString(strRequest);

	JSONHELPER_DEBUG(L"request str:%s", strRequest.c_str());

	Json::Features features;
	Json::Value root;		
	Json::Reader reader(features);

	if (reader.parse(str, root) == false)
	{
		JSONHELPER_PRINT(L"Json parse failed[%s]", str);
		return false;
	}

	if (root["Request"]["Category"].size() <= 0 || root["Request"]["Subject"].size() <= 0 ||
		root["Request"]["Action"].size() <= 0 || root["Request"]["Resource"].size() <= 0)
	{
		JSONHELPER_PRINT(L"Invalid parameters");
		return false;
	}

	jsonHelperRequest.ReturnPolicyIdList = root["Request"]["ReturnPolicyIdList"].asString();
	for (unsigned int i = 0; i < root["Request"]["Category"].size(); ++i)
	{
		CCategory category;
		
		SetCategoryT(root["Request"]["Category"][i], category);
		jsonHelperRequest.Category.push_back(category);
	}	

	jsonHelperRequest.CombinedDecision = root["Request"]["CombinedDecision"].asString();;
	jsonHelperRequest.XPathVersion = root["Request"]["XPathVersion"].asString();;
	for (unsigned int i = 0; i < root["Request"]["Subject"].size(); ++i)
	{
		CSubject subject;
	
		SetCategoryT(root["Request"]["Subject"][i], subject);
		jsonHelperRequest.Subject.push_back(subject);
	}
	for (unsigned int i = 0; i < root["Request"]["Action"].size(); ++i)
	{
		CAction action;

		SetCategoryT(root["Request"]["Action"][i], action);
		jsonHelperRequest.Action.push_back(action);
	}
	
	for (unsigned int i = 0; i < root["Request"]["Resource"].size(); ++i)
	{
		CResource resource;

		SetCategoryT(root["Request"]["Resource"][i], resource);
		jsonHelperRequest.Resource.push_back(resource);
	}
	for (unsigned int i = 0; i < root["Request"]["Recipient"].size(); ++i)
	{
		CRecipient recipient;
		
		SetCategoryT(root["Request"]["Recipient"][i], recipient);
		jsonHelperRequest.Recipient.push_back(recipient);
	}	
	
	SetMultiRequest(root["Request"]["MultiRequests"], jsonHelperRequest.MultiRequests);
	return true;
}

void CTranslateJsonHelper::SetResponse(CJsonHelperResponse& jsonHelperResponse, CEEnforcement_t& enforcement, nextlabs::sdk_functions_t& fns)
{
	CResult result;
		
	if (enforcement.result == CEAllow)
	{
		result.Decision = "Allow";
	}
	else if (enforcement.result == CEDeny)
	{
		result.Decision = "Deny";
	}
	else
	{
		result.Decision = "DontCare";
	}	 

	CStatus status;
	string statusCode = "urn:oasis:names:tc:xacml:1.0:status:ok";
	status.StatusMessage = "success";
	status.StatusCode = statusCode;
	result.Status = status;
	
	if (enforcement.obligation == NULL)
	{
		jsonHelperResponse.Result.push_back(result);
		return;
	}
	
	typedef WCHAR* (*GetString)(CEString);
	GetString pFunc = (GetString)fns.CEM_GetString;
	wchar_t temp_key[128] = { 0 };
	wstring wsvalue = L"";

	swprintf(temp_key, _countof(temp_key), L"CE_ATTR_OBLIGATION_COUNT");	
	if (!GetValue(pFunc, *enforcement.obligation, temp_key, wsvalue))
	{
		JSONHELPER_DEBUG(L"Get [%s] failed", temp_key);
		return;
	}
	int num_obs = _wtoi(wsvalue.c_str());
	
	bool bResult;
	CObligations obligation;
	CAttributeAssignment attributeAssignment;
	string strKey, strValue;
	
	for (int i = 1; i <= num_obs; ++i) /* Obligations [1,n] not [0,n-1] */
	{
		int num_values = 0;

		swprintf(temp_key, _countof(temp_key), L"CE_ATTR_OBLIGATION_NAME:%d", i);    
		bResult = GetValue(pFunc, *enforcement.obligation, temp_key, wsvalue);
		if (!bResult)
		{
			JSONHELPER_DEBUG(L"Get Obligation name failed[%d]", i);
			continue;
		}
		obligation.Id = WstringToString(wsvalue);
		JSONHELPER_DEBUG(L"Obligation name:%s", wsvalue.c_str());

		obligation.AttributeAssignment.clear();

		swprintf(temp_key, _countof(temp_key), L"CE_ATTR_OBLIGATION_POLICY:%d", i);
		bResult = GetValue(pFunc, *enforcement.obligation, temp_key, wsvalue);
		if (!bResult)
		{
			JSONHELPER_DEBUG(L"Get Obligation policy failed[%d]", i);
			continue;
		}
		JSONHELPER_DEBUG(L"Obligation policy:%s", wsvalue.c_str());
		attributeAssignment.AttributeId = "POLICY";
		strValue = WstringToString(wsvalue);
		attributeAssignment.Value.clear();
		attributeAssignment.Value.push_back(strValue);
		obligation.AttributeAssignment.push_back(attributeAssignment);

		swprintf(temp_key, _countof(temp_key), L"CE_ATTR_OBLIGATION_NUMVALUES:%d", i);
		bResult = GetValue(pFunc, *enforcement.obligation, temp_key, wsvalue);
		if (!bResult)
		{
			JSONHELPER_DEBUG(L"Get Obligation numvalues failed[%d]", i);
			continue;
		}
		num_values = _wtoi(wsvalue.c_str());

		JSONHELPER_DEBUG(L"obs number value:[%d],string :[%s] \n", num_values, wsvalue.c_str());
	
		for (int k = 1; k <= num_values; k += 2)
		{			
			swprintf(temp_key, _countof(temp_key), L"CE_ATTR_OBLIGATION_VALUE:%d:%d", i, k);
			bResult = GetValue(pFunc, *enforcement.obligation, temp_key, wsvalue);
			if (!bResult)
			{
				JSONHELPER_DEBUG(L"Get Obligation value[%s] failed", temp_key);
				break;
			}
			strKey = WstringToString(wsvalue);
			wstring dbgStr = wsvalue;
			swprintf(temp_key, _countof(temp_key), L"CE_ATTR_OBLIGATION_VALUE:%d:%d", i, k + 1);
			bResult = GetValue(pFunc, *enforcement.obligation, temp_key, wsvalue);			
			if (!bResult)
			{
				JSONHELPER_DEBUG(L"Get Obligation value[%s] failed", temp_key);
				break;
			}
			JSONHELPER_DEBUG(L"Obligation attribute:[%s:%s]", dbgStr.c_str(), wsvalue.c_str());
			strValue = WstringToString(wsvalue);
			attributeAssignment.AttributeId = strKey;
			attributeAssignment.Value.clear();
			attributeAssignment.Value.push_back(strValue);

			obligation.AttributeAssignment.push_back(attributeAssignment);
		}

		result.Obligations.push_back(obligation);
	}
	jsonHelperResponse.Result.push_back(result);
}

bool CTranslateJsonHelper::TranslateToJsonHelperResponse(CJsonHelperResponse& jsonHelperResponse, CEEnforcement_t* ceEnforcement, int num,
	nextlabs::sdk_functions_t& fns)
{
	JSONHELPER_DEBUG(L"SetResponses num:%d", num);
	for (int i = 0; i < num; ++i)
	{
		JSONHELPER_DEBUG(L"Result obligation addr:%p", ceEnforcement[i].obligation);
		SetResponse(jsonHelperResponse, ceEnforcement[i], fns);
	}

	return true;
}

void CTranslateJsonHelper::TranslateCEUser(CEUser*& user, CEAttributes*& userAttributes, vector<CAttribute>& attribute,
	nextlabs::sdk_functions_t& fns)
{
	vector<CAttribute>::iterator iter;
	wstring key, value;
	string substr("subject:");
	int length = (int)substr.length();
	int i = 0;
	
	user = new CEUser;
	user->userName = user->userID = NULL;
	if (attribute.size() > 2)
	{
		userAttributes = new CEAttributes();
		userAttributes->attrs = new CEAttribute[attribute.size() - 2]();
	}

	for (iter = attribute.begin(); iter != attribute.end(); ++iter)
	{
		substr = iter->AttributeId.substr(length);
		key = StringToWstring(substr);
		value = StringToWstring(iter->Value);
		JSONHELPER_DEBUG(L"User Attribute Prefix:%s", key.c_str());
		if (substr.compare("name") == 0)
		{			
			user->userName = fns.CEM_AllocateString(value.c_str());			
			JSONHELPER_DEBUG(L"user name:%s", value.c_str());
		}
		else if (substr.compare("sid") == 0)
		{
			user->userID = fns.CEM_AllocateString(value.c_str());
			JSONHELPER_DEBUG(L"user id:%s", value.c_str());
		}
		else
		{
			if (value.length() == 0)
			{
				continue;
			}
			userAttributes->attrs[i].key = fns.CEM_AllocateString(key.c_str());
			userAttributes->attrs[i].value = fns.CEM_AllocateString(value.c_str());
			i++;
		}
	}
	if (i != 0)
	{
		userAttributes->count = i;
	}
	else if (attribute.size() > 2)
	{
		delete[] userAttributes->attrs;
		delete userAttributes;
		userAttributes = NULL;
	}
}

void CTranslateJsonHelper::TranslateCEResource(CEResource*& resource, CEAttributes*& resAttributes, vector<CAttribute>& attribute, nextlabs::sdk_functions_t& fns)
{
	vector<CAttribute>::iterator iter;
	wstring id, type, key, value;
	int i = 0;

	string str("resource:");
	int length = (int)str.length();

	if (attribute.size() > 2)
	{
		resAttributes = new CEAttributes();
		resAttributes->attrs = new CEAttribute[attribute.size() - 2]();
	}

	for (iter = attribute.begin(); iter != attribute.end(); ++iter)
	{
		if (iter->AttributeId.compare("resource:resource-id") == 0)
		{
			id = StringToWstring(iter->Value);
		}
		else if (iter->AttributeId.compare("resource:resource-type") == 0)
		{
			type = StringToWstring(iter->Value);;
		}
		else
		{
			key = StringToWstring(iter->AttributeId.substr(length));
			value = StringToWstring(iter->Value);
			JSONHELPER_DEBUG(L"Resource attribute:[%s:%s:%d]", key.c_str(), value.c_str(), value.length());

			if (value.length() == 0)
			{
				continue;
			}

			resAttributes->attrs[i].key = fns.CEM_AllocateString(key.c_str());
			resAttributes->attrs[i].value = fns.CEM_AllocateString(value.c_str());			
			i++;
		}
	}
	if (i != 0)
	{
		resAttributes->count = i;
	}
	else if (attribute.size() > 2)
	{
		delete[] resAttributes->attrs;
		delete resAttributes;
		resAttributes = NULL;
	}

	resource = fns.CEM_CreateResourceW(id.c_str(), type.c_str());
	JSONHELPER_DEBUG(L"Resource id:%s, type:%s", id.c_str(), type.c_str());
}

void CTranslateJsonHelper::TranslateCEApplication(CEApplication*& app, CEAttributes*& appAttributes, vector<CAttribute>& attribute, nextlabs::sdk_functions_t& fns)
{
	vector<CAttribute>::iterator iter;
	wstring key, value;
	string substr("application:");
	int length = (int)substr.length();
	int i = 0;

	app = new CEApplication;
	app->appName = app->appPath = app->appURL = NULL;
	if (attribute.size() > 2)
	{
		appAttributes = new CEAttributes();
		appAttributes->attrs = new CEAttribute[attribute.size() - 2]();		
	}

	for (iter = attribute.begin(); iter != attribute.end(); ++iter)
	{
		substr = iter->AttributeId.substr(length);
		key = StringToWstring(substr);
		value = StringToWstring(iter->Value);
		JSONHELPER_DEBUG(L"App Attribute Prefix:%s", key.c_str());
		if (substr.compare("application-name") == 0)
		{
			app->appName = fns.CEM_AllocateString(value.c_str());
			JSONHELPER_DEBUG(L"Application name:%s", value.c_str());
		}
		else if (substr.compare("application-path") == 0)
		{
			app->appPath = fns.CEM_AllocateString(value.c_str());
			JSONHELPER_DEBUG(L"Application path:%s", value.c_str());
		}
		else
		{
			if (value.length() == 0)
			{
				continue;
			}

			JSONHELPER_DEBUG(L"App attribute:[%s:%s]", key.c_str(), value.c_str());
			appAttributes->attrs[i].key = fns.CEM_AllocateString(key.c_str());
			appAttributes->attrs[i].value = fns.CEM_AllocateString(value.c_str());
			i++;
		}
	}
	if (i != 0)
	{
		appAttributes->count = i;
	}
	else if (attribute.size() > 2)
	{		
		delete[] appAttributes->attrs;
		delete appAttributes;
		appAttributes = NULL;
	}
}

void CTranslateJsonHelper::TranslateCERecipient(CEString*& recipient, CEint32& num, vector<CAttribute>& attribute, nextlabs::sdk_functions_t& fns)
{
	num = (CEint32)attribute.size();
	JSONHELPER_DEBUG(L"Recipient num:%d", num);
	if (num <= 0)
	{
		JSONHELPER_DEBUG(L"Recipient num is zero");
		return;
	}

	wstring wstr;
	recipient = new CEString[num]();
	for (CEint32 i = 0; i < num; ++i)
	{
		wstr = StringToWstring(attribute[i].Value);
		recipient[i] = fns.CEM_AllocateString(wstr.c_str());
		JSONHELPER_DEBUG(L"Recipient[%d]:%s", i, wstr.c_str());
	}
}

bool CTranslateJsonHelper::TranslateToCERequest(CJsonHelperRequest& jsonHelperRequest, CRequestReference& reqRef, CERequest& ceRequest,
	nextlabs::sdk_functions_t& fns)
{
	string category;
	CSubject subject;
	CResource resource;
	CAction action;
	CCategory application;
	CRecipient recipient;
	for (int i = 0; i < reqRef.ReferenceId.size(); ++i)
	{
		wstring wstr2 = StringToWstring(reqRef.ReferenceId[i]);
		JSONHELPER_DEBUG(L"Reference id:%s", wstr2.c_str());
		category = GetCategoryPrefix(reqRef.ReferenceId[i]);
		if (category.compare("subject") == 0)
		{
			FindCategoryT(reqRef.ReferenceId[i], jsonHelperRequest.Subject, subject);
		}
		else if (category.compare("resource") == 0)
		{
			FindCategoryT(reqRef.ReferenceId[i], jsonHelperRequest.Resource, resource);
		}
		else if (category.compare("action") == 0)
		{
			FindCategoryT(reqRef.ReferenceId[i], jsonHelperRequest.Action, action);
		}
		else if (category.compare("application") == 0)
		{
			FindCategoryT(reqRef.ReferenceId[i], jsonHelperRequest.Category, application);
		}
		else if (category.compare("recipient") == 0)
		{
			FindCategoryT(reqRef.ReferenceId[i], jsonHelperRequest.Recipient, recipient);
		}
	}
	if (subject.Attribute.size() <= 0 || resource.Attribute.size() <= 0 ||
		action.Attribute.size() <= 0 || application.Attribute.size() <= 0)
	{
		JSONHELPER_DEBUG(L"subject:%d, res:%d, act:%d, app:%d", subject.Attribute.size(), resource.Attribute.size(), 
			action.Attribute.size(), application.Attribute.size());
		return false;
	}

	TranslateCEUser(ceRequest.user, ceRequest.userAttributes, subject.Attribute, fns);

	TranslateCEResource(ceRequest.source, ceRequest.sourceAttributes, resource.Attribute, fns);

	wstring wstr = StringToWstring(action.Attribute.front().Value);
	ceRequest.operation = fns.CEM_AllocateString(wstr.c_str());
	JSONHELPER_DEBUG(L"Operation:%s", wstr.c_str());

	TranslateCEApplication(ceRequest.app, ceRequest.appAttributes, application.Attribute, fns);

	TranslateCERecipient(ceRequest.recipients, ceRequest.numRecipients, recipient.Attribute, fns);

	ceRequest.noiseLevel = CE_NOISE_LEVEL_APPLICATION;
	ceRequest.performObligation = CETrue;

	ceRequest.target = NULL;
	ceRequest.targetAttributes = NULL;

	return true;
}

void CTranslateJsonHelper::FreeCERequest(CERequest* ceRequest, nextlabs::sdk_functions_t& fns)
{
	FreeCEString(fns, &ceRequest->operation);

	FreeCEResource(fns, ceRequest->source);
	FreeCEAttributes(fns, ceRequest->sourceAttributes);
	delete ceRequest->sourceAttributes;

	FreeCEResource(fns, ceRequest->target);
	FreeCEAttributes(fns, ceRequest->targetAttributes);
	delete ceRequest->targetAttributes;

	if (ceRequest->user != NULL)
	{
		FreeCEString(fns, &ceRequest->user->userName);
		FreeCEString(fns, &ceRequest->user->userID);
		delete ceRequest->user;
	}
	FreeCEAttributes(fns, ceRequest->userAttributes);
	delete ceRequest->userAttributes;

	if (ceRequest->app != NULL)
	{
		FreeCEString(fns, &ceRequest->app->appName);
		FreeCEString(fns, &ceRequest->app->appPath);
		FreeCEString(fns, &ceRequest->app->appURL);
		delete ceRequest->app;
	}
	FreeCEAttributes(fns, ceRequest->appAttributes);
	delete ceRequest->appAttributes;

	if (ceRequest->recipients != NULL)
	{
		for (int i = 0; i < ceRequest->numRecipients; ++i)
		{
			FreeCEString(fns, &ceRequest->recipients[i]);
		}
		delete ceRequest->recipients;
	}

	if (ceRequest->additionalAttributes != NULL)
	{
		for (int i = 0; i < ceRequest->numAdditionalAttributes; ++i)
		{
			FreeCEString(fns, &ceRequest->additionalAttributes[i].name);
			FreeCEAttributes(fns, &ceRequest->additionalAttributes[i].attrs);
		}
		delete ceRequest->additionalAttributes;
	}
}

void CTranslateJsonHelper::FreeResource(int num, CERequest*& ceRequest, CEEnforcement_t*& ceEnforcement, nextlabs::sdk_functions_t& fns)
{
	for (int i = 0; i < num; ++i)
	{
		FreeCERequest(&ceRequest[i], fns);
		fns.CEEVALUATE_FreeEnforcement(ceEnforcement[i]);
	}
}

void CTranslateJsonHelper::TranslateStatusToJson(Json::Value& value, CStatus& status)
{
	Json::Value statusCode;
	statusCode["Value"] = Json::Value(status.StatusCode);

	value["StatusMessage"] = Json::Value(status.StatusMessage);
	value["StatusCode"] = statusCode;
}

void CTranslateJsonHelper::TranslateAttributAssignmentToJson(Json::Value& value, CAttributeAssignment& attributeAssignment)
{
	vector<string>::iterator iter;

	value["AttributeId"] = Json::Value(attributeAssignment.AttributeId);
	for (iter = attributeAssignment.Value.begin(); iter != attributeAssignment.Value.end(); ++iter)
	{
		value["Value"].append(*iter);
	}
}

void CTranslateJsonHelper::TranslateObligationsToJson(Json::Value& value, CObligations& obligation)
{
	Json::Value attributeAssignment;
	vector<CAttributeAssignment>::iterator iter;

	value["Id"] = Json::Value(obligation.Id);
	for (iter = obligation.AttributeAssignment.begin(); iter != obligation.AttributeAssignment.end(); ++iter)
	{
		attributeAssignment.clear();
		TranslateAttributAssignmentToJson(attributeAssignment, *iter);
		value["AttributeAssignment"].append(attributeAssignment);
	}
}

void CTranslateJsonHelper::TranslateResultToJson(Json::Value& value, CResult& result)
{
	Json::Value status, obligations;
	vector<CObligations>::iterator iter;

	value["Decision"] = Json::Value(result.Decision);
	TranslateStatusToJson(status, result.Status);
	value["Status"] = status;
	for (iter = result.Obligations.begin(); iter != result.Obligations.end(); ++iter)
	{
		obligations.clear();
		TranslateObligationsToJson(obligations, *iter);
		value["Obligations"].append(obligations);
	}
}

bool CTranslateJsonHelper::TranslateJsonHelperResponseToJson(CJsonHelperResponse& jsonHelperResponse, Json::Value& root)
{
	Json::Value result;
	vector<CResult>::iterator iter;

	if (jsonHelperResponse.Result.size() < 1)
	{
		return false;
	}

	for (iter = jsonHelperResponse.Result.begin(); iter != jsonHelperResponse.Result.end(); ++iter)
	{
		result.clear();
		TranslateResultToJson(result, *iter);

		root["Response"]["Result"].append(result);
	}
	return true;
}

string CTranslateJsonHelper::TranslateToJson(CJsonHelperResponse& jsonHelperResponse)
{
	// translate to json
	Json::Value root;
	Json::FastWriter writer;

	TranslateJsonHelperResponseToJson(jsonHelperResponse, root);

	string text = writer.write(root);

	//{
	//	wstring wstr;

	//	wstr = StringToWstring(text);
	//	JSONHELPER_DEBUG(L"Response text:%s", wstr.c_str());
	//}

	if (text.compare("null\n") == 0)
	{
		JSONHELPER_PRINT(L"Response is null");
		return "";
	}
	return text;
}

string CTranslateJsonHelper::GetCategoryPrefix(const string& str)
{
	char ch;

	for (int i = 0; i < str.length(); ++i)
	{
		ch = str.c_str()[i];
		if (ch >= '0' && ch <= '9')
		{
			return str.substr(0, i);
		}
	}

	return str;
}
