#pragma once

#include "stdafx.h"

class CNLTagTester
{
public:
    CNLTagTester();
    ~CNLTagTester();

public:
    void Test(_In_ const wstring& kwstrTestFilePath);

private:
    // vector<pair<wstring, wstring>> GetVecTagsFromINLTag(_In_ INLTag* pINLTag) const;
};

