// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

#include "point.h"
#pragma once

// Windows APIs are C style and use the __stdcall calling convention (the callee cleans the stack).
// The standard C calling convention is __cdecl (the caller cleans the stack, allows variable argument lists).


extern "C" __declspec (dllexport) int DoubleCDeclImplicit(int value);
extern "C" __declspec (dllexport) int __cdecl DoubleCDeclExplicit(int value);
extern "C" __declspec (dllexport) int __stdcall DoubleStdCall(int value);
extern "C" __declspec (dllexport) int __stdcall Double(int value);

extern "C" __declspec (dllexport) int __stdcall Sum(int* values, int count);

extern "C" __declspec (dllexport) int AddCDecl(int a, int b);

extern "C" __declspec (dllexport) int __stdcall CharToIntW(wchar_t value);
extern "C" __declspec (dllexport) void* __stdcall StringPass(wchar_t* value, int count);

extern "C" __declspec (dllexport) void* __stdcall DoubleByRef(int* value);

extern "C" __declspec (dllexport) BOOL __stdcall Invert(BOOL value);

extern "C" __declspec (dllexport) void* __stdcall InvertByRef(BOOL* value);
extern "C" __declspec (dllexport) void* __stdcall GetMeaning(int* value);

extern "C" __declspec (dllexport) void __stdcall SwapPoint(Point* point);
extern "C" __declspec (dllexport) void __stdcall SwapPoints(Point* points, int count);

extern "C" __declspec (dllexport) void* __stdcall IntsPointer(int* values);
extern "C" __declspec (dllexport) void* __stdcall PointsPointer(Point* points);
extern "C" __declspec (dllexport) void __stdcall FlipPointers(void** first, void** second);

extern "C" __declspec (dllexport) void* __stdcall VoidReturn();
extern "C" __declspec (dllexport) void __stdcall VoidPass(void* handle);
