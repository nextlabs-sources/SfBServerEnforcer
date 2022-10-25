// CPPTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "MapTester.h"
#include "NLTagTester.h"
#include "Commmon.h"

int _tmain(int nArgc, _TCHAR* szArgv[])
{
    {
        wprintf(L"Please input any key to start: ");
        wchar_t ch = getwchar();
        wprintf(L"Received key:[%c] and start\n", ch);
    }

    {
        // Print argvs
        for (int i=0; i<nArgc; ++i)
        {
            wprintf(L"Arg:[%d]:[%s]\n", i, szArgv[i]);
        }
    }

#if 0   Map test
    {
        CMapTester mapTester;
        mapTester.MapTest();
    }
#endif

#if 1
    {
        wstring wstrFilePath = szArgv[1];
        CNLTagTester tagTester;
        tagTester.Test(wstrFilePath);
    }
#endif

    {
        wprintf(L"Please input any key to exist: ");
        wchar_t ch = getwchar();
        wprintf(L"Received key:[%c] and exit\n", ch);
    }
    return 0;
}

