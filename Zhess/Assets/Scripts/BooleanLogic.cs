using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooleanLogic : MonoBehaviour
{
 
    public bool RookFallCheck(float h, float m)
    {
        if((h < 3|| m == 6))
        {
            return true;
        }
        return false;

    }
    public bool MoveEqualorO(float a, float b)
    {
        if (a == b || b == 0)
        {
            return true;
        }
        return false;
    }
    public bool GroundedOrContinuing(bool g, Vector2 s, bool b)
    {
        if ((g) || (!g && s != Vector2.zero && b))
        {
            return true;
        }
        return false;
    }

    public bool CanJump(float c, bool fh, bool wR)
    {
        if(c > 0 && !fh && wR)
        {
            return true;
        }
        return false;
    }

    public bool PawnDoubleHop(float m, bool p)
    {
        if(m == 2 && p == false)
        {
            return true;
        }
        return false;
    }
}
