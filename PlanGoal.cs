using System;

public abstract class PlanGoal
{
    public String name;

    public float value;
    public float changeOverTime;

    public PlanGoal(float value, float changeOverTime)
    {
        this.value = value;
        this.changeOverTime = changeOverTime;
    }

    public float GetDiscontentment(float value)
    {
        return value;
    }
}