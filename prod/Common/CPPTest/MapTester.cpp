/** precompile header and resource header files */
#include "stdafx.h"
/** current class declare header files */
#include "MapTester.h"

/** C system header files */

/** C++ system header files */
#include <string>
#include <vector>
#include <map>
/** Platform header files */

/** Third part library header files */
/** boost */

/** Other project header files */

/** Current project header files */

using namespace std;

CMapTester::CMapTester()
{
}


CMapTester::~CMapTester()
{
}

void MapTest()
{
    map<string, string> mapTest;
    mapTest.insert(make_pair("1", "1"));
    mapTest.insert(make_pair("2", "2"));
    mapTest.insert(make_pair("3", "3"));
    mapTest.insert(make_pair("4", "4"));

    {
        size_t stLen = mapTest.erase("5");
        wprintf(L"Current length:[%d]\n", stLen);
    }

    {
        size_t stLen = mapTest.erase("1");
        wprintf(L"Current length:[%d]\n", stLen);
    }

    {
        size_t stLen = mapTest.erase("3");
        wprintf(L"Current length:[%d]\n", stLen);
    }
}