# NLSFBEEnforcer
# Notes: To each Visual Studio compilation output. We chain a few lines of scripts
# together. In doing so, Make executes all of them in one process. We want the exit 
# status of this process to be the exit status of code compilation. So we add this 
# line of code at the end. Note that there is no line continuation character "\" at
# the end of this line because this is the end of a chain of commands to be executed
# in one shell process. However, the line before this should have a "\".
#		exit $$COMPILE_STATUS
#
# On the other hand, if we put MSVC call within a if statement where the whole compilation
# binary signing and copying to distribution directory are all in one line. That means
# the command "exit $$COMPILE_STATUS" cannot be the last commnand to be executed in a
# process. In this case, we need to check exit status before calling "exit $$COMPILE_STATUS".
#		if [ $$COMPILE_STATUS -ne 0 ]; then		\
#			exit $$COMPILE_STATUS ;				\
#		fi ;									\
# $(BUILDOUTPUTDIR) is $(NLBUILDROOT)/bin/$(BIN_DIR)
#

ifeq ($(NLBUILDROOT),)
  $(error NLBUILDROOT undefined.)
endif

ifeq ($(NLEXTERNALGITDIR),)
  $(error NLEXTERNALGITDIR undefined.)
endif


#
# Variables
#

TARGETS_SP=compile

include $(NLBUILDROOT)/build/Makefile.ver

VERSION_PRODUCT		= $(VERSION_PRODUCT_SFBSE)
VERSION_MAJOR		= $(VERSION_MAJOR_SFBSE)
VERSION_MINOR		= $(VERSION_MINOR_SFBSE)
VERSION_MAINTENANCE = $(VERSION_MAINTENANCE_SFBSE)
VERSION_PATCH		= $(VERSION_PATCH_SFBSE)

# Makefile.vs contains target all which depends on $(TARGETS_SP)
include $(NLBUILDROOT)/build/Makefile.vs


.PHONY: compile
compile:
	@echo Passing by - nothing to do here
