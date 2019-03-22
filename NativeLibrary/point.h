#pragma once

// Similar to GDI+ Point class https://docs.microsoft.com/en-us/windows/desktop/api/gdiplustypes/nl-gdiplustypes-point
class Point
{
public:
    Point()
    {
        X = Y = 0;
    }

    Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    int X;
    int Y;
};