using UnityEngine;
using DG.Tweening;

public class WaitingArea : MonoBehaviourSingleton<WaitingArea>
{
    public int slotCount = 3;
    public float slotSpacing = 1f;
    private Vector3[] slotPositions;
    private Character[] waitingCharacters;
    private GameObject[] slotVisuals; // Visual representation of slots
    
    private void Awake()
    {
        InitializeSlots();
        CreateSlotVisuals();
    }
    
    private void InitializeSlots()
    {
        slotPositions = new Vector3[slotCount];
        waitingCharacters = new Character[slotCount];
        slotVisuals = new GameObject[slotCount];
        
        float startX = -(slotCount - 1) * slotSpacing / 2f;
        
        for (int i = 0; i < slotCount; i++)
        {
            slotPositions[i] = new Vector3(
                startX + i * slotSpacing, 
                0, 
                0f
            );
        }
    }
    
    private void CreateSlotVisuals()
    {
        for (int i = 0; i < slotCount; i++)
        {
            // Create a new GameObject for each slot
            GameObject slot = new GameObject($"Slot_{i}");
            slot.transform.SetParent(transform);
            slot.transform.localPosition = slotPositions[i];
            
            // Add a visible component (you can customize this)
            var sphere = slot.AddComponent<SphereCollider>();
            sphere.radius = 0.2f;
            sphere.isTrigger = true;

            slotVisuals[i] = slot;
        }
    }
    
    public int? GetAvailableSlot()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (waitingCharacters[i] == null)
            {
                return i;
            }
        }
        return null;
    }
    
    public void OccupySlot(int slotIndex, Character character)
    {
        if (slotIndex >= 0 && slotIndex < slotCount)
        {
            waitingCharacters[slotIndex] = character;
            
            // Parent the character to the slot
            character.transform.SetParent(slotVisuals[slotIndex].transform);
            
            // Center the character in the slot
            character.transform.localPosition = Vector3.zero;
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
                Character character = waitingCharacters[i];
                FreeSlot(i);
                
                // Otobüse binme animasyonu
                character.transform.DOMove(GetBusEntryPosition(), 0.5f)
                    .OnComplete(() => {
                        BusManager.Instance.GetActiveBus()?.OccupySeat();
                        Destroy(character.gameObject);
                    });
            }
        }
    }
    private Vector3 GetBusEntryPosition()
    {
        // Otobüs giriş pozisyonunu döndür (otobüsün konumuna göre ayarlayın)
        return BusManager.Instance.GetActiveBus()?.transform.position ?? Vector3.zero;
    }
}
