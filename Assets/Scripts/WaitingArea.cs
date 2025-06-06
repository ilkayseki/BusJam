using System;
using UnityEngine;
using DG.Tweening;
public enum CharacterState
{
    Idle,         // Bekleme durumu
    Moving,       // Hareket ediyor
    InWaitingArea, // Waiting Area'da
    BoardingBus   // Otobüse biniyor
}
public class WaitingArea : MonoBehaviourSingleton<WaitingArea>
{
    public int slotCount = 3;
    public float slotSpacing = 1f;
    private Vector3[] slotPositions;
    private Character[] waitingCharacters;
    private GameObject[] slotVisuals;
    public event Action OnWaitingAreaFull;

    public void InitializeWaitingArea(int size)
    {
        slotCount = size;
        slotPositions = new Vector3[size];
        waitingCharacters = new Character[size];
        slotVisuals = new GameObject[size];

        float startX = -(size - 1) * slotSpacing / 2f;

        for (int i = 0; i < size; i++)
        {
            slotPositions[i] = new Vector3(
                startX + i * slotSpacing, 
                0.5f, // Character'ların yüksekliğiyle aynı (0.5f)
                0f
            );
        }

        CreateSlotVisuals();
    }
    
    private void CreateSlotVisuals()
    {
        // Önceki slot visual'ları temizle
        foreach (var slot in slotVisuals)
        {
            if (slot != null) Destroy(slot);
        }
        
        for (int i = 0; i < slotVisuals.Length; i++)
        {
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(transform);
            slot.transform.localPosition = slotPositions[i];
            
            var sphere = slot.AddComponent<SphereCollider>();
            sphere.radius = 0.2f;
            sphere.isTrigger = true;

            slotVisuals[i] = slot;
        }
    }
    
    public int? GetAvailableSlot()
    {
        // waitingCharacters.Length kullanarak dizi sınırlarını aşmaktan kaçının
        for (int i = 0; i < waitingCharacters.Length; i++)
        {
            if (waitingCharacters[i] == null)
            {
                return i;
            }
        }

        // Eğer boş slot yoksa direkt GameOver tetikle
        OnWaitingAreaFull?.Invoke();
        return null;
    }
    
    public void OccupySlot(int slotIndex, Character character)
    {
        if (slotIndex >= 0 && slotIndex < slotCount)
        {
            waitingCharacters[slotIndex] = character;
        
            // Parent the character to the slot
            character.transform.SetParent(slotVisuals[slotIndex].transform);
        
            // Center the character in the slot and set correct height
            character.transform.localPosition = new Vector3(0, 0, 0); // Local pozisyonu sıfırla
            character.transform.position = new Vector3(
                character.transform.position.x,
                0.5f, // Karakter yüksekliği
                character.transform.position.z
            );
        }
    }
    
    public void FreeSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotCount)
        {
            waitingCharacters[slotIndex] = null;
        }
    }
    
    public Vector3 GetSlotPosition(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotCount)
        {
            return slotVisuals[slotIndex].transform.position;
        }
        return Vector3.zero;
    }
    
    public void CheckForMatchingCharacters(string busColor)
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (waitingCharacters[i] != null && waitingCharacters[i].CharacterColor == busColor)
            {
                InputManager.Instance.BlockInput(true);
                Character character = waitingCharacters[i];
                FreeSlot(i);
                
                BusManager.Instance.GetActiveBus()?.OccupySeat();
                Destroy(character.gameObject);
                InputManager.Instance.BlockInput(false);
            }
        }
    }
}
