# SDKWrapper
# Need update at here, change SPE to SPOE
VERSION_PRODUCT	= $(VERSION_PRODUCT_SPE)
VERSION_MAJOR		= $(VERSION_MAJOR_SPE)
VERSION_MINOR		= $(VERSION_MINOR_SPE)
VERSION_MAINTENANCE = $(VERSION_MAINTENANCE_SPE)
VERSION_PATCH		= $(VERSION_PATCH_SPE)

COMPILER=VS2013
PLATFORMSDK=7.0

RTLIBTYPE=dynamic

SUBTARGET  = SDKWrapper

#Define the objects of the target module
SRC+= \
 	src/stdafx.cpp \
 	src/CEAttres.cpp \
	src/common_tools.cpp	\
	src/dllmain.cpp \
	src/nlofficerep_only_debug.cpp \
	src/QueryPC.cpp \
	src/SDKWrapper.cpp \
	src/SDKWrapper_i.c 
	
RCSRC = src/SDKWrapper.rc

IDL_SRC = src/SDKWrapper.idl


INCPATH += \
	./include \
  

LIBPATH +=  \
  $(BUILDOUTPUTDIR)/$(LIBDIR) \
  $(NLBUILDROOT)/xlib/$(XLIB_BINDIR) 
  
LIBS = comsuppw Ws2_32 
