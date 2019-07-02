// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pch.h"
#include "cexports.h"

int DoubleCDeclImplicit(int value)
{
    return value + value;
}

int __cdecl DoubleCDeclExplicit(int value)
{
    return value + value;
}

int __stdcall DoubleStdCall(int value)
{
    return value + value;
}

int __stdcall Double(int value)
{
    return value + value;
}

int __stdcall Sum(int* values, int count)
{
    int result = 0;
    for (int i = 0; i < count; i++)
    {
        result += values[i];
        values[i] = 42;
    }
    return result;
}

int AddCDecl(int a, int b)
{
    return a + b;
}

int __stdcall CharToIntW(wchar_t value)
{
    return (int)value;
}

void* __stdcall StringPass(wchar_t* value, int count)
{
    return (void*)value;
}

errno_t __stdcall CopyString(wchar_t* source, wchar_t* destination, int destinationLength)
{
    return wcscpy_s(destination, destinationLength, source);
}

void* __stdcall DoubleByRef(int* value)
{
    *value = *value + *value;
    return (void*)value;
}

BOOL __stdcall Invert(BOOL value)
{
    return value == FALSE ? TRUE : FALSE;
}

void* __stdcall InvertByRef(BOOL* value)
{
    *value = *value == FALSE ? TRUE : FALSE;
    return (void*)value;
}

void* __stdcall GetMeaning(int* value)
{
    *value = 42;
    return (void*)value;
}

void __stdcall SwapPoint(Point* point)
{
    int temp = point->X;
    point->X = point->Y;
    point->Y = temp;
}

void __stdcall SwapPoints(Point* points, int count)
{
    for (int i = 0; i < count; i++)
    {
        int temp = points[i].X;
        points[i].X = points[i].Y;
        points[i].Y = temp;
    }
}

void* __stdcall IntsPointer(int* values)
{
    return (void*)values;
}

void* __stdcall PointsPointer(Point* points)
{
    return (void*)points;
}

void __stdcall FlipPointers(void** first, void** second)
{
    void* temp = first;
    *first = *second;
    *second = temp;
}

void* __stdcall VoidReturn()
{
#pragma warning(push)
#pragma warning(disable:4312) // Not a real address, don't care that it is unsafe
    return (void*)0xDEADBEEF;
#pragma warning(pop)
}

void __stdcall VoidPass(void* handle)
{
    // Do nothing
}
