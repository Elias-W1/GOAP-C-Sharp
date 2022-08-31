using System.Collections.Generic;

public abstract class PlanWorldModel
{
    protected int actionPointer = 0;
    
    public List<PlanAction> possibleActions = new List<PlanAction>();
    public List<PlanGoal> goals = new List<PlanGoal>();
    protected float timePassed = 0f;
    private bool actionsDiscovered = false;
    
    public abstract void ApplyAction(PlanAction action);
    public abstract PlanWorldModel Copy();
    
    protected abstract void DiscoverActions();
    
    public virtual PlanAction GetNextAction()
    {
        if (!actionsDiscovered)
        {
            DiscoverActions();
            actionsDiscovered = true;
        }
    
        // no next action
        if (actionPointer > possibleActions.Count-1) return null;
        // next action available
        PlanAction a = possibleActions[actionPointer];
        actionPointer++;
        return a;
    }

    protected virtual void AddGoal(PlanGoal goal)
    {
        this.goals.Add(goal);
    }

    public virtual float CalculateDiscontentment()
    {
        float discontentment = 0f;
        foreach(PlanGoal goal in goals)
        {
            float newValue = goal.value + (goal.changeOverTime * timePassed);
            discontentment *= goal.GetDiscontentment(newValue);
        }

        return discontentment;
    }
}