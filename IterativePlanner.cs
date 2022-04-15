using System;

public class IterativePlanner
{
    public virtual PlanAction PlanActions(PlanWorldModel worldModel, int maxDepth)
    {
        PlanWorldModel[] models = new PlanWorldModel[maxDepth + 1];
        models[0] = worldModel;
        PlanAction[] actions = new PlanAction[maxDepth];

        PlanAction bestAction = null;
        float bestVal = Single.MaxValue;

        int currentDepth = 0;
        PlanAction nextAction;
        float currentVal;
        while (currentDepth >= 0)
        {
            currentVal = models[currentDepth].CalculateDiscontentment();
            if (currentDepth >= maxDepth)
            {
                // discontentment lower than best discontentment Value so far: this is the best action so far, skip iteration.
                if (currentVal < bestVal)
                {
                    bestVal = currentVal;
                    bestAction = actions[0];
                }
                // we are done at this level
                currentDepth--;
            }
            else {
                // otherwise: Try the next action
                nextAction = models[currentDepth].GetNextAction();
                if (nextAction != null)
                {
                    models[currentDepth + 1] = models[currentDepth].Copy();
                    actions[currentDepth] = nextAction;
                    models[currentDepth + 1].ApplyAction(nextAction);
                    currentDepth++;
                }
                else
                {
                    currentDepth--;
                }
            }
        }
        return bestAction;
    }
}