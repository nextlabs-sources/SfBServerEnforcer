ifeq ($(NLBUILDROOT),)
  $(error NLBUILDROOT undefined.)
endif

ifeq ($(NLEXTERNALDIR),)
  $(error NLEXTERNALDIR undefined.)
endif

include $(NLBUILDROOT)/build/Makefile.ver

VERSION_STR=$(VERSION_MAJOR_SFBSE).$(VERSION_MINOR_SFBSE)
ISM_TEMPLATE_FILE_NAME_X64=SkypeForBusinessServerEnforcer_x64.ism
ISM_RELEASE=Media_MSI
ISM_PRODUCT_CONFIG64=64bit
MSI_FILE64=NextLabs-SfbServerEnforcer64.msi

ifeq ($(BUILDTYPE),)
	BUILDTYPE=dev
endif

ifeq ($(VERSION_BUILD),)
	VERSION_BUILD=1
endif

include $(NLBUILDROOT)/install/Makefile.install
