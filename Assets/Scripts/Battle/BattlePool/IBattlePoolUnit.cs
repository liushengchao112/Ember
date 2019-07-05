using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattlePoolUnit
{
    BattlePool pool { get; set; }

    void Recycle();

    void Clear();
}

