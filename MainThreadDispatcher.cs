using System;
using System.Collections.Concurrent;
using UnityEngine;


public class MainThreadDispatcher : UnityEngine.Object //: MonoBehaviour
{
    // Singleton pattern
    private static MainThreadDispatcher _instance;

    // This thread-save queue stores Actions to be executed in the main thread
    // any thread can add Actions to it via the ExecuteInUpdate method
    // then the existing actions will be executed in the Update loop
    // in the same order they came in
    private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

    // This may be called by threads
    public static void ExecuteInUpdate(Action action)
    {
        if (!_instance)
        {
            Debug.LogError("You can talk but nobody is listening../nNo MainThreadDispatcher in scene!");
            return;
        }

        // Add the action as new entry at the end of the queue
        _instance.actions.Enqueue(action);
    }

    public MainThreadDispatcher()
    {
        Awake();
    }

    // Initialize the singleton
    public void Awake()
    {
        //if (_instance && _instance != this)
        //{
        //    // an instance already exists so destroy this one
        //   // Destroy(gameObject);
        //    return;
        //}

        _instance = this;
      //  DontDestroyOnLoad(this);
    }

    // This will be executed when the game is started
    // AFTER all Awake calls
    // See https://docs.unity3d.com/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html
    //[RuntimeInitializeOnLoadMethod]
    //private static void Initialize()
    //{
    //    // If the instance exists already everything is fine
    //    if (_instance) return;

    //    // otherwise create it lazy
    //    // This adds a new GameObject to the scene with the MainThreadDispatcher
    //    // component attached -> this will call Awake automatically
    //    new GameObject("MainThreadDispatcher", typeof(MainThreadDispatcher));
    //}

    // Executed in the Unity main thread
    public static void Update()
    {
        Action action = null;
        // Every frame check if any actions are enqueued to be executed
        while (_instance.actions.TryDequeue(out  action))
        {
            action?.Invoke();
        }
    }
}