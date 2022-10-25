
#pragma once

#include <string>
#include <vector>

#include "../jsoncpp/include/json.h"

using std::string;
using std::wstring;
using std::vector;

namespace JsonHelper {
	struct CAttribute
	{
		string AttributeId;
		string Value;
		string IncludeInResult;

		string DataType;
	};

	struct CCategory
	{
		string CategoryId;
		vector<CAttribute> Attribute;

		string Id;
	};

	typedef CCategory CSubject;
	typedef CCategory CAction;
	typedef CCategory CResource;
	typedef CCategory CRecipient;

	struct CRequestReference
	{
		vector<string> ReferenceId;
	};

	struct CMultiRequests
	{
		vector<CRequestReference> RequestReference;
	};

	struct CStatus
	{
		string StatusMessage;
		string StatusCode;
	};

	struct CAttributeAssignment
	{
		string AttributeId;
		vector<string> Value;
	};

	struct CObligations
	{
		string Id;
		vector<CAttributeAssignment> AttributeAssignment;
	};

	struct CResult
	{
		string Decision;
		CStatus Status;
		vector<CObligations> Obligations;
	};

	// request
	struct CJsonHelperRequest
	{
		string ReturnPolicyIdList;
		vector<CCategory> Category;

		string CombinedDecision;
		string XPathVersion;
		vector<CSubject> Subject;
		vector<CAction> Action;
		vector<CResource> Resource;
		vector<CRecipient> Recipient;
		CMultiRequests MultiRequests;
	};

	// response
	struct CJsonHelperResponse
	{
		vector<CResult> Result;
	};

	//==========================================================
	class CTranslateJsonHelper
	{
	public:
		CTranslateJsonHelper();
		~CTranslateJsonHelper();

		static bool TranslateToJsonHelperRequest(const wstring& strRequest, CJsonHelperRequest& jsonHelperRequest);				
		static bool TranslateToCERequest(CJsonHelperRequest& jsonHelperRequest, CRequestReference& reqRef, CERequest& ceRequest, nextlabs::sdk_functions_t& fns);
		static bool TranslateToJsonHelperResponse(CJsonHelperResponse& jsonHelperResponse, CEEnforcement_t* ceEnforcement, int num, nextlabs::sdk_functions_t& fns);
		static string TranslateToJson(CJsonHelperResponse& jsonHelperResponse);
		static void FreeResource(int num, CERequest*& ceRequest, CEEnforcement_t*& ceEnforcement, nextlabs::sdk_functions_t& fns);

	private:
		template <typename T>
		static void SetCategoryT(Json::Value value, T& target)
		{
			target.CategoryId = value["CategoryId"].asString();

			for (unsigned int i = 0; i < value["Attribute"].size(); ++i)
			{
				CAttribute attribute;
				SetAttribute(value["Attribute"][i], attribute);
				target.Attribute.push_back(attribute);
			}

			target.Id = value["Id"].asString();
		}
		template <typename T>
		static void FindCategoryT(string& category, vector<T>& vecCategory, T& result)
		{
			vector<T>::iterator iter;
			for (iter = vecCategory.begin(); iter != vecCategory.end(); ++iter)
			{
				if (category.compare(iter->Id) == 0)
				{
					result = *iter;
				}
			}
		}

		static inline void FreeCEString(nextlabs::sdk_functions_t& fns, CEString *str)
		{
			if (str != NULL)
			{
				fns.CEM_FreeString(*str);
			}
		}
		static inline void FreeCEResource(nextlabs::sdk_functions_t& fns, CEResource *resouce)
		{
			if (resouce != NULL)
			{
				fns.CEM_FreeResource(resouce);
			}
		}
		static inline void FreeCEAttributes(nextlabs::sdk_functions_t& fns, CEAttributes* attributes)
		{
			if (attributes != NULL)
			{
				for (LONG i = 0; i < attributes->count; ++i)
				{
					FreeCEString(fns, &(attributes->attrs[i].key));
					FreeCEString(fns, &(attributes->attrs[i].value));
				}
			}
		}

		static string GetCategoryPrefix(const string& str);

		static void SetAttribute(Json::Value value, CAttribute& attribute);
		static void SetMultiRequest(Json::Value value, CMultiRequests& multiRequest);
		static void SetRequestReference(Json::Value value, CRequestReference& reference);
		static void TranslateCEUser(CEUser*& user, CEAttributes*& userAttributes, vector<CAttribute>& attribute, nextlabs::sdk_functions_t& fns);		
		static void TranslateCEResource(CEResource*& resource, CEAttributes*& resAttributes, vector<CAttribute>& attribute, nextlabs::sdk_functions_t& fns);
		static void TranslateCEApplication(CEApplication*& app, CEAttributes*& appAttributes, vector<CAttribute>& attribute, nextlabs::sdk_functions_t& fns);
		static void TranslateCERecipient(CEString*& recipient, CEint32& num, vector<CAttribute>& attribute, nextlabs::sdk_functions_t& fns);
		
		static void SetResponse(CJsonHelperResponse& jsonHelperResponse, CEEnforcement_t& enforcement, nextlabs::sdk_functions_t& fns);
		static void TranslateStatusToJson(Json::Value& value, CStatus& status);
		static void TranslateAttributAssignmentToJson(Json::Value& value, CAttributeAssignment& attributeAssignment);
		static void TranslateObligationsToJson(Json::Value& value, CObligations& obligation);
		static void TranslateResultToJson(Json::Value& value, CResult& result);
		static bool TranslateJsonHelperResponseToJson(CJsonHelperResponse& jsonHelperResponse, Json::Value& root);

		static void FreeCERequest(CERequest *request, nextlabs::sdk_functions_t& fns);
	};
}
