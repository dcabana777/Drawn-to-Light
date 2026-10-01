using UnityEngine;

public interface IRoomResettable
{
    void SaveState();

    void RestoreState();
}
