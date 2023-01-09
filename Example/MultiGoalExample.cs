using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// /* GOAP CODE */
// # region GOAP
//
// public abstract class PlanGoal
// {
//     public String name;
//
//     public float value;
//     public float changeOverTime;
//
//     public PlanGoal(float value, float changeOverTime)
//     {
//         this.changeOverTime = changeOverTime;
//         this.value = value;
//     }
//
//     public virtual float GetDiscontentment(float value)
//     {
//         return value;
//     }
// }
//
// public abstract class PlanAction
// {
//     public Action action;
//
//     private float duration;
//
//     public virtual float GetDuration()
//     {
//         return duration;
//     }
//
//     public PlanAction(Action action, float duration)
//     {
//         this.action = action;
//         this.duration = duration;
//     }
//     
// }
//
// public abstract class PlanWorldModel
// {
//     protected int actionPointer = 0;
//     
//     public List<PlanAction> possibleActions = new List<PlanAction>();
//     public List<PlanGoal> goals = new List<PlanGoal>();
//     protected float timePassed = 0f;
//     private bool actionsDiscovered = false;
//
//     public abstract void ApplyAction(PlanAction action);
//     public abstract PlanWorldModel Copy();
//
//     protected abstract void DiscoverActions();
//
//     protected virtual void AddGoal(PlanGoal goal)
//     {
//         this.goals.Add(goal);
//     }
//
//     public virtual PlanAction GetNextAction()
//     {
//         if (!actionsDiscovered)
//         {
//             DiscoverActions();
//             actionsDiscovered = true;
//         }
//         
//         // no next action
//         if (actionPointer > possibleActions.Count - 1) return null;
//         
//         // next action available
//         PlanAction a = possibleActions[actionPointer];
//         actionPointer++;
//         return a;
//     }
//
//     public virtual float CalculateDiscontentment()
//     {
//         float discontentment = 0f;
//         string s = "";
//         foreach (PlanGoal goal in goals)
//         {
//             
//             float newValue = goal.value + (goal.changeOverTime * timePassed);
//             discontentment += goal.GetDiscontentment(newValue);
//             
//             s += goal + ": " + goal.GetDiscontentment(newValue) + " | ";
//         }
//         
//         Debug.Log(s);
//         
//         return discontentment;
//     }
// }
//
// public class IterativePlanner
// {
//     public virtual PlanAction PlanActions(PlanWorldModel worldModel, int maxDepth)
//     {
//         PlanWorldModel[] models = new PlanWorldModel[maxDepth + 1];
//         models[0] = worldModel;
//         PlanAction[] actions = new PlanAction[maxDepth];
//
//         PlanAction bestAction = null;
//         float bestValue = Single.MaxValue;
//         
//         // dfs
//         int currentDepth = 0;
//         PlanAction nextAction;
//         float currentValue;
//         while (currentDepth >= 0)
//         {
//             currentValue = models[currentDepth].CalculateDiscontentment();
//             Debug.Log("currentValue: "+currentValue);
//             if (currentDepth >= maxDepth)
//             {
//                 Debug.Log(currentValue+" < "+bestValue);
//                 if (currentValue < bestValue)
//                 {
//                     bestValue = currentValue;
//                     bestAction = actions[0];
//                 }
//
//                 currentDepth--;
//             }
//             else
//             {
//                 nextAction = models[currentDepth].GetNextAction();
//                 if(nextAction != null) Debug.Log("Considering: "+nextAction);
//                 if (nextAction != null)
//                 {
//                     models[currentDepth + 1] = models[currentDepth].Copy();
//                     actions[currentDepth] = nextAction;
//                     models[currentDepth+1].ApplyAction(nextAction);
//                     currentDepth++;
//                 }
//                 else
//                 {
//                     currentDepth--;
//                 }
//             }
//         }
//
//         return bestAction;
//     }
// }
//
// # endregion GOAP


/* EXAMPLE CODE */

// Goals.
public enum GoalType
{
    NONE,
    KILL,
    SATURATION
}

public enum ActionType
{
    EAT,
    GOTO_OVEN,
    ATTACK
}

public abstract class RPGGoal : PlanGoal
{
    public GoalType type = GoalType.NONE;

    protected RPGGoal(float value, float changeOverTime) : base(value, changeOverTime)
    {
    }
}

public class SaturationGoal : RPGGoal
{
    public SaturationGoal(float value, float changeOverTime) : base(value, changeOverTime)
    {
        type = GoalType.SATURATION;
    }

    public override float GetDiscontentment(float newValue)
    {
        // give this goal less impact
        return -(newValue / 50f);
    }
}

public class KillGoal : RPGGoal
{
    public KillGoal(float value, float changeOverTime) : base(value, changeOverTime)
    {
        type = GoalType.KILL;
    }
    
    public override float GetDiscontentment(float newValue)
    {
        // I want my character to go for kills, so I'm squaring the KillGoals discontentment and make it negative.
        return -(Mathf.Pow(newValue, 2));
    }
}

// Turned RPGAction into a base class
public class RPGAction : PlanAction
{
    public ActionType type = ActionType.EAT;
    
    public int meatPieceChange = 0;
    public int cookedMeatChange = 0;
    public float saturationChange = 0;

    public RPGAction(Action action, float duration) : base(action, duration)
    {
    }
}

// Food stuff (old RPGAction basically)
public class FoodAction : RPGAction
{
    public FoodAction(int meatPieceChange, int cookedMeatChange, float saturationChange, Action action, float duration) : base(action, duration)
    {
        this.meatPieceChange = meatPieceChange;
        this.cookedMeatChange = cookedMeatChange;
        this.saturationChange = saturationChange;
    }
}

public class GoToOvenAction : RPGAction
{
    public GoToOvenAction(Action action, float duration) : base(action, duration)
    {
        type = ActionType.GOTO_OVEN;
    }
}


// New Stuff
// Damaging Actions
public class HitableEntity
{
    public int hp = 100;

    public HitableEntity(int hp)
    {
        this.hp = hp;
    }
}

public class AttackAction : RPGAction
{
    // Use pointers! You cannot use object references directly, since we always operate on the next copy of the world model
    // while getting our actions in the original world model. Meaning we when discovering actions in the original we would always reference the objects in the original,
    // but we cant manipulate the original objects because we need them to stay clean for later copies. Ugly solution but it works until I think of something better.
    public int targetPointer;
    public int damage;
    
    public AttackAction(int targetPointer, int damage, Action action, float duration) : base(action, duration)
    {
        this.targetPointer = targetPointer;
        this.damage = damage;

        type = ActionType.ATTACK;
    }
}


public class RPGWorld : PlanWorldModel
{
    public int meatPieces = 0;
    public int cookedMeat = 0;
    private bool hasOven = false;

    private List<HitableEntity> enemies = new List<HitableEntity>();

    public RPGWorld(int rawMeatPieces, int cookedMeat, bool hasOven, List<HitableEntity> enemies)
    {
        this.meatPieces = rawMeatPieces;
        this.cookedMeat = cookedMeat;
        this.hasOven = hasOven;

        this.enemies = enemies;
    }
    
    
    
    public override void ApplyAction(PlanAction action)
    {
        timePassed += action.GetDuration();

        RPGAction rpgaction = action as RPGAction;
        switch (rpgaction.type)
        {
            case ActionType.EAT:
                meatPieces += rpgaction.meatPieceChange;
                cookedMeat += rpgaction.cookedMeatChange;
                
                // Change the value of saturation goal
                foreach (RPGGoal goal in goals)
                {
                    if (goal.type == GoalType.SATURATION)
                    {
                        SaturationGoal saturationGoal = goal as SaturationGoal;
                        saturationGoal.value += rpgaction.saturationChange;
                    }
                }
                break;
            
            case ActionType.ATTACK:
                AttackAction attackAction = rpgaction as AttackAction;
                HitableEntity target = enemies[attackAction.targetPointer];
                target.hp -= attackAction.damage;
                
                // check if we just killed someone, then add a kill.
                if (target.hp <= 0)
                {
                    // Change the value of KillGoal
                    foreach (RPGGoal goal in goals)
                    {
                        if (goal.type == GoalType.KILL)
                        {
                            KillGoal killgoal = goal as KillGoal;
                            killgoal.value += 1;
                        }
                    }
                }
                
                break;
            
            case ActionType.GOTO_OVEN:
                hasOven = true;
                break;
        }

    }

    public override PlanWorldModel Copy()
    {
        RPGWorld model = new RPGWorld(this.meatPieces, cookedMeat, hasOven, new List<HitableEntity>());
        
        // always copy lists and never do shallow copies!
        
        // copy goals
        AddGoalsToCopy(model);
        
        // copy enemies
        foreach (HitableEntity enemy in enemies)
        {
            model.enemies.Add(new HitableEntity(enemy.hp));
        }
        
        return model;
    }
    
    
    private void AddGoalsToCopy(PlanWorldModel newModel)
    {
        foreach (RPGGoal goal in goals)
        {
            switch (goal.type)
            {
                case GoalType.KILL:
                    newModel.goals.Add(new KillGoal(goal.value, goal.changeOverTime));
                    break;
                case GoalType.SATURATION:
                    newModel.goals.Add(new SaturationGoal(goal.value, goal.changeOverTime));
                    break;
                case GoalType.NONE:
                    // error
                    throw new Exception("GoalType cannot be NONE.");
            }
        
        }
    }

    protected override void DiscoverActions()
    {
        // new actions
        // pro tip: actions at the top will be considered first when planning
        for (int i = 0; i < enemies.Count; i++)
        {
            HitableEntity enemy = enemies[i];
            if(enemy.hp > 0)
                possibleActions.Add(new AttackAction(i, 50, null, 0.5f));
        }
        
        if (!hasOven)
        {
            possibleActions.Add(new GoToOvenAction(null,15f));
        }
        
        if (hasOven && meatPieces > 0)
        {
            possibleActions.Add(new FoodAction(-1,1,-0.5f, null, 30f));
        }

        if (meatPieces > 0)
        {
            possibleActions.Add(new FoodAction(-1, 0, 5f, null, 3f));
        }

        if (cookedMeat > 0)
        {
            possibleActions.Add(new FoodAction(0, -1, 25f, null, 3f));
        }
    }
}

