using UnityEngine;
using System.Collections.Generic;

public class WaitingArea : MonoBehaviour
{
    [SerializeField] private int capacity = 4;
    private List<Character> waitingCharacters = new List<Character>();

    public bool IsFull => waitingCharacters.Count >= capacity;

    public void AddCharacter(Character character)
    {
        if (!IsFull)
        {
            waitingCharacters.Add(character);
            Debug.Log("Karakter bekleme alanına eklendi.");
        }
    }

    public void RemoveCharacter(Character character)
    {
        waitingCharacters.Remove(character);
    }

    public void Clear()
    {
        waitingCharacters.Clear();
    }
}