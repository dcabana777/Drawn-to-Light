using UnityEngine;

public enum PlayerMovementState
{
    Grounded,
    Airborne
}

public class PlayerState : MonoBehaviour
{
    public int FacingDirection { get; private set; } = 1;

    public bool FacingRight => FacingDirection == 1;
    public bool FacingLeft => FacingDirection == -1;

    public PlayerMovementState CurrentMoveState { get; private set; }

    public void SetFacingDirection(float direction)
    {
        if (direction > 0f)
        {
            FacingDirection = 1;
        }
        else if (direction < 0f)
        {
            FacingDirection = -1;
        }
    }

    public void SetMovementState(PlayerMovementState newState)
    {
        CurrentMoveState = newState;
    }
}