// dllmain.h : Declaration of module class.

class CSDKWrapperModule : public CAtlDllModuleT< CSDKWrapperModule >
{
public :
	DECLARE_LIBID(LIBID_SDKWrapperLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_SDKWRAPPER, "{A04838E6-6191-4540-B638-BF2E7F4B8512}")
};

extern class CSDKWrapperModule _AtlModule;
