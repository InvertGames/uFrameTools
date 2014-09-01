﻿using System.Collections.Generic;
using UnityEngine;

[UBCategory("Gestures")]
public class UBSwipeRightTrigger : UBSwipeTrigger
{
    public static IEnumerable<IUBVariableDeclare> UBSwipeRightTriggerVariables()
    {
        yield return new UBStaticVariableDeclare() { Name = "Swipe", DefaultValue = Vector3.zero };
    }
    public override void Update()
    {
        DetectSwipe(this);
        if (SwipeDirection == Swipe.Right)
        {
            ExecuteSheet();
        }
    }
}