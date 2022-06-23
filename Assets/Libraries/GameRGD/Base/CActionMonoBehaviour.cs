using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleuGames.GameRGD
{
    public class CDelayAction
    {
        public Action action;
        public int startTimeMilisecond;
        public float delaySecond;

        public CDelayAction(Action action, float delaySecond)
        {
            this.action = action;
            this.startTimeMilisecond = CMiscFunc.SafeGetTimer();
            this.delaySecond = delaySecond;
        }
    }

    public class CActionMonoBehaviour : MonoBehaviour
    {
        private Queue<Action> ActionList = new Queue<Action>();
        private Queue<CDelayAction> DelayActionList = new Queue<CDelayAction>();

        protected void EnqueAction(Action action)
        {
            ActionList.Enqueue(action);
        }

        protected void EnqueAction(Action action, float delay)
        {
            DelayActionList.Enqueue(new CDelayAction(action, delay));
        }

        private IEnumerator DelayEnqueAction(CDelayAction delayAction)
        {
            yield return new WaitForSeconds(delayAction.delaySecond);

            EnqueAction(delayAction.action);
        }

        void Update()
        {
            OnUpdateAction();
        }

        void Start()
        {
            OnStartAction();
        }

        void OnEnable()
        {
            OnEnableAction();
        }

        void Awake()
        {
            OnAwakeAction();
        }

        void OnDisable()
        {
            OnDisableAction();
        }

        void OnDestroy()
        {
            OnDestroyAction();
        }

        /// <summary>
        /// Disable()이 호출 될 때마다 호출 됨.		
        ///base.OnDisableAcion() 꼭 호출 해야 됨.
        /// </summary>
        protected virtual void OnDisableAction()
        {
        }

        /// <summary>
        /// Awake()이 호출 될 때마다 호출 됨.		
        ///base.OnAwakeAction() 꼭 호출 해야 됨.
        /// </summary>
        protected virtual void OnAwakeAction()
        {
        }

        /// <summary>
        /// OnEnable()이 호출 될 때마다 호출 됨.		
        ///base.OnEnableAction() 꼭 호출 해야 됨.
        /// </summary>
        protected virtual void OnEnableAction()
        {

        }

        /// <summary>
        /// Start()이 호출 될 때마다 호출 됨.		
        ///base.OnStartAction() 꼭 호출 해야 됨.
        /// </summary>
        protected virtual void OnStartAction()
        {

        }

        /// <summary>
        /// OnDestroy()이 호출 될 때마다 호출 됨.		
        ///base.OnDestroyAction() 꼭 호출 해야 됨.
        /// </summary>
        protected virtual void OnDestroyAction()
        {
            CNotificationCenter.Instance.RemoveHandler(this);
        }

        /// <summary>
        /// Update()이 호출 될 때마다 호출 됨.		
        /// base.OnUpdateAction() 꼭 호출 해야 됨.
        /// </summary>
        protected virtual void OnUpdateAction()
        {
            if (ActionList != null && ActionList.Count > 0)
            {
                var action = ActionList.Dequeue();
                if (action != null)
                {
                    action.Invoke();
                }
            }

            if (DelayActionList != null && DelayActionList.Count > 0)
            {

                CDelayAction delayAction = DelayActionList.Dequeue();
                if (delayAction != null)
                {
                    StartCoroutine(DelayEnqueAction(delayAction));
                }
            }
        }
    }
}
