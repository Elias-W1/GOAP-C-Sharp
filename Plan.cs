using System;

public class RecursivePlanner
{
    public PlanAction PlanActions(PlanWorldModel worldModel, int maxDepth)
    {
        Tuple<float, PlanAction> best = RecursivePlan(worldModel, 0, maxDepth);
        return best.Item2;
    }

    public Tuple<float, PlanAction> RecursivePlan(PlanWorldModel worldModel, int depth, int maxDepth)
    {
        // if we are at max recursion depth, return the discontentment and the last action of our world model,
        // else find the lowest discontentment, best action from all possible successor world models.

        if (depth == maxDepth)
        {
            float current = worldModel.CalculateDiscontentment();
            return new Tuple<Single, PlanAction>(current, null);
        }
        else
        {
            PlanAction nextAction = worldModel.GetNextAction();
            float bestDiscontentment = float.MaxValue;
            PlanAction bestAction = null;
            
            // Loop over all the possible actions in our current world model and make a recursive call for each.
            while (nextAction != null)
            {
                PlanWorldModel modelCopy = worldModel.Copy();
                
                // apply the action to our copied world model before next recursive call
                modelCopy.ApplyAction(nextAction);
                
                // save (discontentment, action) from recursive plan call
                Tuple<float, PlanAction> result = RecursivePlan(modelCopy, depth + 1, maxDepth);
                
                // If the discontentment of the last plan is lower than our best so far, replace action and discontentment with new ones.
                if (result.Item1 < bestDiscontentment)
                {
                    bestDiscontentment = result.Item1;
                    bestAction = nextAction;
                }

                nextAction = worldModel.GetNextAction();
            }

            return new Tuple<float, PlanAction>(bestDiscontentment, bestAction);
        }
        
    }

}

