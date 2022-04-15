using System;

public abstract class PlanAction
{
    public Action action;
    
    private float duration;
    
    public virtual float GetDuration()
    {
        return duration;
    }

}