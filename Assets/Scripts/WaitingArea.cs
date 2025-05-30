using UnityEngine;
using System.Collections.Generic;

public class WaitingArea : MonoBehaviour
{
    public int capacity = 4;
    private List<Character> waitingCharacters = new List<Character>();

    public void AddToWaitingArea(Character character)
    {
        waitingCharacters.Add(character);
        Debug.Log("Karakter bekleme alanına alındı.");
    }

    public bool IsFull()
    {
        return waitingCharacters.Count >= capacity;
    }

    public void RemoveCharacter(Character character)
    {
        waitingCharacters.Remove(character);
    }

    public void ClearWaitingArea()
    {
        waitingCharacters.Clear();
    }
}