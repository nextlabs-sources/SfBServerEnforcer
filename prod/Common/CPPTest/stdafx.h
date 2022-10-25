// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once


/** precompile header and resource header files */
#include "targetver.h"
/** current class declare header files */

/** C system header files */
#include <stdio.h>
#include <tchar.h>
/** C++ system header files */
#include <string>
#include <vector>
#include <map>
/** Platform header files */
// ATL
#pragma warning( push )
#pragma warning( disable: 6387 6011 )
#include <atlbase.h>
#include <atlcom.h>
#include <atlctl.h>
#pragma warning( pop )

// COM
#include <OAIdl.h>
/** Third part library header files */
/** boost */

/** Other project header files */

/** Current project header files */
#include "nl_only_debug.h"
// TODO: reference additional headers your program requires here

using namespace std;


