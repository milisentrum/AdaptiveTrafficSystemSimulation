using System;
using System.Collections;
using System.Collections.Generic;
using UnityDevKit.Patterns;

namespace AdaptiveTrafficSystem.Utils
{
    public class UnityMainThread : Singleton<UnityMainThread>
    {
        private static readonly SyncActionsWorker UpdateFuncSyncActionsWorker = new();
        private static volatile bool _isUpdateWorkerExecuted = true;

        private static readonly SyncActionsWorker LateUpdateFuncSyncActionsWorker = new();
        private static volatile bool _isLateUpdateWorkerExecuted = true;

        private static readonly SyncActionsWorker FixedUpdateFuncSyncActionsWorker = new();
        private static volatile bool _isFixedUpdateWorkerExecuted = true;

        private class SyncActionsWorker
        {
            private readonly List<Action> _actionsToExecuteList = new();
            private readonly List<Action> _executingActionsList = new();

            public bool AddAction(Action action)
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                lock (_actionsToExecuteList)
                {
                    _actionsToExecuteList.Add(action);
                }

                return false; // flag -- has to be executed
            }

            public bool PrepareToExecute()
            {
                lock (_actionsToExecuteList)
                {
                    _executingActionsList.AddRange(_actionsToExecuteList);
                    _actionsToExecuteList.Clear();
                }

                return true; // flag -- ready for execute
            }

            public void Execute()
            {
                for (var i = 0; i < _executingActionsList.Count; i++)
                {
                    _executingActionsList[i].Invoke();
                }

                _executingActionsList.Clear();
            }
        }

        #region Coroutine execution

        public static void ExecuteCoroutine(IEnumerator action)
        {
            ExecuteInUpdate(() => Instance.StartCoroutine(action));
        }

        #endregion

        #region Update execution

        public static void ExecuteInUpdate(Action action)
        {
            _isUpdateWorkerExecuted = UpdateFuncSyncActionsWorker.AddAction(action);
        }

        public static void ExecuteInUpdate<T>(Action<T> action, T param)
        {
            ExecuteInUpdate(WrapActionWithParams(action, param));
        }

        private void Update()
        {
            if (_isUpdateWorkerExecuted)
            {
                return;
            }

            _isUpdateWorkerExecuted = UpdateFuncSyncActionsWorker.PrepareToExecute();
            UpdateFuncSyncActionsWorker.Execute();
        }

        #endregion

        #region LateUpdate execution

        public static void ExecuteInLateUpdate(Action action)
        {
            _isLateUpdateWorkerExecuted = LateUpdateFuncSyncActionsWorker.AddAction(action);
        }

        public static void ExecuteInLateUpdate<T>(Action<T> action, T param)
        {
            ExecuteInLateUpdate(WrapActionWithParams(action, param));
        }

        private void LateUpdate()
        {
            if (_isLateUpdateWorkerExecuted)
            {
                return;
            }

            _isLateUpdateWorkerExecuted = LateUpdateFuncSyncActionsWorker.PrepareToExecute();
            LateUpdateFuncSyncActionsWorker.Execute();
        }

        #endregion

        #region FixedUpdate execution

        public static void ExecuteInFixedUpdate(Action action)
        {
            _isFixedUpdateWorkerExecuted = FixedUpdateFuncSyncActionsWorker.AddAction(action);
        }

        public static void ExecuteInFixedUpdate<T>(Action<T> action, T param)
        {
            ExecuteInFixedUpdate(WrapActionWithParams(action, param));
        }

        private void FixedUpdate()
        {
            if (_isFixedUpdateWorkerExecuted)
            {
                return;
            }

            _isFixedUpdateWorkerExecuted = FixedUpdateFuncSyncActionsWorker.PrepareToExecute();
            FixedUpdateFuncSyncActionsWorker.Execute();
        }

        #endregion

        private static Action WrapActionWithParams<T>(Action<T> action, T param) => () => action(param);
    }
}