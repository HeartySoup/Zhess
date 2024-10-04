using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooleanLogic : MonoBehaviour
{
 
    public bool RookFallCheck(float h, bool r, bool g)
    {
        if((h == 2 || h == 1 || h == 0) && r && !g)
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
        if (g || (!g && s != Vector2.zero && b))
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
}
