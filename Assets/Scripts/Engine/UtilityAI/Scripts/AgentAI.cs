using System.Collections.Generic;
using UnityEngine;

namespace AI.Utility
{
    public class AgentAI : MonoBehaviour
    {
        // public float selectInterval;
        public AIConfig config;
        public bool showLog;

        [Header("RUNTIME")]
        public ActionObj[] actionObjs;
        public ActionObj curActionObj;
        [SerializeField] private float tick;
        [SerializeField] private bool inited;
        public bool debugScore;

        public delegate void AgentHandleDelegate(AgentAI agent);
        public static AgentHandleDelegate onAgentCreate;
        public static AgentHandleDelegate onAgentDestroy;

        public delegate void ActionChangedDelegate(ActionObj act);
        public ActionChangedDelegate onActionChanged;

        private void Start()
        {
            onAgentCreate?.Invoke(this);
        }

        private void OnDestroy() 
        {
            onAgentDestroy?.Invoke(this);
        }

        public void Init(bool log)
        {
            Init(config, true);
        }

        public void Init(AIConfig cfg, bool log)
        {
            Reset();

            if (cfg == null && config == null)
            {
                if (log)
                    Debug.LogError($"{name} cant init because ai config is null");
                return;
            }

            inited = true;
            if (cfg != null)
                config = cfg;

            actionObjs = new ActionObj[config.len];
            int actionObjIdx = 0;
            if (config.actions != null)
            {
                foreach (var action in config.actions)
                {
                    var obj = CreateActionObj(action);
                    actionObjs[actionObjIdx++] = obj;
                }
            }
            if (config.actionGroups != null)
            {
                foreach (var g in config.actionGroups)
                {
                    if (g == null)
                    {
                        if (showLog)
                            AILogger.LogError($"{config.name} action group is null");
                        continue;
                    }
                    foreach (var action in g.actions)
                    {
                        var obj = CreateActionObj(action);
                        actionObjs[actionObjIdx++] = obj;
                    }
                }
            }
        }

        private ActionObj CreateActionObj(Action action)
        {
            if (action == null)
            {
                if (showLog)
                    AILogger.LogError($"{config.name} action is null");
                return null;
            }

            var obj = (ActionObj)gameObject.AddComponent(action.ActionObjType());
            if (obj == null)
            {
                if (showLog)
                    AILogger.LogError($"{config.name} action {action.name} cant create obj");
                return null;
            }
            obj.Init(action);
            return obj;
        }

        public void Reset()
        {
            if (actionObjs != null && actionObjs.Length > 0)
            {
                for (int i = actionObjs.Length -1; i >= 0; --i)
                {
                    var aobj = actionObjs[i];
                    GameObject.Destroy(aobj);
                }
                actionObjs = null;
            }

            inited = false;
            tick = 0f;
            curActionObj = null;
        }

        public void Tick(IContext ctx, float dt)
        {
            if (inited == false)
                return;

            if (debugScore)
            {
                DebugRefreshScore(ctx);
            }
                
            if (curActionObj != null)
            {
                var status = curActionObj.Execute(ctx, dt);
                if (status == ActionObj.Status.FINISHED)
                {
                    curActionObj.Exit(ctx);
                    if (showLog)
                        AILogger.Log($"{name} exit action > {curActionObj.dbgName}");
                    curActionObj = null;
                }
            }

            // tick += dt;
            // if (tick < selectInterval)
            //     return;
            // tick -= selectInterval;

            if (curActionObj != null && !curActionObj.CanInterrupt(ctx))
                return;

            var bestAction = Select(ctx);
            if (bestAction == curActionObj)
                return;

            if (curActionObj != null)
            {
                curActionObj.Exit(ctx);
                if (showLog)
                    AILogger.Log($"{name} exit action > {curActionObj.dbgName}");
            }
            curActionObj = bestAction;
            if (curActionObj != null)
            {
                curActionObj.Enter(ctx);
                if (showLog)
                    AILogger.Log($"{name} enter action > {curActionObj.dbgName}");
            }

            if (onActionChanged != null)
                onActionChanged.Invoke(curActionObj);
        }

        private ActionObj Select(IContext ctx)
        {
            if (actionObjs == null || actionObjs.Length == 0)
                return null;

            float bestScore = float.MinValue;
            ActionObj bestAction = null;
            for (int i = 0; i < actionObjs.Length; ++i)
            {
                var act = actionObjs[i];
                if (act == null)
                    continue;

                if (act.IsInCooldown(ctx))
                    continue;
                if (act.Evaluate(ctx) == false)
                    continue;

                float score = act.Score(ctx);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestAction = act;
                }
            }

            return bestAction;
        }

        private void DebugRefreshScore(IContext ctx)
        {
            if (actionObjs == null || actionObjs.Length == 0)
                return;

            for (int i = 0; i < actionObjs.Length; ++i)
            {
                var act = actionObjs[i];
                if (act == null)
                    continue;
                act.Score(ctx);
            }
        }
    }
}