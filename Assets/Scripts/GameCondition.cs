using System;
using UnityEngine;

public class GameCondition : MonoBehaviour, IRoomResettable
{
    [SerializeField] private bool startsMet = false;

    public bool IsMet { get; private set; }
    private bool savedIsMet;

    public event Action ConditionChanged;

    private void Awake()
    {
        IsMet = startsMet;
    }

    public void MeetCondition()
    {
        SetCondition(true);
    }

    public void ClearCondition()
    {
        SetCondition(false);
    }

    public void ToggleCondition()
    {
        SetCondition(!IsMet);
    }

    public void SetCondition(bool value)
    {
        if (IsMet == value)
            return;

        IsMet = value;
        ConditionChanged?.Invoke();
    }

    public void SaveState()
    {
        savedIsMet = IsMet;
    }

    public void RestoreState()
    {
        IsMet = savedIsMet;
    }
}
