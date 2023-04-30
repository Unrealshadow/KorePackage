using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class EventUtility
{
    // Dictionary of events with string keys and UnityEvent values
    private static Dictionary<string, UnityEventBase> eventDictionary = new Dictionary<string, UnityEventBase>();

    // Adds a listener to the event with the specified name
    public static void AddListener<T>(string eventName, UnityAction<T> action)
    {
        UnityEvent<T> unityEvent;
        if (eventDictionary.TryGetValue(eventName, out UnityEventBase baseEvent))
        {
            unityEvent = baseEvent as UnityEvent<T>;
            if (unityEvent != null)
            {
                unityEvent.AddListener(action);
            }
            else
            {
                Debug.LogError("Incorrect event type for parameter");
            }
        }
        else
        {
            unityEvent = new UnityEvent<T>();
            unityEvent.AddListener(action);
            eventDictionary.Add(eventName, unityEvent);
        }
    }
    public static void AddListener(string eventName, UnityAction action)
    {
        UnityEvent unityEvent;
        if (eventDictionary.TryGetValue(eventName, out UnityEventBase baseEvent))
        {
            unityEvent = baseEvent as UnityEvent;
            if (unityEvent != null)
            {
                unityEvent.AddListener(action);
            }
            else
            {
                Debug.LogError("Incorrect event type for parameter");
            }
        }
        else
        {
            unityEvent = new UnityEvent();
            unityEvent.AddListener(action);
            eventDictionary.Add(eventName, unityEvent);
        }
    }

    public static void RemoveListener(string eventName, UnityAction action)
    {
        UnityEvent unityEvent;
        if (eventDictionary.TryGetValue(eventName, out UnityEventBase baseEvent))
        {
            unityEvent = baseEvent as UnityEvent;
            if (unityEvent != null)
            {
                unityEvent.RemoveListener(action);
            }
        }
    }

    // Removes a listener from the event with the specified name
    public static void RemoveListener<T>(string eventName, UnityAction<T> action)
    {
        UnityEvent<T> unityEvent;
        if (eventDictionary.TryGetValue(eventName, out UnityEventBase baseEvent))
        {
            unityEvent = baseEvent as UnityEvent<T>;
            if (unityEvent != null)
            {
                unityEvent.RemoveListener(action);
            }
        }
    }

    // Fires the event with the specified name
    public static void FireEvent(string eventName)
    {
        UnityEvent unityEvent;
        if (eventDictionary.TryGetValue(eventName, out UnityEventBase baseEvent))
        {
            unityEvent = baseEvent as UnityEvent;
            if (unityEvent != null)
            {
                unityEvent.Invoke();
            }
            else
            {
                Debug.LogError("Incorrect event type for parameter");
            }
        }
    }

    // Fires the event with one parameter
    public static void FireEvent<T>(string eventName, T arg)
    {
        UnityEvent<T> unityEvent;
        if (eventDictionary.TryGetValue(eventName, out UnityEventBase baseEvent))
        {
            unityEvent = baseEvent as UnityEvent<T>;
            if (unityEvent != null)
            {
                unityEvent.Invoke(arg);
            }
            else
            {
                Debug.LogError("Incorrect event type for parameter");
            }
        }
    }

    // Fires the event with two parameters
    public static void FireEvent<T, U>(string eventName, T arg1, U arg2)
    {
        UnityEvent<T, U> unityEvent;
        if (eventDictionary.TryGetValue(eventName, out UnityEventBase baseEvent))
        {
            unityEvent = baseEvent as UnityEvent<T, U>;
            if (unityEvent != null)
            {
                unityEvent.Invoke(arg1, arg2);
            }
            else
            {
                Debug.LogError("Incorrect event type for parameters");
            }
        }
    }

    // Fires the event with custom data
    public static void FireEventWithCustomData<T>(string eventName, T data)
    {
        UnityEvent<T> unityEvent;
        if (eventDictionary.TryGetValue(eventName, out UnityEventBase baseEvent))
        {
            unityEvent = baseEvent as UnityEvent<T>;
            if (unityEvent != null)
            {
                unityEvent.Invoke(data);
            }
            else
            {
                Debug.LogError("Incorrect event type for parameter");
            }
        }
    }
}
