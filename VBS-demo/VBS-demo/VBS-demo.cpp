// VBS_Enclave.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <enclaveapi.h>
#include <windows.h>
//#include <winenclave.h>

using namespace std;

int sum(int a, int b)
{
    return a + b;
}

//Returns the last Win32 error, in string format. Returns an empty string if there is no error.
string GetLastErrorAsString()
{
    //Get the error message, if any.
    DWORD errorMessageID = ::GetLastError();
    if (errorMessageID == 0)
        return std::string(); //No error message has been recorded

    LPSTR messageBuffer = nullptr;
    size_t size = FormatMessageA(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL, errorMessageID, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), (LPSTR)&messageBuffer, 0, NULL);

    std::string message(messageBuffer, size);

    //Free the buffer.
    LocalFree(messageBuffer);

    return message;
}

bool demo()
{
    if (IsEnclaveTypeSupported(ENCLAVE_TYPE_VBS))
    {
        DWORD lpError = 0;
        ENCLAVE_CREATE_INFO_VBS ecrv = { 0 };
        ecrv.Flags = 1;
        LPVOID enclave = CreateEnclave(GetCurrentProcess(), NULL, 1024 * 1024 * 20, NULL, ENCLAVE_TYPE_VBS, &ecrv, sizeof(ENCLAVE_CREATE_INFO_VBS), &lpError);
        if (enclave != NULL)
        {
            cout << "创建VBS enclave成功" << endl;
            cout << enclave << endl;
            HRESULT getAttestationRet = E_FAIL;
            ENCLAVE_CREATE_INFO_VBS ecrv1 = { 0 };
            DWORD lpError1 = 0;
            //getAttestationRet = EnclaveGetAttestationReport();
           /* BOOL initRet = InitializeEnclave(GetCurrentProcess(), enclave, &ecrv1, sizeof(ENCLAVE_CREATE_INFO_VBS), &lpError1)
            if (initRet)
            {
                cout << "初始化VBS enclave成功" << endl;
            }*/

            BOOL loadEnclaveRet = LoadEnclaveImageW(enclave, L"..\\wdksetup.exe");
            if (loadEnclaveRet)
            {
                cout << "加载PE到enclave成功" << endl;
            }
            else
            {
                cout << GetLastErrorAsString() << endl;
            }
            BOOL deleteEnclaveRet = DeleteEnclave(enclave);
            BOOL terminateEnclaveRet = TerminateEnclave(enclave, true);
           
            if (deleteEnclaveRet)
                cout << "销毁VBS enclave成功" << endl;
            if (terminateEnclaveRet)
            {
                cout << "terminate VBS enclave成功" << endl;
            }
            return true;
        }
        else
        {
            cout << "创建VBS enclave失败" << endl;
            return false;
        }
    }
    return false;
}

int main()
{
    demo();
}


// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门使用技巧: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
